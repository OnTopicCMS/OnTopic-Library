/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A collection of <see cref="MemberInfoCollection"/> instances, each associated with a specific <see cref="Type"/>.
  /// </summary>
  internal class TypeCollection : KeyedCollection<Type, MemberInfoCollection> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static                      List<Type>                      _settableTypes                  = null;
    private                     Type                            _attributeFlag                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TypeCollection"/> class.
    /// </summary>
    /// <param name="attributeFlag">
    ///   An optional <see cref="System.Attribute"/> which properties must have defined to be considered writable.
    /// </param>
    internal TypeCollection(Type attributeFlag = null) : base() {
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
    internal MemberInfoCollection GetMembers(Type type) {
      if (!Contains(type)) {
        Add(new MemberInfoCollection(type));
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
    internal MemberInfoCollection<T> GetMembers<T>(Type type) where T: MemberInfo {
      return new MemberInfoCollection<T>(type, GetMembers(type).Where(m => typeof(T).IsAssignableFrom(m.GetType())).Cast<T>());
    }

    /*==========================================================================================================================
    | METHOD: GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local member by a given name, and returns the associated <see cref="MemberInfo"/>
    ///   instance.
    /// </summary>
    internal MemberInfo GetMember(Type type, string name)  {
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
    internal T GetMember<T>(Type type, string name) where T : MemberInfo {
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
    internal bool HasMember(Type type, string name) => GetMember(type, name) != null;

    /*==========================================================================================================================
    | METHOD: HAS MEMBER {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local member of type <typeparamref name="T"/> is available.
    /// </summary>
    internal bool HasMember<T>(Type type, string name) where T: MemberInfo => GetMember<T>(type, name) != null;

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the property is not available.
    /// </remarks>
    internal bool HasSettableProperty(Type type, string name) {
      var property = GetMember<PropertyInfo>(type, name);
      return (
        property != null &&
        property.CanWrite &&
        SettableTypes.Contains(property.PropertyType) &&
        (_attributeFlag == null || System.Attribute.IsDefined(property, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET PROPERTY VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    internal bool SetPropertyValue(object target, string name, string value) {

      if (!HasSettableProperty(target.GetType(), name)) {
        return false;
      }

      var property = GetMember<PropertyInfo>(target.GetType(), name);

      Contract.Assume(property != null);

      object valueObject = GetValueObject(property.PropertyType, value);

      if (valueObject == null) {
        return false;
      }

      property.SetValue(target, valueObject);
      return true;

    }

    /*==========================================================================================================================
    | METHOD: HAS SETTABLE METHOD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local method is available and settable.
    /// </summary>
    /// <remarks>
    ///   Will return false if the method is not available. Methods are only considered settable if they have one parameter of
    ///   a settable type.
    /// </remarks>
    internal bool HasSettableMethod(Type type, string name) {
      var method = GetMember<MethodInfo>(type, name);
      return (
        method != null &&
        method.GetParameters().Count().Equals(1) &&
        SettableTypes.Contains(method.GetParameters().First().ParameterType) &&
        (_attributeFlag == null || System.Attribute.IsDefined(method, _attributeFlag))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    internal bool SetMethodValue(object target, string name, string value) {

      if (!HasSettableMethod(target.GetType(), name)) {
        return false;
      }

      var method = GetMember<MethodInfo>(target.GetType(), name);

      Contract.Assume(method != null);

      object valueObject = GetValueObject(method.GetParameters().First().ParameterType, value);

      if (valueObject == null) {
        return false;
      }

      method.Invoke(target, new object[] {valueObject});

      return true;

    }

    /*==========================================================================================================================
    | METHOD: GET METHOD VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a method, assuming that it has no parameters.
    /// </summary>
    internal object GetMethodValue(object target, string name, Type type = null) {

      var getter = GetMember<MethodInfo>(target.GetType(), name);

      if (getter != null && getter.GetParameters().Length.Equals(0)) {
        return (object)getter.Invoke(target, new object[] { });
      }

      return null;

    }

    /*==========================================================================================================================
    | METHOD: GET VALUE OBJECT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Converts a string value to an object of the target type.
    /// </summary>
    private object GetValueObject(Type type, string value) {

      var valueObject = (object)null;

      if (type.Equals(typeof(bool))) {
        valueObject = value.Equals("1") || value.Equals("true", StringComparison.InvariantCultureIgnoreCase);
      }
      else if (type.Equals(typeof(int))) {
        Int32.TryParse(value, out var intValue);
        valueObject = intValue;
      }
      else if (type.Equals(typeof(string))) {
        valueObject = value;
      }
      else if (type.Equals(typeof(DateTime))) {
        if (DateTime.TryParse(value, out var date)) {
          valueObject = date;
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
    internal List<Type> SettableTypes {
      get {
        if (_settableTypes == null) {
          _settableTypes = new List<Type> {
            typeof(bool),
            typeof(int),
            typeof(string),
            typeof(DateTime)
          };
        }
        return _settableTypes;
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
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Type;
    }

  } //Class

} //Namespace