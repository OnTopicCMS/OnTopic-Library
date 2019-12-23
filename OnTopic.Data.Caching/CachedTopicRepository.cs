/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Data.Caching {

  /*============================================================================================================================
  | CLASS: CACHED TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in memory.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="OnTopic.Repositories.ITopicRepository"/> class, which provides a wrapper
  ///   for an actual data access class.
  /// </remarks>

  public class CachedTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _dataProvider;
    private readonly            Topic                           _cache;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the CachedTopicRepository with a dependency on an underlying ITopicRepository in order
    ///   to provide necessary data access.
    /// </summary>
    /// <param name="dataProvider">A concrete instance of an ITopicRepository, which will be used for data access.</param>
    /// <returns>A new instance of the CachedTopicRepository.</returns>
    public CachedTopicRepository(ITopicRepository dataProvider) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(dataProvider, "A concrete implementation of an ITopicRepository is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _dataProvider = dataProvider;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = _dataProvider.Load();

      Contract.Assume(
        rootTopic,
        $"The topic graph could not be successfully loaded from the {nameof(ITopicRepository)} instance. The " +
        $"{nameof(CachedTopicRepository)} is unable to establish the cache."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      _cache = rootTopic;

    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() => _dataProvider.GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle request for entire tree
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId < 0) {
        return _cache;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recursive search
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache.FindFirst(t => t.Id.Equals(topicId));

    }

    /// <inheritdoc />
    public override Topic? Load(string? topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicKey != null && !topicKey.Length.Equals(0)) {
        return GetTopic(_cache, topicKey);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Return appropriate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _dataProvider.Load(topicId, version);

    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a topic object based on the specified partial or full (prefixed) topic key.
    /// </summary>
    /// <param name="sourceTopic">The root topic to search from.</param>
    /// <param name="uniqueKey">
    ///   The partial or full string value representing the key (or <see cref="Topic.GetUniqueKey"/>) for the topic.
    /// </param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    private Topic? GetTopic(Topic? sourceTopic, string uniqueKey) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceTopic == null) return null;
      if (String.IsNullOrWhiteSpace(uniqueKey)) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide shortcut for local calls
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (uniqueKey.IndexOf(":", StringComparison.InvariantCulture) < 0 && uniqueKey != "Root") {
        if (sourceTopic.Children.Contains(uniqueKey)) {
          return sourceTopic.Children[uniqueKey];
        }
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide implicit root
      >-------------------------------------------------------------------------------------------------------------------------
      | ###NOTE JJC080313: While a root topic is required by the data structure, it should be implicit from the perspective of
      | the calling application.  A developer should be able to call GetTopic("Namespace:TopicPath") to get to a topic, without
      | needing to be aware of the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        !uniqueKey.StartsWith("Root:", StringComparison.OrdinalIgnoreCase) &&
        !uniqueKey.Equals("Root", StringComparison.OrdinalIgnoreCase)
        ) {
        uniqueKey = "Root:" + uniqueKey;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!uniqueKey.StartsWith(sourceTopic.GetUniqueKey(), StringComparison.OrdinalIgnoreCase)) return null;
      if (uniqueKey.Equals(sourceTopic.GetUniqueKey(), StringComparison.OrdinalIgnoreCase)) return sourceTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var remainder = uniqueKey.Substring(sourceTopic.GetUniqueKey().Length + 1);
      var marker = remainder.IndexOf(":", StringComparison.Ordinal);
      var nextChild = (marker < 0) ? remainder : remainder.Substring(0, marker);

      /*------------------------------------------------------------------------------------------------------------------------
      | Find topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!sourceTopic.Children.Contains(nextChild)) return null;

      if (nextChild == remainder) return sourceTopic.Children[nextChild];

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return GetTopic(sourceTopic.Children[nextChild], uniqueKey);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) =>
      _dataProvider.Save(topic, isRecursive, isDraft);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Contracts",
      "TestAlwaysEvaluatingToAConstant",
      Justification = "Sibling may be null from overloaded caller."
      )]
    public override void Move(Topic topic, Topic target, Topic? sibling) => _dataProvider.Move(topic, target, sibling);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive = false) => _dataProvider.Delete(topic, isRecursive);

  } //Class
} //Namespace