namespace Ignia.Topics.Configuration {

/*==============================================================================================================================
| SOURCE ELEMENT
|
| Author        Katherine Trunkey, Ignia LLC (katherine.trunkey@ignia.com)
| Client        Ignia
| Project       Topics Library
|
| Purpose       Provides configuration element references which represent a custom configuration source (e.g., for versioning,
|               the OnTopic editor, etc.). Allows the application to retrieve configuration data from multiple sources. Adapted
|               DIRECTLY from the Ignia Localization library; in the future, these libraries may (and should) share custom
|               configuration classes.
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

/*==============================================================================================================================
| CLASS
\-----------------------------------------------------------------------------------------------------------------------------*/
  public class SourceElement : ConfigurationElement {

  /*============================================================================================================================
  | ATTRIBUTE: SOURCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("source", DefaultValue="QueryString", IsRequired=true, IsKey=true) ]
    public string Source {
      get {
        return this["source"] as string;
        }
      }

  /*============================================================================================================================
  | ATTRIBUTE: ENABLED
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("enabled", DefaultValue="True", IsRequired=false) ]
    public bool Enabled {
      get {
        return Convert.ToBoolean(this["enabled"], CultureInfo.InvariantCulture);
        }
      }

  /*============================================================================================================================
  | ATTRIBUTE: LOCATION
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("location", IsRequired=false) ]
    public string Location {
      get {
        return this["location"] as string;
        }
      }

  /*============================================================================================================================
  | ATTRIBUTE: TRUSTED
  \---------------------------------------------------------------------------------------------------------------------------*/
  [ ConfigurationProperty("trusted", DefaultValue="False", IsRequired=false) ]
    public bool Trusted {
      get {
        return Convert.ToBoolean(this["trusted"], CultureInfo.InvariantCulture);
        }
      }

  /*============================================================================================================================
  | METHOD: GET ELEMENT
  >=============================================================================================================================
  | Looks up a source element given the parent element or collection and expected name.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static SourceElement GetElement(ConfigurationElement parent, string key) {
      if (parent == null) return null;
      return (SourceElement)parent.ElementInformation.Properties[key].Value;
      }

    public static SourceElement GetElement(ConfigurationElementCollection parent, string key) {
      if (parent == null) return null;
      foreach (SourceElement source in parent) {
        if (source.Source.Equals(key)) {
          return source;
          }
        }
      return null;
      }

  /*============================================================================================================================
  | METHOD: GET VALUE
  >=============================================================================================================================
  | Looks up a source element at a given location, identifies the source value and, assuming it's enabled, returns the
  | target value.  Otherwise, returns null.
  \---------------------------------------------------------------------------------------------------------------------------*/
    public static string GetValue(ConfigurationElement parent, string key) {
      return GetValue(GetElement(parent, key));
      }

    public static string GetValue(ConfigurationElementCollection parent, string key) {
      return GetValue(GetElement(parent, key));
      }

    public static string GetValue(SourceElement element) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETURN NULL IF DISABLED OR MISSING
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (element == null || !element.Enabled) return null;

    /*--------------------------------------------------------------------------------------------------------------------------
    | PULL VALUE FROM SUPPORTED SOURCE
    \-------------------------------------------------------------------------------------------------------------------------*/
      string value = null;

      switch (element.Source.ToUpperInvariant()) {
        case("QUERYSTRING") :
          value         = HttpContext.Current.Request.QueryString[element.Location];
          break;
        case("FORM") :
          value         = HttpContext.Current.Request.Form[element.Location];
          break;
        case("APPLICATION") :
          value         = (string)HttpContext.Current.Application[element.Location];
          break;
        case("SESSION") :
          value         = (string)HttpContext.Current.Session[element.Location];
          break;
        case("COOKIE") :
          if (HttpContext.Current.Request.Cookies[element.Location] != null) {
            value       = HttpContext.Current.Request.Cookies[element.Location].Value;
            }
          break;
        case("ROLE") :
          value         = Roles.IsUserInRole(element.Location).ToString();
          break;
        case("HOSTNAME") :
          value         = element.Location;
          break;
        case("URL") :
          value         = HttpContext.Current.Request.Path.Split('/')[Int32.Parse(element.Location, CultureInfo.InvariantCulture)];
          break;
        default :
          throw new ConfigurationErrorsException("The source '" + element.Source + "' in the web.config is invalid.");
        }

      return value;

      }

  /*=========================================================================================================================
  | METHOD: IS ENABLED
  >==========================================================================================================================
  | Looks up a source element at a given location, identifies the source value and returns a boolean value representing
  | whether or not the source is available, enabled or set to true.
  \------------------------------------------------------------------------------------------------------------------------*/
    public static bool IsEnabled(ConfigurationElement parent, string key) {
      return IsEnabled(parent, key, true);
      }

    public static bool IsEnabled(ConfigurationElement parent, string key, bool evaluateValue) {
      return IsEnabled(GetElement(parent, key), evaluateValue);
      }

    public static bool IsEnabled(ConfigurationElementCollection parent, string key) {
      return IsEnabled(parent, key, false);
      }

    public static bool IsEnabled(ConfigurationElementCollection parent, string key, bool evaluateValue) {
      return IsEnabled(GetElement(parent, key), evaluateValue);
      }

    public static bool IsEnabled(SourceElement element, bool evaluateValue) {
      if (element == null) {
        return false;
        }
      if (!element.Enabled) {
        return false;
        }
      else if (!evaluateValue || element.Location == null) {
        return true;
        }

      string value = GetValue(element);

      if (String.IsNullOrEmpty(value)) return false;

      return Convert.ToBoolean(value, CultureInfo.InvariantCulture);

      }

  /*=========================================================================================================================
  | METHOD: IS TRUSTED
  >==========================================================================================================================
  | Looks up a source element at a given location, identifies the source value and returns a boolean value representing
  | whether or not the source is available, enabled or set to true.
  \------------------------------------------------------------------------------------------------------------------------*/
    public static bool IsTrusted(ConfigurationElement parent, string key) {
      return IsTrusted(GetElement(parent, key));
      }

    public static bool IsTrusted(ConfigurationElementCollection parent, string key) {
      return IsTrusted(GetElement(parent, key));
      }

    public static bool IsTrusted(SourceElement element) {
      return (element == null)? false : element.Trusted;
      }

    }

  }
