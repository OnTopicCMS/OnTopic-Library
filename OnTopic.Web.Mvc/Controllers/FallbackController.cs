/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Web.Mvc;

namespace OnTopic.Web.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: FALLBACK CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an empty fallback controller, which will be called if no other controller is identified. The primary purpose of
  ///   this controller is to throw a 404.
  /// </summary>

  public class FallbackController : Controller {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a DefaultController.
    /// </summary>
    /// <returns>A DefaultController.</returns>
    public FallbackController() : base() {
    }

    /*==========================================================================================================================
    | GET: INDEX
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the default page for the site.
    /// </summary>
    /// <returns>The site's default view.</returns>
    [HttpGet]
    public virtual ActionResult Index() => HttpNotFound("No controller available to handle this request.");

  } //Class
} //Namespace