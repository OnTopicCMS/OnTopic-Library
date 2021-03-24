/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Host;
using Xunit;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: TOPIC VIEW RESULT EXECUTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewResultExecutor"/> is responsible for identifying the view based on a variety of sources,
  ///   including the query string, HTTP request headers, <see cref="Topic.View"/>, etc. This test evaluates those to ensure
  ///   that views are being correctly identified based on these criteria.
  /// </summary>
  [Collection("Web Application")]
  public class TopicViewResultExecutorTest: IClassFixture<WebApplicationFactory<Startup>> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            WebApplicationFactory<Startup>  _factory;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicViewResultExecutorTest"/>.
    /// </summary>
    public TopicViewResultExecutorTest(WebApplicationFactory<Startup> factory) {
      _factory = factory;
    }

    /*==========================================================================================================================
    | TEST: QUERY STRING: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a query string parameter to ensure that the expected view is returned.
    /// </summary>
    [Fact]
    public async Task QueryString_ReturnsExpectedView() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri("/Web/ContentList/?View=Accordion", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: HEADER: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a request header to ensure that the expected view is returned.
    /// </summary>
    [Fact]
    public async Task Header_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      client.DefaultRequestHeaders.Add("Accept", "application/json, string/Accordion;level=2;q=0.4, text/html");

      var uri                   = new Uri("/Web/ContentList/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: ACTION: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a specified action to ensure that the expected view is returned.
    /// </summary>
    [Fact]
    public async Task Action_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      var uri                   = new Uri("/Area/Area/Accordion", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: TOPIC: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a specified <see cref="Topic.View"/> to ensure that the expected view is returned.
    /// </summary>
    [Fact]
    public async Task Topic_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      var uri                   = new Uri("/Area/TopicWithView/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: CONTENT TYPE: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with without a view specified to ensure that the expected default view is returned based on the <see
    ///   cref="Topic.ContentType"/>.
    /// </summary>
    [Fact]
    public async Task ContentType_ReturnsExpectedView() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri("/Web/ContentList/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.Equal("~/Views/ContentList/ContentList.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: MISSING VIEW: RETURNS INTERNAL SERVER ERROR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test without a view and without a default implementation for the <see cref="Topic.ContentType"/> and
    ///   ensures that an <see cref="HttpStatusCode.InternalServerError"/> is returned.
    /// </summary>
    [Fact]
    public async Task MissingView_ReturnsInternalServerError() {

      var client = _factory.CreateClient();
      var uri = new Uri("/Web/MissingView/", UriKind.Relative);
      var response = await client.GetAsync(uri).ConfigureAwait(false);

      Assert.Equal<HttpStatusCode?>(HttpStatusCode.InternalServerError, response.StatusCode);

    }

  }
}