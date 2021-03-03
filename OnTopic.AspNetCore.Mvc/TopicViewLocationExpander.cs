/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.AspNetCore.Mvc {

  /*============================================================================================================================
  | CLASS: TOPIC VIEW LOCATION EXPANDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides hints to the configured view engine (presumably, <see cref="RazorViewEngine"/>) on where to find views
  ///   associated with the current request.
  /// </summary>
  /// <remarks>
  ///   In addition to Area (<c>{2}</c>), Controller (<c>{1}</c>), and View (<c>{0}</c>), the <see
  ///   cref="TopicViewLocationExpander"/> also factors in ContentType (<c>{3}</c>). Note that, by default, the action is used
  ///   as the view name. The topic library overrides this behavior by evaluating other sources of views. If a match with an
  ///   controller and an action is found, however, that should be prioritized.
  /// </remarks>
  public class TopicViewLocationExpander : IViewLocationExpander {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewLocationExpander"/> class.
    /// </summary>
    public TopicViewLocationExpander() {
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW LOCATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a static copy of all view locations associated with OnTopic.
    /// </summary>
    public static IEnumerable<string> ViewLocations => new[] {
      "/Views/{1}/{0}.cshtml",                                  //Views/Controller/Action.cshtml
      "/Views/{3}/{0}.cshtml",                                  //Views/ContentType/View.cshtml
      "/Views/{3}/Shared/{0}.cshtml",                           //Views/ContentType/Shared/View.cshtml
      "/Views/ContentTypes/{3}.{0}.cshtml",                     //Views/ContentTypes/ContentType.View.cshtml
      "/Views/{1}/Shared/{0}.cshtml",                           //Views/Controller/Shared/Action.cshtml
      "/Views/ContentTypes/Shared/{0}.cshtml",                  //Views/ContentTypes/Shared/View.cshtml
      "/Views/ContentTypes/{0}.cshtml",                         //Views/ContentTypes/View.cshtml
      "/Views/Shared/{0}.cshtml",                               //Views/Shared/View.cshtml
    };

    /*==========================================================================================================================
    | PROPERTY: AREA VIEW LOCATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a static copy of all areas view locations associated with OnTopic.
    /// </summary>
    public static IEnumerable<string> AreaViewLocations => new[] {
      "/Areas/{2}/Views/{1}/{0}.cshtml",                        //Areas/Area/Views/Controller/Action.cshtml
      "/Areas/{2}/Views/{3}/{0}.cshtml",                        //Areas/Area/Views/ContentType/View.cshtml
      "/Areas/{2}/Views/{3}/Shared/{0}.cshtml",                 //Areas/Area/Views/ContentType/Shared/View.cshtml
      "/Areas/{2}/Views/ContentTypes/{3}.{0}.cshtml",           //Areas/Area/Views/ContentTypes/ContentType.View.cshtml
      "/Areas/{2}/Views/{1}/Shared/{0}.cshtml",                 //Areas/Area/Views/Controller/Shared/Action.cshtml
      "/Areas/{2}/Views/ContentTypes/Shared/{0}.cshtml",        //Areas/Area/Views/ContentTypes/Shared/View.cshtml
      "/Areas/{2}/Views/ContentTypes/{0}.cshtml",               //Areas/Area/Views/ContentTypes/View.cshtml
      "/Areas/{2}/Views/Shared/{0}.cshtml",                     //Areas/Area/Views/Shared/View.cshtml
    };

    /*==========================================================================================================================
    | METHOD: POPULATE VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewLocationExpander"/> class.
    /// </summary>
    /// <seealso href="https://stackoverflow.com/questions/36802661/what-is-iviewlocationexpander-populatevalues-for-in-asp-net-core-mvc"/>
    /// <param name="context">The <see cref="ViewLocationExpanderContext"/> that the request is operating within.</param>
    public void PopulateValues(ViewLocationExpanderContext context) {
      Contract.Requires(context, nameof(context));
      context.Values["action_displayname"] = context.ActionContext.ActionDescriptor.DisplayName;
      context.ActionContext.RouteData.Values.TryGetValue("contenttype", out var contentType);
      context.Values["content_type"] = (string?)contentType;
    }

    /*==========================================================================================================================
    | METHOD: EXPAND VIEW LOCATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(context, nameof(context));
      Contract.Requires(viewLocations, nameof(viewLocations));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      context.ActionContext.RouteData.Values.TryGetValue("contenttype", out var contentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Yield view locations
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var location in ViewLocations) {
        yield return location.Replace(@"{3}", (string?)contentType, StringComparison.Ordinal);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Yield area view locations
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var location in AreaViewLocations) {
        yield return location.Replace(@"{3}", (string?)contentType, StringComparison.Ordinal);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return previous
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var location in viewLocations) {
        yield return location;
      }

    }

  } //Class
} //Namespace