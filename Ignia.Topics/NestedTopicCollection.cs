/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Editor
|
| Purpose       Provides a base class by which to associate NestedTopic objects with specific memebers and methods.
|
>=============================================================================================================================*/
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Ignia.Topics {

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

  } //Class

} //Namespace