/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Ignia.Topics.Collections;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Repositories;
using Ignia.Topics.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  /// <remarks>
  ///   These tests not only validate that the <see cref="FakeTopicRepository"/> is functioning as expected, but also that the
  ///   underlying <see cref="TopicRepositoryBase"/> functions are also operating correctly.
  /// </remarks>
  [TestClass]
  public class ITopicRepositoryTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    ITopicRepository            _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ITopicRepositoryTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public ITopicRepositoryTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topics and ensures there are the expected number of children.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_LoadTest() {

      var rootTopic             = _topicRepository.Load();
      var topic                 = _topicRepository.Load("Root:Configuration:ContentTypes:Page");
      var child                 = TopicFactory.Create("Child", "ContentType", Int32.MaxValue, topic);

      Assert.AreEqual<int>(2, rootTopic.Children.Count);
      Assert.AreEqual<string>("Configuration", rootTopic.Children.First().Key);
      Assert.AreEqual<int>(Int32.MaxValue, _topicRepository.Load(Int32.MaxValue).Id);

    }

    /*==========================================================================================================================
    | TEST: LOAD BY ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads topic by ID and ensures it is found.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_LoadByIdTest() {

      var topic                 = _topicRepository.Load(11111);

      Assert.IsNotNull(topic);
      Assert.AreEqual<string>("Web_1_1_1_1", topic.Key);

    }

    /*==========================================================================================================================
    | TEST: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves topics and ensures their identifiers are properly set.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_SaveTest() {

      var rootTopic             = _topicRepository.Load();
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
    | TEST: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topics and ensures their parents are correctly set.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_MoveTest() {

      var rootTopic             = _topicRepository.Load();
      var source                = _topicRepository.Load("Root:Web:Web_0");
      var destination           = _topicRepository.Load("Root:Web:Web_1");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_2");

      Assert.ReferenceEquals(topic.Parent, source);
      Assert.AreEqual<int>(3, destination.Children.Count());
      Assert.AreEqual<int>(3, source.Children.Count());

      _topicRepository.Move(topic, destination);

      Assert.ReferenceEquals(topic.Parent, destination);
      Assert.AreEqual<int>(2, source.Children.Count());
      Assert.AreEqual<int>(4, destination.Children.Count());

    }

    /*==========================================================================================================================
    | TEST: MOVE TO SIBLING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves topic next to a different sibling and ensures it ends up in the correct location.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_MoveToSiblingTest() {

      var rootTopic             = _topicRepository.Load();
      var parent                = _topicRepository.Load("Root:Web:Web_0");
      var topic                 = _topicRepository.Load("Root:Web:Web_0:Web_0_0");
      var sibling               = _topicRepository.Load("Root:Web:Web_0:Web_0_1");

      Assert.ReferenceEquals(topic.Parent, parent);
      Assert.AreEqual<string>("Web_0_0", parent.Children.First().Key);
      Assert.AreEqual<int>(3, parent.Children.Count());

      _topicRepository.Move(topic, parent, sibling);

      Assert.ReferenceEquals(topic.Parent, parent);
      Assert.AreEqual<int>(3, parent.Children.Count());
      Assert.AreEqual<string>("Web_0_1", parent.Children.First().Key);
      Assert.AreEqual<string>("Web_0_0", parent.Children[1].Key);

    }

    /*==========================================================================================================================
    | TEST: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic to ensure it is properly removed.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_DeleteTest() {

      var parent                = _topicRepository.Load("Root:Web:Web_2");
      var topic                 = _topicRepository.Load("Root:Web:Web_2:Web_2_2");
      var child                 = _topicRepository.Load("Root:Web:Web_2:Web_2_2:Web_2_2_0");

      Assert.AreEqual<int>(3, parent.Children.Count());

      _topicRepository.Delete(topic);

      Assert.AreEqual<int>(2, parent.Children.Count());

      topic = _topicRepository.Load("Root:Web:Web_2:Web_2_2");
      child = _topicRepository.Load("Root:Web:Web_2:Web_2_2:Web_2_2_0");

      Assert.IsNull(topic);
      Assert.IsNull(child);

    }


  } //Class

} //Namespace
