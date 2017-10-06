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

      var properties = new PropertyInfoCollection(typeof(ContentType));

      Assert.IsTrue(properties.Contains("Key"));
      Assert.IsTrue(properties.Contains("SupportedAttributes"));
      Assert.IsFalse(properties.Contains("IsTypeOf"));
      Assert.IsFalse(properties.Contains("InvalidPropertyName"));

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

      var properties = types.GetProperties(typeof(ContentType));

      Assert.IsTrue(properties.Contains("Key"));
      Assert.IsTrue(properties.Contains("SupportedAttributes"));
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

      Assert.IsTrue(types.GetProperty(typeof(ContentType), "Key") != null);
      Assert.IsTrue(types.GetProperty(typeof(ContentType), "SupportedAttributes") != null);
      Assert.IsFalse(types.GetProperty(typeof(ContentType), "IsTypeOf") != null);
      Assert.IsFalse(types.GetProperty(typeof(ContentType), "InvalidPropertyName") != null);

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
      var topic = Topic.Create("Test", "ContentType");

      types.SetProperty(topic, "Id", "555");
      types.SetProperty(topic, "Key", "NewKey");
      types.SetProperty(topic, "IsHidden", "1");
      types.SetProperty(topic, "InvalidProperty", "Invalid");

      Assert.AreEqual<int>(555, topic.Id);
      Assert.AreEqual<string>("NewKey", topic.Key);
      Assert.IsTrue(topic.IsHidden);

    }

  } //Class
} //Namespace
