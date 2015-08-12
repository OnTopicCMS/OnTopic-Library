/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC 
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed implementation of Topic that maps to Attribute Content Types.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The Attribute Content Type represents an individual attribute applied to another Content Type. For instance, the 
  ///     commonly-configured "Page" Content Type has attributes such as "Keywords", "Body", etc. Each of those individually 
  ///     represent instances of the Attribute Content Type.
  ///   </para>
  ///   <para>
  ///     This class is primarily used by the Topic Editor interface to determine how attributes are displayed as part of 
  ///     the CMS; except in very specific scenarios, it is not typically used elsewhere in the Topic Library itself.
  ///   </para>
  /// </remarks>
  public class Attribute : Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="Attribute"/> class.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows the object to be constructed based on the Attribute's <see cref="Topic.Key"/> property.
    /// </remarks>
    public Attribute() : base() { }

    /// <param name="key">
    ///   The string identifier for the <see cref="Attribute"/> Topic.
    /// </param>
    public Attribute(string key) : base(key) { }

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
    public string Type {
      get {
        return this.GetAttribute("Type", "");
      }
      set {
        this.Attributes["Type"].Value = value;
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
    public string DisplayGroup {
      get {
        return this.GetAttribute("DisplayGroup", "");
      }
      set {
        this.Attributes["DisplayGroup"].Value = value;
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
      get {
        return this.GetAttribute("DefaultConfiguration", "");
      }
      set {
        this.Attributes["DefaultConfiguration"].Value = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS HIDDEN?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the attribute should be hidden in the editor.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     By default, all attributes associated with a <see cref="ContentType"/> are rendered in the editor. Optionally, 
    ///     however, attributes can be set to be hidden. This is particularly advantageous when subtyping a Content Type, as
    ///     some parent attributes may not be necessary for child content types (e.g., they may be implicitly assigned). It can
    ///     also be valuable for attributes that are intended to be managed by the system, and not via the editor (e.g., a 
    ///     timestamp or version).
    ///   </para>
    ///   <para>
    ///     The <see cref="IsHidden"/> property does not hide the attribute from the library itself or the views. If the view 
    ///     associated with the <see cref="Topic.View"/> property renders the attribute (e.g., via <see 
    ///     cref="Topic.GetAttribute(string, bool)"/>) then the attribute will be displayed on the page. The <see 
    ///     cref="IsHidden"/> property is used exclusively by the editor.
    ///   </para>
    /// </remarks>
    public bool IsHidden {
      get {
        return this.GetAttribute("IsHidden", "0").Equals("1");
      }
      set {
        this.Attributes["IsHidden"].Value = value.ToString();
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS REQUIRED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether or not the attribute is required. 
    /// </summary>
    /// <remarks>
    ///   This is used to establish a required field validator in the editor interface, and maps to the 
    ///   <see cref="Ignia.Topics.Editor.IEditControl.Required"/> property.
    /// </remarks>
    public bool IsRequired {
      get {
        return this.GetAttribute("IsRequired", "0").Equals("1");
      }
      set {
        this.Attributes["IsRequired"].Value = value.ToString();
      }
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
      get {
        return this.GetAttribute("DefaultValue", "");
        }
      set {
        this.Attributes["DefaultValue"].Value = value;
        }
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
    public bool StoreInBlob {
      get {
        return this.GetAttribute("StoreInBlob", "1").Equals("1");
      }
      set {
        this.Attributes["StoreInBlob"].Value = value.ToString();
      }
    }

  } //Class

} //Namespace
