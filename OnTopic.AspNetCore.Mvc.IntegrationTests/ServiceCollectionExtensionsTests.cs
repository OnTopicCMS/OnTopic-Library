/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Net;
using Microsoft.AspNetCore.Routing;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: SERVICE COLLECTION EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ServiceCollectionExtensions"/> are responsible, primarily, for establishing routes based on the <see cref
  ///   ="IEndpointRouteBuilder"/> interface. These integration tests validate that those routes are operating as expected.
  /// </summary>
  [Collection("Web Application")]
  public class ServiceCollectionExtensionsTests: IClassFixture<WebApplicationFactory<Startup>> {

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
    public ServiceCollectionExtensionsTests(WebApplicationFactory<Startup> factory) {
      _factory = factory;
    }

    /*==========================================================================================================================
    | TEST: REQUEST PAGE: EXPECTED RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates various routes enabled by the routing extension methods to ensure they correctly map to the expected
    ///   controllers, actions, and views.
    /// </summary>
    [Theory]
    [InlineData("/Web/ContentList/", "~/Views/ContentList/ContentList.cshtml")]            // MapTopicRoute()
    [InlineData("/Area/Area/", "~/Areas/Area/Views/ContentType/ContentType.cshtml")]       // MapTopicAreaRoute()
    [InlineData("/Area/Controller/AreaAction/", "~/Areas/Area/Views/Controller/AreaAction.cshtml")]       // MapTopicAreaRoute()
    [InlineData("/Area/Accordion/", "~/Views/ContentList/Accordion.cshtml")]               // MapImplicitAreaControllerRoute()
    [InlineData("/Topic/3/", "~/Views/ContentList/ContentList.cshtml")]                    // MapTopicRedirect()
    [InlineData("/Error/404", "400")]                                                      // MapTopicErrors()
    [InlineData("/Error/Http/404", "400")]                                                 // MapDefaultControllerRoute()
    [InlineData("/Error/Unauthorized/", "Unauthorized")]                                   // MapTopicRoute()
    public async Task RequestPage_ExpectedResults(string path, string expectedContent) {

      var client                = _factory.CreateClient();
      var uri                   = new Uri(path, UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var actualContent         = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal(expectedContent, actualContent);

    }

    /*==========================================================================================================================
    | TEST: MAP TOPIC SITEMAP: RESPONDS TO REQUEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route associated with <see cref="ServiceCollectionExtensions.MapTopicSitemap(IEndpointRouteBuilder)"/> and
    ///   confirms that it responds appropriately.
    /// </summary>
    [Fact]
    public async Task MapTopicSitemap_RespondsToRequest() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri($"/Sitemap/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/xml", response.Content.Headers.ContentType?.ToString());
      Assert.True(content.Contains("/Web/ContentList/</loc>", StringComparison.OrdinalIgnoreCase));

    }

    /*==========================================================================================================================
    | TEST: USE STATUS CODE PAGES: RETURNS EXPECTED STATUS CODE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route with an error, and confirms that it returns a page with the expected status code.
    /// </summary>
    [Theory]
    [InlineData("/MissingPage/", HttpStatusCode.NotFound, "400")]
    [InlineData("/Web/Container/", HttpStatusCode.Forbidden, "400")]
    public async Task UseStatusCodePages_ReturnsExpectedStatusCode(string path, HttpStatusCode statusCode, string expectedContent) {

      var client                = _factory.CreateClient();
      var uri                   = new Uri(path, UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var actualContent         = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      Assert.Equal(statusCode, response.StatusCode);
      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal(expectedContent, actualContent);

    }

    /*==========================================================================================================================
    | TEST: USE RESPONSE CACHING: RETURNS CACHED PAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates a route with response caching, and confirms that the page remains unchanged after subsequent calls.
    /// </summary>
    /// <remarks>
    ///   The <c>Counter.cshtml</c> page will increment a number output for every request to a given path. The <c>CachedPage</c>
    ///   request will not increment because the cached result is being returned; the <c>UncachedPage</c> will increment because
    ///   the results are not cached.
    /// </remarks>
    [Theory]
    [InlineData("/Web/CachedPage/", "1", "1", true)]
    [InlineData("/Web/UncachedPage/", "1", "2", false)]
    public async Task UseResponseCaching_ReturnsCachedPage(
      string path,
      string firstResult,
      string secondResult,
      bool validateHeaders
    ) {

      var client                = _factory.CreateClient();
      var uri                   = new Uri(path, UriKind.Relative);

      var response1             = await client.GetAsync(uri).ConfigureAwait(false);
      var content1              = await response1.Content.ReadAsStringAsync().ConfigureAwait(false);

      var response2             = await client.GetAsync(uri).ConfigureAwait(false);
      var content2              = await response2.Content.ReadAsStringAsync().ConfigureAwait(false);

      response1.EnsureSuccessStatusCode();

      Assert.StartsWith(firstResult, content1, StringComparison.Ordinal);
      Assert.StartsWith(secondResult, content2, StringComparison.Ordinal);
      Assert.Equal(validateHeaders? true : null, response1.Headers.CacheControl?.Public);
      Assert.Equal(validateHeaders? TimeSpan.FromSeconds(10) : null, response1?.Headers.CacheControl?.MaxAge);

    }

  } //Class
} //Namespace