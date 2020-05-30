/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using OnTopic.Collections;
using OnTopic.Data.Caching;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  /// <remarks>
  ///   These tests not only validate that the <see cref="StubTopicRepository"/> is functioning as expected, but also that the
  ///   underlying <see cref="TopicRepositoryBase"/> functions are also operating correctly.
  /// </remarks>
  [TestClass]
  public class ITopicRepositoryTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ITopicRepositoryTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ITopicRepositoryTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: LOAD: DEFAULT: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the default topic and ensures there are the expected number of children.
    /// </summary>
    [TestMethod]
    public void Load_Default_ReturnsTopicTopic() {

      var rootTopic             = _topicRepository.Load();

      Assert.AreEqual<int>(2, rootTopic.Children.Count);
      Assert.AreEqual<string>("Configuration", rootTopic.Children.First().Key);
      Assert.AreEqual<string>("Web", rootTopic.Children.Last().Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: VALID UNIQUE KEY: RETURNS CORRECT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topics and ensures there are the expected number of children.
    /// </summary>
    [TestMethod]
    public void Load_ValidUniqueKey_ReturnsCorrectTopic() {

      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Page");
      var child                 = TopicFactory.Create("Child", "ContentType", Int32.MaxValue, topic);

      Assert.AreEqual<string>("Page", topic.Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: INVALID UNIQUE KEY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads invalid topic key and ensures a null is returned.
    /// </summary>
    [TestMethod]
    public void Load_InvalidUniqueKey_ReturnsTopic() {

      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:InvalidContentType");

      Assert.IsNull(topic);

    }

    /*==========================================================================================================================
    | TEST: LOAD: VALID TOPIC ID: RETURNS CORRECT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topic by ID and ensures it is found.
    /// </summary>
    [TestMethod]
    public void Load_ValidTopicId_ReturnsCorrectTopic() {

      var topic                 = _topicRepository.Load(11111);

      Assert.IsNotNull(topic);
      Assert.AreEqual<string>("Web_1_1_1_1", topic.Key);

    }

    /*==========================================================================================================================
    | TEST: LOAD: INVALID TOPIC ID: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topic by an incorrect ID and ensures it a null is returned.
    /// </summary>
    [TestMethod]
    public void Load_InvalidTopicId_ReturnsNull() {

      var topic                 = _topicRepository.Load(9999999);

      Assert.IsNull(topic);

    }

    /*==========================================================================================================================
    | TEST: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves topics and ensures their identifiers are properly set.
    /// </summary>
    [TestMethod]
    public void Save() {

      var web                   = _topicRepository.Load("Root:Web");
      var configuration         = _topicRepository.Load("Root:Configuration");

      Assert.AreEqual<int>(10000, web.Id);
      Assert.AreEqual<int>(-1, configuration.Id);

      _topicRepository.Save(configuration);

      Assert.AreNotEqual<int>(-1, configuration.Id);
      Assert.AreEqual<int>(-1, configuration.Children.First().Id);

      _topicRepository.Save(configuration, true);

      Assert.AreNotEqual<int>(-1, configuration.Children.First().Id);

    }

    /*==========================================================================================================================
    | TEST: MOVE: TO NEW PARENT: CONFIRMED MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topics and ensures their parents are correctly set.
    /// </summary>
    [TestMethod]
    public void Move_ToNewParent_ConfirmedMove() {

      var source                = _topicRepository.Load("Root:Web:Web_0");
      var destination           = _topicRepository.Load("Root:Web:Web_1");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_1");

      Assert.ReferenceEquals(topic.Parent, source);
      Assert.AreEqual<int>(2, destination.Children.Count);
      Assert.AreEqual<int>(2, source.Children.Count);

      _topicRepository.Move(topic, destination);

      Assert.ReferenceEquals(topic.Parent, destination);
      Assert.AreEqual<int>(1, source.Children.Count);
      Assert.AreEqual<int>(3, destination.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MOVE: TO NEW SIBLING: CONFIRMED MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topic next to a different sibling and ensures it ends up in the correct location.
    /// </summary>
    [TestMethod]
    public void Move_ToNewSibling_ConirmedMove() {

      var parent                = _topicRepository.Load("Root:Web:Web_0");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_0");
      var sibling               = _topicRepository.Load("Root:Web:Web_0:Web_0_1");

      Assert.ReferenceEquals(topic.Parent, parent);
      Assert.AreEqual<string>("Web_0_0", parent.Children.First().Key);
      Assert.AreEqual<int>(2, parent.Children.Count);

      _topicRepository.Move(topic, parent, sibling);

      Assert.ReferenceEquals(topic.Parent, parent);
      Assert.AreEqual<int>(2, parent.Children.Count);
      Assert.AreEqual<string>("Web_0_1", parent.Children.First().Key);
      Assert.AreEqual<string>("Web_0_0", parent.Children[1].Key);

    }

    /*==========================================================================================================================
    | TEST: DELETE: TOPIC: REMOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic to ensure it is properly removed.
    /// </summary>
    [TestMethod]
    public void Delete_Topic_Removed() {

      var parent                = _topicRepository.Load("Root:Web:Web_1");
      var topic                 = _topicRepository.Load("Root:Web:Web_1:Web_1_1");
      var child                 = _topicRepository.Load("Root:Web:Web_1:Web_1_1:Web_1_1_0");

      Assert.AreEqual<int>(2, parent.Children.Count);

      _topicRepository.Delete(topic);

      Assert.AreEqual<int>(1, parent.Children.Count);

    }

  } //Class
} //Namespace