/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
  [ExcludeFromCodeCoverage]
  public class StubTopicRepository : TopicRepository, ITopicRepository {

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
    public override Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) =>
      (topicId < 0)? _cache :_cache.FindFirst(t => t.Id.Equals(topicId));

    /// <inheritdoc />
    public override Topic? Load(string? uniqueKey = null, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (uniqueKey is not null && uniqueKey.Length > 0) {
        uniqueKey = uniqueKey.Contains(":", StringComparison.Ordinal) ? uniqueKey : "Root:" + uniqueKey;
        return _cache.FindFirst(t => t.GetUniqueKey().Equals(uniqueKey, StringComparison.OrdinalIgnoreCase));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.UtcNow, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = Load(topicId, referenceTopic, false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset version
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is not null) {
        topic.LastModified = version;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic?? throw new TopicNotFoundException(topicId);

    }

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Refresh(Topic referenceTopic, DateTime since) { }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void SaveTopic([NotNull]Topic topic, DateTime version, bool persistRelationships) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign faux identity
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.IsNew) {
        topic.Id = _identity++;
      }

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void MoveTopic(Topic topic, Topic target, Topic? sibling = null) { }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override void DeleteTopic(Topic topic) { }

    /*==========================================================================================================================
    | METHOD: GET ATTRIBUTES (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)" />
    public IEnumerable<AttributeRecord> GetAttributesProxy(
      Topic topic,
      bool? isExtendedAttribute,
      bool? isDirty = null,
      bool excludeLastModified = false
    ) => base.GetAttributes(topic, isExtendedAttribute, isDirty, excludeLastModified);

    /*==========================================================================================================================
    | METHOD: GET UNMATCHED ATTRIBUTES (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepository.GetUnmatchedAttributes(Topic)" />
    public IEnumerable<AttributeDescriptor> GetUnmatchedAttributesProxy(Topic topic) => base.GetUnmatchedAttributes(topic);

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTORS (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepository.GetContentTypeDescriptors(ContentTypeDescriptor)" />
    [ExcludeFromCodeCoverage]
    [Obsolete("Deprecated. Instead, use the new SetContentTypeDescriptorsProxy(), which provides the same function.", true)]
    public ContentTypeDescriptorCollection GetContentTypeDescriptorsProxy(ContentTypeDescriptor topicGraph) =>
      base.SetContentTypeDescriptors(topicGraph);

    /*==========================================================================================================================
    | METHOD: SET CONTENT TYPE DESCRIPTORS (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepository.SetContentTypeDescriptors(ContentTypeDescriptor)" />
    public ContentTypeDescriptorCollection SetContentTypeDescriptorsProxy(ContentTypeDescriptor topicGraph) =>
      base.SetContentTypeDescriptors(topicGraph);

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTOR (PROXY)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="TopicRepository.GetContentTypeDescriptor(Topic)" />
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

      addAttribute(contentTypes, "Key", "TextAttributeDescriptor", false, true);
      addAttribute(contentTypes, "ContentType", "TextAttributeDescriptor", false, true);
      addAttribute(contentTypes, "Title", "TextAttributeDescriptor", true, true);
      addAttribute(contentTypes, "BaseTopic", "TopicReferenceAttributeDescriptor", false);

      var contentTypeDescriptor = TopicFactory.Create("ContentTypeDescriptor", "ContentTypeDescriptor", contentTypes);

      addAttribute(contentTypeDescriptor, "ContentTypes", "RelationshipAttributeDescriptor");
      addAttribute(contentTypeDescriptor, "Attributes", "NestedTopicListAttributeDescriptor");

      TopicFactory.Create("Container", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("Lookup", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("LookupListItem", "ContentTypeDescriptor", contentTypes);
      TopicFactory.Create("List", "ContentTypeDescriptor", contentTypes);

      var attributeDescriptor = (ContentTypeDescriptor)TopicFactory.Create("AttributeDescriptor", "ContentTypeDescriptor", contentTypes);

      addAttribute(attributeDescriptor, "DefaultValue", "TextAttributeDescriptor", false, true);
      addAttribute(attributeDescriptor, "IsRequired", "TextAttributeDescriptor", false, true);

      TopicFactory.Create("BooleanAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("NestedTopicListAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("NumberAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("RelationshipAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("TextAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);
      TopicFactory.Create("TopicReferenceAttributeDescriptor", "ContentTypeDescriptor", attributeDescriptor);

      var pageContentType = TopicFactory.Create("Page", "ContentTypeDescriptor", contentTypes);

      addAttribute(pageContentType, "MetaTitle");
      addAttribute(pageContentType, "MetaDescription");
      addAttribute(pageContentType, "IsHidden", "TextAttributeDescriptor", false);
      addAttribute(pageContentType, "TopicReference", "TopicReferenceAttributeDescriptor", false);

      pageContentType.Relationships.SetValue("ContentTypes", pageContentType);
      pageContentType.Relationships.SetValue("ContentTypes", contentTypeDescriptor);

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
        string editorType       = "TextAttributeDescriptor",
        bool isExtended         = true,
        bool isRequired         = false
      ) {
        var container = contentType.Children.GetValue("Attributes");
        if (container is null) {
          container = TopicFactory.Create("Attributes", "List", contentType);
          container.Attributes.SetBoolean("IsHidden", true);
        }
        var attribute = (AttributeDescriptor)TopicFactory.Create(attributeKey, editorType, container, currentAttributeId++);
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
      var web = TopicFactory.Create("Web", "Page", rootTopic, 10000);

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
        var topic = TopicFactory.Create(parent.Key + "_" + i, "Page", parent, parent.Id + (int)Math.Pow(10, depth) * i);
        topic.Attributes.SetValue("ParentKey", parent.Key);
        topic.Attributes.SetValue("DepthCount", (depth+i).ToString(CultureInfo.InvariantCulture));
        if (depth > 0) {
          CreateFakeData(topic, count, depth - 1);
        }
      }
    }

  } //Class
} //Namespace