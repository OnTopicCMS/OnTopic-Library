/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Lookup;
using OnTopic.Metadata;
using OnTopic.Tests.TestDoubles;
using OnTopic.Tests.ViewModels;
using OnTopic.ViewModels;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE LOOKUP SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ITypeLookupService"/> interface and its implementations, including the
  ///   <see cref="StaticTypeLookupService"/> and <see cref="DynamicTypeLookupService"/>.
  /// </summary>
  [TestClass]
  public class ITypeLookupServiceTest {

    /*==========================================================================================================================
    | TEST: COMPOSITE: LOOKUP VALID TYPE: RETURNS TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="CompositeTypeLookupService"/> with two instances of a <see cref="ITypeLookupService"/> and
    ///   confirms that it returns the expected <see cref="Type"/> for a <see cref="ITypeLookupService.Lookup(String)"/> query.
    /// </summary>
    [TestMethod]
    public void Composite_LookupValidType_ReturnsType() {

      var lookupServiceA        = new FakeViewModelLookupService();
      var lookupServiceB        = new TopicViewModelLookupService();
      var compositeLookup       = new CompositeTypeLookupService(lookupServiceA, lookupServiceB);

      Assert.AreEqual(typeof(SlideshowTopicViewModel), compositeLookup.Lookup(nameof(SlideshowTopicViewModel)));
      Assert.AreEqual(typeof(MapToParentTopicViewModel), compositeLookup.Lookup(nameof(MapToParentTopicViewModel)));
      Assert.AreEqual(typeof(Object), compositeLookup.Lookup(nameof(Topic)));

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TOPIC LOOKUP SERVICE: LOOKUP MISSING ATTRIBUTE DESCRIPTOR: RETURNS ATTRIBUTE DESCRIPTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="DynamicTopicLookupService"/> and requests a missing attribute type; confirms it correctly
    ///   falls back to the expected <see cref="AttributeDescriptor"/> as a logical default.
    /// </summary>
    [TestMethod]
    public void DynamicTopicLookupService_LookupMissingAttributeDescriptor_ReturnsAttributeDescriptor() {

      var lookupService         = new DynamicTopicLookupService();
      var attributeType         = lookupService.Lookup("ArbitraryAttributeDescriptor");

      Assert.AreEqual(typeof(AttributeDescriptor), attributeType);

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TOPIC VIEW MODEL LOOKUP SERVICE: LOOKUP TOPIC VIEW MODEL: RETURNS FALLBACK VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="DynamicTopicViewModelLookupService"/> and requests a type with the <c>TopicViewModel</c>
    ///   suffix; confirms it correctly falls back to a type with the <c>ViewModel</c> suffix.
    /// </summary>
    [TestMethod]
    public void DynamicTopicViewModelLookupService_LookupTopicViewModel_ReturnsFallbackViewModel() {

      var lookupService         = new DynamicTopicViewModelLookupService();
      var topicViewModel        = lookupService.Lookup("FallbackTopicViewModel");

      Assert.AreEqual(typeof(FallbackViewModel), topicViewModel);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT TOPIC LOOKUP SERVICE: LOOKUP MISSING ATTRIBUTE DESCRIPTOR: RETURNS ATTRIBUTE DESCRIPTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="DefaultTopicLookupService"/> and requests a missing attribute type; confirms it correctly
    ///   falls back to the expected <see cref="AttributeDescriptor"/> as a logical default.
    /// </summary>
    [TestMethod]
    public void DefaultTopicLookupService_LookupMissingAttributeDescriptor_ReturnsAttributeDescriptor() {

      var lookupService         = new DefaultTopicLookupService();
      var attributeType         = lookupService.Lookup("ArbitraryAttributeDescriptor");

      Assert.AreEqual(typeof(AttributeDescriptor), attributeType);

    }

    /*==========================================================================================================================
    | TEST: DEFAULT TOPIC VIEW MODEL LOOKUP SERVICE: LOOKUP TOPIC VIEW MODEL: RETURNS FALLBACK VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TopicViewModelLookupService"/> and requests a type with the <c>TopicViewModel</c>
    ///   suffix; confirms it correctly falls back to a type with the <c>ViewModel</c> suffix.
    /// </summary>
    [TestMethod]
    public void TopicViewModelLookupService_LookupTopicViewModel_ReturnsFallbackViewModel() {

      var lookupService         = new FakeViewModelLookupService();
      var topicViewModel        = lookupService.Lookup("FallbackTopicViewModel");

      Assert.AreEqual(typeof(FallbackViewModel), topicViewModel);

    }

  } //Class
} //Namespace