/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Attributes;
using OnTopic.Metadata;
using OnTopic.TestDoubles;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: VALIDATE TOPIC ATTRIBUTE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ValidateTopicAttribute"/>.
  /// </summary>
  [ExcludeFromCodeCoverage]
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
    #pragma warning disable CA1024 // Use properties where appropriate
    public static ControllerContext GetControllerContext() =>
      new(
        new() {
          HttpContext               = new DefaultHttpContext(),
          RouteData                 = new(),
          ActionDescriptor          = new ControllerActionDescriptor()
        }
      );
    #pragma warning restore CA1024 // Use properties where appropriate

    /*==========================================================================================================================
    | METHOD: GET TOPIC CONTROLLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Generates a barebones <see cref="ControllerContext"/> for testing a controller.
    /// </summary>
    public static TopicController GetTopicController(Topic? topic) =>
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
    [Fact]
    public void InvalidControllerType_ThrowsException() {

      using var controller      = new DummyController() {
        ControllerContext       = GetControllerContext()
      };

      var validateFilter        = new ValidateTopicAttribute();
      var actionContext         = GetActionExecutingContext(controller);

      Assert.Throws<InvalidOperationException>(() =>
        validateFilter.OnActionExecuting(actionContext)
      );

    }

    /*==========================================================================================================================
    | TEST: NULL TOPIC: RETURNS NOT FOUND
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [Fact]
    public void NullTopic_ReturnsNotFound() {

      var validateFilter        = new ValidateTopicAttribute();
      var controller            = GetTopicController(null);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.IsType<NotFoundObjectResult>(context.Result);

    }

    /*==========================================================================================================================
    | TEST: DISABLED TOPIC: RETURNS NOT AUTHORIZED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="NotFoundObjectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> is null.
    /// </summary>
    [Fact]
    public void DisabledTopic_ReturnsNotFound() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Key", "Page");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      topic.Attributes.SetBoolean("IsDisabled", true);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.IsType<UnauthorizedResult>(context.Result);

    }

    /*==========================================================================================================================
    | TEST: TOPIC WITH URL: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> contains
    ///   a <c>Url</c> attribute.
    /// </summary>
    [Fact]
    public void TopicWithUrl_ReturnsRedirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Key", "Page");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      topic.Attributes.SetValue("Url", "https://www.github.com/Ignia/Topic-Library");

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.IsType<RedirectResult>(context.Result);

    }

    /*==========================================================================================================================
    | TEST: NESTED TOPIC: LIST: RETURNS 403
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="StatusCodeResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>List</c>.
    /// </summary>
    [Fact]
    public void NestedTopic_List_Returns403() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Key", "List");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      var result                = context.Result as StatusCodeResult;

      Assert.NotNull(result);
      Assert.Equal<int?>(403, result?.StatusCode);

    }

    /*==========================================================================================================================
    | TEST: NESTED TOPIC: ITEM: RETURNS 403
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="StatusCodeResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   parent <see cref="Topic"/> with a <see cref="ContentTypeDescriptor"/> of <c>List</c>.
    /// </summary>
    [Fact]
    public void NestedTopic_Item_Returns403() {

      var validateFilter        = new ValidateTopicAttribute();
      var list                  = new Topic("Key", "List");
      var topic                 = new Topic("Item", "Page", list);
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      var result                = context.Result as StatusCodeResult;

      Assert.NotNull(result);
      Assert.Equal<int?>(403, result?.StatusCode);

    }

    /*==========================================================================================================================
    | TEST: NESTED TOPIC: ITEM: RETURNS 403
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="StatusCodeResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   parent <see cref="Topic"/> with a <see cref="ContentTypeDescriptor"/> of <c>List</c>.
    /// </summary>
    [Fact]
    public void Container_Returns403() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Item", "Container");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      var result                = context.Result as StatusCodeResult;

      Assert.NotNull(result);
      Assert.Equal<int?>(403, result?.StatusCode);

    }

    /*==========================================================================================================================
    | TEST: PAGE GROUP TOPIC: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>PageGroup</c>.
    /// </summary>
    [Fact]
    public void PageGroupTopic_ReturnsRedirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Key", "PageGroup");
      var child                 = new Topic("Child", "Page", topic);
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);
      _                         = new Topic("Home", "Page", topic);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.IsType<RedirectResult>(context.Result);
      Assert.Equal(child.GetWebPath(), ((RedirectResult?)context.Result)?.Url);

    }

    /*==========================================================================================================================
    | TEST: PAGE GROUP TOPIC: EMPTY: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="ContentTypeDescriptor"/> of <c>PageGroup</c> with no <see cref="Topic.Children"/>.
    /// </summary>
    [Fact]
    public void PageGroupTopic_Empty_ReturnsRedirect() {

      var validateFilter = new ValidateTopicAttribute();
      var topic = new Topic("Key", "PageGroup");
      var controller = GetTopicController(topic);
      var context = GetActionExecutingContext(controller);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      var result = context.Result as StatusCodeResult;

      Assert.NotNull(result);
      Assert.Equal<int?>(403, result?.StatusCode);

    }

    /*==========================================================================================================================
    | TEST: CANONICAL URL: RETURNS REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="RedirectResult"/> is thrown if the <see cref="TopicController.CurrentTopic"/> has a
    ///   <see cref="Topic.GetWebPath()"/> which doesn't match the current URL, even just by case.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GetActionExecutingContext(Controller)"/> defines a <see cref="DefaultHttpContext"/>, which doesn't
    ///   define a <see cref="HttpRequest.Path"/>. As a result, if no other condition is met, the canonical condition should
    ///   always be tripped as part of these unit tests.
    /// </remarks>
    [Fact]
    public void CanonicalUrl_ReturnsRedirect() {

      var validateFilter        = new ValidateTopicAttribute();
      var topic                 = new Topic("Key", "Page");
      var controller            = GetTopicController(topic);
      var context               = GetActionExecutingContext(controller);
      _                         = new Topic("Home", "Page", topic);

      validateFilter.OnActionExecuting(context);

      controller.Dispose();

      Assert.IsType<RedirectResult>(context.Result);

    }

  } //Class
} //Namespace