/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="RelatedTopicCollection"/> class.
  /// </summary>
  [TestClass]
  public class RelatedTopicCollectionTest {

    /*==========================================================================================================================
    | TEST: SET TOPIC: CREATES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible.
    /// </summary>
    [TestMethod]
    public void SetTopic_CreatesRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      parent.Relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(parent.Relationships.GetTopics("Friends").First(), related);

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that the <see cref="RelatedTopicCollection.IsDirty"/> returns true.
    /// </summary>
    [TestMethod]
    public void SetTopic_IsDirty() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      parent.Relationships.SetTopic("Friends", related);

      Assert.IsTrue(parent.Relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: REMOVES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_RemovesRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      parent.Relationships.SetTopic("Friends", related);
      parent.Relationships.RemoveTopic("Friends", related.Key);

      Assert.IsNull(parent.Relationships.GetTopics("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: REMOVES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed from the incoming relationships
    ///   property.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_RemovesIncomingRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      relationships.SetTopic("Friends", related);
      relationships.RemoveTopic("Friends", related.Key);

      Assert.IsNull(related.IncomingRelationships.GetTopics("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: REMOVES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible on incoming relationships property.
    /// </summary>
    [TestMethod]
    public void SetTopic_CreatesIncomingRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(related.IncomingRelationships.GetTopics("Friends").First(), parent);

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: UPDATES KEY COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and the correct number of keys are returned.
    /// </summary>
    [TestMethod]
    public void SetTopic_UpdatesKeyCount() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Keys.Count);
      Assert.IsTrue(relationships.Keys.Contains("Relationship3"));

    }

    /*==========================================================================================================================
    | TEST: GET ALL TOPICS: RETURNS ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and ensures they are all returned via GetAllTopics().
    /// </summary>
    [TestMethod]
    public void GetAllTopics_ReturnsAllTopics() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<string>("Related3", relationships.GetTopics("Relationship3").First().Key);
      Assert.AreEqual<int>(5, relationships.GetAllTopics().Count());

    }

    /*==========================================================================================================================
    | TEST: GET ALL CONTENT TYPES: RETURNS ALL CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, with different ContentTypes, then filters the results of
    ///   <see cref="RelatedTopicCollection.GetAllTopics(String)"/> by content type.
    /// </summary>
    [TestMethod]
    public void GetAllContentTypes_ReturnsAllContentTypes() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "ContentType" + i));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<int>(1, relationships.GetAllTopics("ContentType3").Count());

    }

  } //Class
} //Namespace