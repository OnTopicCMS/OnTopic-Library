/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Configuration;

namespace Ignia.Topics.Web.Configuration {

  /*============================================================================================================================
  | CLASS: VERSIONING ELEMENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom <see cref="ConfigurationElement"/> which represents the data versioning configuration.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Permits an application to define whether or not draft Topics data should be displayed to the user, and if so under
  ///     which specific location.
  ///   </para>
  ///   <para>
  ///     Adapted from the Ignia Localization library; in the future, these libraries may(and should) share custom
  ///     configuration classes.
  ///   </para>
  /// </remarks>
  public class VersioningElement : ConfigurationElement {

    /*=========================================================================================================================
    | ELEMENT: DRAFT MODE
    \------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the draft mode attribute value from the <see cref="VersioningElement"/> configuration element.
    /// </summary>
    [ConfigurationProperty("draftMode")]
    public SourceElement DraftMode {
      get {
        return this["draftMode"] as SourceElement;
      }
    }

  } // Class

} // Namespace

