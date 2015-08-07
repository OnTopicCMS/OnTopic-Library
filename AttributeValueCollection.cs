namespace Ignia.Topics {

/*==============================================================================================================================
| CLASS: ATTRIBUTE VALUE COLLECTION
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@Ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a base class by which to associate Attribute subclasses with specific memebers and methods.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.06.14        Katherine Trunkey       Created initial version.
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
| CLASS: ATTRIBUTE VALUE COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class AttributeValueCollection : KeyedCollection<string, AttributeValue> {

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public AttributeValueCollection() : base(StringComparer.OrdinalIgnoreCase) {}

  /*============================================================================================================================
  | SET ATTRIBUTE VALUE METHOD
  >=============================================================================================================================
  | Helper method that either adds a new AttributeValue object or updates the value of an existing one, depending on whether
  | that value already exists. Minimizes the need for defensive conditions throughout the library.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public void SetAttributeValue(string key, string value) {
      if (this.Contains(key)) {
        this[key].Value = value;
        }
      else {
        this.Add(new AttributeValue(key, value));
        }
      }

  /*============================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >=============================================================================================================================
  | Method must be overridden for the KeyedCollection to extract the keys from the items.
  \---------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(AttributeValue item) {
      return item.Key;
      }

    }

  }