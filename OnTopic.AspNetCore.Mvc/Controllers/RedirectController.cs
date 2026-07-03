/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.AspNetCore.Mvc.Controllers;

/*==============================================================================================================================
| CLASS: REDIRECT CONTROLLER
\-----------------------------------------------------------------------------------------------------------------------------*/
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
/// <param name="topicRepository">
///   An implementation of an <see cref="ITopicRepository"/> to retrieve the current <see cref="Topic"/> from.
/// </param>
public class RedirectController(ITopicRepository topicRepository) : Controller {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              ITopicRepository                _topicRepository                = topicRepository;

  /*============================================================================================================================
  | REDIRECT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Redirect based on <see cref="Topic.Id"/>.
  /// </summary>
  /// <param name="topicId">The <see cref="Topic.Id"/> to lookup in the <see cref="ITopicRepository"/>.</param>
  public ActionResult Redirect(int topicId) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Find the topic with the correct PageID.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = _topicRepository.Load(topicId);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide error handling
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null) {
      return NotFound("Invalid  TopicID.");
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Perform redirect
    \-------------------------------------------------------------------------------------------------------------------------*/
    return RedirectPermanent(topic.GetWebPath());

  }

} //Class