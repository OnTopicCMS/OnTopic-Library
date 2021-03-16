/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.AspNetCore.Mvc.IntegrationTests.Host;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests {

  /*============================================================================================================================
  | TEST: TOPIC VIEW RESULT EXECUTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicViewResultExecutor"/> is responsible for identifying the view based on a variety of sources,
  ///   including the query string, HTTP request headers, <see cref="Topic.View"/>, etc. This test evaluates those to ensure
  ///   that views are being correctly identified based on these criteria.
  /// </summary>
  [TestClass]
  public class TopicViewResultExecutorTest {

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
    public TopicViewResultExecutorTest() {
      _factory = new WebApplicationFactory<Startup>();
    }

    /*==========================================================================================================================
    | TEST: QUERY STRING: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a query string parameter to ensure that the expected view is returned.
    /// </summary>
    [TestMethod]
    public async Task QueryString_ReturnsExpectedView() {

      var client                = _factory.CreateClient();
      var uri                   = new Uri("/Web/ContentList/?View=Accordion", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: HEADER: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a request header to ensure that the expected view is returned.
    /// </summary>
    [TestMethod]
    public async Task Header_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      client.DefaultRequestHeaders.Add("Accept", "string/Accordion");

      var uri                   = new Uri("/Web/ContentList/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: ACTION: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a specified action to ensure that the expected view is returned.
    /// </summary>
    [TestMethod]
    public async Task Action_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      var uri                   = new Uri("/Area/Area/Accordion", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/Accordion.cshtml", content);

    }

    /*==========================================================================================================================
    | TEST: TOPIC: RETURNS EXPECTED VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a test with a specified <see cref="Topic.View"/> to ensure that the expected view is returned.
    /// </summary>
    [TestMethod]
    public async Task Topic_ReturnsExpectedView() {

      var client                = _factory.CreateClient();

      var uri                   = new Uri("/Area/TopicWithView/", UriKind.Relative);
      var response              = await client.GetAsync(uri).ConfigureAwait(false);
      var content               = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

      response.EnsureSuccessStatusCode();

      Assert.AreEqual<string?>("text/html; charset=utf-8", response.Content.Headers.ContentType?.ToString());
      Assert.AreEqual<string?>("~/Views/ContentList/Accordion.cshtml", content);

    }


  }
}
