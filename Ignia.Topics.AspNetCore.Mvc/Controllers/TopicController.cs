/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default ASP.NET MVC Controller for any paths associated with the Ignia Topic Library. Responsible for
  ///   identifying the topic associated with the given path, determining its content type, and returning a view associated with
  ///   that content type (with potential overrides for multiple views).
  /// </summary>
  public class TopicController : AsyncController {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository                = null;
    private readonly            ITopicRoutingService            _topicRoutingService            = null;
    private readonly            ITopicMappingService            _topicMappingService            = null;
    private                     Topic                           _currentTopic                   = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public TopicController(
      ITopicRepository topicRepository,
      ITopicRoutingService topicRoutingService,
      ITopicMappingService topicMappingService
     ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository != null, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(topicRoutingService != null, "A concrete implementation of an ITopicRoutingService is required.");
      Contract.Requires(topicMappingService!= null, "A concrete implementation of an ITopicMappingService is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository = topicRepository;
      _topicRoutingService = topicRoutingService;
      _topicMappingService = topicMappingService;

    }

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the Topic Repository in order to gain arbitrary access to the entire topic graph.
    /// </summary>
    /// <returns>The TopicRepository associated with the controller.</returns>
    protected ITopicRepository TopicRepository => _topicRepository;

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    protected Topic CurrentTopic {
      get {
        if (_currentTopic == null) {
          _currentTopic = _topicRoutingService.GetCurrentTopic();
        }
        return _currentTopic;
      }
    }

    /*==========================================================================================================================
    | GET: INDEX (VIEW TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to a view associated with the current topic's Content Type, if appropriate, view (as defined by the
    ///   query string or topic's view.
    /// </summary>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    public async virtual Task<ActionResult> IndexAsync(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = await _topicMappingService.MapAsync(CurrentTopic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new TopicViewResult(topicViewModel, CurrentTopic.ContentType, CurrentTopic.View);

    }

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
    protected override void OnActionExecuting(ActionExecutingContext filterContext) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle exceptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (CurrentTopic == null) {
        filterContext.Result = NotFound("There is no topic associated with this path.");
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      //### TODO JJC082817: Should allow this to be bypassed for administrators; requires introduction of Role dependency
      //### e.g., if (!Roles.IsUserInRole(Page?.User?.Identity?.Name ?? "", "Administrators")) {...}
      if (CurrentTopic.IsDisabled) {
        filterContext.Result = new UnauthorizedResult();
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(CurrentTopic.Attributes.GetValue("URL"))) {
        filterContext.Result = RedirectPermanent(CurrentTopic.Attributes.GetValue("URL"));
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics
      >-----------------------------------------------------------------------------------------------------------------------—-
      | Nested topics are not expected to be viewed directly; if a user requests a nested topic, return a 403 to indicate that
      | the request is valid, but forbidden.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (CurrentTopic.ContentType.Equals("List") || CurrentTopic.Parent.ContentType.Equals("List")) {
        filterContext.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle page groups
      >-----------------------------------------------------------------------------------------------------------------------—-
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (CurrentTopic.ContentType.Equals("PageGroup")) {
        filterContext.Result = Redirect(
          CurrentTopic.Children.Where(t => t.IsVisible()).FirstOrDefault().GetWebPath()
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


