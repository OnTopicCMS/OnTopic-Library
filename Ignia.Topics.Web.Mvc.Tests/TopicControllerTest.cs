/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.ViewModels;
using Ignia.Topics.Web.Mvc;
using Ignia.Topics.Web.Mvc.Controllers;
using Ignia.Topics.Web.Mvc.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC CONTROLLER TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicController"/>, and other <see cref="Controller"/> classes that are part of
  ///   the <see cref="Ignia.Topics.Web.Mvc"/> namespace.
  /// </summary>
  [TestClass]
  public class TopicControllerTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    RouteData                       _routeData                      = new RouteData();
    readonly                    ITopicRepository                _topicRepository;
    readonly                    Uri                             _uri;
    readonly                    Topic                           _topic;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicControllerTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph. In addition, it initializes a shared <see cref="Topic"/> reference to use for the various
    ///   tests.
    /// </remarks>
    public TopicControllerTest() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(new FakeTopicRepository());
      _uri                      = new Uri("http://localhost/Web/Web_0/Web_0_1/Web_0_1_1");
      _topic                    = _topicRepository.Load("Root" + _uri.PathAndQuery.Replace("/", ":"))!;

    }

    /*==========================================================================================================================
    | TEST: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="TopicController.IndexAsync(String)" /> action.
    /// </summary>
    [TestMethod]
    public async Task TopicController_IndexAsync() {

      var topicRoutingService   = new MvcTopicRoutingService(_topicRepository, _uri, _routeData);
      var mappingService        = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());

      var controller            = new TopicController(_topicRepository, topicRoutingService, mappingService);
      var result                = await controller.IndexAsync(_topic.GetWebPath()).ConfigureAwait(false) as TopicViewResult;
      var model                 = result.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("Web_0_1_1", model.Title);

    }

    /*==========================================================================================================================
    | TEST: ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorControllerBase{T}.Index(String)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_Index() {

      var controller            = new ErrorController();
      var result                = controller.Index("ErrorPage") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("ErrorPage", model.Title);

    }

    /*==========================================================================================================================
    | TEST: NOT FOUND ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorControllerBase{T}.NotFound(String)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_NotFound() {

      var controller            = new ErrorController();
      var result                = controller.NotFound("NotFoundPage") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("NotFoundPage", model.Title);

    }

    /*==========================================================================================================================
    | TEST: INTERNAL SERVER ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorControllerBase{T}.InternalServer(String)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_InternalServer() {

      var controller            = new ErrorController();
      var result                = controller.InternalServer("InternalServer") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("InternalServer", model.Title);

    }

    /*==========================================================================================================================
    | TEST: FALLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="FallbackController.Index()" /> action.
    /// </summary>
    [TestMethod]
    public void FallbackController_Index() {

      var controller            = new FallbackController();
      var result                = controller.Index() as HttpNotFoundResult;

      controller.Dispose();

      Assert.IsNotNull(result);
      Assert.AreEqual<int>(404, result.StatusCode);
      Assert.AreEqual<string>("No controller available to handle this request.", result.StatusDescription);

    }

    /*==========================================================================================================================
    | TEST: REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="FallbackController.Index()" /> action.
    /// </summary>
    [TestMethod]
    public void RedirectController_TopicRedirect() {

      var controller            = new RedirectController(_topicRepository);
      var result                = controller.Redirect(11110) as RedirectResult;

      controller.Dispose();

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Permanent);
      Assert.AreEqual<string>("/Web/Web_1/Web_1_1/Web_1_1_1/", result.Url);

    }

    /*==========================================================================================================================
    | TEST: SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index()" /> action.
    /// </summary>
    /// <remarks>
    ///   Because the <see cref="SitemapController.Index()"/> method references the <see cref="Controller.Response"/> property,
    ///   which is not set during unit testing, this test is <i>expected</i> to throw an exception. This is not ideal. In the
    ///   future, this may be modified to instead use a mock <see cref="ControllerContext"/> for a more sophisticated test.
    /// </remarks>
    [TestMethod]
    [ExpectedException(typeof(NullReferenceException), AllowDerivedTypes=false)]
    public void SitemapController_Index() {

      var controller            = new SitemapController(_topicRepository);
      var result                = controller.Index() as ViewResult;
      var model                 = result.Model as TopicEntityViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<ITopicRepository>(_topicRepository, model.TopicRepository);
      Assert.AreEqual<string>("Root", model.RootTopic.Key);
      Assert.AreEqual<string>("Root", model.Topic.Key);

    }

    /*==========================================================================================================================
    | TEST: MENU
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="FallbackController.Index()" /> action.
    /// </summary>
    [TestMethod]
    public async Task Menu() {

      var topicRoutingService   = new MvcTopicRoutingService(_topicRepository, _uri, _routeData);
      var mappingService        = new HierarchicalTopicMappingService<NavigationTopicViewModel>(
        _topicRepository,
        new TopicMappingService(
          _topicRepository,
          new TopicViewModelLookupService()
        )
      );

      var controller            = new LayoutController(topicRoutingService, mappingService);
      var result                = await controller.Menu().ConfigureAwait(false) as PartialViewResult;
      var model                 = result.Model as NavigationViewModel<NavigationTopicViewModel>;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>(_topic.GetUniqueKey(), model.CurrentKey);
      Assert.AreEqual<string>("Root:Web", model.NavigationRoot.UniqueKey);
      Assert.AreEqual<int>(2, model.NavigationRoot.Children.Count());
      Assert.IsTrue(model.NavigationRoot.IsSelected(_topic.GetUniqueKey()));

    }

  } //Class
} //Namespace