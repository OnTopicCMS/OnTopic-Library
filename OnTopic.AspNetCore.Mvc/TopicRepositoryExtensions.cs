/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Routing;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides methods that extend the functionality of <see cref="ITopicRepository"/> with features dependent on ASP.NET
  ///   Core, and dependended on by the local <see cref="OnTopic.AspNetCore.Mvc"/> assembly.
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
    ///   prior to calling <see cref="ITopicRepository.Load(String?, Boolean)"/>, assuming they are using the standard routing
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
      var area                  = getRouteValue("area");
      var controller            = getRouteValue("controller");
      var action                = getRouteValue("action");
      var path                  = getRouteValue("path");
      var rootTopic             = getRouteValue("rootTopic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Define search paths based on route variables
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE JJC20191205: If any variables—such as {area}—is not defined, then it will be excluded and any slashes ("/")
      | remaining at the beginning or the end will be removed. As such, there's no need to search for both e.g.
      | {area}/{controller}/{action}/{path} as well as, say, {controller}/{action}. This provides an automatical fallback in
      | case particular routes aren't present. That said, if they are defined, but should be excluded from a fallback, then
      | that path does need to be defined—thus e.g. {area}/{controller}/{path}.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var paths = new List<string?>() {
        cleanPath($"{rootTopic}/{path}"),
        cleanPath($"{area}/{controller}/{action}/{path}"),
        cleanPath($"{area}/{controller}/{path}"),
        cleanPath($"{area}/{action}/{path}"),
        cleanPath($"{area}/{path}")
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Load by path
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = (Topic?)null;

      foreach (var searchPath in paths) {
        if (topic is not null) break;
        if (String.IsNullOrEmpty(searchPath)) continue;
        try {
          topic = topicRepository.Load(searchPath);
        }
        catch (InvalidKeyException) {
          //As route data comes from user-submitted requests, it's expected that some may contain invalid keys. From this
          //method's perspective, this is no different than having an invalid URL. As such, we should return a null result, not
          //bubble up an exception.
        }
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
        .Trim(new char[] { '/' })
        .Replace("//", "/", StringComparison.Ordinal)
        .Replace("/", ":", StringComparison.Ordinal);

    }

  } //Class
} //Namespace