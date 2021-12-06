/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using OnTopic.Attributes;
using OnTopic.Internal.Reflection;
using OnTopic.Metadata;
using OnTopic.Tests.BindingModels;
using OnTopic.Tests.ViewModels;
using OnTopic.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR EXTENSIONS TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TypeAccessorExtensions"/> and <see cref="MemberInfoCollection"/> classes.
  /// </summary>
  /// <remarks>
  ///   These are internal collections and not accessible publicly.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class TypeAccessorExtensionsTest {

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   properties of the type.
    /// </summary>
    [Fact]
    public void Constructor_ValidType_IdentifiesProperty() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.True(properties.Contains("AttributeDescriptors")); //First class collection property
      Assert.False(properties.Contains("InvalidPropertyName")); //Invalid property

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES DERIVED PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   properties derived from the base class.
    /// </summary>
    [Fact]
    public void Constructor_ValidType_IdentifiesDerivedProperty() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.True(properties.Contains("Key")); //Inherited string property

    }

    /*==========================================================================================================================
    | TEST: CONSTRUCTOR: VALID TYPE: IDENTIFIES METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="MemberInfoCollection"/> based on a type, and confirms that the property collection returns
    ///   methods of the type.
    /// </summary>
    [Fact]
    public void Constructor_ValidType_IdentifiesMethod() {

      var properties = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor));

      Assert.False(properties.Contains("IsTypeOf")); //This is a method, not a property

    }

    /*==========================================================================================================================
    | TEST: ADD: DUPLICATE KEY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeMemberInfoCollection"/> and confirms that <see cref="TypeMemberInfoCollection.InsertItem(
    ///   Int32, MemberInfoCollection)"/> throws an exception if a <see cref="MemberInfoCollection"/> with a duplicate <see cref
    ///   ="MemberInfoCollection{T}.Type"/> already exists.
    ///   functions.
    /// </summary>
    [Fact]
    public void Add_DuplicateKey_ThrowsException() =>
      Assert.Throws<ArgumentException>(() =>
        new TypeMemberInfoCollection {
          new(typeof(EmptyViewModel)),
          new(typeof(EmptyViewModel))
        }
      );

    /*==========================================================================================================================
    | TEST: HAS MEMBER: PROPERTY INFO: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeMemberInfoCollection"/> and confirms that <see cref="TypeMemberInfoCollection.HasMember{T
    ///   }(Type, String)"/> returns <c>true</c> when appropriate.
    ///   functions.
    /// </summary>
    [Fact]
    public void HasMember_PropertyInfo_ReturnsExpected() {

      var typeCollection        = new TypeMemberInfoCollection();

      Assert.True(typeCollection.HasMember(typeof(ContentTypeDescriptorTopicBindingModel), "ContentTypes"));
      Assert.False(typeCollection.HasMember(typeof(ContentTypeDescriptorTopicBindingModel), "MissingProperty"));
      Assert.False(typeCollection.HasMember<MethodInfo>(typeof(ContentTypeDescriptorTopicBindingModel), "Attributes"));

    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE PROPERTY: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessorExtensions.HasGettableProperty(
    ///   TypeAccessor, String, Type)"/> returns the expected value.
    ///   functions.
    /// </summary>
    [Fact]
    public void HasGettableProperty_ReturnsExpected() {

      var typeAccessor1         = TypeAccessorCache.GetTypeAccessor<ContentTypeDescriptorTopicBindingModel>();
      var typeAccessor2         = TypeAccessorCache.GetTypeAccessor<TopicReferenceTopicViewModel>();

      Assert.True(typeAccessor1.HasGettableProperty(nameof(ContentTypeDescriptorTopicBindingModel.Key)));
      Assert.False(typeAccessor1.HasGettableProperty(nameof(ContentTypeDescriptorTopicBindingModel.ContentTypes)));
      Assert.False(typeAccessor1.HasGettableProperty("MissingProperty"));
      Assert.True(typeAccessor2.HasGettableProperty(nameof(TopicReferenceTopicViewModel.TopicReference), typeof(TopicViewModel)));

    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE PROPERTY: WITH ATTRIBUTE: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> with a required <see cref="Attribute"/> constraint and confirms that <see
    ///   cref="TypeAccessorExtensions.HasGettableProperty(TypeAccessor, String, Type)"/> returns the expected value.
    /// </summary>
    [Fact]
    public void HasGettableProperty_WithAttribute_ReturnsExpected() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();

      Assert.False(typeAccessor.HasGettableProperty<AttributeSetterAttribute>(nameof(Topic.Key)));
      Assert.True(typeAccessor.HasGettableProperty<AttributeSetterAttribute>(nameof(Topic.View)));

    }

    /*==========================================================================================================================
    | TEST: HAS SETTABLE PROPERTY: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessorExtensions.HasSettableProperty(
    ///   TypeAccessor, String, Type)"/> returns the expected value.
    ///   functions.
    /// </summary>
    [Fact]
    public void HasSettableProperty_ReturnsExpected() {

      var typeAccessor1         = TypeAccessorCache.GetTypeAccessor<ContentTypeDescriptorTopicBindingModel>();
      var typeAccessor2         = TypeAccessorCache.GetTypeAccessor<TopicReferenceTopicViewModel>();

      Assert.True(typeAccessor1.HasSettableProperty(nameof(ContentTypeDescriptorTopicBindingModel.Key)));
      Assert.False(typeAccessor1.HasSettableProperty(nameof(ContentTypeDescriptorTopicBindingModel.ContentTypes)));
      Assert.False(typeAccessor1.HasSettableProperty("MissingProperty"));
      Assert.True(typeAccessor2.HasSettableProperty(nameof(TopicReferenceTopicViewModel.TopicReference), typeof(TopicViewModel)));

    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE METHOD: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessorExtensions.HasGettableMethod(
    ///   TypeAccessor, String, Type)"/> returns the expected value.
    /// </summary>
    [Fact]
    public void HasGettableMethod_ReturnsExpected() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();

      Assert.True(typeAccessor.HasGettableMethod("GetMethod"));
      Assert.False(typeAccessor.HasGettableMethod("SetMethod"));
      Assert.False(typeAccessor.HasGettableMethod("MissingMethod"));
      Assert.False(typeAccessor.HasGettableMethod("GetComplexMethod"));
      Assert.True(typeAccessor.HasGettableMethod("GetComplexMethod", typeof(TopicViewModel)));

    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE METHOD: WITH ATTRIBUTE: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> with a required <see cref="Attribute"/> constraint and confirms that <see
    ///   cref="TypeAccessorExtensions.HasGettableMethod(TypeAccessor, String, Type)"/> returns the expected value.
    /// </summary>
    /// <remarks>
    ///   In practice, we haven't encountered a need for this yet and, thus, don't have any semantically relevant attributes to
    ///   use in this situation. As a result, this example is a bit contrived.
    /// </remarks>
    [Fact]
    public void HasGettableMethod_WithAttribute_ReturnsExpected() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();

      Assert.True(typeAccessor.HasGettableMethod<DisplayNameAttribute>(nameof(MethodBasedViewModel.GetAnnotatedMethod)));
      Assert.False(typeAccessor.HasGettableMethod<DisplayNameAttribute>(nameof(MethodBasedViewModel.GetMethod)));

    }

    /*==========================================================================================================================
    | TEST: HAS SETTABLE METHOD: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessorExtensions.HasSettableMethod(
    ///   TypeAccessor, String, Type)"/> returns the expected value.
    /// </summary>
    [Fact]
    public void HasSettableMethod_ReturnsExpected() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();

      Assert.True(typeAccessor.HasSettableMethod(nameof(MethodBasedViewModel.SetMethod)));
      Assert.False(typeAccessor.HasSettableMethod(nameof(MethodBasedViewModel.GetMethod)));
      Assert.False(typeAccessor.HasSettableMethod(nameof(MethodBasedViewModel.SetComplexMethod)));
      Assert.False(typeAccessor.HasSettableMethod(nameof(MethodBasedViewModel.SetParametersMethod)));
      Assert.False(typeAccessor.HasSettableMethod("MissingMethod"));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBERS: PROPERTY: RETURNS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.GetMembers(MemberTypes)"/>
    ///   functions.
    /// </summary>
    [Fact]
    public void GetMembers_Property_ReturnsProperties() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<ContentTypeDescriptor>();
      var members               = typeAccessor.GetMembers(MemberTypes.Property).Select(m => m.MemberInfo).Cast<PropertyInfo>();
      var properties            = new MemberInfoCollection<PropertyInfo>(typeof(ContentTypeDescriptor), members);

      Assert.True(properties.Contains("Key"));
      Assert.True(properties.Contains("AttributeDescriptors"));
      Assert.False(properties.Contains("IsTypeOf"));
      Assert.False(properties.Contains("InvalidPropertyName"));

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: KEY: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a key value can be properly set using the <see cref="
    ///   TypeAccessorExtensions.SetPropertyValue(TypeAccessor, Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetPropertyValue_Key_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();
      var topic                 = new Topic("Test", "ContentType");

      typeAccessor.SetPropertyValue(topic, "Key", "NewKey");

      var key                   = typeAccessor.GetPropertyValue(topic, "Key")?.ToString();

      Assert.Equal("NewKey", topic.Key);
      Assert.Equal("NewKey", key);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: NULL VALUE: SETS TO NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessorExtensions.SetPropertyValue(
    ///   TypeAccessor, Object, String, Object?, Boolean)"/> method sets the property to <c>null</c>.
    /// </summary>
    [Fact]
    public void SetPropertyValue_NullValue_SetsToNull() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<NullablePropertyTopicViewModel>();
      var model                 = new NullablePropertyTopicViewModel() {
        NullableInteger         = 5
      };

      typeAccessor.SetPropertyValue(model, "NullableInteger", null);

      Assert.Null(model.NullableInteger);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: EMPTY VALUE: SETS TO NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessorExtensions.SetPropertyValue(
    ///   TypeAccessor, Object, String, Object?, Boolean)"/> sets the target property value to <c>null</c> if the value is set
    ///   to <see cref="String.Empty"/>.
    /// </summary>
    [Fact]
    public void SetPropertyValue_EmptyValue_SetsToNull() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<NullablePropertyTopicViewModel>();
      var model                 = new NullablePropertyTopicViewModel() {
        NullableInteger         = 5
      };

      typeAccessor.SetPropertyValue(model, "NullableInteger", "", true);

      Assert.Null(model.NullableInteger);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: EMPTY VALUE: SETS DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessorExtensions.SetPropertyValue(
    ///   TypeAccessor, Object, String, Object?, Boolean)"/> sets the value to its default if the value is set to <see cref="
    ///   String.Empty"/> and the target property type is not nullable.
    /// </summary>
    [Fact]
    public void SetPropertyValue_EmptyValue_SetsDefault() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<NonNullablePropertyTopicViewModel>();
      var model                 = new NonNullablePropertyTopicViewModel();

      typeAccessor.SetPropertyValue(model, "NonNullableInteger", "ABC", true);

      Assert.Equal<int>(0, model.NonNullableInteger);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: BOOLEAN: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a boolean value can be properly set using the <see cref
    ///   ="TypeAccessorExtensions.SetPropertyValue(TypeAccessor, Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetPropertyValue_Boolean_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();
      var topic                 = new Topic("Test", "ContentType");

      typeAccessor.SetPropertyValue(topic, "IsHidden", "1", true);

      Assert.True(topic.IsHidden);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: DATE/TIME: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a date/time value can be properly set using the <see
    ///   cref="TypeAccessorExtensions.SetPropertyValue(TypeAccessor, Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetPropertyValue_DateTime_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();
      var topic                 = new Topic("Test", "ContentType");

      typeAccessor.SetPropertyValue(topic, "LastModified", "June 3, 2008", true);
      Assert.Equal<DateTime>(new(2008, 6, 3), topic.LastModified);

      typeAccessor.SetPropertyValue(topic, "LastModified", "2008-06-03", true);
      Assert.Equal<DateTime>(new(2008, 6, 3), topic.LastModified);

      typeAccessor.SetPropertyValue(topic, "LastModified", "06/03/2008", true);
      Assert.Equal<DateTime>(new(2008, 6, 3), topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: INVALID PROPERTY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that an invalid property being set via the <see cref="
    ///   TypeAccessorExtensions.SetPropertyValue(TypeAccessor, Object, String, Object?, Boolean)"/> method throws an <see cref=
    ///   "InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void SetPropertyValue_InvalidProperty_ReturnsFalse() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(typeof(Topic));
      var topic                 = new Topic("Test", "ContentType");

      Assert.Throws<InvalidOperationException>(() =>
        typeAccessor.SetPropertyValue(topic, "InvalidProperty", "Invalid")
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: VALID VALUE: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value can be properly set using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetMethodValue_ValidValue_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();
      var source                = new MethodBasedViewModel();

      typeAccessor.SetMethodValue(source, "SetMethod", "123", true);

      Assert.Equal<int>(123, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID VALUE: DOESN'T SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value set with an invalid value using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> method returns <c>false</c>.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidValue_DoesNotSetValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();
      var source                = new MethodBasedViewModel();

      typeAccessor.SetMethodValue(source, "SetMethod", "ABC", true);

      Assert.Equal<int>(0, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID MEMBER: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that setting an invalid method name using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> method throws an exception.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidMember_ThrowsException() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();
      var source                = new MethodBasedViewModel();

      Assert.Throws<InvalidOperationException>(() =>
        typeAccessor.SetMethodValue(source, "BogusMethod", "123")
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: VALID REFENCE VALUE: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a reference value can be properly set using the <see
    ///   cref="TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetMethodValue_ValidReferenceValue_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedReferenceViewModel>();
      var source                = new MethodBasedReferenceViewModel();
      var reference             = new TopicViewModel();

      typeAccessor.SetMethodValue(source, "SetMethod", reference);

      Assert.Equal<TopicViewModel?>(reference, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID REFERENCE VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value set with an invalid value using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object, Boolean)"/> method throws an <see cref="
    ///   InvalidCastException"/>.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidReferenceValue_ThrowsException() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedReferenceViewModel>();
      var source                = new MethodBasedReferenceViewModel();
      var reference             = new EmptyViewModel();

      Assert.Throws<InvalidCastException>(() =>
        typeAccessor.SetMethodValue(source, "SetMethod", reference)
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID REFERENCE MEMBER: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that setting an invalid method name using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> method returns <c>false</c>.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidReferenceMember_ThrowsException() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();
      var source                = new MethodBasedViewModel();

      Assert.Throws<InvalidOperationException>(() =>
        typeAccessor.SetMethodValue(source, "BogusMethod", new object())
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: NULL REFERENCE VALUE: DOESN'T SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value set with an null value using the <see cref="
    ///   TypeAccessorExtensions.SetMethodValue(TypeAccessor, Object, String, Object, Boolean)"/> method returns <c>false</c>.
    /// </summary>
    [Fact]
    public void SetMethodValue_NullReferenceValue_DoesNotSetValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedReferenceViewModel>();
      var source                = new MethodBasedReferenceViewModel();

      typeAccessor.SetMethodValue(source, "SetMethod", (object?)null);

      Assert.Null(source.GetMethod());

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
    [Fact]
    public void SetPropertyValue_ReflectionPerformance() {

      var totalIterations       = 1;
      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();
      var topic                 = new Topic("Test", "ContentType");

      int i;
      for (i = 0; i < totalIterations; i++) {
        typeAccessor.SetPropertyValue(topic, "Key", "Key" + i);
      }

      Assert.Equal("Key" + (i-1), topic.Key);

    }

  } //Class
} //Namespace