/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Content types provide schema information for each topic, including which attributes that topic is expected to include.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
  ///     Editor (via the <see cref="SupportedAttributes"/> property). The content type also determines, by default, which view
  ///     is rendered by the <see cref="Topics.TopicRoutingService"/> (assuming the value isn't overwritten down the pipe).
  ///   </para>
  ///   <para>
  ///     Each content type associated with a <see cref="Topic"/> is itself a <see cref="Topic"/> with a Content Type of
  ///     "Content Type". The attributes of the "Content Type" Content Type represent the metadata associated with every content
  ///     type. For example, the "Content Type" Content Type has attributes such as <see cref="SupportedAttributes"/> which
  ///     represents which attributes should be associated with each instance of a <see cref="ContentTypeDescriptor"/>. To represent this,
  ///     the <see cref="ContentTypeDescriptor"/> class provides a strongly-typed derivation of the <see cref="Topic"/> class, with
  ///     properties mapping to attributes specific to the "Content Type" Content Type.
  ///   </para>
  /// </remarks>
  //  ### TODO JJC082515: Should add a Get() to simply checks against the collection (e.g., via Contains() then indexer[]).
  public class ContentTypeDescriptor : Topic {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private   TopicCollection<AttributeDescriptor>              _supportedAttributes            = null;
    private   ReadOnlyTopicCollection<ContentTypeDescriptor>    _permittedContentTypes          = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentTypeDescriptor"/> class.
    /// </summary>
    public ContentTypeDescriptor() : base() { }

    /// <summary>
    ///  Initializes a new instance of the <see cref="ContentTypeDescriptor"/> class based on the specified <see cref="Topic.Key"/>.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="ContentTypeDescriptor"/> Topic.
    /// </param>
    public ContentTypeDescriptor(string key) : base(key) { }

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
    ///     add a child topic. This functionality is not enforced by the library itself. As such, developers may programmatically
    ///     add child topics to a topic regardless of whether it's associated <see cref="ContentTypeDescriptor"/> is set to <see
    ///     cref="DisableChildTopics"/>.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public bool DisableChildTopics {
      get => (Attributes.GetValue("DisableChildTopics", "0").Equals("1"));
      set => SetAttributeValue("DisableChildTopics", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | PROPERTY: PERMITTED CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a readonly collection of what types of content types are permitted to be created as children of instances of
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
    ///     cref="Topic.SetRelationship(String, Topic, Boolean)"/>.
    ///   </para>
    /// </remarks>
    public ReadOnlyTopicCollection<ContentTypeDescriptor> PermittedContentTypes {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate contract
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<ReadOnlyCollection<ContentTypeDescriptor>>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate values from relationships
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_permittedContentTypes == null) {
          var permittedContentTypes = new TopicCollection<ContentTypeDescriptor>();
          var contentTypes = Relationships.GetTopics("ContentTypes");
          foreach (ContentTypeDescriptor contentType in contentTypes) {
            permittedContentTypes.Add(contentType);
          }
          _permittedContentTypes = new ReadOnlyTopicCollection<ContentTypeDescriptor>(permittedContentTypes);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return values
        \---------------------------------------------------------------------------------------------------------------------*/
        return _permittedContentTypes;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: SUPPORTED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of <see cref="AttributeDescriptor"/> objects that are supported for objects implementing this
    ///   ContentType.
    /// </summary>
    /// <remarks>
    ///   Attributes are not just derived from the specific Content Type topic in the database. They are also inherited from
    ///   any parent content types. For instance, if a Content Type "Page" has an attribute "Body", then all Content Types
    ///   created underneath "Page" will also have an attribute "Body". As such, the <see cref="SupportedAttributes"/> property
    ///   must crawl through each parent Content Type to collate the list of supported attributes.
    /// </remarks>
    public TopicCollection<AttributeDescriptor> SupportedAttributes {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<TopicCollection<AttributeDescriptor>>() != null);

        if (_supportedAttributes == null) {

          /*--------------------------------------------------------------------------------------------------------------------
          | Create new instance
          \-------------------------------------------------------------------------------------------------------------------*/
          _supportedAttributes = new TopicCollection<AttributeDescriptor>();

          /*--------------------------------------------------------------------------------------------------------------------
          | Validate Attributes collection
          \-------------------------------------------------------------------------------------------------------------------*/
          if (!Children.Contains("Attributes") || Children["Attributes"] == null) {
            throw new Exception(
              "The ContentType '" + Title + "' does not contain a nested topic named 'Attributes' as expected."
            );
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from self
          >---------------------------------------------------------------------------------------------------------------------
          | ### NOTE KLT052015: The (ContentType)Topic.Attributes property is an AttributeValue collection, not an Attribute
          | collection.
          >---------------------------------------------------------------------------------------------------------------------
          | ### NOTE KLT052015: The only place this is really used (and where the strongly-typed Attribute is needed) is in
          | SqlTopicDataProvider.cs (lines 408 - 422), where it is used to add Attributes to the null Attributes collection; the
          | Type property is used for determining whether the Attribute Topic is a Relationships definition or Nested Topic.
          \-------------------------------------------------------------------------------------------------------------------*/
          foreach (AttributeDescriptor attribute in Children["Attributes"].Children) {
            _supportedAttributes.Add(attribute);
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Get values from parent
          \-------------------------------------------------------------------------------------------------------------------*/
          var parent = Parent as ContentTypeDescriptor;
          if (parent?.SupportedAttributes != null) {
            foreach (var attribute in parent.SupportedAttributes) {
              if (!_supportedAttributes.Contains(attribute.Key)) {
                _supportedAttributes.Add(attribute);
              }
            }
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return the dictionary object
        \---------------------------------------------------------------------------------------------------------------------*/
        return _supportedAttributes;

      }
    }

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

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentTypeName), "The attribute contentTypeName must be specified.");
      Topic.ValidateKey(contentTypeName);

      /*----------------------------------------------------------------------------------------------------------------------
      | Determine match
      \---------------------------------------------------------------------------------------------------------------------*/
      var contentType = this;

      while (contentType != null) {
        if (contentType.Key.Equals(contentTypeName, StringComparison.CurrentCultureIgnoreCase)) {
          return true;
        }
        contentType = contentType.Parent as ContentTypeDescriptor;
      }

      return true;

    }

  } // Class

} // Namespace
