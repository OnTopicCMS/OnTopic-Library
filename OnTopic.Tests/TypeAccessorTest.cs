/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel;
using System.Reflection;
using OnTopic.Attributes;
using OnTopic.Internal.Reflection;
using OnTopic.Metadata;
using OnTopic.Tests.BindingModels;
using OnTopic.Tests.Fixtures;
using OnTopic.Tests.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TypeAccessor"/> classes.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class TypeAccessorTest: IClassFixture<TypeAccessorFixture<MemberAccessorViewModel>> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    internal readonly           TypeAccessor                    _typeAccessor;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TypeAccessorTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This establishes a shared <see cref="TypeAccessor"/> based on the <see cref="MemberAccessorViewModel"/> so that
    ///   members can be processed once, and then evaluated from a single instance.
    /// </remarks>
    public TypeAccessorTest(TypeAccessorFixture<MemberAccessorViewModel> fixture) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(fixture, nameof(fixture));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _typeAccessor             = fixture.TypeAccessor;

    }

    /*==========================================================================================================================
    | TEST: GET MEMBERS: MIXED VALIDITY: RETURNS VALID MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TypeAccessor"/> from a <see cref="Type"/> that contains both valid and invalid members.
    ///   Ensures that only the valid members are returned via <see cref="TypeAccessor.GetMembers(MemberTypes)"/>.
    /// </summary>
    [Fact]
    public void GetMembers_MixedValidity_ReturnsValidMembers() {

      var members               = _typeAccessor.GetMembers();

      Assert.Equal(7+4, members.Count); // Includes four compliant members inherited from object

    }

    /*==========================================================================================================================
    | TEST: GET MEMBERS: PROPERTIES: RETURNS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="TypeAccessor"/> from a <see cref="Type"/> that contains both methods and properties.
    ///   Ensures that only the properties are returned when <see cref="TypeAccessor.GetMembers(MemberTypes)"/> is called with
    ///   <see cref="MemberTypes.Property"/>.
    /// </summary>
    [Fact]
    public void GetMembers_Properties_ReturnsProperties() {

      var memberAccessors       = _typeAccessor.GetMembers(MemberTypes.Property);

      Assert.Equal(5, memberAccessors.Count);

    }

    /*==========================================================================================================================
    | TEST: GET MEMBERS: PROPERTY INFO: RETURNS PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.GetMembers(MemberTypes)"/>
    ///   functions.
    /// </summary>
    [Fact]
    public void GetMembers_PropertyInfo_ReturnsProperties() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<ContentTypeDescriptor>();
      var properties            = typeAccessor.GetMembers<PropertyInfo>();

      Assert.Contains(properties, p => p.Name.Equals(nameof(ContentTypeDescriptor.Key), StringComparison.Ordinal));
      Assert.Contains(properties, p => p.Name.Equals(nameof(ContentTypeDescriptor.AttributeDescriptors), StringComparison.Ordinal));
      Assert.DoesNotContain(properties, p => p.Name.Equals(nameof(ContentTypeDescriptor.IsTypeOf), StringComparison.Ordinal));
      Assert.DoesNotContain(properties, p => p.Name.Equals("InvalidPropertyName", StringComparison.Ordinal));

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER: VALID MEMBER: RETURNS MEMBER ACCESSOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="TypeAccessor.GetMember(String)"/> returns the expected <see cref="MemberAccessor"/>.
    /// </summary>
    [Fact]
    public void GetMember_ValidMember_ReturnsMemberAccessor() {

      var memberAccessor        = _typeAccessor.GetMember(nameof(MemberAccessorViewModel.NonNullableProperty))!;

      Assert.NotNull(memberAccessor);
      Assert.Equal(nameof(MemberAccessorViewModel.NonNullableProperty), memberAccessor.Name);
      Assert.False(memberAccessor.IsNullable);

    }

    /*==========================================================================================================================
    | TEST: GET MEMBER: INVALID MEMBER: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="TypeAccessor.GetMember(String)"/> returns the null if an invalid member is requests.
    /// </summary>
    [Fact]
    public void GetMember_InvalidMember_ReturnsNull() {

      var memberAccessor        = _typeAccessor.GetMember(nameof(MemberAccessorViewModel.InvalidGetMethod));

      Assert.Null(memberAccessor);

    }

    /*==========================================================================================================================
    | TEST: HAS GETTER: NAMES: RETURNS RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="TypeAccessor.HasGetter(String)"/> returns <c>true</c> if the member is, in fact, gettable.
    /// </summary>
    [Theory]
    [InlineData(nameof(MemberAccessorViewModel.GetMethod), true)]
    [InlineData(nameof(MemberAccessorViewModel.InvalidGetMethod), false)]
    [InlineData(nameof(MemberAccessorViewModel.SetMethod), false)]
    [InlineData(nameof(MemberAccessorViewModel.NonNullableProperty), true)]
    [InlineData(nameof(MemberAccessorViewModel.ReadOnlyProperty), true)]
    [InlineData(nameof(MemberAccessorViewModel.WriteOnlyProperty), false)]
    [InlineData("MissingProperty", false)]
    public void HasGetter_Names_ReturnsResult(string name, bool result) {
      Assert.Equal(result, _typeAccessor.HasGetter(name));
    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE PROPERTY: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.HasGettableProperty(String, Type)"
    ///   /> returns the expected value.
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
    ///   cref="TypeAccessor.HasGettableProperty(String, Type)"/> returns the expected value.
    /// </summary>
    [Fact]
    public void HasGettableProperty_WithAttribute_ReturnsExpected() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();

      Assert.False(typeAccessor.HasGettableProperty<AttributeSetterAttribute>(nameof(Topic.Key)));
      Assert.True(typeAccessor.HasGettableProperty<AttributeSetterAttribute>(nameof(Topic.View)));

    }

    /*==========================================================================================================================
    | TEST: HAS GETTABLE METHOD: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.HasGettableMethod(String, Type)"/>
    ///   returns the expected value.
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
    ///   cref="TypeAccessor.HasGettableMethod(String, Type)"/> returns the expected value.
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
    | TEST: GET VALUE: NAMES: RETURNS RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="TypeAccessor.GetValue(Object, String)"/> returns the expected default value.
    /// </summary>
    [Theory]
    [InlineData(nameof(MemberAccessorViewModel.NonNullableProperty), 1)]
    [InlineData(nameof(MemberAccessorViewModel.NullableProperty), 2)]
    [InlineData(nameof(MemberAccessorViewModel.NonNullableReferenceGetter), typeof(MemberAccessorTest))]
    [InlineData(nameof(MemberAccessorViewModel.GetMethod), 3)]
    [InlineData(nameof(MemberAccessorViewModel.ReadOnlyProperty), null)]
    public void GetValue_Names_ReturnsResult(string name, object result) {

      var sourceObject          = new MemberAccessorViewModel() {
        NonNullableProperty     = 1,
        NullableProperty        = 2,
        NonNullableReferenceGetter = typeof(MemberAccessorTest)
      };

      sourceObject.SetMethod(3);

      Assert.Equal(result, _typeAccessor.GetValue(sourceObject, name));
    }

    /*==========================================================================================================================
    | TEST: HAS SETTER: NAMES: RETURNS RESULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that <see cref="TypeAccessor.HasSetter(String)"/> returns <c>true</c> if the member is, in fact, settable.
    /// </summary>
    [Theory]
    [InlineData(nameof(MemberAccessorViewModel.SetMethod), true)]
    [InlineData(nameof(MemberAccessorViewModel.InvalidSetMethod), false)]
    [InlineData(nameof(MemberAccessorViewModel.GetMethod), false)]
    [InlineData(nameof(MemberAccessorViewModel.NonNullableProperty), true)]
    [InlineData(nameof(MemberAccessorViewModel.WriteOnlyProperty), true)]
    [InlineData(nameof(MemberAccessorViewModel.ReadOnlyProperty), false)]
    [InlineData("MissingProperty", false)]
    public void HasSetter_Names_ReturnsResult(string name, bool result) {
      Assert.Equal(result, _typeAccessor.HasSetter(name));
    }

    /*==========================================================================================================================
    | TEST: HAS SETTABLE PROPERTY: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.HasSettableProperty(String, Type)"
    ///   /> returns the expected value.
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
    | TEST: HAS SETTABLE METHOD: RETURNS EXPECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that <see cref="TypeAccessor.HasSettableMethod(String, Type)"/>
    ///   returns the expected value.
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
    | TEST: SET VALUE: NAMES: SETS RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a variety of values on a <see cref="MemberAccessorViewModel"/> using <see cref="TypeAccessor.SetValue(Object,
    ///   String, Object?, Boolean)"/> and ensures that they are, in fact, set correctly.
    /// </summary>
    [Fact]
    public void SetValue_Names_SetsResults() {

      var sourceObject          = new MemberAccessorViewModel() {
        NullableProperty        = 5
      };

      _typeAccessor.SetValue(sourceObject, nameof(MemberAccessorViewModel.NullableProperty), null);
      _typeAccessor.SetValue(sourceObject, nameof(MemberAccessorViewModel.NonNullableProperty), 1);
      _typeAccessor.SetValue(sourceObject, nameof(MemberAccessorViewModel.NonNullableReferenceGetter), typeof(MemberAccessorTest));
      _typeAccessor.SetValue(sourceObject, nameof(MemberAccessorViewModel.SetMethod), 5);
    //_typeAccessor.SetValue(sourceObject, nameof(MemberAccessorViewModel.WriteOnlyProperty), null); // Edge case is unsupported

      Assert.Null(sourceObject.NullableProperty);
      Assert.Equal(1, sourceObject.NonNullableProperty);
      Assert.Equal(typeof(MemberAccessorTest), sourceObject.NonNullableReferenceGetter);
      Assert.Equal(5, sourceObject.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: KEY: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a key value can be properly set using the <see cref="
    ///   TypeAccessor.SetPropertyValue(Object, String, Object?, Boolean)"/> method.
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
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessor.SetPropertyValue(Object,
    ///   String, Object?, Boolean)"/> method sets the property to <c>null</c>.
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
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessor.SetPropertyValue(Object,
    ///   String, Object?, Boolean)"/> sets the target property value to <c>null</c> if the value is set to <see cref="
    ///   String.Empty"/>.
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
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that the <see cref="TypeAccessor.SetPropertyValue(Object,
    ///   String, Object?, Boolean)"/> sets the value to its default if the value is set to <see cref="String.Empty"/> and the
    ///   target property type is not nullable.
    /// </summary>
    [Fact]
    public void SetPropertyValue_EmptyValue_SetsDefault() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<NonNullablePropertyTopicViewModel>();
      var model                 = new NonNullablePropertyTopicViewModel();

      typeAccessor.SetPropertyValue(model, "NonNullableInteger", "ABC", true);

      Assert.Equal(0, model.NonNullableInteger);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: BOOLEAN: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a boolean value can be properly set using the <see cref
    ///   ="TypeAccessor.SetPropertyValue(Object, String, Object?, Boolean)"/> method.
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
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a date/time value can be properly set using the <see cref="
    ///   TypeAccessor.SetPropertyValue(Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetPropertyValue_DateTime_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<Topic>();
      var topic                 = new Topic("Test", "ContentType");

      typeAccessor.SetPropertyValue(topic, "LastModified", "June 3, 2008", true);
      Assert.Equal(new(2008, 6, 3), topic.LastModified);

      typeAccessor.SetPropertyValue(topic, "LastModified", "2008-06-03", true);
      Assert.Equal(new(2008, 6, 3), topic.LastModified);

      typeAccessor.SetPropertyValue(topic, "LastModified", "06/03/2008", true);
      Assert.Equal(new(2008, 6, 3), topic.LastModified);

    }

    /*==========================================================================================================================
    | TEST: SET PROPERTY VALUE: INVALID PROPERTY: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that an invalid property being set via the <see cref="
    ///   TypeAccessor.SetPropertyValue(Object, String, Object?, Boolean)"/> method throws an <see cref=
    ///   "InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public void SetPropertyValue_InvalidProperty_ReturnsFalse() {

      var topic                 = new MemberAccessorViewModel();

      Assert.Throws<InvalidOperationException>(() =>
        _typeAccessor.SetPropertyValue(topic, "InvalidProperty", "Invalid")
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: VALID VALUE: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value can be properly set using the <see cref="
    ///   TypeAccessor.SetMethodValue(Object, String, Object?, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetMethodValue_ValidValue_SetsValue() {

      var source                = new MemberAccessorViewModel();

      _typeAccessor.SetMethodValue(source, nameof(MemberAccessorViewModel.SetMethod), "123", true);

      Assert.Equal(123, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID VALUE: DOESN'T SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value set with an invalid value using the <see cref="
    ///   TypeAccessor.SetMethodValue(Object, String, Object?, Boolean)"/> method returns <c>false</c>.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidValue_DoesNotSetValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedViewModel>();
      var source                = new MethodBasedViewModel();

      typeAccessor.SetMethodValue(source, nameof(MethodBasedViewModel.SetMethod), "ABC", true);

      Assert.Equal(0, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID MEMBER: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that setting an invalid method name using the <see cref="
    ///   TypeAccessor.SetMethodValue(Object, String, Object?, Boolean)"/> method throws an exception.
    /// </summary>
    [Fact]
    public void SetMethodValue_InvalidMember_ThrowsException() {

      var source                = new MemberAccessorViewModel();

      Assert.Throws<InvalidOperationException>(() =>
        _typeAccessor.SetMethodValue(source, "BogusMethod", "123")
      );

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: VALID REFENCE VALUE: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a reference value can be properly set using the <see
    ///   cref="TypeAccessor.SetMethodValue(Object, String, Object, Boolean)"/> method.
    /// </summary>
    [Fact]
    public void SetMethodValue_ValidReferenceValue_SetsValue() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor<MethodBasedReferenceViewModel>();
      var source                = new MethodBasedReferenceViewModel();
      var reference             = new TopicViewModel();

      typeAccessor.SetMethodValue(source, "SetMethod", reference);

      Assert.Equal(reference, source.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET METHOD VALUE: INVALID REFERENCE VALUE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a <see cref="TypeAccessor"/> and confirms that a value set with an invalid value using the <see cref="
    ///   TypeAccessor.SetMethodValue(Object, String, Object, Boolean)"/> method throws an <see cref="
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
    ///   TypeAccessor.SetMethodValue(Object, String, Object?, Boolean)"/> method returns <c>false</c>.
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
    ///   TypeAccessor.SetMethodValue(Object, String, Object, Boolean)"/> method returns <c>false</c>.
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

    /*==========================================================================================================================
    | TEST: GET TYPE ACCESSOR: VALID TYPE: RETURNS TYPE ACCESSOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls the static <see cref="TypeAccessorCache"/> to retrieve a given <see cref="TypeAccessor"/> and ensures it is
    ///   correctly returned from the cache.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="TypeAccessorCache"/> is separate from the <see cref="TypeAccessor"/> class that this set of tests
    ///   is focused on, it is closely related, and the <see cref="TypeAccessorCache.GetTypeAccessor(Type)"/> currently only
    ///   necessitates one unit test. As a result, it's being grouped in with the <see cref="TypeAccessorTest"/>.
    /// </remarks>
    [Fact]
    public void GetTypeAccessor_ValidType_ReturnsTypeAccessor() {

      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(typeof(MemberAccessorViewModel));

      Assert.NotNull(typeAccessor);
      Assert.Equal(typeof(MemberAccessorViewModel), typeAccessor.Type);

    }

  } //Class
} //Namespace