/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents the value of a particular attribute on a <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Provides values and metadata specific to individual attribute values, such as state (e.g., the <see cref="IsDirty"/> 
  ///     property signifies whether the attribute value has changed) and its <see cref="LastModified"/> date. State (IsDirty) 
  ///     is evaluated as part of the setter for <see cref="Value"/>; i.e., when the value changes, IsDirty is automatically set 
  ///     to <c>true</c>, if it wasn't previously.
  ///   </para>  
  ///   <para>
  ///     Typically, the <see cref="AttributeValue"/> will be exposed as part of a <see cref="AttributeValueCollection"/> via 
  ///     the <see cref="Topic.Attributes"/> collection. 
  ///   </para>
  ///   <para>
  ///     Be aware that while <see cref="AttributeValue"/> represents the value of a specific attribute, the metadata for 
  ///     describing the purpose, constraints, and usage of that particular attribute is described by the <see 
  ///     cref="Attribute"/> class. 
  ///   </para>
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
    ///   Initializes a new instance of the <see cref="AttributeValue"/> class.
    /// </summary>
    public AttributeValue() { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValue"/> class, using the specified key/value pair.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the key/value pair." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    public AttributeValue(string key, string value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(key));
      Topic.ValidateKey(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set local values
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Key          = key;
      this.Value        = value;

    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValue"/> class, using the specified key/value pair as well as
    ///   the manual setting for whether the item has changed.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="value">
    ///   The string value text for the <see cref="AttributeValue"/> collection item key/value pair.
    /// </param>
    /// <param name="isDirty">
    ///   The boolean indicator noting whether the <see cref="AttributeValue"/> collection item has been changed.
    /// </param>
    public AttributeValue(string key, string value, bool isDirty) : this(key, value) {
      this.IsDirty      = isDirty;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the key of the attribute.
    /// </summary>
    /// <requires description="The value from the getter must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public string Key {
      get {
        return _key;
      }
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        Topic.ValidateKey(value);
        _key = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the current value of the attribute. Automatically sets <see cref="IsDirty"/> based on whether the value
    ///   has changed.
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
    ///   Boolean setting which is set automatically when an attribute's <see cref="Value"/> is set to a new value.
    /// </summary>
    /// <remarks>
    ///   The IsDirty property is used by the <see cref="Topics.Providers.TopicDataProviderBase"/> to determine whether or not 
    ///   the value has been persisted to the database. If it is set to true, the attribute's value is sent to the database 
    ///   when <see cref="Topics.Providers.TopicDataProviderBase.Save(Topic, bool, bool)"/> is called. Otherwise, it is ignored, 
    ///   thus preventing the need to update attributes (or create new versions of attributes) whose values haven't changed.
    /// </remarks>
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
    ///   Read-only reference to the last DateTime the <see cref="AttributeValue"/> instance was updated.
    /// </summary>
    public readonly DateTime LastModified = DateTime.Now;

  } // Class

} // Namespace
