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

    }
  }
