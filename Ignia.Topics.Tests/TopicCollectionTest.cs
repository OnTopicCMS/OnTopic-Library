/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ignia.Topics.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  [TestClass]
  public class TopicCollectionTest {

    /*==========================================================================================================================
    | TEST: INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then accesses them by key.
    /// </summary>
    [TestMethod]
    public void TopicCollection_SetTopicTest() {

      var topics = new TopicCollection();

      for(var i=0; i<10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      Assert.AreEqual<string>("Topic3", topics["Topic3"].Key);

    }

    /*==========================================================================================================================
    | TEST: PREPOPULATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, then seeds a new <see cref="TopicCollection{T}"/> with them.
    /// </summary>
    [TestMethod]
    public void TopicCollection_PrepopulateTest() {

      var topics = new List<Topic>();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var topicsCollection = new TopicCollection(topics);

      Assert.AreEqual<int>(10, topicsCollection.Count);

    }

    /*==========================================================================================================================
    | TEST: AS READ ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, converts the collection to read only, and ensures they are still present.
    /// </summary>
    [TestMethod]
    public void TopicCollection_AsReadOnlyTest() {

      var topics = new TopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      var readOnlyCollection = topics.AsReadOnly();

      Assert.AreEqual<int>(10, readOnlyCollection.Count);
      Assert.AreEqual<string>("Topic0", readOnlyCollection.First().Key);

    }

    /*==========================================================================================================================
    | TEST: SORTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a number of topics, with varying <see cref="Topic.SortOrder"/>s, and ensures the results are sorted.
    /// </summary>
    [TestMethod]
    public void TopicCollection_SortedTest() {

      var topics = new TopicCollection();

      for (var i = 0; i < 10; i++) {
        topics.Add(TopicFactory.Create("Topic" + i, "Page"));
      }

      for (var i = 0; i < 10; i++) {
        topics[i].SortOrder = 50-i;
      }

      Assert.AreEqual<string>("Topic9", topics.Sorted.First().Key);

    }

  } //Class
} //Namespace
