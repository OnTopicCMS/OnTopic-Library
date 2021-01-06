/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A collection of <see cref="MemberInfoCollection"/> instances, each associated with a specific <see cref="Type"/>.
  /// </summary>
  public class TypeMemberInfoCollection : KeyedCollection<Type, MemberInfoCollection> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Type?                           _attributeFlag;

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes static properties on <see cref="TypeMemberInfoCollection"/>.
    /// </summary>
    static TypeMemberInfoCollection() {
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
    ///   Initializes a new instance of the <see cref="TypeMemberInfoCollection"/> class.
    /// </summary>
    /// <param name="attributeFlag">
    ///   An optional <see cref="System.Attribute"/> which properties must have defined to be considered writable.
    /// </param>
    public TypeMemberInfoCollection(Type? attributeFlag = null) : base() {
      _attributeFlag = attributeFlag;
    }

    /*==========================================================================================================================
    | METHOD: GET MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns a collection of <see cref="MemberInfo"/> objects associated with a specific type.
    /// </summary>
    /// <remarks>
    ///   If the collection cannot be found locally, it will be created.
    /// </remarks>
    /// <param name="type">The type for which the members should be retrieved.</param>
    public MemberInfoCollection GetMembers(Type type) {
      if (!Contains(type)) {
        lock(Items) {
          if (!Contains(type)) {
            Add(new(type));
          }
        }
      }
      return this[type];
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
    public MemberInfoCollection<T> GetMembers<T>(Type type) where T: MemberInfo =>
      new(type, GetMembers(type).Where(m => typeof(T).IsAssignableFrom(m.GetType())).Cast<T>());

    /*==========================================================================================================================
    | METHOD: GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local member by a given name, and returns the associated <see cref="MemberInfo"/>
    ///   instance.
    /// </summary>
    public MemberInfo? GetMember(Type type, string name) {
      var members = GetMembers(type);
      if (members.Contains(name)) {
        return members[name];
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: GET MEMBER {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local member by a given name, and returns the associated <typeparamref name="T"/>
    ///   instance.
    /// </summary>
    public T? GetMember<T>(Type type, string name) where T : MemberInfo {
      var members = GetMembers(type);
      if (members.Contains(name) && typeof(T).IsAssignableFrom(members[name].GetType())) {
        return members[name] as T;
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: HAS MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local member is available.
    /// </summary>
    public bool HasMember(Type type, string name) => GetMember(type, name) is not null;

    /*==========================================================================================================================
    | METHOD: HAS MEMBER {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local member of type <typeparamref name="T"/> is available.
    /// </summary>
    public bool HasMember<T>(Type type, string name) where T: MemberInfo => GetMember<T>(type, name) is not null;

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
    public bool HasSettableProperty(Type type, string name, Type? targetType = null) {
      var property = GetMember<PropertyInfo>(type, name);
      return (
        property is not null and { CanWrite: true } &&
        IsSettableType(property.PropertyType, targetType) &&
        (_attributeFlag is null || System.Attribute.IsDefined(property, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    /// <param name="target">The object on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="value">The value to set on the property.</param>
    public bool SetPropertyValue(object target, string name, string? value) {

      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      if (!HasSettableProperty(target.GetType(), name)) {
        return false;
      }

      var property = GetMember<PropertyInfo>(target.GetType(), name);

      Contract.Assume(property, $"The {name} property could not be retrieved.");

      var valueObject = GetValueObject(property.PropertyType, value);

      if (valueObject is null) {
        return false;
      }

      property.SetValue(target, valueObject);
      return true;

    }

    /// <summary>
    ///   Uses reflection to call a property, assuming that the property value is compatible with the <paramref name="value"/>
    ///   type.
    /// </summary>
    /// <param name="target">The object on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="value">The value to set on the property.</param>
    public bool SetPropertyValue(object target, string name, object? value) {

      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      if (!HasSettableProperty(target.GetType(), name, value?.GetType())) {
        return false;
      }

      var property = GetMember<PropertyInfo>(target.GetType(), name);

      Contract.Assume(property, $"The {name} property could not be retrieved.");

      if (value is null) {
        return false;
      }

      property.SetValue(target, value);
      return true;

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
    public bool HasGettableProperty(Type type, string name, Type? targetType = null) {
      var property = GetMember<PropertyInfo>(type, name);
      return (
        property is not null and { CanRead: true } &&
        IsSettableType(property.PropertyType, targetType) &&
        (_attributeFlag is null || System.Attribute.IsDefined(property, _attributeFlag))
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
    public object? GetPropertyValue(object target, string name, Type? targetType = null) {

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
    public bool HasSettableMethod(Type type, string name, Type? targetType = null) {
      var method = GetMember<MethodInfo>(type, name);
      return (
        method is not null &&
        method.GetParameters().Length is 1 &&
        IsSettableType(method.GetParameters().First().ParameterType, targetType) &&
        (_attributeFlag is null || System.Attribute.IsDefined(method, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    /// <remarks>
    ///   Be aware that this will only succeed if the method has a single parameter of a settable type. If additional parameters
    ///   are present it will return <c>false</c>, even if those additional parameters are optional.
    /// </remarks>
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="value">The value to set the method to.</param>
    public bool SetMethodValue(object target, string name, string? value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate member type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!HasSettableMethod(target.GetType(), name)) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = GetMember<MethodInfo>(target.GetType(), name);

      Contract.Assume(method, $"The {name}() method could not be retrieved.");

      var valueObject = GetValueObject(method.GetParameters().First().ParameterType, value);

      if (valueObject is null) {
        return false;
      }

      method.Invoke(target, new object[] { valueObject });

      return true;

    }

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
    public bool SetMethodValue(object target, string name, object? value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate member type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!HasSettableMethod(target.GetType(), name, value?.GetType())) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var method = GetMember<MethodInfo>(target.GetType(), name);

      Contract.Assume(method, $"The {name}() method could not be retrieved.");

      if (value is null) {
        return false;
      }

      method.Invoke(target, new object[] { value });

      return true;

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
    public bool HasGettableMethod(Type type, string name, Type? targetType = null) {
      var method = GetMember<MethodInfo>(type, name);
      return (
        method is not null &&
        !method.GetParameters().Any() &&
        IsSettableType(method.ReturnType, targetType) &&
        (_attributeFlag is null || System.Attribute.IsDefined(method, _attributeFlag))
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
    public object? GetMethodValue(object target, string name, Type? targetType = null) {

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
        return sourceType.Equals(targetType);
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
    ///   A list of types that are allowed to be set using <see cref="SetPropertyValue(Object, String, String)"/>.
    /// </summary>
    public static Collection<Type> SettableTypes { get; }

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Fires any time an item is added to the collection.
    /// </summary>
    /// <remarks>
    ///   Compared to the base implementation, will throw a specific <see cref="ArgumentException" /> error if a duplicate key
    ///   is inserted. This conveniently provides the <see cref="MemberInfoCollection{MemberInfo}.Type" />, so it's clear what
    ///   is being duplicated.
    /// </remarks>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The <see cref="MemberInfoCollection" /> instance to insert.</param>
    /// <exception cref="ArgumentException">
    ///   The TypeMemberInfoCollection already contains the MemberInfoCollection of the Type '{item.Type}'.
    /// </exception>
    protected override void InsertItem(int index, MemberInfoCollection item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(item, nameof(item));

      /*------------------------------------------------------------------------------------------------------------------------
      | Insert item, if not already present
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(item.Type)) {
        base.InsertItem(index, item);
      }
      else {
        throw new ArgumentException(
          $"The '{nameof(TypeMemberInfoCollection)}' already contains the {nameof(MemberInfoCollection)} of the Type " +
          $"'{item.Type}'.",
          nameof(item)
        );
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override Type GetKeyForItem(MemberInfoCollection item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Type;
    }

  } //Class
} //Namespace