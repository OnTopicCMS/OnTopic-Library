/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
|
| Purpose       Provides a custom configuration element which represents the (default: OnTopic) editor configuration. Permits an
|               application to define whether or not an editor is enabled and, if so, under which conditions it should be used.
|               Child elements allow definition of preview and authorization capabilities. Adapted from the Ignia Localization
|               library; in the future, these libraries may (and should) share custom configuration classes.
|
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Globalization;

namespace Ignia.Topics.Configuration {

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


  } //Class

} //Namespace

