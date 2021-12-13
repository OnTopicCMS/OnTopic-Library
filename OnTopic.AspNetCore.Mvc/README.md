# OnTopic for ASP.NET Core
The `OnTopic.AspNetCore.Mvc` assembly provides a default implementation for utilizing OnTopic with ASP.NET Core (3.0 and above) . It is the recommended client for working with OnTopic.

[![OnTopic.AspNetCore.Mvc package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/4db5e20c-69c6-4134-823a-c3de06d1176e/Badge)](https://www.nuget.org/packages/OnTopic.AspNetCore.Mvc/)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

### Contents
- [Components](#components)
- [Controllers and View Components](#controllers-and-view-components)
- [View Conventions](#view-conventions)
  - [View Matching](#view-matching)
  - [View Locations](#view-locations)
  - [Example](#example)
- [Configuration](#configuration)
  - [Dependencies](#dependencies)
  - [Application](#application)
  - [Route Configuration](#route-configuration)
  - [Composition Root](#composition-root)
  - [Error Handling](#error-handling)

## Components
There are five key components at the heart of the ASP.NET Core implementation.
- **`TopicController`**: This is a default controller instance that can be used for _any_ topic path. It will automatically validate that the `Topic` exists, that it is not disabled (`!IsDisabled`), and will honor any redirects (e.g., if the `Url` attribute is filled out). Otherwise, it will return a `TopicViewResult` based on a view model, view name, and content type.
- **`TopicRouteValueTransformer`**: A `DynamicRouteValueTransformer` for use with the ASP.NET Core's `MapDynamicControllerRoute()` method, allowing for route parameters to be implicitly inferred; notably, it will use the `area` as the default `controller` and `rootTopic`, if those route parameters are not otherwise defined.
- **`TopicViewLocationExpander`**: Assists the out-of-the-box Razor view engine in locating views associated with OnTopic, e.g. by looking in `~/Views/ContentTypes/{ContentType}.cshtml`, or `~/Views/{ContentType}/{View}.cshtml`. See [View Locations](#view-locations) below.
- **`TopicViewResultExecutor`**: When the `TopicController` returns a `TopicViewResult`, the `TopicViewResultExecutor` takes over and attempts to identify the correct view based on the `accept` headers, `?view=` query string parameter, topic's default `View` attribute and, finally, the topic's `ContentType` attribute. See [View Matching](#view-matching) below.
- **`ServiceCollectionExtensions`**: A set of extensions to be used in an ASP.NET Core website's `Startup` class that automatically handle registering services, controllers, and other extensions from `OnTopic.AspNetCore.Mvc`.
- **`ITopicRepositoryExtensions`**: A set of extensions that allows loading topics based on an ASP.NET Core `RouteData` collection, including `OnTopic` route variables, such as `path` and `contenttype`.

## Controllers and View Components
There are five main controllers and view components that ship with the ASP.NET Core implementation. In addition to the core **`TopicController`**, these include the following ancillary classes:
- **[`ErrorController`](Controllers/ErrorController.cs)**: Provides a specialized `TopicController` with an `Http()` action for handling status code errors (e.g., from `UseStatusCodePages()`).
- **[`RedirectController`](Controllers/RedirectController.cs)**: Provides a single `Redirect` action which can be bound to a route such as `/Topic/{ID}/`; this provides support for permanent URLs that are independent of the `GetWebPath()`.
- **[`SitemapController`](Controllers/SitemapController.cs)**: Provides a single `Sitemap` action which recurses over the entire Topic graph, including all attributes, and returns an XML document with a sitemaps.org schema.
- **[`MenuViewComponentBase<T>`](Components/MenuViewComponentBase{T}.cs)**: Provides support for a navigation menu by automatically mapping the top three tiers of the current namespace (e.g., `Web`, its children, and grandchildren). Can accept any `INavigationTopicViewModel` as a generic argument; that will be used as the view model for each mapped instance.
- **[`PageLevelNavigationViewComponentBase<T>`](Components/PageLevelNavigationViewComponentBase{T}.cs)**: Provides support for page-level navigation by automatically mapping the child topics from the nearest `PageGroup`. Can accept any `INavigationTopicViewModel` as a generic argument; that will be used as the view model for each mapped instance.

> **Note:** There is no practical way for ASP.NET Core to provide routing for generic controllers and view components. As such, these _must_ be subclassed by each implementation. The derived class needn't do anything outside of provide a specific type reference to the generic base. For example:
> ```csharp
> public class MenuViewComponent: MenuViewComponentBase<NavigationTopicViewModel> {
>   public MenuViewComponent(
>     ITopicRepository topicRepository,
>     IHierarchicalTopicMappingService<NavigationTopicViewModel> hierarchicalTopicMappingService
>   ): base(topicRepository, hierarchicalTopicMappingService) {}
> }

## View Conventions
By default, OnTopic matches views based on the current topic's `ContentType` and, if available, `View`.

### View Matching
There are multiple ways for a view to be set. The `TopicViewResultExecutor` will automatically evaluate views based on the following locations. The first one to match a valid view name is selected.
- **`?View=`** query string parameter (e.g., `?View=Accordion`)
- **`Accept`** headers (e.g., `Accept=application/json`); will treat the segment after the `/` as a possible view name
- **`Action`** name (e.g., `Index()` or `JsonAsync()`); will exclude the `Async` suffix
- **`View`** attribute (i.e., `topic.View`)
- **`ContentType`** attribute (i.e., `topic.ContentType`)

This allows multiple views to be available for any individual content type, thus allowing pages using the same content type to potentially be rendered with different layouts or, even, different content types (e.g., JSON vs. HTML).

### View Locations
For each of the above [View Matching](#view-matching) rules, the `TopicViewLocationExpander` will search the following locations for a matching view:
- `~/Views/{Controller}/{View}.cshtml`
- `~/Views/{ContentType}/{View}.cshtml`
- `~/Views/{ContentType}/Shared/{View}.cshtml`
- `~/Views/ContentTypes/{ContentType}.{View}.cshtml`
- `~/Views/{Controller}/Shared/{View}.cshtml`
- `~/Views/ContentTypes/Shared/{View}.cshtml`
- `~/Views/ContentTypes/{View}.cshtml`
- `~/Views/Shared/{View}.cshtml`

> *Note:* After searching each of these locations for each of the [View Matching](#view-matching) rules, control will be handed over to the [`RazorViewEngine`](https://msdn.microsoft.com/en-us/library/system.web.mvc.razorviewengine%28v=vs.118%29.aspx?f=255&MSPPError=-2147217396), which will search the out-of-the-box default locations for ASP.NET Core.

### Example
If the `topic.ContentType` is `ContentList` and the `Accept` header is `application/json` then the `TopicViewResult` and `TopicViewEngine` would coordinate to search the following paths:
- `~/Views/Topic/JSON.cshtml`
- `~/Views/ContentList/JSON.cshtml`
- `~/Views/ContentList/Shared/JSON.cshtml`
- `~/Views/ContentTypes/ContentList.JSON.cshtml`
- `~/Views/Topic/Shared/JSON.cshtml`
- `~/Views/ContentTypes/Shared/JSON.cshtml`
- `~/Views/ContentTypes/JSON.cshtml`
- `~/Views/Shared/JSON.cshtml`

If no match is found, then the next `Accept` header will be searched. Eventually, if no match can be found on the various [View Matching](#view-matching) rules, then the following will be searched:

- `~/Views/Topic/ContentList.cshtml`
- `~/Views/ContentList/ContentList.cshtml`
- `~/Views/ContentList/Shared/ContentList.cshtml`
- `~/Views/ContentTypes/ContentList.ContentList.cshtml`
- `~/Views/Topic/Shared/ContentList.cshtml`
- `~/Views/ContentTypes/Shared/ContentList.cshtml`
- `~/Views/ContentTypes/ContentList.cshtml`

## Configuration

### Dependencies
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.AspNetCore.Mvc` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.AspNetCore.Mvc" Version="5.0.0" />
  </ItemGroup>
</Project>
```

### Application
In the `Startup` class, OnTopic's ASP.NET Core support can be registered by calling the `AddTopicSupport()` extension method:
```csharp
public class Startup {
  public void ConfigureServices(IServiceCollection services) {
    services.AddMvc().AddTopicSupport();
  }
}
```
> *Note:* This will register the `TopicViewLocationExpander`, `TopicViewResultExecutor`, `TopicRouteValueTransformer`, as well as all [Controllers](#controllers) that ship with `OnTopic.AspNetCore.Mvc`.

In addition, within the same `ConfigureServices()` method, you will need to establish a class that implements `IControllerActivator` and `IViewComponentActivator`, and will represent the site's _Composition Root_ for dependency injection. This will typically look like:
```csharp
var activator = new OrganizationNameActivator(Configuration.GetConnectionString("OnTopic"))
services.AddSingleton<IControllerActivator>(activator);
services.AddSingleton<IViewComponentActivator>(activator);
```
See [Composition Root](#composition-root) below for information on creating an implementation of `IControllerActivator` and `IViewComponentActivator` .

> *Note:* The controller activator name is arbitrary, and should follow the conventions appropriate for the site. Ignia typically uses `{OrganizationName}Activator` (e.g., `IgniaActivator`), but OnTopic doesn't need to know or care what the name is; that is between your application and ASP.NET Core.

> *Note:* The connection string can come from any source, or even be hard-coded. Ignia recommends storing it as part of your `secrets.json` during development and as part of the hosting environment's application settings. The above code, for instance, will work with a local `secrets.json` and Azure's connection configuration.

### Route Configuration
When registering routes via `Startup.Configure()` you may register any routes for OnTopic using the extension method:
```csharp
public class Startup {
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    app.UseEndpoints(endpoints => {

      endpoints.MapTopicAreaRoute();                    // {area:exists}/{**path}
      endpoints.MapImplicitAreaControllerRoute();       // {area:exists}/{action=Index}
      endpoints.MapDefaultControllerRoute();            // {controller=Home}/{action=Index}/{id?}
      endpoints.MapDefaultAreaControllerRoute();        // {area:exists}/{controller}/{action=Index}/{id?}

      endpoints.MapTopicErrors();                       // Error/{errorCode}
      endpoints.MapTopicRoute("Web");                   // Web/{**path}
      endpoints.MapTopicRedirect();                     // Topic/{topicId}
      endpoints.MapControllers();

    });
  }
}
```
> *Note:* Because OnTopic relies on wildcard path names, a new route should be configured for every root namespace (e.g., `/Web`). While it's possible to configure OnTopic to evaluate _all_ paths, this makes it difficult to delegate control to other controllers and handlers, when necessary. As a result, it is recommended that each root container be registered individually.

### Composition Root
As OnTopic relies on constructor injection, the application must be configured in a **Composition Root**—in the case of ASP.NET Core, that means a custom controller activator for controllers, and view component activator for view components. For controllers, the basic structure of this might look like:
```csharp
var sqlTopicRepository          = new SqlTopicRepository(connectionString);
var cachedTopicRepository       = new CachedTopicRepository(sqlTopicRepository);
var topicViewModelLookupService = new TopicViewModelLookupService();
var topicMappingService         = new TopicMappingService(cachedTopicRepository, topicViewModelLookupService);

return controllerType.Name switch {
  nameof(TopicController)       => new TopicController(_topicRepository, _topicMappingService),
  nameof(RedirectController)    => new RedirectController(_topicRepository),
  nameof(SitemapController)     => new SitemapController(_topicRepository),
  _                             => throw new InvalidOperationException($"Unknown controller {controllerType.Name}")
};
```
For a complete reference template, including the ancillary controllers, view components, and a more maintainable structure, see the [`OrganizationNameActivator.cs`](https://gist.github.com/JeremyCaney/00c04b1b9f40d9743793cd45dfaaa606) Gist. Optionally, you may use a dependency injection container.

> *Note:* The default `TopicController` will automatically identify the current topic (based on the `RouteData`), map the current topic to a corresponding view model (based on [the `TopicMappingService` conventions](../OnTopic/Mapping/README.md)), and then return a corresponding view (based on the [view conventions](#view-conventions)). For most applications, this is enough. If custom mapping rules or additional presentation logic are needed, however, implementors can subclass `TopicController`.

### Error Handling
The `ErrorController` provides support for handling ASP.NET Core's `UseStatusCodePages()` middleware, while continuing to support a range of other options. Routing to the controller can be supported by any of the following options, in isolation or together:
```csharp
public class Startup {
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
    app.UseEndpoints(endpoints => {
      endpoints.MapTopicErrors();                        // Error/{errorCode}
      endpoints.MapTopicErrors("Errors");                // Errors/{errorCode}
      endpoints.MapDefaultControllerRoute();             // Error/Http/{errorCode}
      endpoints.MapTopicRoute("Error");                  // Error/{path}; e.g., Error/Unauthorized
    }
  }
}
```
The first three of these options all use the `Http()` action, which will provide the following fallback logic:
- If `Error:{errorCode}` exists, use that (e.g., `Error:404`)
- If `Error:{errorCode/100*100} exists, use that (e.g., `Error:400`)
- If `Error` exists, use that (e.g., `Error`)

These are all intended to be used with one of ASP.NET Core's `UseStatusCodePages()` methods. For instance:
```csharp
app.UseStatusCodePagesWithReExecute("/Error/{0}");
```
The last option allows the same `ErrorController` to be used with any other custom error handling that might be configured—such as middleware, or the legacy `<httpErrors />` handler—to handle any custom page under the `Error` topic.