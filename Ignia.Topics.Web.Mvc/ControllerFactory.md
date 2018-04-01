# Controller Factory
The following is a reference Controller Factory for use with the `Ignia.Topics.Web.Mvc` assembly.

```
/*==============================================================================================================================
| Author        Ignia, LLC
| Client        OrganizationName
| Project       Website
\=============================================================================================================================*/
using System;
using System.Web.Mvc;
using System.Web.Routing;
using Ignia.Topics;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.Web;
using Ignia.Topics.Web.Mvc;

namespace OrganizationName.Web {

  /*============================================================================================================================
  | CLASS: CONTROLLER FACTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Responsible for creating instances of factories in response to web requests. Represents the Composition Root 
  ///   for Dependency Injection.
  /// </summary>
  class OrganizationNameControllerFactory : DefaultControllerFactory {

    /*==========================================================================================================================
    | PRIVATE INSTANCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicMappingService            _topicMappingService            = null;
    private readonly            ITopicRepository                _topicRepository                = null;
    private readonly            Topic                           _rootTopic                      = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of the <see cref="OrganizationNameControllerFactory"/>, including any shared  
    ///   dependencies to be used across instances of controllers.
    /// </summary>
    public OrganizationNameControllerFactory() : base() {

      var connectionString      = ConfigurationManager.ConnectionStrings["OnTopic"].ConnectionString;
      var sqlTopicRepository    = new SqlTopicRepository(connectionString);
      var cachedTopicRepository = new CachedTopicRepository(sqlTopicRepository);

      _topicRepository          = cachedTopicRepository;
      _topicMappingService      = new TopicMappingService(_topicRepository);
      _rootTopic                = cachedTopicRepository.Load();

    }

    /*==========================================================================================================================
    | GET CONTROLLER INSTANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Overrides the factory method for creating new instances of controllers.
    /// </summary>
    /// <returns>A concrete instance of an <see cref="IController"/>.</returns>
    protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Register
      \-----------------------------------------------------------------------------------------------------------------------*/
      var mvcTopicRoutingService        = new MvcTopicRoutingService(
        _topicRepository,
        requestContext.HttpContext.Request.Url,
        requestContext.RouteData
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Resolve
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (controllerType == typeof(TopicController)) {
        return new TopicController(_topicRepository, mvcTopicRoutingService, _topicMappingService);
      }

      return base.GetControllerInstance(requestContext, controllerType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Release
      \-----------------------------------------------------------------------------------------------------------------------*/
      //There are no resources to release

    }

  } //Class
} //Namespace
```