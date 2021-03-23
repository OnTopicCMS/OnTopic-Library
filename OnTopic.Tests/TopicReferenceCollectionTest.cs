/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Associations;
using OnTopic.Tests.Entities;
using OnTopic.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Xunit;

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
  [ExcludeFromCodeCoverage]
  public class TopicReferenceCollectionTest {

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is correctly set.
    /// </summary>
    [Fact]
    public void Add_NewReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.Equal<int>(1, topic.References.Count);
      Assert.True(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NEW REFERENCE: NOT DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is not set.
    /// </summary>
    [Fact]
    public void SetValue_NewReference_NotDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page", 2);

      topic.References.SetValue("Reference", reference, false);

      Assert.Equal<int>(1, topic.References.Count);
      Assert.False(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: REMOVE: EXISTING REFERENCE: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a topic reference, removes that reference using <see cref=
    ///   "TrackedRecordCollection{TItem, TValue, TAttribute}.RemoveItem(Int32)"/>, and confirms that <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is set.
    /// </summary>
    [Fact]
    public void Remove_ExistingReference_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference, false);
      topic.References.Remove("Reference");

      Assert.Equal<int>(0, topic.References.Count);
      Assert.True(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: CLEAR: EXISTING REFERENCES: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, calls <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.ClearItems()"/> and confirms that <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.IsDirty()"/> is set. Also confirms that items are correctly removed
    ///   from recipricol <see cref="Topic.IncomingRelationships"/>.
    /// </summary>
    [Fact]
    public void Clear_ExistingReferences_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add(new("Reference1", reference, false));
      topic.References.Add(new("Reference2", null, false));
      topic.References.Clear();

      Assert.Equal<int>(0, topic.References.Count);
      Assert.Equal<int?>(0, reference.IncomingRelationships.GetValues("Reference")?.Count);
      Assert.True(topic.References.IsDirty());

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
    [Fact]
    public void Add_NewTopic_IsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference, false);

      Assert.True(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: INCOMING RELATIONSHIP SET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, and confirms that
    ///   <see cref="Topic.IncomingRelationships"/> reference is correctly set.
    /// </summary>
    [Fact]
    public void Add_NewReference_IncomingRelationshipSet() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.Equal<int>(1, reference.IncomingRelationships.GetValues("Reference").Count);

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
    /// <remarks>
    ///   This calls <see cref="KeyedCollection{TKey, TItem}.Remove(TKey)"/> twice. The first to confirm that the <see cref="
    ///   Topic.IncomingRelationships"/> is removed, the second to ensure that the attempt to call <see cref="Topic.
    ///   IncomingRelationships"/> isn't disrupted by the fact that the <see cref="TrackedRecord{T}.Value"/> is <c>null</c>.
    /// </remarks>
    [Fact]
    public void Remove_ExistingReference_IncomingRelationshipRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.Add(new("Reference1", reference, false));
      topic.References.Add(new("Reference2", null, false));

      topic.References.Remove("Reference1");
      topic.References.Remove("Reference2");

      Assert.Equal<int>(0, reference.IncomingRelationships.GetValues("Reference").Count);

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
    [Fact]
    public void SetValue_ExistingReference_TopicUpdated() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");
      var newReference          = TopicFactory.Create("NewReference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.SetValue("Reference", newReference);

      Assert.Equal<Topic?>(newReference, topic.References.GetValue("Reference"));
      Assert.Equal<int?>(0, reference.IncomingRelationships.GetValues("Reference")?.Count);
      Assert.Equal<int?>(1, newReference.IncomingRelationships.GetValues("Reference")?.Count);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NULL REFERENCE: TOPIC UPDATED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)"/>, updates the
    ///   reference using <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?,
    ///   DateTime?)"/> with a <c>null</c> value, and confirms that the <see cref="Topic"/> reference and <see cref="Topic.
    ///   IncomingRelationships"/> are correctly updated.
    /// </summary>
    /// <remarks>
    ///   This calls <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.SetValue(String, TValue, Boolean?, DateTime?)
    ///   "/> twice. The first to confirm that the <see cref="Topic.IncomingRelationships"/> is set, the second to ensure that
    ///   the attempt to call <see cref="Topic.IncomingRelationships"/> isn't disrupted by the fact that the <see cref="
    ///   TrackedRecord{T}.Value"/> will now be <c>null</c>.
    /// </remarks>
    [Fact]
    public void SetValue_ExistingReference_IncomingRelationshipsUpdates() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.SetValue("Reference", null);
      topic.References.SetValue("Reference", null);

      Assert.True(topic.References.Contains("Reference"));
      Assert.Null(topic.References.GetValue("Reference"));
      Assert.Equal<int?>(0, reference.IncomingRelationships.GetValues("Reference")?.Count);

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
    [Fact]
    public void SetValue_NullReference_TopicRemoved() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);
      topic.References.SetValue("Reference", null);

      Assert.Equal<int>(1, topic.References.Count);
      Assert.Null(topic.References.GetValue("Reference"));

    }

    /*==========================================================================================================================
    | TEST: ADD: NEW REFERENCE: TOPIC IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="Topic.IsDirty(Boolean, Boolean)"/> is correctly set.
    /// </summary>
    [Fact]
    public void Add_NewReference_TopicIsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var reference             = TopicFactory.Create("Reference", "Page", 2);

      topic.References.SetValue("Reference", reference);

      Assert.True(topic.IsDirty(true));
      Assert.False(reference.IsDirty(true));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: EXISTING REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns the <see
    ///   cref="Topic"/>.
    /// </summary>
    [Fact]
    public void GetTopic_ExistingReference_ReturnsTopic() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.Equal<Topic?>(reference, topic.References.GetValue("Reference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: MISSING REFERENCE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/>, adds a new <see cref="Topic"/> reference, and confirms that
    ///   <see cref="TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns <c>null
    ///   </c> if an incorrect <c>referencedKey</c> is entered.
    /// </summary>
    [Fact]
    public void GetTopic_MissingReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("Reference", reference);

      Assert.Null(topic.References.GetValue("MissingReference"));

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC: INHERITED REFERENCE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicReferenceCollection"/> with a <see cref="Topic.BaseTopic"/>, adds a new <see cref="
    ///   Topic"/> reference to the <see cref="Topic.BaseTopic"/>, and confirms that <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.GetValue(String, Boolean)"/> correctly returns the related topic reference, inheriting from both
    ///   <see cref="Topic.Parent"/> and <see cref="Topic.BaseTopic"/>.
    /// </summary>
    [Fact]
    public void GetTopic_InheritedReference_ReturnsTopic() {

      var parentTopic           = TopicFactory.Create("Parent", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", parentTopic);
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      parentTopic.BaseTopic     = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.Equal<Topic?>(reference, topic.References.GetValue("Reference", true));

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
    [Fact]
    public void GetTopic_InheritedReference_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.Null(topic.References.GetValue("MissingReference", true));

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
    [Fact]
    public void GetTopic_InheritedReferenceWithoutInheritance_ReturnsNull() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("Base", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.References.SetValue("Reference", reference);

      Assert.Null(topic.References.GetValue("Reference", null, false, false));

    }

    /*==========================================================================================================================
    | TEST: ADD: TOPIC REFERENCE WITH BUSINESS LOGIC: IS RETURNED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance; ensures it is routed through the corresponding property and correctly
    ///   retrieved.
    /// </summary>
    [Fact]
    public void Add_TopicReferenceWithBusinessLogic_IsReturned() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("TopicReference", reference);

      Assert.Equal<Topic?>(reference, topic.TopicReference);

    }

    /*==========================================================================================================================
    | TEST: ADD: TOPIC REFERENCE WITH BUSINESS LOGIC: REMOVES REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance, then sets it to null, ensuring that it is correctly removed.
    /// </summary>
    [Fact]
    public void Add_TopicReferenceWithBusinessLogic_RemovedReference() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Page");

      topic.References.SetValue("BaseTopic", reference);
      topic.References.SetValue("BaseTopic", null);

      Assert.Null(topic.TopicReference);

    }

    /*==========================================================================================================================
    | TEST: ADD: TOPIC REFERENCE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [Fact]
    public void Add_TopicReferenceWithBusinessLogic_ThrowsException() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Container");

      Assert.Throws<ArgumentOutOfRangeException>(() =>
        topic.References.SetValue("TopicReference", reference)
      );

    }

    /*==========================================================================================================================
    | TEST: SET: TOPIC REFERENCE WITH BUSINESS LOGIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a topic reference on a topic instance with an invalid value; ensures an exception is thrown.
    /// </summary>
    [Fact]
    public void Set_TopicReferenceWithBusinessLogic_ThrowsException() {

      var topic                 = new CustomTopic("Test", "Page");
      var reference             = TopicFactory.Create("Reference", "Container");

      Assert.Throws<ArgumentOutOfRangeException>(() =>
        topic.References.Add(new("TopicReference", reference))
      );

    }

  } //Class
} //Namespace