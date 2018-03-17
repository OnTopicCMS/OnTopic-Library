/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
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
    | TEST: CREATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures it's correctly returned.
    /// </summary>
    [TestMethod]
    public void Topic_CreateTest() {
      var topic = TopicFactory.Create("Test", "ContentType");
      Assert.IsNotNull(topic);
      Assert.IsInstanceOfType(topic, typeof(ContentTypeDescriptor));
      Assert.AreEqual<string>(topic.Key, "Test");
      Assert.AreEqual<string>(topic.Attributes.GetValue("ContentType"), "ContentType");
    }

    /*==========================================================================================================================
    | TEST: IS EMPTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the default constructor, and ensures it's returned as empty.
    /// </summary>
    [TestMethod]
    public void Topic_IsEmptyTest() {
      var topic = new Topic();
      Assert.IsTrue(topic.IsEmpty);
    }

    /*==========================================================================================================================
    | TEST: CHANGE ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures that the ID cannot be modified.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException), "Topic permitted the ID to be reset; this should never happen.")]
    public void Topic_Change_IdTest() {

      var topic                 = TopicFactory.Create("Test", "ContentType", 123);
      topic.Id                  = 124;

      Assert.AreEqual<int>(123, topic.Id);
      Assert.AreNotEqual<int>(124, topic.Id);

    }

    /*==========================================================================================================================
    | TEST: IS (CONTENT) TYPE OF
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several contnet types, and confirms that the topic is reported as a type of those content
    ///   types.
    /// </summary>
    [TestMethod]
    public void Topic_IsContentTypeOf() {

      var contentType = (ContentTypeDescriptor)TopicFactory.Create("Root", "ContentTypeDescriptor");
      for (var i=0; i<5; i++) {
        var childContentType = (ContentTypeDescriptor)TopicFactory.Create("ContentType" + i, "ContentTypeDescriptor", contentType);
        contentType             = childContentType;
      }

      Assert.IsTrue(contentType.IsTypeOf("Root"));

    }

    /*==========================================================================================================================
    | TEST: SET PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    [TestMethod]
    public void Topic_Set_ParentTest() {

      var parentTopic           = TopicFactory.Create("Parent", "ContentType");
      var childTopic            = TopicFactory.Create("Child", "ContentType");

      parentTopic.Id            = 5;
      childTopic.Parent         = parentTopic;

      Assert.ReferenceEquals(parentTopic.Children["Child"], childTopic);
      Assert.AreEqual<int>(5, Int32.Parse(childTopic.Attributes.GetValue("ParentId", "0")));

    }

    /*==========================================================================================================================
    | TEST: CHANGE PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the parent of a topic and ensures it is correctly reflected in the object model.
    /// </summary>
    // ### TODO JJC20150816: This invokes dependencies on the TopicDataProvider and, in turn, the Configuration namespace.  This
    // is going to call for the creation of mocks and dependency injection before it will pass. In the meanwhile, it is disabled.
    public void Topic_Change_ParentTest() {

      var sourceParent          = TopicFactory.Create("SourceParent", "ContentType");
      var targetParent          = TopicFactory.Create("TargetParent", "ContentType");
      var childTopic            = TopicFactory.Create("ChildTopic", "ContentType");

      sourceParent.Id           = 5;
      targetParent.Id           = 10;
      childTopic.Parent         = sourceParent;
      childTopic.Parent         = targetParent;

      Assert.ReferenceEquals(targetParent.Children["ChildTopic"], childTopic);
      Assert.IsFalse(sourceParent.Children.Contains("ChildTopic"));
      Assert.AreEqual<int>(10, Int32.Parse(childTopic.Attributes.GetValue("ParentId", "0")));

    }

    /*==========================================================================================================================
    | TEST: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures the Unique Key is correct for a deeply nested child.
    /// </summary>
    [TestMethod]
    public void Topic_UniqueKeyTest() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page");
      var childTopic            = TopicFactory.Create("ChildTopic", "Page");
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page");

      childTopic.Parent         = parentTopic;
      grandChildTopic.Parent    = childTopic;

      Assert.AreEqual<string>("ParentTopic:ChildTopic:GrandChildTopic", grandChildTopic.UniqueKey);
      Assert.AreEqual<string>("/ParentTopic/ChildTopic/GrandChildTopic/", grandChildTopic.WebPath);

    }

    /*==========================================================================================================================
    | TEST: FIND ALL BY ATTRIBUTE VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks for a deeply nested child topic using only the attribute value.
    /// </summary>
    [TestMethod]
    public void Topic_FindAllByAttributeValueTest() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", 20);
      var grandNieceTopic       = TopicFactory.Create("GrandNieceTopic", "Page", 3);
      var greatGrandChildTopic  = TopicFactory.Create("GreatGrandChildTopic", "Page", 7);

      childTopic.Parent         = parentTopic;
      grandChildTopic.Parent    = childTopic;
      grandNieceTopic.Parent    = childTopic;
      greatGrandChildTopic.Parent = grandChildTopic;

      grandChildTopic.Attributes.SetValue("Foo", "Baz");
      greatGrandChildTopic.Attributes.SetValue("Foo", "Bar");
      grandNieceTopic.Attributes.SetValue("Foo", "Bar");

      Assert.ReferenceEquals(parentTopic.FindAllByAttribute("Foo", "Bar").First(), grandNieceTopic);
      Assert.AreEqual<int>(2, parentTopic.FindAllByAttribute("Foo", "Bar").Count());
      Assert.ReferenceEquals(parentTopic.FindAllByAttribute("Foo", "Baz").First(), grandChildTopic);

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks for a deeply nested child topic using the GetTopic() methods.
    /// </summary>
    [TestMethod]
    public void Topic_GetTopicTest() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", 2);

      childTopic.Parent         = parentTopic;
      grandChildTopic.Parent    = childTopic;

      Assert.ReferenceEquals(parentTopic.GetTopic("ParentTopic"), parentTopic);
      Assert.ReferenceEquals(parentTopic.GetTopic("ParentTopic:ChildTopic"), childTopic);
      Assert.ReferenceEquals(parentTopic.GetTopic("ParentTopic:ChildTopic:GrandChildTopic"), grandChildTopic);

      Assert.ReferenceEquals(childTopic.GetTopic("GrandChildTopic"), grandChildTopic);

      Assert.ReferenceEquals(parentTopic.GetTopic(1), parentTopic);
      Assert.ReferenceEquals(parentTopic.GetTopic(5), childTopic);
      Assert.ReferenceEquals(parentTopic.GetTopic(2), grandChildTopic);

    }

    /*==========================================================================================================================
    | TEST: IS VISIBLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that IsVisible returns expected values based on IsHidden and IsDisabled.
    /// </summary>
    [TestMethod]
    public void Topic_IsVisibleTest() {

      var hiddenTopic           = TopicFactory.Create("HiddenTopic", "Page");
      var disabledTopic         = TopicFactory.Create("DisabledTopic", "Page");
      var visibleTopic          = TopicFactory.Create("VisibleTopic", "Page");

      hiddenTopic.IsHidden      = true;
      disabledTopic.IsDisabled  = true;

      Assert.IsFalse(hiddenTopic.IsVisible());
      Assert.IsFalse(hiddenTopic.IsVisible(true));
      Assert.IsFalse(disabledTopic.IsVisible());
      Assert.IsTrue(disabledTopic.IsVisible(true));
      Assert.IsTrue(visibleTopic.IsVisible());
      Assert.IsTrue(visibleTopic.IsVisible(true));

    }

    /*==========================================================================================================================
    | TEST: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that the title falls back appropriately.
    /// </summary>
    [TestMethod]
    public void Topic_TitleTest() {

      var untitledTopic         = TopicFactory.Create("UntitledTopic", "Page");
      var titledTopic           = TopicFactory.Create("TitledTopic", "Page");

      titledTopic.Title         = "Titled Topic";

      Assert.AreEqual<string>(untitledTopic.Title, "UntitledTopic");
      Assert.AreEqual<string>(titledTopic.Title, "Titled Topic");

    }

    /*==========================================================================================================================
    | TEST: DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a derived topic, and ensures it is referenced correctly.
    /// </summary>
    [TestMethod]
    public void Topic_DerivedTopicTest() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      var derivedTopic          = TopicFactory.Create("DerivedTopic", "Page");

      topic.DerivedTopic        = derivedTopic;

      Assert.ReferenceEquals(topic.DerivedTopic, derivedTopic);

    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the <see cref="Ignia.Topics.AttributeDescriptor.DefaultConfiguration"/> property, then confirms that it is
    ///   correctly parsed via the <see cref="Ignia.Topics.AttributeDescriptor.Configuration"/> property.
    /// </summary>
    [TestMethod]
    public void Topic_AttributeConfigurationTest() {

      var attribute = (AttributeDescriptor)TopicFactory.Create("Topic", "AttributeDescriptor");
      attribute.DefaultConfiguration = "IsRequired=\"True\" DisplayName=\"Display Name\"";

      Assert.IsFalse(attribute.Configuration.ContainsKey("MissingAttribute"));
      Assert.IsTrue(attribute.Configuration.ContainsKey("IsRequired"));
      Assert.IsTrue(attribute.Configuration.ContainsKey("DisplayName"));
      Assert.ReferenceEquals("True", attribute.Configuration["IsRequired"]);
      Assert.ReferenceEquals("Display Name", attribute.Configuration["DisplayName"]);
      Assert.ReferenceEquals("True", attribute.GetConfigurationValue("DisplayName"));
      Assert.ReferenceEquals("NotFound", attribute.GetConfigurationValue("MissingAttribute", "NotFound"));

    }

  } //Class

} //Namespace
