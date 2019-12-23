/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Web.Mvc;
using OnTopic.Repositories;

namespace OnTopic.Web.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: REDIRECT CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Handles redirect based on TopicID, thus allowing permanent redirects to be setup.
  /// </summary>
  /// <remarks>
  ///   Typically, a page <see cref="Topic"/> is requested based on the <see cref="Topic.GetWebPath"/> value, which is a hash of
  ///   its <see cref="Topic.GetUniqueKey"/>. When a <see cref="Topic"/> is moved to a different location in the topic graph,
  ///   however, its <see cref="Topic.GetUniqueKey"/> will return a different value, corresponding to its new location. To allow
  ///   permanent references to page, therefore, the <see cref="RedirectController"/> accepts paths based on the <see
  ///   cref="Topic.Id"/>, which is expected to be stable for the lifetime of a <see cref="Topic"/> entity.
  /// </remarks>
  public class RedirectController : Controller {

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
    /// <param name="topicRepository">
    ///   An implementation of an <see cref="ITopicRepository"/> to retrieve the current <see cref="Topic"/> from.
    /// </param>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public RedirectController(ITopicRepository topicRepository) : base() {
      _topicRepository          = topicRepository;
    }

    /*==========================================================================================================================
    | REDIRECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Redirect based on <see cref="Topic.Id"/>.
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> to lookup in the <see cref="ITopicRepository"/>.</param>
    [HttpGet]
    public virtual ActionResult Redirect(int topicId) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Find the topic with the correct PageID.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic                 = _topicRepository.Load(topicId);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide error handling
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) {
        return HttpNotFound("Invalid TopicID.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Perform redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      return RedirectPermanent(topic.GetWebPath());

    }

  } //Class
} //Namespace