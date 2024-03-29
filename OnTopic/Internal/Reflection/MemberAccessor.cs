﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: MEMBER ACCESSOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides metadata and accessor methods for getting and setting data, as appropriate, from properties and methods.
  /// </summary>
  /// <remarks>
  ///   This provides comparatively low-level access. It doesn't make any attempt to validate that e.g. the value submitted is
  ///   compatible with the property or method parameter type being set, nor does it make any effort to provide explicit
  ///   conversions. Those capabilities will be handled by higher-level libraries.
  /// </remarks>
  internal class MemberAccessor: ItemMetadata {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     Action<object, object?>?        _setter;
    private                     Func<object, object?>?          _getter;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberAccessor"/> class associated with a <see cref="MemberInfo"/>
    ///   instance.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> associated with the <see cref="MemberAccessor"/>.</param>
    internal MemberAccessor(MemberInfo memberInfo): base(memberInfo.Name, memberInfo) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(
        memberInfo is PropertyInfo or MethodInfo,
        $"The {nameof(memberInfo)} parameter must be of type {nameof(PropertyInfo)} or {nameof(MethodInfo)}."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Set Properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      MemberInfo                = memberInfo;
      MemberType                = MemberInfo.MemberType;
      Type                      = GetType(memberInfo);

    }

    /*==========================================================================================================================
    | MEMBER INFO
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to the underlying <see cref="MemberInfo"/> upon which the <see cref="MemberAccessor"/> is based.
    /// </summary>
    internal MemberInfo MemberInfo { get; }

    /*==========================================================================================================================
    | MEMBER TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="MemberInfo.MemberType"/>
    internal MemberTypes MemberType { get; }

    /*==========================================================================================================================
    | CAN READ?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the member can be used to retrieve a value.
    /// </summary>
    /// <remarks>
    ///   Unlike the the <see cref="PropertyInfo.CanRead"/>, the <see cref="CanRead"/> property can be set for either properties
    ///   or methods. To be readable by <see cref="GetValue(Object)"/>, a method must have a return type and no parameters.
    /// </remarks>
    internal bool CanRead { get; private set; }

    /*==========================================================================================================================
    | CAN WRITE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determine if the member can be used to set a value.
    /// </summary>
    /// <remarks>
    ///   Unlike the the <see cref="PropertyInfo.CanWrite"/>, the <see cref="CanWrite"/> property can be set for either
    ///   properties or methods. To be writable by <see cref="GetValue(Object)"/>, a method must be void and have exactly one
    ///   parameter.
    /// </remarks>
    internal bool CanWrite { get; private set; }

    /*==========================================================================================================================
    | METHOD: IS SETTABLE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether the current member is settable, either assuming the list of <see cref="AttributeValueConverter"/>,
    ///   or provided a specific <paramref name="sourceType"/>.
    /// </summary>
    /// <param name="sourceType">The <see cref="System.Type"/> to evaluate against <see cref="Type"/>.</param>
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal bool IsSettable(Type? sourceType = null, bool allowConversion = false) {
      if (sourceType is not null && (sourceType == Type || Type.IsAssignableFrom(sourceType))) {
        return true;
      }
      if (allowConversion) {
        var isSourceConvertible  = sourceType is null || AttributeValueConverter.IsConvertible(sourceType);
        return IsConvertible && isSourceConvertible;
      }
      return false;
    }

    /*==========================================================================================================================
    | GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the value of the given member from a <paramref name="source"/> object.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GetValue(object)"/> method can retrieve values from either property getters or methods which accept no
    ///   parameters and have a return type.
    /// </remarks>
    /// <param name="source">The object from which the member value should be retrieved.</param>
    /// <returns>The value of the member, if available.</returns>
    internal object? GetValue(object source) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (MemberInfo.DeclaringType != source.GetType() && !MemberInfo.DeclaringType.IsAssignableFrom(source.GetType())) {
        throw new ArgumentException(
          $"The {nameof(MemberAccessor)} for {MemberInfo.DeclaringType} cannot be used to access a member of {source.GetType()}",
          nameof(source)
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate member type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!CanRead) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Getter?.Invoke(source);

    }

    /*==========================================================================================================================
    | SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the value of the member on the <paramref name="target"/> object to the <paramref name="value"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <see cref="SetValue(Object, Object?, Boolean)"/> method can set values on either property setters or methods
    ///     which accept one parameter and don't have a return type.
    ///   </para>
    ///   <para>
    ///     The <see cref="SetValue(Object, Object?, Boolean)"/> method makes no attempt to validate whether <paramref name="
    ///     value"/> is compatible with the target property or method parameter type. If it is not compatible, the underlying
    ///     reflection library will throw an exception. It is expected that callers will validate the types before calling this
    ///     method.
    ///   </para>
    /// </remarks>
    /// <param name="target">The object on which the member should be set.</param>
    /// <param name="value">The value that the member should be set to.</param>
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal void SetValue(object target, object? value, bool allowConversion = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (MemberInfo.DeclaringType != target.GetType() && !MemberInfo.DeclaringType.IsAssignableFrom(target.GetType())) {
        throw new ArgumentException(
          $"The {nameof(MemberAccessor)} for {MemberInfo.DeclaringType} cannot be used to set a member of {target.GetType()}",
          nameof(target)
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var valueObject           = value;
      var sourceType            = value?.GetType();

      if (!CanWrite) {
        return;
      }
      else if (value is null && !IsNullable) {
        return;
      }
      else if (value is null || Type.IsAssignableFrom(sourceType)) {
        //Proceed with conversion
      }
      else if (allowConversion && value is string) {
        valueObject = AttributeValueConverter.Convert(value as string, Type);
      }

      if (valueObject is null && !IsNullable) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Setter?.Invoke(target, valueObject);

    }

    /*==========================================================================================================================
    | METHOD: VALIDATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target DTO, will automatically identify any attributes that derive from <see cref="ValidationAttribute"/> and
    ///   ensure that their conditions are satisfied.
    /// </summary>
    /// <param name="target">The target DTO to validate the current property on.</param>
    internal void Validate(object target) {
      foreach (ValidationAttribute validator in CustomAttributes.OfType<ValidationAttribute>()) {
        validator.Validate(GetValue(target), Name);
      }
    }

    /*==========================================================================================================================
    | EXCLUDED MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of member names automatically generated by the compiler for <c>record</c> types, but which aren't
    ///   relevant to mapping and should be excluded.
    /// </summary>
    private static List<string> ExcludedMembers { get; } = new List<string>() {
      "EqualityContract",
      "GetHashCode",
      "ToString",
      "<Clone>$"
    };


    /*==========================================================================================================================
    | IS VALID?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a given <see cref="MemberInfo"/> instance is compatible with the <see cref="MemberAccessor"/>.
    /// </summary>
    /// <remarks>
    ///   This is used by the <see cref="TypeAccessor"/> to validate whether a <see cref="MemberAccessor"/> should be created
    ///   for a given <see cref="MemberInfo"/>. For example, constructors, private members, and methods with more than one
    ///   parameter will return <c>false</c>.
    /// </remarks>
    internal static Func<MemberInfo, bool> IsValid => memberInfo => {

      /*------------------------------------------------------------------------------------------------------------------------
      | Exclude unsupported types and members
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (memberInfo.DeclaringType == typeof(object)) return false;
      if (memberInfo is not MethodInfo and not PropertyInfo) return false;
      if (memberInfo.Name.Contains("et_", StringComparison.Ordinal)) return false;
      if (ExcludedMembers.Contains(memberInfo.Name)) return false;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (memberInfo is PropertyInfo) return true;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish method variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var methodInfo            = (MethodInfo)memberInfo;
      var parameters            = methodInfo.GetParameters();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate get methods
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (methodInfo.ReturnType != typeof(void)) {
        return parameters.Length == 0;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate set methods
      \-----------------------------------------------------------------------------------------------------------------------*/
      return parameters.Length == 1;

    };

    /*==========================================================================================================================
    | GET TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="memberInfo"/> reference, will evaluate features of the underlying <see cref="MemberInfo"/> and
    ///   use them to set the properties of this <see cref="MemberAccessor"/> instance.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GetType(MemberInfo)"/> method is private, and intended exclusively to break up the functionality
    ///   required by the <see cref="MemberAccessor"/> constructor.
    /// </remarks>
    /// <param name="memberInfo">The source <see cref="MemberInfo"/> that this instance is based on.</param>
    /// <returns></returns>
    private Type GetType(MemberInfo memberInfo) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Set type as return type of property
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (memberInfo is PropertyInfo propertyInfo) {
        CanRead                 = propertyInfo.CanRead;
        CanWrite                = propertyInfo.CanWrite;
        return propertyInfo.PropertyType;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate method
      \-----------------------------------------------------------------------------------------------------------------------*/
      var methodInfo            = (MethodInfo)memberInfo;
      var parameters            = methodInfo.GetParameters();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set type as return type of method
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (methodInfo.ReturnType != typeof(void)) {
        Contract.Assume(
          parameters.Length == 0,
          $"The '{memberInfo.Name}()' method must not expect any parameters if the return type is not void."
        );
        CanRead = true;
        return methodInfo.ReturnType;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set type as first parameter of method
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        parameters.Length == 1,
        $"The '{memberInfo.Name}()' method must have one—and exactly one—parameter if the return type is void. This parameter " +
        $"will be used as the value of the setter."
      );

      CanWrite = true;
      return parameters[0].ParameterType;

    }

    /*==========================================================================================================================
    | GETTER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a strongly typed delegate for accessing either a property getter or a method with a return type and no
    ///   parameters.
    /// </summary>
    /// <remarks>
    ///   The value is cached alongside this instance of the <see cref="MemberAccessor"/> to improve performance across multiple
    ///   calls.
    /// </remarks>
    private Func<object, object?>? Getter {
      get {
        if (_getter is null) {
          var delegateType      = typeof(Func<,>).MakeGenericType(MemberInfo.DeclaringType, Type);
          var delegateGetter    = (Delegate?)null;
          if (MemberInfo is PropertyInfo propertInfo) {
            delegateGetter      = propertInfo.GetGetMethod().CreateDelegate(delegateType);
          }
          else if (MemberInfo is MethodInfo methodInfo) {
            delegateGetter      = methodInfo.CreateDelegate(delegateType);
          }
          var getterWithTypes   = GetterDelegateMethod.MakeGenericMethod(MemberInfo.DeclaringType, Type);
          _getter               = (Func<object, object?>)getterWithTypes.Invoke(null, new[] { delegateGetter });
        }
        return _getter;
      }
    }

    /*==========================================================================================================================
    | SETTER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a strongly typed delegate for accessing either a property setter or a void method with a single parameter.
    /// </summary>
    /// <remarks>
    ///   The value is cached alongside this instance of the <see cref="MemberAccessor"/> to improve performance across multiple
    ///   calls.
    /// </remarks>
    private Action<object, object?>? Setter {
      get {
        if (_setter is null) {
          var delegateType      = typeof(Action<,>).MakeGenericType(MemberInfo.DeclaringType, Type);
          var delegateSetter    = (Delegate?)null;
          if (MemberInfo is PropertyInfo propertInfo) {
            delegateSetter      = propertInfo.GetSetMethod().CreateDelegate(delegateType);
          }
          else if (MemberInfo is MethodInfo methodInfo) {
            delegateSetter      = methodInfo.CreateDelegate(delegateType);
          }
          var setterWithTypes   = SetterDelegateMethod.MakeGenericMethod(MemberInfo.DeclaringType, Type);
          _setter               = (Action<object, object?>)setterWithTypes.Invoke(null, new[] { delegateSetter });
        }
        return _setter;
      }
    }

    /*==========================================================================================================================
    | CREATE SETTER DELEGATE SIGNATURE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a generic signature for a setter action.
    /// </summary>
    /// <typeparam name="TClass">The object containing the member to set the value on.</typeparam>
    /// <typeparam name="TValue">The value that the member should be set to.</typeparam>
    /// <param name="deleg">The strongly typed <see cref="Action{T, TValue}"/> used to set the value.</param>
    /// <returns>
    ///   A loosely typed <see cref="Action{Object, Object}"/> that wraps the strongly typed <paramref name="deleg"/>.
    /// </returns>
    private static Action<object, object?> CreateSetterDelegateSignature<TClass, TValue>(Action<TClass, TValue?> deleg)
      => (instance, value) => deleg((TClass)instance, (TValue?)value);

    /*==========================================================================================================================
    | CREATE GETTER DELEGATE SIGNATURE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a generic signature for a getter action.
    /// </summary>
    /// <typeparam name="TClass">The object containing the member to retrieve.</typeparam>
    /// <typeparam name="TResult">The return value of the member.</typeparam>
    /// <param name="deleg">The strongly typed <see cref="Func{T, TResult}"/> used to retrieve the value.</param>
    /// <returns>
    ///   A loosely typed <see cref="Func{Object, Object}"/> that wraps the strongly typed <paramref name="deleg"/>.
    /// </returns>
    private static Func<object, object?> CreateGetterDelegateSignature<TClass, TResult>(Func<TClass, TResult?> deleg)
      => instance => deleg((TClass)instance);

    /*==========================================================================================================================
    | SETTER DELEGATE METHOD REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="CreateSetterDelegateSignature{TClass, TValue}(Action{TClass, TValue})"/> method.
    /// </summary>
    private static readonly MethodInfo SetterDelegateMethod =
      typeof(MemberAccessor).GetMethod(nameof(CreateSetterDelegateSignature), BindingFlags.NonPublic | BindingFlags.Static)!;

    /*==========================================================================================================================
    | GETTER DELEGATE METHOD REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="CreateGetterDelegateSignature{TClass, TResult}(Func{TClass, TResult})"/> method.
    /// </summary>
    private static readonly MethodInfo GetterDelegateMethod =
      typeof(MemberAccessor).GetMethod(nameof(CreateGetterDelegateSignature), BindingFlags.NonPublic | BindingFlags.Static)!;

  } //Class
} //Namespace