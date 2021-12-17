/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Querying;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Host.Repositories {

  /*============================================================================================================================
  | CLASS: STUB TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics hard-coded in this stub implementation of a <see cref="ITopicRepository"/>.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITopicRepository"/> without maintaining a dependency on a live
  ///   database, or working against actual data. This is faster and safer for test methods since it doesn't maintain a
  ///   dependency on a live database or persistent data.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class StubTopicRepository : TopicRepository, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Topic                           _cache;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the StubTopicRepository.
    /// </summary>
    /// <returns>A new instance of the StubTopicRepository.</returns>
    public StubTopicRepository() : base() {
      _cache = CreateFakeData();
      Contract.Assume(_cache);
    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicId
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = _cache;

      if (topicId > 0) {
        topic = _cache.FindFirst(t => t.Id.Equals(topicId));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic != null) {
        OnTopicLoaded(new(topic, isRecursive));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <inheritdoc />
    public override Topic? Load(string uniqueKey, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(uniqueKey)) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = _cache.GetByUniqueKey(uniqueKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic != null) {
        OnTopicLoaded(new(topic, isRecursive));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) =>
      throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Refresh(Topic referenceTopic, DateTime since) =>
      throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void SaveTopic([NotNull]Topic topic, DateTime version, bool persistRelationships) =>
      throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void MoveTopic(Topic topic, Topic target, Topic? sibling = null) =>
      throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void DeleteTopic(Topic topic) =>
      throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data that loosely mimics a bare bones database.
    /// </summary>
    private static Topic CreateFakeData() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentAttributeId    = 1;
      var rootTopic             = new Topic("Root", "Container", null, currentAttributeId++);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var web                   = new Topic("Web", "Page", rootTopic, currentAttributeId++);
      _                         = new Topic("ContentList", "ContentList", web, currentAttributeId++);
      _                         = new Topic("MissingView", "Missing", web, currentAttributeId++);
      _                         = new Topic("Container", "Container", web, currentAttributeId++);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish area topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var area                  = new Topic("Area", "ContentType", rootTopic, currentAttributeId++);

      _                         = new Topic("AreaContentTypes", "AreaContentTypes", area, currentAttributeId++);
      _                         = new Topic("Accordion", "ContentList", area, currentAttributeId++);

      _                         = new Topic("TopicWithView", "ContentList", area, currentAttributeId++) {
        View                    = "Accordion"
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish error topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var error                 = new Topic("Error", "Page", rootTopic, currentAttributeId++);

      _                         = new Topic("400", "Page", error, currentAttributeId++);
      _                         = new Topic("Unauthorized", "Page", error, currentAttributeId++);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish caching tests
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheProfile          = new Topic("CacheProfile", "CacheProfile", rootTopic, currentAttributeId++);
      var cachedPage            = new Topic("CachedPage", "Page", web, currentAttributeId++);
      var uncachedPage          = new Topic("UncachedPage", "Page", web, currentAttributeId++);

      cacheProfile.Attributes.SetValue("Duration", "10");
      cacheProfile.Attributes.SetValue("Location", "Any");

      cachedPage.References.SetValue("CacheProfile", cacheProfile);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set to cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return rootTopic;

    }

  } //Class
} //Namespace