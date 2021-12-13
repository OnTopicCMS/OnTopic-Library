/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace OnTopic.AspNetCore.Mvc.Tests.TestDoubles {

  /*============================================================================================================================
  | CLASS: FAKE CONTROLLER CONTEXT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a factory method for generating a fake <see cref="ControllerContext"/> for the purpose of testing controllers.
  /// </summary>
  [ExcludeFromCodeCoverage]
  internal static class FakeControllerContext {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new instances of a <see cref="FakeControllerContext"/>.
    /// </summary>
    public static ControllerContext GetControllerContext(string rootTopic, string? path = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish view model context
      \-----------------------------------------------------------------------------------------------------------------------*/
      var routes                = new RouteData();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set routes based on parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      routes.Values.Add("rootTopic", rootTopic);

      if (path is not null) {
        routes.Values.Add("path", path);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create action context
      \-----------------------------------------------------------------------------------------------------------------------*/
      var actionContext         = new ActionContext {
        HttpContext             = new DefaultHttpContext(),
        RouteData               = routes,
        ActionDescriptor        = new ControllerActionDescriptor()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return new ControllerContext instance
      \-----------------------------------------------------------------------------------------------------------------------*/
      return new(actionContext);

    }

  } //Class
} //Namespace
