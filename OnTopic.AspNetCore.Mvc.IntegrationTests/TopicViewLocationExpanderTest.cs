/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Areas.Area.Controllers;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Host;
using Xunit;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: TOPIC VIEW LOCATION EXPANDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewLocationExpander"/> is responsible for locating whether a view exists based on a set of file
  ///   path conventions. This test evaluates those to ensure that view locations are being correctly evaluated based on those
  ///   conventions.
  /// </summary>
  public class TopicViewLocationExpanderTest: IDisposable {

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
    [Theory]
    [InlineData(                "AreaContentTypeView",          "ContentType/AreaContentTypeView.cshtml")]
    [InlineData(                "AreaContentTypeSharedView",    "ContentType/Shared/AreaContentTypeSharedView.cshtml")]
    [InlineData(                "AreaContentTypesView",         "ContentTypes/ContentType.AreaContentTypesView.cshtml")]
    [InlineData(                "AreaContentTypesSharedView",   "ContentTypes/Shared/AreaContentTypesSharedView.cshtml")]
    [InlineData(                "AreaContentTypesFallbackView", "ContentTypes/AreaContentTypesFallbackView.cshtml")]
    [InlineData(                "AreaSharedView",               "Shared/AreaSharedView.cshtml")]
    [InlineData(                "ContentTypeView",              "ContentType/ContentTypeView.cshtml")]
    [InlineData(                "ContentTypeSharedView",        "ContentType/Shared/ContentTypeSharedView.cshtml")]
    [InlineData(                "ContentTypesView",             "ContentTypes/ContentType.ContentTypesView.cshtml")]
    [InlineData(                "ContentTypesSharedView",       "ContentTypes/Shared/ContentTypesSharedView.cshtml")]
    [InlineData(                "ContentTypesFallbackView",     "ContentTypes/ContentTypesFallbackView.cshtml")]
    [InlineData(                "SharedView",                   "Shared/SharedView.cshtml")]
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

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal(viewLocation, content);

    }

    /*==========================================================================================================================
    | TEST: EXPAND VIEW LOCATIONS: ACTIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates multiple actions off of the <see cref="ControllerController"/> to ensure they fallback to the appropriate
    ///   locations as defined in <see cref="TopicViewLocationExpander.ViewLocations"/> and <see cref="TopicViewLocationExpander
    ///   .AreaViewLocations"/>.
    /// </summary>
    [Fact]
    [InlineData(                "AreaAction",                   "Controller/AreaAction.cshtml")]
    [InlineData(                "AreaFallbackAction",           "AreaFallbackAction.cshtml")]
    [InlineData(                "AreaSharedAction",             "Controller/Shared/AreaSharedAction.cshtml")]
    [InlineData(                "AreaSharedFallbackAction",     "Shared/AreaSharedFallbackAction.cshtml")]
    [InlineData(                "Action",                       "Controller/Action.cshtml")]
    [InlineData(                "FallbackAction",               "FallbackAction.cshtml")]
    [InlineData(                "SharedAction",                 "Controller/Shared/SharedAction.cshtml")]
    [InlineData(                "SharedFallbackAction",         "Shared/SharedFallbackAction.cshtml")]
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

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal(viewLocation, content);

    }

    /*==========================================================================================================================
    | DISPOSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        if (_factory != null) {
          _factory.Dispose();
        }
      }
    }

  }
}
