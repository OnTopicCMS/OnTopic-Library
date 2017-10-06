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
using System.Text;

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A collection of <see cref="PropertyInfoCollection"/> instances, each associated with a specific <see cref="Type"/>.
  /// </summary>
  internal class TypeCollection : KeyedCollection<Type, PropertyInfoCollection> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    List<Type>                  _callableTypes                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TypeCollection"/> class. Assumes no parent topic is available.
    /// </summary>
    /// <param name="callableTypes">An optional list of supported types.</param>
    internal TypeCollection(List<Type> callableTypes = null) : base() {
      _callableTypes = callableTypes;
    }

    /*==========================================================================================================================
    | PROPERTY: CALLABLE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A list of types that are allowed to be called.
    /// </summary>
    internal List<Type> CallableTypes {
      get {
        if (_callableTypes == null) {
          _callableTypes = new List<Type>();
          _callableTypes.Add(typeof(bool));
          _callableTypes.Add(typeof(int));
          _callableTypes.Add(typeof(string));
        }
        return _callableTypes;
      }
    }

    /*==========================================================================================================================
    | METHOD: GET PROPERTIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns a collection of <see cref="PropertyInfo"/> objects associated with a specific type.
    /// </summary>
    /// <remarks>
    ///   If the collection cannot be found locally, it will be created.
    /// </remarks>
    /// <param name="type">The type for which the properties should be retrieved.</param>
    internal PropertyInfoCollection GetProperties(Type type) {
      if (!Contains(type)) {
        Add(new PropertyInfoCollection(type));
      }
      return this[type];
    }

    /*==========================================================================================================================
    | METHOD: GET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local property by a given name, and returns the associated <see cref="PropertyInfo"/>
    ///   instance.
    /// </summary>
    internal PropertyInfo GetProperty(Type type, string name) {
      var properties = GetProperties(type);
      if (properties.Contains(name)) {
        return properties[name];
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: SET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    internal bool SetProperty(object target, string name, string value) {

      var property = GetProperty(target.GetType(), name);

      if (property == null || !property.CanWrite || !CallableTypes.Contains(property.PropertyType)) {
        return false;
      }

      object valueObject;

      if (property.PropertyType.Equals(typeof(Boolean))) {
        valueObject = value.Equals("1");
      }
      else if (property.PropertyType.Equals(typeof(Int32))) {
        Int32.TryParse(value, out int intValue);
        valueObject = intValue;
      }
      else if (property.PropertyType.Equals(typeof(String))) {
        valueObject = value;
      }
      else {
        throw new Exception(target.GetType().ToString() + ", " + name + ", " + property.PropertyType);
        return false;
      }

      property.SetValue(target, valueObject);
      return true;

    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override Type GetKeyForItem(PropertyInfoCollection item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Type;
    }

  } //Class

} //Namespace