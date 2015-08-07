namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| VERSIONING ELEMENT
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the data versioning configuration. Permits an
|               application to define whether or not draft Topics data should be displayed to the user, and if so under which
|               specific location. Adapted from the Ignia Localization library; in the future, these libraries may (and should)
|               share custom configuration classes.
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
  public class VersioningElement : ConfigurationElement {

  /*=========================================================================================================================
  | ELEMENT: DRAFT MODE
  \------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("draftMode") ]
    public SourceElement DraftMode {
      get {
        return this["draftMode"] as SourceElement;
        }
      }

    }

  }
