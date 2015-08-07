namespace Ignia.Topics {

/*==============================================================================================================================
| CLASS: NESTED TOPIC COLLECTION
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@Ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a base class by which to associate NestedTopic objects with specific memebers and methods.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.15.14        Katherine Trunkey       Created initial version.
|               MM.DD.YY        FName LName             Description
\-----------------------------------------------------------------------------------------------------------------------------*/

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used to compile the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Linq;
  using System.Xml;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;

/*==============================================================================================================================
| CLASS
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class NestedTopicCollection : KeyedCollection<string, NestedTopic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public NestedTopicCollection() : base(StringComparer.OrdinalIgnoreCase) { }

    public NestedTopicCollection(Topic source) : base(StringComparer.OrdinalIgnoreCase) {
      foreach (Topic topic in source.Where(t => t.ContentType.Key == "List")) {
        string          key             = topic.Key;
        NestedTopic     nestedTopic     = new NestedTopic(key);
        this.Add(nestedTopic);
        }
      }


  /*============================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >=============================================================================================================================
  | Method must be overridden for the KeyedCollection to extract the keys from the items.
  \---------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(NestedTopic item) {
      string            key             = item.Key;
      return key;
      }

    }

  }