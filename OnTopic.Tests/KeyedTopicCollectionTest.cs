/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: KEYED TOPIC COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="KeyedTopicCollection"/> class.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class KeyedTopicCollectionTest {

    /*==========================================================================================================================
    | TEST: SET TOPIC: INDEXER: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then accesses them by key.
    /// </summary>
    [TestMethod]
    public void SetTopic_Indexer_ReturnsTopic() {

      var topics = new KeyedTopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      Assert.AreEqual<string>("Topic3", topics["Topic3"].Key);

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: IENUMERABLE: SEEDS TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then seeds a new <see cref="KeyedTopicCollection{T}"/> with them.
    /// </summary>
    [TestMethod]
    public void Constructor_IEnumerable_SeedsTopics() {

      var topics = new List<Topic>();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var topicsCollection = new KeyedTopicCollection(topics);

      Assert.AreEqual<int>(10, topicsCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: INSERT ITEM: DUPLICATE KEY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to add two <see cref="Topic"/> instances with the same <see cref="Topic.Key"/> to a <see cref="
    ///   KeyedTopicCollection{T}"/> and confirms that a <see cref="ArgumentException"/> is correctly thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void InsertItem_DuplicateKey_ThrowsException() =>
      new KeyedTopicCollection {
        new Topic("Key", "Page"),
        new Topic("Key", "Page")
      };

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: EMPTY COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection"/> without a backing <see cref="KeyedTopicCollection"/>
    ///   and confirms that it successfully initialized with zero items.
    /// </summary>
    [TestMethod]
    public void ReadOnlyKeyedTopicCollection_EmptyCollection() {

      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection();

      Assert.AreEqual<int>(0, readOnlyCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: GET VALUE: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it successfully returns a <see cref="Topic"/> by <see cref="Topic.Key"/> using <see cref="
    ///   ReadOnlyKeyedTopicCollection{T}.GetValue(String)"/>.
    /// </summary>
    [TestMethod]
    public void ReadOnlyKeyedTopicCollection_GetValue_ReturnsValue() {

      var collection            = new KeyedTopicCollection();
      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection(collection);
      var topic                 = new Topic("Topic", "Page");

      collection.Add(topic);

      Assert.AreEqual<Topic?>(topic, readOnlyCollection.GetValue(topic.Key));

    }

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: INDEXER: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it successfully returns a <see cref="Topic"/> by <see cref="Topic.Key"/> using the indexer on <see cref=
    ///   "ReadOnlyKeyedTopicCollection{T}"/>.
    /// </summary>
    [TestMethod]
    public void ReadOnlyKeyedTopicCollection_Indexer_ReturnsValue() {

      var collection            = new KeyedTopicCollection();
      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection(collection);
      var topic                 = new Topic("Topic", "Page");

      collection.Add(topic);

      Assert.AreEqual<Topic?>(topic, readOnlyCollection[topic.Key]);

    }


    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: GET VALUE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it returns null if when calling <see cref="ReadOnlyKeyedTopicCollection{T}.GetValue(String)"/> with an
    ///   invalid <see cref="Topic.Key"/>.
    /// </summary>
    [TestMethod]
    public void ReadOnlyKeyedTopicCollection_GetValue_ReturnsNull() {

      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection();

      Assert.IsNull(readOnlyCollection.GetValue("InvalidKey"));

    }

    /*==========================================================================================================================
    | TEST: AS READ ONLY: RETURNS READ ONLY KEYED TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, converts the collection to read only, and ensures they are still present.
    /// </summary>
    [TestMethod]
    public void AsReadOnly_ReturnsReadOnlyKeyedTopicCollection() {

      var topics = new KeyedTopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var readOnlyCollection = topics.AsReadOnly();

      Assert.AreEqual<int>(10, readOnlyCollection.Count);
      Assert.AreEqual<string>("Topic0", readOnlyCollection.First().Key);

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