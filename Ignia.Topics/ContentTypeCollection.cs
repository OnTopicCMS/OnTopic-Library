/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;

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
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the KeyedCollection to extract the keys from the items.
    /// </summary>
    protected override string GetKeyForItem(ContentType item) {
      return item.Key;
    }

  } //Class

} //Namespace