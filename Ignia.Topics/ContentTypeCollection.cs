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
  | CLASS: CONTENT TYPE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the abstract base class for a collection of <see cref="ContentType"/> objects whose string keys are embedded
  ///   in the values.
  /// </summary>
  public class ContentTypeCollection : KeyedCollection<string, ContentType> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContentTypeCollection"/> class.
    /// </summary>
    public ContentTypeCollection() : base(StringComparer.OrdinalIgnoreCase) { }

    /*==========================================================================================================================
    | METHOD: GET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the content type with a given key, and otherwise returns null.
    /// </summary>
    /// <param name="contentType">The content type to return, if available.</param>
    /// <returns>The specified <see cref="ContentType"/>.</returns>
    public ContentType Get(string contentType) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType), "Requires a content type parameter.");
      if (Contains(contentType)) {
        return this[contentType];
      }
      return null;
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the KeyedCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="ContentType"/> element from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(ContentType item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

  } // Class

} // Namespace