/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Reflection;
using OnTopic.Tests.Fixtures;
using OnTopic.Tests.ViewModels;
using Xunit;

namespace OnTopic.Tests
{

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides basic unit tests for the <see cref="TypeAccessor"/> class.
  /// </summary>
  /// <remarks>
  ///   Most access to the <see cref="TypeAccessor"/> class will be through higher-level classes which provide type validation
  ///   and conversion. As a result, the unit tests for the <see cref="TypeAccessor"/> will focus on the more fundamental
  ///   behavior of validating and translating a <see cref="Type"/> object into a <see cref="TypeAccessor"/>.
  /// </remarks>
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

      Assert.Equal<int>(7+4, members.Count); // Includes four compliant members inherited from object

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

      Assert.Equal<int>(5, memberAccessors.Count);

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
      Assert.Equal<bool>(result, _typeAccessor.HasGetter(name));
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
      Assert.Equal<bool>(result, _typeAccessor.HasSetter(name));
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

      var sourceObject = new MemberAccessorViewModel()
      {
        NonNullableProperty     = 1,
        NullableProperty        = 2,
        NonNullableReferenceGetter = typeof(MemberAccessorTest)
      };

      sourceObject.SetMethod(3);

      Assert.Equal<object?>(result, _typeAccessor.GetValue(sourceObject, name));
    }

    /*==========================================================================================================================
    | TEST: SET VALUE: NAMES: SETS RESULTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a variety of values on a <see cref="MemberAccessorViewModel"/> using <see cref="TypeAccessor.SetValue(Object,
    ///   String, Object?)"/> and ensures that they are, in fact, set correctly.
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
      Assert.Equal<int>(1, sourceObject.NonNullableProperty);
      Assert.Equal<Type>(typeof(MemberAccessorTest), sourceObject.NonNullableReferenceGetter);
      Assert.Equal<int?>(5, sourceObject.GetMethod());

    }

  } //Class
} //Namespace