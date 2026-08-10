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
///   A <see cref="StubLazyLoadingTopicRepository"/> that counts every <see cref="LoadTopic(Int32, Topic?, TopicPayload, Int32)"
///   /> and <see cref="EnsureLoaded"/> call and, while "armed", suspends inside the corresponding one until released, thus
///   letting a test provably interleave two concurrent lazy loads of the same topic without <see cref="Thread.Sleep(int)"/> or
///   other timing hacks.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class BlockingStubLazyLoadingTopicRepository: StubLazyLoadingTopicRepository {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private                       TaskCompletionSource?           _loadGate;
  private                       TaskCompletionSource?           _ensureLoadedGate;

  /*============================================================================================================================
  | PROPERTY: LOAD FETCH COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="LoadTopic(Int32, Topic?, TopicPayload, Int32)"/> has been called.
  /// </summary>
  public int                    LoadFetchCount                  { get; private set; }

  /*============================================================================================================================
  | PROPERTY: ENSURE LOADED FETCH COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="EnsureLoaded"/> has been called.
  /// </summary>
  public int                    EnsureLoadedFetchCount          { get; private set; }

  /*============================================================================================================================
  | METHOD: ARM LOAD GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   "Arms" the gate so the next <see cref="LoadTopic(Int32, Topic?, TopicPayload, Int32)"/> call suspends until <see cref=
  ///   "ReleaseLoadGate"/> is called.
  /// </summary>
  public void ArmLoadGate() => _loadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);

  /*============================================================================================================================
  | METHOD: RELEASE LOAD GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Releases a suspended <see cref="LoadTopic(Int32, Topic?, TopicPayload, Int32)"/> call "armed" via <see cref=
  ///   "ArmLoadGate"/>.
  /// </summary>
  public void ReleaseLoadGate() => _loadGate?.SetResult();

  /*============================================================================================================================
  | METHOD: ARM ENSURE LOADED GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   "Arms" the gate so the next <see cref="EnsureLoaded"/> call suspends until <see cref="ReleaseEnsureLoadedGate"/> is
  ///   called.
  /// </summary>
  public void ArmEnsureLoadedGate() => _ensureLoadedGate = new(TaskCreationOptions.RunContinuationsAsynchronously);

  /*============================================================================================================================
  | METHOD: RELEASE ENSURE LOADED GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Releases a suspended <see cref="EnsureLoaded"/> call "armed" via <see cref="ArmEnsureLoadedGate"/>.
  /// </summary>
  public void ReleaseEnsureLoadedGate() => _ensureLoadedGate?.SetResult();

  /*============================================================================================================================
  | METHOD: FAULT ENSURE LOADED GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Faults a suspended <see cref="EnsureLoaded"/> call "armed" via <see cref="ArmEnsureLoadedGate"/> with the supplied
  ///   <paramref name="exception"/>, so a test can simulate a lazy load that throws while a second pass awaits the same entry.
  /// </summary>
  /// <param name="exception">The exception to surface from the suspended <see cref="EnsureLoaded"/> call.</param>
  public void FaultEnsureLoadedGate(Exception exception) => _ensureLoadedGate?.SetException(exception);

  /*============================================================================================================================
  | METHOD: LOAD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override async Task<Topic?> LoadTopic(
    int topicId,
    Topic? referenceTopic,
    TopicPayload payload,
    int depth
  ) {

    // Record the fetch
    LoadFetchCount++;

    // If "armed", suspend until released
    if (_loadGate is not null) {
      await _loadGate.Task.ConfigureAwait(false);
    }

    // Delegate to the base implementation to perform the actual fill
    return await base.LoadTopic(topicId, referenceTopic, payload, depth).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHODS: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override async Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default) {

    // Record the fetch
    EnsureLoadedFetchCount++;

    // If "armed", suspend until released
    if (_ensureLoadedGate is not null) {
      await _ensureLoadedGate.Task.ConfigureAwait(false);
    }

    // Delegate to the base implementation to perform the actual fill
    await base.EnsureLoaded(topic, payload, cancellationToken).ConfigureAwait(false);

  }

} //Class