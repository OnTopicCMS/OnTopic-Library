﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace OnTopic.AspNetCore.Mvc {

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
    ///   Will look for a view, in order, from the query string (<c>?View=</c>), <see cref="HttpRequest.Headers"/> collection
    ///   (for matches in the <c>accepts</c> header), then the <see cref="Topic.View"/> property, if set, and finally falls back
    ///   to the <see cref="Topic.ContentType"/>. If none of those yield any results, will default to a content type of "Page",
    ///   which expects to find <c>~/Views/Page/Page.cshtml</c>.
    /// </remarks>
    /// <param name="actionContext">The <see cref="ActionContext"/> associated with the current request.</param>
    /// <param name="viewResult">The <see cref="TopicViewResult"/>.</param>
    /// <returns>A <see cref="ViewEngineResult"/>.</returns>
    public ViewEngineResult FindView(ActionContext actionContext, TopicViewResult viewResult) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(actionContext, nameof(actionContext));
      Contract.Requires(viewResult, nameof(viewResult));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       routeData                       = actionContext.RouteData;
      var                       contentType                     = viewResult.TopicContentType;
      var                       topicView                       = viewResult.TopicView;
      var                       viewEngine                      = viewResult.ViewEngine?? ViewEngine;
      var                       requestContext                  = actionContext.HttpContext.Request;
      var                       view                            = (ViewEngineResult?)null;
      var                       searchedPaths                   = new List<string>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache content type as route variable
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE JJC20191123 This isn't required by the TopicViewResultExecutor itself, but is needed by the
      | TopicViewLocationExpander, which is responsible for finding views that correspond to the ViewEngineResult returned by
      | the TopicViewResultExecutor. This is necessary because by the time the TopicViewLocationExpander is executed, it only
      | has access to the view name and the view data, but not the original TopicViewResult, or even the ViewEngineResult.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!routeData.Values.ContainsKey("contenttype")) {
        routeData.Values.Add("contenttype", contentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Check Querystring
      >-------------------------------------------------------------------------------------------------------------------------
      | Determines if the view is defined in the querystring.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (requestContext.Query.ContainsKey("View")) {
        var queryStringValue = requestContext.Query["View"].First<string>();
        if (queryStringValue is not null) {
          view = viewEngine.FindView(actionContext, queryStringValue, isMainPage: true);
          searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull Headers
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!(view?.Success ?? false) && requestContext.Headers.ContainsKey("Accept")) {
        foreach (var header in requestContext.Headers["Accept"]) {
          var value = header.Replace("+", "-", StringComparison.Ordinal);
          if (value.Contains('/', StringComparison.Ordinal)) {
            value = value[(value.IndexOf("/", StringComparison.Ordinal)+1)..];
          }
          if (value.Contains(';', StringComparison.Ordinal)) {
            value = value[..(value.IndexOf(";", StringComparison.Ordinal))];
          }
          if (value is not null) {
            view = viewEngine.FindView(actionContext, value, isMainPage: true);
            searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
          }
          if (view?.Success ?? false) {
            break;
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull from action name
      >-------------------------------------------------------------------------------------------------------------------------
      | Typically, views in the topic library are contextual to the request, topic, or content type. When deriving from the
      | TopicController, however, and implementing custom actions, we should prioritize views that correspond to that action,
      | if they exist. This maps closely to how the default ViewResultExecutor works, but places it in the appropriate order for
      | evaluation against other view sources.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!view?.Success ?? true) {
        if (routeData.Values.TryGetValue("action", out var action)) {
          var actionName = action?.ToString()?.Replace("Async", "", StringComparison.OrdinalIgnoreCase);
          view = ViewEngine.FindView(actionContext, actionName, isMainPage: true);
          searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
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
      if (view is not null and { Success: true }) {
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
      Contract.Requires(context, nameof(context));
      Contract.Requires(result, nameof(result));

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
        ).ConfigureAwait(false);
      }

    }

  } //Class
} //Namespace