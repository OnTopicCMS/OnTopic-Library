/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Associations;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC RELATIONSHIP MULTI-MAP TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRelationshipMultiMap"/> class.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class TopicRelationshipMultiMapTest {

    /*==========================================================================================================================
    | TEST: SET VALUE: CREATES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible.
    /// </summary>
    [TestMethod]
    public void SetValue_CreatesRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      parent.Relationships.SetValue("Friends", related);

      Assert.AreEqual<Topic?>(parent.Relationships.GetValues("Friends").First(), related);

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INVALID RELATIONSHIP KEY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a <see cref="Topic"/> from a <see cref="Topic.Relationships"/> where the <see cref="KeyValuesPair{
    ///   TKey, TValue}.Key"/> is invalid. In this case, <see cref="TopicRelationshipMultiMap.Remove(String, Topic)"/> should
    ///   return <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Remove_InvalidRelationshipKey_ReturnsFalse() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var unrelated             = TopicFactory.Create("Unrelated", "Page");

      Assert.IsFalse(parent.Relationships.Remove("Unrelated", unrelated));

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INVALID TOPIC KEY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a <see cref="Topic"/> from a <see cref="TopicMultiMap"/> where the <see cref="Topic"/> is does not
    ///   exist. In this case, <see cref="TopicMultiMap.Remove(String, Topic)"/> should return <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Remove_InvalidTopicKey_ReturnsFalse() {

      var multiMap              = new TopicMultiMap();
      var unrelated             = TopicFactory.Create("Unrelated", "Page");

      multiMap.Add("Related", new("Test", "Page"));

      Assert.IsFalse(multiMap.Remove("Related", unrelated));

    }

    /*==========================================================================================================================
    | TEST: REMOVE: REMOVES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed.
    /// </summary>
    [TestMethod]
    public void Remove_RemovesRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      parent.Relationships.SetValue("Friends", related);
      parent.Relationships.Remove("Friends", related);

      Assert.IsNull(parent.Relationships.GetValues("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: REMOVES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed from the incoming relationships
    ///   property.
    /// </summary>
    [TestMethod]
    public void Remove_RemovesIncomingRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetValue("Friends", related);
      relationships.Remove("Friends", related);

      Assert.IsNull(related.IncomingRelationships.GetValues("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: CREATES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible on incoming relationships property.
    /// </summary>
    [TestMethod]
    public void SetValue_CreatesIncomingRelationship() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetValue("Friends", related);

      Assert.AreEqual<Topic?>(related.IncomingRelationships.GetValues("Friends").First(), parent);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INCOMING RELATIONSHIPS: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to set a relationship on a <see cref="TopicRelationshipMultiMap"/> that is marked as <c>isIncoming</c>
    ///   without setting the <c>isIncoming</c> parameter on <see cref="TopicRelationshipMultiMap.SetValue(String, Topic,
    ///   Boolean?, Boolean)"/> and verifies that a <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void SetValue_IncomingRelationships_ThrowsException() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent, true);

      relationships.SetValue("Friends", related);

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INCOMING RELATIONSHIPS: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a relationship from a <see cref="TopicRelationshipMultiMap"/> that is marked as <c>isIncoming</c>
    ///   without setting the <c>isIncoming</c> parameter on <see cref="TopicRelationshipMultiMap.Remove(String, Topic, Boolean)
    ///   "/> and verifies that a <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Remove_IncomingRelationships_ThrowsException() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var related               = TopicFactory.Create("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent, true);

      relationships.Remove("Friends", related);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: UPDATES KEY COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and the correct number of keys are returned.
    /// </summary>
    [TestMethod]
    public void SetValue_UpdatesKeyCount() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetValue("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Keys.Count);
      Assert.IsTrue(relationships.Keys.Contains("Relationship3"));

    }

    /*==========================================================================================================================
    | TEST: GET ENUMERATOR: RETURNS KEY/VALUES PAIRS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Enumerates over the <see cref="ReadOnlyTopicMultiMap"/>, ensuring that the enumerator defined by the <see cref="
    ///   ReadOnlyTopicMultiMap.GetEnumerator()"/> interface implementation successfully relays the call to the underlying
    ///   <see cref="TopicMultiMap"/>.
    /// </summary>
    [TestMethod]
    public void GetEnumerator_ReturnsKeyValuesPairs() {

      var counter               = 0;
      var multiMap              = new TopicMultiMap();
      var readOnlyRelationships = new ReadOnlyTopicMultiMap(multiMap);

      for (var i = 0; i < 5; i++) {
        multiMap.Add(new("Relationship" + i, new()));
      }

      foreach (var pair in readOnlyRelationships) {
        counter++;
      }

      Assert.AreEqual<int>(5, readOnlyRelationships.Count);
      Assert.AreEqual<int>(5, counter);

    }

    /*==========================================================================================================================
    | TEST: INDEXER: RETURNS KEY/VALUES PAIRS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyTopicMultiMap"/> and ensures that a <see cref="ReadOnlyTopicCollection"/> can be
    ///   retrieved by <see cref="KeyValuesPair{TKey, TValue}.Key"/> via the <see cref="ReadOnlyTopicMultiMap"/> indexer.
    /// </summary>
    [TestMethod]
    public void Indexer_ReturnsKeyValuesPair() {

      var multiMap              = new TopicMultiMap();
      var readOnlyTopicMultiMap = new ReadOnlyTopicMultiMap(multiMap);
      var topics                = new TopicCollection();
      var keyValuesPair         = new KeyValuesPair<string, TopicCollection>("Relationship", topics);
      var topic                 = new Topic("Test", "Test");

      topics.Add(topic);
      multiMap.Add(keyValuesPair);

      Assert.AreEqual<int>(1, readOnlyTopicMultiMap.Count);
      Assert.AreEqual<Topic?>(topic, readOnlyTopicMultiMap["Relationship"].FirstOrDefault());

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
        relationships.SetValue("Relationship" + i, TopicFactory.Create("Related" + i, "Page"));
      }

      Assert.AreEqual<int>(5, relationships.Count);
      Assert.AreEqual<string>("Related3", relationships.GetValues("Relationship3").First().Key);
      Assert.AreEqual<int>(5, relationships.GetAllValues().Count);

    }

    /*==========================================================================================================================
    | TEST: GET ALL VALUES: CONTENT TYPES: RETURNS ALL CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, with different ContentTypes, then filters the results of <see cref="
    ///   ReadOnlyTopicMultiMap.GetAllValues(String)"/> by content type.
    /// </summary>
    [TestMethod]
    public void GetAllValues_ContentTypes_ReturnsAllContentTypes() {

      var parent                = TopicFactory.Create("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetValue("Relationship" + i, TopicFactory.Create("Related" + i, "ContentType" + i));
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

      relationships.SetValue("Related", related);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: IS DUPLICATE: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetValue_IsDuplicate_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 2);

      relationships.SetValue("Related", related);
      relationships.MarkClean();

      relationships.SetValue("Related", related);

      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: IS DUPLICATE: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetSetValue_IsDuplicate_StaysDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related1              = TopicFactory.Create("Topic", "Page");
      var related2              = TopicFactory.Create("Topic", "Page");

      relationships.SetValue("Related", related1);
      relationships.SetValue("Related", related2);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes an existing <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value for
    ///   <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Remove_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetValue("Related", related);
      relationships.MarkClean();
      relationships.Remove("Related", related);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: MISSING TOPIC: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value
    ///   for <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Remove_MissingTopic_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      var isSuccessful          = relationships.Remove("Related", related);

      Assert.IsFalse(isSuccessful);
      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: MISSING TOPIC: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value
    ///   for <see cref="TopicRelationshipMultiMap.IsDirty()"/> stays <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Remove_MissingTopic_StaysDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic1", "Page");
      var missing               = TopicFactory.Create("Topic2", "Page");

      relationships.SetValue("Related", related);

      var isSuccessful          = relationships.Remove("Related", missing);

      Assert.IsFalse(isSuccessful);
      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING TOPICS: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingTopics_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetValue("Related", related);
      relationships.MarkClean();
      relationships.Clear("Related");

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: NO TOPICS: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> with no existing <see cref="Topic"/>s and confirms that the
    ///   value of <see cref="TopicRelationshipMultiMap.IsDirty()"/> is set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Clear_NoTopics_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);

      relationships.Clear("Related");

      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: MARK NOT DIRTY: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an existing <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> and confirms that <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c> if <see cref="TopicRelationshipMultiMap.SetValue(String,
    ///   Topic, Boolean?, Boolean)"/> is called with the <c>markDirty</c> parameter set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetValue_MarkNotDirty_IsNotDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 2);

      relationships.SetValue("Related", related, false);

      Assert.IsFalse(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NEW PARENT: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an existing <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic.
    ///   IsNew"/> <see cref="Topic"/> and confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c>
    ///   even if <see cref="TopicRelationshipMultiMap.SetValue(String, Topic, Boolean?, Boolean)"/> is called with the <c>
    ///   markDirty</c> parameter set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetValue_NewParent_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 1);

      relationships.SetValue("Related", related, false);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NEW TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a new <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with an existing <see cref="
    ///   Topic"/> and confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c> even if <see cref="
    ///   TopicRelationshipMultiMap.SetValue(String, Topic, Boolean?, Boolean)"/> is called with the <c>markDirty</c> parameter
    ///   set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetValue_NewTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetValue("Related", related, false);

      Assert.IsTrue(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic"/>.
    ///   Confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c> after calling <see cref="
    ///   TopicRelationshipMultiMap.MarkClean(String)"/>.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkClean_ReturnsFalse() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 2);

      relationships.SetValue("Related", related);

      relationships.MarkClean("Related");

      Assert.IsFalse(relationships.IsDirty());
      Assert.IsFalse(relationships.IsDirty("Related"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic"/> that is
    ///   <see cref="Topic.IsNew"/>. Confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c> even
    ///   after calling <see cref="TopicRelationshipMultiMap.MarkClean(String)"/> since new topics cannot be clean.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkClean_ReturnsTrue() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page", 2);

      relationships.SetValue("Related", related, false);

      relationships.MarkClean();
      relationships.MarkClean("Related");

      Assert.IsTrue(relationships.IsDirty());
      Assert.IsTrue(relationships.IsDirty("Related"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN WITH NEW TOPIC: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic"/>.
    ///   Confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c> even after calling <see cref="
    ///   TopicRelationshipMultiMap.MarkClean(String)"/> if any of the <see cref="Topic"/>s in the <see cref="
    ///   TopicRelationshipMultiMap"/> are marked as <see cref="Topic.IsNew"/>.
    /// </summary>
    [TestMethod]
    public void IsDirty_MarkCleanWithNewTopic_ReturnsTrue() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.SetValue("Related", related, false);

      relationships.MarkClean();
      relationships.MarkClean("Related");

      Assert.IsTrue(relationships.IsDirty());
      Assert.IsTrue(relationships.IsDirty("Related"));

    }

  } //Class
} //Namespace