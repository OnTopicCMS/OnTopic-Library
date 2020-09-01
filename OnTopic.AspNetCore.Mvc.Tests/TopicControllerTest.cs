/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.ViewModels;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC CONTROLLER TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicController"/>, and other <see cref="Controller"/> classes that are part of
  ///   the <see cref="OnTopic.Web.Mvc"/> namespace.
  /// </summary>
  [TestClass]
  public class TopicControllerTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _topicMappingService;
    readonly                    Topic                           _topic;
    readonly                    ControllerContext               _context;

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
    public TopicControllerTest() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(new StubTopicRepository());
      _topic                    = _topicRepository.Load("Root:Web:Web_0:Web_0_1:Web_0_1_1")!;
      _topicMappingService      = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish view model context
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routes                = new RouteData();

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_1");

      var actionContext         = new ActionContext {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = routes,
        ActionDescriptor        = new ControllerActionDescriptor()
      };
      _context                  = new ControllerContext(actionContext);

    }

    /*==========================================================================================================================
    | TEST: TOPIC CONTROLLER: INDEX ASYNC: RETURNS TOPIC VIEW RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="TopicController.IndexAsync(String)" /> action.
    /// </summary>
    [TestMethod]
    public async Task TopicController_IndexAsync_ReturnsTopicViewResult() {

      var controller            = new TopicController(_topicRepository, _topicMappingService) {
        ControllerContext       = _context
      };

      var result                = await controller.IndexAsync(_topic.GetWebPath()).ConfigureAwait(false) as TopicViewResult;
      var model                 = result.Model as PageTopicViewModel;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("Web_0_1_1", model.Title);

    }

    /*==========================================================================================================================
    | TEST: REDIRECT CONTROLLER: REDIRECT: RETURNS REDIRECT RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="FallbackController.Index()" /> action.
    /// </summary>
    [TestMethod]
    public void RedirectController_TopicRedirect_ReturnsRedirectResult() {

      var controller            = new RedirectController(_topicRepository);
      var result                = controller.Redirect(11110) as RedirectResult;

      controller.Dispose();

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Permanent);
      Assert.AreEqual<string>("/Web/Web_1/Web_1_1/Web_1_1_1/", result.Url);

    }

    /*==========================================================================================================================
    | TEST: SITEMAP CONTROLLER: INDEX: RETURNS SITEMAP XML
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the index action of the <see cref="SitemapController.Index()" /> action.
    /// </summary>
    [TestMethod]
    public void SitemapController_Index_ReturnsSitemapXml() {

      var actionContext         = new ActionContext {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = new RouteData(),
        ActionDescriptor        = new ControllerActionDescriptor()
      };
      var controller            = new SitemapController(_topicRepository) {
        ControllerContext       = new ControllerContext(actionContext)
      };
      var result                = controller.Index() as ContentResult;
      var model                 = result.Content as string;

      controller.Dispose();

      Assert.IsNotNull(model);
      Assert.IsTrue(model.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>"));
      Assert.IsTrue(model.Contains("/Web/Web_1/Web_1_1/Web_1_1_1/</loc>"));

    }

  } //Class
} //Namespace