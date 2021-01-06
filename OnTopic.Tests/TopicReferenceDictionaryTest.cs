/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Collections;
using OnTopic.Tests.Entities;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCE DICTIONARY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicReferenceDictionary"/>, with a particular emphasis on the custom features
  ///   such as <see cref="TopicReferenceDictionary.IsDirty"/>, <see cref="TopicReferenceDictionary.GetTopic(String, Boolean)"
  ///   />, <see cref="TopicReferenceDictionary.SetTopic(String, Topic?, Boolean?)"/>, and the cross-referencing of reciprocal
  ///   values in the <see cref="Topic.IncomingRelationships"/> property.
  /// </summary>
  [TestClass]
  public class TopicReferenceDictionaryTest {

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TopicReferenceDictionary.IsDirty"/> is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);

      Assert.AreEqual<int>(1, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: NEW REFERENCE: NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.SetTopic(String, Topic?, Boolean?)"/>, and confirms that <see cref="
    ///   TopicReferenceDictionary.IsDirty"/> is not set.
    /// </summary>
    [TestMethod]
    public void SetTopic_NewReference_NotDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetTopic("Reference", reference, false);

      Assert.AreEqual<int>(1, topic.References.Count);
      Assert.IsFalse(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: EXISTING REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/> with a topic reference, removes that reference using <see cref=
    ///   "TopicReferenceDictionary.Remove(String)"/> , and confirms that <see cref="TopicReferenceDictionary.IsDirty"/> is set.
    /// </summary>
    [TestMethod]
    public void Remove_ExistingReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetTopic("Reference", reference, false);
      topic.References.Remove("Reference");

      Assert.AreEqual<int>(0, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING REFERENCES: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.SetTopic(String, Topic?, Boolean?)"/>, calls <see cref="TopicReferenceDictionary
    ///   .Clear()"/> and confirms that <see cref="TopicReferenceDictionary.IsDirty"/> is set.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingReferences_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetTopic("Reference", reference, false);
      topic.References.Clear();

      Assert.AreEqual<int>(0, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: INCOMING RELATIONSHIP SET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.Add(String, Topic)"/>, and confirms that <see cref="Topic.IncomingRelationships"/>
    ///   reference is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_IncomingRelationshipSet() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);

      Assert.AreEqual<int>(1, reference.IncomingRelationships.GetTopics("Reference").Count);

    }

    /*==========================================================================================================================
    | TEST: REMOVE: EXISTING REFERENCE: INCOMING RELATIONSHIP REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.Add(String, Topic)"/>, removes the reference using <see cref="TopicReferenceDictionary
    ///   .Remove(String)"/>, and confirms that the <see cref="Topic.IncomingRelationships"/> reference is correctly removed as
    ///   well.
    /// </summary>
    [TestMethod]
    public void Remove_ExistingReference_IncomingRelationshipRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);
      topic.References.Remove("Reference");

      Assert.AreEqual<int>(0, reference.IncomingRelationships.GetTopics("Reference").Count);

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: EXISTING REFERENCE: TOPIC UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.Add(String, Topic)"/>, updates the reference using <see cref="TopicReferenceDictionary
    ///   .SetTopic(String, Topic?, Boolean?)"/>, and confirms that the <see cref="Topic"/> reference is correctly updated.
    /// </summary>
    [TestMethod]
    public void SetTopic_ExistingReference_TopicUpdated() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");
      var newReference          = TopicFactory.Create("NewReference", "Page");

      topic.References.Add("Reference", reference);
      topic.References.SetTopic("Reference", newReference);

      Assert.AreEqual(newReference, topic.References.GetTopic("Reference"));

    }

    /*==========================================================================================================================
    | TEST: SET TOPIC: NULL REFERENCE: TOPIC REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference using <see
    ///   cref="TopicReferenceDictionary.Add(String, Topic)"/>, updates the reference with a null value using <see cref=
    ///   "TopicReferenceDictionary.SetTopic(String, Topic?, Boolean?)"/>, and confirms that the <see cref="Topic"/> reference
    ///   is correctly removed.
    /// </summary>
    [TestMethod]
    public void SetTopic_NullReference_TopicRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);
      topic.References.SetTopic("Reference", null);

      Assert.AreEqual<int>(0, topic.References.Count);

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: TOPIC IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="Topic.IsDirty(Boolean, Boolean)"/> is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_TopicIsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page", 2);

      topic.References.Add("Reference", reference);

      Assert.IsTrue(topic.IsDirty(true));
      Assert.IsFalse(reference.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: EXISTING REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TopicReferenceDictionary.GetTopic(String, Boolean)"/> correctly returns the <see cref="Topic"/>.
    /// </summary>
    [TestMethod]
    public void GetTopic_ExistingReference_ReturnsTopic() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);

      Assert.AreEqual(reference, topic.References.GetTopic("Reference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: MISSING REFERENCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TopicReferenceDictionary.GetTopic(String, Boolean)"/> correctly returns <c>null</c> if an incorrect
    ///   <c>referencedKey</c> is entered.
    /// </summary>
    [TestMethod]
    public void GetTopic_MissingReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("Reference", reference);

      Assert.IsNull(topic.References.GetTopic("MissingReference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: DERIVED REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/> with a <see cref="Topic.DerivedTopic"/>, adds a new <see
    ///   cref="Topic"/> reference to the <see cref="Topic.DerivedTopic"/>, and confirms that <see cref=
    ///   "TopicReferenceDictionary.GetTopic(String, Boolean)"/> correctly returns the related topic reference.
    /// </summary>
    [TestMethod]
    public void GetTopic_DerivedReference_ReturnsTopic() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var derived               = TopicFactory.Create("Derived", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.DerivedTopic        = derived;
      derived.References.Add("Reference", reference);

      Assert.AreEqual(reference, topic.References.GetTopic("Reference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: DERIVED REFERENCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/> with a <see cref="Topic.DerivedTopic"/>, adds a new <see
    ///   cref="Topic"/> reference to the <see cref="Topic.DerivedTopic"/>, and confirms that <see cref=
    ///   "TopicReferenceDictionary.GetTopic(String, Boolean)"/> correctly returns <c>null</c> if an incorrect <c>referencedKey
    ///   </c> is entered.
    /// </summary>
    [TestMethod]
    public void GetTopic_DerivedReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var derived               = TopicFactory.Create("Derived", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.DerivedTopic        = derived;
      derived.References.Add("Reference", reference);

      Assert.IsNull(topic.References.GetTopic("MissingReference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: DERIVED REFERENCE WITHOUT INHERIT: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceDictionary"/> with a <see cref="Topic.DerivedTopic"/>, adds a new <see
    ///   cref="Topic"/> reference to the <see cref="Topic.DerivedTopic"/>, and confirms that <see cref=
    ///   "TopicReferenceDictionary.GetTopic(String, Boolean)"/> correctly returns <c>null</c> if <c>inheritFromDerived</c> is
    ///   set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void GetTopic_DerivedReferenceWithoutInherit_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var derived               = TopicFactory.Create("Derived", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.DerivedTopic        = derived;
      derived.References.Add("Reference", reference);

      Assert.IsNull(topic.References.GetTopic("Reference", false));

    }

    /*==========================================================================================================================
    | TEST: ADD: TOPIC REFERENCE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void Add_TopicReferenceWithBusinessLogic_IsReturned() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add("TopicReference", reference);

      Assert.AreEqual<Topic>(reference, topic.TopicReference);

    }

    /*==========================================================================================================================
    | TEST: ADD: TOPIC REFERENCE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(ArgumentOutOfRangeException),
      "The topic allowed a key to be set via a back door, without routing it through the NumericValue property."
    )]
    public void Add_TopicReferenceWithBusinessLogic_ThrowsException() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Container");

      topic.References.Add("TopicReference", reference);

    }

    /*==========================================================================================================================
    | TEST: SET: TOPIC REFERENCE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(ArgumentOutOfRangeException),
      "The topic allowed a key to be set via a back door, without routing it through the NumericValue property."
    )]
    public void Set_TopicReferenceWithBusinessLogic_ThrowsException() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Container");

      topic.References["TopicReference"] = reference;

    }

  } //Class
} //Namespace