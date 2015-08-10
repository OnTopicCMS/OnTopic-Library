/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Web.UI;
using System.Web.Security;
using System.Linq;

namespace Ignia.Topics.Web {

  /*============================================================================================================================
  | CLASS: TOPIC PAGE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a base class for Topic-related pages.
  /// </summary>
  public class TopicPage : Page {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
    private     bool                    _enableValidation                       = true;
    private     Topic                   _topic                                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicPage"/> class.
    /// </summary>
    public TopicPage() : base() { }

    /*==========================================================================================================================
    | PROPERTY: ENABLE VALIDATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   If enabled, <c>PageTopic</c> validation is performed, which checks for <c>IsDisabled</c>, <c>PageID</c>,
    ///   and <c>URL</c>.
    /// </summary>
    /// <remarks>
    ///   This can optionally be disabled, which is useful for the Editor.
    /// </remarks>
    public bool EnableValidation {
      get {
        return _enableValidation;
      }
      set {
        _enableValidation = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic associated with the page based on the RouteData.
    /// </summary>
    /// <remarks>
    ///   If the topic cannot be identified then a null reference is returned.
    /// </remarks>
    public Topic Topic {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Get cached instance
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_topic != null) return _topic;

        /*----------------------------------------------------------------------------------------------------------------------
        | Define page path
        \---------------------------------------------------------------------------------------------------------------------*/
        string    pagePath        = null;

        // Pull from RouteData
        if (Page.RouteData.Values["directory"] != null) {
          pagePath = (string)Page.RouteData.Values["directory"];
        }

        // Pull from QueryString
        if (String.IsNullOrEmpty(pagePath) && Request.QueryString["Path"] != null) {
          pagePath = Request.QueryString["Path"].ToString().Replace(":", "/");
        }

        // Default to blank
        pagePath = pagePath?? "";

        // Strip trailing colon
        if (pagePath.EndsWith(":")) {
          pagePath = pagePath.Substring(0, pagePath.Length-1);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Define variables
        \---------------------------------------------------------------------------------------------------------------------*/
        string    topicPath       = pagePath.Replace("/", ":");
        string    nameSpace       = pagePath.IndexOf("/") > 0? pagePath.Substring(0, pagePath.IndexOf("/")) : pagePath;
        Topic     rootTopic       = String.IsNullOrEmpty(nameSpace)? null : TopicRepository.RootTopic.GetTopic(nameSpace);

        /*----------------------------------------------------------------------------------------------------------------------
        | Lookup topic
        \---------------------------------------------------------------------------------------------------------------------*/
        _topic = TopicRepository.RootTopic.GetTopic(topicPath);

        if (rootTopic != null) {
          ValidatePageTopic(_topic);
        }
        else {
          _topic = TopicRepository.RootTopic.FirstOrDefault();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return topic
        \---------------------------------------------------------------------------------------------------------------------*/
        return _topic;

      }
      set {
        ValidatePageTopic(value);
        _topic = value;
      }
    }

    /*==========================================================================================================================
    | METHOD: VALIDATE PAGE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Verifies that the page topic is valid and performs any processing necessitated by the page topic, such as handling
    ///   a redirect.
    /// </summary>
    /// <param name="pageTopic">The topic object associated with the page.</param>
    public void ValidatePageTopic(Topic pageTopic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle missing or disabled topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (EnableValidation && pageTopic != null && pageTopic.GetAttribute("IsDisabled", true).Equals("1")) {
        if (!Roles.IsUserInRole(Page.User.Identity.Name, "Administrators")) {
          if (Request.QueryString["PageID"] != null) {
            Response.Redirect("/Redirector.aspx?PageID=" + Request.QueryString["PageID"]);
          }
          Response.StatusCode = 404;
          Response.SuppressContent = true;
          Response.End();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle redirect
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (EnableValidation && pageTopic != null && !String.IsNullOrEmpty(pageTopic.GetAttribute("URL"))) {
        Response.RedirectPermanent(pageTopic.GetAttribute("URL"), true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set title
      \-----------------------------------------------------------------------------------------------------------------------*/
      Page.Title = ((pageTopic != null)? pageTopic.GetAttribute("MetaTitle", pageTopic.GetAttribute("Title", pageTopic.Key)) : "");

    }

    /*==========================================================================================================================
    | EDIT URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Identifies the appropriate route to edit the current page.
    /// </summary>
    /// <remarks>
    ///   ###TODO KLT081015: Wire up to the <c><editor /></c> element <c>location</c> attribute in the configuration. 
    /// </remarks>
    public String EditUrl {
      get {
        return "/!Admin/Topics/Default.aspx?Path=" + this.Topic.UniqueKey;
      }
    }

  } //Class

} //Namespace