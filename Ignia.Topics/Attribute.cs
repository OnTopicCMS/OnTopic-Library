/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC (Jeremy.Caney@Ignia.com)
| Client        Ignia
| Project       Topics Editor
\=============================================================================================================================*/

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a specific implementation of Topic that is optimized for working with Attribute Topics.
  /// </summary>
  /// <remarks>
  ///   This class Inherits from the <see cref="Topic"/> class, but defines properties specifically associated with Attributes.
  /// </remarks>
  public class Attribute : Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="Attribute"/> class, with an overload available accepting the Attribute's
    ///   <see cref="Topic.Key"/> property.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="Attribute"/> Topic.
    /// </param>
    public Attribute() : base() { }

    public Attribute(string key) : base(key) { }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the filename refence to the Attribute Type control associate with the Topic object.
    /// </summary>
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
    ///   Gets or sets the .
    /// </summary>
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
    ///   Gets or sets the .
    /// </summary>
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
    ///   Gets or sets the .
    /// </summary>
    public bool IsRequired {
      get {
        return this.GetAttribute("IsRequired", "0").Equals("1");
      }
      set {
        this.Attributes["IsRequired"].Value = value.ToString();
      }
    }

  /*============================================================================================================================
  | PROPERTY: DEFAULT VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
    public string DefaultValue {
      get {
        return this.GetAttribute("DefaultValue", "");
        }
      set {
        this.Attributes["DefaultValue"].Value = value;
        }
      }

  /*============================================================================================================================
  | PROPERTY: STORE IN BLOB?
  \---------------------------------------------------------------------------------------------------------------------------*/
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
