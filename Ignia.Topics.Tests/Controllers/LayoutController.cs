/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;
using Ignia.Topics.Web.Mvc.Controllers;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: ERROR CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Concrete implementation of <see cref="LayoutControllerBase{T}"/> class, suitable for test purposes.
  /// </summary>
  public class LayoutController : LayoutControllerBase<NavigationTopicViewModel> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public LayoutController(
      ITopicRepository topicRepository,
      ITopicRoutingService topicRoutingService,
      ITopicMappingService topicMappingService
    ) : base(
      topicRepository,
      topicRoutingService,
      topicMappingService
    ) {}

  } //Class
} //Namespace
