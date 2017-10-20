/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Web;
using System.Web.Security;

namespace Ignia.Topics.Web.Configuration {

  /*============================================================================================================================
  | CLASS: SOURCE ELEMENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom <see cref="ConfigurationElement"/> implementation which represents a custom configuration source
  ///   (e.g., for versioning, the OnTopic editor, etc.). Allows the application to retrieve configuration data from multiple
  ///   sources.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Adapted DIRECTLY from the Ignia Localization library; in the future, these libraries may (and should) share custom
  ///     configuration classes.
  ///   </para>
  /// </remarks>
  public class SourceElement : ConfigurationElement {

    /*==========================================================================================================================
    | ATTRIBUTE: SOURCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the source for the configuration setting.
    /// </summary>
    [ConfigurationProperty("source", DefaultValue="QueryString", IsRequired=true, IsKey=true)]
    public string Source {
      get {
        return this["source"] as string;
      }
    }

    /*==========================================================================================================================
    | ATTRIBUTE: ENABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a value indicating whether this <see cref="SourceElement"/> is enabled.
    /// </summary>
    [ConfigurationProperty("enabled", DefaultValue="True", IsRequired=false)]
    public bool Enabled {
      get {
        return Convert.ToBoolean(this["enabled"], CultureInfo.InvariantCulture);
      }
    }

    /*==========================================================================================================================
    | ATTRIBUTE: LOCATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the location attribute value.
    /// </summary>
    [ConfigurationProperty("location", IsRequired=false)]
    public string Location {
      get {
        return this["location"] as string;
      }
    }

    /*==========================================================================================================================
    | ATTRIBUTE: TRUSTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a value indicating whether this <see cref="SourceElement"/> is trusted.
    /// </summary>
    [ConfigurationProperty("trusted", DefaultValue="False", IsRequired=false)]
    public bool Trusted {
      get {
        return Convert.ToBoolean(this["trusted"], CultureInfo.InvariantCulture);
      }
    }

    /*==========================================================================================================================
    | METHOD: GET ELEMENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the source element given the parent element and expected name.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElement"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>The matching source configuration element, or null if not found.</returns>
    public static SourceElement GetElement(ConfigurationElement parent, string key) {
      if (parent == null) return null;
      return (SourceElement)parent.ElementInformation.Properties[key]?.Value;
    }

    /// <summary>
    ///   Gets the source element given the parent element collection and expected name.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElementCollection"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>The matching source configuration element, or null if not found.</returns>
    public static SourceElement GetElement(ConfigurationElementCollection parent, string key) {
      if (parent == null) return null;
      foreach (SourceElement source in parent) {
        if (source.Source.Equals(key)) {
          return source;
        }
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks up a source element given the parent element and expected name, and, assuming it's enabled, identifies the
    ///   source value.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElement"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>The target value, or null if one is not found.</returns>
    public static string GetValue(ConfigurationElement parent, string key) => GetValue(GetElement(parent, key));

    /// <summary>
    ///   Looks up a source element given the parent element collection and expected name and, assuming it's enabled,
    ///   identifies the source value.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElementCollection"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>The target value, or null if one is not found.</returns>
    public static string GetValue(ConfigurationElementCollection parent, string key) => GetValue(GetElement(parent, key));

    /// <summary>
    ///   Gets the value for the specified element, assuming it's enabled, and based on the type of specified element's source,
    ///   looks up its value.
    /// </summary>
    /// <param name="element">The configuration element.</param>
    /// <returns>The target value, or null if one is not found.</returns>
    public static string GetValue(SourceElement element) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(HttpContext.Current != null, "Assumes the current HTTP context is available.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Return null if the element is disabled or missing
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (element == null || !element.Enabled) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull value from support source
      \-----------------------------------------------------------------------------------------------------------------------*/
      string value = null;

      switch (element.Source?.ToUpperInvariant()?? "") {
        case("QUERYSTRING") :
          value         = HttpContext.Current.Request.QueryString[element.Location];
          break;
        case("FORM") :
          value         = HttpContext.Current.Request.Form[element.Location];
          break;
        case("APPLICATION") :
          value         = (string)HttpContext.Current.Application?[element.Location];
          break;
        case("SESSION") :
          value         = (string)HttpContext.Current.Session?[element.Location];
          break;
        case("COOKIE") :
          value         = HttpContext.Current.Request.Cookies[element.Location]?.Value;
          break;
        case("ROLE") :
          value         = Roles.IsUserInRole(element.Location).ToString();
          break;
        case("HOSTNAME") :
          value         = element.Location;
          break;
        case("URL") :
          if (Int32.Parse(element.Location, CultureInfo.InvariantCulture) >= 0) {
            value       = HttpContext.Current.Request.Path.Split('/')[Int32.Parse(element.Location, CultureInfo.InvariantCulture)];
          }
          break;
        default :
          throw new ConfigurationErrorsException("The source '" + element.Source + "' in the web.config is invalid.");
      }

