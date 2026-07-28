/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;
using OnTopic.TestDoubles.LazyLoading;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: BLOCKING STUB LAZY LOADING TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   A <see cref="StubLazyLoadingTopicRepository"/> that counts every <see cref="EnsureLoaded"/> call and, while "armed",
///   suspends inside it until released, thus letting a test provably interleave two concurrent lazy loads of the same topic
///   without <see cref="Thread.Sleep(int)"/> or other timing hacks.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class BlockingStubLazyLoadingTopicRepository: StubLazyLoadingTopicRepository {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private                       TaskCompletionSource?           _gate;

  /*============================================================================================================================
  | PROPERTY: FETCH COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="EnsureLoaded"/> has been called.
  /// </summary>
  public int                    FetchCount                      { get; private set; }

  /*============================================================================================================================
  | METHOD: ARM GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   "Arms" the gate so the next <see cref="EnsureLoaded"/> call suspends until <see cref="ReleaseGate"/> is called.
  /// </summary>
  public void ArmGate() => _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

  /*============================================================================================================================
  | METHOD: RELEASE GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Releases a suspended <see cref="EnsureLoaded"/> call "armed" via <see cref="ArmGate"/>.
  /// </summary>
  public void ReleaseGate() => _gate?.SetResult();

  /*============================================================================================================================
  | METHODS: TOPIC LAZY LOADER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override async Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default) {

    // Record the fetch
    FetchCount++;

    // If "armed", suspend until released
    if (_gate is not null) {
      await _gate.Task.ConfigureAwait(false);
    }

    // Delegate to the base implementation to perform the actual fill
    await base.EnsureLoaded(topic, payload, cancellationToken).ConfigureAwait(false);

  }

} //Class