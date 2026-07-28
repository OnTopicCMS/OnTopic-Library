/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;

namespace OnTopic.TestDoubles;

/*==============================================================================================================================
| CLASS: STUB SITEMAP TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides an <see cref="ISitemapTopicRepository"/> backed by an existing <see cref="ITopicRepository"/>, for testing
///   consumers of <see cref="ISitemapTopicRepository"/> without a SQL-backed <c>SqlSitemapTopicRepository</c>.
/// </summary>
/// <remarks>
///   Unlike a SQL-backed implementation, this does not source a lean, purpose-built graph; it simply defers to the wrapped
///   <see cref="ITopicRepository"/>'s own <see cref="ITopicRepository.Load(Int32, Topic?, TopicPayload, Int32)"/>, requesting
///   the full descendant tree explicitly since <c>depth</c> defaults to <c>0</c>.
/// </remarks>
/// <param name="topicRepository">The <see cref="ITopicRepository"/> to source the sitemap's topic graph from.</param>
[ExcludeFromCodeCoverage]
public class StubSitemapTopicRepository(ITopicRepository topicRepository) : ISitemapTopicRepository {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              ITopicRepository                _topicRepository                = Contract.Requires(topicRepository);

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<Topic> Load() {
    var topic                   = await _topicRepository.Load(-1, depth: -1).ConfigureAwait(false);
    Contract.Assume(topic, "The wrapped ITopicRepository did not return a topic graph.");
    return topic;
  }

} //Class