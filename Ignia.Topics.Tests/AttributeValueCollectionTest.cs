/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: ATTRIBUTE VALUE COLLECTION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="AttributeValueCollection"/> class.
  /// </summary>
  [TestClass]
  public class AttributeValueCollectionTest {

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and ensures that the key can be returned as an attribute.
    /// </summary>
    [TestMethod]
    public void GetAttributeTest() {
      var topic = Topic.Create("Test", "Container");
      Assert.AreEqual<string>("Test", topic.Attributes.GetValue("Key"));
    }

    /*==========================================================================================================================
    | TEST: DEFAULT ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and requests an invalid attribute; ensures falls back to the default.
    /// </summary>
    [TestMethod]
    public void DefaultAttributeTest() {
      var topic = Topic.Create("Test", "Container");
      Assert.AreEqual<string>("Foo", topic.Attributes.GetValue("InvalidAttribute", "Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic and ensures it can be retrieved.
    /// </summary>
    [TestMethod]
    public void SetAttributeTest() {
      var topic = Topic.Create("Test", "Container");
      topic.Attributes.SetValue("Foo", "Bar");
      Assert.AreEqual<string>("Bar", topic.Attributes.GetValue("Foo"));
    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE INHERITANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the parent of a topic and ensures it can be retrieved using inheritance.
    /// </summary>
    [TestMethod]
    public void AttributeInheritanceTest() {

      var childTopic = Topic.Create("Child", "Container");
      var parentTopic = Topic.Create("Parent", "Container");

      childTopic.Parent = parentTopic;

      parentTopic.Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(childTopic.Attributes.GetValue("Foo", null));
      Assert.AreEqual<string>(childTopic.Attributes.GetValue("Foo", null, true), "Bar");

    }

    /*==========================================================================================================================
    | TEST: LIMIT ATTRIBUTE INHERITANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on a descendent and ensures that it is correctly retrieved via inheritance.
    /// </summary>
    [TestMethod]
    public void LimitAttributeInheritanceTest() {

      var topics = new Topic[5];

      for (var i = 0; i <= 4; i++) {
        var topic = Topic.Create("Topic" + i, "Container");
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(topics[4].Attributes.GetValue("Foo", null));
      Assert.AreEqual<string>(topics[4].Attributes.GetValue("Foo", true), "Bar");

    }

  } //Class
} //Namespace
