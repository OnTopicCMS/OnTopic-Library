namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| EDITOR ELEMENT
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the (default: OnTopic) editor configuration. Permits an
|               application to define whether or not an editor is enabled and, if so, under which conditions it should be used.
|               Child elements allow definition of preview and authorization capabilities. Adapted from the Ignia Localization
|               library; in the future, these libraries may (and should) share custom configuration classes.
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
  using System.Configuration;
  using System.Data;
  using System.Globalization;
  using System.Web;
  using System.Web.Security;

/*===========================================================================================================================
| CLASS
\--------------------------------------------------------------------------------------------------------------------------*/
  public class EditorElement : ConfigurationElement {

  /*=========================================================================================================================
  | ATTRIBUTE: ENABLED
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("enabled", DefaultValue="True", IsRequired=false) ]
    public bool Enabled {
      get {
        return Convert.ToBoolean(this["enabled"], CultureInfo.InvariantCulture);
        }
      }

  /*=========================================================================================================================
  | ATTRIBUTE: LOCATION
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("location", IsRequired=false) ]
    public string Location {
      get {
        return this["source"] as string;
        }
      }

  /*=========================================================================================================================
  | ELEMENT: ADMIN
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("admin") ]
    public SourceElement Admin {
      get {
        return this["admin"] as SourceElement;
        }
      }


    }

  }
