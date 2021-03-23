/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing;
using OnTopic.AspNetCore.Mvc;
using OnTopic.Data.Caching;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY EXTENSIONS TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRepositoryExtensions"/> class.
  /// </summary>
  [ExcludeFromCodeCoverage]
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
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicRepositoryExtensionsTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: LOAD: BY ROUTE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that a topic is correctly identified based on that route.
    /// </summary>
    [Fact]
    public void Load_ByRoute_ReturnsTopic() {

      var routes                = new RouteData();
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_1:Web_0_1_1");

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web_0/Web_0_1/Web_0_1_1");

      var currentTopic          = _topicRepository.Load(routes);

      Assert.IsNotNull(currentTopic);
      Assert.AreEqual<Topic?>(topic, currentTopic);
      Assert.AreEqual<string?>("Web_0_1_1", currentTopic?.Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: BY ROUTE: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data and ensures that the root topic is correctly identified based on that route.
    /// </summary>
    [Fact]
    public void Load_ByRoute_ReturnsRootTopic() {

      var routes                = new RouteData();
      var topic                 = _topicRepository.Load("Root");

      routes.Values.Add("path", "Root/");

      var currentTopic          = _topicRepository.Load(routes);

      Assert.IsNotNull(currentTopic);
      Assert.AreEqual<Topic?>(topic, currentTopic);
      Assert.AreEqual<string?>("Root", currentTopic?.Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: BY ROUTE: SWALLOWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes route data with an invalid key and ensures that no result is returned, but no exception is thrown.
    /// </summary>
    [Fact]
    public void Load_ByRoute_SwallowsException() {

      var routes                = new RouteData();

      routes.Values.Add("rootTopic", "Web");
      routes.Values.Add("path", "Web/~* &$#@");

      var currentTopic          = _topicRepository.Load(routes);

      Assert.IsNull(currentTopic);

    }

  } //Class
} //Namespace