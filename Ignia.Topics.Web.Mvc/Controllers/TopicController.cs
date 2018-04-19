/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Diagnostics.Contracts;
using Ignia.Topics;
using Ignia.Topics.Repositories;
using Ignia.Topics.Mapping;

namespace Ignia.Topics.Web.Mvc {

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
    protected ITopicRepository TopicRepository {
    	get {
        return _topicRepository;
      }
    }

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
    public virtual ActionResult Index(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = _topicMappingService.Map(CurrentTopic);

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
        filterContext.Result = HttpNotFound("There is no topic associated with this path.");
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      //### TODO JJC082817: Should allow this to be bypassed for administrators; requires introduction of Role dependency
      //### e.g., if (!Roles.IsUserInRole(Page?.User?.Identity?.Name ?? "", "Administrators")) {...}
      if (CurrentTopic.IsDisabled) {
        filterContext.Result = new HttpUnauthorizedResult("The topic at this location is disabled.");
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
      | Handle page group
      >-----------------------------------------------------------------------------------------------------------------------—-
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (CurrentTopic.ContentType.Equals("PageGroup")) {
        filterContext.Result = Redirect(
          CurrentTopic.Children.Where(t => t.IsVisible()).DefaultIfEmpty(new Topic()).FirstOrDefault().GetWebPath()
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


