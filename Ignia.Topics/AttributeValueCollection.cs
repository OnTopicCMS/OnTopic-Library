/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="AttributeValue"/> objects.
  /// </summary>
  /// <remarks>
  ///   <see cref="AttributeValue"/> objects represent individual instances of attributes associated with particular topics. 
  ///   The <see cref="Topic"/> class tracks these through its <see cref="Topic.Attributes"/> property, which is an instance of 
  ///   the <see cref="AttributeValueCollection"/> class.
  /// </remarks>
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     readonly        Topic   _parent = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValueCollection"/> class.
    /// </summary>
    /// <remarks>
    ///   The <see cref="AttributeValueCollection"/> is intended exclusively for providing access to attributes via the 
    ///   <see cref="Topic.Attributes"/> property. For this reason, the constructor is marked as internal. 
    /// </remarks>
    /// <param name="parentTopic">A reference to the topic that the current attribute collection is bound to.</param>
    internal AttributeValueCollection(Topic parentTopic) : base(StringComparer.OrdinalIgnoreCase) {
      _parent = parentTopic;
    }

    /*==========================================================================================================================
    | METHOD: GET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    public string Get(string name, bool inheritFromParent = false) {
      return Get(name, "", inheritFromParent);
    }

    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value and an optional enabling of 
    ///   inheritance.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    public string Get(string name, string defaultValue, bool inheritFromParent = false) {
      return Get(name, defaultValue, inheritFromParent, true);
    }

    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value, an optional setting for enabling
    ///   of inheritance, and an optional setting for searching through derived topics for values.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="inheritFromDerived">
    ///   Boolean indicator nothing whether to search through any of the topic's <see cref="Topic.DerivedTopic"/> topics in
    ///   order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    public string Get(string name, string defaultValue, bool inheritFromParent, bool inheritFromDerived = true) {
      return Get(name, defaultValue, inheritFromParent, (inheritFromDerived? 5 : 0));
    }

    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary with a specified default value and an optional number of 
    ///   <see cref="Topic.DerivedTopic"/>s through whom to crawl to retrieve an inherited value.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="defaultValue">A string value to which to fall back in the case the value is not found.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <param name="maxHops">The number of recursions to perform when attempting to get the value.</param>
    /// <returns>The string value for the Attribute.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   description="The scope should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !scope.Contains(" ")
    /// </requires>
    /// <requires
    ///   description="The maximum number of hops should be a positive number." exception="T:System.ArgumentException">
    ///   maxHops &gt;= 0
    /// </requires>
    /// <requires
    ///   description="The maximum number of hops should not exceed 100." exception="T:System.ArgumentException">
    ///   maxHops &lt;= 100
    /// </requires>
    private string Get(string name, string defaultValue, bool inheritFromParent, int maxHops) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name));
      Contract.Requires<ArgumentException>(maxHops >= 0, "The maximum number of hops should be a positive number.");
      Contract.Requires<ArgumentException>(maxHops <= 100, "The maximum number of hops should not exceed 100.");
      Topic.ValidateKey(name);

      string value = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from Attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (this.Contains(name)) {
        value = this[name]?.Value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from topic pointer
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && _parent.DerivedTopic != null && maxHops > 0) {
        value = _parent.DerivedTopic.Attributes.Get(name, null, false, maxHops - 1);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && inheritFromParent && _parent != null) {
        value = _parent.Attributes.Get(name, defaultValue, inheritFromParent);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value, if found
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(value)) {
        return value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Finaly, return default
      \-----------------------------------------------------------------------------------------------------------------------*/
      return defaultValue;

    }


    /*==========================================================================================================================
    | METHOD: SET ATTRIBUTE VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new <see cref="AttributeValue"/> object or updates the value of an existing one,
    ///   depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   Minimizes the need for defensive conditions throughout the library.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is 
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being 
    ///   persisted to the data store on <see cref="Topic.Save()"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    public void Set(string key, string value, bool? isDirty = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      Topic.ValidateKey(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Update existing attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (this.Contains(key) && this[key] != null) {
        this[key].Value = value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {
        this.Add(new AttributeValue(key, value));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Optionally override IsDirty, regardless of the default behavior
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isDirty.HasValue) {
        this[key].IsDirty = isDirty.Value;
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the <see cref="KeyedCollection{TKey, TItem}"/> to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="AttributeValue"/> element from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(AttributeValue item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

  } // Class

} // Namespace