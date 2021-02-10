/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Associations;

namespace OnTopic.Metadata {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Content types provide schema information for each topic, including which attributes that topic is expected to include.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
  ///     Editor (via the <see cref="AttributeDescriptors"/> property).
  ///   </para>
  ///   <para>
  ///     Each content type associated with a <see cref="Topic"/> is itself a <see cref="Topic"/> with a Content Type of
  ///     "Content Type". The attributes of the "Content Type" Content Type represent the metadata associated with every content
  ///     type. For example, the "Content Type" Content Type has attributes such as <see cref="AttributeDescriptors"/> which
  ///     represents which attributes should be associated with each instance of a <see cref="ContentTypeDescriptor"/>. To
  ///     represent this, the <see cref="ContentTypeDescriptor"/> class provides a strongly-typed derivation of the <see
  ///     cref="Topic"/> class, with properties mapping to attributes specific to the "Content Type" Content Type.
  ///   </para>
  /// </remarks>
  //  ### TODO JJC082515: Should add a Get() to simply checks against the collection (e.g., via Contains() then indexer[]).
  public class ContentTypeDescriptor : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private   AttributeDescriptorCollection?                    _attributeDescriptors;
    private   ReadOnlyKeyedTopicCollection<ContentTypeDescriptor>? _permittedContentTypes;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="ContentTypeDescriptor"/> class with a <see cref="Topic.Key"/>, <see
    ///   cref="Topic.ContentType"/>, and, optionally, <see cref="Topic.Parent"/>, <see cref="Topic.Id"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Topic.Key"/> and <see
    ///   cref="Topic.ContentType"/> will be set to <see cref="TrackedRecord{T}.IsDirty"/>, which is required in order to
    ///   correctly save new topics to the database. When the <paramref name="id"/> parameter is set, however, the <see
    ///   cref="TrackedRecord{T}.IsDirty"/> property is set to <c>false</c> on <see cref="Topic.Key"/> as well as on <see
    ///   cref="Topic.ContentType"/>, since it is assumed these are being set to the same values currently used in the
    ///   persistence store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public ContentTypeDescriptor(
      string key,
      string contentType,
      Topic? parent = null,
      int id = -1
    ) : base(
      key,
      contentType,
      parent,
      id
    ) {
    }

    /*==========================================================================================================================
    | PROPERTY: DISABLE CHILD TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether instances of this <see cref="ContentTypeDescriptor"/> are permitted to contain child topics.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, topics are hierarchical: any topic can contain any number of child topics. While this flexibility makes
    ///     sense for both the library and the underlying data stores, however, it doesn't always make sense for individual
    ///     topics. For example, while it may make sense for a "Page" topic to have child "Page" topics, it likely won't make
    ///     sense for an "Article" topic to. This property allows this functionality to be disabled.
    ///   </para>
    ///   <para>
    ///     It is important to note that this functionality only affects the Topic Editor interface, by disabling the option to
    ///     add a child topic. This functionality is not enforced by the library itself. As such, developers may
    ///     programmatically add child topics to a topic regardless of whether it's associated <see
    ///     cref="ContentTypeDescriptor"/> is set to <see cref="DisableChildTopics"/>.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public bool DisableChildTopics {
      get => Attributes.GetBoolean("DisableChildTopics");
      set => SetAttributeValue("DisableChildTopics", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | PROPERTY: PERMITTED CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a read-only collection of what types of content types are permitted to be created as children of instances of
    ///   this content type.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, topics are hierarchical: any topic can contain any number of child topics. While this flexibility makes
    ///     sense for both the library and the underlying data stores, however, it doesn't always make sense for individual
    ///     topics. For example, while it may make sense for a "Page" topic to have child "Comment" topics, it wouldn't make
    ///     sense for "Comment" topics to have child "Page" topics. This property allows the topics library to configure which
    ///     types of topics can be created under instances of the current content type.
    ///   </para>
    ///   <para>
    ///     It is important to note that this functionality only affects the Topic Editor interface by restricting the content
    ///     type of topics created under instances of this content type. This functionality is not enforced by the library
    ///     itself. As such, developers may programmatically add child topics to a topic regardless of whether it's associated
    ///     <see cref="ContentTypeDescriptor"/> permits topics of that Content Type to be created.
    ///   </para>
    ///   <para>
    ///     To add content types to the <see cref="PermittedContentTypes"/> collection, use <see
    ///     cref="TopicRelationshipMultiMap.SetTopic(String, Topic, Boolean?)"/>.
    ///   </para>
    /// </remarks>
    public ReadOnlyKeyedTopicCollection<ContentTypeDescriptor> PermittedContentTypes {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate values from relationships
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_permittedContentTypes is null) {
          var permittedContentTypes = new KeyedTopicCollection<ContentTypeDescriptor>();
          var contentTypes = Relationships.GetTopics("ContentTypes");
          foreach (ContentTypeDescriptor contentType in contentTypes) {
            permittedContentTypes.Add(contentType);
          }
          _permittedContentTypes = new(permittedContentTypes);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return values
        \---------------------------------------------------------------------------------------------------------------------*/
        return _permittedContentTypes;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of <see cref="AttributeDescriptor"/> objects that are supported for objects implementing this
    ///   ContentType.
    /// </summary>
    /// <remarks>
    ///   Attributes are not just derived from the specific Content Type topic in the database. They are also inherited from
    ///   any parent content types. For instance, if a Content Type "Page" has an attribute "Body", then all Content Types
    ///   created underneath "Page" will also have an attribute "Body". As such, the <see cref="AttributeDescriptors"/> property
    ///   must crawl through each parent Content Type to collate the list of supported attributes.
    /// </remarks>
    public AttributeDescriptorCollection AttributeDescriptors {
      get {

        if (_attributeDescriptors is null) {

          /*--------------------------------------------------------------------------------------------------------------------
          | Create new instance
          \-------------------------------------------------------------------------------------------------------------------*/
          _attributeDescriptors = new();

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from nested topics
          \-------------------------------------------------------------------------------------------------------------------*/
          if (Children.Contains("Attributes")) {
            foreach (AttributeDescriptor attribute in Children["Attributes"].Children) {
              _attributeDescriptors.Add(attribute);
            }
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from parent
          \-------------------------------------------------------------------------------------------------------------------*/
          var parent = Parent as ContentTypeDescriptor;
          if (parent?.AttributeDescriptors is not null) {
            foreach (var attribute in parent.AttributeDescriptors) {
              if (!_attributeDescriptors.Contains(attribute.Key)) {
                _attributeDescriptors.Add(attribute);
              }
            }
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return the dictionary object
        \---------------------------------------------------------------------------------------------------------------------*/
        return _attributeDescriptors;

      }

    }

    /*==========================================================================================================================
    | METHOD: RESET ATTRIBUTE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Resets the list of <see cref="AttributeDescriptor"/>s stored in the <see cref="AttributeDescriptors"/> collection on
    ///   not only this <see cref="ContentTypeDescriptor"/>, but also any descendent <see cref="ContentTypeDescriptor"/>s.
    /// </summary>
    /// <remarks>
    ///   Each <see cref="ContentTypeDescriptor"/> has an <see cref="AttributeDescriptors"/> collection which includes not only
    ///   the <see cref="AttributeDescriptor"/>s associated with that <see cref="ContentTypeDescriptor"/>, but <i>also</i> any
    ///   <see cref="AttributeDescriptor"/>s from any parent <see cref="ContentTypeDescriptor"/>s in the topic graph. This
    ///   reflects the fact that attributes are inherited from parent content types. As a result, however, when an <see
    ///   cref="AttributeDescriptor"/> is added or removed, or a <see cref="ContentTypeDescriptor"/> is moved to a new parent,
    ///   this cache should be reset on the associated <see cref="ContentTypeDescriptor"/> and all descendent <see
    ///   cref="ContentTypeDescriptor"/>s to ensure the change is reflected.
    /// </remarks>
    internal void ResetAttributeDescriptors() {
      _attributeDescriptors = null!;
      foreach (var topic in Children.Where(t => t is ContentTypeDescriptor).Cast<ContentTypeDescriptor>()) {
        topic.ResetAttributeDescriptors();
      }
    }

    /*==========================================================================================================================
    | METHOD: RESET PERMITTED CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Resets the list of <see cref="ContentTypeDescriptor"/>s stored in the <see cref="PermittedContentTypes"/> collection
    ///   on this <see cref="ContentTypeDescriptor"/>.
    /// </summary>
    /// <remarks>
    ///   Each <see cref="ContentTypeDescriptor"/> has a <see cref="PermittedContentTypes"/> collection which includes the <see
    ///   cref="ContentTypeDescriptor"/>s associated with that <see cref="ContentTypeDescriptor"/>. As this is cached, however,
    ///   the value should be reset whenever a <see cref="ContentTypeDescriptor"/> has been modified, in case those values have
    ///   changed.
    /// </remarks>
    internal void ResetPermittedContentTypes() => _permittedContentTypes = null;

    /*==========================================================================================================================
    | METHOD: IS TYPE OF?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether the Content Type that the current topic implements is, or derives from, a particular Content Type.
    /// </summary>
    /// <param name="contentTypeName">The string identifier for the Content Type to compare to.</param>
    /// <returns>A Boolean value stating whether the current topic implements a particular Content Type.</returns>
    /// <requires description="The Content Type name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentTypeName)
    /// </requires>
    public bool IsTypeOf(string contentTypeName) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(!String.IsNullOrWhiteSpace(contentTypeName), "The attribute contentTypeName must be specified.");
      TopicFactory.ValidateKey(contentTypeName);

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine match
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType = (ContentTypeDescriptor?)this;

      while (contentType is not null) {
        if (contentType.Key.Equals(contentTypeName, StringComparison.OrdinalIgnoreCase)) {
          return true;
        }
        contentType = contentType.Parent as ContentTypeDescriptor;
      }

      return true;

    }

  } //Class
} //Namespace