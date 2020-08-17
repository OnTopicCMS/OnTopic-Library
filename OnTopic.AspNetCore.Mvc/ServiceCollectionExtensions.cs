/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Internal.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: SERVICE COLLECTION EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides extension methods for configuring the OnTopic library with ASP.NET Core MVC.
  /// </summary>
  public static class ServiceCollectionExtensions {

    /*==========================================================================================================================
    | EXTENSION: ADD TOPIC SUPPORT (IMVCBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Configures the Razor engine to include OnTopic view locations via the <see cref="TopicViewLocationExpander"/>.
    /// </summary>
    public static IMvcBuilder AddTopicSupport(this IMvcBuilder services) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(services, nameof(services));

      /*------------------------------------------------------------------------------------------------------------------------
      | Register services
      \-----------------------------------------------------------------------------------------------------------------------*/
      services.Services.TryAddSingleton<IActionResultExecutor<TopicViewResult>, TopicViewResultExecutor>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure services
      \-----------------------------------------------------------------------------------------------------------------------*/
      services.AddRazorOptions(o => {
        o.ViewLocationExpanders.Add(new TopicViewLocationExpander());
      });

      /*------------------------------------------------------------------------------------------------------------------------
      | Register local controllers
      \-----------------------------------------------------------------------------------------------------------------------*/
      //Add Topic assembly into scope
      services.AddApplicationPart(typeof(TopicController).Assembly);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return services for fluent API
      \-----------------------------------------------------------------------------------------------------------------------*/
      return services;
    }

    /*==========================================================================================================================
    | EXTENSION: MAP TOPIC ROUTE (IROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an MVC route for handling OnTopic related requests, and maps it to the <see cref="TopicController"/> by default.
    /// </summary>
    /// <remarks>
    ///   For ASP.NET Core 3, prefer instead <see cref="MapTopicRoute(IEndpointRouteBuilder, String, String, String)"/>, as
    ///   endpoint routing is preferred in ASP.NET Core 3. OnTopic also offers far more extension methods for endpoint routing,
    ///   while this method is provided exclusively for backward compatibility.
    /// </remarks>
    [Obsolete("This method is deprecated and will be removed in OnTopic 5. Callers should migrate to endpoint routing.", false)]
    public static IRouteBuilder MapTopicRoute(
      this IRouteBuilder routes,
      string rootTopic,
      string controller = "Topic",
      string action = "Index"
    ) =>
      routes.MapRoute(
        name: $"{rootTopic}Topic",
        template: rootTopic + "/{*path}",
        defaults: new { controller, action, rootTopic }
      );

    /*==========================================================================================================================
    | EXTENSION: MAP TOPIC ROUTE (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the <c>{rootTopic}/{**path}</c> endpoint route for a specific root topic, which enables that root to be mapped to
    ///   specific topics via the <see cref="TopicRepositoryExtensions.Load(Repositories.ITopicRepository, RouteData)"/>
    ///   extension method used by <see cref="TopicController"/>.
    /// </summary>
    public static ControllerActionEndpointConventionBuilder MapTopicRoute(
      this IEndpointRouteBuilder routes,
      string rootTopic,
      string controller = "Topic",
      string action = "Index"
    ) =>
      routes.MapControllerRoute(
        name: $"{rootTopic}Topic",
        pattern: rootTopic + "/{**path}",
        defaults: new { controller, action, rootTopic }
      );

    /*==========================================================================================================================
    | EXTENSION: MAP TOPIC AREA ROUTE (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the <c>{areaName}/{**path}</c> endpoint route for a specific area, which enables the areas to be mapped to
    ///   specific topics via the <see cref="TopicRepositoryExtensions.Load(Repositories.ITopicRepository, RouteData)"/>
    ///   extension method used by <see cref="TopicController"/>.
    /// </summary>
    /// <remarks>
    ///   If there are multiple routes that fit this description, you can instead opt to use the <see cref=
    ///   "MapTopicAreaRoute(IEndpointRouteBuilder)"/> extension, which will register all areas.
    /// </remarks>
    public static ControllerActionEndpointConventionBuilder MapTopicAreaRoute(
      this IEndpointRouteBuilder routes,
      string areaName,
      string? controller = null,
      string action = "Index"
    ) =>
      routes.MapAreaControllerRoute(
        name: $"TopicAreas",
        areaName: areaName,
        pattern: areaName + "/{**path}",
        defaults: new { controller = controller?? areaName, action, rootTopic = areaName }
      );

    /*==========================================================================================================================
    | EXTENSION: MAP TOPIC REDIRECT (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds an MVC route for handling OnTopic redirect requests, and maps it to the <see cref="RedirectController"/>.
    /// </summary>
    public static ControllerActionEndpointConventionBuilder MapTopicRedirect(this IEndpointRouteBuilder routes) =>
      routes.MapControllerRoute(
        name: "TopicRedirect",
        pattern: "Topic/{topicId}",
        defaults: new { controller = "Redirect", action = "Redirect" }
      );

  } //Class
} //Namespace