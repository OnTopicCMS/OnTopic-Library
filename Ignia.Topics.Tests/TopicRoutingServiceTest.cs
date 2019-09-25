/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Web.Routing;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC ROUTING SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="MvcTopicRoutingService"/> class.
  /// </summary>
  [TestClass]
  public class TopicRoutingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository            _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRoutingServiceTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicRoutingServiceTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: TOPIC (ROUTE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that a topic is correctly identified based on that route.
    /// </summary>
    [TestMethod]
    public void TopicRoute() {

      var routes                = new RouteData();
      var uri                   = new Uri("http://localhost/Topics/Web/Web_0/Web_0_1/Web_0_1_1");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_1");

      var topicRoutingService   = new MvcTopicRoutingService(_topicRepository, uri, routes);
      var currentTopic          = topicRoutingService.GetCurrentTopic();

      Assert.IsNotNull(currentTopic);
      Assert.ReferenceEquals(topic, currentTopic);
      Assert.AreEqual<string>("Web_0_1_1", currentTopic.Key);

    }

    /*==========================================================================================================================
    | TEST: TOPIC (URI)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a URI based on a path and ensures that a topic is correctly identified based on that URI.
    /// </summary>
    [TestMethod]
    public void TopicUri() {

      var routes                = new RouteData();
      var uri                   = new Uri("http://localhost/Web/Web_0/Web_0_1/Web_0_1_1");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      var topicRoutingService   = new MvcTopicRoutingService(_topicRepository, uri, routes);
      var currentTopic          = topicRoutingService.GetCurrentTopic();

      Assert.IsNotNull(currentTopic);
      Assert.ReferenceEquals(topic, currentTopic);
      Assert.AreEqual<string>("Web_0_1_1", currentTopic.Key);

    }

    /*==========================================================================================================================
    | TEST: ROUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that those routes are available after initializing a new instance of the
    ///   <see cref="MvcTopicRoutingService"/>.
    /// </summary>
    [TestMethod]
    public void Routes() {

      var routes                = new RouteData();
      var uri                   = new Uri("http://localhost/Web/Web_0/Web_0_1/Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_1");

      var topicRoutingService   = new MvcTopicRoutingService(_topicRepository, uri, routes);
      var currentTopic          = topicRoutingService.GetCurrentTopic();

      Assert.IsNotNull(currentTopic);
      Assert.AreEqual<string>("Web", routes.GetRequiredString("rootTopic"));
      Assert.AreEqual<string>("Web_0/Web_0_1/Web_0_1_1", routes.GetRequiredString("path"));
      Assert.AreEqual<string>("Page", routes.GetRequiredString("contenttype"));

    }


  } //Class

} //Namespace
