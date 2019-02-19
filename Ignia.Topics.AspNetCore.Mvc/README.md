# `Ignia.Topics.AspNetCore.Mvc`
The `Ignia.Topics.AspNetCore.Mvc` assembly provides a default implementation for utilizing OnTopic with the ASP.NET Core 2.x Framework.

### Contents
- [Components](#components)
- [Controllers](#controllers)
- [View Conventions](#view-conventions)
  - [View Matching](#view-matching)
  - [View Locations](#view-locations)
  - [Example](#example)
- [Configuration](#configuration)
  - [Application](#application)
  - [Route Configuration](#route-configuration)
  - [Controller Activator](#controller-activator)

## Components
There are five key components at the heart of the ASP.NET Core implementation.
- **`MvcTopicRoutingService`**: This is a concrete implementation of the `ITopicRoutingService` which accepts contextual information about a given request (in this case, the URL and routing data) and then uses it to retrieve the current `Topic` from an `ITopicRepository`.
- **`TopicController`**: This is a default controller instance that can be used for _any_ topic path. It will automatically validate that the `Topic` exists, that it is not disabled (`IsDisabled`), and will honor any redirects (e.g., if the `Url` attribute is filled out). Otherwise, it will return a `TopicViewResult` based on a view model, view name, and content type. 
- **`TopicViewLocationExpander`**: Assists the out-of-the-box Razor view engine in locating views associated with OnTopic, e.g. by looking in `~/Views/ContentTypes/ContentType.cshtml`, or `~/Views/ContentType/View.cshtml. See [View Locations](#view-locations) below.
- **`TopicViewResultExecutor`**: When the `TopicController` returns a `TopicViewResult`, the `TopicViewResultExecutor` takes over and attempts to identify the correct view based on the `accept` headers, `view` query string parameter, topic's default `View` attribute and, finally, the topic's `ContentType` attribute. See [View Matching](#view-matching) below.
- **`ServiceCollectionExtensions`**: A set of extensions to be used in an ASP.NET Core website's `Startup` class that automatically handle registering services, controllers, and other extensions from `Ignia.Topics.AspNetCore.Mvc`.

> **Note:** In [`Ignia.Topics.Web.Mvc`](../Ignia.Topics.Web.Mvc/), the `TopicViewEngine` took on the responsibilities now handled by the `TopicViewLocationExpander`, and the `TopicViewResult` took on responsibilities now handled by the `TopicViewResultExecutor`.

## Controllers
There are six main controllers that ship with the ASP.NET Core implementation. In addition to the core **`TopicController`**, these include the following ancillary controllers:
- **`ErrorControllerBase<T>`**: Provides support for `Error`, `NotFound`, and `InternalServer` actions. Can accept any `IPageTopicViewModel` as a generic argument; that will be used as the view model.
- **`FallbackController`**: Used in a [Controller Factory](#controller-factory) as a fallback, in case no other controllers can accept the request. Simply returns a `NotFoundResult` with a predefined message.
- **`LayoutControllerBase<T>`**: Provides support for a navigation menu by automatically mapping the top three tiers of the current namespace (e.g., `Web`, its children, and grandchildren). Can accept any `INavigationTopicViewModel` as a generic argument; that will be used as the view model for each mapped instance. 
- **`RedirectController`**: Provides a single `Redirect` action which can be bound to a route such as `/Topic/{ID}/`; this provides support for permanent URLs that are independent of the `GetWebPath()`. 
- **`SitemapController`**: Provides a single `Sitemap` action which returns a reference to the `ITopicRepository`, thus allowing a sitemap view to recurse over the entire Topic graph, including all attributes.

> **Note:** There is not a practical way for ASP.NET Core to provide routing for generic controllers. As such, these _must_ be subclassed by each implementation. The derived controller needn't do anything outside of provide a specific type reference to the generic base.

## View Conventions
By default, OnTopic matches views based on the current topic's `ContentType` and, if available, `View`.

### View Matching
There are multiple ways for a view to be set. The `TopicViewResultExecutor` will automatically evaluate views based on the following locations. The first one to match a valid view name is selected.
- **`?View=`** query string parameter (e.g., `?View=Accordion`)
- **`Accept`** headers (e.g., `Accept=application/json`); will treat the segment after the `/` as a possible view name
- **`View`** attribute (i.e., `topic.View`)
- **`ContentType`** attribute (i.e., `topic.ContentType`)

### View Locations
For each of the above [View Matching](#view-matching) rules, the `TopicViewLocationExpander` will search the following locations for a matching view:
- `~/Views/{ContentType}/{View}.cshtml`
- `~/Views/ContentTypes/{ContentType}.{View}.cshtml`
- `~/Views/ContentTypes/{ContentType}.cshtml`
- `~/Views/Shared/{View}.cshtml`

> *Note:* After searching each of these locations for each of the [View Matching](#view-matching) rules, control will be handed over to the [`RazorViewEngine`](https://msdn.microsoft.com/en-us/library/system.web.mvc.razorviewengine%28v=vs.118%29.aspx?f=255&MSPPError=-2147217396), which will search the out-of-the-box default locations for ASP.NET MVC.

### Example
If the `topic.ContentType` is `ContentList` and the `Accept` header is `application/json` then the `TopicViewResult` and `TopicViewEngine` would coordinate to search the following paths:
- `~/Views/ContentList/JSON.cshtml`
- `~/Views/ContentTypes/ContentList.JSON.cshtml`
- `~/Views/ContentTypes/JSON.cshtml`
- `~/Views/Shared/JSON.cshtml`

If no match is found, then the next `Accept` header will be searched. Eventually, if no match can be found on the various [View Matching](#view-matching) rules, then the following will be searched:

- `~/Views/ContentList/ContentList.cshtml`
- `~/Views/ContentTypes/ContentList.ContentList.cshtml`
- `~/Views/ContentTypes/ContentList.cshtml`
- `~/Views/Shared/ContentList.cshtml`

## Configuration

### Application
In the `Startup` class, OnTopic's ASP.NET Core support can be registered by calling the `AddTopicSupport()` extension method:
```
public class Startup {
  public void ConfigureServices(IServiceCollection services) {
	  services.AddMvc()
		  .AddTopicSupport();
	}
}
```
> *Note:* This will register the `TopicViewLocationExpander`, `TopicViewResultExecutor`, as well as all [Controllers](#controllers) that ship with `Ignia.Topics.AspNetCore.Mvc`.

In addition, within the same `ConfigureServices()` method, you will need to establish an `IControllerActivator`, which represents the site's _Composition Root_ for dependency injection and pass it a _connection string_. This might look like:
```
services.AddSingleton<IControllerActivator>(new OrganizationNameControllerActivator(Configuration.GetConnectionString("OnTopic"));
```
See [Controller Activator](#controller-activator) below for information on creating an `IControllerActivator` implementation.

> *Note:* The controller activator name is arbitrary, and should follow the conventions appropriate for the site. Ignia typically uses `{OrganizationName}ControlActivator` (e.g., `IgniaControllerActivator`), but OnTopic doesn't need to know or care what the name is; that is between your application and ASP.NET Core.
> *Note:* The connection string can come from any source, or even be hard-coded. Ignia recommends storing it as part of your `secrets.json` during development and as part of the hosting environment's application settings. The above code, for instance, will work with a local `secrets.json` and Azure's connection configuration.

### Route Configuration
When registering routes via `Startup.Configure()` you may register any routes for OnTopic using the extension method:
```
public class Startup {
  public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
    app.UseMvcWithDefaultRoute();
    app.UseMvc(routes => routes.MapTopicRoute("Web"));
	}
}
```
> *Note:* Because OnTopic relies on wildcard pathnames, a new route should be configured for every root namespace (e.g., `/Web`). While it's possible to configure OnTopic to evaluate _all_ paths, this makes it difficult to delegate control to other controllers and handlers, when necessary. As a result, it is recommended that each root container be registered individually.

### Controller Activator
As OnTopic relies on constructor injection, the application must be configured in a **Composition Root**—in the case of ASP.NET Core, that means a custom controller activator. The basic structure of this might look like:
```
var sqlTopicRepository          = new SqlTopicRepository(connectionString);
var cachedTopicRepository       = new CachedTopicRepository(sqlTopicRepository);
var topicViewModelLookupService = new TopicViewModelLookupService();
var topicMappingService         = new TopicMappingService(cachedTopicRepository, topicViewModelLookupService);

var mvcTopicRoutingService      = new MvcTopicRoutingService(
  cachedTopicRepository,
  new Uri($"https://{context.HttpContext.Request.Host}/{context.HttpContext.Request.Path}"),
  context.RouteData
);

switch (controllerType.Name) {

  case nameof(TopicController):
    return new TopicController(sqlTopicRepository, mvcTopicRoutingService, topicMappingService);

  case default:
    throw new Exception($"Unknown controller {controllerType.Name}");

}

```
For a complete reference template, including the ancillary controllers and a more maintainable structure, see the [`OrganizationNameControllerActivator.cs`](https://gist.github.com/JeremyCaney/00c04b1b9f40d9743793cd45dfaaa606) Gist.

> *Note:* The default `TopicController` will automatically identify the current topic (based on e.g. the URL), map the current topic to a corresponding view model (based on [the `TopicMappingService` conventions](../Ignia.Topics/Mapping/)), and then return a corresponding view (based on the [view conventions](#view-conventions)). For most applications, this is enough. If custom mapping rules or additional presentation logic are needed, however, implementors can subclass `TopicController`.
