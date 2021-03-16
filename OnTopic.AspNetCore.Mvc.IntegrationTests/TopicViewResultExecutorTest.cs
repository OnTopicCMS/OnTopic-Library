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


  }
}
