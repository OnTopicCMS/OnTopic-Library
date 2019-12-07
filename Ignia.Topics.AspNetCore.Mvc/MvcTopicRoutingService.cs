/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Internal.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: MVC TOPIC ROUTING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given contextual information (including a URL and <see cref="ITopicRepository"/>) will determine the current Topic.
  /// </summary>
  /// <remarks>
  ///   The <see cref="MvcTopicRoutingService"/> is distributed with, and intended exclusively for use with, the ASP.NET MVC
  ///   version of OnTopic. As such, it makes no attempt to abstract out basic infrastructure components that ship with ASP.NET
  ///   MVC, such as the <see cref="RouteData"/>. That said, it also fully encapsulates them to ensure that they are not leaked
  ///   through the <see cref="ITopicRoutingService"/> abstraction.
  /// </remarks>
  [Obsolete(
    "The MvcTopicRoutingService is deprecate for use in ASP.NET Core. Instead, use the ITopicRepository.Load(RouteData) " +
    "extension method to retrieve the current topic based on the routing data.",
    false
  )]
  public class MvcTopicRoutingService : ITopicRoutingService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository;
    private readonly            RouteData                       _routes;
    private readonly            Uri                             _uri;
    private                     Topic?                          _topic                          = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingService"/> class based on a URL instance, a fully qualified
    ///   path to the views Directory, and, optionally, the expected filename suffix of each view file.
    /// </summary>
    public MvcTopicRoutingService(
      ITopicRepository          topicRepository,
      Uri                       uri,
      RouteData                 routeData
     ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(uri, "An instance of a Uri instantiated to the requested URL is required.");
      Contract.Requires(routeData, "An instance of a RouteData dictionary is required. It can be empty.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = topicRepository;
      _uri                      = uri;
      _routes                   = routeData;

    }

    /*==========================================================================================================================
    | METHOD: GET CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    public Topic? GetCurrentTopic() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topic == null) {
        var path = _uri.AbsolutePath;
        if (_routes.Values.TryGetValue("path", out var routePath)) {
          path = (string)routePath;
          if (_routes.Values.TryGetValue("rootTopic", out var rootTopic)) {
            path = rootTopic + "/" + path;
          }
        }
        path = path.Trim(new char[] { '/' }).Replace("//", "/", StringComparison.InvariantCulture);
        _topic = _topicRepository.Load(path.Replace("/", ":", StringComparison.InvariantCulture));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set route data
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topic != null) {
        if (_routes.Values.ContainsKey("contenttype")) {
          _routes.Values.Remove("contenttype");
        }
        _routes.Values.Add("contenttype", _topic.ContentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return Topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _topic;

    }

  } //Class
} //Namespace