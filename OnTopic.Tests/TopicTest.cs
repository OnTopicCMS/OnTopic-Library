/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Metadata;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Topic"/> class.
  /// </summary>
  [TestClass]
  public class TopicTest {

    /*==========================================================================================================================
    | TEST: CREATE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures it's correctly returned.
    /// </summary>
    [TestMethod]
    public void Create_ReturnsTopic() {
      var topic = TopicFactory.Create("Test", "Page");
      Assert.IsNotNull(topic);
      Assert.AreEqual<string>(topic.Key, "Test");
      Assert.AreEqual<string>(topic.ContentType, "Page");
    }

    /*==========================================================================================================================
    | TEST: CREATE: CONTENT TYPE: RETURNS DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic of a content type which maps to a class derived from <see cref="Topic"/>, and ensures the derived
    ///   version of the <see cref="Topic"/> class is returned.
    /// </summary>
    [TestMethod]
    public void Create_ContentType_ReturnsDerivedTopic() {
      var topic = TopicFactory.Create("Test", "ContentTypeDescriptor");
      Assert.IsNotNull(topic);
      Assert.IsInstanceOfType(topic, typeof(ContentTypeDescriptor));
    }

    /*==========================================================================================================================
    | TEST: ID: CHANGE VALUE: THROWS ARGUMENT EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures that the ID cannot be modified.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), "Topic permitted the ID to be reset; this should never happen.")]
    public void Id_ChangeValue_ThrowsArgumentException() {

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor", 123);
      topic.Id                  = 124;

    }

    /*==========================================================================================================================
    | TEST: IS TYPE OF: DERIVED CONTENT TYPE: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several content types, and confirms that the topic is reported as a type of those content
    ///   types.
    /// </summary>
    [TestMethod]
    public void IsTypeOf_DerivedContentType_ReturnsTrue() {

      var contentType = (ContentTypeDescriptor)TopicFactory.Create("Root", "ContentTypeDescriptor");
      for (var i = 0; i < 5; i++) {
        var childContentType = (ContentTypeDescriptor)TopicFactory.Create("ContentType" + i, "ContentTypeDescriptor", contentType);
        contentType             = childContentType;
      }

      Assert.IsTrue(contentType.IsTypeOf("Root"));

    }

    /*==========================================================================================================================
    | TEST: IS TYPE OF: INVALID CONTENT TYPE: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several content types, and confirms that the topic is not reported as a type of a content
    ///   type that is not in that chain.
    /// </summary>
    [TestMethod]
    public void IsTypeOf_InvalidContentType_ReturnsFalse() {

      var contentType = (ContentTypeDescriptor)TopicFactory.Create("Root", "ContentTypeDescriptor");
      for (var i = 0; i < 5; i++) {
        var childContentType = (ContentTypeDescriptor)TopicFactory.Create("ContentType" + i, "ContentTypeDescriptor", contentType);
        contentType             = childContentType;
      }

      Assert.IsTrue(contentType.IsTypeOf("DifferentRoot"));

    }

    /*==========================================================================================================================
    | TEST: PARENT: SET VALUE: UPDATES PARENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [TestMethod]
    public void Parent_SetValue_UpdatesParentTopic() {

      var parentTopic           = TopicFactory.Create("Parent", "ContentTypeDescriptor");
      var childTopic            = TopicFactory.Create("Child", "ContentTypeDescriptor");

      parentTopic.Id            = 5;
      childTopic.Parent         = parentTopic;

      Assert.ReferenceEquals(parentTopic.Children["Child"], childTopic);
      Assert.AreEqual<int>(5, childTopic.Parent.Id);

    }

    /*==========================================================================================================================
    | TEST: PARENT: CHANGE VALUE: UPDATES PARENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [TestMethod]
    public void Parent_ChangeValue_UpdatesParentTopic() {

      var sourceParent          = TopicFactory.Create("SourceParent", "ContentTypeDescriptor");
      var targetParent          = TopicFactory.Create("TargetParent", "ContentTypeDescriptor");
      var childTopic            = TopicFactory.Create("ChildTopic", "ContentTypeDescriptor");

      sourceParent.Id           = 5;
      targetParent.Id           = 10;
      childTopic.Parent         = sourceParent;
      childTopic.Parent         = targetParent;

      Assert.ReferenceEquals(targetParent.Children["ChildTopic"], childTopic);
      Assert.IsFalse(sourceParent.Children.Contains("ChildTopic"));
      Assert.AreEqual<int>(10, childTopic.Parent.Id);

    }

    /*==========================================================================================================================
    | TEST: UNIQUE KEY: RETURNS UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures the Unique Key is correct for a deeply nested child.
    /// </summary>
    [TestMethod]
    public void UniqueKey_ReturnsUniqueKey() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page");
      var childTopic            = TopicFactory.Create("ChildTopic", "Page");
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page");

      childTopic.Parent         = parentTopic;
      grandChildTopic.Parent    = childTopic;

      Assert.AreEqual<string>("ParentTopic:ChildTopic:GrandChildTopic", grandChildTopic.GetUniqueKey());
      Assert.AreEqual<string>("/ParentTopic/ChildTopic/GrandChildTopic/", grandChildTopic.GetWebPath());

    }

    /*==========================================================================================================================
    | TEST: IS VISIBLE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="Topic.IsVisible(Boolean)"/> returns expected values based on <see cref="Topic.IsHidden"/> and
    ///   <see cref="Topic.IsDisabled"/>.
    /// </summary>
    [TestMethod]
    public void IsVisible_ReturnsExpectedValue() {

      var hiddenTopic           = TopicFactory.Create("HiddenTopic", "Page");
      var disabledTopic         = TopicFactory.Create("DisabledTopic", "Page");
      var visibleTopic          = TopicFactory.Create("VisibleTopic", "Page");

      hiddenTopic.IsHidden      = true;
      disabledTopic.IsDisabled  = true;

      Assert.IsFalse(hiddenTopic.IsVisible());
      Assert.IsFalse(hiddenTopic.IsVisible(true));
      Assert.IsFalse(disabledTopic.IsVisible());
      Assert.IsTrue(disabledTopic.IsVisible(true));
      Assert.IsTrue(visibleTopic.IsVisible());
      Assert.IsTrue(visibleTopic.IsVisible(true));

    }

    /*==========================================================================================================================
    | TEST: TITLE: NULL VALUE: RETURNS KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that the title falls back appropriately.
    /// </summary>
    [TestMethod]
    public void Title_NullValue_ReturnsKey() {

      var untitledTopic         = TopicFactory.Create("UntitledTopic", "Page");
      var titledTopic           = TopicFactory.Create("TitledTopic", "Page");

      titledTopic.Title         = "Titled Topic";

      Assert.AreEqual<string>(untitledTopic.Title, "UntitledTopic");
      Assert.AreEqual<string>(titledTopic.Title, "Titled Topic");

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="Topic.LastModified"/>, and ensures it's returned correctly.
    /// </summary>
    [TestMethod]
    public void LastModified_UpdateLastModified_ReturnsExpectedValue() {

      var topic                 = TopicFactory.Create("Topic1", "Page");
      var lastModified          = new DateTime(1976, 10, 15);

      topic.LastModified        = lastModified;

      Assert.AreEqual<DateTime>(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="Topic.VersionHistory"/>, and ensures it's returned correctly.
    /// </summary>
    [TestMethod]
    public void LastModified_UpdateVersionHistory_ReturnsExpectedValue() {

      var topic                 = TopicFactory.Create("Topic2", "Page");

      var lastModified          = new DateTime(1976, 10, 15);

      topic.VersionHistory.Add(lastModified);

      Assert.AreEqual<DateTime>(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE ATTRIBUTE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="AttributeValueCollection"/>, and ensures it's returned correctly.
    /// </summary>
    [TestMethod]
    public void LastModified_UpdateValue_ReturnsExpectedValue() {

      var topic                 = TopicFactory.Create("Topic3", "Page");

      var lastModified          = new DateTime(1976, 10, 15);

      topic.Attributes.SetValue("LastModified", lastModified.ToShortDateString());

      Assert.AreEqual<DateTime>(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: BASE TOPIC: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a base topic to a topic entity, then replaces the references with a new topic entity. Ensures that both the
    ///   base topic as well as the underlying <see cref="AttributeValue"/> correctly reference the new value.
    /// </summary>
    [TestMethod]
    public void BaseTopic_UpdateValue_ReturnsExpectedValue() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var firstBaseTopic        = TopicFactory.Create("BaseTopic", "Page");
      var secondBaseTopic       = TopicFactory.Create("BaseTopic", "Page", 1);
      var finalBaseTopic        = TopicFactory.Create("BaseTopic", "Page", 2);

      topic.BaseTopic           = firstBaseTopic;
      topic.BaseTopic           = secondBaseTopic;
      topic.BaseTopic           = finalBaseTopic;

      Assert.ReferenceEquals(topic.BaseTopic, finalBaseTopic);
      Assert.AreEqual<int>(2, topic.References.GetValue("BaseTopic").Id);

    }

    /*==========================================================================================================================
    | TEST: BASE TOPIC: RESAVED VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a base topic to an unsaved topic entity, then saves the entity and reestablishes the reference. Ensures that the
    ///   base topic is correctly set as a <see cref="Topic.References"/> entry.
    /// </summary>
    [TestMethod]
    public void BaseTopic_ResavedValue_ReturnsExpectedValue() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var baseTopic             = TopicFactory.Create("BaseTopic", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.Id              = 5;
      topic.BaseTopic           = baseTopic;

      Assert.ReferenceEquals(topic.BaseTopic, baseTopic);
      Assert.AreEqual<int>(5, topic.References.GetValue("BaseTopic").Id);

    }

    /*==========================================================================================================================
    | IS DIRTY: NEW TOPIC: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_NewTopic_ReturnsTrue() {

      var topic                 = TopicFactory.Create("Topic", "Page");

      Assert.IsTrue(topic.IsDirty());

    }

    /*==========================================================================================================================
    | IS DIRTY: EXISTING TOPIC: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_ExistingTopic_ReturnsFalse() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);

      Assert.IsFalse(topic.IsDirty());

    }

    /*==========================================================================================================================
    | IS DIRTY: CHANGE KEY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Key"/>, and confirms that <see cref="Topic.IsDirty(Boolean,
    ///   Boolean)"/> returns <c>true</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_ChangeKey_ReturnsTrue() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);

      topic.Key                 = "NewTopic";

      Assert.IsTrue(topic.IsDirty());

    }

    /*==========================================================================================================================
    | IS DIRTY: CHANGE COLLECTIONS: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Attributes"/>, <see cref="Topic.References"/>, and <see cref=
    ///   "Topic.Relationships"/> collections, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns
    ///   <c>true</c>.
    /// </summary>
    [TestMethod]
    public void IsDirty_ChangeCollections_ReturnsTrue() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var related               = TopicFactory.Create("Related", "Page", 2);

      topic.Attributes.SetValue("Related", related.Key);
      topic.References.SetValue("Related", related);
      topic.Relationships.SetTopic("Related", related);

      Assert.IsTrue(topic.IsDirty(true));

    }

    /*==========================================================================================================================
    | MARK CLEAN: CHANGE COLLECTION: RESETS IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Attributes"/>, <see cref="Topic.References"/>, and <see cref=
    ///   "Topic.Relationships"/> collections, and confirms that <see cref="Topic.MarkClean(Boolean, DateTime?)"/> resets the
    ///   value of <see cref="Topic.IsDirty(Boolean, Boolean)"/>.
    /// </summary>
    [TestMethod]
    public void MarkClean_ChangeCollection_ResetIsDirty() {

      var topic                 = TopicFactory.Create("Topic", "Page", 1);
      var related               = TopicFactory.Create("Related", "Page", 2);

      topic.Attributes.SetValue("Related", related.Key);
      topic.References.SetValue("Related", related);
      topic.Relationships.SetTopic("Related", related);

      topic.MarkClean(true);

      Assert.IsFalse(topic.IsDirty(true));

    }

  } //Class
} //Namespace