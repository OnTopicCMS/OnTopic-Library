/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Internal.Reflection;
using OnTopic.Metadata;
using OnTopic.Tests.ViewModels;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="MemberDispatcher"/> and <see cref="MemberInfoCollection"/> classes.
  /// </summary>
  /// <remarks>
  ///   These are internal collections and not accessible publicly.
  /// </remarks>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class MemberDispatcherTest {

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   properties of the type.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidType_IdentifiesProperty() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("AttributeDescriptors")); //First class collection property
      Assert.IsFalse(properties.Contains("InvalidPropertyName")); //Invalid property

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES DERIVED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   properties derived from the base class.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidType_IdentifiesDerivedProperty() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key")); //Inherited string property

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   methods of the type.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidType_IdentifiesMethod() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsFalse(properties.Contains("IsTypeOf")); //This is a method, not a property

    }

    /*==========================================================================================================================
    | TEST: GET MEMBERS: PROPERTY INFO: RETURNS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that <see cref="MemberDispatcher.GetMembers{T}(Type)"/>
    ///   functions.
    /// </summary>
    [TestMethod]
    public void GetMembers_PropertyInfo_ReturnsProperties() {

      var types = new MemberDispatcher();

      var properties = types.GetMembers<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.IsTrue(properties.Contains("Key"));
      Assert.IsTrue(properties.Contains("AttributeDescriptors"));
      Assert.IsFalse(properties.Contains("IsTypeOf"));
      Assert.IsFalse(properties.Contains("InvalidPropertyName"));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER: PROPERTY INFO BY KEY: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that <see cref="MemberDispatcher.GetMember{T}(Type, String)"
    ///   /> correctly returns the expected properties.
    /// </summary>
    [TestMethod]
    public void GetMember_PropertyInfoByKey_ReturnsValue() {

      var types = new MemberDispatcher();

      Assert.IsNotNull(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "Key"));
      Assert.IsNotNull(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "AttributeDescriptors"));
      Assert.IsNull(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "InvalidPropertyName"));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER: METHOD INFO BY KEY: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that <see
    ///   cref="MemberDispatcher.GetMember{T}(Type, String)"/> correctly returns the expected methods.
    /// </summary>
    [TestMethod]
    public void GetMember_MethodInfoByKey_ReturnsValue() {

      var types = new MemberDispatcher();

      Assert.IsNotNull(types.GetMember<MethodInfo>(typeof(ContentTypeDescriptor), "GetWebPath"));
      Assert.IsNull(types.GetMember<MethodInfo>(typeof(ContentTypeDescriptor), "AttributeDescriptors"));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER: GENERIC TYPE MISMATCH: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that <see cref="MemberDispatcher.GetMember{T}(Type, String)
    ///   "/> does not return values if the types mismatch.
    /// </summary>
    [TestMethod]
    public void GetMember_GenericTypeMismatch_ReturnsNull() {

      var types = new MemberDispatcher();

      Assert.IsNull(types.GetMember<PropertyInfo>(typeof(ContentTypeDescriptor), "IsTypeOf"));
      Assert.IsNull(types.GetMember<MethodInfo>(typeof(ContentTypeDescriptor), "AttributeDescriptors"));

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: KEY: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that a key value can be properly set using the <see cref="
    ///   MemberDispatcher.SetPropertyValue(Object, String, Object?)"/> method.
    /// </summary>
    [TestMethod]
    public void SetPropertyValue_Key_SetsValue() {

      var types                 = new MemberDispatcher();
      var topic                 = TopicFactory.Create("Test", "ContentType");

      var isKeySet              = types.SetPropertyValue(topic, "Key", "NewKey");

      var key                   = types.GetPropertyValue(topic, "Key", typeof(string))?.ToString();

      Assert.IsTrue(isKeySet);
      Assert.AreEqual<string>("NewKey", topic.Key);
      Assert.AreEqual<string?>("NewKey", key);

    }


    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: BOOLEAN: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that a boolean value can be properly set using the <see cref
    ///   ="MemberDispatcher.SetPropertyValue(Object, String, Object?)"/> method.
    /// </summary>
    [TestMethod]
    public void SetPropertyValue_Boolean_SetsValue() {

      var types                 = new MemberDispatcher();
      var topic                 = TopicFactory.Create("Test", "ContentType");

      types.SetPropertyValue(topic, "IsHidden", "1");

      Assert.IsTrue(topic.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: DATE/TIME: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that a date/time value can be properly set using the <see
    ///   cref="MemberDispatcher.SetPropertyValue(Object, String, Object?)"/> method.
    /// </summary>
    [TestMethod]
    public void SetPropertyValue_DateTime_SetsValue() {

      var types                 = new MemberDispatcher();
      var topic                 = TopicFactory.Create("Test", "ContentType");

      var isDateSet             = types.SetPropertyValue(topic, "LastModified", "June 3, 2008");
          isDateSet             = types.SetPropertyValue(topic, "LastModified", "2008-06-03") && isDateSet;
          isDateSet             = types.SetPropertyValue(topic, "LastModified", "06/03/2008") && isDateSet;

      var lastModified          = DateTime.Parse(
        types.GetPropertyValue(topic, "LastModified", typeof(DateTime))?.ToString()?? "",
        CultureInfo.InvariantCulture
      );

      Assert.IsTrue(isDateSet);
      Assert.AreEqual<DateTime>(new(2008, 6, 3), topic.LastModified);
      Assert.AreEqual<DateTime>(new(2008, 6, 3), lastModified);

    }


    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: INVALID PROPERTY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that an invalid property being set via the <see cref="
    ///   MemberDispatcher.SetPropertyValue(Object, String, Object?)"/> method returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetPropertyValue_InvalidProperty_ReturnsFalse() {

      var types                 = new MemberDispatcher();
      var topic                 = TopicFactory.Create("Test", "ContentType");

      var isInvalidPropertySet  = types.SetPropertyValue(topic, "InvalidProperty", "Invalid");

      Assert.IsFalse(isInvalidPropertySet);

    }

    /*==========================================================================================================================
    | TEST: SET METHOD: VALID VALUE: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that a value can be properly set using the <see cref="
    ///   MemberDispatcher.SetMethodValue(Object, String, String)"/> method.
    /// </summary>
    [TestMethod]
    public void SetMethod_ValidValue_SetsValue() {

      var types                 = new MemberDispatcher();
      var source                = new MethodBasedViewModel();

      var isValueSet            = types.SetMethodValue(source, "SetMethod", "123");
      var value                 = types.GetMethodValue(source, "GetMethod")?? 0;

      Assert.IsTrue(isValueSet);
      Assert.IsTrue(value is int);
      Assert.AreEqual<int>(123, (int)value);

    }

    /*==========================================================================================================================
    | TEST: SET METHOD: INVALID VALUE: DOESN'T SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that a value set with an invalid value using the <see cref="
    ///   MemberDispatcher.SetMethodValue(Object, String, String)"/> method returns an exception.
    /// </summary>
    [TestMethod]
    public void SetMethod_InvalidValue_DoesNotSetValue() {

      var types                 = new MemberDispatcher();
      var source                = new MethodBasedViewModel();

      var isValueSet            = types.SetMethodValue(source, "SetMethod", "ABC");
      var value                 = types.GetMethodValue(source, "GetMethod")?? 0;

      Assert.IsFalse(isValueSet);
      Assert.IsTrue(value is int);
      Assert.AreEqual<int>(0, (int)value);

    }


    /*==========================================================================================================================
    | TEST: SET METHOD: INVALID MEMBER: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberDispatcher"/> and confirms that setting an invalid property name using the <see cref="
    ///   MemberDispatcher.SetMethodValue(Object, String, String)"/> method returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void SetMethod_Integer_SetsValue() {

      var types                 = new MemberDispatcher();
      var source                = new MethodBasedViewModel();

      var isInvalidSet          = types.SetMethodValue(source, "BogusMethod", "123");

      Assert.IsFalse(isInvalidSet);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: REFLECTION PERFORMANCE
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
    public void SetPropertyValue_ReflectionPerformance() {

      var totalIterations = 1;
      var types = new MemberDispatcher();
      var topic = TopicFactory.Create("Test", "ContentType");

      int i;
      for (i = 0; i < totalIterations; i++) {
        types.SetPropertyValue(topic, "Key", "Key" + i);
      }

      Assert.AreEqual<string>("Key" + (i-1), topic.Key);

    }

  } //Class
} //Namespace