/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Repositories;
using System.Diagnostics.Contracts;
using System.Configuration;
using System.Data;
using System.Xml;
using System.Globalization;
using Ignia.Topics.Collections;

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

  public class CachedTopicRepository : TopicRepositoryBase {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    ITopicRepository            _dataProvider                   = null;
    Topic                       _cache                          = null;

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
      _dataProvider = dataProvider;
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
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_cache == null) {
        _cache = _dataProvider.Load();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicId
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId >= 0) {
        return _cache.GetTopic(topicId);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(string topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_cache == null) {
        _cache = _dataProvider.Load();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrWhiteSpace(topicKey)) {
        return _cache.GetTopic(topicKey);
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
    public override Topic Load(int topicId, DateTime version) => _dataProvider.Load(topicId, version);

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
    ///   The Content Type <c>topic.Attributes.GetValue(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found under
    ///   Configuration:ContentTypes. There are <c>TopicRepository.ContentTypes.Count</c> ContentTypes in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via
    ///   <c>ConfigurationManager.ConnectionStrings[TopicsServer].ConnectionString</c>: <c>ex.Message</c>
    /// </exception>
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {
      return _dataProvider.Save(topic, isRecursive, isDraft);
    }

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
