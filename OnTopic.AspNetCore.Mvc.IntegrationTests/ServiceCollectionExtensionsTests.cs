/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
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
  class ServiceCollectionExtensionsTests {

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

  }
}
