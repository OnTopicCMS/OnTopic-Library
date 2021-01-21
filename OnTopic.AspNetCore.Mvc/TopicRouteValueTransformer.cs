/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC ROUTE VALUE TRANSFORMER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Interprets endpoint routes associated with OnTopic in order to dynamically fill in any missing metadata based on the
  ///   needs of the <see cref="TopicRepositoryExtensions.Load(Repositories.ITopicRepository, RouteData)"/> method.
  /// </summary>
  public class TopicRouteValueTransformer: DynamicRouteValueTransformer {

    /*==========================================================================================================================
    | OVERRIDE: TRANSFORM (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates the current <see cref="RouteValueDictionary"/> for any missing attributes, and attempts to dynamically
    ///   inject them based on other values.
    /// </summary>
    #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(httpContext, nameof(httpContext));
      Contract.Requires(values, nameof(values));

      /*------------------------------------------------------------------------------------------------------------------------
      | Implicitly set controller
      >-------------------------------------------------------------------------------------------------------------------------
      | If the area is set, but not the controller, assume that the controller is named after the area by convention. If the
      | controller is being set in the route pattern, this won't change that.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var controller            = (string?)values["controller"];
      var area                  = (string?)values["area"];
      if (area is not null && controller is null) {
        values["controller"]    = area;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Implicitly set action
      >-------------------------------------------------------------------------------------------------------------------------
      | If the action isn't defined in the route, assume Index—which is the default action for the TopicController.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var action                = (string?)values["action"];
      if (action is null) {
        action                  = "Index";
        values["action"]        = action;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Implicitly set root topic
      >-------------------------------------------------------------------------------------------------------------------------
      | If the path is set, but the root topic isn't, then set the root topic to the area/controller name. The root topic is
      | required by the TopicRepositoryExtensions to create a fully qualified topic path, and correctly identify the topic
      | based on the path. It is not needed when routing by controller/action pairs.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var path                  = (string?)values["path"];
      if (path is not null || action.Equals("Index", StringComparison.OrdinalIgnoreCase)) {
        values["rootTopic"]     = area;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return values
      \-----------------------------------------------------------------------------------------------------------------------*/
      return values;

    }
    #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

  } //Class
} //Namespace