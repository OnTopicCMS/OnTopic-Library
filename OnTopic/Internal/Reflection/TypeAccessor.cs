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
  | CLASS: MEMBER ACCESSOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to a collection of <see cref="MemberAccessor"/> instances related to a specific <see cref="Type"/> in
  ///   order to simplify common access scenarios for properties and methods.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     For retrieving values, the typical workflow is for a caller to check either <see cref="HasGettableMethod(String,
  ///     Type?)"/> or <see cref="HasGettableProperty(String, Type?)"/>, followed by <see cref="GetPropertyValue(Object, String)
  ///     "/> or <see cref="GetMethodValue(Object, String)"/> to retrieve the value.
  ///   </para>
  ///   <para>
  ///     For setting values, the typical workflow is for a caller to check either <see cref="HasSettableMethod(String, Type?)"
  ///     /> or <see cref="HasSettableProperty(String, Type?)"/>, followed by <see cref="SetMethodValue(Object, String, Object?,
  ///     Boolean)"/> or <see cref="SetMethodValue(Object, String, Object?, Boolean)"/> to retrieve the value. In these
  ///     scenarios, the <see cref="TypeAccessor"/> will attempt to deserialize the <c>value</c> parameter from <see cref="
  ///     String"/> to the type expected by the corresponding property or method. Typically, this will be a <see cref="Int32"/>,
  ///     <see cref="Double"/>, <see cref="Boolean"/>, or <see cref="DateTime"/>.
  ///   </para>
  ///   <para>
  ///     Alternatively, setters can call <see cref="SetMethodValue(Object, String, Object?, Boolean)"/> or <see cref="
  ///     SetPropertyValue(Object, String, Object?, Boolean)"/>, in which case the final <c>value</c> parameter will be set the
  ///     target property, or passed as the parameter of the method without any attempt to convert it. Obviously, this requires
  ///     that the target type be assignable from the <c>value</c> object.
  ///   </para>
  ///   <para>
  ///     The <see cref="TypeAccessor"/> is an internal service intended to meet the specific needs of OnTopic, and comes with
  ///     certain limitations. It only supports setting values of methods with a single parameter, which is assumed to
  ///     correspond to the <c>value</c> parameter. It will only operate against the first overload of a method, and/or the most
  ///     derived version of a member.
  ///   </para>
  /// </remarks>
  internal class TypeAccessor {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Dictionary<string, MemberAccessor>                              _members;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="TypeAccessor"/> based on a given <paramref name="type"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="TypeAccessor"/> constructor automatically identifies each supported <see cref="MemberInfo"/> on the
    ///   <see cref="Type"/> and adds an associated <see cref="MemberAccessor"/> to the <see cref="TypeAccessor"/> for each.
    /// </remarks>
    /// <param name="type"></param>
    internal TypeAccessor(Type type) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(type, nameof(type));

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize fields, properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      _members                  = new(StringComparer.OrdinalIgnoreCase);
      Type                      = type;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get members from type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var members               = type.GetMembers(
          BindingFlags.Instance |
          BindingFlags.FlattenHierarchy |
          BindingFlags.NonPublic |
          BindingFlags.Public
      );
      foreach (var member in members.Where(t => MemberAccessor.IsValid(t))) {
        if (!_members.ContainsKey(member.Name)) {
          _members.Add(member.Name, new(member));
        }
      }

    }

    /*==========================================================================================================================
    | TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the return type of a getter or the argument type of a setter.
    /// </summary>
    internal Type Type { get; }

    /*==========================================================================================================================
    | GET MEMBERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of properties and methods supported by the <see cref="TypeAccessor"/>.
    /// </summary>
    /// <param name="memberTypes">Optionally filters the list of members by a <see cref="MemberTypes"/> list.</param>
    /// <returns>A list of <see cref="MemberAccessor"/> instances.</returns>
    internal List<MemberAccessor> GetMembers(MemberTypes memberTypes = MemberTypes.All) =>
      _members.Values.Where(m => memberTypes == MemberTypes.All || memberTypes.HasFlag(m.MemberType)).ToList();

    /// <summary>
    ///   Retrieves a list of properties and methods as <see cref="MemberInfo"/> objects, instead of <see cref="MemberAccessor"
    ///   />s.
    /// </summary>
    /// <returns>A list of <see cref="MemberInfo"/> instances.</returns>
    internal IEnumerable<T> GetMembers<T>() where T : MemberInfo
      => GetMembers().Select(m => m.MemberInfo).Where(t => t is T).Cast<T>();

    /*==========================================================================================================================
    | GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a property or method supported by the <see cref="TypeAccessor"/> by <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>A <see cref="MemberAccessor"/> instance for getting or setting values on a given member.</returns>
    internal MemberAccessor? GetMember(string name) => _members.GetValueOrDefault(name);

    /*==========================================================================================================================
    | METHOD: GET PRIMARY CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    internal ConstructorInfo GetPrimaryConstructor() =>
      Type.GetConstructors(
        BindingFlags.Instance |
        BindingFlags.FlattenHierarchy |
        BindingFlags.Public
      ).FirstOrDefault();

    /*==========================================================================================================================
    | HAS GETTER?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="MemberAccessor"/> with a getter exists for a member with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>True if a gettable member exists; otherwise false.</returns>
    internal bool HasGetter(string name) => GetMember(name)?.CanRead ?? false;

    /*==========================================================================================================================
    | METHOD: HAS GETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and gettable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal bool HasGettableProperty(string name, Type? targetType = null, Type? attributeFlag = null) {
      var property = GetMember(name);
      return (
        property is not null and { CanRead: true, MemberType: MemberTypes.Property } &&
        IsSettableType(property.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(property.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasGettableProperty(string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal bool HasGettableProperty<T>(string name, Type? targetType = null) where T : Attribute
      => HasGettableProperty(name, targetType, typeof(T));


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
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal bool HasGettableMethod(string name, Type? targetType = null, Type? attributeFlag = null) {
      var method = GetMember(name);
      return (
        method is not null and { CanRead: true, MemberType: MemberTypes.Method } &&
        IsSettableType(method.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(method.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasGettableMethod(String, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal bool HasGettableMethod<T>(string name, Type? targetType = null) where T : Attribute
      => HasGettableMethod(name, targetType, typeof(T));

    /*==========================================================================================================================
    | GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the value from a member named <paramref name="memberName"/> from the supplied <paramref name="source"/>
    ///   object.
    /// </summary>
    /// <param name="source">The <see cref="Object"/> instance from which the value should be retrieved.</param>
    /// <param name="memberName">The name of the method or property from which the value should be retrieved.</param>
    /// <returns>The value returned from the member.</returns>
    internal object? GetValue(object source, string memberName) => GetMember(memberName)?.GetValue(source);

    /*==========================================================================================================================
    | METHOD: GET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) readable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    /// <param name="target">The object instance on which the property is defined.</param>
    /// <param name="name">The name of the property to assess.</param>
    internal object? GetPropertyValue(object target, string name) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve member
      \-----------------------------------------------------------------------------------------------------------------------*/
      var member = GetMember(name);

      if (member is null || member.MemberType != MemberTypes.Property) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return member.GetValue(target);

    }

    /*==========================================================================================================================
    | METHOD: GET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it has no parameters.
    /// </summary>
    /// <param name="target">The object instance on which the method is defined.</param>
    /// <param name="name">The name of the method to assess.</param>
    internal object? GetMethodValue(object target, string name) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve member
      \-----------------------------------------------------------------------------------------------------------------------*/
      var member = GetMember(name);

      if (member is null || member.MemberType != MemberTypes.Method) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return member.GetValue(target);

    }

    /*==========================================================================================================================
    | HAS SETTER?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="MemberAccessor"/> with a setter exists for a member with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the <see cref="MemberAccessor"/>, derived from <see cref="MemberInfo.Name"/>.</param>
    /// <returns>True if a settable member exists; otherwise false.</returns>
    internal bool HasSetter(string name) => GetMember(name)?.CanWrite ?? false;

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    /// <param name="name">The name of the property to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal bool HasSettableProperty(string name, Type? targetType = null, Type? attributeFlag = null) {
      var property = GetMember(name);
      return (
        property is not null and { CanWrite: true, MemberType: MemberTypes.Property } &&
        IsSettableType(property.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(property.MemberInfo as PropertyInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasSettableProperty(string, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal bool HasSettableProperty<T>(string name, Type? targetType = null) where T : Attribute
      => HasSettableProperty(name, targetType, typeof(T));

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
    /// <param name="name">The name of the method to assess.</param>
    /// <param name="targetType">Optional, the <see cref="Type"/> expected.</param>
    /// <param name="attributeFlag">Optional, the <see cref="Attribute"/> expected on the property.</param>
    internal bool HasSettableMethod(string name, Type? targetType = null, Type? attributeFlag = null) {
      var method = GetMember(name);
      return (
        method is not null and { CanWrite: true, MemberType: MemberTypes.Method } &&
        IsSettableType(method.Type, targetType) &&
        (attributeFlag is null || Attribute.IsDefined(method.MemberInfo, attributeFlag))
      );
    }

    /// <inheritdoc cref="HasSettableMethod(String, Type?, Type?)"/>
    /// <typeparam name="T">The <see cref="Attribute"/> expected on the property.</typeparam>
    internal bool HasSettableMethod<T>(string name, Type? targetType = null) where T : Attribute
      => HasSettableMethod(name, targetType, typeof(T));

    /*==========================================================================================================================
    | SET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets the value of a member named <paramref name="memberName"/> on the supplied <paramref name="source"/> object.
    /// </summary>
    /// <param name="source">The <see cref="Object"/> instance on which the value should be set.</param>
    /// <param name="memberName">The name of the method or property on which the value should be set.</param>
    /// <param name="value">The <see cref="Object"/> value to set the member to.</param>
    internal void SetValue(object source, string memberName, object? value) => GetMember(memberName)?.SetValue(source, value);

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
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal void SetPropertyValue(object target, string name, object? value, bool allowConversion = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var member = GetMember(name);

      Contract.Assume(member, $"The {name} property could not be retrieved.");
      Contract.Assume(member.MemberType == MemberTypes.Property, $"The {name} member is not a property.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      member.SetValue(target, value, allowConversion);

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
    /// <param name="allowConversion">
    ///   Determines whether a fallback to <see cref="AttributeValueConverter.Convert(String?, Type)"/> is permitted.
    /// </param>
    internal void SetMethodValue(object target, string name, object? value, bool allowConversion = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Requires(name, nameof(name));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var member = GetMember(name);

      Contract.Assume(member, $"The {name}() method could not be retrieved.");
      Contract.Assume(member.MemberType == MemberTypes.Method, $"The {name} member is not a method.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set value
      \-----------------------------------------------------------------------------------------------------------------------*/
      member.SetValue(target, value, allowConversion);

    }

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
