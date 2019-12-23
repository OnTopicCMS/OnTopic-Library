/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       OnTopicSample OnTopic Site
\=============================================================================================================================*/
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace OnTopic.Web.Mvc.Host {

  /*============================================================================================================================
  | CLASS: GLOBAL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides default configuration for the application, including any special processing that needs to happen relative to
  ///   application events (such as <see cref="Application_Start"/> or <see cref="System.Web.HttpApplication.Error"/>.
  /// </summary>
  public class Global: HttpApplication {

    /*==========================================================================================================================
    | METHOD: APPLICATION START (EVENT HANDLER)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides initial configuration for the application, including registration of MVC routes via the
    ///   <see cref="RouteConfig"/> class.
    /// </summary>
    void Application_Start(object sender, EventArgs e) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Register controller factory
      \-----------------------------------------------------------------------------------------------------------------------*/
      ControllerBuilder.Current.SetControllerFactory(
        new SampleControllerFactory()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Register view engine
      \-----------------------------------------------------------------------------------------------------------------------*/
      ViewEngines.Engines.Insert(0, new TopicViewEngine());

      /*------------------------------------------------------------------------------------------------------------------------
      | Register routes
      \-----------------------------------------------------------------------------------------------------------------------*/
      RouteConfig.RegisterRoutes(RouteTable.Routes);

      /*------------------------------------------------------------------------------------------------------------------------
      | Require HTTPS
      \-----------------------------------------------------------------------------------------------------------------------*/
      GlobalFilters.Filters.Add(new RequireHttpsAttribute());

    }

  } //Class
} //Namespace