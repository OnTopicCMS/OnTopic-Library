/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: NAMED TOPIC COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="NamedTopicCollection"/> class.
  /// </summary>
  [TestClass]
  public class NamedTopicCollectionTest {

    /*==========================================================================================================================
    | TEST: ADD TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a topic to a <see cref="NamedTopicCollection"/> and confirms that <see cref="NamedTopicCollection.IsDirty"/> is
    ///   set.
    /// </summary>
    [TestMethod]
    public void AddTopic_IsDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.Add(related);

      Assert.IsTrue(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: ADD TOPIC: IS DUPLICATE: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="NamedTopicCollection"/> and confirms that value of <see
    ///   cref="NamedTopicCollection.IsDirty"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void AddTopic_IsDuplicate_IsNotDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related1              = TopicFactory.Create("Topic", "Page");
      var related2              = TopicFactory.Create("Topic", "Page");

      relationships.Add(related1);
      relationships.IsDirty     = false;

      try {
        relationships.Add(related2);
      }
      catch (ArgumentException) {
        //Expected due to duplicate key
      }

      Assert.IsFalse(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: ADD TOPIC: IS DUPLICATE: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a duplicate topic to a <see cref="NamedTopicCollection"/> and confirms that value of <see
    ///   cref="NamedTopicCollection.IsDirty"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void AddTopic_IsDuplicate_StaysDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related1              = TopicFactory.Create("Topic", "Page");
      var related2              = TopicFactory.Create("Topic", "Page");

      relationships.Add(related1);

      try {
        relationships.Add(related2);
      }
      catch (ArgumentException) {
        //Expected due to duplicate key
      }

      Assert.IsTrue(relationships.IsDirty);

    }


    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes an existing <see cref="Topic"/> from a <see cref="NamedTopicCollection"/> and conirms that the value for <see
    ///   cref="NamedTopicCollection.IsDirty"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_IsDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.Add(related);
      relationships.IsDirty     = false;
      relationships.Remove(related);

      Assert.IsTrue(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: MISSING TOPIC: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="NamedTopicCollection"/> and conirms that the value for
    ///   <see cref="NamedTopicCollection.IsDirty"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_MissingTopic_IsNotDirty() {

      var related               = TopicFactory.Create("Topic", "Page");
      var relationships         = new NamedTopicCollection("Test");

      relationships.Remove(related);

      Assert.IsFalse(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: REMOVE TOPIC: MISSING TOPIC: STAYS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a non-existent <see cref="Topic"/> from a <see cref="NamedTopicCollection"/> and conirms that the value for
    ///   <see cref="NamedTopicCollection.IsDirty"/> stays <c>true</c>.
    /// </summary>
    [TestMethod]
    public void RemoveTopic_MissingTopic_StaysDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related               = TopicFactory.Create("Topic1", "Page");
      var missing               = TopicFactory.Create("Topic2", "Page");

      relationships.Add(related);
      relationships.Remove(missing);

      Assert.IsTrue(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING TOPICS: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="NamedTopicCollection.ClearItems"/> and confirms that value of <see
    ///   cref="NamedTopicCollection.IsDirty"/> is <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingTopics_IsDirty() {

      var relationships         = new NamedTopicCollection("Test");
      var related               = TopicFactory.Create("Topic", "Page");

      relationships.Add(related);
      relationships.IsDirty     = false;
      relationships.Clear();

      Assert.IsTrue(relationships.IsDirty);

    }

    /*==========================================================================================================================
    | TEST: CLEAR: NO TOPICS: IS NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Call <see cref="NamedTopicCollection.ClearItems"/> with no existing <see cref="Topic"/>s and confirms that value of
    ///   <see cref="NamedTopicCollection.IsDirty"/> is <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Clear_NoTopics_IsNotDirty() {

      var relationships         = new NamedTopicCollection("Test");

      relationships.Clear();

      Assert.IsFalse(relationships.IsDirty);

    }

  } //Class
} //Namespace