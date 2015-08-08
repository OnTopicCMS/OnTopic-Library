/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a base class by which to associate ContentType subclasses with specific memebers and methods.
|
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;

namespace Ignia.Topics {

  /*==============================================================================================================================
  | CLASS: CONTENT TYPE COLLECTION
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class ContentTypeCollection : KeyedCollection<string, ContentType> {

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public ContentTypeCollection() : base(StringComparer.OrdinalIgnoreCase) { }

  /*============================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >=============================================================================================================================
  | Method must be overridden for the KeyedCollection to extract the keys from the items.
  \---------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(ContentType item) {
      return item.Key;
    }

  } //Class

} //Namespace