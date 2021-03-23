/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Routing;
using OnTopic.AspNetCore.Mvc.Components;
using OnTopic.AspNetCore.Mvc.Host.Components;
using OnTopic.AspNetCore.Mvc.Models;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW COMPONENT TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="NavigationTopicViewComponentBase{T}"/>, and derived classes that are part of
  ///   the <see cref="OnTopic.AspNetCore.Mvc"/> namespace.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
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
    private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalMappingService;

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
      _context                  = GetViewComponentContext(_topic.GetWebPath());

    }

    /*==========================================================================================================================
    | METHOD: GET VIEW COMPONENT CONTEXT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ViewComponentContext"/> based on a given <paramref name="webPath"/>.
    /// </summary>
    private static ViewComponentContext GetViewComponentContext(string webPath) {

      var routes                = new RouteData();

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", webPath);

      var viewContext           = new ViewContext() {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = routes
      };
      var viewComponentContext  = new ViewComponentContext() {
        ViewContext             = viewContext
      };

      return viewComponentContext;

    }

    /*==========================================================================================================================
    | TEST: MENU: INVOKE: RETURNS NAVIGATION VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="MenuViewComponent"/> and confirms the resulting values.
    /// </summary>
    [TestMethod]
    public async Task Menu_Invoke_ReturnsNavigationViewModel() {

      var viewComponent         = new MenuViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = _context
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult?.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string?>(_topic.GetWebPath(), model?.CurrentWebPath);
      Assert.AreEqual<string?>("/Web/", model?.NavigationRoot?.WebPath);
      Assert.AreEqual<int?>(3, model?.NavigationRoot?.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MENU: INVOKE: RETURNS CONFIGURED NAVIGATION ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="MenuViewComponent"/> with a context defining an alternate <c>NavigationRoot</c>, and confirms
    ///   that is returned as the <see cref="NavigationTopicViewModel"/>.
    /// </summary>
    [TestMethod]
    public async Task Menu_Invoke_ReturnsConfiguredNavigationRoot() {

      var webPath               = "/Web/Web_3/Web_3_1/Web_3_1_0/";
      var viewComponent         = new MenuViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = GetViewComponentContext(webPath)
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult?.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string?>(webPath, model?.CurrentWebPath);
      Assert.AreEqual<string?>("/Configuration/", model?.NavigationRoot?.WebPath);

    }

    /*==========================================================================================================================
    | TEST: NAVIGATION TOPIC VIEW MODEL: IS SELECTED: RETURNS EXPECTED OUTPUT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="NavigationTopicViewModel"/> with a child instance, and ensures that the <see cref="
    ///   NavigationTopicViewModel.IsSelected(String)"/> method returns the expected results.
    /// </summary>
    [TestMethod]
    public void NavigationTopicViewModel_IsSelected_ReturnsExpectedOutput() {

      var parent                = new NavigationTopicViewModel() {
        WebPath                 = "/Web/"
      };
      var child                 = new NavigationTopicViewModel() {
        WebPath                 = parent.WebPath + "Path/"
      };
      parent.Children.Add(child);

      Assert.IsTrue(child.IsSelected(child.WebPath));
      Assert.IsTrue(parent.IsSelected(child.WebPath));
      Assert.IsFalse(child.IsSelected(parent.WebPath));

    }

    /*==========================================================================================================================
    | TEST: PAGE-LEVEL NAVIGATION: INVOKE: RETURNS NAVIGATION VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="PageLevelNavigationViewComponent"/> and confirms the resulting values.
    /// </summary>
    [TestMethod]
    public async Task PageLevelNavigation_Invoke_ReturnsNavigationViewModel() {

      var viewComponent         = new PageLevelNavigationViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = _context
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult?.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string?>(_topic.GetWebPath(), model?.CurrentWebPath);
      Assert.AreEqual<string?>("/Web/Web_3/", model?.NavigationRoot?.WebPath);
      Assert.AreEqual<int?>(2, model?.NavigationRoot?.Children.Count);
      Assert.IsTrue(model?.NavigationRoot?.IsSelected(_topic.GetWebPath())?? false);

    }

    /*==========================================================================================================================
    | TEST: PAGE-LEVEL NAVIGATION: INVOKE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="PageLevelNavigationViewComponent"/> with a root that does not derive from a <c>PageGroup</c>
    ///   and confirms the resulting <see cref="NavigationTopicViewModel"/> is <c>null</c>.
    /// </summary>
    [TestMethod]
    public async Task PageLevelNavigation_Invoke_ReturnsNull() {

      var webPath               = "/Web/Web_1/Web_1_0/";

      var viewComponent         = new PageLevelNavigationViewComponent(_topicRepository, _hierarchicalMappingService) {
        ViewComponentContext    = GetViewComponentContext(webPath)
      };

      var result                = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult        = result as ViewViewComponentResult;
      var model                 = concreteResult?.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string?>(webPath, model?.CurrentWebPath);
      Assert.IsNull(model?.NavigationRoot);

    }

    /*==========================================================================================================================
    | TEST: PAGE-LEVEL NAVIGATION: INVOKE WITH NULL TOPIC: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads a new <see cref="PageLevelNavigationViewComponent"/> with a null topic reference and confirms the resulting <see
    ///   cref="NavigationTopicViewModel"/> is <c>null</c>. This occurs when handling 404 errors.
    /// </summary>
    [TestMethod]
    public async Task PageLevelNavigation_InvokeWithNullTopic_ReturnsNull()
    {

      var webPath = "/Invalid/Path/";

      var viewComponent = new PageLevelNavigationViewComponent(_topicRepository, _hierarchicalMappingService)
      {
        ViewComponentContext = GetViewComponentContext(webPath)
      };

      var result = await viewComponent.InvokeAsync().ConfigureAwait(false);
      var concreteResult = result as ViewViewComponentResult;
      var model = concreteResult?.ViewData.Model as NavigationViewModel<NavigationTopicViewModel>;

      Assert.IsNotNull(model);
      Assert.AreEqual<string?>(String.Empty, model?.CurrentWebPath);
      Assert.IsNull(model?.NavigationRoot);

    }

  } //Class
} //Namespace