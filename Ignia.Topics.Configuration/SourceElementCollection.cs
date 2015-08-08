/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ConfigurationElementCollection responsible for encapsulating a set of
|               SourceElement elements. Adapted DIRECTLY from the Ignia Localization library; in the future, these libraries may
|               (and should) share custom configuration classes.
|
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*===========================================================================================================================
  | CLASS
  \--------------------------------------------------------------------------------------------------------------------------*/
  public class SourceElementCollection : ConfigurationElementCollection {

  /*=========================================================================================================================
  | INDEXER
  \------------------------------------------------------------------------------------------------------------------------*/
    public SourceElement this[int index] {
      get {
        return base.BaseGet(index) as SourceElement;
      }
      set {
        if (base.BaseGet(index) != null) {
          base.BaseRemoveAt(index);
        }
        this.BaseAdd(index, value);
      }
    }

  /*=========================================================================================================================
  | METHOD: CREATE NEW ELEMENT
  \------------------------------------------------------------------------------------------------------------------------*/
    protected override ConfigurationElement CreateNewElement() {
      return new SourceElement();
    }

  /*=========================================================================================================================
  | METHOD: GET ELEMENT KEY
  \------------------------------------------------------------------------------------------------------------------------*/
    protected override object GetElementKey(ConfigurationElement element) {
      return ((SourceElement)element).Source;
    }

  } //Class

} //Namespace
