/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Internal.Diagnostics;

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
  ///   cref="RedirectResult"/>. All of this logic can be enforced by adding the <see cref="ValidateTopicFilter"/> to an action.
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
    ///   filters, this specific implementation is focused on validating the state of the <see cref="CurrentTopic"/>. Namely,
    ///   it will provide error handling (if the <see cref="CurrentTopic"/> is null), a redirect (if the <see
    ///   cref="CurrentTopic"/>'s <c>Url</c> attribute is set, and an unauthorized response (if the <see cref="CurrentTopic"/>'s
    ///   <see cref="Topic.IsDisabled"/> flag is set.
    /// </remarks>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    [NonAction]
    public override void OnActionExecuting(ActionExecutingContext filterContext) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(filterContext, nameof(filterContext));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var controller            = filterContext.Controller as TopicController;
      var currentTopic          = controller?.CurrentTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate context
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (controller is null) {
        throw new InvalidOperationException(
          $"The {nameof(ValidateTopicAttribute)} can only be applied to a controller deriving from {nameof(TopicController)}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle exceptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic is null) {
        if (!AllowNull) {
          filterContext.Result = controller.NotFound("There is no topic associated with this path.");
        }
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      //### TODO JJC082817: Should allow this to be bypassed for administrators; requires introduction of Role dependency
      //### e.g., if (!Roles.IsUserInRole(Page?.User?.Identity?.Name ?? "", "Administrators")) {...}
      if (currentTopic.IsDisabled) {
        filterContext.Result = new UnauthorizedResult();
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(currentTopic.Attributes.GetValue("URL"))) {
        filterContext.Result = controller.RedirectPermanent(currentTopic.Attributes.GetValue("URL"));
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics
      >-------------------------------------------------------------------------------------------------------------------------
      | Nested topics are not expected to be viewed directly; if a user requests a nested topic, return a 403 to indicate that
      | the request is valid, but forbidden.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic.ContentType == "List" || currentTopic.Parent?.ContentType == "List") {
        filterContext.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle containers
      >-------------------------------------------------------------------------------------------------------------------------
      | Like nested topics, containers are not expected to be viewed directly; if a user requests a container, return a 403 to
      | indicate that the request is valid, but forbidden. Unlike nested topics, children of containers are potentially valid.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic.ContentType == "Container") {
        filterContext.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle page groups
      >-------------------------------------------------------------------------------------------------------------------------
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (currentTopic.ContentType == "PageGroup") {
        filterContext.Result = controller.Redirect(
          currentTopic.Children.Where(t => t.IsVisible()).FirstOrDefault().GetWebPath()
        );
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Base processing
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.OnActionExecuting(filterContext);

    }

  } //Class
} //Namespace