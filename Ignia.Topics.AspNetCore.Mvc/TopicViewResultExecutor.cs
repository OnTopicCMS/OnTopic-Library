/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ignia.Topics.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW RESULT EXECUTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  /// Finds and executes an <see cref="IView"/> for a <see cref="ViewResult"/>.
  /// </summary>
  public class TopicViewResultExecutor : ViewExecutor, IActionResultExecutor<TopicViewResult> {

    /// <summary>
    /// Creates a new <see cref="ViewResultExecutor"/>.
    /// </summary>
    /// <param name="viewOptions">The <see cref="IOptions{MvcViewOptions}"/>.</param>
    /// <param name="writerFactory">The <see cref="IHttpResponseStreamWriterFactory"/>.</param>
    /// <param name="viewEngine">The <see cref="ICompositeViewEngine"/>.</param>
    /// <param name="tempDataFactory">The <see cref="ITempDataDictionaryFactory"/>.</param>
    /// <param name="diagnosticListener">The <see cref="DiagnosticListener"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
    public TopicViewResultExecutor(
      IOptions<MvcViewOptions> viewOptions,
      IHttpResponseStreamWriterFactory writerFactory,
      ICompositeViewEngine viewEngine,
      ITempDataDictionaryFactory tempDataFactory,
      DiagnosticListener diagnosticListener,
      IModelMetadataProvider modelMetadataProvider
    ) : base(
      viewOptions, writerFactory, viewEngine, tempDataFactory, diagnosticListener, modelMetadataProvider
    ) {
    }

    /*==========================================================================================================================
    | METHOD: FIND VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loops through potential sources for views to identify the most appropriate <see cref="RazorView"/>.
    /// </summary>
    /// <remarks>
    ///   Will look for a view, in order, from the query string (<code>?View=</code>), <see cref="HttpRequest.Headers"/>
    ///   collection (for matches in the <code>accepts</code> header), then the <see cref="Topic.View"/> property, if set, and
    ///   finally falls back to the <see cref="Topic.ContentType"/>. If none of those yield any results, will default to a
    ///   content type of "Page", which expects to find <code>~/Views/Page/Page.cshtml</code>.
    /// </remarks>
    /// <param name="actionContext">The <see cref="ActionContext"/> associated with the current request.</param>
    /// <param name="viewResult">The <see cref="TopicViewResult"/>.</param>
    /// <returns>A <see cref="ViewEngineResult"/>.</returns>
    public ViewEngineResult FindView(ActionContext actionContext, TopicViewResult viewResult) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(actionContext != null, nameof(actionContext));
      Contract.Requires<ArgumentNullException>(viewResult != null, nameof(viewResult));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       contentType                     = viewResult.TopicContentType;
      var                       topicView                       = viewResult.TopicView;
      var                       viewEngine                      = viewResult.ViewEngine ?? ViewEngine;
      var                       requestContext                  = actionContext.HttpContext.Request;
      var                       view                            = (ViewEngineResult)null;
      var                       searchedPaths                   = new List<string>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Check Querystring
      >-------------------------------------------------------------------------------------------------------------------------
      | Determines if the view is defined in the querystring.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!(view?.Success ?? false) && requestContext.Query.ContainsKey("View")) {
        var queryStringValue = requestContext.Query["View"].First<string>();
        if (queryStringValue != null) {
          view = viewEngine.FindView(actionContext, queryStringValue, isMainPage: true);
          searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull Headers
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!(view?.Success ?? false) && requestContext.Headers.ContainsKey("Accept")) {
        var acceptHeaders = requestContext.Headers["Accept"].First<string>();
        // Validate the content-type after the slash, then validate it against available views
        var splitHeaders = acceptHeaders.Split(new char[] { ',', ';' });
        // Validate the content-type after the slash, then validate it against available views
        for (var i = 0; i < splitHeaders.Length; i++) {
          if (splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
            // Get content-type after the slash and replace '+' characters in the content-type to '-' for view file encoding purposes
            var acceptHeader = splitHeaders[i].Substring(splitHeaders[i].IndexOf("/") + 1).Replace("+", "-");
            // Validate against available views; if content-type represents a valid view, stop validation
            if (acceptHeader != null) {
              view = viewEngine.FindView(actionContext, acceptHeader, isMainPage: true);
              searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
            }
            if (view != null) {
              break;
            }
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull from topic attribute
      >-------------------------------------------------------------------------------------------------------------------------
      | Pull from Topic's View Attribute; additional check against the Topic's ContentType Topic View Attribute is not necessary
      | as it is set as the default View value for the Topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!(view?.Success ?? false) && !String.IsNullOrEmpty(topicView)) {
        view = viewEngine.FindView(actionContext, topicView, isMainPage: true);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Default to content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!view?.Success ?? true) {
        view = viewEngine.FindView(actionContext, contentType, isMainPage: true);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return view, if found
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view?.Success ?? false) {
        return view;
      }
      return ViewEngineResult.NotFound(contentType, searchedPaths);

    }

    /*==========================================================================================================================
    | METHOD: EXECUTE (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task ExecuteAsync(ActionContext context, TopicViewResult result) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(context != null, nameof(context));
      Contract.Requires<ArgumentNullException>(result != null, nameof(result));

      /*------------------------------------------------------------------------------------------------------------------------
      | Find view
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewEngineResult = FindView(context, result);
      viewEngineResult.EnsureSuccessful(originalLocations: null);
      var view = viewEngineResult.View;

      /*------------------------------------------------------------------------------------------------------------------------
      | Execute
      \-----------------------------------------------------------------------------------------------------------------------*/
      using (view as IDisposable) {
        await ExecuteAsync(
          context,
          view,
          result.ViewData,
          result.TempData,
          result.ContentType,
          result.StatusCode
        );
      }

    }

  } //Class
} //Namespace