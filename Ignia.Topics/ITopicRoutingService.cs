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
  ///   Given contextual information (such as URL) will determine the current Topic.
  /// </summary>
  /// <remarks>
  ///   Each environment (e.g., ASP.NET MVC) will require a platform-specific <see cref="ITopicRoutingService"/> to act as an
  ///   adapter to framework-specific libraries (e.g., <code>HttpContext</code>). In addition, custom versions may be created
  ///   in order to establish custom mappings between URL or route data and the hierarchy of topics, should the need arise.
  /// </remarks>
  public interface ITopicRoutingService {

    /*==========================================================================================================================
    | METHOD: GET CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    Topic GetCurrentTopic();

    }
  }
