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
    | TEST: CREATE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures it's correctly returned.
    /// </summary>
    [TestMethod]
    public void CreateTopicTest() {
      var topic = Topic.Create("Test", "ContentType");
      Assert.IsNotNull(topic);
      Assert.IsInstanceOfType(topic, typeof(ContentType));
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
    public void IsEmptyTest() {
      var topic = new Topic();
      Assert.IsTrue(topic.IsEmpty);
    }

    /*==========================================================================================================================
    | TEST: RENAME ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a topic using the factory method, and ensures that the ID cannot be modified.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ArgumentException), "Topic permitted the ID to be reset; this should never happen.")]
    public void RenameIdTest() {
      var topic = Topic.Create("Test", "ContentType", 123);
      topic.Id = 124;
    }

    /*==========================================================================================================================
    | TEST: IS (CONTENT) TYPE OF
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates a new topic with several contnet types, and confirms that the topic is reported as a type of those content
    ///   types.
    /// </summary>
    [TestMethod]
    public void IsContentTypeOf() {
      var contentType = new ContentType("Root");
      for (var i=0; i<5; i++) {
        var childContentType = new ContentType("ContentType" + i) {
          Parent = contentType
        };
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
    public void SetParentTest() {

      var parentTopic           = Topic.Create("Parent", "ContentType");
      var childTopic            = Topic.Create("Child", "ContentType");

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
    public void ChangeParentTest() {

      var sourceParent          = Topic.Create("SourceParent", "ContentType");
      var targetParent          = Topic.Create("TargetParent", "ContentType");
      var childTopic            = Topic.Create("ChildTopic", "ContentType");

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
    public void UniqueKeyTest() {

      var parentTopic = Topic.Create("ParentTopic", "Page");
      var childTopic = Topic.Create("ChildTopic", "Page");
      var grandChildTopic = Topic.Create("GrandChildTopic", "Page");

      childTopic.Parent = parentTopic;
      grandChildTopic.Parent = childTopic;

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
    public void FindAllByAttributeValueTest() {

      var parentTopic = Topic.Create("ParentTopic", "Page");
      var childTopic = Topic.Create("ChildTopic", "Page");
      var grandChildTopic = Topic.Create("GrandChildTopic", "Page");

      childTopic.Parent = parentTopic;
      grandChildTopic.Parent = childTopic;

      grandChildTopic.Attributes.SetValue("Foo", "Bar");

      Assert.ReferenceEquals(parentTopic.FindAllByAttribute("Foo", "Bar").First(), grandChildTopic);
      Assert.AreEqual<int>(1, parentTopic.FindAllByAttribute("Foo", "Bar").Count());

    }

    /*==========================================================================================================================
    | TEST: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks for a deeply nested child topic using the GetTopic() methods.
    /// </summary>
    [TestMethod]
    public void GetTopicTest() {

      var parentTopic = Topic.Create("ParentTopic", "Page", 1);
      var childTopic = Topic.Create("ChildTopic", "Page", 5);
      var grandChildTopic = Topic.Create("GrandChildTopic", "Page", 2);

      childTopic.Parent = parentTopic;
      grandChildTopic.Parent = childTopic;

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
    public void IsVisibleTest() {

      var hiddenTopic = Topic.Create("HiddenTopic", "Page");
      var disabledTopic = Topic.Create("DisabledTopic", "Page");
      var visibleTopic = Topic.Create("VisibleTopic", "Page");

      hiddenTopic.IsHidden = true;
      disabledTopic.IsDisabled = true;

      Assert.IsFalse(hiddenTopic.IsVisible());
      Assert.IsFalse(hiddenTopic.IsVisible(true));
      Assert.IsFalse(disabledTopic.IsVisible());
      Assert.IsTrue(disabledTopic.IsVisible(true));
      Assert.IsTrue(visibleTopic.IsVisible());
      Assert.IsTrue(visibleTopic.IsVisible(true));

    }

    /*==========================================================================================================================
    | TEST: TITLE TEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that the title falls back appropriately.
    /// </summary>
    [TestMethod]
    public void TitleTest() {

      var untitledTopic = Topic.Create("UntitledTopic", "Page");
      var titledTopic = Topic.Create("TitledTopic", "Page");

      titledTopic.Title = "Titled Topic";

      Assert.AreEqual<string>(untitledTopic.Title, "UntitledTopic");
      Assert.AreEqual<string>(titledTopic.Title, "Titled Topic");

    }

    /*==========================================================================================================================
    | TEST: DERIVED TOPIC TEST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a derived topic, and ensures it is referenced correctly.
    /// </summary>
    [TestMethod]
    public void DerivedTopicTest() {

      var topic = Topic.Create("Topic", "Page");
      var derivedTopic = Topic.Create("DerivedTopic", "Page");

      topic.DerivedTopic = derivedTopic;

      Assert.ReferenceEquals(topic.DerivedTopic, derivedTopic);

    }

  } //Class
} //Namespace
