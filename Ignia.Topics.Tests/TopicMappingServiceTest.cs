/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Ignia.Topics.Collections;
using Ignia.Topics.Models;
using Ignia.Topics.Tests.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicMappingService"/> using local DTOs.
  /// </summary>
  [TestClass]
  public class TopicMappingServiceTest {

    /*==========================================================================================================================
    | TEST: MAP (INSTANCE)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by providing it with an already
    ///   constructed instance of a DTO.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapGeneric() {

      var mappingService        = new TopicMappingService();
      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target = (PageTopicViewModel)mappingService.Map(topic, new PageTopicViewModel());

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests setting basic scalar values by allowing it to dynamically
    ///   determine the instance type.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapDynamic() {

      var mappingService        = new TopicMappingService();
      var topic                 = TopicFactory.Create("Test", "Page");

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      var target = (PageTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: MAP PARENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the parent tree.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapParents() {

      var mappingService        = new TopicMappingService();
      var grandParent           = TopicFactory.Create("Grandparent", "Sample");
      var parent                = TopicFactory.Create("Parent", "Page", grandParent);
      var topic                 = TopicFactory.Create("Test", "Page", parent);

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      parent.Attributes.SetValue("Title", "Value2");
      parent.Attributes.SetValue("IsHidden", "1");

      grandParent.Attributes.SetValue("Title", "Value3");
      grandParent.Attributes.SetValue("IsHidden", "1");
      grandParent.Attributes.SetValue("Property", "ValueB");

      var viewModel             = (PageTopicViewModel)mappingService.Map(topic);
      var parentViewModel       = viewModel?.Parent;
      var grandParentViewModel  = parentViewModel?.Parent as SampleTopicViewModel;

      Assert.IsNotNull(viewModel);
      Assert.IsNotNull(parentViewModel);
      Assert.IsNotNull(grandParentViewModel);
      Assert.AreEqual<string>("ValueA", viewModel.MetaTitle);
      Assert.AreEqual<string>("Value1", viewModel.Title);
      Assert.AreEqual<bool>(true, viewModel.IsHidden);
      Assert.AreEqual<string>("Value2", parentViewModel.Title);
      Assert.AreEqual<string>("Value3", grandParentViewModel.Title);
      Assert.AreEqual<string>("ValueB", grandParentViewModel.Property);

    }

    /*==========================================================================================================================
    | TEST: MAP RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the relationships.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapRelationships() {

      var mappingService        = new TopicMappingService();
      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "Page");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "Index");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "Page");
      var topic                 = TopicFactory.Create("Test", "Sample");

      topic.Relationships.SetTopic("Cousins", relatedTopic1);
      topic.Relationships.SetTopic("Cousins", relatedTopic2);
      topic.Relationships.SetTopic("Siblings", relatedTopic3);

      var target = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.Cousins.Count);
      Assert.IsNotNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic1")));
      Assert.IsNotNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic2")));
      Assert.IsNull(target.Cousins.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));

    }

    /*==========================================================================================================================
    | TEST: MAP NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapNestedTopics() {

      var mappingService        = new TopicMappingService();
      var topic                 = TopicFactory.Create("Test", "Sample");
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", topic);
      var topicList             = TopicFactory.Create("Categories", "TopicList", topic);
      var nestedTopic1          = TopicFactory.Create("NestedTopic1", "Page", topicList);
      var nestedTopic2          = TopicFactory.Create("NestedTopic2", "Index", topicList);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(2, target.Categories.Count);
      Assert.IsNotNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("NestedTopic1")));
      Assert.IsNotNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("NestedTopic2")));
      Assert.IsNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("Categories")));
      Assert.IsNull(target.Categories.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic")));

    }

    /*==========================================================================================================================
    | TEST: MAP CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully crawls the nested topics.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapChildren() {

      var mappingService        = new TopicMappingService();
      var topic                 = TopicFactory.Create("Test", "Sample");
      var childTopic1           = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2           = TopicFactory.Create("ChildTopic2", "Page", topic);
      var childTopic3           = TopicFactory.Create("ChildTopic3", "Sample", topic);
      var childTopic4           = TopicFactory.Create("ChildTopic4", "Index", childTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<int>(3, target.Children.Count);
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic1")));
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic2")));
      Assert.IsNotNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3")));
      Assert.IsNull(target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic4")));
      Assert.AreEqual<int>(0, ((SampleTopicViewModel)target.Children.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3"))).Children.Count);

    }

    /*==========================================================================================================================
    | TEST: MAP TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether it successfully includes <see cref="Topic"/>
    ///   instances if called for by the model. This isn't a best practice, but is maintained for edge cases.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapTopics() {

      var mappingService        = new TopicMappingService();
      var relatedTopic1         = TopicFactory.Create("RelatedTopic1", "Page");
      var relatedTopic2         = TopicFactory.Create("RelatedTopic2", "Index");
      var relatedTopic3         = TopicFactory.Create("RelatedTopic3", "Page");
      var topic                 = TopicFactory.Create("Test", "Sample");

      topic.Relationships.SetTopic("Related", relatedTopic1);
      topic.Relationships.SetTopic("Related", relatedTopic2);
      topic.Relationships.SetTopic("Related", relatedTopic3);

      var target                = (SampleTopicViewModel)mappingService.Map(topic);
      var relatedTopic3copy     = ((Topic)target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));

      Assert.AreEqual<int>(3, target.Related.Count);
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic1")));
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic2")));
      Assert.IsNotNull(target.Related.FirstOrDefault((t) => t.Key.StartsWith("RelatedTopic3")));
      Assert.AreEqual(relatedTopic3.Key, relatedTopic3copy.Key);

    }

    /*==========================================================================================================================
    | TEST: FILTER BY CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TopicMappingService"/> and tests whether the resulting object's <see
    ///   cref="SampleTopicViewModel.Children"/> property can be filtered by <see cref="TopicViewModel.ContentType"/>.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_FilterByContentType() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Test", "Sample");
      var childTopic1 = TopicFactory.Create("ChildTopic1", "Page", topic);
      var childTopic2 = TopicFactory.Create("ChildTopic2", "Index", topic);
      var childTopic3 = TopicFactory.Create("ChildTopic3", "Index", topic);
      var childTopic4 = TopicFactory.Create("ChildTopic4", "Index", childTopic3);

      var target = (SampleTopicViewModel)mappingService.Map(topic);

      var indexes = target.Children.GetByContentType("Index");

      Assert.AreEqual<int>(2, indexes.Count);
      Assert.IsNotNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic2")));
      Assert.IsNotNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic3")));
      Assert.IsNull(indexes.FirstOrDefault((t) => t.Key.StartsWith("ChildTopic4")));

    }

    /*==========================================================================================================================
    | TEST: MAP GETTER METHODS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a property corresponding to a getter method on <see cref="Topic"/> to ensure that it is
    ///   correctly populated.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapGetterMethods() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Topic", "Sample");
      var childTopic = TopicFactory.Create("Child", "Page", topic);
      var grandChildTopic = TopicFactory.Create("GrandChild", "Index", childTopic);

      var target = (IndexTopicViewModel)mappingService.Map(grandChildTopic);

      Assert.AreEqual<string>("Topic:Child:GrandChild", target.UniqueKey);

    }

    /*==========================================================================================================================
    | TEST: MAP REQUIRED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is not thrown if it is set.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapRequiredProperty() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Topic", "Required");

      topic.Attributes.SetValue("RequiredAttribute", "Required");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("Required", target.RequiredAttribute);

    }

    /*==========================================================================================================================
    | TEST: MAP REQUIRED PROPERTY EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TopicMappingService_MapRequiredPropertyException() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Topic", "Required");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

    }

    /*==========================================================================================================================
    | TEST: REQUIRED OBJECT PROPERTY EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has a required property. Ensures that an error is thrown if it isn't set.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    public void TopicMappingService_MapRequiredObjectPropertyException() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Topic", "RequiredObject");

      var target = (RequiredTopicViewModel)mappingService.Map(topic);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT VALUE PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a content type that has default properties. Ensures that each is set appropriately.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapDefaultValueProperties() {

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Topic", "DefaultValue");

      var target = (DefaultValueTopicViewModel)mappingService.Map(topic);

      Assert.AreEqual<string>("Default", target.DefaultString);
      Assert.AreEqual<int>(10, target.DefaultInt);
      Assert.IsTrue(target.DefaultBool);

    }


  } //Class

} //Namespace

