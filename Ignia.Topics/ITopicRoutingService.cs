/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ignia.Topics {

  /*============================================================================================================================
  | INTERFACE: TOPIC ROUTING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given contextual information (such as URL) will determine the current Topic, View, etc.
  /// </summary>
  /// <remarks>
  ///   The view location will change based on the environment. That said, in all cases, the view will factor in the topic's
  ///   Content Type and (if set) View properties, and optionally the view determined via the URL (as either a URL part, or a
  ///   query string parameter. For instance, a view can be set as /a/b/c/viewName/ or /a/b/c/?View=viewName.
  /// </remarks>
  public interface ITopicRoutingService {

    /*==========================================================================================================================
    | METHOD: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    Topic Topic { get; }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the content type associated with the topic associated with the current URL.
    /// </summary>
    string ContentType { get; }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the view name based on the content type, in combination with the query string, headers, and
    ///   topic attributes.
    /// </summary>
    string View { get; }

    /*==========================================================================================================================
    | PROPERTY: VIEW PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the full path to the target view, including the view directory and extension.
    /// </summary>
    string ViewPath { get; }

    /*==========================================================================================================================
    | METHOD: IS VALID VIEW?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Checks the specified view name against its availability in the views collection.
    /// </summary>
    /// <param name="contentType">The name of the topic's <see cref="Ignia.Topics.ContentType"/>.</param>
    /// <param name="viewName">The filename (minus extension) of the view.</param>
    /// <param name="matchedView">The string identifier for the matched view.</param>
    /// <returns>
    ///   A boolean value representing the view's validity as well as the output variable 'matchedView', indicating the
    ///   <see cref="Topic.View"/> name.
    /// </returns>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   contentType != null
    /// </requires>
    bool IsValidView(string contentType, string viewName, out string matchedView);

    }
  }
