/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Ignia.Topics.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Routing;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY EXTENSIONS TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRepositoryExtensions"/> class.
  /// </summary>
  [TestClass]
  public class TopicRepositoryExtensionsTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository            _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicRepositoryExtensionsTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicRepositoryExtensionsTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: LOAD (ROUTE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that a topic is correctly identified based on that route.
    /// </summary>
    [TestMethod]
    public void LoadRoute() {

      var routes                = new RouteData();
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_1");

      var currentTopic          = _topicRepository.Load(routes);

      Assert.IsNotNull(currentTopic);
      Assert.ReferenceEquals(topic, currentTopic);
      Assert.AreEqual<string>("Web_0_1_1", currentTopic.Key);

    }

  } //Class
} //Namespace