/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Areas.Area.Controllers {

  /*============================================================================================================================
  | CLASS: CONTROLLER CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ControllerController"/> establishes a controller named <c>Controller</c> which will be used to look for
  ///   views located in e.g. <c>/Views/Controller/{Action}.cshtml</c>. The name <c>Controller</c> helps reinforce that it is
  ///   acting as a stand-in for an actual <see cref="Controller"/>, as would be implemented in a non-test application.
  /// </summary>
  [Area("Area")]
  public class ControllerController: Controller {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="ControllerController"/>.
    /// </summary>
    /// <returns>A <see cref="ControllerController"/> for exposing specific actions.</returns>
    public ControllerController() {}

    /*==========================================================================================================================
    | GET: ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>Action</c>.
    /// </summary>
    /// <returns>A view associated with the <c>Action</c> action.</returns>
    public IActionResult Action() => View();

    /*==========================================================================================================================
    | GET: FALLBACK ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>FallbackAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>FallbackAction</c> action.</returns>
    public IActionResult FallbackAction() => View();

    /*==========================================================================================================================
    | GET: SHARED ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>SharedAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>SharedAction</c> action.</returns>
    public IActionResult SharedAction() => View();

    /*==========================================================================================================================
    | GET: SHARED FALLBACK ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>SharedFallbackAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>SharedFallbackAction</c> action.</returns>
    public IActionResult SharedFallbackAction() => View();

    /*==========================================================================================================================
    | GET: AREA ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>AreaAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>AreaAction</c> action.</returns>
    public IActionResult AreaAction() => View();

    /*==========================================================================================================================
    | GET: AREA FALLBACK ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>AreaFallbackAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>AreaFallbackAction</c> action.</returns>
    public IActionResult AreaFallbackAction() => View();

    /*==========================================================================================================================
    | GET: AREA SHARED ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>AreaSharedAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>AreaSharedAction</c> action.</returns>
    public IActionResult AreaSharedAction() => View();

    /*==========================================================================================================================
    | GET: AREA SHARED FALLBACK ACTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Exposes a method which will, by default, be associated with views named <c>AreaSharedFallbackAction</c>.
    /// </summary>
    /// <returns>A view associated with the <c>AreaSharedFallbackAction</c> action.</returns>
    public IActionResult AreaSharedFallbackAction() => View();


  } //Class
} //Namespace