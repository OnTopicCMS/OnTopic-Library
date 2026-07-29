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
///   A <see cref="StubLazyLoadingTopicRepository"/> that counts every <see cref="Load(Int32, Topic?, TopicPayload, Int32)"/>
///   and <see cref="EnsureLoaded"/> call and, while "armed", suspends inside the corresponding one until released, thus
///   letting a test provably interleave two concurrent lazy loads of the same topic without <see cref="Thread.Sleep(int)"/> or
///   other timing hacks.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class BlockingStubLazyLoadingTopicRepository: StubLazyLoadingTopicRepository {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private                       TaskCompletionSource?           _loadGate;
  private                       TaskCompletionSource?           _gate;

  /*============================================================================================================================
  | PROPERTY: LOAD FETCH COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="Load(Int32, Topic?, TopicPayload, Int32)"/> has been called.
  /// </summary>
  public int                    LoadFetchCount                  { get; private set; }

  /*============================================================================================================================
  | PROPERTY: FETCH COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="EnsureLoaded"/> has been called.
  /// </summary>
  public int                    FetchCount                      { get; private set; }

  /*============================================================================================================================
  | METHOD: ARM LOAD GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   "Arms" the gate so the next <see cref="Load(Int32, Topic?, TopicPayload, Int32)"/> call suspends until <see cref=
  ///   "ReleaseLoadGate"/> is called.
  /// </summary>
  public void ArmLoadGate() => _loadGate = new(TaskCreationOptions.RunContinuationsAsynchronously);

  /*============================================================================================================================
  | METHOD: RELEASE LOAD GATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Releases a suspended <see cref="Load(Int32, Topic?, TopicPayload, Int32)"/> call "armed" via <see cref="ArmLoadGate"/>.
  /// </summary>
  public void ReleaseLoadGate() => _loadGate?.SetResult();

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
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    TopicPayload payload        = TopicPayload.None,
    int depth                   = 0
  ) {

    // Record the fetch
    LoadFetchCount++;

    // If "armed", suspend until released
    if (_loadGate is not null) {
      await _loadGate.Task.ConfigureAwait(false);
    }

    // Delegate to the base implementation to perform the actual fill
    return await base.Load(topicId, referenceTopic, payload, depth).ConfigureAwait(false);

  }

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