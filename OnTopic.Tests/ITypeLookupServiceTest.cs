/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Reflection;
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
    public void Assume_ObjectIsNull_ThrowInvalidOperationException() {

      var lookupServiceA        = new FakeViewModelLookupService();
      var lookupServiceB        = new TopicViewModelLookupService();
      var compositeLookup       = new CompositeTypeLookupService(lookupServiceA, lookupServiceB);

      Assert.AreEqual(typeof(SlideshowTopicViewModel), compositeLookup.Lookup(nameof(SlideshowTopicViewModel)));
      Assert.AreEqual(typeof(MapToParentTopicViewModel), compositeLookup.Lookup(nameof(MapToParentTopicViewModel)));
      Assert.AreEqual(typeof(Object), compositeLookup.Lookup(nameof(Topic)));

    }

  } //Class
} //Namespace