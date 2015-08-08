/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Editor
|
| Purpose       Provides a base class for Topic-related pages.
|
\=============================================================================================================================*/
using System;
using System.Web.UI;
using System.Web.Security;
using System.Linq;

namespace Ignia.Topics.Web {

  /*===========================================================================================================================
  | CLASS
  \--------------------------------------------------------------------------------------------------------------------------*/
  public class TopicPage : Page {

  /*=========================================================================================================================
  | DECLARE PRIVATE VARIABLES
  >==========================================================================================================================
  | Declare variables for property use
  \------------------------------------------------------------------------------------------------------------------------*/
    private     bool                    _enableValidation                       = true;
    private     Topic                   _topic                                  = null;

  /*=========================================================================================================================
  | CONSTRUCTOR
  \------------------------------------------------------------------------------------------------------------------------*/
    public TopicPage() : base() { }

  /*=========================================================================================================================
  | PROPERTY: ENABLE VALIDATION
  >==========================================================================================================================
  | If enabled, PageTopic validation is performed, which checks for IsDisabled, PageID and URL.  This can optionally be
  | disabled, which is useful for the Editor.
  \------------------------------------------------------------------------------------------------------------------------*/
    public bool EnableValidation {
      get {
        return _enableValidation;
      }
      set {
        _enableValidation = value;
      }
    }

  /*=========================================================================================================================
  | PROPERTY: TOPIC
  >==========================================================================================================================
  | Identifies the topic associated with the page based on the RouteData.  If the topic cannot be identified then a null
  | reference is returned.
  \------------------------------------------------------------------------------------------------------------------------*/
    public Topic Topic {
      get {

      /*---------------------------------------------------------------------------------------------------------------------
      | GET CACHED INSTANCE
      \--------------------------------------------------------------------------------------------------------------------*/
        if (_topic != null) return _topic;

      /*---------------------------------------------------------------------------------------------------------------------
      | DEFINE PAGE PATH
      \--------------------------------------------------------------------------------------------------------------------*/
        string    pagePath        = null;

      //Pull from RouteData
        if (Page.RouteData.Values["directory"] != null) {
          pagePath = (string)Page.RouteData.Values["directory"];
        }

      //Pull from QueryString
        if (String.IsNullOrEmpty(pagePath) && Request.QueryString["Path"] != null) {
          pagePath = Request.QueryString["Path"].ToString().Replace(":", "/");
        }

      //Default to blank
        pagePath = pagePath?? "";

      //Strip trailing colon
        if (pagePath.EndsWith(":")) {
          pagePath = pagePath.Substring(0, pagePath.Length-1);
        }

      /*---------------------------------------------------------------------------------------------------------------------
      | DEFINE VARIABLES
      \--------------------------------------------------------------------------------------------------------------------*/
        string    topicPath       = pagePath.Replace("/", ":");
        string    nameSpace       = pagePath.IndexOf("/") > 0? pagePath.Substring(0, pagePath.IndexOf("/")) : pagePath;
        Topic     rootTopic       = String.IsNullOrEmpty(nameSpace)? null : TopicRepository.RootTopic.GetTopic(nameSpace);

      /*---------------------------------------------------------------------------------------------------------------------
      | LOOKUP TOPIC
      \--------------------------------------------------------------------------------------------------------------------*/
        _topic = TopicRepository.RootTopic.GetTopic(topicPath);

        if (rootTopic != null) {
          ValidatePageTopic(_topic);
        }
        else {
          _topic = TopicRepository.RootTopic.FirstOrDefault();
        }

      /*---------------------------------------------------------------------------------------------------------------------
      | RETURN TOPIC
      \--------------------------------------------------------------------------------------------------------------------*/
        return _topic;

      }
      set {
        ValidatePageTopic(value);
        _topic = value;
      }
    }

  /*=========================================================================================================================
  | METHOD: VALIDATE PAGE TOPIC
  >==========================================================================================================================
  | Verifies that the page topic is valid and performs any processing necessitated by the page topic, such as handling a
  | redirect.
  \------------------------------------------------------------------------------------------------------------------------*/
    public void ValidatePageTopic(Topic pageTopic) {

    /*-----------------------------------------------------------------------------------------------------------------------
    | HANDLE MISSING OR DISABLED TOPIC
    \----------------------------------------------------------------------------------------------------------------------*/
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

    /*-----------------------------------------------------------------------------------------------------------------------
    | HANDLE REDIRECT
    \----------------------------------------------------------------------------------------------------------------------*/
      if (EnableValidation && pageTopic != null && !String.IsNullOrEmpty(pageTopic.GetAttribute("URL"))) {
        Response.RedirectPermanent(pageTopic.GetAttribute("URL"), true);
      }

    /*-----------------------------------------------------------------------------------------------------------------------
    | SET TITLE
    \----------------------------------------------------------------------------------------------------------------------*/
      Page.Title = ((pageTopic != null)? pageTopic.GetAttribute("MetaTitle", pageTopic.GetAttribute("Title", pageTopic.Key)) : "");

    }

  /*=========================================================================================================================
  | EDIT URL
  >==========================================================================================================================
  | Identifies the appropriate route to edit the current page.
  \------------------------------------------------------------------------------------------------------------------------*/
    public String EditUrl {
      get {
        return "/!Admin/Topics/Default.aspx?Path=" + this.Topic.UniqueKey;
      }
    }

  } //Class

} //Namespace