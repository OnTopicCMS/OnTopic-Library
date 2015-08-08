/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the data versioning configuration. Permits an
|               application to define whether or not draft Topics data should be displayed to the user, and if so under which
|               specific location. Adapted from the Ignia Localization library; in the future, these libraries may (and should)
|               share custom configuration classes.
|
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Configuration {

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

  } //Class

} //Namespace

