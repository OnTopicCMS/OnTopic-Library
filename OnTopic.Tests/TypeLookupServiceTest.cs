/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Lookup;
using OnTopic.Metadata;
using OnTopic.Tests.BindingModels;
using OnTopic.Tests.Entities;
using OnTopic.Tests.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE LOOKUP SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="ITypeLookupService"/> interface and its implementations, such as the <see cref="
  ///   StaticTypeLookupService"/>, <see cref="DynamicTypeLookupService"/>, <see cref="DynamicTopicBindingModelLookupService"/>,
  ///   and the underlying <see cref="TypeCollection"/>.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class TypeLookupServiceTest {

    /*==========================================================================================================================
    | TEST: TYPE COLLECTION: CONSTRUCTOR: CONTAINS UNIQUE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new <see cref="TypeCollection"/> with a list of <see cref="Type"/> objects, including a duplicate.
    ///   Confirms that only the unique values are provided.
    /// </summary>
    [Fact]
    public void TypeCollection_Constructor_ContainsUniqueTypes() {

      var topics = new List<Type> {
        typeof(BasicTopicBindingModel),
        typeof(CustomTopic),
        typeof(CustomTopic)
      };
      var typeCollection        = new TypeCollection(topics);

      Assert.Equal<int>(2, typeCollection.Count);
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
    [Fact]
    public void StaticLookupService_TryAdd_ReturnsExpected() {

      var topics = new List<Type> {
        typeof(CustomTopic)
      };
      var lookupService         = new DummyStaticTypeLookupService(topics);

      Assert.IsFalse(lookupService.TryAdd(typeof(CustomTopic)));
      Assert.IsTrue(lookupService.TryAdd(typeof(Topic)));

    }

    /*==========================================================================================================================
    | TEST: STATIC TYPE LOOKUP SERVICE: LOOKUP: RETURNS FALLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="StaticTypeLookupService"/> and calls <see cref="StaticTypeLookupService.Lookup(String[])"/>
    ///   to ensure it correctly falls back to subsequent items.
    /// </summary>
    [Fact]
    public void StaticLookupService_Lookup_ReturnsFallback() {

      var topics = new List<Type> {
        typeof(AscendentTopicViewModel),
        typeof(FallbackViewModel)
      };
      var lookupService         = new StaticTypeLookupService(topics);

      Assert.Equal<Type?>(typeof(FallbackViewModel), lookupService.Lookup(nameof(EmptyViewModel), nameof(FallbackViewModel)));

    }

    /*==========================================================================================================================
    | TEST: STATIC TYPE LOOKUP SERVICE: ADD OR REPLACE: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="StaticTypeLookupService"/> and calls <see cref="StaticTypeLookupService.AddOrReplace(Type)"/>
    ///   to ensure it correctly adds new items, but and replaces duplicate items.
    /// </summary>
    [Fact]
    public void StaticLookupService_AddOrReplace_ReturnsExpected() {

      var lookupService         = new DummyStaticTypeLookupService();

      lookupService.AddOrReplace(typeof(System.Diagnostics.Contracts.Contract));
      lookupService.AddOrReplace(typeof(Internal.Diagnostics.Contract));

      Assert.Equal<Type?>(typeof(Internal.Diagnostics.Contract), lookupService.Lookup(nameof(Internal.Diagnostics.Contract)));

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TYPE LOOKUP SERVICE: PREDICATE: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="DynamicTypeLookupService"/> with a custom predicate and calls the underlying <see cref="
    ///   StaticTypeLookupService.Lookup(String[])"/> to ensure it correctly adds the expected items.
    /// </summary>
    [Fact]
    public void DynamicTypeLookupService_Predicate_ReturnsExpected() {

      var lookupService         = new DynamicTypeLookupService(t =>
        t.Namespace ==  typeof(KeyOnlyTopicViewModel).Namespace &&
        typeof(KeyOnlyTopicViewModel).IsAssignableFrom(t)
      );

      Assert.IsNotNull(lookupService.Lookup(nameof(KeyOnlyTopicViewModel)));
      Assert.IsNotNull(lookupService.Lookup(nameof(AmbiguousRelationTopicViewModel)));
      Assert.Null(lookupService.Lookup(nameof(EmptyViewModel)));

    }

    /*==========================================================================================================================
    | TEST: COMPOSITE TYPE LOOKUP SERVICE: LOOKUP: RETURNS FALLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="CompositeTypeLookupService"/> and calls <see cref="CompositeTypeLookupService.Lookup(String[]
    ///   )"/> to ensure it correctly falls back to each <see cref="ITypeLookupService"/> implementation.
    /// </summary>
    [Fact]
    public void CompositeTypeLookupService_Lookup_ReturnsFallback() {

      var lookupService1        = new StaticTypeLookupService(
        new List<Type> {
          typeof(EmptyViewModel),
          typeof(FallbackViewModel),
          typeof(Internal.Diagnostics.Contract)
        }
      );

      var lookupService2        = new StaticTypeLookupService(
        new List<Type> {
          typeof(AscendentTopicViewModel),
          typeof(FallbackViewModel),
          typeof(System.Diagnostics.Contracts.Contract)
        }
      );

      var lookupService         = new CompositeTypeLookupService(lookupService1, lookupService2);

      Assert.Equal<Type?>(typeof(System.Diagnostics.Contracts.Contract), lookupService.Lookup("Contract"));
      Assert.Equal<Type?>(typeof(FallbackViewModel), lookupService.Lookup("Missing", "FallbackViewModel"));
      Assert.Equal<Type?>(typeof(FallbackViewModel), lookupService.Lookup("Missing")?? typeof(FallbackViewModel));

    }

    /*==========================================================================================================================
    | TEST: DEFAULT TOPIC LOOKUP SERVICE: LOOKUP: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the <see cref="DefaultTopicLookupService"/> to ensure it correctly identifies not only the built-in models, but
    ///   also an additional model injected via the constructor.
    /// </summary>
    [Fact]
    public void DefaultTopicLookupService_Lookup_ReturnsExpected() {

      var topics = new List<Type> {
        typeof(CustomTopic)
      };
      var lookupService         = new DefaultTopicLookupService(topics);

      Assert.Equal<Type?>(typeof(AttributeDescriptor), lookupService.Lookup(nameof(AttributeDescriptor)));
      Assert.Equal<Type?>(typeof(CustomTopic), lookupService.Lookup(nameof(CustomTopic)));
      Assert.Null(lookupService.Lookup("TextAttributeDescriptor"));

    }

    /*==========================================================================================================================
    | TEST: DYNAMIC TOPIC BINDING MODEL LOOKUP SERVICE: LOOKUP: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the <see cref="DynamicTopicBindingModelLookupService"/> to ensure it correctly identifies binding models that
    ///   are defined as part of the test project.
    /// </summary>
    [Fact]
    public void DynamicTopicBindingModelLookupService_Lookup_ReturnsExpected() {

      var lookupService         = new DynamicTopicBindingModelLookupService();

      Assert.Equal<Type?>(typeof(PageTopicBindingModel), lookupService.Lookup(nameof(PageTopicBindingModel)));
      Assert.Null(lookupService.Lookup("MissingTopicBindingModel"));

    }

  } //Class
} //Namespace