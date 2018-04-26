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
  ///   <para>
  ///     Implementers may wish to provide derived classes which return more specific error messages. This class provides a
  ///     generic implementation that should suit most requirements.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="ErrorController{T}"/> does not assume that a particular view
  ///     model will be used, and instead accepts a generic argument for any view model that implements the interface <see
  ///     cref="IPageTopicViewModelCore"/>. Since generic controllers cannot be effectively routed to, however, that means that
  ///     implementors must, at minimum, provide a local instance of <see cref="ErrorController{T}"/> which sets the generic
  ///     value to the desired view model. To help enforce this, while avoiding ambiguity, this class is marked as
  ///     <c>abstract</c> and suffixed with <b>Base</b>.
  ///   </para>
  /// </remarks>
  public abstract class ErrorControllerBase<T> : Controller where T : IPageTopicViewModelCore, new() {

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
      var viewModel             = CreateErrorViewModel("Error", title);

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
      var viewModel             = CreateErrorViewModel("NotFound", title);

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
      var viewModel             = CreateErrorViewModel("InternalServer", title);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View("InternalServer", viewModel);

    }

    /*==========================================================================================================================
    | CREATE ERROR VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes an empty view model, populates it with appropriate values based on the method parameters, and returns it
    ///   for use in one of the actions.
    /// </summary>
    /// <param name="key">
    ///   The key name to use for the <see cref="IPageTopicViewModelCore"/>'s <c>Key</c> and <c>WebPath</c> properties.
    /// </param>
    /// <param name="title">The title of the error page.</param>
    /// <returns>A view model representing an error message.</returns>
    public virtual T CreateErrorViewModel(string key, string title) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel = new T();
      viewModel.Key = key;
      viewModel.WebPath = "/Error/" + key;
      viewModel.ContentType = "Page";
      viewModel.Title = title;
      viewModel.MetaKeywords = "";
      viewModel.MetaDescription = "";

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return viewModel;

    }

  } // Class

} // Namespace