/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Repositories;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;
using System.Collections.Generic;

namespace Ignia.Topics.Tests.TestDoubles {

  /*============================================================================================================================
  | CLASS: FAKE TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics hard-coded in this fake implementation of a <see cref="ITopicRepository"/>.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITopicRepository"/> without maintaining a dependency on a live
  ///   database, or working against actual data. This is faster and safer for test methods.
  /// </remarks>

  public class FakeTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    int                         _identity                       = 1;
    Topic                       _cache                          = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the FakeTopicRepository.
    /// </summary>
    /// <returns>A new instance of the FakeTopicRepository.</returns>
    public FakeTopicRepository() : base() {
      CreateFakeData();
    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
    /// </summary>
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() {
      throw new NotImplementedException();
    }

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
      throw new NotImplementedException();
    }

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(string topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrWhiteSpace(topicKey)) {
        throw new NotImplementedException();
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
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Get topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = Load(topicId);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset version
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.LastModified = version;

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

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
    ///   The Content Type <c>topic.Attributes.GetValue(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found under
    ///   Configuration:ContentTypes. There are <c>TopicRepository.ContentTypes.Count</c> ContentTypes in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via
    ///   <c>ConfigurationManager.ConnectionStrings[TopicsServer].ConnectionString</c>: <c>ex.Message</c>
    /// </exception>
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse through children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Id < 0) {
        topic.Id = _identity++;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse through children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          Save(childTopic, isRecursive, isDraft);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return identity
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic.Id;

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
    public override void Move(Topic topic, Topic target, Topic sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset dirty status
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ParentId", target.Id.ToString(), false);

    }

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
    public override void Delete(Topic topic, bool isRecursive = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Delete(topic, isRecursive);

    }

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data that loosely mimics a barebones database.
    /// </summary>
    private void CreateFakeData() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = TopicFactory.Create("Root", "Container");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish configuration
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration = TopicFactory.Create("Configuration", "Container", rootTopic);
      var contentTypes = TopicFactory.Create("ContentTypes", "ContentTypeDescriptor", configuration);

      TopicFactory.Create("ContentType", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("Page", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("Container", "ContentTypeDescriptor", contentTypes);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish metadata
      \-----------------------------------------------------------------------------------------------------------------------*/
      var metadata = TopicFactory.Create("Metadata", "Container", configuration);
      var categories = TopicFactory.Create("Categories", "Lookup", metadata);
      var lookup = TopicFactory.Create("LookupList", "List", categories);

      for (var i=1; i<=5; i++) {
        TopicFactory.Create("Category" + i, "LookupListItem", lookup);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish content
      \-----------------------------------------------------------------------------------------------------------------------*/
      var web = TopicFactory.Create("Web", "Page", rootTopic);

      CreateFakeData(web, 3, 3);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set to cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      _cache = rootTopic;

    }

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data recursively based on a parent topic, and set number of levels.
    /// </summary>
    private void CreateFakeData(Topic parent, int count = 3, int depth = 3) {
      for (var i = 0; i < count; i++) {
        var topic = TopicFactory.Create(parent.Key + "_" + i, "Page", parent);
        topic.Attributes.SetValue("ParentKey", parent.Key);
        topic.Attributes.SetValue("DepthCount", (depth+i).ToString());
        if (depth > 0) {
          CreateFakeData(topic, count, depth - 1);
        }
      }
    }

  } //Class

} //Namespace
