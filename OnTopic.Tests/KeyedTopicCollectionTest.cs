﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: KEYED TOPIC COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="KeyedTopicCollection"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class KeyedTopicCollectionTest {

    /*==========================================================================================================================
    | TEST: SET TOPIC: INDEXER: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then accesses them by key.
    /// </summary>
    [Fact]
    public void SetTopic_Indexer_ReturnsTopic() {

      var topics = new KeyedTopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(new("Topic" + i, "Page"));
      }

      Assert.Equal("Topic3", topics["Topic3"].Key);

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: IENUMERABLE: SEEDS TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then seeds a new <see cref="KeyedTopicCollection{T}"/> with them.
    /// </summary>
    [Fact]
    public void Constructor_IEnumerable_SeedsTopics() {

      var topics = new List<Topic>();

      for (var i = 0; i < 10; i++) {
        topics.Add(new("Topic" + i, "Page"));
      }

      var topicsCollection = new KeyedTopicCollection(topics);

      Assert.Equal(10, topicsCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: INSERT ITEM: DUPLICATE KEY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to add two <see cref="Topic"/> instances with the same <see cref="Topic.Key"/> to a <see cref="
    ///   KeyedTopicCollection{T}"/> and confirms that a <see cref="ArgumentException"/> is correctly thrown.
    /// </summary>
    [Fact]
    public void InsertItem_DuplicateKey_ThrowsException() =>
      Assert.Throws<ArgumentException>(() =>
        new KeyedTopicCollection {
          new Topic("Key", "Page"),
          new Topic("Key", "Page")
        }
      );

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: EMPTY COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection"/> without a backing <see cref="KeyedTopicCollection"/>
    ///   and confirms that it successfully initialized with zero items.
    /// </summary>
    [Fact]
    public void ReadOnlyKeyedTopicCollection_EmptyCollection() {

      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection();

      Assert.Empty(readOnlyCollection);

    }

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: GET VALUE: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it successfully returns a <see cref="Topic"/> by <see cref="Topic.Key"/> using <see cref="
    ///   ReadOnlyKeyedTopicCollection{T}.GetValue(String)"/>.
    /// </summary>
    [Fact]
    public void ReadOnlyKeyedTopicCollection_GetValue_ReturnsValue() {

      var collection            = new KeyedTopicCollection();
      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection(collection);
      var topic                 = new Topic("Topic", "Page");

      collection.Add(topic);

      Assert.Equal(topic, readOnlyCollection.GetValue(topic.Key));

    }

    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: INDEXER: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it successfully returns a <see cref="Topic"/> by <see cref="Topic.Key"/> using the indexer on <see cref=
    ///   "ReadOnlyKeyedTopicCollection{T}"/>.
    /// </summary>
    [Fact]
    public void ReadOnlyKeyedTopicCollection_Indexer_ReturnsValue() {

      var collection            = new KeyedTopicCollection();
      var readOnlyCollection    = new ReadOnlyKeyedTopicCollection(collection);
      var topic                 = new Topic("Topic", "Page");

      collection.Add(topic);

      Assert.Equal(topic, readOnlyCollection[topic.Key]);

    }


    /*==========================================================================================================================
    | TEST: READ ONLY KEYED TOPIC COLLECTION: GET VALUE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyKeyedTopicCollection{T}"/> with a backing <see cref="KeyedTopicCollection"/> and
    ///   confirms that it returns null if when calling <see cref="ReadOnlyKeyedTopicCollection{T}.GetValue(String)"/> with an
    ///   invalid <see cref="Topic.Key"/>.
    /// </summary>
    [Fact]
    public void ReadOnlyKeyedTopicCollection_GetValue_ReturnsNull() =>
      Assert.Null(new ReadOnlyKeyedTopicCollection().GetValue("InvalidKey"));

    /*==========================================================================================================================
    | TEST: AS READ ONLY: RETURNS READ ONLY KEYED TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, converts the collection to read only, and ensures they are still present.
    /// </summary>
    [Fact]
    public void AsReadOnly_ReturnsReadOnlyKeyedTopicCollection() {

      var topics = new KeyedTopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(new("Topic" + i, "Page"));
      }

      var readOnlyCollection = topics.AsReadOnly();

      Assert.Equal(10, readOnlyCollection.Count);
      Assert.Equal("Topic0", readOnlyCollection.First().Key);

    }

    /*==========================================================================================================================
    | TEST: AS READ ONLY: RETURNS READ ONLY TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, converts the collection to read only, and ensures they are still present.
    /// </summary>
    [Fact]
    public void AsReadOnly_ReturnsReadOnlyTopicCollection() {

      var topics = new TopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(new("Topic" + i, "Page"));
      }

      var readOnlyCollection = topics.AsReadOnly();

      Assert.Equal(10, readOnlyCollection.Count);
      Assert.Equal("Topic0", readOnlyCollection.First().Key);

    }

  } //Class
} //Namespace