      return value;

    }

    /*==========================================================================================================================
    | METHOD: IS ENABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks up a source element at a given location and based on a specified parent configuration elemnt and the target
    ///   element's key, identifies the source value, and verifies whether the element is available, enabled, or set to true.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElement"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>
    ///   A boolean value representing whether or not the source is available, enabled or set to true.
    /// </returns>
    public static bool IsEnabled(ConfigurationElement parent, string key) => IsEnabled(parent, key, true);

    /// <summary>
    ///   Looks up a source element at a given location and based on a specified parent configuration elemnt and the target
    ///   element's key, identifies the source value, and verifies whether the element is available, enabled, or set to true.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElement"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <param name="evaluateValue">Boolean indicator noting whether to use the Value to determine enabled status.</param>
    /// <returns>
    ///   A boolean value representing whether or not the source is available, enabled or set to true.
    /// </returns>
    public static bool IsEnabled(ConfigurationElement parent, string key, bool evaluateValue) {
      return IsEnabled(GetElement(parent, key), evaluateValue);
    }

    /// <summary>
    ///   Looks up a source element at a given location and based on a specified parent configuration elemnt collection and
    ///   the target element's key, identifies the source value, and verifies whether the element is available, enabled, or
    ///   set to true.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElementCollection"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>
    ///   A boolean value representing whether or not the source is available, enabled or set to true.
    /// </returns>
    public static bool IsEnabled(ConfigurationElementCollection parent, string key) => IsEnabled(parent, key, false);

    /// <summary>
    ///   Looks up a source element at a given location and based on a specified parent configuration elemnt collection and
    ///   the target element's key, identifies the source value, and verifies whether the element is available, enabled, or
    ///   set to true.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElementCollection"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <param name="evaluateValue">Boolean indicator noting whether to use the Value to determine enabled status.</param>
    /// <returns>
    ///   Boolean value representing whether or not the source is available, enabled or set to true.
    /// </returns>
    public static bool IsEnabled(ConfigurationElementCollection parent, string key, bool evaluateValue) {
      return IsEnabled(GetElement(parent, key), evaluateValue);
    }

    /// <summary>
    ///   Looks up a source element at a given location, identifies the source value, and verifies whether the element is
    ///   available, enabled, or set to true.
    /// </summary>
    /// <param name="element">The target <see cref="SourceElement"/>.</param>
    /// <param name="evaluateValue">Boolean indicator noting whether to use the Value to determine enabled status.</param>
    /// <returns>
    ///   Boolean value representing whether or not the source is available, enabled or set to true.
    /// </returns>
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

      var value = GetValue(element);

      if (String.IsNullOrEmpty(value)) return false;

      return Convert.ToBoolean(value, CultureInfo.InvariantCulture);

    }

    /*==========================================================================================================================
    | METHOD: IS TRUSTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether the specified element is trusted, using the parent element and expected element name to find the
    ///   element.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElement"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>
    ///   Boolean value representing whether the specified element is available.
    /// </returns>
    public static bool IsTrusted(ConfigurationElement parent, string key) => IsTrusted(GetElement(parent, key));

    /// <summary>
    ///   Determines whether the specified element is trusted, using the parent element collection and expected element name to
    ///   find the element.
    /// </summary>
    /// <param name="parent">The parent <see cref="ConfigurationElementCollection"/>.</param>
    /// <param name="key">The string key (expected name).</param>
    /// <returns>
    ///   Boolean value representing whether the specified element is available.
    /// </returns>
    public static bool IsTrusted(ConfigurationElementCollection parent, string key) => IsTrusted(GetElement(parent, key));

    /// <summary>
    ///   Determines whether the specified element is trusted via its availability.
    /// </summary>
    /// <param name="element">The <see cref="SourceElement"/> object.</param>
    /// <returns>
    ///   Boolean value representing whether the specified element is available.
    /// </returns>
    public static bool IsTrusted(SourceElement element) => (element == null)? false : element.Trusted;

  } // Class

} // Namespace

