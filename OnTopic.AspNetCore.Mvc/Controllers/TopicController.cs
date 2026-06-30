/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: TOPIC CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default ASP.NET MVC Controller for any paths associated with the Ignia Topic Library. Responsible for
  ///   identifying the topic associated with the given path, determining its content type, and returning a view associated with
  ///   that content type (with potential overrides for multiple views).
  /// </summary>
  /// <param name="topicRepository">
  ///   A concrete implementation of an <see cref="ITopicRepository"/> for loading topics.
  /// </param>
  /// <param name="topicMappingService">
  ///   A concrete implementation of an <see cref="ITopicMappingService"/> for mapping topics to view models.
  /// </param>
  [TopicResponseCache]
  public class TopicController(ITopicRepository topicRepository, ITopicMappingService topicMappingService) : Controller {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicMappingService            _topicMappingService            = Contract.Requires(topicMappingService);

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the Topic Repository in order to gain arbitrary access to the entire topic graph.
    /// </summary>
    /// <returns>The TopicRepository associated with the controller.</returns>
    protected internal ITopicRepository TopicRepository { get; } = Contract.Requires(topicRepository);

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    public Topic? CurrentTopic {
      get {
        field ??= TopicRepository.Load(RouteData);
        return field;
      }
      set => field = value;
    }

    /*==========================================================================================================================
    | GET: INDEX (VIEW TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to a view associated with the current topic's Content Type, if appropriate, view (as defined by the
    ///   query string or topic's view.
    /// </summary>
    /// <returns>A view associated with the requested topic's Content Type and view.</returns>
    [ValidateTopic]
    public async virtual Task<IActionResult> IndexAsync(string path) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicViewModel = await _topicMappingService.MapAsync(CurrentTopic).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        topicViewModel,
        $"A topic view model could not be created for the '{CurrentTopic?.GetUniqueKey()}' topic."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return TopicView(topicViewModel, CurrentTopic?.View);

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
    [ExcludeFromCodeCoverage]
    public TopicViewResult TopicView(object model, string? viewName = null) =>
      new(ViewData, TempData, model, CurrentTopic?.ContentType, viewName);

  } //Class
} //Namespace