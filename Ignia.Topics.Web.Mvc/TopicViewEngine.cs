/*==============================================================================================================================
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
    | METHOD: FIND VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a view name, determines if the view exists and, if it does, returns a new instance of it.
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
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routeData             = controllerContext.RouteData;
      var area                  = "";
      var controller            = "Topic";
      var contentType           = "Page";
      var searchedPaths         = new List<string>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (routeData.Values.ContainsKey("area")) {
        area = routeData.GetRequiredString("area");
      }
      if (routeData.Values.ContainsKey("controller")) {
        area = routeData.GetRequiredString("controller");
      }
      if (routeData.Values.ContainsKey("contenttype")) {
        contentType = routeData.GetRequiredString("contenttype");
      }
      else {
        throw new Exception("The route `contenttype` was not found. This should be defined in the `IControllerFactory`.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through patterns to identify views
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var pathPattern in ViewLocationFormats) {
        var path = String.Format(pathPattern, controller, viewName, area, contentType);
        searchedPaths.Add(path);
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
      //return base.FindView(controllerContext, viewName, masterName, useCache);
      return new ViewEngineResult(searchedPaths.ToArray());

    }

  } //Class

} //Namespace
