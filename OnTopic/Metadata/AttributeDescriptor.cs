/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using OnTopic.Attributes;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Metadata {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE DESCRIPTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing an attribute, including information on how it will be presented and validated in the
  ///   editor.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Every <see cref="Topic"/> in the Topic Library is associated with a <see cref="ContentTypeDescriptor"/>, which
  ///     determines the expected schema of the topic. That schema is described through the
  ///     <see cref="ContentTypeDescriptor.AttributeDescriptors"/> collection, which provides a list of
  ///     <see cref="AttributeDescriptor"/>s associated with that content type. For instance, the commonly-configured "Page"
  ///     Content Type has attributes such as "Keywords", "Body", etc. Each of those are individually represented by instances
  ///     of the <see cref="AttributeDescriptor"/> class.
  ///   </para>
  ///   <para>
  ///     The purpose of the <see cref="AttributeDescriptor"/> class is only to describe the schema of an attribute. For each
  ///     individual <see cref="Topic"/>, the actual values of attributes are stored in <see cref="AttributeRecord"/> objects via
  ///     the <see cref="Topic.Attributes"/> property. By contrast to <see cref="AttributeDescriptor"/>, the
  ///     <see cref="AttributeRecord"/> class is focused exclusively on representing the attribute's value; it is not aware of
  ///     whether that attribute is required, what data type it represents, or how it should be displayed in the editor.
  ///   </para>
  ///   <para>
  ///     This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of
  ///     the CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  ///   </para>
  /// </remarks>
  public class AttributeDescriptor : Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="AttributeDescriptor"/> class with a <see cref="Topic.Key"/>, <see
    ///   cref="Topic.ContentType"/>, and, optionally, <see cref="Topic.Parent"/>, <see cref="Topic.Id"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeRecord"/>s for both <see cref="Topic.Key"/> and <see
    ///   cref="Topic.ContentType"/> will be set to <see cref="TrackedRecord{T}.IsDirty"/>, which is required in order to
    ///   correctly save new topics to the database. When the <paramref name="id"/> parameter is set, however, the <see
    ///   cref="TrackedRecord{T}.IsDirty"/> property is set to <c>false</c> on <see cref="Topic.Key"/> as well as on <see
    ///   cref="Topic.ContentType"/>, since it is assumed these are being set to the same values currently used in the
    ///   persistence store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public AttributeDescriptor(
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
    | PROPERTY: MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an <see cref="Metadata.ModelType"/> value suggesting how this attribute is modeled in the object graph.
    /// </summary>
    /// <remarks>
    ///   Often, there are several editor controls (represented by <see cref="EditorType"/>) which correspond to a single model
    ///   structure in the Topic Library. For instance, both <c>Relationships</c> and <c>TokenizedTopicList</c> are ways of
    ///   exposing the same underlying relationship to the editor in different forms. The <see cref="ModelType"/> property
    ///   reduces these down into a single type based on how they're exposed in the Topic Library, not based on how they're
    ///   exposed in the editor.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public virtual ModelType ModelType { get; protected init; } = ModelType.ScalarValue;

    /*==========================================================================================================================
    | PROPERTY: EDITOR TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the filename reference to the Attribute Type control associate with the Topic object.
    /// </summary>
    /// <remarks>
    ///   The type attribute maps to the name of a control or view component in the editor representing the specific type of
    ///   attribute. For instance, a value of "Boolean" might map to a <c>BooleanViewComponent</c> which displays an attribute's
    ///   value as a standard HTML checkbox. There is no validation of the type at the library level; it is up to the editor to
    ///   provide a match and, if not found, display an error.
    /// </remarks>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires description="Type values should not contain spaces, slashes." exception="T:System.ArgumentException">
    ///   !value.Contains(" ") &amp;&amp; !value.Contains("/")
    /// </requires>
    [ExcludeFromCodeCoverage]
    public string EditorType => GetType().Name.Replace("AttributeDescriptor", "", StringComparison.OrdinalIgnoreCase);

    /*==========================================================================================================================
    | PROPERTY: DISPLAY GROUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the tab in the editor within which the Attribute should be displayed.
    /// </summary>
    /// <remarks>
    ///   When attributes are displayed in the editor, they are grouped by tabs. The tabs are not predetermined, but rather set
    ///   by individual attributes. If five attributes, for instance, have a display group of "Settings", then a tab will be
    ///   rendered called "Settings" and will list those five attributes (assuming none are set to <see
    ///   cref="Topic.IsHidden"/>).
    /// </remarks>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    [ExcludeFromCodeCoverage]
    public string? DisplayGroup {
      get => Attributes.GetValue("DisplayGroup", "");
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value), nameof(value));
        SetAttributeValue("DisplayGroup", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS REQUIRED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether or not the attribute is required.
    /// </summary>
    /// <remarks>
    ///   This is used to establish a required field validator in the editor interface. This should be used by the form
    ///   validation in the editor to ensure the field contains a value.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public bool IsRequired {
      get => Attributes.GetBoolean("IsRequired");
      set => SetAttributeValue("IsRequired", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | PROPERTY: DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the default value that should be used if an explicit value is not defined.
    /// </summary>
    /// <remarks>
    ///   The default value is only used if the value is not otherwise defined. Once a topic has been saved in the editor, this
    ///   value (if not overwritten) is committed to the database and, thus, that version is used in the future. As such, the
    ///   default value only affects the topic when it is first being created via the editor.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public string? DefaultValue {
      get => Attributes.GetValue("DefaultValue", null);
      set => SetAttributeValue("DefaultValue", value);
    }

    /*==========================================================================================================================
    | PROPERTY: IS EXTENDED ATTRIBUTE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether or not the attribute is stored as part of the attributes XML.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Optionally, attributes may be stored as extended attributes, which means they may not be loaded initially, and not
    ///     initially accessible to in-memory queries. On the backend, these may be stored independently, such as in an XML,
    ///     which may be more efficient to store. This is required for larger values, as indexed attributes are limited to 255
    ///     characters.
    ///   </para>
    ///   <para>
    ///     Attributes that are needed to provide indexes, sitemaps, navigation, etc. should be indexed so that they're always
    ///     available in memory without requiring an additional database query. These increase the memory requirements of the
    ///     application, but reduce the number of database round-trips required for topics that are accessed outside of a single
    ///     page. For instance, the title and description of a topic may be cross-referenced on other pages or as part of the
    ///     navigation, and should thus be indexed. Indexed attributes are those not stored as extended attributes.
    ///   </para>
    ///   <para>
    ///     This property and its corresponding attribute was named <c>StoreInBlob</c> in versions of OnTopic prior to 4.0.
    ///   </para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public bool IsExtendedAttribute {
      get => Attributes.GetBoolean("IsExtendedAttribute", Attributes.GetBoolean("StoreInBlob"));
      set => SetAttributeValue("IsExtendedAttribute", value ? "1" : "0");
    }

  } //Class
} //Namespace