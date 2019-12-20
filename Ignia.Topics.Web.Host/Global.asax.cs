/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       OnTopicSample OnTopic Site
\=============================================================================================================================*/
using System;
using System.Configuration;
using System.Web.Routing;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Data.Sql;

namespace Ignia.Topics.Web.Host {

  /*============================================================================================================================
  | CLASS: GLOBAL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides default configuration for the application, including any special processing that needs to happen relative to
  ///   application events (such as <see cref="Application_Start"/> or <see cref="System.Web.HttpApplication.Error"/>.
  /// </summary>
  public class Global : System.Web.HttpApplication {

    /*==========================================================================================================================
    | EVENT: APPLICATION START
    >===========================================================================================================================
    | Runs once when the first page of your application is run for the first time by any user
    \-------------------------------------------------------------------------------------------------------------------------*/
    [Obsolete]
    protected void Application_Start(object sender, EventArgs e) {

      /*------------------------------------------------------------------------------------------------------------------------
      | ESTABLISH ERROR TRACKING VARIABLES
      \-----------------------------------------------------------------------------------------------------------------------*/
      Application["Debug"]      = false;
      Application["ErrorTime"]  = DateTime.Now;
      Application["ErrorCount"] = 0;

      /*------------------------------------------------------------------------------------------------------------------------
      | CONFIGURE REPOSITORY
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connectionString      = ConfigurationManager.ConnectionStrings["OnTopic"].ConnectionString;
      var sqlTopicRepository    = new SqlTopicRepository(connectionString);
      var topicRepository       = new CachedTopicRepository(sqlTopicRepository);

      TopicRepository.DataProvider = topicRepository;

      /*------------------------------------------------------------------------------------------------------------------------
      | REGISTER ROUTES
      \-----------------------------------------------------------------------------------------------------------------------*/
      RegisterRoutes(RouteTable.Routes);

    }

    /*==========================================================================================================================
    | METHOD: REGISTER ROUTES
    >===========================================================================================================================
    | Establishes URL routes and maps them to the appropriate page handlers.
    \-------------------------------------------------------------------------------------------------------------------------*/
    void RegisterRoutes(RouteCollection routes) {
      routes.Add(
        "TopicUrl",
        new Route(
          "{rootTopic}/{*path}",
          new TopicsRouteHandler()
        )
      );
    }

  } //Class
} //Namespace