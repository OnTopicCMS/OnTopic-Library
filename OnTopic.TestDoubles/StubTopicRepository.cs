/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Globalization;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.TestDoubles {

  /*============================================================================================================================
  | CLASS: STUB TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics hard-coded in this stub implementation of a <see cref="ITopicRepository"/>.
  /// </summary>
  /// <remarks>
  ///   Allows testing of services that depend on <see cref="ITopicRepository"/> without maintaining a dependency on a live
  ///   database, or working against actual data. This is faster and safer for test methods since it doesn't maintain a
  ///   dependency on a live database or persistent data.
  /// </remarks>
  public class StubTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     int                             _identity                       = 1;
    private readonly            Topic                           _cache                          ;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the StubTopicRepository.
    /// </summary>
    /// <returns>A new instance of the StubTopicRepository.</returns>
    public StubTopicRepository() : base() {
      _cache = CreateFakeData();
      Contract.Assume(_cache);
    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, bool isRecursive = true) =>
      (topicId < 0)? _cache :_cache.FindFirst(t => t.Id.Equals(topicId));

    /// <inheritdoc />
    public override Topic? Load(string? topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicKey != null && topicKey.Length > 0) {
        topicKey = topicKey.Contains(":") ? topicKey : "Root:" + topicKey;
        return _cache.FindFirst(t => t.GetUniqueKey().Equals(topicKey, StringComparison.InvariantCultureIgnoreCase));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = Load(topicId);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset version
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic != null) {
        topic.LastModified = version;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic?? throw new TopicNotFoundException(topicId);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse through children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.IsNew) {
        topic.Id = _identity++;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse through children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          Save(childTopic, isRecursive, isDraft);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return identity
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic.Id;

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Move(Topic topic, Topic target, Topic? sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset dirty status
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ParentId", target.Id.ToString(CultureInfo.InvariantCulture), false);

    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive = false) => base.Delete(topic, isRecursive);

    /*==========================================================================================================================
    | METHOD: GET ATTRIBUTES (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?)" />
    public IEnumerable<AttributeValue> GetAttributesProxy(
      Topic topic,
      bool? isExtendedAttribute,
      bool? isDirty = null,
      bool excludeLastModified = false
    ) => base.GetAttributes(topic, isExtendedAttribute, isDirty, excludeLastModified);

    /*==========================================================================================================================
    | METHOD: GET UNMATCHED ATTRIBUTES (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepositoryBase.GetUnmatchedAttributes(Topic)" />
    public IEnumerable<AttributeDescriptor> GetUnmatchedAttributesProxy(Topic topic) => base.GetUnmatchedAttributes(topic);

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTORS (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepositoryBase.GetContentTypeDescriptors(ContentTypeDescriptor)" />
    [Obsolete("Deprecated. Instead, use the new SetContentTypeDescriptorsProxy(), which provides the same function.", false)]
    public ContentTypeDescriptorCollection GetContentTypeDescriptorsProxy(ContentTypeDescriptor topicGraph) =>
      base.SetContentTypeDescriptors(topicGraph);

    /*==========================================================================================================================
    | METHOD: SET CONTENT TYPE DESCRIPTORS (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepositoryBase.SetContentTypeDescriptors(ContentTypeDescriptor)" />
    public ContentTypeDescriptorCollection SetContentTypeDescriptorsProxy(ContentTypeDescriptor topicGraph) =>
      base.SetContentTypeDescriptors(topicGraph);

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTOR (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepositoryBase.GetContentTypeDescriptor(Topic)" />
    public ContentTypeDescriptor? GetContentTypeDescriptorProxy(Topic sourceTopic) =>
      base.GetContentTypeDescriptor(sourceTopic);

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data that loosely mimics a bare bones database.
    /// </summary>
    private Topic CreateFakeData() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = TopicFactory.Create("Root", "Container", 900);
      var currentAttributeId = 800;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish configuration
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration = TopicFactory.Create("Configuration", "Container", rootTopic);
      var contentTypes = TopicFactory.Create("ContentTypes", "ContentTypeDescriptor", configuration);

      addAttribute(contentTypes, "Key", "TextAttribute", false, true);
      addAttribute(contentTypes, "ContentType", "TextAttribute", false, true);
      addAttribute(contentTypes, "Title", "TextAttribute", true, true);
      addAttribute(contentTypes, "TopicId", "TopicReferenceAttribute", false);

      var contentTypeDescriptor = TopicFactory.Create("ContentTypeDescriptor", "ContentTypeDescriptor", contentTypes);

      addAttribute(contentTypeDescriptor, "ContentTypes", "RelationshipAttribute");
      addAttribute(contentTypeDescriptor, "Attributes", "NestedTopicListAttribute");

      TopicFactory.Create("Container", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("Lookup", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("LookupListItem", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("List", "ContentTypeDescriptor", contentTypes);

      var attributeDescriptor = (ContentTypeDescriptor)TopicFactory.Create("AttributeDescriptor", "ContentTypeDescriptor", contentTypes);

      addAttribute(attributeDescriptor, "DefaultValue", "TextAttribute", false, true);
      addAttribute(attributeDescriptor, "IsRequired", "TextAttribute", false, true);

      TopicFactory.Create("BooleanAttribute", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("NestedTopicListAttribute", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("NumberAttribute", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("RelationshipAttribute", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("TextAttribute", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("TopicReferenceAttribute", "ContentTypeDescriptor", attributeDescriptor);

      var pageContentType = TopicFactory.Create("Page", "ContentTypeDescriptor", contentTypes);

      addAttribute(pageContentType, "MetaTitle");
      addAttribute(pageContentType, "MetaDescription");
      addAttribute(pageContentType, "IsHidden", "TextAttribute", false);
      addAttribute(pageContentType, "TopicReference", "TopicReferenceAttribute", false);

      pageContentType.Relationships.SetTopic("ContentTypes", pageContentType);
      pageContentType.Relationships.SetTopic("ContentTypes", contentTypeDescriptor);

      var contactContentType = TopicFactory.Create("Contact", "ContentTypeDescriptor", contentTypes);

      addAttribute(contactContentType, "Name", isExtended: false);
      addAttribute(contactContentType, "AlternateEmail", isExtended: false);
      addAttribute(contactContentType, "BillingContactEmail", isExtended: false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Local addAttribute() helper function
      \-----------------------------------------------------------------------------------------------------------------------*/
      AttributeDescriptor addAttribute(
        Topic contentType,
        string attributeKey,
        string editorType       = "TextAttribute",
        bool isExtended         = true,
        bool isRequired         = false
      ) {
        var container = contentType.Children.GetTopic("Attributes");
        if (container == null) {
          container = TopicFactory.Create("Attributes", "List", contentType);
          container.Attributes.SetBoolean("IsHidden", true);
        }
        var attribute = (AttributeDescriptor)TopicFactory.Create(attributeKey, editorType, currentAttributeId++, container);
        attribute.IsRequired = isRequired;
        attribute.IsExtendedAttribute = isExtended;
        return attribute;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish metadata
      \-----------------------------------------------------------------------------------------------------------------------*/
      var metadata = TopicFactory.Create("Metadata", "Container", configuration);
      var categories = TopicFactory.Create("Categories", "Lookup", metadata);
      var lookup = TopicFactory.Create("LookupList", "List", categories);

      for (var i=1; i<=5; i++) {
        TopicFactory.Create("Category" + i, "LookupListItem", lookup);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish content
      \-----------------------------------------------------------------------------------------------------------------------*/
      var web = TopicFactory.Create("Web", "Page", 10000, rootTopic);

      CreateFakeData(web, 2, 3);

      var pageGroup = TopicFactory.Create("Web_3", "PageGroup", web);

      TopicFactory.Create("Web_3_0", "Page", pageGroup);
      TopicFactory.Create("Web_3_1", "Page", pageGroup);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set to cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return rootTopic;

    }

    /*==========================================================================================================================
    | METHOD: CREATE FAKE DATA
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a collection of fake data recursively based on a parent topic, and set number of levels.
    /// </summary>
    private void CreateFakeData(Topic parent, int count = 3, int depth = 3) {
      for (var i = 0; i < count; i++) {
        var topic = TopicFactory.Create(parent.Key + "_" + i, "Page", parent.Id + (int)Math.Pow(10, depth) * i, parent);
        topic.Attributes.SetValue("ParentKey", parent.Key);
        topic.Attributes.SetValue("DepthCount", (depth+i).ToString(CultureInfo.InvariantCulture));
        if (depth > 0) {
          CreateFakeData(topic, count, depth - 1);
        }
      }
    }

  } //Class
} //Namespace