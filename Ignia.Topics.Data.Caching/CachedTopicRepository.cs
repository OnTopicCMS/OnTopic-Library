/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Collections;
using Ignia.Topics.Metadata;
using Ignia.Topics.Repositories;
using Ignia.Topics.Querying;

namespace Ignia.Topics.Data.Caching {

  /*============================================================================================================================
  | CLASS: CACHED TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in memory.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="Ignia.Topics.Repositories.ITopicRepository"/> class, which provides a wrapper
  ///   for an actual data access class.
  /// </remarks>

  public class CachedTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _dataProvider                   = null;
    private                     Topic                           _cache                          = null;

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
      Contract.Requires(dataProvider != null, "A concrete implementation of an ITopicRepository is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _dataProvider = dataProvider;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      _cache = _dataProvider.Load();

    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
    /// </summary>
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() => _dataProvider.GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, bool isRecursive = true) {

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

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(string topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrWhiteSpace(topicKey)) {
        return GetTopic(_cache, topicKey);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <summary>
    ///   Loads a specific version of a topic based on its version.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, DateTime version) {

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
    private Topic GetTopic(Topic sourceTopic, string uniqueKey) {

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
    /// <summary>
    ///   Interface method that saves topic attributes; also used for renaming a topic since name is stored as an attribute.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>The integer return value from the execution of the <c>topics_UpdateTopic</c> stored procedure.</returns>
    /// <exception cref="Exception">
    ///   The Content Type <c>topic.Attributes.GetValue(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found
    ///   under Configuration:ContentTypes. There are <c>TopicRepository.ContentTypes.Count</c> ContentTypes in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via
    ///   <c>ConfigurationManager.ConnectionStrings[TopicsServer].ConnectionString</c>: <c>ex.Message</c>
    /// </exception>
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) =>
      _dataProvider.Save(topic, isRecursive, isDraft);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <remarks>
    ///   May optionally specify a sibling. If specified, it is expected that the topic will be placed immediately after the
    ///   topic.
    /// </remarks>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">The target (parent) topic object under which the topic should be moved.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Contracts",
      "TestAlwaysEvaluatingToAConstant",
      Justification = "Sibling may be null from overloaded caller."
      )]
    public override void Move(Topic topic, Topic target, Topic sibling) => _dataProvider.Move(topic, target, sibling);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well. If set to false
    ///   (the default) and the topic has children, including any nested topics, an exception will be thrown.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    /// <exception cref="Exception">Failed to delete Topic <c>topic.Key</c> (<c>topic.Id</c>): <c>ex.Message</c></exception>
    public override void Delete(Topic topic, bool isRecursive = false) => _dataProvider.Delete(topic, isRecursive);

  } //Class

} //Namespace
