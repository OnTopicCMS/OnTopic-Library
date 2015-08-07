namespace Ignia.Topics {

/*==============================================================================================================================
| CLASS: CONTENT TYPE COLLECTION
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@Ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a base class by which to associate ContentType subclasses with specific memebers and methods.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.10.14        Katherine Trunkey       Created initial version.
|               MM.DD.YY        FName LName             Description
\-----------------------------------------------------------------------------------------------------------------------------*/

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used to compile the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Xml;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

/*==============================================================================================================================
| CLASS: CONTENT TYPE COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class ContentTypeCollection : KeyedCollection<string, ContentType> {

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public ContentTypeCollection() : base(StringComparer.OrdinalIgnoreCase) {}

  /*============================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >=============================================================================================================================
  | Method must be overridden for the KeyedCollection to extract the keys from the items.
  \---------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(ContentType item) {
      return item.Key;
      }

    }

  }