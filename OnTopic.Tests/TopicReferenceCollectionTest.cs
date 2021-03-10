/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Associations;
using OnTopic.Tests.Entities;
using OnTopic.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCE COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicReferenceCollection"/>, with a particular emphasis on the custom features
  ///   such as <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/>, <see cref="TrackedRecordCollection{
  ///   TItem, TValue, TAttribute}.GetValue(String, Boolean)"/>, <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.
  ///   SetValue(String, TValue, Boolean?, DateTime?)"/>, and the cross-referencing of reciprocal values in the <see cref="
  ///   Topic.IncomingRelationships"/> property.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class TopicReferenceCollectionTest {

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.AreEqual<int>(1, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NEW REFERENCE: NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is not set.
    /// </summary>
    [TestMethod]
    public void SetValue_NewReference_NotDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page", 2);

      topic.References.SetValue("Reference", reference, false);

      Assert.AreEqual<int>(1, topic.References.Count);
      Assert.IsFalse(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: EXISTING REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a topic reference, removes that reference using <see cref=
    ///   "TrackedRecordCollection{TItem, TValue, TAttribute}.RemoveItem(Int32)"/>, and confirms that <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is set.
    /// </summary>
    [TestMethod]
    public void Remove_ExistingReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference, false);
      topic.References.Remove("Reference");

      Assert.AreEqual<int>(0, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING REFERENCES: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, calls <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.ClearItems()"/> and confirms that <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is set.
    /// </summary>
    [TestMethod]
    public void Clear_ExistingReferences_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference, false);
      topic.References.Clear();

      Assert.AreEqual<int>(0, topic.References.Count);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW TOPIC: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> and adds a new <see cref="Topic"/> reference using <see cref="
    ///   KeyedCollection{TKey, TItem}.InsertItem(Int32, TItem)"/> with <see cref="TrackedRecord{T}.IsDirty"/> set to <c>false
    ///   </c>, confirming that <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> remains <c>true</c>
    ///   since the target <see cref="Topic"/> is unsaved.
    /// </summary>
    [TestMethod]
    public void Add_NewTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference, false);

      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: INCOMING RELATIONSHIP SET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, and confirms that
    ///   <see cref="Topic.IncomingRelationships"/> reference is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_IncomingRelationshipSet() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.AreEqual<int>(1, reference.IncomingRelationships.GetValues("Reference").Count);

    }

    /*==========================================================================================================================
    | TEST: REMOVE: EXISTING REFERENCE: INCOMING RELATIONSHIP REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, removes the
    ///   reference using <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.RemoveItem(Int32)"/>, and confirms that
    ///   the <see cref="Topic.IncomingRelationships"/> reference is correctly removed as well.
    /// </summary>
    [TestMethod]
    public void Remove_ExistingReference_IncomingRelationshipRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.Remove("Reference");

      Assert.AreEqual<int>(0, reference.IncomingRelationships.GetValues("Reference").Count);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: EXISTING REFERENCE: TOPIC UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, updates the
    ///   reference using <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?,
    ///   DateTime?)"/>, and confirms that the <see cref="Topic"/> reference and <see cref="Topic.IncomingRelationships"/> are
    ///   correctly updated.
    /// </summary>
    [TestMethod]
    public void SetValue_ExistingReference_TopicUpdated() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");
      var newReference          = TopicFactory.Create("NewReference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.SetValue("Reference", newReference);

      Assert.AreEqual(newReference, topic.References.GetValue("Reference"));
      Assert.AreEqual<int?>(0, reference.IncomingRelationships.GetValues("Reference")?.Count);
      Assert.AreEqual<int?>(1, newReference.IncomingRelationships.GetValues("Reference")?.Count);

    }

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NULL REFERENCE: TOPIC REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, updates the
    ///   reference with a null value using <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String,
    ///   TValue, Boolean?, DateTime?)"/>, and confirms that the <see cref="Topic"/> reference is correctly removed.
    /// </summary>
    [TestMethod]
    public void SetValue_NullReference_TopicRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.SetValue("Reference", null);

      Assert.AreEqual<int>(1, topic.References.Count);
      Assert.IsNull(topic.References.GetValue("Reference"));

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: TOPIC IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="Topic.IsDirty(Boolean, Boolean)"/> is correctly set.
    /// </summary>
    [TestMethod]
    public void Add_NewReference_TopicIsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page", 2);

      topic.References.SetValue("Reference", reference);

      Assert.IsTrue(topic.IsDirty(true));
      Assert.IsFalse(reference.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: EXISTING REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns the <see
    ///   cref="Topic"/>.
    /// </summary>
    [TestMethod]
    public void GetTopic_ExistingReference_ReturnsTopic() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.AreEqual(reference, topic.References.GetValue("Reference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: MISSING REFERENCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns <c>null
    ///   </c> if an incorrect <c>referencedKey</c> is entered.
    /// </summary>
    [TestMethod]
    public void GetTopic_MissingReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.IsNull(topic.References.GetValue("MissingReference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: INHERITED REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a <see cref="Topic.BaseTopic"/>, adds a new <see cref="
    ///   Topic"/> reference to the <see cref="Topic.BaseTopic"/>, and confirms that <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns the related topic reference.
    /// </summary>
    [TestMethod]
    public void GetTopic_InheritedReference_ReturnsTopic() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.AreEqual(reference, topic.References.GetValue("Reference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: INHERITED REFERENCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a <see cref="Topic.BaseTopic"/>, adds a new <see cref="
    ///   Topic"/> reference to the <see cref="Topic.BaseTopic"/>, and confirms that <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns <c>null</c> if an incorrect <c>referencedKey</c> is
    ///   entered.
    /// </summary>
    [TestMethod]
    public void GetTopic_InheritedReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.IsNull(topic.References.GetValue("MissingReference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: INHERITED REFERENCE WITHOUT INHERITANCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a <see cref="Topic.BaseTopic"/>, adds a new <see cref="
    ///   Topic"/> reference to the <see cref="Topic.BaseTopic"/>, and confirms that <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns <c>null</c> if <c>inheritFromBase</c> is set to
    ///   <c>false</c>.
    /// </summary>
    [TestMethod]
    public void GetTopic_InheritedReferenceWithoutInheritance_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.IsNull(topic.References.GetValue("Reference", null, false, false));

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

      topic.References.SetValue("TopicReference", reference);

      Assert.AreEqual<Topic?>(reference, topic.TopicReference);

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

      topic.References.SetValue("TopicReference", reference);

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

      topic.References.Add(new("TopicReference", reference));

    }

  } //Class
} //Namespace