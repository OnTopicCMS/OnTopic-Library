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
  | CLASS: TYPE COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TypeCollection"/> and <see cref="PropertyInfoCollection"/> classes.
  /// </summary>
  /// <remarks>
  ///   These are internal collections and not accessible publicly.
  /// </remarks>
  [TestClass]
  public class TypeCollectionTest {

    /*==========================================================================================================================
    | TEST: PROPERTY INFO COLLECTION CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="PropertyInfoCollection"/> based on a type, and confirms that the property collection is
    ///   returning expected types.
    /// </summary>
    [TestMethod]
    public void PropertyInfoCollection_ConstructorTest() {

      var properties = new PropertyInfoCollection(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key")); //Inherited string property
      Assert.IsTrue(properties.Contains("AttributeDescriptors")); //First class collection property
      Assert.IsFalse(properties.Contains("IsTypeOf")); //This is a method, not a property
      Assert.IsFalse(properties.Contains("InvalidPropertyName")); //Invalid property

    }

    /*==========================================================================================================================
    | TEST: GET TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that <see cref="TypeCollection.GetProperties(Type)"/>
    ///   functions.
    /// </summary>
    [TestMethod]
    public void TypeCollection_GetPropertiesTest() {

      var types = new TypeCollection();

      var properties = types.GetProperties(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key"));
      Assert.IsTrue(properties.Contains("AttributeDescriptors"));
      Assert.IsFalse(properties.Contains("IsTypeOf"));
      Assert.IsFalse(properties.Contains("InvalidPropertyName"));

    }

    /*==========================================================================================================================
    | TEST: GET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that <see cref="TypeCollection.GetProperty(Type, String)"/>
    ///   correctly returns the expected properties.
    /// </summary>
    [TestMethod]
    public void TypeCollection_GetPropertyTest() {

      var types = new TypeCollection();

      Assert.IsTrue(types.GetProperty(typeof(ContentTypeDescriptor), "Key") != null);
      Assert.IsTrue(types.GetProperty(typeof(ContentTypeDescriptor), "AttributeDescriptors") != null);
      Assert.IsFalse(types.GetProperty(typeof(ContentTypeDescriptor), "IsTypeOf") != null);
      Assert.IsFalse(types.GetProperty(typeof(ContentTypeDescriptor), "InvalidPropertyName") != null);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that a value can be properly set using the
    ///   <see cref="TypeCollection.SetProperty(Object, String, String)"/> method.
    /// </summary>
    [TestMethod]
    public void TypeCollection_SetPropertyTest() {

      var types = new TypeCollection();
      var topic = TopicFactory.Create("Test", "ContentType");

      types.SetProperty(topic, "IsHidden", "1");

      var isKeySet = types.SetProperty(topic, "Key", "NewKey");
      var isInvalidPropertySet = types.SetProperty(topic, "InvalidProperty", "Invalid");

      Assert.IsTrue(isKeySet);
      Assert.IsFalse(isInvalidPropertySet);
      Assert.AreEqual<string>("NewKey", topic.Key);
      Assert.IsTrue(topic.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: REFLECTION PERFORMANCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets properties via reflection n number of times; quick way of evaluating the relative performance impact of changes.
    /// </summary>
    /// <remarks>
    ///   Unit tests should run quickly, so this isn't an optimal place to test performance. As such, the counter should be set
    ///   to a small number when not being actively tested. Nevertheless, this provides a convenient test harness for quickly
    ///   evaluating the performance impact of changes or optimizations without setting up a fully performance test. To adjust
    ///   the number of iterations, simply increment the "totalIterations" variable.
    /// </remarks>
    [TestMethod]
    public void TypeCollection_ReflectionPerformanceTest() {

      var totalIterations = 1;
      var types = new TypeCollection();
      var topic = TopicFactory.Create("Test", "ContentType");

      var i = 0;
      for (i = 0; i < totalIterations; i++) {
        types.SetProperty(topic, "Key", "Key" + i);
      }

      Assert.AreEqual<string>("Key" + (i-1), topic.Key);

    }

  } //Class
} //Namespace
