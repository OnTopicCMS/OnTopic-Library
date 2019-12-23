/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Routing;
using OnTopic.Internal.Diagnostics;
using OnTopic.Repositories;

namespace OnTopic {

  /*============================================================================================================================
  | CLASS: TOPIC ROUTING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given an ITopicRepository, URL, and views directory, will determine the associated Topic, as well as the appropriate
  ///   view.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The view location will change based on the environment. That said, in all cases, the view will factor in the topic's
  ///     Content Type and (if set) View properties, and optionally the view determined via the URL (as either a URL part, or a
  ///     query string parameter. For instance, a view can be set as /a/b/c/viewName/ or /a/b/c/?View=viewName. Inherits from
  ///     the CLR <seealso cref="System.Uri"/> class to provide a familiar interface with additional useful capabilities.
  ///   </para>
  ///   <para>
  ///     The <see cref="WebFormsRoutingService"/> is distributed with, and intended exclusively for use with, the ASP.NET
  ///     WebForms version of OnTopic. Since this version is intended exclusively for maintaining backward compatibility with
  ///     legacy websites, testability is not a priority; legacy applications requiring updates should be updated to supported
  ///     versions of OnTopic. In addition, since Dependency Injection is not utilized by the legacy version, it adheres to the
  ///     <see cref="ITopicRoutingService"/> interface, but also exposes additional properties used exclusively by the ASP.NET
  ///     WebForms version. For instance, it additionally provides view routing capabilities.
  ///   </para>
  /// </remarks>
  public class WebFormsTopicRoutingService : ITopicRoutingService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository                = null;
    private readonly            RouteData                       _routes                         = null;
    private readonly            Uri                             _uri                            = null;
    private                     Topic                           _topic                          = null;
    private                     List<string>                    _views                          = null;
    private                     string                          _view                           = null;
    private readonly            NameValueCollection             _headers                        = null;
    private readonly            string                          _viewsDirectory                 = null;
    private readonly            string                          _viewExtension                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingService"/> class based on a URL instance, a fully qualified
    ///   path to the views Directory, and, optionally, the expected filename suffix of each view file.
    /// </summary>
    /// <remarks>
    ///   Because the <see cref="ViewRoutingService"/> is distributed with, and intended exclusively for use with, the ASP.NET
    ///   WebForms version of OnTopic, it does not adhere to strict Dependency Injection rules; it is considered an
    ///   infrastructure component, and not an application library. As a result, it not only includes optional parameters, but
    ///   also makes no effort to abstract out the <see cref="RequestContext"/>.
    /// </remarks>
    public WebFormsTopicRoutingService(
      ITopicRepository          topicRepository,
      RequestContext            requestContext,
      string                    viewsDirectory                  = "~/Common/Templates/",
      string                    viewExtension                   = "aspx"
     ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, "A concrete implementation of an ITopicRepository is required.");
      Contract.Requires(requestContext, "An instance of a RequestContext is required.");
      Contract.Requires(viewsDirectory, "The viewsDirectory must be set.");
      Contract.Requires(viewExtension, "The viewExtension must be set.");
      Contract.Requires(
        viewsDirectory.IndexOf("/", StringComparison.InvariantCulture) >= 0,
        "The viewsDirectory parameter should be a relative path (e.g., '/Views/`)."
      );
      Contract.Requires(
        viewExtension.IndexOf(".", StringComparison.InvariantCulture) < 0,
        "The viewExtension parameter only contain the extension value (e.g., 'cshtml')."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = topicRepository;
      _uri                      = requestContext.HttpContext.Request.Url;
      _headers                  = requestContext.HttpContext.Request.Headers;
      _routes                   = requestContext.RouteData;
      _viewsDirectory           = viewsDirectory;
      _viewExtension            = viewExtension.ToUpperInvariant();

      LocalViewsDirectory       = requestContext.HttpContext.Server.MapPath(viewsDirectory);

    }

    /*==========================================================================================================================
    | METHOD: GET CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public Topic GetCurrentTopic() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topic == null) {
        var path = _uri.AbsolutePath;
        if (_routes.Values.ContainsKey("path")) {
          path = _routes.GetRequiredString("path");
          if (_routes.Values.ContainsKey("rootTopic")) {
            path = _routes.GetRequiredString("rootTopic") + "/" + path;
          }
        }
        path = path.Trim(new char[] { '/' }).Replace("//", "/");
        _topic = _topicRepository.Load(path.Replace("/", ":"));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set route data
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_topic != null) {
        if (_routes.Values.ContainsKey("contenttype")) {
          _routes.Values.Remove("contenttype");
        }
        _routes.Values.Add("contenttype", _topic.ContentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return Topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _topic;

    }

    /*==========================================================================================================================
    | PROPERTY: LOCAL VIEWS DIRECTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected location for View files. Must be set during the constructor.
    /// </summary>
    public string LocalViewsDirectory { get; set; }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines the view name based on the content type, in combination with the query string, headers, and
    ///   topic attributes.
    /// </summary>
    public string View {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Return view, if found
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_view != null) return _view;

        /*----------------------------------------------------------------------------------------------------------------------
        | Set variables
        \---------------------------------------------------------------------------------------------------------------------*/
        var topic = GetCurrentTopic();
        var contentType = topic.ContentType;

