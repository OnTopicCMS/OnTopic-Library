/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
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
  [ExcludeFromCodeCoverage]
  public class TopicRelationshipMultiMapTest {

    /*==========================================================================================================================
    | TEST: SET VALUE: CREATES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible.
    /// </summary>
    [Fact]
    public void SetValue_CreatesRelationship() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");

      parent.Relationships.SetValue("Friends", related);

      Assert.Equal(parent.Relationships.GetValues("Friends").First(), related);

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INVALID RELATIONSHIP KEY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a <see cref="Topic"/> from a <see cref="Topic.Relationships"/> where the <see cref="KeyValuesPair{
    ///   TKey, TValue}.Key"/> is invalid. In this case, <see cref="TopicRelationshipMultiMap.Remove(String, Topic)"/> should
    ///   return <c>false</c>.
    /// </summary>
    [Fact]
    public void Remove_InvalidRelationshipKey_ReturnsFalse() {

      var parent                = new Topic("Parent", "Page");
      var unrelated             = new Topic("Unrelated", "Page");

      Assert.False(parent.Relationships.Remove("Unrelated", unrelated));

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INVALID TOPIC KEY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a <see cref="Topic"/> from a <see cref="TopicMultiMap"/> where the <see cref="Topic"/> is does not
    ///   exist. In this case, <see cref="TopicMultiMap.Remove(String, Topic)"/> should return <c>false</c>.
    /// </summary>
    [Fact]
    public void Remove_InvalidTopicKey_ReturnsFalse() {

      var multiMap              = new TopicMultiMap();
      var unrelated             = new Topic("Unrelated", "Page");

      multiMap.Add("Related", new("Test", "Page"));

      Assert.False(multiMap.Remove("Related", unrelated));

    }

    /*==========================================================================================================================
    | TEST: REMOVE: REMOVES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed.
    /// </summary>
    [Fact]
    public void Remove_RemovesRelationship() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");

      parent.Relationships.SetValue("Friends", related);
      parent.Relationships.Remove("Friends", related);

      Assert.Null(parent.Relationships.GetValues("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: REMOVES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and then removes it by key, and confirms that it is removed from the incoming relationships
    ///   property.
    /// </summary>
    [Fact]
    public void Remove_RemovesIncomingRelationship() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetValue("Friends", related);
      relationships.Remove("Friends", related);

      Assert.Null(related.IncomingRelationships.GetValues("Friends").FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: CREATES INCOMING RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a relationship and confirms that it is accessible on incoming relationships property.
    /// </summary>
    [Fact]
    public void SetValue_CreatesIncomingRelationship() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      relationships.SetValue("Friends", related);

      Assert.Equal(related.IncomingRelationships.GetValues("Friends").First(), parent);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: INCOMING RELATIONSHIPS: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to set a relationship on a <see cref="TopicRelationshipMultiMap"/> that is marked as <c>isIncoming</c>
    ///   without setting the <c>isIncoming</c> parameter on <see cref="TopicRelationshipMultiMap.SetValue(String, Topic,
    ///   Boolean?, Boolean)"/> and verifies that a <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [Fact]
    public void SetValue_IncomingRelationships_ThrowsException() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent, true);

      Assert.Throws<InvalidOperationException>(() =>
        relationships.SetValue("Friends", related)
      );

    }

    /*==========================================================================================================================
    | TEST: REMOVE: INCOMING RELATIONSHIPS: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to remove a relationship from a <see cref="TopicRelationshipMultiMap"/> that is marked as <c>isIncoming</c>
    ///   without setting the <c>isIncoming</c> parameter on <see cref="TopicRelationshipMultiMap.Remove(String, Topic, Boolean)
    ///   "/> and verifies that a <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    [Fact]
    public void Remove_IncomingRelationships_ThrowsException() {

      var parent                = new Topic("Parent", "Page");
      var related               = new Topic("Related", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent, true);

      Assert.Throws<InvalidOperationException>(() =>
        relationships.Remove("Friends", related)
      );

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: UPDATES KEY COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and the correct number of keys are returned.
    /// </summary>
    [Fact]
    public void SetValue_UpdatesKeyCount() {

      var parent                = new Topic("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetValue("Relationship" + i, new Topic("Related" + i, "Page"));
      }

      Assert.Equal(5, relationships.Keys.Count);
      Assert.Contains("Relationship3", relationships.Keys);

    }

    /*==========================================================================================================================
    | TEST: GET ENUMERATOR: RETURNS KEY/VALUES PAIRS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Enumerates over the <see cref="ReadOnlyTopicMultiMap"/>, ensuring that the enumerator defined by the <see cref="
    ///   ReadOnlyTopicMultiMap.GetEnumerator()"/> interface implementation successfully relays the call to the underlying
    ///   <see cref="TopicMultiMap"/>.
    /// </summary>
    [Fact]
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

      Assert.Equal(5, readOnlyRelationships.Count);
      Assert.Equal(5, counter);

    }

    /*==========================================================================================================================
    | TEST: INDEXER: RETURNS KEY/VALUES PAIRS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="ReadOnlyTopicMultiMap"/> and ensures that a <see cref="ReadOnlyTopicCollection"/> can be
    ///   retrieved by <see cref="KeyValuesPair{TKey, TValue}.Key"/> via the <see cref="ReadOnlyTopicMultiMap"/> indexer.
    /// </summary>
    [Fact]
    public void Indexer_ReturnsKeyValuesPair() {

      var multiMap              = new TopicMultiMap();
      var readOnlyTopicMultiMap = new ReadOnlyTopicMultiMap(multiMap);
      var topics                = new TopicCollection();
      var keyValuesPair         = new KeyValuesPair<string, TopicCollection>("Relationship", topics);
      var topic                 = new Topic("Test", "Test");

      topics.Add(topic);
      multiMap.Add(keyValuesPair);

      Assert.Single(readOnlyTopicMultiMap);
      Assert.Equal(topic, readOnlyTopicMultiMap["Relationship"].FirstOrDefault());

    }

    /*==========================================================================================================================
    | TEST: GET ALL VALUES: RETURNS ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, and ensures they are all returned via <see cref="ReadOnlyTopicMultiMap.
    ///   GetAllValues(String)"/>.
    /// </summary>
    [Fact]
    public void GetAllValues_ReturnsAllTopics() {

      var parent                = new Topic("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetValue("Relationship" + i, new Topic("Related" + i, "Page"));
      }

      Assert.Equal(5, relationships.Count);
      Assert.Equal("Related3", relationships.GetValues("Relationship3").First().Key);
      Assert.Equal(5, relationships.GetAllValues().Count);

    }

    /*==========================================================================================================================
    | TEST: GET ALL VALUES: CONTENT TYPES: RETURNS ALL CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets relationships in multiple namespaces, with different ContentTypes, then filters the results of <see cref="
    ///   ReadOnlyTopicMultiMap.GetAllValues(String)"/> by content type.
    /// </summary>
    [Fact]
    public void GetAllValues_ContentTypes_ReturnsAllContentTypes() {

      var parent                = new Topic("Parent", "Page");
      var relationships         = new TopicRelationshipMultiMap(parent);

      for (var i = 0; i < 5; i++) {
        relationships.SetValue("Relationship" + i, new Topic("Related" + i, "ContentType" + i));
      }

      Assert.Equal(5, relationships.Keys.Count);
      Assert.Single(relationships.GetAllValues("ContentType3"));

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that <see cref="TopicRelationshipMultiMap.
    ///   IsDirty()"/> is set.
    /// </summary>
    [Fact]
    public void SetTopic_IsDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      relationships.SetValue("Related", related);

      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: IS DUPLICATE: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [Fact]
    public void SetValue_IsDuplicate_IsNotDirty() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page", null, 2);

      relationships.SetValue("Related", related);
      relationships.MarkClean();

      relationships.SetValue("Related", related);

      Assert.False(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: IS DUPLICATE: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="TopicRelationshipMultiMap"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>false</c>.
    /// </summary>
    [Fact]
    public void SetSetValue_IsDuplicate_StaysDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related1              = new Topic("Topic", "Page");
      var related2              = new Topic("Topic", "Page");

      relationships.SetValue("Related", related1);
      relationships.SetValue("Related", related2);

      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes an existing <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value for
    ///   <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void Remove_IsDirty() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      relationships.SetValue("Related", related);
      relationships.MarkClean();
      relationships.Remove("Related", related);

      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: MISSING TOPIC: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value
    ///   for <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c>.
    /// </summary>
    [Fact]
    public void Remove_MissingTopic_IsNotDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      var isSuccessful          = relationships.Remove("Related", related);

      Assert.False(isSuccessful);
      Assert.False(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: MISSING TOPIC: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="TopicRelationshipMultiMap"/> and conirms that the value
    ///   for <see cref="TopicRelationshipMultiMap.IsDirty()"/> stays <c>true</c>.
    /// </summary>
    [Fact]
    public void Remove_MissingTopic_StaysDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic1", "Page");
      var missing               = new Topic("Topic2", "Page");

      relationships.SetValue("Related", related);

      var isSuccessful          = relationships.Remove("Related", missing);

      Assert.False(isSuccessful);
      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING TOPICS: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> and confirms that value of <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> is <c>true</c>.
    /// </summary>
    [Fact]
    public void Clear_ExistingTopics_IsDirty() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      relationships.SetValue("Related", related);
      relationships.MarkClean();
      relationships.Clear("Related");

      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: NO TOPICS: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="TopicRelationshipMultiMap.Clear(String)"/> with no existing <see cref="Topic"/>s and confirms that the
    ///   value of <see cref="TopicRelationshipMultiMap.IsDirty()"/> is set to <c>false</c>.
    /// </summary>
    [Fact]
    public void Clear_NoTopics_IsNotDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);

      relationships.Clear("Related");

      Assert.False(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: MARK NOT DIRTY: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an existing <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> and confirms that <see cref="
    ///   TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c> if <see cref="TopicRelationshipMultiMap.SetValue(String,
    ///   Topic, Boolean?, Boolean)"/> is called with the <c>markDirty</c> parameter set to <c>false</c>.
    /// </summary>
    [Fact]
    public void SetValue_MarkNotDirty_IsNotDirty() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page", null, 2);

      relationships.SetValue("Related", related, false);

      Assert.False(relationships.IsDirty());

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
    [Fact]
    public void SetValue_NewParent_IsDirty() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page", null, 1);

      relationships.SetValue("Related", related, false);

      Assert.True(relationships.IsDirty());

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
    [Fact]
    public void SetValue_NewTopic_IsDirty() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      relationships.SetValue("Related", related, false);

      Assert.True(relationships.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic"/>.
    ///   Confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>false</c> after calling <see cref="
    ///   TopicRelationshipMultiMap.MarkClean(String)"/>.
    /// </summary>
    [Fact]
    public void IsDirty_MarkClean_ReturnsFalse() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page", null, 2);

      relationships.SetValue("Related", related);

      relationships.MarkClean("Related");

      Assert.False(relationships.IsDirty());
      Assert.False(relationships.IsDirty("Related"));

    }

    /*==========================================================================================================================
    | TEST: IS DIRTY: MARK CLEAN: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an <see cref="Topic"/> to a <see cref="TopicRelationshipMultiMap"/> associated with a <see cref="Topic"/> that is
    ///   <see cref="Topic.IsNew"/>. Confirms that <see cref="TopicRelationshipMultiMap.IsDirty()"/> returns <c>true</c> even
    ///   after calling <see cref="TopicRelationshipMultiMap.MarkClean(String)"/> since new topics cannot be clean.
    /// </summary>
    [Fact]
    public void IsDirty_MarkClean_ReturnsTrue() {

      var topic                 = new Topic("Test", "Page");
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page", null, 2);

      relationships.SetValue("Related", related, false);

      relationships.MarkClean();
      relationships.MarkClean("Related");

      Assert.True(relationships.IsDirty());
      Assert.True(relationships.IsDirty("Related"));

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
    [Fact]
    public void IsDirty_MarkCleanWithNewTopic_ReturnsTrue() {

      var topic                 = new Topic("Test", "Page", null, 1);
      var relationships         = new TopicRelationshipMultiMap(topic);
      var related               = new Topic("Topic", "Page");

      relationships.SetValue("Related", related, false);

      relationships.MarkClean();
      relationships.MarkClean("Related");

      Assert.True(relationships.IsDirty());
      Assert.True(relationships.IsDirty("Related"));

    }

  } //Class
} //Namespace