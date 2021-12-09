/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Data.Caching;
using OnTopic.Lookup;
using OnTopic.Mapping;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.Tests.TestDoubles;

namespace OnTopic.Tests.Fixtures
{

  /*============================================================================================================================
  | CLASS: TOPIC INFRASTRUCTURE FIXTURE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Introduces a shared context to use for unit tests depending on an <see cref="ITypeLookupService"/>, <see cref="
  ///   ITopicRepository"/>, and, optionally, an <see cref="ITopicMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   This basic fixture uses the <see cref="TopicViewModelLookupService"/>, <see cref="FakeViewModelLookupService"/>, <see
  ///   cref="StubTopicRepository"/>, <see cref="CachedTopicMappingService"/>, and <see cref="TopicMappingService"/>.
  /// </remarks>
  public class TopicInfrastructureFixture<T> where T: ITopicRepository, new() {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    public TopicInfrastructureFixture() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Create composite topic lookup service
      \-----------------------------------------------------------------------------------------------------------------------*/
      TypeLookupService         = new CompositeTypeLookupService(
        new TopicViewModelLookupService(),
        new FakeViewModelLookupService()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Assemble dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository           = new T();
      CachedTopicRepository     = new CachedTopicRepository(TopicRepository);
      MappingService            = new TopicMappingService(CachedTopicRepository, TypeLookupService);

    }

    /*==========================================================================================================================
    | TYPE LOOKUP SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A <see cref="CompositeTypeLookupService"/> for accessing view models from the <see cref="TopicViewModelLookupService"
    ///   /> and the <see cref="FakeViewModelLookupService"/>.
    /// </summary>
    public ITypeLookupService TypeLookupService { get; private set; }

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A <typeparamref name="T"/> without a caching layer.
    /// </summary>
    public ITopicRepository TopicRepository { get; private set; }

    /*==========================================================================================================================
    | CACHED TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A <typeparamref name="T"/> wrapped in a <see cref="CachedTopicRepository"/>.
    /// </summary>
    public ITopicRepository CachedTopicRepository { get; private set; }

    /*==========================================================================================================================
    | MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A <see cref="TopicMappingService"/> based on a <see cref="StubTopicRepository"/>, looking up view models from the <see
    ///   cref="TopicViewModelLookupService"/> and the <see cref="FakeViewModelLookupService"/>.
    /// </summary>
    public ITopicMappingService MappingService { get; private set; }

  }
}
