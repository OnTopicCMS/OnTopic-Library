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

  }
}
