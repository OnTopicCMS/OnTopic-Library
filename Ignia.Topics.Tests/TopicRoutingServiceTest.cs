/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Reflection;
using System.Web.Routing;
using Ignia.Topics.Collections;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC ROUTING SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRoutingService"/> class.
  /// </summary>
  [TestClass]
  public class TopicRoutingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    ITopicRepository            _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingServiceTest"/> with shared resources.
    /// </summary>
    public TopicRoutingServiceTest() {
      _topicRepository = new FakeTopicRepository();
    }

    /*==========================================================================================================================
    | TEST: TOPIC (ROUTE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that a topic is correctly identified based on that route.
    /// </summary>
    [TestMethod]
    public void TopicRoutingService_TopicRouteTest() {

      var routes                = new RouteData();
      var rootTopic             = _topicRepository.Load();
      var uri                   = new Uri("http://localhost/Topics/Web/Web_0/Web_0_1/Web_0_1_1");
      var topic                 = rootTopic.GetTopic("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_2");

      var topicRoutingService   = new TopicRoutingService(_topicRepository, uri, routes);

      Assert.IsNotNull(topicRoutingService.Topic);
      Assert.ReferenceEquals(topic, topicRoutingService.Topic);
      Assert.AreEqual<string>("Web_0_1_2", topicRoutingService.Topic.Key);

    }

    /*==========================================================================================================================
    | TEST: TOPIC (URI)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a URI based on a path and ensures that a topic is correctly identified based on that URI.
    /// </summary>
    [TestMethod]
    public void TopicRoutingService_TopicUriTest() {

      var routes                = new RouteData();
      var rootTopic             = _topicRepository.Load();
      var uri                   = new Uri("http://localhost/Web/Web_0/Web_0_1/Web_0_1_1");
      var topic                 = rootTopic.GetTopic("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      var topicRoutingService   = new TopicRoutingService(_topicRepository, uri, routes);

      Assert.IsNotNull(topicRoutingService.Topic);
      Assert.ReferenceEquals(topic, topicRoutingService.Topic);
      Assert.AreEqual<string>("Web_0_1_1", topicRoutingService.Topic.Key);

    }

    /*==========================================================================================================================
    | TEST: ROUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that those routes are available after initializing a new instance of the
    ///   <see cref="TopicRoutingService"/>.
    /// </summary>
    [TestMethod]
    public void TopicRoutingService_RoutesTest() {

      var routes                = new RouteData();
      var rootTopic             = _topicRepository.Load();
      var uri                   = new Uri("http://localhost/Web/Web_0/Web_0_1/Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_2");

      var topicRoutingService   = new TopicRoutingService(_topicRepository, uri, routes);

      Assert.IsNotNull(topicRoutingService.Topic);
      Assert.AreEqual<string>("Web", routes.GetRequiredString("rootTopic"));
      Assert.AreEqual<string>("Web_0/Web_0_1/Web_0_1_2", routes.GetRequiredString("path"));
      Assert.AreEqual<string>("Page", routes.GetRequiredString("contenttype"));

    }


  } //Class

} //Namespace
