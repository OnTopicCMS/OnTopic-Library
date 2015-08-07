namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| PAGE TYPE ELEMENT
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents a page type (default: TopicPage) as developed for the
|               application. Permits the application to define multiple page types.
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
  using System.ComponentModel;
  using System.Configuration;
  using System.Data;
  using System.Globalization;
  using System.Web;
  using System.Web.Security;

/*===========================================================================================================================
| CLASS
\--------------------------------------------------------------------------------------------------------------------------*/
  public class PageTypeElement : ConfigurationElement {

  /*=========================================================================================================================
  | ATTRIBUTE: NAME
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("name", IsRequired=true, IsKey=true) ]
    public string Name {
      get {
        return this["name"] as string;
        }
      }

  /*=========================================================================================================================
  | ATTRIBUTE: TYPE
  \------------------------------------------------------------------------------------------------------------------------*/
  [ TypeConverter(typeof(TypeNameConverter)) ]
  [ ConfigurationProperty("type", IsRequired = false) ]
    public Type Type {
      get {
        return this["type"] as Type;
        }
      }

    }
  }
