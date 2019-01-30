/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel;
using System.Configuration;
using Ignia.Topics.Diagnostics;

namespace Ignia.Topics.Web.Configuration {

  /*============================================================================================================================
  | CLASS: PAGE TYPE ELEMENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom <see cref="ConfigurationElement"/> which represents a page type (default:
  ///   <see cref="Ignia.Topics.Web.TopicPage"/>) as developed for the application.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Permits the application to define multiple page types.
  ///   </para>
  /// </remarks>
  public class PageTypeElement : ConfigurationElement {

    /*==========================================================================================================================
    | ATTRIBUTE: NAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the name of the page type; typically set to <see cref="Ignia.Topics.Web.TopicPage"/>.
    /// </summary>
    [ConfigurationProperty("name", IsRequired=true, IsKey=true)]
    public string Name {
      get {
        return this["name"] as string?? throw new NullReferenceException("The name element is not defined.");
      }
    }

    /*==========================================================================================================================
    | ATTRIBUTE: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the page type class definition, including namespace if provided.
    /// </summary>
    [TypeConverter(typeof(TypeNameConverter))]
    [ConfigurationProperty("type", IsRequired = false)]
    public Type Type => this["type"] as Type;

  } // Class

} // Namespace

