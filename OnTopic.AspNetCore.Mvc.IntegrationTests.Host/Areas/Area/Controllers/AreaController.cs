/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OnTopic.AspNetCore.Mvc.Controllers;
using OnTopic.Mapping;
using OnTopic.Repositories;

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

  } //Class
} //Namespace