        /*----------------------------------------------------------------------------------------------------------------------
        | Derive expected view
        >-----------------------------------------------------------------------------------------------------------------------
        | Check expected page view type and validate against available View (page) templates, based on the following fallback
        | structure: user source (QueryString), Accept header, the Topic's View Attribute (with the default set as the Topic's
        | ContentType Topic View Attribute), and finally to the Content Type name.
        \---------------------------------------------------------------------------------------------------------------------*/
        string viewName = null;

        // Pull from QueryString
        if (viewName == null && QueryParameters["View"] != null) {
          IsValidView(contentType, QueryParameters["View"].ToString(CultureInfo.InvariantCulture), out viewName);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Pull Headers
        >-----------------------------------------------------------------------------------------------------------------------
        | ### TODO JJC071617: This introduces an unwanted dependency on HttpContext. While this is possible, it may introduce
        | incompatibilities with the context (e.g., do ASP.NET MVC and ASP.NET Web Forms expose the same HttpContext objects?
        | Preferably, this would be handled by each environment as appropriate, but that might muddle the logic.
        \---------------------------------------------------------------------------------------------------------------------*/
        if (viewName == null && _headers["Accept"] != null) {
          var acceptHeaders = _headers["Accept"].ToString(CultureInfo.InvariantCulture);
          var splitHeaders = acceptHeaders.Split(new char[] { ',', ';' });
          // Validate the content-type after the slash, then validate it against available views
          for (var i = 0; i < splitHeaders.Length; i++) {
            if (splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
              // Get content-type after the slash and replace '+' characters in the content-type to '-' for view file
              // purposes
              var acceptHeader = splitHeaders[i]
                .Substring(splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1)
                .Replace("+", "-");
              // Validate against available views; if content-type represents a valid view, stop validation
              if (IsValidView(contentType, acceptHeader, out viewName)) {
                break;
              }
            }
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Pull from topic attribute
        >-----------------------------------------------------------------------------------------------------------------------
        | Pull from Topic's View Attribute; additional check against the Topic's ContentType Topic View Attribute is not
        | necessary as it is set as the default View value for the Topic
        \---------------------------------------------------------------------------------------------------------------------*/
        if (viewName == null && !String.IsNullOrEmpty(topic.View)) {
          IsValidView(contentType, topic.View, out viewName);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Default to content type
        \---------------------------------------------------------------------------------------------------------------------*/
        if (viewName == null) {
          IsValidView(contentType, contentType, out viewName);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Set view, and return
        \---------------------------------------------------------------------------------------------------------------------*/
        _view = viewName?? "Page";
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
          | Discover all view templates available via the configured path
          \-------------------------------------------------------------------------------------------------------------------*/
          // Get top-level (generic) view files
          foreach (var file in viewsDirectoryInfo.GetFiles(searchPattern, searchOption)) {
            // Strip off the extension (must do even for the FileInfo instance)
            var fileName = file.Name.ToUpperInvariant().Replace("." + _viewExtension, "");
            views.Add(fileName);
          }
          // Get view files specific to Content Type
          foreach (var subDirectory in subDirectories) {
            var subDirectoryName = subDirectory.Name.ToUpperInvariant();
            foreach (var file in subDirectory.GetFiles(searchPattern, searchOption)) {
              var fileName = file.Name.ToUpperInvariant().Replace("." + _viewExtension, "");
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
    public string ViewPath => _viewsDirectory + View + "." + _viewExtension;

    /*==========================================================================================================================
    | PROPERTY: IS VALID VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Checks the specified view name against its availability in the <see cref="Views"/> collection.
    /// </summary>
    /// <param name="contentType">The name of the topic's <see cref="OnTopic.ContentTypeDescriptor"/>.</param>
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

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(contentType, "contentType");
      TopicFactory.ValidateKey(contentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Check for content type specific view
      \-----------------------------------------------------------------------------------------------------------------------*/
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
    private NameValueCollection QueryParameters => HttpUtility.ParseQueryString(_uri.Query);

  } //Class
} //Namespace