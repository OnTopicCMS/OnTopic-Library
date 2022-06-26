/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using OnTopic.Mapping;

namespace OnTopic.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: ERROR CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a default handler for responding to requests from the <see cref="StatusCodePagesExtensions.UseStatusCodePages(
  ///   IApplicationBuilder)" /> configuration.
  /// </summary>
  /// <remarks>
  ///   The <see cref="StatusCodePagesExtensions.UseStatusCodePages(IApplicationBuilder)"/> will redirect to a URL with the
  ///   HTTP error code in the route. This is fine if there is one error page that, perhaps, injects the error code into the
  ///   content. It's also fine if there is an error page for every HTTP error. In practice, however, many sites handle <i>some
  ///   </i> HTTP errors, but not others. Given this, the <see cref="ErrorController"/> provides logic to deliver a <see cref="
  ///   TopicController.CurrentTopic"/> associated with the HTTP error, if available, and otherwise to fallback first to the
  ///   HTTP category (e.g., 5xx), and otherwise to a generic error.
  /// </remarks>
  public class ErrorController : TopicController {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of an Error Controller with necessary dependencies.
    /// </summary>
    /// <returns>An error controller for loading views associated with HTTP error codes.</returns>
    public ErrorController(
      ITopicRepository topicRepository,
      ITopicMappingService topicMappingService
    ) : base(
      topicRepository,
      topicMappingService
    ) { }

    /*==========================================================================================================================
    | GET: HTTP ERROR (VIEW TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates the <paramref name="statusCode"/> against configured <see cref="Topic"/>s, and returns the most appropriate
    ///   content available.
    /// </summary>
    /// <returns>A view associated with the requested current <paramref name="statusCode"/>.</returns>
    public async virtual Task<IActionResult> HttpAsync([FromRoute(Name="id")] int statusCode, bool includeStaticFiles = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass for resources
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!includeStaticFiles) {
        var feature             = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        if (feature?.OriginalPath.Contains('.', StringComparison.Ordinal)?? false) {
          return Content("The resource requested could not found.");
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify base path
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic             = HttpContext.Request.RouteValues.GetValueOrDefault("rootTopic") as string?? "Error";

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify relevant topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      CurrentTopic              = TopicRepository.Load($"{rootTopic}:{statusCode}")??
                                  TopicRepository.Load($"{rootTopic}:{statusCode/100*100}")??
                                  TopicRepository.Load($"{rootTopic}");

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return await IndexAsync(CurrentTopic?.GetWebPath()?? "Error").ConfigureAwait(false);

    }

  } //Class
} //Namespace