/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Host;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: SERVICE COLLECTION EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ServiceCollectionExtensions"/> are responsible, primarily, for establishing routes based on the <see cref
  ///   ="IEndpointRouteBuilder"/> interface. These integration tests validate that those routes are operating as expected.
  /// </summary>
  [TestClass]
  public class ServiceCollectionExtensionsTests {

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
    public ServiceCollectionExtensionsTests() {
      _factory = new WebApplicationFactory<Startup>();
    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC ROUTE: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapTopicRoute(IEndpointRouteBuilder, String,
    ///   String, String)"/> and confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapTopicRoute_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Web/ContentList/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/ContentList.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC AREA ROUTE: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapTopicAreaRoute(IEndpointRouteBuilder)"/>
    ///   and confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapTopicAreaRoute_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Area/Area/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Areas/Area/Views/ContentType/ContentType.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: MAP DEFAULT AREA CONTROLLER ROUTE: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapDefaultAreaControllerRoute(
    ///   IEndpointRouteBuilder)"/> and confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapDefaultAreaControllerRoute_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Area/Controller/AreaAction/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Areas/Area/Views/Controller/AreaAction.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: MAP IMPLCIT AREA CONTROLLER ROUTE: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapDefaultAreaControllerRoute(
    ///   IEndpointRouteBuilder)"/> and confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapImplicitAreaControllerRoute_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Area/Accordion/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC SITEMAP: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapTopicSitemap(IEndpointRouteBuilder)"/> and
    ///   confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapTopicSitemap_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Sitemap/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/xml", response.Content.Headers.ContentType?.ToString());
      Assert.IsTrue(content.Contains("/Web/ContentList/</loc>", StringComparison.OrdinalIgnoreCase));

    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC REDIRECT: REDIRECTS REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapTopicRedirect(IEndpointRouteBuilder)"/>
    ///   and confirms that it responds appropriately.
    /// </summary>
    [TestMethod]
    public async Task MapTopicRedirect_RedirectsRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Topic/3/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/ContentList.cshtml", content);

    }

  }
}