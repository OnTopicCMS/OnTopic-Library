/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.Routing;
using System.Web.Compilation;
using Ignia.Topics.Web.Configuration;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Web {

  /*============================================================================================================================
  | CLASS: TOPICS ROUTE HANDLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides routing for any path that matches a path managed by the Topics database.
  /// </summary>
  /// <remarks>
  ///   If a match is found, then the user is routed to a template corresponding to the Topic's <see cref="Topic.ContentType"/>.
  ///   Otherwise, the originally-requested page is rendered (although this may yield a 404).
  /// </remarks>
  public class TopicsRouteHandler : IRouteHandler {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     List<string>    _views                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicsRouteHandler"/> class.
    /// </summary>
    public TopicsRouteHandler() { }

    /*==========================================================================================================================
    | PROPERTY: VIEWS PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected location for View files; attempts to retrieve the value from the <c><topics /></c> configuration
    ///   section, but defaults to ~/Common/Templates/.
    /// </summary>
    private string ViewsPath {
      get {
        string                  viewsPath               = "~/Common/Templates/";
        // Use configuration settings, if available
        TopicsSection           topicsSection           = (TopicsSection)ConfigurationManager.GetSection("topics");
        if (topicsSection != null && topicsSection.Views != null && !String.IsNullOrEmpty(topicsSection.Views.Path)) {
          viewsPath                                     = topicsSection.Views.Path;
        }
        return viewsPath;
      }
    }

    /*==========================================================================================================================
    | GET HTTP HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates the <see cref="RouteData"/> to identify the most appropriate HTTP Handler to use, then returns an instance
    ///   of that handler.
    /// </summary>
    /// <param name="requestContext">The HTTP request context.</param>
    /// <returns>An <see cref="IHttpHandler"/> instance.</returns>
    /// <requires description="The HTTP request context must be provided." exception="T:System.ArgumentNullException">
    ///   requestContext != null
    /// </requires>
    /// <exception cref="Exception">
    ///   The ContentType for the Topic <c>topic.UniqueKey</c> (<c>topic.Id</c>) is not set. Set the ContentType value of the
    ///   Topic based on the template that should be associated with it. E.g., a standard page will have the ContentType of
    ///   Page.
    /// </exception>
    public IHttpHandler GetHttpHandler(RequestContext requestContext) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Define assumptions
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(requestContext.RouteData != null, "Assumes the request context and route data are available.");
      Contract.Assume(requestContext.RouteData.Values != null, "Assumes route data values are available.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set variables
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE: JJC071617: Most of these variables are only needed to establish the routing variables. If those are not 
      | actually needed for backward compatibility, then much of this can be removed. Alternatively, it can be inferred from the
      | TopicRoutingService, if there's some flexibility on the exact format.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var       routeData                       = requestContext.RouteData;
      var       nameSpace                       = (string)routeData.Values["namespace"]?? "";
      var       path                            = (string)routeData.Values["path"]?? "";
      var       directory                       = nameSpace + "/" + path;

      if (directory.StartsWith("/"))            directory       = directory.Substring(1);
      if (directory.EndsWith("/"))              directory       = directory.Substring(0, directory.Length-1);
      if (path.EndsWith("/"))                   path            = path.Substring(0, path.Length-1);

      /*------------------------------------------------------------------------------------------------------------------------
      | Get topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var       topicRoutingService             = new TopicRoutingService(TopicRepository.DataProvider, requestContext, ViewsPath, "aspx");
      var       topic                           = topicRoutingService.Topic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate path
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) {
        return BuildManager.CreateInstanceFromVirtualPath("~/" + path, typeof(Page)) as IHttpHandler;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set route variables
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE: JJC071617: See above with regard to whether these routes are still necessary.
      \-----------------------------------------------------------------------------------------------------------------------*/
      string    contentType                     = topic.ContentType;
      var       firstColon                      = topic.UniqueKey.IndexOf(":");

      if (String.IsNullOrEmpty(nameSpace) && firstColon >= 0) {
        nameSpace                               = topic.UniqueKey.Substring(0, firstColon);
        path                                    = topic.UniqueKey.Substring(firstColon+1);
      }

      routeData.Values["contentType"]           = topicRoutingService.ContentType;
      routeData.Values["directory"]             = directory;
      routeData.Values["path"]                  = path;

      /*------------------------------------------------------------------------------------------------------------------------
      | Derive expected view
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewName = topicRoutingService.View;

      /*------------------------------------------------------------------------------------------------------------------------
      | Set target path
      \-----------------------------------------------------------------------------------------------------------------------*/
      string    targetPath                      = ViewsPath + viewName + ".aspx";

      requestContext.RouteData.Values["targetPath"] = targetPath;

      /*------------------------------------------------------------------------------------------------------------------------
      | SET TARGET TYPES
      >-------------------------------------------------------------------------------------------------------------------------
      | ###REM JJC101713: Failed attempt to inject strongly typed Topic into page instantiation.  May revisit later.
      >-------------------------------------------------------------------------------------------------------------------------
      Type      topicPageType           = typeof(TypedTopicPage<>);
      Type      topicType               = topic.GetType();
      Type      typedPageType           = topicPageType.MakeGenericType(new Type[] {topicType});
      \-----------------------------------------------------------------------------------------------------------------------*/

      /*------------------------------------------------------------------------------------------------------------------------
      | Return page handler
      \-----------------------------------------------------------------------------------------------------------------------*/
      return BuildManager.CreateInstanceFromVirtualPath(targetPath, typeof(TopicPage)) as IHttpHandler;

    }

  } // Class

} // Namespace
