/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Web.Mvc;
using Ignia.Topics.Repositories;
using Ignia.Topics.Web.Mvc.Models;

namespace Ignia.Topics.Web.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: SITEMAP CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Responds to requests for a sitemap according to sitemap.org's schema. The view is expected to recursively loop over
  ///   child topics to generate the appropriate markup.
  /// </summary>
  public class SitemapController : Controller {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public SitemapController(ITopicRepository topicRepository) {
      _topicRepository          = topicRepository;
    }

    /*==========================================================================================================================
    | GET: /SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the Sitemap.org sitemap for the site.
    /// </summary>
    /// <returns>The site's homepage view.</returns>
    [HttpGet]
    public virtual ActionResult Index() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish Page Topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = new TopicEntityViewModel(_topicRepository, _topicRepository.Load());

      /*------------------------------------------------------------------------------------------------------------------------
      | DEFINE CONTENT TYPE
      \-----------------------------------------------------------------------------------------------------------------------*/
      Response.ContentType = "text/xml";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the homepage view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("Sitemap", topicViewModel);

    }

  } // Class

} // Namespace