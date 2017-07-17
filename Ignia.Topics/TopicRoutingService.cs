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
  public class TopicRoutingService : Uri {

    /*============================================================================================================================
    | PRIVATE VARIABLES
    \---------------------------------------------------------------------------------------------------------------------------*/
    private     ITopicRepository        _topicRepository                = null;
    private     RequestContext          _requestContext                 = null;
    private     string                  _contentType                    = null;
    private     List<string>            _views                          = null;
    private     string                  _view                           = null;
    private     string                  _viewsPath                      = null;
    private     string                  _localViewsPath                 = null;
    private     string                  _viewExtension                  = null;
    private     Topic                   _topic                          = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingService"/> class based on a URL instance, a fully qualified 
    ///   path to the views folder, and, optionally, the expected filename suffix fo each view file. 
    /// </summary>
    public TopicRoutingService(
      ITopicRepository topicRepository,
      RequestContext requestContext,
      string viewsPath = "~/Shared/Views/", 
      string viewExtension = ".cshtml"
     ) : base(requestContext.HttpContext.Request.Url.ToString()) {
      _topicRepository = topicRepository;
      _requestContext = requestContext;
      _viewsPath = viewsPath;
      _viewExtension = viewExtension;
    }

    /*==========================================================================================================================
    | PROPERTY: VIEWS PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected location for View files. Must be set during the constructor. 
    /// </summary>
    public string ViewsPath {
      get {
        return _viewsPath;
      }
      private set {
        _viewsPath = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: LOCAL VIEWS PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected location for View files. Must be set during the constructor. 
    /// </summary>
    public string LocalViewsPath {
      get {
        if (_localViewsPath == null) {
          _requestContext.HttpContext.Request.MapPath(ViewsPath);
        }
        return _localViewsPath;
      }
      private set {
        _localViewsPath = value;
      }
    }


    /*==========================================================================================================================
    | PROPERTY: VIEW SUFFIX
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the expected file suffix for View files. Can be set via the appropriate constructor overload. Defaults to 
    ///   ".cshtml".
    /// </summary>
    public string ViewExtension {
      get {
        return _viewExtension;
      }
      private set {
        _viewExtension = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the topic associated with the current URL.
    /// </summary>
    public Topic Topic {
      get {

        /*------------------------------------------------------------------------------------------------------------------------
        | Validate Topic
        \-----------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(_topic != null, "A topic was not able to be found that maps to the current URL.");

        /*------------------------------------------------------------------------------------------------------------------------
        | Retrieve Topic via URL
        \-----------------------------------------------------------------------------------------------------------------------*/
        if (_topic == null) {
          var directory = this.AbsolutePath;
          if (directory.StartsWith("/")) directory = directory.Substring(1);
          if (directory.EndsWith("/")) directory = directory.Substring(0, directory.Length - 1);
          _topic = _topicRepository.Load().GetTopic(directory.Replace("/", ":"));
        }

        /*------------------------------------------------------------------------------------------------------------------------
        | Return Topic
        \-----------------------------------------------------------------------------------------------------------------------*/
        return _topic;

      }
      private set {
        _topic = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the content type associated with the topic associated with the current URL.
    /// </summary>
    public string ContentType {
      get {

        /*------------------------------------------------------------------------------------------------------------------------
        | Validate Topic
        \-----------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Topic.ContentType != null, "The current topic is not associated with a valid content type.");

        /*------------------------------------------------------------------------------------------------------------------------
        | Return Topic
        \-----------------------------------------------------------------------------------------------------------------------*/
        return Topic.ContentType;

      }
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
        Topic topic = this.Topic;
        string contentType = this.ContentType;

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
        if (viewName == null && _requestContext.HttpContext.Request.Headers["Accept"] != null) {
          string acceptHeaders = _requestContext.HttpContext.Request.Headers["Accept"].ToString();
          string[] splitHeaders = acceptHeaders.Split(new Char[] { ',', ';' });
          // Validate the content-type after the slash, then validate it against available views
          for (int i = 0; i < splitHeaders.Length; i++) {
            if (splitHeaders[i].IndexOf("/", StringComparison.InvariantCultureIgnoreCase) >= 0) {
              // Get content-type after the slash and replace '+' characters in the content-type to '-' for view file encoding purposes
              string acceptHeader = splitHeaders[i].Substring(splitHeaders[i].IndexOf("/") + 1).Replace("+", "-");
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
          viewName = contentType;
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
          List<string> views = new List<string>();
          string searchPattern = "*." + ViewExtension;
          DirectoryInfo viewsDirectoryInfo = new DirectoryInfo(_requestContext.HttpContext.Request.MapPath(ViewsPath));
          SearchOption searchOption = SearchOption.TopDirectoryOnly;
          DirectoryInfo[] subDirectories = viewsDirectoryInfo?.GetDirectories("*", SearchOption.AllDirectories);

          /*--------------------------------------------------------------------------------------------------------------------
          | Disvoer all view templates available via the configured path
          \-------------------------------------------------------------------------------------------------------------------*/
          // Get top-level (generic) view files
          foreach (FileInfo file in viewsDirectoryInfo.GetFiles(searchPattern, searchOption)) {
            // Strip off the extension (must do even for the FileInfo instance)
            string fileName = file.Name.ToLower().Replace("." + ViewExtension, "");
            views.Add(fileName);
          }
          // Get view files specific to Content Type
          foreach (DirectoryInfo subDirectory in subDirectories) {
            string subDirectoryName = subDirectory.Name;
            foreach (FileInfo file in subDirectory.GetFiles(searchPattern, searchOption)) {
              string fileName = file.Name.ToLower().Replace("." + ViewExtension, "");
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
    | PROPERTY: IS VALID VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Checks the specified view name against its availability in the <see cref="Views"/> collection.
    /// </summary>
    /// <param name="contentType">The name of the topic's <see cref="Ignia.Topics.ContentType"/>.</param>
    /// <param name="viewName">The filename (minus extension) of the view.</param>
    /// <param name="matchedView">The string identifier for the matched view.</param>
    /// <returns>
    ///   A boolean value representing the view's validity as well as the output variable 'matchedView', indicating the
    ///   <see cref="Topic.View"/> name.
    /// </returns>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   contentType != null
    /// </requires>
    private bool IsValidView(string contentType, string viewName, out string matchedView) {

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
        Views.Contains(contentType + "\\" + viewName, StringComparer.InvariantCultureIgnoreCase) &&
        File.Exists(ViewsPath + "\\" + contentType + "\\" + viewName + "." + ViewExtension)
        ) {
        matchedView = contentType + "\\" + viewName;
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
    public NameValueCollection QueryParameters {
      get {
        return HttpUtility.ParseQueryString(this.Query);
      }
    }

  } // Class

} // Namespace
