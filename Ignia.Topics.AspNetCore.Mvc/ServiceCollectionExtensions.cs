/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.AspNetCore.Mvc.Controllers;
using Ignia.Topics.Internal.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ignia.Topics.AspNetCore.Mvc {

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
      services.AddApplicationPart(typeof(TopicController).Assembly)

      //Add controllers to scope as dependencies
      .AddControllersAsServices();

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

  } //Class
} //Namespace