/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the abstract base class for a collection of <see cref="AttributeValue"/> objects whose string keys are embedded
  ///   in the values.
  /// </summary>
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValueCollection"/> class.
    /// </summary>
    public AttributeValueCollection() : base(StringComparer.OrdinalIgnoreCase) { }

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
    public void SetAttributeValue(string key, string value) {
      if (this.Contains(key)) {
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
      return item.Key;
    }

  } //Class

} //Namespace