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
    | TEST: SET RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible.
    /// </summary>
    [TestMethod]
    public void SetAttributeValueTest() {
      var parent = new Topic();
      var related = Topic.Create("Related", "Page");
      var relationships = new RelatedTopicCollection(parent);

      relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(relationships.GetTopics("Friends").First(), related);

    }

    /*==========================================================================================================================
    | TEST: RECIPROCAL RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible on incoming relationships property.
    /// </summary>
    [TestMethod]
    public void ReciprocalRelationshipTest() {
      var parent = new Topic();
      var related = Topic.Create("Related", "Page");
      var relationships = new RelatedTopicCollection(parent);

      relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(related.IncomingRelationships.GetTopics("Friends").First(), parent);

    }

    /*==========================================================================================================================
    | TEST: GET ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and ensures they are all returned via GetAllTopics().
    /// </summary>
    [TestMethod]
    public void GetAllTopicsTest() {
      var parent = new Topic();
      var relationships = new RelatedTopicCollection(parent);

      for(var i=0; i<5; i++) {
        relationships.SetTopic("Relationship" + i, Topic.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<string>("Related3", relationships.GetTopics("Relationship3").First().Key);
      Assert.AreEqual<int>(5, relationships.GetAllTopics().Count());

    }

  } //Class
} //Namespace
