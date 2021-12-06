/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE ACCESSOR EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TypeAccessorExtensions"/> provides methods that simplify late-binding access to properties and methods.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="TypeAccessorExtensions"/> allows properties and members to be looked up and called based on string
  ///     representations of both the member names as well as, optionally, the values. String values can be deserialized into
  ///     various value formats supported by <see cref="AttributeValueConverter"/>.
  ///   </para>
  ///   <para>
  ///     For retrieving values, the typical workflow is for a caller to check either <see cref="HasGettableMethod(TypeAccessor,
  ///     String, Type?)"/> or <see cref="HasGettableProperty(TypeAccessor, String, Type?)"/>, followed by <see cref="
  ///     GetPropertyValue(TypeAccessor, Object, String, Type?)"/> or <see cref="GetMethodValue(TypeAccessor, Object, String,
  ///     Type?)"/> to retrieve the value.
  ///   </para>
  ///   <para>
  ///     For setting values, the typical workflow is for a caller to check either <see cref="HasSettableMethod(TypeAccessor,
  ///     String, Type?)"/> or <see cref="HasSettableProperty(TypeAccessor, String, Type?)"/>, followed by <see cref="
  ///     SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> or <see cref="SetMethodValue(TypeAccessor, Object,
  ///     String, Object?, Boolean)"/> to retrieve the value. In these scenarios, the <see cref="TypeAccessorExtensions"/> will
  ///     attempt to deserialize the <c>value</c> parameter from <see cref="String"/> to the type expected by the corresponding
  ///     property or method. Typically, this will be a <see cref="Int32"/>, <see cref="Double"/>, <see cref="Boolean"/>, or
  ///     <see cref="DateTime"/>.
  ///   </para>
  ///   <para>
  ///     Alternatively, setters can call <see cref="SetMethodValue(TypeAccessor, Object, String, Object?, Boolean)"/> or <see
  ///     cref="SetPropertyValue(TypeAccessor, Object, String, Object?, Boolean)"/>, in which case the final <c>value</c>
  ///     parameter will be set the target property, or passed as the parameter of the method without any attempt to convert it.
  ///     Obviously, this requires that the target type be assignable from the <c>value</c> object.
  ///   </para>
  ///   <para>
  ///     The <see cref="TypeAccessorExtensions"/> is an internal service intended to meet the specific needs of OnTopic, and
  ///     comes with certain limitations. It only supports setting values of methods with a single parameter, which is assumed
  ///     to correspond to the <c>value</c> parameter. It will only operate against the first overload of a method, and/or the
  ///     most derived version of a member.
  ///   </para>
  /// </remarks>
  internal static class TypeAccessorExtensions {

    /*==========================================================================================================================
    | METHOD: GET MEMBERS {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns a collection of <typeparamref name="T"/> objects associated with a specific type.
    /// </summary>
    /// <remarks>
    ///   If the collection cannot be found locally, it will be created.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the members should be retrieved.</param>
    internal static MemberInfoCollection<T> GetMembers<T>(this TypeAccessor typeAccessor) where T : MemberInfo {
      List<MemberAccessor> members = new List<MemberAccessor>();
      if (typeof(T) == typeof(PropertyInfo)) {
        members = typeAccessor.GetMembers(MemberTypes.Property);
      }
      else if (typeof(T) == typeof(MethodInfo)) {
        members = typeAccessor.GetMembers(MemberTypes.Method);
      }
      else if (typeof(T) == typeof(ConstructorInfo)) {
        return new MemberInfoCollection<T>(typeAccessor.Type);
      }
      return new MemberInfoCollection<T>(typeAccessor.Type, members.Select(m => m.MemberInfo).Cast<T>());
    }

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal static bool HasSettableProperty(
      this TypeAccessor typeAccessor,
      string name,
      Type? targetType = null,
      Type? attributeFlag = null
    ) {
      var property = typeAccessor.GetMember(name);
      return (
        property is not null and { CanWrite: true, MemberType: MemberTypes.Property } &&
        IsSettableType(property.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(property.MemberInfo as PropertyInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasSettableProperty(TypeAccessor, string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal static bool HasSettableProperty<T>(this TypeAccessor typeAccessor, string name, Type? targetType = null) where T : Attribute
      => typeAccessor.HasSettableProperty(name, targetType, typeof(T));

    /*==========================================================================================================================
    | METHOD: SET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>, <see cref="
    ///   Int32"/>, or <see cref="Boolean"/>, or is otherwise compatible with the <paramref name="value"/> type.
    /// </summary>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="target">The object on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="value">The value to set on the property.</param>
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal static void SetPropertyValue(
      this TypeAccessor typeAccessor,
      object target,
      string name,
      object? value,
      bool allowConversion = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var property = typeAccessor.GetMember(name);

      Contract.Assume(property, $"The {name} property could not be retrieved.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      property.SetValue(target, value, allowConversion);

    }

    /*==========================================================================================================================
    | METHOD: HAS GETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and gettable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal static bool HasGettableProperty(
      this TypeAccessor typeAccessor,
      string name,
      Type? targetType = null,
      Type? attributeFlag = null
    ) {
      var property = typeAccessor.GetMember(name);
      return (
        property is not null and { CanRead: true, MemberType: MemberTypes.Property } &&
        IsSettableType(property.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(property.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasGettableProperty(TypeAccessor, string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal static bool HasGettableProperty<T>(this TypeAccessor typeAccessor, string name, Type? targetType = null) where T : Attribute
      => typeAccessor.HasGettableProperty(name, targetType, typeof(T));

    /*==========================================================================================================================
    | METHOD: GET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) readable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="target">The object instance on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal static object? GetPropertyValue(this TypeAccessor typeAccessor, object target, string name, Type? targetType = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var property = typeAccessor.GetMember(name);

      if (property is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return property.GetValue(target);

    }

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local method is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the method is not available. Methods are only considered settable if they have one parameter of
    ///   a settable type. Be aware that this will return <c>false</c> if the method has additional parameters, even if those
    ///   additional parameters are optional.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal static bool HasSettableMethod(this TypeAccessor typeAccessor, string name, Type? targetType = null, Type? attributeFlag = null) {
      var method = typeAccessor.GetMember(name);
      return (
        method is not null and { CanWrite: true, MemberType: MemberTypes.Method } &&
        IsSettableType(method.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(method.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasSettableMethod(TypeAccessor, string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal static bool HasSettableMethod<T>(this TypeAccessor typeAccessor, string name, Type? targetType = null) where T : Attribute
      => typeAccessor.HasSettableMethod(name, targetType, typeof(T));

    /*==========================================================================================================================
    | METHOD: SET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that the parameter value is compatible with the <paramref name="value"/>
    ///   type.
    /// </summary>
    /// <remarks>
    ///   Be aware that this will only succeed if the method has a single parameter of a settable type. If additional parameters
    ///   are present it will return <c>false</c>, even if those additional parameters are optional.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="value">The value to set the method to.</param>
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal static void SetMethodValue(
      this TypeAccessor typeAccessor,
      object target,
      string name,
      object? value,
      bool allowConversion = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = typeAccessor.GetMember(name);

      Contract.Assume(method, $"The {name}() method could not be retrieved.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      method.SetValue(target, value, allowConversion);

    }

    /*==========================================================================================================================
    | METHOD: HAS GETTABLE METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local method is available and gettable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the method is not available. Methods are only considered gettable if they have no parameters and
    ///   their return value is a settable type.
    /// </remarks>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal static bool HasGettableMethod(this TypeAccessor typeAccessor, string name, Type? targetType = null, Type? attributeFlag = null) {
      var method = typeAccessor.GetMember(name);
      return (
        method is not null and { CanRead: true, MemberType: MemberTypes.Method } &&
        IsSettableType(method.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(method.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasGettableMethod(TypeAccessor, string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal static bool HasGettableMethod<T>(this TypeAccessor typeAccessor, string name, Type? targetType = null) where T : Attribute
      => typeAccessor.HasGettableMethod(name, targetType, typeof(T));

    /*==========================================================================================================================
    | METHOD: GET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it has no parameters.
    /// </summary>
    /// <param name="typeAccessor">The <see cref="TypeAccessor"/> on which the property is defined.</param>
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal static object? GetMethodValue(this TypeAccessor typeAccessor, object target, string name, Type? targetType = null, Type? attributeFlag = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = typeAccessor.GetMember(name);

      if (method is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return method.GetValue(target);

    }

    /// <inheritdoc cref="GetMethodValue(TypeAccessor, object, string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the method.</typeparam>
    internal static object? GetMethodValue<T>(this TypeAccessor typeAccessor, object target, string name, Type? targetType = null) where T : Attribute
      => typeAccessor.GetMethodValue(target, name, targetType, typeof(T));

    /*==========================================================================================================================
    | METHOD: IS SETTABLE TYPE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether a given type is settable, either assuming the list of <see cref="AttributeValueConverter"/>, or
    ///   provided a specific <paramref name="targetType"/>.
    /// </summary>
    private static bool IsSettableType(Type sourceType, Type? targetType = null) {

      if (targetType is not null) {
        return sourceType.IsAssignableFrom(targetType);
      }
      return AttributeValueConverter.IsConvertible(sourceType);

    }

  } //Class
} //Namespace