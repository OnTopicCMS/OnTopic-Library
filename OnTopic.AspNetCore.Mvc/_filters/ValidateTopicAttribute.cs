﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc.Filters;
using OnTopic.AspNetCore.Mvc.Controllers;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: VALIDATE TOPIC FILTER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   When applied to an action on a <see cref="TopicController"/>—or derived class—will validate the topic prior to further
  ///   execution.
  /// </summary>
  /// <remarks>
  ///   Not all topics are appropriate to display in a view. If the topic isn't in the repository, the action should return a
  ///   <see cref="NotFoundResult"/>. If the topic is marked as <see cref="Topic.IsDisabled"/>, then the action should return a
  ///   <see cref="UnauthorizedResult"/>. If the topic contains a <c>Url</c> attribute, then the action should return a <see
  ///   cref="RedirectResult"/>. All of this logic can be enforced by adding the <see cref="ValidateTopicAttribute"/> to an
  ///   action.
  /// </remarks>
  [AttributeUsage(AttributeTargets.Method)]
  public sealed class ValidateTopicAttribute : ActionFilterAttribute {

    /*==========================================================================================================================
    | PROPERTY: ALLOW NULL?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not the action requires a current topic.
    /// </summary>
    /// <remarks>
    ///   If <see cref="AllowNull"/> is set to <c>false</c> (the default) then a <see cref="NotFoundResult"/> is returned if
    ///   the <see cref="TopicController.CurrentTopic"/> cannot be resolved.
    /// </remarks>
    public bool AllowNull { get; set; }

    /*==========================================================================================================================
    | EVENT: ON ACTION EXECUTING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides universal validation of calls to any action on <see cref="TopicController"/> or its derivatives.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="OnActionExecuting(ActionExecutingContext)"/> event can be used to provide a wide variety of
    ///   filters, this specific implementation is focused on validating the state of the <see cref="TopicController.
    ///   CurrentTopic"/>. Namely, it will provide error handling (if the <see cref="TopicController.CurrentTopic"/> is null), a
    ///   redirect (if the <see cref="TopicController.CurrentTopic"/>'s <c>Url</c> attribute is set, and an unauthorized
    ///   response (if the <see cref="TopicController.CurrentTopic"/>'s <see cref="Topic.IsDisabled"/> flag is set.
    /// </remarks>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    [NonAction]
    public override void OnActionExecuting(ActionExecutingContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(context, nameof(context));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate controller
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (context.Controller is not TopicController controller) {
        throw new InvalidOperationException(
          $"The {nameof(TopicResponseCacheAttribute)} can only be applied to a controller deriving from {nameof(TopicController)}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate current topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = controller.CurrentTopic;

      if (currentTopic is null) {
        if (!AllowNull) {
          context.Result = controller.NotFound();
        }
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      //### TODO JJC082817: Should allow this to be bypassed for administrators; requires introduction of Role dependency
      //### e.g., if (!Roles.IsUserInRole(Page?.User?.Identity?.Name ?? "", "Administrators")) {...}
      if (currentTopic.IsDisabled) {
        context.Result = new UnauthorizedResult();
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(currentTopic.Attributes.GetValue("URL"))) {
        context.Result = controller.RedirectPermanent(currentTopic.Attributes.GetValue("URL"));
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics
      >-------------------------------------------------------------------------------------------------------------------------
      | Nested topics are not expected to be viewed directly; if a user requests a nested topic, return a 403 to indicate that
      | the request is valid, but forbidden.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic is { ContentType: "List"} or { Parent: {ContentType: "List" } }) {
        context.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle containers
      >-------------------------------------------------------------------------------------------------------------------------
      | Like nested topics, containers are not expected to be viewed directly; if a user requests a container, return a 403 to
      | indicate that the request is valid, but forbidden. Unlike nested topics, children of containers are potentially valid.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic.ContentType is "Container") {
        context.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle page groups
      >-------------------------------------------------------------------------------------------------------------------------
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic.ContentType is "PageGroup") {
        var target = currentTopic.Children.Where(t => t.IsVisible()).FirstOrDefault()?.GetWebPath();
        context.Result = target is null? new StatusCodeResult(403) : controller.Redirect(target);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle canonical URL
      >-------------------------------------------------------------------------------------------------------------------------
      | Most search engines are case sensitive, even though many web servers are configured case insensitive. To help avoid
      | mismatches between the requested URL and the canonical URL, and to help ensure that references to topics maintain the
      | same case as assigned in the topic graph, URLs that vary only by case will be redirected to the expected case.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!currentTopic.GetWebPath().Equals(context.HttpContext.Request.Path, StringComparison.Ordinal)) {
        context.Result = controller.RedirectPermanent(currentTopic.GetWebPath());
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Base processing
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.OnActionExecuting(context);

    }

  } //Class
} //Namespace