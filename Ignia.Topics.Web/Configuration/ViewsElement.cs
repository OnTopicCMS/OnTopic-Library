/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Web.Configuration {

  /*============================================================================================================================
  | CLASS: VIEWS ELEMENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom <see cref="ConfigurationElement"/> which represents the views settings configuration.
  /// </summary>
  /// <remarks>
  ///   Permits an application to define the location of common views files.
  /// </remarks>
  public class ViewsElement : ConfigurationElement {

    /*==========================================================================================================================
    | ATTRIBUTE: PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the path attribute value from the <see cref="ViewsElement"/> configuration element.
    /// </summary>
    [ConfigurationProperty("path", IsRequired=false)]
    public string Path {
      get {
        return this["path"] as string;
      }
    }

  } // Class

} // Namespace

