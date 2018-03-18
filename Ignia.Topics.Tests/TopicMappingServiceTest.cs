/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
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
    | TEST: PROPERTY INFO COLLECTION CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="PropertyInfoCollection"/> based on a type, and confirms that the property collection is
    ///   returning expected types.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapGeneric() {

      var mappingService = new TopicMappingService();
      var grandParent = TopicFactory.Create("Grandparent", "Page");
      var parent = TopicFactory.Create("Parent", "Page", grandParent);
      var topic = TopicFactory.Create("Test", "Page", parent);

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      parent.Attributes.SetValue("Title", "Value2");
      parent.Attributes.SetValue("IsHidden", "1");

      grandParent.Attributes.SetValue("Title", "Value3");
      grandParent.Attributes.SetValue("IsHidden", "1");

      var target = (PageTopicViewModel)mappingService.Map(topic, new PageTopicViewModel());

      Assert.AreEqual<string>("ValueA", target.MetaTitle);
      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);
      Assert.AreEqual<string>("Value2", target.Parent.Title);
      Assert.AreEqual<string>("Value3", target.Parent.Parent.Title);

    }

    /*==========================================================================================================================
    | TEST: PROPERTY INFO COLLECTION CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="PropertyInfoCollection"/> based on a type, and confirms that the property collection is
    ///   returning expected types. This includes both types found in the <see cref="Models"/> namespace as well as a custom
    ///   class defined only in this test.
    /// </summary>
    [TestMethod]
    public void TopicMappingService_MapDynamic() {

      var mappingService = new TopicMappingService();
      var grandParent = TopicFactory.Create("Grandparent", "Sample");
      var parent = TopicFactory.Create("Parent", "Page", grandParent);
      var topic = TopicFactory.Create("Test", "Page", parent);

      topic.Attributes.SetValue("MetaTitle", "ValueA");
      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      parent.Attributes.SetValue("Title", "Value2");
      parent.Attributes.SetValue("IsHidden", "1");

      grandParent.Attributes.SetValue("Title", "Value3");
      grandParent.Attributes.SetValue("IsHidden", "1");
      grandParent.Attributes.SetValue("Property", "ValueB");

      var viewModel = (PageTopicViewModel)mappingService.Map(topic);
      var parentViewModel = viewModel.Parent;
      var grandParentViewModel = parentViewModel.Parent as SampleTopicViewModel;

      Assert.AreEqual<string>("ValueA", viewModel.MetaTitle);
      Assert.AreEqual<string>("Value1", viewModel.Title);
      Assert.AreEqual<bool>(true, viewModel.IsHidden);
      Assert.AreEqual<string>("Value2", parentViewModel.Title);
      Assert.AreEqual<string>("Value3", grandParentViewModel.Title);
      Assert.AreEqual<string>("ValueB", grandParentViewModel.Property);

    }

  } //Class

  public class SampleTopicViewModel : PageTopicViewModel {
    public string Property { get; set; }
  } //Class

} //Namespace

