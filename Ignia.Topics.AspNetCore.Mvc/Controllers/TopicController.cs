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
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default ASP.NET MVC Controller for any paths associated with the Ignia Topic Library. Responsible for
  ///   identifying the topic associated with the given path, determining its content type, and returning a view associated with
  ///   that content type (with potential overrides for multiple views).
  /// </summary>
  public class TopicController : Controller {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
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
      TopicRepository = topicRepository;
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
    protected ITopicRepository TopicRepository { get; }

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
    public async virtual Task<IActionResult> Index(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = await _topicMappingService.MapAsync(CurrentTopic).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return TopicView(topicViewModel, CurrentTopic.View);

    }

    /*==========================================================================================================================
    | METHOD: TOPIC VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="TopicViewResult"/> object by specifying a <paramref name="viewName"/> and the <paramref
    ///   name="model"/> to be rendered by the view.
    /// </summary>
    /// <param name="model">The model that is rendered by the view.</param>
    /// <param name="viewName">The optional name of the view that is rendered to the response.</param>
    /// <returns>The created <see cref="TopicViewResult"/> object for the response.</returns>
    [NonAction]
    public virtual TopicViewResult TopicView(object model, string viewName = null) =>
      new TopicViewResult(ViewData, TempData, model, CurrentTopic.ContentType, viewName);

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
      if (CurrentTopic.ContentType == "List" || CurrentTopic.Parent.ContentType == "List") {
        filterContext.Result = new StatusCodeResult(403);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle page groups
      >-----------------------------------------------------------------------------------------------------------------------—-
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (CurrentTopic.ContentType == "PageGroup") {
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


