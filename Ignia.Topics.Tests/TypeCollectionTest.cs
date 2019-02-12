/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using System.Reflection;
using Ignia.Topics.Collections;
using Ignia.Topics.Reflection;
using Ignia.Topics.Tests.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TypeCollection"/> and <see cref="MemberInfoCollection"/> classes.
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
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection is
    ///   returning expected types.
    /// </summary>
    [TestMethod]
    public void Constructor() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key")); //Inherited string property
      Assert.IsTrue(properties.Contains("AttributeDescriptors")); //First class collection property
      Assert.IsFalse(properties.Contains("IsTypeOf")); //This is a method, not a property
      Assert.IsFalse(properties.Contains("InvalidPropertyName")); //Invalid property

    }

    /*==========================================================================================================================
    | TEST: GET TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that <see cref="TypeCollection.GetMembers{T}(Type)"/>
    ///   functions.
    /// </summary>
    [TestMethod]
    public void GetProperties() {

      var types = new TypeMemberInfoCollection();

      var properties = types.GetMembers<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key"));
      Assert.IsTrue(properties.Contains("AttributeDescriptors"));
      Assert.IsFalse(properties.Contains("IsTypeOf"));
      Assert.IsFalse(properties.Contains("InvalidPropertyName"));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that <see cref="TypeCollection.GetMember(Type, String)"/>
    ///   correctly returns the expected properties.
    /// </summary>
    [TestMethod]
    public void GetMember() {

      var types = new TypeMemberInfoCollection();

      Assert.IsTrue(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "Key") != null);
      Assert.IsTrue(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "AttributeDescriptors") != null);
      Assert.IsFalse(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "IsTypeOf") != null);
      Assert.IsFalse(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "InvalidPropertyName") != null);
      Assert.IsTrue(types.GetMember<MethodInfo>(typeof(ContentTypeDescriptor), "GetWebPath") != null);
      Assert.IsFalse(types.GetMember<MethodInfo>(typeof(ContentTypeDescriptor), "AttributeDescriptors") != null);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that a value can be properly set using the
    ///   <see cref="TypeCollection.SetPropertyValue(Object, String, String)"/> method.
    /// </summary>
    [TestMethod]
    public void SetProperty() {

      var types                 = new TypeMemberInfoCollection();
      var topic                 = TopicFactory.Create("Test", "ContentType");

      types.SetPropertyValue(topic, "IsHidden", "1");

      var isDateSet             = types.SetPropertyValue(topic, "LastModified", "June 3, 2008");
          isDateSet             = types.SetPropertyValue(topic, "LastModified", "2008-06-03") && isDateSet;
          isDateSet             = types.SetPropertyValue(topic, "LastModified", "06/03/2008") && isDateSet;
      var isKeySet              = types.SetPropertyValue(topic, "Key", "NewKey");
      var isInvalidPropertySet  = types.SetPropertyValue(topic, "InvalidProperty", "Invalid");

      var lastModified          = DateTime.Parse(
        types.GetPropertyValue(topic, "LastModified", typeof(DateTime)).ToString(),
        CultureInfo.InvariantCulture
      );
      var key                   = types.GetPropertyValue(topic, "Key", typeof(string)).ToString();

      Assert.IsTrue(isDateSet);
      Assert.IsTrue(isKeySet);
      Assert.IsFalse(isInvalidPropertySet);
      Assert.AreEqual<string>("NewKey", topic.Key);
      Assert.AreEqual<string>("NewKey", key);
      Assert.AreEqual<DateTime>(new DateTime(2008, 6, 3), topic.LastModified);
      Assert.AreEqual<DateTime>(new DateTime(2008, 6, 3), lastModified);
      Assert.IsTrue(topic.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: SET METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeCollection"/> and confirms that a value can be properly set using the
    ///   <see cref="TypeCollection.SetMethodValue(Object, String, String)"/> method.
    /// </summary>
    [TestMethod]
    public void SetMethod() {

      var types                 = new TypeMemberInfoCollection();
      var source                = new MethodBasedViewModel();

      var isValueSet            = types.SetMethodValue(source, "SetMethod", "123");
      var isInvalidSet          = types.SetMethodValue(source, "BogusMethod", "123");

      var value                 = types.GetMethodValue(source, "GetMethod");

      Assert.IsTrue(isValueSet);
      Assert.IsFalse(isInvalidSet);
      Assert.IsTrue(value is int);
      Assert.AreEqual<int>(123, (int)value);

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
    public void ReflectionPerformance() {

      var totalIterations = 1;
      var types = new TypeMemberInfoCollection();
      var topic = TopicFactory.Create("Test", "ContentType");

      var i = 0;
      for (i = 0; i < totalIterations; i++) {
        types.SetPropertyValue(topic, "Key", "Key" + i);
      }

      Assert.AreEqual<string>("Key" + (i-1), topic.Key);

    }

  } //Class
} //Namespace
