/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;

namespace Ignia.Topics {

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
    private                     Dictionary<string, string>      _configuration                  = new Dictionary<string, string>();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeDescriptor"/> class.
    /// </summary>
    public AttributeDescriptor() : base() { }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the filename refence to the Attribute Type control associate with the Topic object.
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
    public string Type {
      get => Attributes.GetValue("Type", "");
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        Topic.ValidateKey(value);
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
    ///   rendered called "Settings" and will list those five attributes (assuming none are set to <see cref="IsHidden"/>)
    ///   according to their <see cref="Topic.SortOrder"/> values.
    /// </remarks>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    [AttributeSetter]
    public string DisplayGroup {
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
    ///     Properties available for configuration are up to the control associated with the <see cref="Type"/>, and the format
    ///     will be  dependent on the framework in which the attribute type control is written. For example, for ASP.NET User
    ///     Controls as well as AngularJS Directives, the format is Property1="Value" Propert2="Value".
    ///   </para>
    /// </remarks>
    public string DefaultConfiguration {
      get => Attributes.GetValue("DefaultConfiguration", "");
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
        Contract.Ensures(Contract.Result<IDictionary<string, string>>() != null);
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
    | PROPERTY: IS HIDDEN?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the attribute should be hidden in the editor.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, all attributes associated with a <see cref="ContentTypeDescriptor"/> are rendered in the editor.
    ///     Optionally, however, attributes can be set to be hidden. This is particularly advantageous when subtyping a Content
    ///     Type Descriptor, as some parent attributes may not be necessary for child content types (e.g., they may be
    ///     implicitly assigned). It c be valuable for attributes that are intended to be managed by the system, and not via the
    ///     editor (e.g., a timestamp or version).
    ///   </para>
    ///   <para>
    ///     The <see cref="IsHidden"/> property does not hide the attribute from the library itself or the views. If the view
    ///     associated with the <see cref="Topic.View"/> property renders the attribute (e.g., via <see
    ///     cref="AttributeValueCollection.GetValue(String, Boolean)"/>) then the attribute will be displayed on the page. The
    ///     <see cref="IsHidden"/> property is used exclusively by the editor.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public new bool IsHidden {
      get => Attributes.GetValue("IsHidden", "0").Equals("1");
      set => SetAttributeValue("IsHidden", value? "1" : "0");
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
      get => Attributes.GetValue("IsRequired", "0").Equals("1");
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
    public string DefaultValue {
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
    ///     application, but reduce the number of database roundtrips required for topics that are accessed outside of a single
    ///     page. For instance, the title and description of a topic may be cross-referenced on other pages or as part of the
    ///     navigation, and should thus be indexed.
    ///   </para>
    /// </remarks>
    [AttributeSetter]
    public bool StoreInBlob {
      get => Attributes.GetValue("StoreInBlob", "1").Equals("1");
      set => SetAttributeValue("StoreInBlob", value ? "1" : "0");
    }

  } // Class

} // Namespace
