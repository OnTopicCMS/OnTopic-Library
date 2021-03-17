/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Areas.Area.Controllers;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Host;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: TOPIC VIEW LOCATION EXPANDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewLocationExpander"/> is responsible for locating whether a view exists based on a set of file
  ///   path conventions. This test evaluates those to ensure that view locations are being correctly evaluated based on those
  ///   conventions.
  /// </summary>
  [TestClass]
  public class TopicViewLocationExpanderTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            WebApplicationFactory<Startup>  _factory;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewLocationExpanderTest"/>.
    /// </summary>
    public TopicViewLocationExpanderTest() {
      _factory = new WebApplicationFactory<Startup>();
    }

    /*==========================================================================================================================
    | TEST: EXPAND VIEW LOCATIONS: VIEWS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates multiple views to ensure they fallback to the appropriate locations as defined in <see cref="
    ///   TopicViewLocationExpander.ViewLocations"/> and <see cref="TopicViewLocationExpander.AreaViewLocations"/>.
    /// </summary>
    [TestMethod]
    [DataRow(                   "AreaContentTypeView",          "ContentType/AreaContentTypeView.cshtml")]
    [DataRow(                   "AreaContentTypeSharedView",    "ContentType/Shared/AreaContentTypeSharedView.cshtml")]
    [DataRow(                   "AreaContentTypesView",         "ContentTypes/ContentType.AreaContentTypesView.cshtml")]
    [DataRow(                   "AreaContentTypesSharedView",   "ContentTypes/Shared/AreaContentTypesSharedView.cshtml")]
    [DataRow(                   "AreaContentTypesFallbackView", "ContentTypes/AreaContentTypesFallbackView.cshtml")]
    [DataRow(                   "AreaSharedView",               "Shared/AreaSharedView.cshtml")]
    [DataRow(                   "ContentTypeView",              "ContentType/ContentTypeView.cshtml")]
    [DataRow(                   "ContentTypeSharedView",        "ContentType/Shared/ContentTypeSharedView.cshtml")]
    [DataRow(                   "ContentTypesView",             "ContentTypes/ContentType.ContentTypesView.cshtml")]
    [DataRow(                   "ContentTypesSharedView",       "ContentTypes/Shared/ContentTypesSharedView.cshtml")]
    [DataRow(                   "ContentTypesFallbackView",     "ContentTypes/ContentTypesFallbackView.cshtml")]
    [DataRow(                   "SharedView",                   "Shared/SharedView.cshtml")]
    public async Task ExpandViewLocations_Views(string viewName, string viewLocation) {

      if (viewName is not null && viewName.StartsWith("Area", StringComparison.OrdinalIgnoreCase)) {
        viewLocation = $"~/Areas/Area/Views/{viewLocation}";
      }
      else {
        viewLocation = $"~/Views/{viewLocation}";
      }

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Area/?View={viewName}", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>(viewLocation, content);

    }

    /*==========================================================================================================================
    | TEST: EXPAND VIEW LOCATIONS: ACTIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates multiple actions off of the <see cref="ControllerController"/> to ensure they fallback to the appropriate
    ///   locations as defined in <see cref="TopicViewLocationExpander.ViewLocations"/> and <see cref="TopicViewLocationExpander
    ///   .AreaViewLocations"/>.
    /// </summary>
    [TestMethod]
    [DataRow(                   "AreaAction",                   "Controller/AreaAction.cshtml")]
    [DataRow(                   "AreaFallbackAction",           "AreaFallbackAction.cshtml")]
    [DataRow(                   "AreaSharedAction",             "Controller/Shared/AreaSharedAction.cshtml")]
    [DataRow(                   "AreaSharedFallbackAction",     "Shared/AreaSharedFallbackAction.cshtml")]
    [DataRow(                   "Action",                       "Controller/Action.cshtml")]
    [DataRow(                   "FallbackAction",               "FallbackAction.cshtml")]
    [DataRow(                   "SharedAction",                 "Controller/Shared/SharedAction.cshtml")]
    [DataRow(                   "SharedFallbackAction",         "Shared/SharedFallbackAction.cshtml")]
    public async Task ExpandViewLocations_Actions(string viewName, string viewLocation) {

      if (viewName is not null && viewName.StartsWith("Area", StringComparison.OrdinalIgnoreCase)) {
        viewLocation = $"~/Areas/Area/Views/{viewLocation}";
      }
      else {
        viewLocation = $"~/Views/{viewLocation}";
      }

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Area/Controller/{viewName}/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>(viewLocation, content);

    }

  }
}
