/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a base class by which to associate ChildTopic objects with specific memebers and methods.
|
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace Ignia.Topics {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class ChildTopicCollection : KeyedCollection<string, ChildTopic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  >=============================================================================================================================
  | Allows a new object to be instantiated.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public ChildTopicCollection() : base(StringComparer.OrdinalIgnoreCase) { }

    public ChildTopicCollection(Topic source) : base(StringComparer.OrdinalIgnoreCase) {
      foreach (Topic topic in source.Where(t => t.ContentType.Key != "List")) {
        ChildTopic      childTopic      = new ChildTopic(topic.Key);
        this.Add(childTopic);
      }
    }


  /*============================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >=============================================================================================================================
  | Method must be overridden for the KeyedCollection to extract the keys from the items.
  \---------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(ChildTopic item) {
      return item.Key;
    }

  } //Class

} //Namespace