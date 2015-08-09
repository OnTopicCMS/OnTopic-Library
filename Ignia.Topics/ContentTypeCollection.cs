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
  ///   Provides a base class by which to associate ContentType subclasses with specific memebers and methods.
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