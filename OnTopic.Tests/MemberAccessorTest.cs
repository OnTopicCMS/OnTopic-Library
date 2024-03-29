﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Reflection;
using OnTopic.Internal.Reflection;
using OnTopic.Tests.ViewModels;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: MEMBER ACCESSOR TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides basic unit tests for the <see cref="MemberAccessor"/> class.
  /// </summary>
  /// <remarks>
  ///   Most access to the <see cref="MemberAccessor"/> class will be through higher-level classes which provide type validation
  ///   and conversion. As a result, the unit tests for the <see cref="MemberAccessor"/> will focus on the more fundamental
  ///   behavior of validating and translating a <see cref="MemberInfo"/> object into a <see cref="MemberAccessor"/>.
  /// </remarks>
  [ExcludeFromCodeCoverage]
  public class MemberAccessorTest {

    /*==========================================================================================================================
    | TEST: IS NULLABLE: NULLABLE PROPERTY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is nullable, and confirms that the
    ///   <see cref="ItemMetadata.IsNullable"/> property is set to <c>true</c>.
    /// </summary>
    [Fact]
    public void IsNullable_NullableProperty_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NullableProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.True(memberAccessor.IsNullable);

    }

    /*==========================================================================================================================
    | TEST: IS NULLABLE: NON-NULLABLE PROPERTY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is not nullable, and confirms that
    ///   the <see cref="ItemMetadata.IsNullable"/> property is set to <c>false</c>.
    /// </summary>
    [Fact]
    public void IsNullable_NonNullableProperty_ReturnsFalse() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NonNullableProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.False(memberAccessor.IsNullable);

    }

    /*==========================================================================================================================
    | TEST: IS NULLABLE: NON-NULLABLE REFERENCE PROPERTY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is a non-nullable reference type,
    ///   and confirms that the <see cref="ItemMetadata.IsNullable"/> property is set to <c>true</c>.
    /// </summary>
    /// <remarks>
    ///   Unfortunately, the .NET reflection libraries don't (yet) have the ability to determine if a nullable reference types
    ///   are nullable or not, and thus this should always return <c>true</c>.
    /// </remarks>
    [Fact]
    public void IsNullable_NonNullableReferenceProperty_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NonNullableReferenceGetter)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.True(memberAccessor.IsNullable);

    }

    /*==========================================================================================================================
    | TEST: IS GETTABLE: READ-ONLY PROPERTY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is read-only, and confirms that the
    ///   <see cref="MemberAccessor.CanRead"/> property is set to <c>true</c>.
    /// </summary>
    [Fact]
    public void IsGettable_ReadOnlyProperty_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.ReadOnlyProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.True(memberAccessor.CanRead);

    }

    /*==========================================================================================================================
    | TEST: IS GETTABLE: WRITE-ONLY PROPERTY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is write-only, and confirms that the
    ///   <see cref="MemberAccessor.CanRead"/> property is set to <c>false</c>.
    /// </summary>
    [Fact]
    public void IsGettable_WriteOnlyProperty_ReturnsFalse() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.WriteOnlyProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.False(memberAccessor.CanRead);

    }

    /*==========================================================================================================================
    | TEST: IS SETTABLE: WRITE-ONLY PROPERTY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is write-only, and confirms that the
    ///   <see cref="MemberAccessor.CanRead"/> property is set to <c>false</c>.
    /// </summary>
    [Fact]
    public void IsSettable_WriteOnlyProperty_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.WriteOnlyProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.True(memberAccessor.CanWrite);

    }

    /*==========================================================================================================================
    | TEST: IS SETTABLE: READ-ONLY PROPERTY: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/> that is read-only, and confirms that the
    ///   <see cref="MemberAccessor.CanWrite"/> property is set to <c>false</c>.
    /// </summary>
    [Fact]
    public void IsSettable_ReadOnlyProperty_ReturnsFalse() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.ReadOnlyProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.False(memberAccessor.CanWrite);

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: VALID PROPERTY: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.GetValue(Object)"/> with a compliant object, expecting that the correct value will be returned.
    /// </summary>
    [Fact]
    public void GetValue_ValidProperty_ReturnsValue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NonNullableProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);
      var sourceObject          = new MemberAccessorViewModel() { NonNullableProperty = 15 };
      var returnObject          = memberAccessor.GetValue(sourceObject);

      Assert.Equal(sourceObject.NonNullableProperty, returnObject);

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: VALID METHOD: RETURNS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.GetValue(Object)"/> with a compliant object, expecting that the correct value will be returned.
    /// </summary>
    [Theory]
    [InlineData(15)]
    [InlineData(null)]
    public void GetValue_ValidMethod_ReturnsValue(int? value) {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.GetMethod)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);
      var sourceObject          = new MemberAccessorViewModel();

      sourceObject.SetMethod(value);

      var returnObject          = memberAccessor.GetValue(sourceObject);

      Assert.Equal(value, returnObject);

    }

    /*==========================================================================================================================
    | TEST: GET VALUE: TYPE MISMATCH: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.GetValue(Object)"/> with an object that doesn't contain the <see cref="MemberInfo"/>, expecting that an
    ///   <see cref="ArgumentException"/> will be thrown.
    /// </summary>
    [Fact]
    public void GetValue_TypeMismatch_ThrowsException() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.ReadOnlyProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.Throws<ArgumentException>(() =>
        memberAccessor.GetValue(new object())
      );

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALID PROPERTY: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.SetValue(Object, Object?, Boolean)"/> with a compliant object, expecting that the correct value will be
    ///   set.
    /// </summary>
    [Theory]
    [InlineData(15)]
    [InlineData(null)]
    public void SetValue_ValidProperty_SetsValue(int? value) {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NullableProperty)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);
      var sourceObject          = new MemberAccessorViewModel {
        NullableProperty        = 5
      };
      memberAccessor.SetValue(sourceObject, value);

      Assert.Equal(value, sourceObject.NullableProperty);

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: VALID METHOD: SETS VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.SetValue(Object, Object?, Boolean)"/> with a compliant object, expecting that the correct value will be
    ///   set.
    /// </summary>
    [Theory]
    [InlineData(15)]
    [InlineData(null)]
    public void SetValue_ValidMethod_SetsValue(int? value) {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.SetMethod)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);
      var sourceObject          = new MemberAccessorViewModel();

      sourceObject.SetMethod(5);
      memberAccessor.SetValue(sourceObject, value);

      Assert.Equal(value, sourceObject.GetMethod());

    }

    /*==========================================================================================================================
    | TEST: SET VALUE: MEMBER TYPE MISMATCH: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assembles a new <see cref="MemberAccessor"/> from a <see cref="MemberInfo"/>, and attempts to call <see cref="
    ///   MemberAccessor.SetValue(Object, Object?, Boolean)"/> with an object that isn't compatible with the <see cref="
    ///   MemberInfo.MemberType"/>, expecting that an <see cref="InvalidCastException"/> will be thrown.
    /// </summary>
    [Fact]
    public void SetValue_MemberTypeMismatch_ThrowsException() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMember(nameof(MemberAccessorViewModel.NonNullableReferenceGetter)).FirstOrDefault()!;
      var memberAccessor        = new MemberAccessor(memberInfo);

      Assert.Throws<InvalidCastException>(() =>
        memberAccessor.SetValue(new MemberAccessorViewModel(), new object())
      );

    }

    /*==========================================================================================================================
    | TEST: IS VALID: PROPERTY: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is a <see cref="PropertyInfo"/> and confirms that it is accepted.
    /// </summary>
    [Fact]
    public void IsValid_Property_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetProperty(nameof(MemberAccessorViewModel.NonNullableProperty))!;

      Assert.True(MemberAccessor.IsValid(memberInfo));

    }

    /*==========================================================================================================================
    | TEST: IS VALID: GETTER METHOD: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is a <see cref="MethodInfo"/> with a return type and no parameters and
    ///   confirms that it is accepted.
    /// </summary>
    [Fact]
    public void IsValid_GetterMethod_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMethod(nameof(MemberAccessorViewModel.GetMethod))!;

      Assert.NotNull(memberInfo);
      Assert.True(MemberAccessor.IsValid(memberInfo));

    }

    /*==========================================================================================================================
    | TEST: IS VALID: INVALID GETTER METHOD: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is a <see cref="MethodInfo"/> with a parameter and confirms that it is
    ///   rejected.
    /// </summary>
    /// <remarks>
    ///   The <see cref="MemberAccessor"/> is only designed to be able to get methods that have a return type and no parameters.
    /// </remarks>
    [Fact]
    public void IsValid_InvalidGetterMethod_ReturnsFalse()
    {
      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMethod(nameof(MemberAccessorViewModel.InvalidGetMethod))!;

      Assert.NotNull(memberInfo);
      Assert.False(MemberAccessor.IsValid(memberInfo));

    }

    /*==========================================================================================================================
    | TEST: IS VALID: SETTER METHOD: RETURNS TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is a <see cref="MethodInfo"/> with no return type and one parameter and
    ///   confirms that it is accepted.
    /// </summary>
    [Fact]
    public void IsValid_SetterMethod_ReturnsTrue() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMethod(nameof(MemberAccessorViewModel.SetMethod))!;

      Assert.True(MemberAccessor.IsValid(memberInfo));

    }

    /*==========================================================================================================================
    | TEST: IS VALID: INVALID SETTER METHOD: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is a <see cref="MethodInfo"/> with no return type and two parameters and
    ///   confirms that it is rejected.
    /// </summary>
    /// <remarks>
    ///   The <see cref="MemberAccessor"/> is only designed to be able to set methods that are void and accept a single
    ///   parameter.
    /// </remarks>
    [Fact]
    public void IsValid_InvalidSetterMethod_ReturnsFalse()
    {
      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetMethod(nameof(MemberAccessorViewModel.InvalidSetMethod))!;

      Assert.NotNull(memberInfo);
      Assert.False(MemberAccessor.IsValid(memberInfo));

    }

    /*==========================================================================================================================
    | TEST: IS VALID: CONSTRUCTOR: RETURNS FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Validates a <see cref="MemberInfo"/> that is not a <see cref="PropertyInfo"/> or a <see cref="MethodInfo"/> and
    ///   confirms that it is rejected.
    /// </summary>
    [Fact]
    public void IsValid_Constructor_ReturnsFalse() {

      var type                  = typeof(MemberAccessorViewModel);
      var memberInfo            = type.GetConstructor(Array.Empty<Type>())!;

      Assert.NotNull(memberInfo);
      Assert.False(MemberAccessor.IsValid(memberInfo));

    }

  } //Class
} //Namespace