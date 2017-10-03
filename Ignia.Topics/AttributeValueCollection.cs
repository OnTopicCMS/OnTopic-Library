/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple and safe interface for accessing the underlying <see cref="AttributeValueCollection"/>, without
  ///   exposing potentially damaging (and otherwise unnecessary) methods.
  /// </summary>
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="AttributeValueCollection"/>.
    /// </summary>
    public AttributeValueCollection() {
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

  } //Class

} //Namespace