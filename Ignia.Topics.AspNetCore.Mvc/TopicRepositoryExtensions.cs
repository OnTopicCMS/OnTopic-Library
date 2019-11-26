/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Repositories;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace Ignia.Topics.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides methods that extend the functionality of <see cref="ITopicRepository"/> with features dependent on ASP.NET
  ///   Core, and dependended on by the local <see cref="Ignia.Topics.AspNetCore.Mvc"/> assembly.
  /// </summary>
  public static class TopicRepositoryExtensions {

    /*==========================================================================================================================
    | EXTENSION: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the current topic based on variables defined in the <see cref="RouteData"/> dictionary.
    /// </summary>
    /// <remarks>
    ///   The ASP.NET Core MVC <see cref="EndpointDataSource"/> defines specified route names. In addition to the common, out
    ///   of the box routes, such as <c>controller</c> and <c>action</c>, the <see cref="ServiceCollectionExtensions"/> defines
    ///   additional topic-specific routes, such as <c>rootTopic</c> and <c>path</c>. These can be combined to identify a topic
    ///   in the repository. By using the extension method, callers needn't assemble their own <see cref="Topic.GetUniqueKey"/>
    ///   prior to calling <see cref="ITopicRepository.Load(String uniqueKey)"/>, assuming they are using the standard routing
    ///   variables.
    /// </remarks>
    public static Topic? Load(
      this ITopicRepository topicRepository,
      RouteData routeData
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, nameof(topicRepository));
      Contract.Requires(routeData, nameof(routeData));

      /*------------------------------------------------------------------------------------------------------------------------
      | Define parameters from route
      \-----------------------------------------------------------------------------------------------------------------------*/
      var primaryPath           = (string?)null;
      var fallbackPath          = (string?)null;
      var controller            = getRouteValue("controller");
      var action                = getRouteValue("action");
      var path                  = getRouteValue("path");
      var rootTopic             = getRouteValue("rootTopic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Define search paths based on route variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var paths = new List<String?>() {
       cleanPath($"{rootTopic}/{path}"),
       cleanPath($"{controller}/{action}/{path}"),
       cleanPath($"{controller}/{path}"),
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Load by path
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = (Topic?)null;

      foreach (var searchPath in paths) {
       if (topic != null) break;
       if (String.IsNullOrEmpty(searchPath)) continue;
       topic = topicRepository.Load(searchPath);
      }

      return topic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get route value
      \-----------------------------------------------------------------------------------------------------------------------*/
      string? getRouteValue(string parameter) {
        if (routeData.Values.TryGetValue(parameter, out var value)) {
          return value?.ToString();
        }
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Clean path
      \-----------------------------------------------------------------------------------------------------------------------*/
      static string? cleanPath(string? path) => path?
        .Trim(new char[] {'/'})
        .Replace("//", "/", StringComparison.InvariantCulture)
        .Replace("/", ":", StringComparison.InvariantCulture);

    }

  } //Class
} //Namespace