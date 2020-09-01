/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  [TestClass]
  public class TopicCollectionTest {

    /*==========================================================================================================================
    | TEST: SET TOPIC: INDEXER: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then accesses them by key.
    /// </summary>
    [TestMethod]
    public void SetTopic_Indexer_ReturnsTopic() {

      var topics = new TopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      Assert.AreEqual<string>("Topic3", topics["Topic3"].Key);

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: IENUMERABLE: SEEDS TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then seeds a new <see cref="TopicCollection{T}"/> with them.
    /// </summary>
    [TestMethod]
    public void Constructor_IEnumerable_SeedsTopics() {

      var topics = new List<Topic>();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var topicsCollection = new TopicCollection(topics);

      Assert.AreEqual<int>(10, topicsCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: AS READ ONLY: RETURNS READ ONLY TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, converts the collection to read only, and ensures they are still present.
    /// </summary>
    [TestMethod]
    public void AsReadOnly_ReturnsReadOnlyTopicCollection() {

      var topics = new TopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var readOnlyCollection = topics.AsReadOnly();

      Assert.AreEqual<int>(10, readOnlyCollection.Count);
      Assert.AreEqual<string>("Topic0", readOnlyCollection.First().Key);

    }

  } //Class
} //Namespace