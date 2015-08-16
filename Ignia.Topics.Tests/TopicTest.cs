using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  [TestClass]
  public class TopicTest {

    [TestMethod]
    public void CreateTopicTest() {
      Topic topic = Topic.Create("Test", "ContentType");
      Assert.IsNotNull(topic);
      Assert.IsInstanceOfType(topic, typeof(ContentType));
      Assert.AreEqual<string>(topic.Key, "Test");
      Assert.AreEqual<string>(topic.GetAttribute("ContentType"), "ContentType");
    }

    [TestMethod]
    public void GetAttributeTest() {
      Topic topic = new Topic("Test");
      Assert.AreEqual<string>("Test", topic.GetAttribute("Key"));
    }

    [TestMethod]
    public void DefaultAttributeTest() {
      Topic topic = new Topic("Test");
      Assert.AreEqual<string>("Foo", topic.GetAttribute("InvalidAttribute", "Foo"));
    }

    [TestMethod]
    public void SetAttributeTest() {
      Topic topic = new Topic("Test");
      topic.Attributes.SetAttributeValue("Foo", "Bar");
      Assert.AreEqual<string>("Bar", topic.GetAttribute("Foo"));
    }

    [TestMethod]
    public void AttributeInheritanceTest() {
      Topic childTopic = new Topic("Child");
      Topic parentTopic = new Topic("Parent");

      childTopic.Parent = parentTopic;
      parentTopic.Attributes.SetAttributeValue("Foo", "Bar");

      Assert.IsNull(childTopic.GetAttribute("Foo", null));
      Assert.AreEqual<string>(childTopic.GetAttribute("Foo", null, true), "Bar");

    }

    [TestMethod]
    public void LimitAttributeInheritanceTest() {

      Topic[] topics = new Topic[5];

      for (int i=0; i<=4; i++) {
        Topic topic = new Topic("Topic" + i);
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.SetAttributeValue("Foo", "Bar");

      Assert.IsNull(topics[4].GetAttribute("Foo", null));
      Assert.AreEqual<string>(topics[4].GetAttribute("Foo", true), "Bar");

    }

    [TestMethod]
    public void SetParentTest() {
      Topic parentTopic = Topic.Create("Parent", "ContentType");
      Topic childTopic = Topic.Create("Child", "ContentType");

      parentTopic.Id = 5;
      childTopic.Parent = parentTopic;

      Assert.ReferenceEquals(parentTopic["Child"], childTopic);
      Assert.AreEqual<int>(5, Int32.Parse(childTopic.GetAttribute("ParentId", "0")));

    }

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
      Assert.AreEqual<int>(10, Int32.Parse(childTopic.GetAttribute("ParentId", "0")));

    }

  } //Class
} //Namespace
