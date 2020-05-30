/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Sample OnTopic Site
\=============================================================================================================================*/
using System;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.AspNetCore.Mvc.Host.Components;
using OnTopic.Data.Caching;
using OnTopic.Data.Sql;
using OnTopic.Mapping;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Reflection;
using OnTopic.Repositories;
using OnTopic.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace OnTopic.AspNetCore.Mvc.Host {

  /*============================================================================================================================
  | CLASS: SAMPLE ACTIVATOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Responsible for creating instances of factories in response to web requests. Represents the Composition Root for
  ///   Dependency Injection.
  /// </summary>
  public class SampleActivator : IControllerActivator, IViewComponentActivator {

    /*==========================================================================================================================
    | PRIVATE INSTANCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITypeLookupService              _typeLookupService              = null;
    private readonly            ITopicMappingService            _topicMappingService            = null;
    private readonly            ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalMappingService = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of the <see cref="SampleControllerFactory"/>, including any shared dependencies to be used
    ///   across instances of controllers.
    /// </summary>
    /// <remarks>
    ///   The constructor is responsible for establishing dependencies with the singleton lifestyle so that they are available
    ///   to all requests.
    /// </remarks>
    public SampleActivator(string connectionString) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize Topic Repository
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       sqlTopicRepository              = new SqlTopicRepository(connectionString);
      var                       cachedTopicRepository           = new CachedTopicRepository(sqlTopicRepository);
      _                                                         = new PageTopicViewModel();

      /*------------------------------------------------------------------------------------------------------------------------
      | Preload repository
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository                                          = cachedTopicRepository;
      _typeLookupService                                        = new DynamicTopicViewModelLookupService();
      _topicMappingService                                      = new TopicMappingService(_topicRepository, _typeLookupService);
      _                                                         = _topicRepository.Load();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish hierarchical topic mapping service
      \-----------------------------------------------------------------------------------------------------------------------*/
      _hierarchicalMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
        new HierarchicalTopicMappingService<NavigationTopicViewModel>(
          _topicRepository,
          _topicMappingService
        )
      );

    }

    /*==========================================================================================================================
    | METHOD: CREATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Registers dependencies, and injects them into new instances of controllers in response to each request.
    /// </summary>
    /// <returns>A concrete instance of an <see cref="IController"/>.</returns>
    public object Create(ControllerContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine controller type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var type = context.ActionDescriptor.ControllerTypeInfo.AsType();

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure and return appropriate controller
      \-----------------------------------------------------------------------------------------------------------------------*/
      return type.Name switch {
        nameof(TopicController) =>
          new TopicController(_topicRepository, _topicMappingService),
        nameof(SitemapController) =>
          new SitemapController(_topicRepository),
        _ => throw new Exception($"Unknown controller {type.Name}")
      };

    }

    /// <summary>
    ///   Registers dependencies, and injects them into new instances of view components in response to each request.
    /// </summary>
    /// <returns>A concrete instance of an <see cref="IController"/>.</returns>
    public object Create(ViewComponentContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine view component type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var type = context.ViewComponentDescriptor.TypeInfo.AsType();

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure and return appropriate view component
      \-----------------------------------------------------------------------------------------------------------------------*/
      return type.Name switch {
        nameof(MenuViewComponent) =>
          new MenuViewComponent(_topicRepository, _hierarchicalMappingService),
        nameof(PageLevelNavigationViewComponent) =>
          new PageLevelNavigationViewComponent(_topicRepository, _hierarchicalMappingService),
        _ => throw new Exception($"Unknown view component {type.Name}")
      };

    }

    /*==========================================================================================================================
    | METHOD: RELEASE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Responds to a request to release resources associated with a particular controller.
    /// </summary>
    public void Release(ControllerContext context, object controller) { }

    /// <summary>
    ///   Responds to a request to release resources associated with a particular view component.
    /// </summary>
    public void Release(ViewComponentContext context, object viewComponent) { }

  } //Class
} //Namespace