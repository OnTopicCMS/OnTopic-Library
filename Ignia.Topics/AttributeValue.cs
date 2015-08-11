/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents an instance of an Attribute value.
  /// </summary>
  /// <remarks>
  ///   Provides values and metadata specific to individual attribute value instances, such as state(IsDirty = has changed) and 
  ///   last modified date.State(IsDirty) is evaluated as part of the setter for Value.
  /// </remarks>
  public class AttributeValue {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     string  _key            = null;
    private     string  _value          = null;
    private     bool    _isDirty        = false;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Initializes a new instance of the <see cref="AttributeValue"/> class.
    /// </summary>
    /// <remarks>
    ///   Optional overloads allow object to be constructed based on specified key/value pairs or for the
    ///   IsDirty (has been changed) property to be set.
    /// </remarks>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   The boolean indicator noting whether the <see cref="AttributeValue"/> collection item has been changed.
    /// </param>
    public AttributeValue() { }

    public AttributeValue(string key, string value) {
      this.Key          = key;
      this.Value        = value;
    }

    public AttributeValue(string key, string value, bool isDirty) {
      this.Key          = key;
      this.Value        = value;
      this.IsDirty      = isDirty;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  String containing the key of the attribute.
    /// </summary>
    public string Key {
      get {
        return _key;
      }
      set {
        _key = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  String containing the current value of the attribute. Sets IsDirty based on whether the value has changed.
    /// </summary>
    public string Value {
      get {
        return _value;
      }
      set {
        _isDirty        = !value.Equals(_value) || _isDirty;
        _value          = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Boolean setting which is set automatically when an attribute's Value is set to a new value.
    /// </summary>
    public bool IsDirty {
      get {
        return _isDirty;
      }
      set {
        _isDirty        = value;
      }
    }

    /*=========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///  Reference to last DateTime the <see cref="AttributeValue"/> instance was updated.
    /// </summary>
    public readonly DateTime LastModified       = DateTime.Now;

  } //Class

} //Namepace
