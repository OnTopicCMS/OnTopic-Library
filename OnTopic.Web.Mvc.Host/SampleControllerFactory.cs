/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       OnTopicSample OnTopic Site
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using OnTopic.Data.Caching;
using OnTopic.Data.Sql;
using OnTopic.Mapping;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Repositories;
using OnTopic.ViewModels;
using OnTopic.Web.Mvc.Controllers;
using OnTopic.Web.Mvc.Host.Controllers;

namespace OnTopic.Web.Mvc.Host {

  /*============================================================================================================================
  | CLASS: CONTROLLER FACTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Responsible for creating instances of factories in response to web requests. Represents the Composition Root for
  ///   Dependency Injection.
  /// </summary>
  class SampleControllerFactory : DefaultControllerFactory {

    /*==========================================================================================================================
    | PRIVATE INSTANCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITypeLookupService              _typeLookupService              = null;
    private readonly            ITopicMappingService            _topicMappingService            = null;
    private readonly            ITopicRepository                _topicRepository                = null;
    private readonly            Topic                           _rootTopic                      = null;

    private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalTopicMappingService = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of the <see cref="SampleControllerFactory"/>, including any shared dependencies to be used
    ///   across instances of controllers.
    /// </summary>
    public SampleControllerFactory() : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | ESTABLISH DATABASE CONNECTION
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connectionString      = ConfigurationManager.ConnectionStrings["OnTopic"].ConnectionString;
      var sqlTopicRepository    = new SqlTopicRepository(connectionString);

      /*------------------------------------------------------------------------------------------------------------------------
      | SAVE STANDARD DEPENDENCIES
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(sqlTopicRepository);
      _typeLookupService        = new TopicViewModelLookupService();
      _topicMappingService      = new TopicMappingService(_topicRepository, _typeLookupService);
      _rootTopic                = _topicRepository.Load();

      /*------------------------------------------------------------------------------------------------------------------------
      | CONSTRUCT HIERARCHICAL TOPIC MAPPING SERVICE
      \-----------------------------------------------------------------------------------------------------------------------*/
      var service = new HierarchicalTopicMappingService<NavigationTopicViewModel>(
        _topicRepository,
        _topicMappingService
      );

      _hierarchicalTopicMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
        service
      );

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
      switch (controllerType.Name) {

        case nameof(RedirectController):
          return new RedirectController(_topicRepository);

        case nameof(SitemapController):
          return new SitemapController(_topicRepository);

        case nameof(ErrorController):
          return new ErrorController();

        case nameof(LayoutController):
          return new LayoutController(mvcTopicRoutingService, _hierarchicalTopicMappingService, _topicRepository);

        case nameof(TopicController):
          return new TopicController(_topicRepository, mvcTopicRoutingService, _topicMappingService);

        default:
          return base.GetControllerInstance(requestContext, controllerType);

      }

    }

  } //Class
} //Namespace
