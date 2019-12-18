/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.TestDoubles;
using Ignia.Topics.ViewModels;
using Ignia.Topics.AspNetCore.Mvc;
using Ignia.Topics.AspNetCore.Mvc.Controllers;
using Ignia.Topics.AspNetCore.Mvc.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.AspNetCore.Mvc.Host.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW COMPONENT TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="NavigationTopicViewComponentBase{T}"/>, and derived classes that are part of
  ///   the <see cref="Ignia.Topics.AspNetCore.Mvc"/> namespace.
  /// </summary>
  [TestClass]
  public class TopicViewComponentTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _topicMappingService;
    readonly                    Topic                           _topic;
    readonly                    ViewComponentContext            _context;

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalMappingService = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicControllerTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph. In addition, it initializes a shared <see cref="Topic"/> reference to use for the various
    ///   tests.
    /// </remarks>
    public TopicViewComponentTest() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(new StubTopicRepository());
      _topic                    = _topicRepository.Load("Root:Web:Web_3:Web_3_0")!;
      _topicMappingService      = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish hierarchical topic mapping service
      \-----------------------------------------------------------------------------------------------------------------------*/
      _hierarchicalMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
        new HierarchicalTopicMappingService<NavigationTopicViewModel>(
          _topicRepository,
          _topicMappingService
        )
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish view model context
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routes                = new RouteData();

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_3/Web_3_0");

      var viewContext           = new ViewContext() {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = routes
      };
      var viewComponentContext  = new ViewComponentContext() {
        ViewContext             = viewContext
      };

      _context                  = viewComponentContext;

    }

    /*==========================================================================================================================
    | TEST: MENU
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="MenuViewComponent"/> and confirms the resulting values.
    /// </summary>
    [TestMethod]
    public async Task Menu() {

      var viewComponent         = new MenuViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = _context
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string>(_topic.GetUniqueKey(), model.CurrentKey);
      Assert.AreEqual<string>("Root:Web", model.NavigationRoot.UniqueKey);
      Assert.AreEqual<int>(3, model.NavigationRoot.Children.Count());
      Assert.IsTrue(model.NavigationRoot.IsSelected(_topic.GetUniqueKey()));

    }

    /*==========================================================================================================================
    | TEST: PAGE-LEVEL NAVIGATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="PageLevelNavigationViewComponent"/> and confirms the resulting values.
    /// </summary>
    [TestMethod]
    public async Task PageLevelNavigation() {

      var viewComponent         = new PageLevelNavigationViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = _context
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string>(_topic.GetUniqueKey(), model.CurrentKey);
      Assert.AreEqual<string>("Root:Web:Web_3", model.NavigationRoot.UniqueKey);
      Assert.AreEqual<int>(2, model.NavigationRoot.Children.Count());
      Assert.IsTrue(model.NavigationRoot.IsSelected(_topic.GetUniqueKey()));

    }

  } //Class
} //Namespace