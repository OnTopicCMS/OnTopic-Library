/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ConfigurationElementCollection responsible for encapsulating a set of
|               PageTypeElement elements. Includes Default property that exposes the default page type (TopicPage).
|
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*===========================================================================================================================
  | CLASS
  \--------------------------------------------------------------------------------------------------------------------------*/
  public class PageTypeElementCollection : ConfigurationElementCollection {

  /*=========================================================================================================================
  | INDEXER
  \------------------------------------------------------------------------------------------------------------------------*/
    public PageTypeElement this[int index] {
      get {
        return base.BaseGet(index) as PageTypeElement;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

  /*=========================================================================================================================
  | ATTRIBUTE: DEFAULT
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("default", DefaultValue="TopicPage", IsRequired = false) ]
    public string Default {
      get {
        return (string)base["default"];
      }
    }

  /*=========================================================================================================================
  | METHOD: CREATE NEW ELEMENT
  \------------------------------------------------------------------------------------------------------------------------*/
    protected override ConfigurationElement CreateNewElement() {
      return new PageTypeElement();
    }

  /*=========================================================================================================================
  | METHOD: GET ELEMENT KEY
  \------------------------------------------------------------------------------------------------------------------------*/
    protected override object GetElementKey(ConfigurationElement element) {
      return ((PageTypeElement)element).Name;
    }

  } //Class

} //Namespace
