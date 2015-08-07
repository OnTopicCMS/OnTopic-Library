namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| VIEWS ELEMENT
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the views settings configuration. Permits an
|               application to define the location of common views files.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               05.27.15        Katherine Trunkey       Created initial version.
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

  }

}
