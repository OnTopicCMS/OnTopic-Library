/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Ignia.Topics.Attributes;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Metadata;

namespace Ignia.Topics.Web.Editor {

  /*============================================================================================================================
  | CLASS: CONFIGURABLE ATTRIBUTE DESCRIPTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents metadata for describing an attribute, including information on how it will be presented and validated in the
  ///   editor.
  /// </summary>
  /// <remarks>
  ///   Extends the out-of-the-box <see cref="AttributeDescriptor"/> with a version that can be extended via a <see
  ///   cref="DefaultConfiguration"/>. This is used by the legacy ASP.NET WebForms version of the OnTopic Editor. Newer releases
  ///   instead use the <see cref="AttributeTypeDescriptor"/> content types, which provide configurable values via attributes.
  /// </remarks>
  public class ConfigurableAttributeDescriptor : AttributeDescriptor {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     Dictionary<string, string>      _configuration;
    private                     ModelType?                      _modelType                      = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="ConfigurableAttributeDescriptor"/> class with a <see cref="Topic.Key"/>,
    ///   <see cref="Topic.ContentType"/>, and, optionally, <see cref="Topic.Parent"/>, <see cref="Topic.Id"/>.
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
    public ConfigurableAttributeDescriptor(
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
    /// <inheritdoc />
    public override ModelType ModelType {
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

        Contract.Assume(
          editorType,
          $"The 'EditorType' property is not set for the '{Key}' attribute, and thus a 'ModelType' cannot be determined."
        );

        if (editorType.LastIndexOf(".", StringComparison.InvariantCulture) >= 0) {
          editorType = editorType.Substring(0, editorType.LastIndexOf(".", StringComparison.InvariantCulture));
        }

        if (new[] { "Relationships", "TokenizedTopicList" }.Contains(editorType)) {
          _modelType = ModelType.Relationship;
        }
        else if (
          new[] { "TopicPointer" }.Contains(editorType) ||
          Key.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase) &&
          !Key.Equals("Id", StringComparison.InvariantCultureIgnoreCase)
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
    /// <inheritdoc />
    public override string EditorType => Attributes.GetValue("Type", "");

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
    ///     Properties available for configuration are up to the control associated with the <see cref="EditorType"/>, and the
    ///     format will be  dependent on the framework in which the attribute type control is written. For example, for ASP.NET
    ///     User Controls as well as AngularJS Directives, the format is Property1="Value" Propert2="Value".
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
    public string GetConfigurationValue(string key, string defaultValue = null) {
      Contract.Requires(!String.IsNullOrWhiteSpace(key));
      if (Configuration.ContainsKey(key) && Configuration[key] != null) {
        return Configuration[key];
      }
      return defaultValue;
    }

    /*==========================================================================================================================
    | PROPERTY: STORE IN BLOB?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether or not the attribute is stored as part of the attributes XML.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, all attributes are stored in the blob, which means they may not be loaded initially, and not accessible
    ///     to in-memory queries. This is more efficient to store, and is required for larger values.
    ///   <para>
    ///     Attributes that are needed to provide indexes, sitemaps, navigation, etc. should be indexed so that they're always
    ///     available in memory without requiring an additional database query. These increase the memory requirements of the
    ///     application, but reduce the number of database round-trips required for topics that are accessed outside of a single
    ///     page. For instance, the title and description of a topic may be cross-referenced on other pages or as part of the
    ///     navigation, and should thus be indexed. Indexed attributes are those not stored as a blob.
    ///   </para>
    ///   <para>
    ///     This property and its corresponding attribute was named <c>StoreInBlob</c> in versions of OnTopic prior to 4.0.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public virtual bool StoreInBlob {
      get => Attributes.GetBoolean("StoreInBlob", true);
      set => SetAttributeValue("StoreInBlob", value ? "1" : "0");
    }

  } //Class
} //Namespace