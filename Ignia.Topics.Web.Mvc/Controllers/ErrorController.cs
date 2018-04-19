/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Web.Mvc;

namespace Ignia.Topics.Web.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: ERROR CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to the views associated with 400 and 500 error results.
  /// </summary>
  /// <remarks>
  ///   Implementers may wish to provide derived classes which return more specific error messages. This class provides a
  ///   generic implementation that should suit most requirements.
  /// </remarks>
  public class ErrorController<T> : Controller where T : IPageTopicViewModelCore, new() {

    /*==========================================================================================================================
    | GET: /Error/Error
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the default custom error page for the site.
    /// </summary>
    /// <returns>The site's default error view.</returns>
    public virtual ActionResult Error(string title = "General Error") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel             = new T();
      viewModel.Key             = "Error";
      viewModel.WebPath         = "/Error/Error";
      viewModel.ContentType     = "Page";
      viewModel.Title           = title;
      viewModel.MetaKeywords    = "";
      viewModel.MetaDescription = "";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("Error", viewModel);

    }

    /*==========================================================================================================================
    | GET: /Error/NotFound
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the custom 404 error page for the site.
    /// </summary>
    /// <returns>The site's 404 (not found) error view.</returns>
    public virtual ActionResult NotFound(string title = "Page Not Found") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the proper status code
      \-----------------------------------------------------------------------------------------------------------------------*/
      Response.StatusCode       = 404;

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel             = new T();
      viewModel.Key             = "NotFound";
      viewModel.WebPath         = "/Error/NotFound";
      viewModel.ContentType     = "Page";
      viewModel.Title           = title;
      viewModel.MetaKeywords    = "";
      viewModel.MetaDescription = "";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("NotFound", viewModel);

    }

    /*==========================================================================================================================
    | GET: /Error/InternalServer
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the custom 500 error page for the site.
    /// </summary>
    /// <returns>The site's 500 (internal server) error view.</returns>
    public virtual ActionResult InternalServer(string title = "Internal Server Error") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the proper status code
      \-----------------------------------------------------------------------------------------------------------------------*/
      Response.StatusCode       = 500;

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel             = new T();
      viewModel.Key             = "InternalServer";
      viewModel.WebPath         = "/Error/InternalServer";
      viewModel.ContentType     = "Page";
      viewModel.Title           = title;
      viewModel.MetaKeywords    = "";
      viewModel.MetaDescription = "";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("InternalServer", viewModel);

    }

  } // Class

} // Namespace