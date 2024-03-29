﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;
using OnTopic.Metadata;
using OnTopic.Repositories;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Topic"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class TopicTest {

    /*==========================================================================================================================
    | TEST: CREATE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures it's correctly returned.
    /// </summary>
    [Fact]
    public void Create_ReturnsTopic() {
      var topic = TopicFactory.Create("Test", "Page");
      Assert.NotNull(topic);
      Assert.Equal("Test", topic.Key);
      Assert.Equal("Page", topic.ContentType);
    }

    /*==========================================================================================================================
    | TEST: CREATE: CONTENT TYPE: RETURNS DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic of a content type which maps to a class derived from <see cref="Topic"/>, and ensures the derived
    ///   version of the <see cref="Topic"/> class is returned.
    /// </summary>
    [Fact]
    public void Create_ContentType_ReturnsDerivedTopic() {
      var topic = TopicFactory.Create("Test", "ContentTypeDescriptor");
      Assert.NotNull(topic);
      Assert.IsType<ContentTypeDescriptor>(topic);
    }

    /*==========================================================================================================================
    | TEST: CREATE: ATTRIBUTE DESCRIPTOR: RETURNS FALLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic with a <see cref="Topic.ContentType"/> ending with <c>AttributeDescriptor</c> and ensures that, by
    ///   convention, a <see cref="AttributeDescriptor"/> is returned.
    /// </summary>
    /// <remarks>
    ///   This is a special use case to address the fact that we expect concrete types of <see cref="AttributeDescriptor"/> to
    ///   be in external plugin libraries, but the <see cref="ITopicRepository"/> only needs to know that they're an <see cref="
    ///   AttributeDescriptor"/>. This is similar to how other types will fallback to <see cref="Topic"/> if no matching type
    ///   can be found in the <see cref="TopicFactory.TypeLookupService"/>.
    /// </remarks>
    [Fact]
    public void Create_AttributeDescriptor_ReturnsFallback() {
      var topic = TopicFactory.Create("Test", "ArbitraryAttributeDescriptor");
      Assert.NotNull(topic);
      Assert.IsType<AttributeDescriptor>(topic);
    }

    /*==========================================================================================================================
    | TEST: ID: CHANGE VALUE: THROWS ARGUMENT EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures that the ID cannot be modified.
    /// </summary>
    [Fact]
    public void Id_ChangeValue_ThrowsArgumentException() {

      var topic                 = new ContentTypeDescriptor("Test", "ContentTypeDescriptor", null, 123);

      Assert.Throws<InvalidOperationException>(() =>
        topic.Id = 124
      );

    }

    /*==========================================================================================================================
    | TEST: KEY: CHANGE VALUE: UPDATES PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes a <see cref="Topic.Key"/>, and confirms that the <see cref="Topic.Parent"/>'s <see cref="Topic.Children"/>
    ///   collection is updated to reflect the new <see cref="Topic.Key"/>.
    /// </summary>
    /// <remarks>
    ///   By default, <see cref="KeyedTopicCollection{T}"/> won't automatically update its key if the underlying <see cref="
    ///   Topic.Key"/> changed. We have code that will handle that, however.
    /// </remarks>
    [Fact]
    public void Key_ChangeValue_UpdatesParent() {

      var parent                = new ContentTypeDescriptor("Test", "ContentTypeDescriptor", null, 1);
      var topic                 = new ContentTypeDescriptor("Original", "ContentTypeDescriptor", parent, 2) {
        Key                     = "New"
      };

      Assert.Equal("New", topic.Key);
      Assert.True(topic.IsDirty("Key"));
      Assert.True(parent.Children.Contains("New"));
      Assert.False(parent.Children.Contains("Original"));

    }

    /*==========================================================================================================================
    | TEST: PARENT: SET VALUE: UPDATES PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [Fact]
    public void Parent_SetValue_UpdatesParent() {

      var parentTopic           = new ContentTypeDescriptor("Parent", "ContentTypeDescriptor");
      var childTopic            = new ContentTypeDescriptor("Child", "ContentTypeDescriptor");

      parentTopic.Id            = 5;
      childTopic.Parent         = parentTopic;

      Assert.Equal(parentTopic.Children["Child"], childTopic);
      Assert.Equal(5, childTopic.Parent.Id);

    }

    /*==========================================================================================================================
    | TEST: PARENT: SET TO DESCENDANT: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the <see cref="Topic.Parent"/> to a <see cref="Topic"/> that is a descendant, and ensure it throws an <see cref="
    ///   ArgumentOutOfRangeException"/>.
    /// </summary>
    [Fact]
    public void Parent_SetToDescendant_ThrowsException() {

      var parentTopic           = new ContentTypeDescriptor("Parent", "ContentTypeDescriptor");
      var childTopic            = new ContentTypeDescriptor("Child", "ContentTypeDescriptor", parentTopic);

      Assert.Throws<ArgumentOutOfRangeException>(() =>
        parentTopic.Parent      = childTopic
      );

    }

    /*==========================================================================================================================
    | TEST: PARENT: DUPLICATE KEY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the <see cref="Topic.Parent"/> to a <see cref="Topic"/> whose <see cref="Topic.Key"/> already exists in the new
    ///   <see cref="Topic.Parent"/> and ensures that an <see cref="InvalidKeyException"/> is thrown.
    /// </summary>
    [Fact]
    public void Parent_DuplicateKey_ThrowsException() {

      var parentTopic           = new Topic("Parent", "ContentTypeDescriptor");
      _                         = new Topic("Child", "ContentTypeDescriptor", parentTopic);

      Assert.Throws<InvalidKeyException>(() =>
        _                       = new Topic("Child", "ContentTypeDescriptor", parentTopic)
      );

    }

    /*==========================================================================================================================
    | TEST: PARENT: CHANGE VALUE: UPDATES PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [Fact]
    public void Parent_ChangeValue_UpdatesParent() {

      var sourceParent          = new ContentTypeDescriptor("SourceParent", "ContentTypeDescriptor", null, 5);
      var targetParent          = new ContentTypeDescriptor("TargetParent", "ContentTypeDescriptor", null, 10);
      var childTopic            = new ContentTypeDescriptor("ChildTopic", "ContentTypeDescriptor", sourceParent) {
        Parent                  = targetParent
      };

      Assert.Equal(targetParent.Children["ChildTopic"], childTopic);
      Assert.True(targetParent.Children.Contains("ChildTopic"));
      Assert.False(sourceParent.Children.Contains("ChildTopic"));
      Assert.Equal(10, childTopic.Parent.Id);

    }

    /*==========================================================================================================================
    | TEST: UNIQUE KEY: RETURNS UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures the Unique Key is correct for a deeply nested child.
    /// </summary>
    [Fact]
    public void UniqueKey_ReturnsUniqueKey() {

      var parentTopic           = new Topic("ParentTopic", "Page");
      var childTopic            = new Topic("ChildTopic", "Page");
      var grandChildTopic       = new Topic("GrandChildTopic", "Page");

      childTopic.Parent         = parentTopic;
      grandChildTopic.Parent    = childTopic;

      Assert.Equal("ParentTopic:ChildTopic:GrandChildTopic", grandChildTopic.GetUniqueKey());
      Assert.Equal("/ParentTopic/ChildTopic/GrandChildTopic/", grandChildTopic.GetWebPath());

    }

    /*==========================================================================================================================
    | TEST: IS VISIBLE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="Topic.IsVisible(Boolean)"/> returns expected values based on <see cref="Topic.IsHidden"/> and
    ///   <see cref="Topic.IsDisabled"/>.
    /// </summary>
    [Fact]
    public void IsVisible_ReturnsExpectedValue() {

      var hiddenTopic           = new Topic("HiddenTopic", "Page");
      var disabledTopic         = new Topic("DisabledTopic", "Page");
      var visibleTopic          = new Topic("VisibleTopic", "Page");

      hiddenTopic.IsHidden      = true;
      disabledTopic.IsDisabled  = true;

      Assert.False(hiddenTopic.IsVisible());
      Assert.False(hiddenTopic.IsVisible(true));
      Assert.False(disabledTopic.IsVisible());
      Assert.True(disabledTopic.IsVisible(true));
      Assert.True(visibleTopic.IsVisible());
      Assert.True(visibleTopic.IsVisible(true));

    }

    /*==========================================================================================================================
    | TEST: TITLE: NULL VALUE: RETURNS KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that the title falls back appropriately.
    /// </summary>
    [Fact]
    public void Title_NullValue_ReturnsKey() {

      var titledTopic           = new Topic("TitledTopic", "Page");
      var untitledTopic         = new Topic("UntitledTopic", "Page");

      titledTopic.Title         = "Titled Topic";

      Assert.Equal("UntitledTopic", untitledTopic.Title);
      Assert.Equal("Titled Topic", titledTopic.Title);

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="Topic.LastModified"/>, and ensures it's returned correctly.
    /// </summary>
    [Fact]
    public void LastModified_UpdateLastModified_ReturnsExpectedValue() {

      var topic                 = new Topic("Topic1", "Page");
      var lastModified          = new DateTime(1976, 10, 15);

      topic.LastModified        = lastModified;

      Assert.Equal(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="Topic.VersionHistory"/>, and ensures it's returned correctly.
    /// </summary>
    [Fact]
    public void LastModified_UpdateVersionHistory_ReturnsExpectedValue() {

      var topic                 = new Topic("Topic2", "Page");

      var lastModified          = new DateTime(1976, 10, 15);

      topic.VersionHistory.Add(lastModified);

      Assert.Equal(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: LAST MODIFIED: UPDATE ATTRIBUTE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the last modified date via <see cref="AttributeCollection"/>, and ensures it's returned correctly.
    /// </summary>
    [Fact]
    public void LastModified_UpdateValue_ReturnsExpectedValue() {

      var topic                 = new Topic("Topic3", "Page");

      var lastModified          = new DateTime(1976, 10, 15);

      topic.Attributes.SetValue("LastModified", lastModified.ToShortDateString());

      Assert.Equal(lastModified, topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: BASE TOPIC: UPDATE VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a base topic to a topic entity, then replaces the references with a new topic entity. Ensures that both the
    ///   base topic as well as the underlying <see cref="AttributeRecord"/> correctly reference the new value.
    /// </summary>
    [Fact]
    public void BaseTopic_UpdateValue_ReturnsExpectedValue() {

      var topic                 = new Topic("Topic", "Page");
      var firstBaseTopic        = new Topic("BaseTopic", "Page");
      var secondBaseTopic       = new Topic("BaseTopic", "Page", null, 1);
      var finalBaseTopic        = new Topic("BaseTopic", "Page", null, 2);

      topic.BaseTopic           = firstBaseTopic;
      topic.BaseTopic           = secondBaseTopic;
      topic.BaseTopic           = finalBaseTopic;

      Assert.Equal(topic.BaseTopic, finalBaseTopic);
      Assert.Equal(2, topic.References.GetValue("BaseTopic")?.Id);

    }

    /*==========================================================================================================================
    | TEST: BASE TOPIC: RESAVED VALUE: RETURNS EXPECTED VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a base topic to an unsaved topic entity, then saves the entity and reestablishes the reference. Ensures that the
    ///   base topic is correctly set as a <see cref="Topic.References"/> entry.
    /// </summary>
    [Fact]
    public void BaseTopic_ResavedValue_ReturnsExpectedValue() {

      var topic                 = new Topic("Topic", "Page");
      var baseTopic             = new Topic("BaseTopic", "Page");

      topic.BaseTopic           = baseTopic;
      baseTopic.Id              = 5;
      topic.BaseTopic           = baseTopic;

      Assert.Equal(topic.BaseTopic, baseTopic);
      Assert.Equal(5, topic.References.GetValue("BaseTopic")?.Id);

    }

    /*==========================================================================================================================
    | TEST: BASE TOPIC: SET TO NULL: REMOVES VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a base topic to a topic entity, then updates it to a null value. Ensures that the base topic is correctly
    ///   removed.
    /// </summary>
    [Fact]
    public void BaseTopic_SetToNull_RemovesValue() {

      var topic                 = new Topic("Topic", "Page");
      var baseTopic             = new Topic("BaseTopic", "Page");

      topic.BaseTopic           = baseTopic;
      topic.BaseTopic           = null;

      Assert.Null(topic.BaseTopic);

    }

    /*==========================================================================================================================
    | IS DIRTY: NEW TOPIC: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_NewTopic_ReturnsTrue() =>
      Assert.True(new Topic("Topic", "Page").IsDirty());

    /*==========================================================================================================================
    | IS DIRTY: EXISTING TOPIC: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns <c>false</c>.
    /// </summary>
    [Fact]
    public void IsDirty_ExistingTopic_ReturnsFalse() =>
      Assert.False(new Topic("Topic", "Page", null, 1).IsDirty());

    /*==========================================================================================================================
    | IS DIRTY: CHANGE KEY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Key"/>, and confirms that <see cref="Topic.IsDirty(Boolean,
    ///   Boolean)"/> returns <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_ChangeKey_ReturnsTrue() =>
      Assert.True(
        new Topic("Topic", "Page", null, 1) {
          Key                     = "NewTopic"
        }.IsDirty()
      );

    /*==========================================================================================================================
    | TEST: IS DIRTY: EXISTING VALUES: REMAINS CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, and updates the <see cref="Topic.Key"/>, <see cref="Topic.ContentType"/>, and <see cref="
    ///   Topic.Parent"/> to their existing values. Ensures that <see cref="Topic.IsDirty(String)"/> remains <c>false</c>.
    /// </summary>
    [Fact]
    public void IsDirty_ExistingValue_RemainsClean() {

      var parent                = new Topic("Parent", "Page", null, 1);
      var topic                 = new Topic("Topic", "Page", parent, 2);

      topic.Key                 = topic.Key;
      topic.ContentType         = topic.ContentType;
      topic.Parent              = parent;

      Assert.False(topic.IsDirty());

    }

    /*==========================================================================================================================
    | IS DIRTY: CHANGE COLLECTIONS: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Attributes"/>, <see cref="Topic.References"/>, and <see cref=
    ///   "Topic.Relationships"/> collections, and confirms that <see cref="Topic.IsDirty(Boolean, Boolean)"/> returns
    ///   <c>true</c>.
    /// </summary>
    [Fact]
    public void IsDirty_ChangeCollections_ReturnsTrue() {

      var topic                 = new Topic("Topic", "Page", null, 1);
      var related               = new Topic("Related", "Page", null, 2);

      topic.Attributes.SetValue("Related", related.Key);
      topic.References.SetValue("Related", related);
      topic.Relationships.SetValue("Related", related);

      Assert.True(topic.IsDirty(true));
      Assert.True(topic.IsDirty("Related", true));

    }

    /*==========================================================================================================================
    | MARK CLEAN: CHANGE COLLECTIONS: RESETS IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Attributes"/>, <see cref="Topic.References"/>, and <see cref=
    ///   "Topic.Relationships"/> collections, and confirms that <see cref="Topic.MarkClean(Boolean, DateTime?)"/> resets the
    ///   value of <see cref="Topic.IsDirty(Boolean, Boolean)"/>.
    /// </summary>
    [Fact]
    public void MarkClean_ChangeCollections_ResetIsDirty() {

      var topic                 = new Topic("Topic", "Page", null, 1);
      var related               = new Topic("Related", "Page", null, 2);

      topic.Attributes.SetValue("Related", related.Key);
      topic.References.SetValue("Related", related);
      topic.Relationships.SetValue("Related", related);

      topic.MarkClean(true);

      Assert.False(topic.IsDirty(true));

    }

    /*==========================================================================================================================
    | MARK CLEAN: INCLUDE COLLECTIONS: RESETS IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates an existing topic, changes the <see cref="Topic.Attributes"/>, <see cref="Topic.References"/>, and <see cref=
    ///   "Topic.Relationships"/> collections, and confirms that <see cref="Topic.MarkClean(String, Boolean)"/> resets the value
    ///   of <see cref="Topic.IsDirty(Boolean, Boolean)"/>.
    /// </summary>
    [Fact]
    public void MarkClean_IncludeCollections_ResetsIsDirty() {

      var topic                 = new Topic("Topic", "Page", null, 1);
      var related               = new Topic("Related", "Page", null, 2);

      topic.Attributes.SetValue("Related", related.Key);
      topic.References.SetValue("Related", related);
      topic.Relationships.SetValue("Related", related);

      topic.MarkClean("Related", true);

      Assert.False(topic.IsDirty("Related", true));
      Assert.False(topic.IsDirty(true));

    }


    /*==========================================================================================================================
    | MARK CLEAN: NEW TOPIC: REMAINS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new <see cref="Topic"/> and confirms that <see cref="Topic.MarkClean()"/> does <i>not</i> reset the value of
    ///   <see cref="Topic.IsDirty(Boolean, Boolean)"/>. Topics that are marked as <see cref="Topic.IsNew"/> cannot be clean.
    /// </summary>
    [Fact]
    public void MarkClean_NewTopic_RemainsDirty() {

      var topic                 = new Topic("Topic", "Page");

      topic.Attributes.SetValue("Attribute", "Test");

      topic.MarkClean("Attribute", true);
      topic.MarkClean(true);

      Assert.True(topic.IsDirty());
      Assert.True(topic.IsDirty("Attribute", true));

    }

  } //Class
} //Namespace