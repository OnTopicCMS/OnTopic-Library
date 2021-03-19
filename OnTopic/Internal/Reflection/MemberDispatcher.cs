/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: MEMBER DISPATCHER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="MemberDispatcher"/> provides methods that simplify late-binding access to properties and methods.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="MemberDispatcher"/> allows properties and members to be looked up and called based on string
  ///     representations of both the member names as well as, optionally, the values. String values can be deserialized into
  ///     various value formats supported by <see cref="SettableTypes"/>.
  ///   </para>
  ///   <para>
  ///     For retrieving values, the typical workflow is for a caller to check either <see cref="HasGettableMethod(Type, String,
  ///     Type?)"/> or <see cref="HasGettableProperty(Type, String, Type?)"/>, followed by <see cref="GetPropertyValue(Object,
  ///     String, Type?)"/> or <see cref="GetMethodValue(Object, String, Type?)"/> to retrieve the value.
  ///   </para>
  ///   <para>
  ///     For setting values, the typical workflow is for a caller to check either <see cref="HasSettableMethod(Type, String,
  ///     Type?)"/> or <see cref="HasSettableProperty(Type, String, Type?)"/>, followed by <see cref="SetMethodValue(Object,
  ///     String, Object?)"/> or <see cref="SetMethodValue(Object, String, Object?)"/> to retrieve the value. In these
  ///     scenarios, the <see cref="MemberDispatcher"/> will attempt to deserialize the <c>value</c> parameter from <see cref=
  ///     "String"/> to the type expected by the corresponding property or method. Typically, this will be a <see cref="Int32"
  ///     />, <see cref="Double"/>, <see cref="Boolean"/>, or <see cref="DateTime"/>.
  ///   </para>
  ///   <para>
  ///     Alternatively, setters can call <see cref="SetMethodValue(Object, String, Object?)"/> or <see cref="SetPropertyValue(
  ///     Object, String, Object?)"/>, in which case the final <c>value</c> parameter will be set the target property, or passed
  ///     as the parameter of the method without any attempt to convert it. Obviously, this requires that the target type be
  ///     assignable from the <c>value</c> object.
  ///   </para>
  ///   <para>
  ///     The <see cref="MemberDispatcher"/> is an internal service intended to meet the specific needs of OnTopic, and comes
  ///     with certain limitations. It only supports setting values of methods with a single parameter, which is assumed to
  ///     correspond to the <c>value</c> parameter. It will only operate against the first overload of a method, and/or the most
  ///     derived version of a member.
  ///   </para>
  /// </remarks>
  internal class MemberDispatcher {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Type?                           _attributeFlag;
    private readonly            TypeMemberInfoCollection        _memberInfoCache                = new();

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes static properties on <see cref="MemberDispatcher"/>.
    /// </summary>
    static MemberDispatcher() {
      SettableTypes = new() {
        typeof(bool),
        typeof(bool?),
        typeof(int),
        typeof(int?),
        typeof(double),
        typeof(double?),
        typeof(string),
        typeof(DateTime),
        typeof(DateTime?),
        typeof(Uri)
      };
    }

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberDispatcher"/> class.
    /// </summary>
    /// <param name="attributeFlag">
    ///   An optional <see cref="Attribute"/> which properties must have defined to be considered writable.
    /// </param>
    internal MemberDispatcher(Type? attributeFlag = null) : base() {
      _attributeFlag = attributeFlag;
    }

    /*==========================================================================================================================
    | METHOD: GET MEMBERS {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns a collection of <typeparamref name="T"/> objects associated with a specific type.
    /// </summary>
    /// <remarks>
    ///   If the collection cannot be found locally, it will be created.
    /// </remarks>
    /// <param name="type">The type for which the members should be retrieved.</param>
    internal MemberInfoCollection<T> GetMembers<T>(Type type) where T : MemberInfo => _memberInfoCache.GetMembers<T>(type);

    /*==========================================================================================================================
    | METHOD: GET MEMBER {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local member by a given name, and returns the associated <typeparamref name="T"/>
    ///   instance.
    /// </summary>
    internal T? GetMember<T>(Type type, string name) where T : MemberInfo => _memberInfoCache.GetMember<T>(type, name);

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    /// <param name="type">The <see cref="Type"/> on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal bool HasSettableProperty(Type type, string name, Type? targetType = null) {
      var property = GetMember<PropertyInfo>(type, name);
      return (
        property is not null and { CanWrite: true } &&
        IsSettableType(property.PropertyType, targetType) &&
        (_attributeFlag is null || Attribute.IsDefined(property, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>, <see cref="
    ///   Int32"/>, or <see cref="Boolean"/>, or is otherwise compatible with the <paramref name="value"/> type.
    /// </summary>
    /// <param name="target">The object on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="value">The value to set on the property.</param>
    internal void SetPropertyValue(object target, string name, object? value) {

      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      var isString = value?.GetType() == typeof(string);

      var property = GetMember<PropertyInfo>(target.GetType(), name);

      Contract.Assume(property, $"The {name} property could not be retrieved.");

      var valueObject = isString? GetValueObject(property.PropertyType, value as string) : value;

      property.SetValue(target, valueObject);

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
    /// <param name="type">The <see cref="Type"/> on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal bool HasGettableProperty(Type type, string name, Type? targetType = null) {
      var property = GetMember<PropertyInfo>(type, name);
      return (
        property is not null and { CanRead: true } &&
        IsSettableType(property.PropertyType, targetType) &&
        (_attributeFlag is null || Attribute.IsDefined(property, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: GET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) readable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    /// <param name="target">The object instance on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal object? GetPropertyValue(object target, string name, Type? targetType = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate member type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!HasGettableProperty(target.GetType(), name, targetType)) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var property = GetMember<PropertyInfo>(target.GetType(), name);

      Contract.Assume(property, $"The {name} property could not be retrieved.");

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
    /// <param name="type">The <see cref="Type"/> on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal bool HasSettableMethod(Type type, string name, Type? targetType = null) {
      var method = GetMember<MethodInfo>(type, name);
      return (
        method is not null &&
        method.GetParameters().Length is 1 &&
        IsSettableType(method.GetParameters().First().ParameterType, targetType) &&
        (_attributeFlag is null || Attribute.IsDefined(method, _attributeFlag))
      );
    }

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
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="value">The value to set the method to.</param>
    internal void SetMethodValue(object target, string name, object? value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = GetMember<MethodInfo>(target.GetType(), name);

      Contract.Assume(method, $"The {name}() method could not be retrieved.");

      var valueObject = value;

      if (valueObject is string) {
        valueObject = GetValueObject(method.GetParameters().First().ParameterType, valueObject as string);
      }

      method.Invoke(target, new object?[] { valueObject });

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
    /// <param name="type">The <see cref="Type"/> on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal bool HasGettableMethod(Type type, string name, Type? targetType = null) {
      var method = GetMember<MethodInfo>(type, name);
      return (
        method is not null &&
        !method.GetParameters().Any() &&
        IsSettableType(method.ReturnType, targetType) &&
        (_attributeFlag is null || Attribute.IsDefined(method, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: GET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it has no parameters.
    /// </summary>
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    internal object? GetMethodValue(object target, string name, Type? targetType = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate member type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!HasGettableMethod(target.GetType(), name, targetType)) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = GetMember<MethodInfo>(target.GetType(), name);

      Contract.Assume(method, $"The method '{name}' could not be found on the '{target.GetType()}' class.");

      return method.Invoke(target, Array.Empty<object>());

    }

    /*==========================================================================================================================
    | METHOD: IS SETTABLE TYPE?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether a given type is settable, either assuming the list of <see cref="SettableTypes"/>, or provided a
    ///   specific <paramref name="targetType"/>.
    /// </summary>
    private static bool IsSettableType(Type sourceType, Type? targetType = null) {

      if (targetType is not null) {
        return sourceType.IsAssignableFrom(targetType);
      }
      return SettableTypes.Contains(sourceType);

    }

    /*==========================================================================================================================
    | METHOD: GET VALUE OBJECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Converts a string value to an object of the target type.
    /// </summary>
    private static object? GetValueObject(Type type, string? value) {

      var valueObject = (object?)null;

      //Treat empty as null for non-strings, regardless of whether they’re nullable
      if (!type.Equals(typeof(string)) && String.IsNullOrWhiteSpace(value)) {
        return null;
      }

      if (value is null) return null;

      if (type.Equals(typeof(string))) {
        valueObject = value;
      }
      else if (type.Equals(typeof(bool)) || type.Equals(typeof(bool?))) {
        if (value is "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase)) {
          valueObject = true;
        }
        else if (value is "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase)) {
          valueObject = false;
        }
      }
      else if (type.Equals(typeof(int)) || type.Equals(typeof(int?))) {
        if (Int32.TryParse(value, out var intValue)) {
          valueObject = intValue;
        }
      }
      else if (type.Equals(typeof(double)) || type.Equals(typeof(double?))) {
        if (Double.TryParse(value, out var doubleValue)) {
          valueObject = doubleValue;
        }
      }
      else if (type.Equals(typeof(DateTime)) || type.Equals(typeof(DateTime?))) {
        if (DateTime.TryParse(value, out var date)) {
          valueObject = date;
        }
      }
      else if (type.Equals(typeof(Uri))) {
        if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)) {
          valueObject = uri;
        }
      }

      return valueObject;

    }

    /*==========================================================================================================================
    | PROPERTY: SETTABLE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A list of types that are allowed to be set using <see cref="SetPropertyValue(Object, String, Object?)"/>.
    /// </summary>
    internal static Collection<Type> SettableTypes { get; }

  } //Class
} //Namespace