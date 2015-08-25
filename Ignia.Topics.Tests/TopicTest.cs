/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="Topic"/> class.
  /// </summary>
  [TestClass]
  public class TopicTest {

    /*==========================================================================================================================
    | TEST: CREATE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures it's correctly returned.
    /// </summary>
    [TestMethod]
    public void CreateTopicTest() {
      Topic topic = Topic.Create("Test", "ContentType");
      Assert.IsNotNull(topic);
      Assert.IsInstanceOfType(topic, typeof(ContentType));
      Assert.AreEqual<string>(topic.Key, "Test");
      Assert.AreEqual<string>(topic.Attributes.Get("ContentType"), "ContentType");
    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and ensures that the key can be returned as an attribute.
    /// </summary>
    [TestMethod]
    public void GetAttributeTest() {
      Topic topic = new Topic("Test");
      Assert.AreEqual<string>("Test", topic.Attributes.Get("Key"));
    }

    /*==========================================================================================================================
    | TEST: DEFAULT ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and requests an invalid attribute; ensures falls back to the default.
    /// </summary>
    [TestMethod]
    public void DefaultAttributeTest() {
      Topic topic = new Topic("Test");
      Assert.AreEqual<string>("Foo", topic.Attributes.Get("InvalidAttribute", "Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic and ensures it can be retrieved.
    /// </summary>
    [TestMethod]
    public void SetAttributeTest() {
      Topic topic = new Topic("Test");
      topic.Attributes.Set("Foo", "Bar");
      Assert.AreEqual<string>("Bar", topic.Attributes.Get("Foo"));
    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE INHERITANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the parent of a topic and ensures it can be retrieved using inheritance.
    /// </summary>
    [TestMethod]
    public void AttributeInheritanceTest() {
      Topic childTopic = new Topic("Child");
      Topic parentTopic = new Topic("Parent");

      childTopic.Parent = parentTopic;
      parentTopic.Attributes.Set("Foo", "Bar");

      Assert.IsNull(childTopic.Attributes.Get("Foo", null));
      Assert.AreEqual<string>(childTopic.Attributes.Get("Foo", null, true), "Bar");

    }

    /*==========================================================================================================================
    | TEST: LIMIT ATTRIBUTE INHERITANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on a descendent and ensures that it is correctly retrieved via inheritance.
    /// </summary>
    [TestMethod]
    public void LimitAttributeInheritanceTest() {

      Topic[] topics = new Topic[5];

      for (int i=0; i<=4; i++) {
        Topic topic = new Topic("Topic" + i);
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.Set("Foo", "Bar");

      Assert.IsNull(topics[4].Attributes.Get("Foo", null));
      Assert.AreEqual<string>(topics[4].Attributes.Get("Foo", true), "Bar");

    }

    /*==========================================================================================================================
    | TEST: SET PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [TestMethod]
    public void SetParentTest() {
      Topic parentTopic = Topic.Create("Parent", "ContentType");
      Topic childTopic = Topic.Create("Child", "ContentType");

      parentTopic.Id = 5;
      childTopic.Parent = parentTopic;

      Assert.ReferenceEquals(parentTopic["Child"], childTopic);
      Assert.AreEqual<int>(5, Int32.Parse(childTopic.Attributes.Get("ParentId", "0")));

    }

    /*==========================================================================================================================
    | TEST: CHANGE PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    // ### TODO JJC20150816: This invokes dependencies on the TopicDataProvider and, in turn, the Configuration namespace.  This
    // is going to call for the creation of mocks and dependency injection before it will pass. In the meanwhile, it is disabled.
    public void ChangeParentTest() {
      Topic sourceParent = Topic.Create("SourceParent", "ContentType");
      Topic targetParent = Topic.Create("TargetParent", "ContentType");
      Topic childTopic = Topic.Create("ChildTopic", "ContentType");

      sourceParent.Id = 5;
      targetParent.Id = 10;
      childTopic.Parent = sourceParent;
      childTopic.Parent = targetParent;

      Assert.ReferenceEquals(targetParent["ChildTopic"], childTopic);
      Assert.IsFalse(sourceParent.Contains("ChildTopic"));
      Assert.AreEqual<int>(10, Int32.Parse(childTopic.Attributes.Get("ParentId", "0")));

    }

  } //Class
} //Namespace
