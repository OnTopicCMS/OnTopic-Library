/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Attributes;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.ViewModels;
using Ignia.Topics.AspNetCore.Mvc;
using Ignia.Topics.AspNetCore.Mvc.Controllers;
using Ignia.Topics.AspNetCore.Mvc.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: VALIDATE TOPIC ATTRIBUTE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ValidateTopicAttribute"/>.
  /// </summary>
  [TestClass]
  public class ValidateTopicAttributeTest {

    /*==========================================================================================================================
    | METHOD: GET ACTION EXECUTING CONTEXT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Controller"/>, generates a barebones <see cref="ActionExecutingContext"/> for testing.
    /// </summary>
    public ActionExecutingContext GetActionExecutingContext(Controller controller) {

      var modelState            = new ModelStateDictionary();

      var actionContext = new ActionContext(
        new DefaultHttpContext(),
        new RouteData(),
        new ControllerActionDescriptor(),
        modelState
      );

      var actionExecutingContext = new ActionExecutingContext(
        actionContext,
        new List<IFilterMetadata>(),
        new Dictionary<string, object>(),
        controller
      );

      return actionExecutingContext;

    }

    /*==========================================================================================================================
    | METHOD: GET CONTROLLER CONTEXT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Generates a barebones <see cref="ControllerContext"/> for testing a controller.
    /// </summary>
    public ControllerContext GetControllerContext() =>
      new ControllerContext(
        new ActionContext() {
          HttpContext               = new DefaultHttpContext(),
          RouteData                 = new RouteData(),
          ActionDescriptor          = new ControllerActionDescriptor()
        }
      );

    /*==========================================================================================================================
    | METHOD: GET TOPIC CONTROLLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Generates a barebones <see cref="ControllerContext"/> for testing a controller.
    /// </summary>
    public TopicController GetTopicController(Topic? topic) =>
      new TopicController(
          new DummyTopicRepository(),
          new DummyTopicMappingService()
      ) {
        CurrentTopic            = topic,
        ControllerContext       = GetControllerContext()
      };

    /*==========================================================================================================================
    | TEST: NULL TOPIC RETURNS NOT FOUND
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [TestMethod]
    public async Task NullTopic_Returns_NotFound() {

      var validateFilter        = new ValidateTopicAttribute();
      var controller            = GetTopicController(null);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(NotFoundObjectResult), context.Result.GetType());

    }

    /*==========================================================================================================================
    | TEST: DISABLED TOPIC RETURNS NOT AUTHORIZED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [TestMethod]
    public async Task DisabledTopic_Returns_NotFound() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = TopicFactory.Create("Key", "Page");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      topic.Attributes.SetBoolean("IsDisabled", true);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(UnauthorizedResult), context.Result.GetType());

    }

    /*==========================================================================================================================
    | TEST: TOPIC WITH URL RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> contains
    ///   a <c>Url</c> attribute.
    /// </summary>
    [TestMethod]
    public async Task TopicWithUrl_Returns_Redirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = TopicFactory.Create("Key", "Page");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      topic.Attributes.SetValue("Url", "https://www.github.com/Ignia/Topic-Library");

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(RedirectResult), context.Result.GetType());

    }

    /*==========================================================================================================================
    | TEST: NESTED TOPIC RETURNS 403
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="StatusCodeResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>List</c>.
    /// </summary>
    [TestMethod]
    public async Task NestedTopic_Returns_403() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = TopicFactory.Create("Key", "List");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      var result                = context.Result as StatusCodeResult;

      Assert.IsNotNull(result);
      Assert.AreEqual(403, result.StatusCode);

    }

    /*==========================================================================================================================
    | TEST: PAGE GROUP TOPIC RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>PageGroup</c>.
    /// </summary>
    [TestMethod]
    public async Task PageGroupTopic_Returns_Redirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = TopicFactory.Create("Key", "PageGroup");
      var childTopic            = TopicFactory.Create("Home", "Page", topic);
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(RedirectResult), context.Result.GetType());

    }


  } //Class
} //Namespace