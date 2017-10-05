/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Ignia.Topics.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  [TestClass]
  public class RelatedTopicCollectionTest {

    /*==========================================================================================================================
    | TEST: SET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible.
    /// </summary>
    [TestMethod]
    public void RelatedTopicCollection_SetTopic() {

      var parent                = new Topic();
      var related               = new Topic();

      parent.Relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(parent.Relationships.GetTopics("Friends").First(), related);

    }

    /*==========================================================================================================================
    | TEST: INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible on incoming relationships property.
    /// </summary>
    [TestMethod]
    public void RelatedTopicCollection_IncomingRelationshipTest() {

      var parent                = new Topic();
      var related               = Topic.Create("Related", "Page");
      var relationships         = new RelatedTopicCollection(parent);

      relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(related.IncomingRelationships.GetTopics("Friends").First(), parent);

    }

    /*==========================================================================================================================
    | TEST: KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and the correct number of keys are returned.
    /// </summary>
    [TestMethod]
    public void RelatedTopicCollection_KeysTest() {

      var parent                = new Topic();
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, Topic.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Keys.Count);
      Assert.IsTrue(relationships.Keys.Contains("Relationship3"));

    }

    /*==========================================================================================================================
    | TEST: GET ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and ensures they are all returned via GetAllTopics().
    /// </summary>
    [TestMethod]
    public void RelatedTopicCollection_GetAllTopicsTest() {

      var parent                = new Topic();
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, Topic.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<string>("Related3", relationships.GetTopics("Relationship3").First().Key);
      Assert.AreEqual<int>(5, relationships.GetAllTopics().Count());

    }

    /*==========================================================================================================================
    | TEST: GET ALL CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, with different ContentTypes, then filters the results of
    ///   <see cref="RelatedTopicCollection.GetAllTopics(string)"/> by content type.
    /// </summary>
    [TestMethod]
    public void RelatedTopicCollection_GetAllContentTypesTest() {

      var parent                = new Topic();
      var relationships         = new RelatedTopicCollection(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, Topic.Create("Related" + i, "ContentType" + i));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<int>(1, relationships.GetAllTopics("ContentType3").Count());

    }

  } //Class

} //Namespace
