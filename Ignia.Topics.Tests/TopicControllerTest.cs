/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ignia.Topics.Querying;
using Ignia.Topics.Web.Mvc;
using Ignia.Topics.Repositories;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.Web.Mvc.Controllers;
using Ignia.Topics.ViewModels;
using System.Web.Mvc;

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
    ITopicRepository            _topicRepository                = null;

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
    ///   crawling the object graph.
    /// </remarks>
    public TopicControllerTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorController{T}.Error(string)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_Error() {

      var controller            = new ErrorController<PageTopicViewModel>();
      var result                = controller.Error("ErrorPage") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("ErrorPage", model.Title);

    }

    /*==========================================================================================================================
    | TEST: NOT FOUND ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorController{T}.NotFound(string)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_NotFound() {

      var controller            = new ErrorController<PageTopicViewModel>();
      var result                = controller.Error("NotFoundPage") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

      Assert.IsNotNull(model);
      Assert.AreEqual<string>("NotFoundPage", model.Title);

    }

    /*==========================================================================================================================
    | TEST: INTERNAL SERVER ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Triggers the <see cref="ErrorController{T}.InternalServer(string)" /> action.
    /// </summary>
    [TestMethod]
    public void ErrorController_InternalServer() {

      var controller            = new ErrorController<PageTopicViewModel>();
      var result                = controller.Error("InternalServer") as ViewResult;
      var model                 = result.Model as PageTopicViewModel;

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
      var result                = controller.TopicRedirect(11110) as RedirectResult;

      Assert.IsNotNull(result);
      Assert.IsTrue(result.Permanent);
      Assert.AreEqual<string>("/Web/Web_1/Web_1_1/Web_1_1_1/", result.Url);

    }

  } //Class

} //Namespace
