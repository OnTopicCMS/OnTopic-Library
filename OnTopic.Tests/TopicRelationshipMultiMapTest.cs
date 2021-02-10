/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections.Specialized;
using OnTopic.Associations;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC RELATIONSHIP MULTI-MAP TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRelationshipMultiMap"/> class.
  /// </summary>
  [TestClass]
  public class TopicRelationshipMultiMapTest {

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

      Assert.ReferenceEquals(parent.Relationships.GetValues("Friends").First(), related);

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
      parent.Relationships.RemoveTopic("Friends", related);

      Assert.IsNull(parent.Relationships.GetValues("Friends").FirstOrDefault());

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
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetTopic("Friends", related);
      relationships.RemoveTopic("Friends", related);

      Assert.IsNull(related.IncomingRelationships.GetValues("Friends").FirstOrDefault());

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
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetTopic("Friends", related);

      Assert.ReferenceEquals(related.IncomingRelationships.GetValues("Friends").First(), parent);

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
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Keys.Count);
      Assert.IsTrue(relationships.Keys.Contains("Relationship3"));

    }

    /*==========================================================================================================================
    | TEST: GET ALL VALUES: RETURNS ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and ensures they are all returned via <see cref="ReadOnlyTopicMultiMap.
    ///   GetAllValues(String)"/>.
    /// </summary>
    [TestMethod]
    public void GetAllValues_ReturnsAllTopics() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<string>("Related3", relationships.GetValues("Relationship3").First().Key);
      Assert.AreEqual<int>(5, relationships.GetAllValues().Count);

    }

    /*==========================================================================================================================
    | TEST: GET ALL VALUES: CONTENT TYPES: RETURNS ALL CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, with different ContentTypes, then filters the results of
    ///   <see cref="ReadOnlyTopicMultiMap.GetAllValues(String)"/> by content type.
    /// </summary>
    [TestMethod]
    public void GetAllValues_ContentTypes_ReturnsAllContentTypes() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetTopic("Relationship" + i, TopicFactory.Create("Related" + i, "ContentType" + i));
      }

      Assert.AreEqual<int>(5, relationships.Keys.Count);
      Assert.AreEqual<int>(1, relationships.GetAllValues("ContentType3").Count);

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that <see cref="TopicRelationshipMultiMap.
    ///   IsDirty()"/> is set.
    /// </summary>
    [TestMethod]
    public void SetTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetTopic("Related", related);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: IS DUPLICATE: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see
    ///   cref="TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetTopic_IsDuplicate_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 2);

      relationships.SetTopic("Related", related);
      relationships.MarkClean();

      relationships.SetTopic("Related", related);

      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: IS DUPLICATE: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see
    ///   cref="TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetTopic_IsDuplicate_StaysDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related1              = TopicFactory.Create("Topic", "Page");
      var related2              = TopicFactory.Create("Topic", "Page");

      relationships.SetTopic("Related", related1);
      relationships.SetTopic("Related", related2);

      Assert.IsTrue(relationships.IsDirty());

    }


    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes an existing <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value for <see
    ///   cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetTopic("Related", related);
      relationships.MarkClean();
      relationships.RemoveTopic("Related", related);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: MISSING TOPIC: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value for
    ///   <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_MissingTopic_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      var isSuccessful          = relationships.RemoveTopic("Related", related);

      Assert.IsFalse(isSuccessful);
      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: MISSING TOPIC: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value for
    ///   <see cref="TopicRelationshipMultiMap.IsDirty()"/> stays <c>true</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_MissingTopic_StaysDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic1", "Page");
      var missing               = TopicFactory.Create("Topic2", "Page");

      relationships.SetTopic("Related", related);

      var isSuccessful          = relationships.RemoveTopic("Related", missing);

      Assert.IsFalse(isSuccessful);
      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING TOPICS: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> and confirms that value of <see
    ///   cref="TopicRelationshipMultiMap.IsDirty()"/> is <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingTopics_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetTopic("Related", related);
      relationships.MarkClean();
      relationships.Clear("Related");

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: NO TOPICS: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> with no existing <see cref="Topic"/>s and confirms that
    ///   the value of <see cref="TopicRelationshipMultiMap.IsDirty()"/> is set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Clear_NoTopics_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);

      relationships.Clear("Related");

      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: NEW PARENT: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an existing <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic.
    ///   IsNew"/> <see cref="Topic"/> and confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c>
    ///   even if <see cref="TopicRelationshipMultiMap.SetTopic(String, Topic, Boolean?, Boolean)"/> is called with the <c>
    ///   markDirty</c> parameter set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetTopic_NewParent_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 1);

      relationships.SetTopic("Related", related, false);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: NEW TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with an existing <see cref="
    ///   Topic"/> and confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c> even if <see cref="
    ///   TopicRelationshipMultiMap.SetTopic(String, Topic, Boolean?, Boolean)"/> is called with the <c>markDirty</c> parameter
    ///   set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetTopic_NewTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetTopic("Related", related, false);

      Assert.IsTrue(relationships.IsDirty());

    }

  } //Class
} //Namespace