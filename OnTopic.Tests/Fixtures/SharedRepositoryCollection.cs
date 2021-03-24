/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.TestDoubles;
using Xunit;

namespace OnTopic.Tests.Fixtures {

  /*============================================================================================================================
  | CLASS: SHARED REPOSITORY COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a common context used to share an implementation of the <see cref="TopicInfrastructureFixture{T}"/> between
  ///   tests using a common <see cref="StubTopicRepository"/>.
  /// </summary>
  [CollectionDefinition("Shared Repository")]
  public class SharedRepositoryCollection: ICollectionFixture<TopicInfrastructureFixture<StubTopicRepository>> {

  }
}
