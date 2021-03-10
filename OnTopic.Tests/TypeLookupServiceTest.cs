/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Internal.Diagnostics;
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