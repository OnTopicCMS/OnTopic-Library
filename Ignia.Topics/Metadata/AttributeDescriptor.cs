/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using Ignia.Topics.Internal.Diagnostics;
using System.Linq;

namespace Ignia.Topics.Metadata {

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
  ///     individual <see cref="Topic"/>, the actual values of attributes are stored in <see cref="AttributeValue"/> objects via
  ///     the <see cref="Topic.Attributes"/> property. By contrast to <see cref="AttributeDescriptor"/>, the
  ///     <see cref="AttributeValue"/> class is focused exclusively on representing the attribute's value; it is not aware of
  ///     whether that attribute is required, what data type it represents, or how it should be displayed in the editor.
  ///   </para>
  ///   <para>
  ///     This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of
  ///     the CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  ///   </para>
  /// </remarks>
  public class AttributeDescriptor : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     Dictionary<string, string>      _configuration;
    private                     ModelType?                      _modelType                      = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="AttributeDescriptor"/> class with a <see cref="Topic.Key"/>, <see
    ///   cref="Topic.ContentType"/>, and, optionally, <see cref="Topic.Parent"/>, <see cref="Topic.Id"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Topic.Key"/> and <see
    ///   cref="Topic.ContentType"/> will be set to <see cref="AttributeValue.IsDirty"/>, which is required in order to
    ///   correctly save new topics to the database. When the <paramref name="id"/> parameter is set, however, the <see
    ///   cref="AttributeValue.IsDirty"/> property is set to <c>false</c> on <see cref="Topic.Key"/> as well as on <see
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
    public AttributeDescriptor(
      string key,
      string contentType,
      Topic parent,
      int id = -1
    ) : base(
      key,
      contentType,
      parent,
      id
    ) {
      _configuration = new Dictionary<string, string>();
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
    public ModelType ModelType {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Return cached value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_modelType.HasValue) {
          return _modelType.Value;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Determine value
        \---------------------------------------------------------------------------------------------------------------------*/
        var editorType = EditorType;

        if (editorType.LastIndexOf(".", StringComparison.InvariantCulture) >= 0) {
          editorType = editorType.Substring(0, EditorType.LastIndexOf(".", StringComparison.InvariantCulture));
        }

        if (new [] { "Relationships", "TokenizedTopicList"}.Contains(editorType)) {
          _modelType = ModelType.Relationship;
        }
        else if (
          new[] { "TopicLookup", "TopicPointer" }.Contains(editorType) ||
          Key.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase)
        ) {
          _modelType = ModelType.Reference;
        }
        else if (new[] { "TopicList" }.Contains(editorType)) {
          _modelType = ModelType.NestedTopic;
        }
        else {
          _modelType = ModelType.ScalarValue;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return value
        \---------------------------------------------------------------------------------------------------------------------*/
        return _modelType.Value;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: EDITOR TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the filename reference to the Attribute Type control associate with the Topic object.
    /// </summary>
    /// <remarks>
    ///   The type attribute maps to the name of a control, directive, or partial view in the editor representing the specific
    ///   type of attribute. For instance, a value of "Checkbox" might map to a file "Checkbox.ascx" which displays an
    ///   attribute's value as a standard HTML checkbox. There is no validation of the type at the library level; it is up to
    ///   the editor to provide a match and, if not found, display an error.
    /// </remarks>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires description="Type values should not contain spaces, slashes." exception="T:System.ArgumentException">
    ///   !value.Contains(" ") &amp;&amp; !value.Contains("/")
    /// </requires>
    [AttributeSetter]
    public string? EditorType {
      get => Attributes.GetValue("Type", "");
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        TopicFactory.ValidateKey(value);
        SetAttributeValue("Type", value);
      }
    }

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
    [AttributeSetter]
    public string? DisplayGroup {
      get => Attributes.GetValue("DisplayGroup", "");
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        SetAttributeValue("DisplayGroup", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: DEFAULT CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the default configuration.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When an attribute is bound to an attribute type control in the editor, the default configuration is injected into
    ///     the control's configuration. This allows attribute type-specific properties to be set on a per-attribute basis.
    ///   </para>
    ///   <para>
    ///     Properties available for configuration are up to the control associated with the <see cref="EditorType"/>, and the format
    ///     will be  dependent on the framework in which the attribute type control is written. For example, for ASP.NET User
    ///     Controls as well as AngularJS Directives, the format is Property1="Value" Propert2="Value".
    ///   </para>
    /// </remarks>
    public string DefaultConfiguration {
      get => Attributes.GetValue("DefaultConfiguration", "")?? "";
      set {
        SetAttributeValue("DefaultConfiguration", value);
        _configuration.Clear();
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a dictionary representing a parsed collection of key/value pairs from the
    ///   <see cref="DefaultConfiguration"/>.
    /// </summary>
    public IDictionary<string, string> Configuration {
      get {
        if (_configuration.Count.Equals(0) && DefaultConfiguration.Length > 0) {
          _configuration = DefaultConfiguration
            .Split(' ')
            .Select(value => value.Split('='))
            .ToDictionary(
              pair => pair[0],
              pair => pair.Count().Equals(2)? pair[1]?.Replace("\"", "") : null
            );
        }
        return _configuration;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: GET CONFIGURATION VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a configuration value from the <see cref="Configuration"/> dictionary; if the value doesn't exist, then
    ///   optionally returns a default value.
    /// </summary>
    public string? GetConfigurationValue(string key, string? defaultValue = null) {
      Contract.Requires(!String.IsNullOrWhiteSpace(key));
      if (Configuration.ContainsKey(key) && Configuration[key] != null) {
        return Configuration[key];
      }
      return defaultValue;
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
    [AttributeSetter]
    public bool IsRequired {
      get => Attributes.GetBoolean("IsRequired", false);
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
    public string? DefaultValue {
      get => Attributes.GetValue("DefaultValue", "");
      set => SetAttributeValue("DefaultValue", value);
    }

    /*==========================================================================================================================
    | PROPERTY: STORE IN BLOB?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether or not the attribute is stored in the XML blob.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, all attributes are stored in the blob, which means they may not be loaded initially, and not accessible
    ///     to in-memory queries. This is more efficient to store, and is required for larger values.
    ///   </para>
    ///   <para>
    ///     Attributes that are needed to provide indexes, sitemaps, navigation, etc. should be indexed so that they're always
    ///     available in memory without requiring an additional database query. These increase the memory requirements of the
    ///     application, but reduce the number of database round-trips required for topics that are accessed outside of a single
    ///     page. For instance, the title and description of a topic may be cross-referenced on other pages or as part of the
    ///     navigation, and should thus be indexed.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public bool StoreInBlob {
      get => Attributes.GetBoolean("StoreInBlob", true);
      set => SetAttributeValue("StoreInBlob", value ? "1" : "0");
    }

  } // Class

} // Namespace
