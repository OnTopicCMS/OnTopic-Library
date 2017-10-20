/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Reflection;
using Ignia.Topics.Collections;
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
    public ITopicRepositoryTest() {
      _topicRepository = new FakeTopicRepository();
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
      var topic                 = rootTopic.GetTopic("Root:Configuration:ContentTypes:Page");
      var child                 = Topic.Create("Child", "ContentType", Int32.MaxValue, topic);

      Assert.AreEqual<int>(2, rootTopic.Children.Count);
      Assert.AreEqual<string>("Configuration", rootTopic.Children.First().Key);
      Assert.AreEqual<int>(Int32.MaxValue, rootTopic.GetTopic(Int32.MaxValue).Id);

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
      var web                   = rootTopic.GetTopic("Root:Web");
      var configuration         = rootTopic.GetTopic("Root:Configuration");

      Assert.AreEqual<int>(-1, web.Id);
      Assert.AreEqual<int>(-1, configuration.Id);

      _topicRepository.Save(web);

      Assert.AreNotEqual<int>(-1, web.Id);
      Assert.AreEqual<int>(-1, web.Children.First().Id);

      _topicRepository.Save(web, true);

      Assert.AreNotEqual<int>(-1, web.Children.First().Id);
      Assert.AreEqual<int>(-1, configuration.Id);

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
      var source                = rootTopic.GetTopic("Root:Web:Web_0");
      var destination           = rootTopic.GetTopic("Root:Web:Web_1");
      var topic                 = rootTopic.GetTopic("Root:Web:Web_0:Web_0_2");

      Assert.ReferenceEquals(topic.Parent, source);
      Assert.AreEqual<int>(3, destination.Children.Count());
      Assert.AreEqual<int>(3, source.Children.Count());

      _topicRepository.Move(topic, destination);

      Assert.ReferenceEquals(topic.Parent, destination);
      Assert.AreEqual<int>(2, source.Children.Count());
      Assert.AreEqual<int>(4, destination.Children.Count());

    }

    /*==========================================================================================================================
    | TEST: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic to ensure it is properly removed.
    /// </summary>
    [TestMethod]
    public void ITopicRepository_DeleteTest() {

      var rootTopic             = _topicRepository.Load();
      var parent                = rootTopic.GetTopic("Root:Web:Web_2");
      var topic                 = rootTopic.GetTopic("Root:Web:Web_2:Web_2_2");
      var child                 = rootTopic.GetTopic("Root:Web:Web_2:Web_2_2:Web_2_2_0");

      Assert.AreEqual<int>(3, parent.Children.Count());

      _topicRepository.Delete(topic);

      Assert.AreEqual<int>(2, parent.Children.Count());

      topic = rootTopic.GetTopic("Root:Web:Web_2:Web_2_2");
      child = rootTopic.GetTopic("Root:Web:Web_2:Web_2_2:Web_2_2_0");

      Assert.IsNull(topic);
      Assert.IsNull(child);

    }


  } //Class

} //Namespace
