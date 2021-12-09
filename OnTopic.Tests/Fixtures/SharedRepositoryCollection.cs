/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.TestDoubles;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

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

#pragma warning restore CA1711 // Identifiers should not have incorrect suffix