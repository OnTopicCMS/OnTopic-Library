/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Reflection;
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
    | TEST: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and ensures that the key can be returned as an attribute.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_GetValueTest() {
      var topic = TopicFactory.Create("Test", "Container");
      Assert.AreEqual<string>("Test", topic.Attributes.GetValue("Key"));
    }

    /*==========================================================================================================================
    | TEST: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_GetIntegerTest() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetInteger("Number1", 1);
      topic.Attributes.SetInteger("Number2", 2);
      topic.Attributes.SetValue("Number3", "Invalid");

      Assert.AreEqual<int>(1, topic.Attributes.GetInteger("Number1", 5));
      Assert.AreEqual<int>(2, topic.Attributes.GetInteger("Number2", 5));
      Assert.AreEqual<int>(5, topic.Attributes.GetInteger("Number3", 5));
      Assert.AreEqual<int>(5, topic.Attributes.GetInteger("InvalidKey", 5));

    }

    /*==========================================================================================================================
    | TEST: GET DATETIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that integer values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_GetDateTimeTest() {

      var topic                 = TopicFactory.Create("Test", "Container");
      var dateTime1             = new DateTime(1976, 10, 15);
      var dateTime2             = new DateTime(1981, 06, 03);

      topic.Attributes.SetDateTime("DateTime1", dateTime1);
      topic.Attributes.SetDateTime("DateTime2", dateTime2);
      topic.Attributes.SetValue("DateTime3", "Invalid");

      Assert.AreEqual<DateTime>(dateTime1, topic.Attributes.GetDateTime("DateTime1", DateTime.MinValue));
      Assert.AreEqual<DateTime>(dateTime2, topic.Attributes.GetDateTime("DateTime2", DateTime.MinValue));
      Assert.AreEqual<DateTime>(dateTime1, topic.Attributes.GetDateTime("DateTime3", dateTime1));
      Assert.AreEqual<DateTime>(dateTime2, topic.Attributes.GetDateTime("InvalidKey", dateTime2));

    }

    /*==========================================================================================================================
    | TEST: GET BOOLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that boolean values can be set and retrieved as expected.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_GetBooleanTest() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetBoolean("IsValue1", true);
      topic.Attributes.SetBoolean("IsValue2", false);
      topic.Attributes.SetValue("IsValue3", "Invalid");

      Assert.IsTrue(topic.Attributes.GetBoolean("IsValue1", false));
      Assert.IsFalse(topic.Attributes.GetBoolean("IsValue2", true));
      Assert.IsTrue(topic.Attributes.GetBoolean("IsValue3", true));
      Assert.IsFalse(topic.Attributes.GetBoolean("IsValue3", false));
      Assert.IsTrue(topic.Attributes.GetBoolean("InvalidKey", true));
      Assert.IsFalse(topic.Attributes.GetBoolean("InvalidKey", false));

    }



    /*==========================================================================================================================
    | TEST: DEFAULT VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a new topic and requests an invalid attribute; ensures falls back to the default.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_DefaultValueTest() {
      var topic = TopicFactory.Create("Test", "Container");
      Assert.AreEqual<string>("Foo", topic.Attributes.GetValue("InvalidAttribute", "Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic and ensures it can be retrieved.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_SetValueTest() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.SetValue("Foo", "Bar");
      Assert.AreEqual<string>("Bar", topic.Attributes.GetValue("Foo"));
    }

    /*==========================================================================================================================
    | TEST: SET VALUE: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Modifies the value of a custom attribute on a topic and ensures it is marked as IsDirty.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_SetValue_IsDirtyTest() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Foo", "Bar", false);
      topic.Attributes.SetValue("Fah", "Bar", false);
      topic.Attributes.SetValue("Foo", "Baz");
      topic.Attributes.SetValue("Fah", "Bar");

      Assert.AreEqual<string>("Baz", topic.Attributes.GetValue("Foo"));
      Assert.AreEqual<string>("Bar", topic.Attributes.GetValue("Fah"));
      Assert.AreEqual<bool>(true, topic.Attributes["Foo"].IsDirty);
      Assert.AreEqual<bool>(false, topic.Attributes["Fah"].IsDirty);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE BACKDOOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a custom attribute on a topic by directly adding an <see cref="AttributeValue"/> instance; ensures it can be
    ///   retrieved.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_SetValue_BackdoorTest() {

      var topic = TopicFactory.Create("Test", "Container");

      topic.Attributes.SetValue("Key", "NewKey");
      topic.Attributes.SetValue("Foo", "Bar");

      Assert.AreEqual<string>("NewKey", topic.Key);
      Assert.AreEqual<string>("Bar", topic.Attributes.GetValue("Foo"));

    }

    /*==========================================================================================================================
    | TEST: ENFORCE BUSINESS LOGIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing the property setter; ensures that business logic is enforced.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(TargetInvocationException), "The topic allowed a key to be set via a backdoor, without routing it through the Key property.")]
    public void AttributeValueCollection_EnforceBusinessLogicTest() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.SetValue("Key", "# ?");
    }

    /*==========================================================================================================================
    | TEST: ENFORCE BUSINESS LOGIC BACKDOOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to violate the business logic by bypassing SetValue() entirely; ensures that business logic is enforced.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(TargetInvocationException), "The topic allowed a key to be set via a backdoor, without routing it through the Key property.")]
    public void AttributeValueCollection_EnforceBusinessLogic_BackdoorTest() {
      var topic = TopicFactory.Create("Test", "Container");
      topic.Attributes.Remove("Key");
      topic.Attributes.Add(new AttributeValue("Key", "# ?"));
    }

    /*==========================================================================================================================
    | TEST: INHERIT FROM PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the parent of a topic and ensures it can be retrieved using inheritance.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_InheritFromParentTest() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = TopicFactory.Create("Topic" + i, "Container");
        if (i > 0) topic.Parent = topics[i - 1];
        topics[i] = topic;
      }

      topics[0].Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(topics[4].Attributes.GetValue("Foo", null));
      Assert.AreEqual<string>("Bar", topics[7].Attributes.GetValue("Foo", true));
      Assert.AreNotEqual<string>("Bar", topics[7].Attributes.GetValue("Foo", false));

    }

    /*==========================================================================================================================
    | TEST: MAX HOPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a long tree of derives topics, and ensures that inheritance will pursue no more than five hops.
    /// </summary>
    [TestMethod]
    public void AttributeValueCollection_MaxHopsTest() {

      var topics = new Topic[8];

      for (var i = 0; i <= 7; i++) {
        var topic = TopicFactory.Create("Topic" + i, "Container");
        if (i > 0) topics[i - 1].DerivedTopic = topic;
        topics[i] = topic;
      }

      topics[7].Attributes.SetValue("Foo", "Bar");

      Assert.IsNull(topics[0].Attributes.GetValue("Foo", null, true, false));
      Assert.AreEqual<string>("Bar", topics[2].Attributes.GetValue("Foo", null, true, true));
      Assert.AreNotEqual<string>("Bar", topics[1].Attributes.GetValue("Foo", null, true, true));
      Assert.AreNotEqual<string>("Bar", topics[0].Attributes.GetValue("Foo", null, true, true));

    }

  } //Class

} //Namespace
