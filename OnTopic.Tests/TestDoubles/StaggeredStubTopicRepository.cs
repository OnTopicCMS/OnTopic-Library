/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;
using OnTopic.TestDoubles;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: STAGGERED STUB TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   A <see cref="StubTopicRepository"/> that delays <see cref="LoadTopic(String, Topic?, TopicPayload, Int32)"/> by a per-key
///   <see cref="TimeSpan"/>, letting a test invert completion order relative to call order.
/// </summary>
/// <remarks>
///   <para>
///     This is similar to <see cref="StaggeredTopicLazyLoader"/>, except that it staggers calls to <see cref=
///     "ITopicRepository.Load()"/>, not <see cref="ITopicLazyLoader.EnsureLoaded"/>.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class StaggeredStubTopicRepository: StubTopicRepository {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              IReadOnlyDictionary<string, TimeSpan> _delaysByKey;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="StaggeredStubTopicRepository"/> with a delay for each unique key that
  ///   should complete out of call order.
  /// </summary>
  /// <param name="delaysByKey">A map of unique topic key to the delay that should precede its resolution.</param>
  public StaggeredStubTopicRepository(IReadOnlyDictionary<string, TimeSpan> delaysByKey) {
    _delaysByKey                = delaysByKey;
  }

  /*============================================================================================================================
  | METHOD: LOAD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override async Task<Topic?> LoadTopic(
    string uniqueKey,
    Topic? referenceTopic,
    TopicPayload payload,
    int depth
  ) {

    // Delay resolution of this key, if configured
    if (_delaysByKey.TryGetValue(uniqueKey, out var delay) && delay > TimeSpan.Zero) {
      await Task.Delay(delay).ConfigureAwait(false);
    }

    // Delegate to the base implementation to perform the actual lookup
    return await base.LoadTopic(uniqueKey, referenceTopic, payload, depth).ConfigureAwait(false);

  }

} //Class