/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic {

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
  [Obsolete(
    "The ITopicRoutingService is no longer used in the latest clients, and will be removed in OnTopic Library 5.0. In the " +
    "latest OnTopic.AspNetCore.Mvc libraries, it is replaced with a ITopicRepository.Load() extension method which accepts " +
    "RouteData as an argument, in order to lookup topics based on that library's routing conventions."
  )]
  public interface ITopicRoutingService {

    /*==========================================================================================================================
    | METHOD: GET CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    Topic? GetCurrentTopic();

  } //Class
} //Namespace
