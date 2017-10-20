/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Diagnostics.Contracts;
using Ignia.Topics.Repositories;
using System.Collections.Specialized;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC ROUTING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a URL, ITopicProvider, and views directory, will determine the associated Topic, as well as the appropriate view.
  /// </summary>
  /// <remarks>
  ///   The view location will change based on the environment. That said, in all cases, the view will factor in the topic's
  ///   Content Type and (if set) View properties, and optionally the view determined via the URL (as either a URL part, or a
  ///   query string parameter. For instance, a view can be set as /a/b/c/viewName/ or /a/b/c/?View=viewName. Inherits from the
  ///   CLR <seealso cref="System.Uri"/> class to provide a familiar interface with additional useful capabilities.
  /// </remarks>
  public class ViewRoutingService : TopicRoutingService {

    /*============================================================================================================================
    | PRIVATE VARIABLES
    \---------------------------------------------------------------------------------------------------------------------------*/
    private                     NameValueCollection             _headers                        = null;
    private                     List<string>                    _views                          = null;
    private                     Uri                             _uri                            = null;
    private                     string                          _view                           = null;
    private                     string                          _viewsDirectory                 = null;
    private                     string                          _localViewsDirectory            = null;
    private                     string                          _viewExtension                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingService"/> class based on a URL instance, a fully qualified
    ///   path to the views Directory, and, optionally, the expected filename suffix fo each view file.
    /// </summary>
    public ViewRoutingService(
      ITopicRepository          topicRepository,
      Uri                       uri,
      RouteData                 routeData                       = null,
      NameValueCollection       headers                         = null,
      string                    localViewsDirectory             = null,
      string                    viewsDirectory                  = "~/Views/",
      string                    viewExtension                   = "cshtml"
     ) : base(topicRepository, uri, routeData) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository != null, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(uri != null, "An instance of a Uri instantiated to the requested URL is required.");
      Contract.Requires(viewsDirectory != null, "A value for the viewsDirectory is required. Will default to '~/Views/'");
      Contract.Requires(viewExtension != null, "A value for the viewExtension is required. Will default to 'cshtml'");
      Contract.Requires(viewsDirectory.IndexOf("/") >= 0, "The viewsDirectory parameter should be a relative path (e.g., '/Views/`).");
      Contract.Requires(viewExtension.IndexOf(".") < 0, "The viewExtension parameter only contain the extension value (e.g., 'cshtml').");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _uri                      = uri;
      _headers                  = headers?? new NameValueCollection(0);
      _localViewsDirectory      = localViewsDirectory;
      _viewsDirectory           = viewsDirectory;
      _viewExtension            = viewExtension;

    }

    /// <summary>
    ///   Initializes a new instance of the <see cref ="TopicRoutingService"/> class based on a URL instance, a fully qualified
    ///   path to the views Directory, and, optionally, the expected filename suffix fo each view file.
    /// </summary>
    public ViewRoutingService(
      ITopicRepository          topicRepository,
      RequestContext            requestContext,
      string                    viewsDirectory                  = "~/Views/",
      string                    viewExtension                   = "cshtml"
     ) : base(topicRepository, requestContext.HttpContext.Request.Url, requestContext.RouteData) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository != null, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(requestContext != null, "An instance of a RequestContext is required.");
      Contract.Requires(viewsDirectory != null, "A value for the viewsDirectory is required. Will default to '~/Views/'");
      Contract.Requires(viewExtension != null, "A value for the viewExtension is required. Will default to 'cshtml'");
      Contract.Requires(viewsDirectory.IndexOf("/") >= 0, "The viewsDirectory parameter should be a relative path (e.g., '/Views/`).");
      Contract.Requires(viewExtension.IndexOf(".") < 0, "The viewExtension parameter only contain the extension value (e.g., 'cshtml').");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _uri                      = requestContext.HttpContext.Request.Url;
      _headers                  = requestContext.HttpContext.Request.Headers;
      _localViewsDirectory      = requestContext.HttpContext.Server.MapPath(viewsDirectory);
      _viewsDirectory           = viewsDirectory;
      _viewExtension            = viewExtension;

    }

    /*==========================================================================================================================
    | PROPERTY: LOCAL VIEWS DIRECTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected location for View files. Must be set during the constructor.
    /// </summary>
    public string LocalViewsDirectory {
      get => _localViewsDirectory;
      private set => _localViewsDirectory = value;
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the view name based on the content type, in combination with the query string, headers, and
    ///   topic attributes.
    /// </summary>
    public string View {
      get {

        /*------------------------------------------------------------------------------------------------------------------------
        | Return view, if found
        \-----------------------------------------------------------------------------------------------------------------------*/
        if (_view != null) return _view;

        /*------------------------------------------------------------------------------------------------------------------------
        | Set variables
        \-----------------------------------------------------------------------------------------------------------------------*/
        var topic = Topic;
        var contentType = ContentType;

        /*------------------------------------------------------------------------------------------------------------------------
        | Derive expected view
        >-------------------------------------------------------------------------------------------------------------------------
        | Check expected page view type and validate against available View (page) templates, based on the following fallback
        | structure: user source (QueryString), Accept header, the Topic's View Attribute (with the default set as the Topic's
        | ContentType Topic View Attribute), and finally to the Content Type name.
        \-----------------------------------------------------------------------------------------------------------------------*/
        string viewName = null;

        // Pull from QueryString
        if (viewName == null && QueryParameters["View"] != null) {
          IsValidView(contentType, QueryParameters["View"].ToString(), out viewName);
        }

        /*------------------------------------------------------------------------------------------------------------------------
        | Pull Headers
        >-------------------------------------------------------------------------------------------------------------------------
        | ### TODO JJC071617: This introduces an unwanted dependency on HttpContext. While this is possible, it may introduce
        | incompatibilities with the context (e.g., do ASP.NET MVC and ASP.NET Web Forms expose the same HttpContext objects?
        | Preferrably, this would be handled by each environment as appropriate, but that might muddle the logic.
        \-----------------------------------------------------------------------------------------------------------------------*/
        if (viewName == null && _headers["Accept"] != null) {
          var acceptHeaders = _headers["Accept"].ToString();
          var splitHeaders = acceptHeaders.Split(new char[] { ',', ';' });
          // Validate the content-type after the slash, then validate it against available views
          for (var i = 0; i < splitHeaders.Length; i++) {
            if (splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
              // Get content-type after the slash and replace '+' characters in the content-type to '-' for view file encoding purposes
              var acceptHeader = splitHeaders[i].Substring(splitHeaders[i].IndexOf("/") + 1).Replace("+", "-");
              // Validate against available views; if content-type represents a valid view, stop validation
              if (IsValidView(contentType, acceptHeader, out viewName)) {
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
        if (viewName == null && !String.IsNullOrEmpty(topic.View)) {
          IsValidView(contentType, topic.View, out viewName);
        }

        /*------------------------------------------------------------------------------------------------------------------------
        | Default to content type
        \-----------------------------------------------------------------------------------------------------------------------*/
        if (viewName == null) {
          IsValidView(contentType, contentType, out viewName);
        }

        /*------------------------------------------------------------------------------------------------------------------------
        | Set view, and return
        \-----------------------------------------------------------------------------------------------------------------------*/
        _view = viewName;
        return _view;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: VIEWS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Iterates through available 'view' template files within the specified directory, collecting them to make them
    ///   available for validation against a specified View (and consequently, target path).
    /// </summary>
    private List<string> Views {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Define assumptions
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_views == null) {

          /*--------------------------------------------------------------------------------------------------------------------
          | Define view template search variables
          \-------------------------------------------------------------------------------------------------------------------*/
          var views = new List<string>();
          var searchPattern = "*." + _viewExtension;
          var viewsDirectoryInfo = new DirectoryInfo(LocalViewsDirectory);
          var searchOption = SearchOption.TopDirectoryOnly;
          var subDirectories = viewsDirectoryInfo.GetDirectories("*", SearchOption.AllDirectories);

          /*--------------------------------------------------------------------------------------------------------------------
          | Disvoer all view templates available via the configured path
          \-------------------------------------------------------------------------------------------------------------------*/
          // Get top-level (generic) view files
          foreach (var file in viewsDirectoryInfo.GetFiles(searchPattern, searchOption)) {
            // Strip off the extension (must do even for the FileInfo instance)
            var fileName = file.Name.ToLower().Replace("." + _viewExtension, "");
            views.Add(fileName);
          }
          // Get view files specific to Content Type
          foreach (var subDirectory in subDirectories) {
            var subDirectoryName = subDirectory.Name;
            foreach (var file in subDirectory.GetFiles(searchPattern, searchOption)) {
              var fileName = file.Name.ToLower().Replace("." + _viewExtension, "");
              views.Add(subDirectoryName + "/" + fileName);
            }
          }

          /*--------------------------------------------------------------------------------------------------------------------
          | Set views
          \-------------------------------------------------------------------------------------------------------------------*/
          _views = views;

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return views
        \---------------------------------------------------------------------------------------------------------------------*/
        return _views;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the full path to the target view, including the views directory extension.
    /// </summary>
    public string ViewPath {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        return _viewsDirectory + View + "." + _viewExtension;
      }

    }

    /*==========================================================================================================================
    | PROPERTY: IS VALID VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Checks the specified view name against its availability in the <see cref="Views"/> collection.
    /// </summary>
    /// <param name="contentType">The name of the topic's <see cref="Ignia.Topics.ContentTypeDescriptor"/>.</param>
    /// <param name="viewName">The filename (minus extension) of the view.</param>
    /// <param name="matchedView">The string identifier for the matched view.</param>
    /// <returns>
    ///   A boolean value representing the view's validity as well as the output variable 'matchedView', indicating the
    ///   <see cref="Topic.View"/> name.
    /// </returns>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   contentType != null
    /// </requires>
    public bool IsValidView(string contentType, string viewName, out string matchedView) {

      /*-------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \------------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(contentType != null, "contentType");
      Topic.ValidateKey(contentType);

      /*-------------------------------------------------------------------------------------------------------------------------
      | Check for content type specific view
      \------------------------------------------------------------------------------------------------------------------------*/
      if (
        !String.IsNullOrEmpty(viewName) &&
        Views.Contains(contentType + "/" + viewName, StringComparer.InvariantCultureIgnoreCase)
        ) {
        matchedView = contentType + "/" + viewName;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Check for generic view
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (!String.IsNullOrEmpty(viewName) && Views.Contains(viewName, StringComparer.InvariantCultureIgnoreCase)) {
        matchedView = viewName;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return null (invalid) view
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {
        matchedView = null;
        return false;
      }
      return true;
    }

    /*==========================================================================================================================
    | QUERY PARAMETERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates the <see cref="Uri.Query"/> to identify any parameters set and return them as a
    ///   <see cref="Dictionary{String, String}"/> object.
    /// </summary>
    /// <returns>An <see cref="Dictionary{String, String}"/> instance of key/value pairs.</returns>
    private NameValueCollection QueryParameters {
      get {
        return HttpUtility.ParseQueryString(_uri.Query);
      }
    }

  } // Class

} // Namespace
