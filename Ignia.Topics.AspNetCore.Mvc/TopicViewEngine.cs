/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Razor.Language;

namespace Ignia.Topics.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW ENGINE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a custom <see cref="RazorViewEngine"/> which provides custom support for organizing views by
  ///   <see cref="Topic.ContentType"/>.
  /// </summary>
  public class TopicViewEngine : RazorViewEngine {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of <see cref="TopicViewEngine"/>.
    /// </summary>
    /// <remarks>
    ///   When instantiated, the <see cref="TopicViewEngine.TopicViewEngine"/> constructor will initialize location formats with
    ///   extensions intended to support organizing views by <see cref="Topic.ContentType"/>.
    /// </remarks>
    public TopicViewEngine(IViewPageActivator viewPageActivator = null) : base(viewPageActivator) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define view location
      >-------------------------------------------------------------------------------------------------------------------------
      | Supports the following replacement tokens: {0} Controller, {1} View, {2} Area, and {3} Content Type.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewLocations = new[] {
        "~/Views/{3}/{1}.cshtml",
        "~/Views/ContentTypes/{3}.{1}.cshtml",
        "~/Views/ContentTypes/{1}.cshtml",
        "~/Views/Shared/{1}.cshtml",
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Set view locations
      \-----------------------------------------------------------------------------------------------------------------------*/
      ViewLocationFormats = ViewLocationFormats.Union(viewLocations).ToArray();
      MasterLocationFormats = MasterLocationFormats.Union(viewLocations).ToArray();
      PartialViewLocationFormats = PartialViewLocationFormats.Union(viewLocations).ToArray();

      /*------------------------------------------------------------------------------------------------------------------------
      | Update view locations for areas
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewLocations = viewLocations.Select(v => v.Replace("~", "~/{2}/")).ToArray();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set area view locations
      \-----------------------------------------------------------------------------------------------------------------------*/
      AreaViewLocationFormats = AreaViewLocationFormats.Union(viewLocations).ToArray();
      AreaMasterLocationFormats = AreaMasterLocationFormats.Union(viewLocations).ToArray();
      AreaPartialViewLocationFormats = AreaPartialViewLocationFormats.Union(viewLocations).ToArray();

    }

    /*==========================================================================================================================
    | METHOD: FIND PARTIAL VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provided a partial view name, determines if the view exists and, if it does, returns a new instance of it.
    /// </summary>
    /// <remarks>
    ///   Compared to the base <see cref="RazorViewEngine"/>, this override will look for <code>{3}</code> in the path pattern
    ///   and attempt to replace it with the <code>contenttype</code> <see cref="RouteDataCollection"/>.
    /// </remarks>
    /// <param name="controllerContext">The current <see cref="ControllerContext"/>.</param>
    /// <param name="viewName">The requested name of the view.</param>
    /// <param name="masterName">The requested name of the master (layout) view.</param>
    /// <param name="useCache">Determines whether the request is appropriate for caching.</param>
    public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify search paths
      \-----------------------------------------------------------------------------------------------------------------------*/
      var searchPaths = GetSearchPaths(controllerContext, partialViewName, PartialViewLocationFormats, useCache);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through patterns to identify views
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var path in searchPaths) {
        if (FileExists(controllerContext, path)) {
          return new ViewEngineResult(
            CreatePartialView(controllerContext, path),
            this
          );
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base processing, if view was not found
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new ViewEngineResult(searchPaths.ToArray());

    }

    /*==========================================================================================================================
    | METHOD: FIND VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provided a view name, determines if the view exists and, if it does, returns a new instance of it.
    /// </summary>
    /// <remarks>
    ///   Compared to the base <see cref="RazorViewEngine"/>, this override will look for <code>{3}</code> in the path pattern
    ///   and attempt to replace it with the <code>contenttype</code> <see cref="RouteDataCollection"/>.
    /// </remarks>
    /// <param name="controllerContext">The current <see cref="ControllerContext"/>.</param>
    /// <param name="viewName">The requested name of the view.</param>
    /// <param name="masterName">The requested name of the master (layout) view.</param>
    /// <param name="useCache">Determines whether the request is appropriate for caching.</param>
    public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify search paths
      \-----------------------------------------------------------------------------------------------------------------------*/
      var searchPaths = GetSearchPaths(controllerContext, viewName, ViewLocationFormats, useCache);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through patterns to identify views
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var path in searchPaths) {
        if (FileExists(controllerContext, path)) {
          return new ViewEngineResult(
            CreateView(controllerContext, path, masterName),
            this
          );
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base processing, if view was not found
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new ViewEngineResult(searchPaths.ToArray());

    }

    /*==========================================================================================================================
    | METHOD: GET SEARCH PATHS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provided the name of the view, and the context of the controller, identifies the paths that should be searched.
    /// </summary>
    /// <remarks>
    ///   Compared to the base <see cref="RazorViewEngine"/>, this override will look for <code>{3}</code> in the path pattern
    ///   and attempt to replace it with the <code>contenttype</code> <see cref="RouteDataCollection"/>.
    /// </remarks>
    /// <param name="controllerContext">The current <see cref="ControllerContext"/>.</param>
    /// <param name="viewName">The requested name of the view.</param>
    /// <param name="locationFormats">The list of path format patterns.</param>
    /// <param name="useCache">Determines whether the request is appropriate for caching.</param>
    private static List<string> GetSearchPaths(ControllerContext controllerContext, string viewName, string[] locationFormats, bool useCache) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routeData             = controllerContext.RouteData;
      var area                  = (object)"";
      var controller            = (object)"Topic";
      var contentType           = (object)null;
      var searchPaths           = new List<string>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      routeData.Values.TryGetValue("area", out area);
      routeData.Values.TryGetValue("controller", out controller);
      routeData.Values.TryGetValue("contenttype", out contentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through patterns to identify views
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var pathPattern in locationFormats) {
        if (!pathPattern.Contains("{3}") || !String.IsNullOrEmpty((string)contentType)) {
          var path = String.Format(pathPattern, controller, viewName, area, contentType);
          searchPaths.Add(path);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return list of views
      \-----------------------------------------------------------------------------------------------------------------------*/
      return searchPaths;

    }

  } //Class

} //Namespace
