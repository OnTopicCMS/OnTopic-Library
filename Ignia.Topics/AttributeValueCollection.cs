/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
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
    Topic                       _parent                         = null;

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
    public void SetAttributeValue(string key, string value) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value), "value");
      Contract.Requires<ArgumentException>(
        !value.Contains(" "),
        "The key should be an alphanumeric sequence; it should not contain spaces or symbols"
      );
      if (this.Contains(key)) {
        Contract.Assume(this[key] != null, "Assumes the AttributeValue is available, if the collection contains the key.");
        this[key].Value = value;
      }
      else {
        this.Add(new AttributeValue(key, value));
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

      /*------------------------------------------------------------------------------------------------------------------------
      | Define assumptions for external callers
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");

      return item.Key;
    }

  } // Class

} // Namespace