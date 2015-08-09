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
  ///   Provides a base class by which to associate Attribute subclasses with specific memebers and methods.
  /// </summary>
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Allows a new object to be instantiated.
    /// </summary>
    public AttributeValueCollection() : base(StringComparer.OrdinalIgnoreCase) { }

    /*==========================================================================================================================
    | METHOD: SET ATTRIBUTE VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper method that either adds a new AttributeValue object or updates the value of an existing one, depending on 
    ///   whether that value already exists.
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
    ///   Method must be overridden for the KeyedCollection to extract the keys from the items.
    /// </summary>
    protected override string GetKeyForItem(AttributeValue item) {
      return item.Key;
    }

  } //Class

} //Namespace