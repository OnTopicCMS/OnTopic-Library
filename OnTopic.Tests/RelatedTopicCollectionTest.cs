/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using OnTopic.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
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
    | TEST: SET TOPIC: CREATES INCOMING RELATIONSHIP
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
      Assert.AreEqual<int>(5, relationships.GetAllTopics().Count);

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
      Assert.AreEqual<int>(1, relationships.GetAllTopics("ContentType3").Count);

    }

  } //Class
} //Namespace