# `Ignia.Topics.Web.Mvc`
The `Ignia.Topics.Web.Mvc` assembly provides a default implementation for utilizing OnTopic with the ASP.NET MVC 5.x Framework. 

### Contents
- [Components](#components)
- [View Conventions](#view-conventions)
  - [View Matching](#view-matching)
  - [View Locations](#view-locations)
  - [Example](#example)
- [Configuration](#configuration)
  - [Application](#application)
  - [Route Configuration](#route-configuration)
  - [Controller Factory](#controller-factory)

## Components
There are three key components at the heart of the MVC implementation.
- **`MvcTopicRoutingService`**: This is a concrete implementation of the `ITopicRoutingService` which accepts contextual information about a given request (in this case, the URL and routing data) and then uses it to retrieve the current `Topic` from an `ITopicRepository`.
- **`TopicController`**: This is a default controller instance that can be used for _any_ topic path. It will automatically validate that the `Topic` exists, that it is not disabled (`IsDisabled`), and will honor any redirects (e.g., if the `Url` attribute is filled out). Otherwise, it will return `TopicViewResult` based on a view model, view name, and content type.
- **`TopicViewEngine`**: The `TopicViewEngine` is called every time a view is requested. It works in conjunction with `TopicViewResult` to identify matching MVC views based on predetermined locations and conventions. These are discussed below.

## View Conventions
By default, OnTopic matches views based on the current topic's `ContentType` and, if available, `View`. 

### View Matching
There are multiple ways for a view to be set. The `TopicViewResult` will automatically evaluate views based on the following locations. The first one to match a valid view name is selected.
- **`?View=`** query string parameter (e.g., `?View=Accordion`)
- **`Accept`** headers (e.g., `Accept=application/json`); will treat the segment after the `/` as a possible view name
- **`View`** attribute (i.e., `topic.View`)
- **`ContentType`** attribute (i.e., `topic.ContentType`)

### View Locations
For each of the above [View Matching](#view-matching) rules, the `TopicViewEngine` will search the following locations for a matching view:
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
In the `global.asax.cs`, the following components should be registered under the `Application_Start` event handler:
```
ControllerBuilder.Current.SetControllerFactory(new OrganizationNameControllerFactory());
ViewEngines.Engines.Insert(0, new TopicViewEngine());
```

### Route Configuration
When registering routes via `RouteConfig.RegisterRoutes()` (typically via the `RouteConfig` class), register a route for any OnTopic routes:
```
routes.MapRoute(
  name: "WebTopics",
  url: "Web/{*path}",
  defaults: new { controller = "Topic", action = "Index", id = UrlParameter.Optional, rootTopic = "Web" }
);
```
> *Note:* Because OnTopic relies on wildcard pathnames, a new route should be configured for every root namespace (e.g., `/Web`). While it's possible to configure OnTopic to evaluate _all_ paths, this makes it difficult to delegate control to other controllers and handlers, when necessary.

### Controller Factory
As OnTopic relies on constructor injection, the application must be configured in a **Composition Root**—in the case of ASP.NET MVC, that means a custom controller factory. This might look like, for exmample:
```
namespace OrganizationName.Web {
 
 class OrganizationNameControllerFactory : DefaultControllerFactory {

    static readonly ITopicRepository _topicRepository = null;

    protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType) {

      /*----------------------------------------------------------------------------------------------------------
      | Register
      \---------------------------------------------------------------------------------------------------------*/
      if (_topicRepository == null) {
        var connectionString            = ConfigurationManager.ConnectionStrings["OnTopic"].ConnectionString;
        var sqlTopicRepository          = new SqlTopicRepository(connectionString);
        var cachedTopicRepository       = new CachedTopicRepository(sqlTopicRepository);
        _topicRepository                = cachedTopicRepository;
      }
      
      var mvcTopicRoutingService        = new MvcTopicRoutingService(
        _topicRepository,
        requestContext.HttpContext.Request.Url,
        requestContext.RouteData
      );

      var topicMappingService           = new TopicMappingService(_topicRepository);

      /*----------------------------------------------------------------------------------------------------------
      | Resolve
      \---------------------------------------------------------------------------------------------------------*/
      if (controllerType == typeof(TopicController)) {
        return new TopicController(_topicRepository, mvcTopicRoutingService, topicMappingService);
      }

      return base.GetControllerInstance(requestContext, controllerType);

      /*----------------------------------------------------------------------------------------------------------
      | Release
      \---------------------------------------------------------------------------------------------------------*/
      // There are no resources to release

    }

  } // Class
} //Namespace
```
> *Note:* The default `TopicController` will automatically identify the current topic (based on e.g. the URL), map the current topic to a corresponding view model (based on [the `TopicMappingService` conventions](../Ignia.Topics/Mapping/)), and then return a corresponding view (based on the [view conventions](#view-conventions)). For most applications, this is enough. If custom mapping rules or additional presentation logic are needed, however, implementors can subclass `TopicController`. 


