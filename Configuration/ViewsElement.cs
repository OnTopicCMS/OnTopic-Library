/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the views settings configuration. Permits an
|               application to define the location of common views files.
|
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class ViewsElement : ConfigurationElement {

  /*============================================================================================================================
  | ATTRIBUTE: PATH
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("path", IsRequired=false) ]
    public string Path {
      get {
        return this["path"] as string;
      }
    }

  } //Class

} //Namespace

