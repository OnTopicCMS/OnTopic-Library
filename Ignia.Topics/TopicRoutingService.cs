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
  | CLASS: TOPIC ROUTING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a URL or route data, ITopicProvider will determine the associated Topic.
  /// </summary>
  public class TopicRoutingService : ITopicRoutingService {

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
    public TopicRoutingService(
      ITopicRepository          topicRepository,
      Uri                       uri,
      RouteData                 routeData
     ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository != null, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(uri != null, "An instance of a Uri instantiated to the requested URL is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = topicRepository;
      _uri                      = uri;
      _routes                   = routeData ?? new RouteData();

    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    public Topic Topic {
      get {

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
          _topic = _topicRepository.Load().GetTopic(path.Replace("/", ":"));
        }

        /*------------------------------------------------------------------------------------------------------------------------
        | Return Topic
        \-----------------------------------------------------------------------------------------------------------------------*/
        return _topic;

      }
      private set => _topic = value;
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the content type associated with the topic associated with the current URL.
    /// </summary>
    public string ContentType {
      get {
        return Topic?.ContentType;
      }
    }

  } // Class

} // Namespace
