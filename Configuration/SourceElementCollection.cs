namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| SOURCE ELEMENT COLLECTION
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom implementation of a ConfigurationElementCollection responsible for encapsulating a set of
|               SourceElement elements. Adapted DIRECTLY from the Ignia Localization library; in the future, these libraries may
|               (and should) share custom configuration classes.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.14.14        Katherine Trunkey       Created initial version.
\-----------------------------------------------------------------------------------------------------------------------------*/

/*==============================================================================================================================
| DEFINE ASSEMBLY ATTRIBUTES
>===============================================================================================================================
| Declare and define attributes used in the compiling of the finished assembly.
\-----------------------------------------------------------------------------------------------------------------------------*/
  using System;
  using System.Data;
  using System.Configuration;
  using System.Web;
  using System.Web.Security;
  using System.Globalization;

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

    }

  }