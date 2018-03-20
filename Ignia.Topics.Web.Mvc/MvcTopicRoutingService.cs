/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Diagnostics.Contracts;
using Ignia.Topics.Repositories;
using System.Collections.Specialized;

namespace Ignia.Topics {

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
  public class MvcTopicRoutingService : ITopicRoutingService {

    /*============================================================================================================================
    | PRIVATE VARIABLES
    \---------------------------------------------------------------------------------------------------------------------------*/
    private                     ITopicRepository                _topicRepository                = null;
    private                     RouteData                       _routes                         = null;
    private                     Uri                             _uri                            = null;
    private                     Topic                           _topic                          = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingService"/> class based on a URL instance, a fully qualified
    ///   path to the views Directory, and, optionally, the expected filename suffix fo each view file.
    /// </summary>
    public MvcTopicRoutingService(
      ITopicRepository          topicRepository,
      Uri                       uri,
      RouteData                 routeData
     ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository != null, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(uri != null, "An instance of a Uri instantiated to the requested URL is required.");
      Contract.Requires(routeData != null, "An instance of a RouteData dictionary is required. It can be empty.");

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
    public Topic GetCurrentTopic() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topic == null) {
        var path = _uri.AbsolutePath;
        if (_routes.Values.ContainsKey("path")) {
          path = _routes.GetRequiredString("path");
          if (_routes.Values.ContainsKey("rootTopic")) {
            path = _routes.GetRequiredString("rootTopic") + "/" + path;
          }
        }
        path = path.Trim(new char[] { '/' }).Replace("//", "/");
        _topic = _topicRepository.Load(path.Replace("/", ":"));
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

  } // Class

} // Namespace
