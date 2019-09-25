/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.Repositories;
using Ignia.Topics.AspNetCore.Mvc.Models;
using Ignia.Topics.Internal.Diagnostics;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

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
    private readonly            ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public SitemapController(ITopicRepository topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, nameof(topicRepository));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository = topicRepository;

    }

    /*==========================================================================================================================
    | GET: /SITEMAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the Sitemap.org sitemap for the site.
    /// </summary>
    /// <returns>The site's homepage view.</returns>
    public virtual ActionResult Index() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = _topicRepository.Load();

      Contract.Assume(
        rootTopic,
        $"The topic graph could not be successfully loaded from the {nameof(ITopicRepository)} instance. The " +
        $"{nameof(SitemapController)} is unable to establish a local copy to work off of."
      );
      
      /*------------------------------------------------------------------------------------------------------------------------
      | Establish view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = new TopicEntityViewModel(_topicRepository, rootTopic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Define content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      Response.ContentType = "text/xml";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the homepage view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("Sitemap", topicViewModel);

    }

  } // Class

} // Namespace