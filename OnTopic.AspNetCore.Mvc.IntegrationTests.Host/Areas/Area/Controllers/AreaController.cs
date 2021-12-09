/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Mapping;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Areas.Area.Controllers {

  /*============================================================================================================================
  | CLASS: AREA CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="AreaController"/> establishes a controller with the same name as the current area—i.e., <c>Area</c>—as a
  ///   means of testing the <see cref="ServiceCollectionExtensions.MapImplicitAreaControllerRoute(IEndpointRouteBuilder)"/>
  ///   extension method, and its underlying <see cref="TopicRouteValueTransformer"/>.
  /// </summary>
  [Area("Area")]
  public class AreaController: TopicController {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="AreaController"/> with necessary dependencies.
    /// </summary>
    /// <returns>A <see cref="AreaController"/> for loading OnTopic views.</returns>
    public AreaController(
      ITopicRepository topicRepository,
      ITopicMappingService topicMappingService
    ): base(topicRepository, topicMappingService) {}

    /*==========================================================================================================================
    | GET: ACCORDION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>Accordion</c>.
    /// </summary>
    /// <returns>A view associated with the <c>Accordion</c> action.</returns>
    public IActionResult Accordion() => TopicView(new(), CurrentTopic!.View);

  } //Class
} //Namespace