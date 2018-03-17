/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Collections {

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
    private                     Topic                           _associatedTopic                = null;
    static                      TypeCollection                  _typeCache                      = new TypeCollection();
    private                     int                             _setCounter                     = 0;

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
    internal AttributeValueCollection(Topic parentTopic) : base(StringComparer.InvariantCultureIgnoreCase) {
      _associatedTopic = parentTopic;
    }

    /*==========================================================================================================================
    | METHOD: IS DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if a given attribute is marked as dirty. Will return false if the attribute key cannot be found.
    /// </summary>
    /// <remarks>
    ///   This method is intended primarily for data storage providers, such as
    ///   <see cref="Ignia.Topics.Repositories.ITopicRepository"/>, which may need to determine if a specific attribute key is
    ///   dirty prior to saving it to the data storage medium. Because isDirty is a state of the current attribute value, it
    ///   does not support inheritFromParent or inheritFromDerived (which otherwise default to true).
    /// </remarks>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <returns>True if if the attribute value is marked as dirty; otherwise false.</returns>
    public bool IsDirty(string name) {
      if (!Contains(name)) {
        return false;
      }
      return this[name].IsDirty;
    }

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/>.</param>
    /// <param name="inheritFromParent">
    ///   Boolean indicator nothing whether to search through the topic's parents in order to get the value.
    /// </param>
    /// <returns>The string value for the Attribute.</returns>
    public string GetValue(string name, bool inheritFromParent = false) => GetValue(name, "", inheritFromParent);

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
    public string GetValue(string name, string defaultValue, bool inheritFromParent = false, bool inheritFromDerived = true) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name));
      return GetValue(name, defaultValue, inheritFromParent, (inheritFromDerived? 5 : 0));
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
    private string GetValue(string name, string defaultValue, bool inheritFromParent, int maxHops) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name));
      Contract.Requires<ArgumentException>(maxHops >= 0, "The maximum number of hops should be a positive number.");
      Contract.Requires<ArgumentException>(maxHops <= 100, "The maximum number of hops should not exceed 100.");
      TopicFactory.ValidateKey(name);

      string value = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from Attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (Contains(name)) {
        value = this[name]?.Value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from topic pointer
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && !name.Equals("TopicId") && _associatedTopic.DerivedTopic != null && maxHops > 0) {
        value = _associatedTopic.DerivedTopic.Attributes.GetValue(name, null, false, maxHops - 1);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && inheritFromParent && _associatedTopic.Parent != null) {
        value = _associatedTopic.Parent.Attributes.GetValue(name, defaultValue, inheritFromParent);
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
    | METHOD: SET VALUE
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
    ///   persisted to the data store on <see cref="Topic.Save(Boolean, Boolean)"/>.
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
    public void SetValue(string key, string value, bool? isDirty = null) => SetValue(key, value, isDirty, true);

    /// <summary>
    ///   Protected helper method that either adds a new <see cref="AttributeValue"/> object or updates the value of an existing
    ///   one, depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   When this overload is called, no attempt will be made to route the call through corresponding properties, if
    ///   available. As such, this is intended specifically to be called by internal properties as a means of avoiding a
    ///   feedback loop.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Topic.Save(Boolean, Boolean)"/>.
    /// </param>
    /// <param name="enforceBusinessLogic">
    ///   Instructs the underlying code to call corresponding properties, if available, to ensure business logic is enforced.
    ///   This should be set to false if setting attributes from internal properties in order to avoid an infinite loop.
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
    internal void SetValue(string key, string value, bool? isDirty, bool enforceBusinessLogic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      TopicFactory.ValidateKey(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish secret handshake for later enforcement of properties
      >-------------------------------------------------------------------------------------------------------------------------
      | ###HACK JJC100617: We want to ensure that any attempt to set attributes that have corresponding (writable) properties
      | use those properties, thus enforcing business logic. In order to ensure this is enforced on all entry points exposed by
      | KeyedCollection, and not just SetValue, the underlying interceptors (e.g., InsertItem, SetItem) will look for the
      | EnforceBusinessLogic property. If it is set to false, they assume the property set the value (e.g., by calling the
      | protected SetValue method with enforceBusinessLogic set to false). Otherwise, the corresponding property will be called.
      | The EnforceBusinessLogic thus avoids a redirect loop in this scenario. This, of course, assumes that properties are
      | correctly written to call the enforceBusinessLogic parameter.
      \-----------------------------------------------------------------------------------------------------------------------*/
      enforceBusinessLogic = (enforceBusinessLogic && _typeCache.HasSettableProperty(_associatedTopic.GetType(), key));

      /*------------------------------------------------------------------------------------------------------------------------
      | Update existing attribute value
      >-----------------------------------------------------------------------------------------------------------------------—
      | Because AttributeValue is immutable, a new instance must be constructed to replace the previous version.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (Contains(key)) {
        var originalAttribute = this[key];
        var markAsDirty = originalAttribute.IsDirty;
        if (isDirty.HasValue) {
          markAsDirty = isDirty.Value;
        }
        else if (!originalAttribute.Value.Equals(value)) {
          markAsDirty = true;
        }
        var newAttribute = new AttributeValue(key, value, markAsDirty, enforceBusinessLogic);
        this[IndexOf(originalAttribute)] = newAttribute;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {
        Add(new AttributeValue(key, value, isDirty ?? true, enforceBusinessLogic));
      }

    }

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to insert a new <see cref="AttributeValue"/> into the collection, to ensure that local
    ///   business logic is enforced.
    /// </summary>
    /// <remarks>
    ///   If a settable property is available corresponding to the <see cref="AttributeValue.Key"/>, the call should be routed
    ///   through that to ensure local business logic is enforced. This is determined by looking for the "__" prefix, which is
    ///   set by the <see cref="SetValue(String, String, Boolean?, Boolean)"/>'s enforceBusinessLogic parameter. To avoid an
    ///   infinite loop, internal setters _must_ call this overload.
    /// </remarks>
    /// <param name="index">The location that the <see cref="AttributeValue"/> should be set.</param>
    /// <param name="item">The <see cref="AttributeValue"/> object which is being inserted.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override void InsertItem(int index, AttributeValue item) {
      if (EnforceBusinessLogic(item, out item)) {
        base.InsertItem(index, item);
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Intercepts all attempts to update a <see cref="AttributeValue"/> in the collection, to ensure that local business
    ///   logic is enforced.
    /// </summary>
    /// <remarks>
    ///   If a settable property is available corresponding to the <see cref="AttributeValue.Key"/>, the call should be routed
    ///   through that to ensure local business logic is enforced. This is determined by looking for the "__" prefix, which is
    ///   set by the <see cref="SetValue(String, String, Boolean?, Boolean)"/>'s enforceBusinessLogic parameter. To avoid an
    ///   infinite loop, internal setters _must_ call this overload.
    /// </remarks>
    /// <param name="index">The location that the <see cref="AttributeValue"/> should be set.</param>
    /// <param name="item">The <see cref="AttributeValue"/> object which is being inserted.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override void SetItem(int index, AttributeValue item) {
      if (EnforceBusinessLogic(item, out item)) {
        base.SetItem(index, item);
      }
    }

    /*==========================================================================================================================
    | METHOD: ENFORCE BUSINESS LOGIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Inspects a provided <see cref="AttributeValue"/> to determine if the value should be routed through local business
    ///   logic.
    /// </summary>
    /// <remarks>
    ///   If a settable property is available corresponding to the <see cref="AttributeValue.Key"/>, the call should be routed
    ///   through that to ensure local business logic is enforced. This is determined by looking for the "__" prefix, which is
    ///   set by the <see cref="SetValue(String, String, Boolean?, Boolean)"/>'s enforceBusinessLogic parameter. To avoid an
    ///   infinite loop, internal setters _must_ call this overload.
    /// </remarks>
    /// <param name="originalAttribute">The <see cref="AttributeValue"/> object which is being inserted.</param>
    /// <param name="settableAttribute">
    ///   Outputs the <see cref="AttributeValue"/> that should be set; will return null if it should not be set.
    /// </param>
    /// <returns>The <see cref="AttributeValue"/> with the business logic applied.</returns>
    private bool EnforceBusinessLogic(AttributeValue originalAttribute, out AttributeValue settableAttribute) {
      settableAttribute = originalAttribute;
      if (!originalAttribute.EnforceBusinessLogic) {
        originalAttribute.EnforceBusinessLogic = true;
        return true;
      }
      else if (_typeCache.HasSettableProperty(_associatedTopic.GetType(), originalAttribute.Key)) {
        _setCounter++;
        if (_setCounter > 3) {
          throw new Exception(
            "An infinite loop has occurred when setting `" + originalAttribute.Key +
            "`; be sure to call `Topic.SetAttributeValue()` when setting attributes from `Topic` properties."
          );
        }
        _typeCache.SetProperty(_associatedTopic, originalAttribute.Key, originalAttribute.Value);
        this[originalAttribute.Key].IsDirty = originalAttribute.IsDirty;
        _setCounter = 0;
        return false;
      }
      return true;
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(AttributeValue item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

  } // Class

} // Namespace