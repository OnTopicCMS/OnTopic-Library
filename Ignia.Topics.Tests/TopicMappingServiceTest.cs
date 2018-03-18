﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Ignia.Topics.Collections;
using Ignia.Topics.Models;
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

      var mappingService = new TopicMappingService();
      var topic = TopicFactory.Create("Test", "Page");

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

  } //Class

  public class SampleTopicViewModel : PageTopicViewModel {
    public string Property { get; set; }
    public Collection<PageTopicViewModel> Children { get; set; } = new Collection<PageTopicViewModel>();
    public Collection<PageTopicViewModel> Cousins { get; set; } = new Collection<PageTopicViewModel>();
    public Collection<PageTopicViewModel> Categories { get; set; } = new Collection<PageTopicViewModel>();
  } //Class

} //Namespace
