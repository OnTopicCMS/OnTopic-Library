/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.TestDoubles;
using Xunit;

namespace OnTopic.Tests.Fixtures;

/*==============================================================================================================================
| CLASS: SHARED REPOSITORY COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Defines a common context used to share an implementation of the <see cref="TopicInfrastructureFixture{T}"/> between
///   tests using a common <see cref="StubTopicRepository"/>.
/// </summary>
[CollectionDefinition("Shared Repository")]
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Follows xUnit's naming convention for collection fixture definitions, not a .NET collection type.")]
public class SharedRepositoryCollection: ICollectionFixture<TopicInfrastructureFixture<StubTopicRepository>> {

}