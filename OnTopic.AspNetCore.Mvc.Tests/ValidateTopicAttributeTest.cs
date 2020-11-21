/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Attributes;
using OnTopic.TestDoubles;

namespace OnTopic.Tests {

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
    public static ActionExecutingContext GetActionExecutingContext(Controller controller) {

      var modelState            = new ModelStateDictionary();

      var actionContext = new ActionContext(
        new DefaultHttpContext(),
        new(),
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
    public static ControllerContext GetControllerContext() =>
      new(
        new() {
          HttpContext               = new DefaultHttpContext(),
          RouteData                 = new(),
          ActionDescriptor          = new ControllerActionDescriptor()
        }
      );

    /*==========================================================================================================================
    | METHOD: GET TOPIC CONTROLLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Generates a barebones <see cref="ControllerContext"/> for testing a controller.
    /// </summary>
    public static TopicController GetTopicController(Topic topic) =>
      new(
          new DummyTopicRepository(),
          new DummyTopicMappingService()
      ) {
        CurrentTopic            = topic,
        ControllerContext       = GetControllerContext()
      };

    /*==========================================================================================================================
    | TEST: INVALID CONTROLLER TYPE THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a controller that doesn't derive from <see cref="TopicController"/> throws a <see
    ///   cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void InvalidControllerType_ThrowsException() {

      var validateFilter        = new ValidateTopicAttribute();
      var controller            = new DummyController() {
        ControllerContext       = GetControllerContext()
      };
      var context               = GetActionExecutingContext(controller);

      try {
        validateFilter.OnActionExecuting(context);
      }
      finally {
        controller.Dispose();
      }

    }
    /*==========================================================================================================================
    | TEST: NULL TOPIC: RETURNS NOT FOUND
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [TestMethod]
    public void NullTopic_ReturnsNotFound() {

      var validateFilter        = new ValidateTopicAttribute();
      var controller            = GetTopicController(null);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(NotFoundObjectResult), context.Result.GetType());

    }

    /*==========================================================================================================================
    | TEST: DISABLED TOPIC: RETURNS NOT AUTHORIZED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [TestMethod]
    public void DisabledTopic_ReturnsNotFound() {

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
    | TEST: TOPIC WITH URL: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> contains
    ///   a <c>Url</c> attribute.
    /// </summary>
    [TestMethod]
    public void TopicWithUrl_ReturnsRedirect() {

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
    | TEST: NESTED TOPIC: RETURNS 403
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="StatusCodeResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>List</c>.
    /// </summary>
    [TestMethod]
    public void NestedTopic_Returns403() {

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
    | TEST: PAGE GROUP TOPIC: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>PageGroup</c>.
    /// </summary>
    [TestMethod]
    public void PageGroupTopic_ReturnsRedirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = TopicFactory.Create("Key", "PageGroup");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      TopicFactory.Create("Home", "Page", topic);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.AreEqual(typeof(RedirectResult), context.Result.GetType());

    }

  } //Class
} //Namespace