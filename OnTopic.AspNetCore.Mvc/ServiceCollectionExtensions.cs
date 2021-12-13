/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OnTopic.AspNetCore.Mvc.Controllers;

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
      services.Services.TryAddSingleton<TopicRouteValueTransformer>();

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
    [ExcludeFromCodeCoverage]
    [Obsolete("This method is deprecated and will be removed in OnTopic 5. Callers should migrate to endpoint routing.", true)]
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
    [ExcludeFromCodeCoverage]
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

    /// <summary>
    ///   Adds the <c>{area:exists}/{**path}</c> endpoint route for all areas, which enables the areas to be mapped to specific
    ///   topics via the <see cref="TopicRepositoryExtensions.Load(Repositories.ITopicRepository, RouteData)"/> extension method
    ///   used by <see cref="TopicController"/>.
    /// </summary>
    /// <remarks>
    ///   Be aware that this method uses the <see cref="ControllerEndpointRouteBuilderExtensions.MapDynamicControllerRoute{
    ///   TTransformer}(IEndpointRouteBuilder, String)"/> method. In .NET 3.x, this is incompatible with both the <see cref=
    ///   "AnchorTagHelper"/> and <see cref="LinkGenerator"/> classes. This means that e.g. <c>@Url.Action()</c> references
    ///   in views won't be properly formed. If these are required, prefer registering each route individually using <see cref=
    ///   "MapTopicAreaRoute(IEndpointRouteBuilder, String, String?, String)"/>.
    /// </remarks>
    public static void MapTopicAreaRoute(this IEndpointRouteBuilder routes) =>
      routes.MapDynamicControllerRoute<TopicRouteValueTransformer>("{area:exists}/{**path}");

    /*==========================================================================================================================
    | EXTENSION: MAP DEFAULT AREA CONTROLLER ROUTES (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the fully-qualified <c>{area:exists}/{controller}/{action=Index}/{id?}</c> endpoint route for all areas.
    /// </summary>
    /// <remarks>
    ///   This is analogous to the standard <see cref="ControllerEndpointRouteBuilderExtensions.MapDefaultControllerRoute(
    ///   IEndpointRouteBuilder)"/> method that ships with ASP.NET, except that it works with areas.
    /// </remarks>
    public static void MapDefaultAreaControllerRoute(this IEndpointRouteBuilder routes) =>
      routes.MapControllerRoute(
        name: "TopicAreas",
        pattern: "{area:exists}/{controller}/{action=Index}/{id?}"
      );

    /*==========================================================================================================================
    | EXTENSION: MAP IMPLICIT AREA CONTROLLER ROUTES (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the <c>{areaName}/{action=Index}</c> endpoint route for a specific area where the controller has the same name as
    ///   the area.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This extension method implicitly assigns the controller name based on the area name. This is advantageous when an
    ///     area has a single controller which is named after the area—e.g., <c>[Area("Forms")]</c> and <c>FormsController</c>—
    ///     as this allows the redundant <c>Controller</c> to be ommited from the route (e.g., <c>/Forms/Forms/{action}</c>.
    ///   </para>
    ///   <para>
    ///     If there are multiple routes that fit this description, you can instead opt to use the <see cref=
    ///     "MapImplicitAreaControllerRoute(IEndpointRouteBuilder)"/> overload, which will register all areas.
    ///   </para>
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public static void MapImplicitAreaControllerRoute(this IEndpointRouteBuilder routes, string areaName) =>
      routes.MapAreaControllerRoute(
        name: $"{areaName}TopicArea",
        areaName: areaName,
        pattern: $"{areaName}/{{action}}",
        defaults: new { controller = areaName }
      );

    /// <summary>
    ///   Adds the <c>{area:exists}/{action=Index}</c> endpoint route for all areas where the controller has the same name as
    ///   the area.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This extension method implicitly assigns the controller name based on the area name. This is advantageous when there
    ///     are multiple areas which have a single controller which is named after the area—e.g., <c>[Area("Forms")]</c> and
    ///     <c>FormsController: Controller</c>—as this allows those to be collectively registered under a single route, without
    ///     needing the redundant <c>Controller</c> value to be defined in the route (e.g., <c>/Forms/Forms/{action}</c>.
    ///   </para>
    ///   <para>
    ///     Be aware that this method uses the <see cref="ControllerEndpointRouteBuilderExtensions.MapDynamicControllerRoute{
    ///     TTransformer}(IEndpointRouteBuilder, String)"/> method. In .NET 3.x, this is incompatible with both the <see cref=
    ///     "AnchorTagHelper"/> and <see cref="LinkGenerator"/> classes. This means that e.g. <c>@Url.Action()</c> references
    ///     in views won't be properly formed. If these are required, prefer registering each route individually using <see
    ///     cref="MapImplicitAreaControllerRoute(IEndpointRouteBuilder, String)"/>.
    ///   </para>
    /// </remarks>
    public static void MapImplicitAreaControllerRoute(this IEndpointRouteBuilder routes) =>
      routes.MapDynamicControllerRoute<TopicRouteValueTransformer>("{area:exists}/{action=Index}");

    /*==========================================================================================================================
    | EXTENSION: MAP ERROR ROUTE (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the <c>/Error/{errorCode}</c> endpoint route for the <see cref="ErrorController"/>.
    /// </summary>
    /// <remarks>
    ///   This allows the <see cref="ErrorController"/> to be used in conjunction with e.g., the <see cref="
    ///   StatusCodePagesExtensions.UseStatusCodePages(IApplicationBuilder)"/>, by providing a route for capturing the <c>
    ///   errorCode</c>.
    /// </remarks>
    public static ControllerActionEndpointConventionBuilder MapTopicErrors(this IEndpointRouteBuilder routes, string rootTopic = "Error") =>
      routes.MapControllerRoute(
        name: "TopicError",
        pattern: $"{rootTopic}/{{id:int}}/",
        defaults: new { controller = "Error", action = "Http", rootTopic }
      );

    /*==========================================================================================================================
    | EXTENSION: MAP TOPIC SITEMAP (IENDPOINTROUTEBUILDER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds the <c>Sitemap/{action=Index}</c> endpoint route for the OnTopic sitemap.
    /// </summary>
    /// <remarks>
    ///   For most implementations, this will be covered by the default route, such as that implemented by the standard <see
    ///   cref="ControllerEndpointRouteBuilderExtensions.MapDefaultControllerRoute(IEndpointRouteBuilder)"/> method that ships
    ///   with ASP.NET. This extension method is provided as a convenience method for implementations that aren't using the
    ///   standard route, for whatever reason, and want a specific route setup for the sitemap.
    /// </remarks>
    public static ControllerActionEndpointConventionBuilder MapTopicSitemap(this IEndpointRouteBuilder routes) =>
      routes.MapControllerRoute(
        name: "TopicSitemap",
        pattern: "Sitemap/{action=Index}",
        defaults: new { controller = "Sitemap" }
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