/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Lookup;
using OnTopic.Metadata;
using OnTopic.Tests.BindingModels;
using OnTopic.Tests.Entities;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE LOOKUP SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ITypeLookupService"/> interface and its implementations, such as the <see cref="
  ///   StaticTypeLookupService"/>, <see cref="DynamicTypeLookupService"/>, <see cref="DynamicTopicBindingModelLookupService"/>,
  ///   and the underlying <see cref="TypeCollection"/>.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class TypeLookupServiceTest {

    /*==========================================================================================================================
    | TEST: TYPE COLLECTION: CONSTRUCTOR: CONTAINS UNIQUE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="TypeCollection"/> with a list of <see cref="Type"/> objects, including a duplicate.
    ///   Confirms that only the unique values are provided.
    /// </summary>
    [TestMethod]
    public void TypeCollection_Constructor_ContainsUniqueTypes() {

      var topics = new List<Type> {
        typeof(BasicTopicBindingModel),
        typeof(CustomTopic),
        typeof(CustomTopic)
      };
      var typeCollection        = new TypeCollection(topics);

      Assert.AreEqual<int>(2, typeCollection.Count);
      Assert.IsTrue(typeCollection.Contains(typeof(CustomTopic)));
      Assert.IsFalse(typeCollection.Contains(typeof(Topic)));

    }

    /*==========================================================================================================================
    | TEST: STATIC TYPE LOOKUP SERVICE: TRY ADD: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="StaticTypeLookupService"/> and calls <see cref="StaticTypeLookupService.TryAdd(Type)"/> to
    ///   ensure it correctly adds new items, but not duplicate items.
    /// </summary>
    [TestMethod]
    public void StaticLookupService_TryAdd_ReturnsExpected() {

      var topics = new List<Type> {
        typeof(CustomTopic)
      };
      var lookupService         = new DummyStaticTypeLookupService(topics);

      Assert.IsFalse(lookupService.TryAdd(typeof(CustomTopic)));
      Assert.IsTrue(lookupService.TryAdd(typeof(Topic)));

    }

    /*==========================================================================================================================
    | TEST: DEFAULT TOPIC LOOKUP SERVICE: LOOKUP: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the <see cref="DefaultTopicLookupService"/> to ensure it correctly identifies not only the built-in models, but
    ///   also an additional model injected via the constructor.
    /// </summary>
    [TestMethod]
    public void DefaultTopicLookupService_Lookup_ReturnsExpected() {

      var topics = new List<Type> {
        typeof(CustomTopic)
      };
      var lookupService         = new DefaultTopicLookupService(topics);

      Assert.AreEqual<Type?>(typeof(AttributeDescriptor), lookupService.Lookup(nameof(AttributeDescriptor)));
      Assert.AreEqual<Type?>(typeof(CustomTopic), lookupService.Lookup(nameof(CustomTopic)));
      Assert.IsNull(lookupService.Lookup("TextAttributeDescriptor"));

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TOPIC BINDING MODEL LOOKUP SERVICE: LOOKUP: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the <see cref="DynamicTopicBindingModelLookupService"/> to ensure it correctly identifies binding models that
    ///   are defined as part of the test project.
    /// </summary>
    [TestMethod]
    public void DynamicTopicBindingModelLookupService_Lookup_ReturnsExpected() {

      var lookupService         = new DynamicTopicBindingModelLookupService();

      Assert.AreEqual<Type?>(typeof(PageTopicBindingModel), lookupService.Lookup(nameof(PageTopicBindingModel)));
      Assert.IsNull(lookupService.Lookup("MissingTopicBindingModel"));

    }

  } //Class
} //Namespace