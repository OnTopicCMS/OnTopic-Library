/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Internal.Diagnostics;
using OnTopic.Lookup;
using OnTopic.Tests.BindingModels;

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