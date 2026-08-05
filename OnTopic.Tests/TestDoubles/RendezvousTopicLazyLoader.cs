/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: RENDEZVOUS TOPIC LAZY LOADER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   An <see cref="ITopicLazyLoader"/> that suspends each <see cref="ITopicLazyLoader.EnsureLoaded"/> call until a fixed number
///   of concurrent passes have arrived, then releases them together, so a stress test can force two concurrent mapping passes
///   to modify a shared collection at the same time without timing hacks. Conceptually, this is an asynchronous, cyclic <see
///   cref="Barrier"/> for a fixed number of mapping passes.
/// </summary>
/// <remarks>
///   <para>
///     Unlike a repository-backed lazy loader, this performs no fetching: The stress test wires the shared topic's associations
///     before mapping, so <see cref="ITopicLazyLoader.EnsureLoaded"/> only needs to a) "rendezvous" the concurrent passes and
///     b) mark <see cref="Topic.Children"/> as <see cref="LoadState.Loaded"/> so the base pass's nested-topic search, which
///     reads the autoloading <see cref="Topic.Children"/> getter, doesn't re-enter this loader. The rendezvous rearms after
///     each release, so a single instance serves every repetition.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class RendezvousTopicLazyLoader: ITopicLazyLoader {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              object                          _lock                           = new();
  private readonly              int                             _participantCount;
  private readonly              TimeSpan                        _timeout;
  private                       TaskCompletionSource            _gate                           = new(TaskCreationOptions.RunContinuationsAsynchronously);
  private                       int                             _arrivals;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instantiates a new instance of the <see cref="RendezvousTopicLazyLoader"/> that releases once <paramref name=
  ///   "participantCount"/> passes have arrived.
  /// </summary>
  /// <param name="participantCount">The number of concurrent passes to await before releasing them together.</param>
  /// <param name="timeoutSeconds">
  ///   The number of seconds an arrived pass waits for the others before throwing, guarding against a hang if the expected
  ///   concurrency never materializes.
  /// </param>
  public RendezvousTopicLazyLoader(int participantCount = 2, int timeoutSeconds = 10) {
    _participantCount           = participantCount;
    _timeout                    = TimeSpan.FromSeconds(timeoutSeconds);
  }

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  async Task ITopicLazyLoader.EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {

    // Suspend until the concurrent passes rendezvous, so they resume together and race on the shared collection
    await Rendezvous(cancellationToken).ConfigureAwait(false);

    // Mark children Loaded so the base pass's nested-topic probe, which reads the autoloading Topic.Children getter, doesn't
    // re-enter this loader. Relationships needs no such treatment: Its targets are preloaded, so its (derived) LoadState is
    // already Loaded and so relationship never autoload.
    ((ITopicBackingAccessor)topic).Children.LoadState = LoadState.Loaded;

  }

  /*============================================================================================================================
  | METHOD: RENDEZVOUS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Suspends the caller until <see cref="_participantCount"/> passes have arrived, then releases them together and re-arms
  ///   for the next batch.
  /// </summary>
  /// <param name="cancellationToken">A token used to cancel the wait.</param>
  private Task Rendezvous(CancellationToken cancellationToken) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Register arrival and, if last, re-arm the gate for the next batch
    \-------------------------------------------------------------------------------------------------------------------------*/
    TaskCompletionSource        gate;
    bool                        release;
    lock (_lock) {
      gate                      = _gate;
      release                   = ++_arrivals >= _participantCount;
      if (release) {
        _arrivals               = 0;
        _gate                   = new(TaskCreationOptions.RunContinuationsAsynchronously);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | The last arrival releases the batch and proceeds without waiting
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (release) {
      gate.TrySetResult();
      return Task.CompletedTask;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Earlier arrivals wait for the last, throwing on timeout so a broken assumption fails loudly instead of hanging
    \-------------------------------------------------------------------------------------------------------------------------*/
    return AwaitGate(gate, cancellationToken);

  }

  /*============================================================================================================================
  | METHOD: AWAIT GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Waits for <paramref name="gate"/> to be released, throwing a <see cref="TimeoutException"/> if the batch never
  ///   completes.
  /// </summary>
  /// <param name="gate">The gate to wait on for the current batch.</param>
  /// <param name="cancellationToken">A token used to cancel the wait.</param>
  private async Task AwaitGate(TaskCompletionSource gate, CancellationToken cancellationToken) {

    // Race the gate against a timeout, so an unexpected participant count can't hang the test suite
    var completed               = await Task.WhenAny(gate.Task, Task.Delay(_timeout, cancellationToken)).ConfigureAwait(false);

    // Surface a failed rendezvous as an exception rather than proceeding with a corrupt result
    if (completed != gate.Task) {
      throw new TimeoutException(
        $"The rendezvous timed out after {_timeout.TotalSeconds:0} seconds waiting for {_participantCount} concurrent " +
        $"passes; the expected concurrency did not occur."
      );
    }

  }

} //Class