﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Ignia.Topics.Web.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW RESULT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom version of <see cref="ViewResult" /> capable of determining the most appropriate view.
  /// </summary>
  public class TopicViewResult : ViewResult {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instances of a <see cref="TopicViewResult"/> based on a <see cref="TopicViewModel"/>.
    /// </summary>
    /// <remarks>
    ///   All topic related views should be strongly typed to a <see cref="TopicViewModel"/> or derivative. This is also
    ///   necessary to provide the <see cref="FindView(ControllerContext)"/> method with a reference to the <see cref="Topic"/>
    ///   class, which informs it of the default <see cref="Topic.View"/> for the current topic, if set.
    /// </remarks>
    public TopicViewResult(TopicViewModel viewModel) : base() {
      ViewData.Model = viewModel;
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
      | Set variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       topic                           = ((TopicViewModel)ViewData.Model).Topic;
      var                       contentType                     = topic.ContentType;
      var                       viewEngine                      = ViewEngines.Engines;
      var                       requestContext                  = context.HttpContext.Request;
      var                       view                            = new ViewEngineResult(new string[] { });
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
          searchedPaths = searchedPaths.Union(view.SearchedLocations?? new string[] { }).ToList();
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
            var acceptHeader = splitHeaders[i].Substring(splitHeaders[i].IndexOf("/") + 1).Replace("+", "-");
            // Validate against available views; if content-type represents a valid view, stop validation
            if (acceptHeader != null) {
              view = viewEngine.FindView(context, acceptHeader, MasterName);
              searchedPaths = searchedPaths.Union(view.SearchedLocations ?? new string[] { }).ToList();
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
      if (view.View == null && !String.IsNullOrEmpty(topic.View)) {
        view = viewEngine.FindView(context, topic.View, MasterName);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? new string[] { }).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Default to content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null) {
        view = viewEngine.FindView(context, contentType, MasterName);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? new string[] { }).ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt default search
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (view.View == null) {
        view = base.FindView(context);
        searchedPaths = searchedPaths.Union(view.SearchedLocations ?? new string[] { }).ToList();
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