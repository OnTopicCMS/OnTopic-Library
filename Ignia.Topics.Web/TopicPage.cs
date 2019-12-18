/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Internal.Diagnostics;
using System.Linq;
using System.Web.Security;
using System.Web.UI;
using System.Diagnostics.CodeAnalysis;

namespace Ignia.Topics.Web {

  /*============================================================================================================================
  | CLASS: TOPIC PAGE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a base class for Topic-related pages.
  /// </summary>
  [SuppressMessage("Security", "CA5368", Justification = "Deprecated code; non-breaking issue.")]
  public class TopicPage : Page {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     Topic                           _topic                          = null;

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
    public bool EnableValidation { get; set; }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic associated with the page based on the <see cref="Page.RouteData"/>.
    /// </summary>
    /// <remarks>
    ///   If the topic cannot be identified then a null reference is returned.
    /// </remarks>
    public Topic Topic {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Lookup topic
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_topic == null) {
          var path = Request.QueryString["Path"];
          if (path != null) {
            _topic = TopicRepository.DataProvider.Load(path);
          }
          else {
            var topicRoutingService = new WebFormsTopicRoutingService(TopicRepository.DataProvider, Request.RequestContext);
            _topic = topicRoutingService.GetCurrentTopic();
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Provide Fallback
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_topic == null) {
          _topic = TopicRepository.RootTopic.Children.FirstOrDefault();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate topic
        \---------------------------------------------------------------------------------------------------------------------*/
        ValidatePageTopic(_topic);

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
      if (EnableValidation && pageTopic != null && pageTopic.Attributes.GetValue("IsDisabled", true) == "1") {
        if (!Roles.IsUserInRole(Page.User.Identity.Name?? "", "Administrators")) {
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
      if (EnableValidation && pageTopic != null && !String.IsNullOrEmpty(pageTopic.Attributes.GetValue("URL"))) {
        Response.RedirectPermanent(pageTopic.Attributes.GetValue("URL"), true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set title
      \-----------------------------------------------------------------------------------------------------------------------*/
      Page.Title = ((pageTopic != null) ? pageTopic.Attributes.GetValue("MetaTitle", pageTopic.Attributes.GetValue("Title", pageTopic.Key)) : "");

    }

    /*==========================================================================================================================
    | EDIT URL
    >===========================================================================================================================
    | ###TODO KLT081015: Wire up to the <editor /> element location attribute in the configuration.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Identifies the appropriate route to edit the current page.
    /// </summary>
    /// <remarks>The URL in the editor associated with the current topic.</remarks>
    public string EditUrl {
      get {
        Contract.Requires(Topic, "Assumes the page topic is not null.");
        return "/!Admin/Topics/Default.aspx?Path=" + Topic.GetUniqueKey();
      }
    }

  } //Class
} //Namespace