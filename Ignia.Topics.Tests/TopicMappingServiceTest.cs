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
      var parent = TopicFactory.Create("Parent", "Sample");
      var topic = TopicFactory.Create("Test", "Sample", parent);

      topic.Attributes.SetValue("Title", "Value1");
      topic.Attributes.SetValue("IsHidden", "1");

      parent.Attributes.SetValue("Title", "Value2");
      parent.Attributes.SetValue("IsHidden", "1");

      var target = (SampleTopicViewModel)mappingService.Map(topic, new SampleTopicViewModel());

      Assert.AreEqual<string>("Value1", target.Title);
      Assert.AreEqual<bool>(true, target.IsHidden);
      Assert.AreEqual<string>("Value2", target.Parent.Title);

    }

  } //Class

  /*==========================================================================================================================
  | SAMPLE TOPIC VIEW MODEL
  \-------------------------------------------------------------------------------------------------------------------------*/
  public class SampleTopicViewModel {
    public string Title { get; set; }
    public bool IsHidden { get; set; }
    public SampleTopicViewModel Parent { get; set; }
  }


} //Namespace

