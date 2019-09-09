/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Models;

namespace Ignia.Topics.Web.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW RESULT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom version of <see cref="ViewResult" /> capable of determining the most appropriate view.
  /// </summary>
  public class TopicViewResult : ViewResult {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    string                          _contentType                    = "";
    readonly                    string                          _topicView                      = "";

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on the <see cref="ITopicViewModelCore.View"/> and
    ///   <see cref="ITopicViewModelCore.ContentType"/>.
    /// </summary>
    /// <remarks>
    ///   If the <see cref="ITopicViewModelCore.ContentType"/> is unavailable, it is assumed to be <c>Page</c>. If the <see
    ///   cref="ITopicViewModelCore.View"/> is unavailable, it is assumed to be the same as the <see
    ///   cref="ITopicViewModelCore.ContentType"/>.
    /// </remarks>
    public TopicViewResult(ITopicViewModel viewModel) : base() {
      Contract.Requires(viewModel, nameof(viewModel));
      ViewData.Model = viewModel;
      _contentType = viewModel.ContentType ?? "Page";
      _topicView = viewModel.View ?? _contentType;
    }

    /// <summary>
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on a supplied <paramref name="contentType"/> and,
    ///   optionally, <paramref name="view"/>.
    /// </summary>
    /// <remarks>
    ///   If the <paramref name="contentType"/> is not provided, it is assumed to be <c>Page</c>. If the <paramref name="view"/>
    ///   is not provided, it is assumed to be <paramref name="contentType"/>.
    /// </remarks>
    public TopicViewResult(object viewModel, string contentType = "Page", string view = null) : base() {
      ViewData.Model = viewModel;
      _contentType = contentType;
      _topicView = view ?? _contentType;
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
    protected override ViewEngineResult FindView(ControllerContext context) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(context, nameof(context));
      
      /*------------------------------------------------------------------------------------------------------------------------
      | Set variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       contentType                     = _contentType;
      var                       viewEngine                      = ViewEngines.Engines;
      var                       requestContext                  = context.HttpContext.Request;
      var                       view                            = new ViewEngineResult(Array.Empty<string>());
      var                       searchedPaths                   = new List<string>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Check Querystring
      >-------------------------------------------------------------------------------------------------------------------------
      | Determines if the view is defined in the querystring.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null && requestContext.QueryString.AllKeys.Contains("View")) {
        var queryStringValue = requestContext.QueryString["View"];
        if (queryStringValue != null) {
          view = viewEngine.FindView(context, queryStringValue, MasterName);
          searchedPaths = searchedPaths.Union(view.SearchedLocations?? Array.Empty<string>()).ToList();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Pull Headers
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null && requestContext.Headers.AllKeys.Contains("Accept")) {
        var acceptHeaders = requestContext.Headers.GetValues("Accept");
        // Validate the content-type after the slash, then validate it against available views
        var splitHeaders = acceptHeaders[0].Split(new char[] { ',', ';' });
        // Validate the content-type after the slash, then validate it against available views
        for (var i = 0; i < splitHeaders.Length; i++) {
          if (splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
            // Get content-type after the slash and replace '+' characters in the content-type to '-' for view file encoding purposes
            var acceptHeader = splitHeaders[i]
              .Substring(splitHeaders[i].IndexOf("/", StringComparison.InvariantCulture) + 1)
              .Replace("+", "-");
            // Validate against available views; if content-type represents a valid view, stop validation
            if (acceptHeader != null) {
              view = viewEngine.FindView(context, acceptHeader, MasterName);
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
      if (view.View == null && !String.IsNullOrEmpty(_topicView)) {
        view = viewEngine.FindView(context, _topicView, MasterName);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Default to content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null) {
        view = viewEngine.FindView(context, contentType, MasterName);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt default search
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null) {
        view = base.FindView(context);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? Array.Empty<string>()).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return view, if found
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View != null) {
        return view;
      }
      return new ViewEngineResult(searchedPaths);

    }

  } //Class

} //Namespace
