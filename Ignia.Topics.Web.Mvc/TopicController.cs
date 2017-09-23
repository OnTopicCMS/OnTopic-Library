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
using Ignia.Topics;
using Ignia.Topics.Repositories;

namespace Ignia.Topics.Web.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default ASP.NET MVC Controller for any paths associated with the Ignia Topic Library. Responsible for
  ///   identifying the topic associated with the given path, determining its content type, and returning a view associated with
  ///   that content type (with potential overrides for multiple views).
  /// </summary>
  public class TopicController<T> : AsyncController where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     ITopicRepository                _topicRepository        = null;
    private     T                               _currentTopic           = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public TopicController(ITopicRepository topicRepository, T currentTopic) {
      _topicRepository = topicRepository;
      _currentTopic = currentTopic;
    }

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the Topic Repository in order to gain arbitrary access to the entire topic graph.
    /// </summary>
    /// <returns>The TopicRepository associated with the controller.</returns>
    public ITopicRepository TopicRepository {
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
    public Topic CurrentTopic {
      get {
        return _currentTopic;
      }
    }

    /*==========================================================================================================================
    | GET: /PATH/PATH/PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to a view associated with the current topic's Content Type, if appropriate, view (as defined by the query
    ///   string or topic's view.
    /// </summary>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    public async Task<ActionResult> Index(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish Page Topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicRoutingService = new TopicRoutingService(_topicRepository, this.HttpContext.Request.RequestContext);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle exceptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicRoutingService.Topic == null) {
        return HttpNotFound("There is no topic associated with this path.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      //### TODO JJC082817: Should allow this to be bypassed for administrators; requires introduction of Role dependency
      //### e.g., if (!Roles.IsUserInRole(Page?.User?.Identity?.Name ?? "", "Administrators")) {...}
      if (topicRoutingService.Topic.IsDisabled) {
        return new HttpUnauthorizedResult("The topic at this location is disabled.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(topicRoutingService.Topic.Attributes.Get("URL"))) {
        return RedirectPermanent(topicRoutingService.Topic.Attributes.Get("URL"));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle page group
      >-----------------------------------------------------------------------------------------------------------------------—-
      | PageGroups are a special content type for packaging multiple pages together. When a PageGroup is identified, the user is
      | redirected to the first (non-hidden, non-disabled) page in the page group.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicRoutingService.Topic.ContentType.Equals("PageGroup")) {
        return Redirect(topicRoutingService.Topic.SortedChildren.Where(t => t.IsVisible()).DefaultIfEmpty(new Topic()).FirstOrDefault().WebPath);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = new TopicViewModel(_topicRepository, topicRoutingService.Topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify associated razor view
      \-----------------------------------------------------------------------------------------------------------------------*/
      var view = new RazorView(
        this.ControllerContext,
        topicRoutingService.ViewPath,
        null,
        true,
        null
       );

      /*------------------------------------------------------------------------------------------------------------------------
      | Return view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View(view, topicViewModel);

    }

  } //Class

} //Namespace


