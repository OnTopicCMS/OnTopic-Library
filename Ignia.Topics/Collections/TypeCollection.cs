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
    static                      List<Type>                      _settableTypes                  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TypeCollection"/> class.
    /// </summary>
    internal TypeCollection() : base() {
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
    | METHOD: HAS PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local property is available.
    /// </summary>
    internal bool HasProperty(Type type, string name) => GetProperty(type, name) != null;

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
      var property = GetProperty(type, name);
      return (
        property != null &&
        property.CanWrite &&
        SettableTypes.Contains(property.PropertyType) &&
        System.Attribute.IsDefined(property, typeof(AttributeSetterAttribute))
      );
    }

    /*==========================================================================================================================
    | METHOD: SET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Uses reflection to call a property, assuming that it is a) writable, and b) of type <see cref="String"/>,
    ///   <see cref="Int32"/>, or <see cref="Boolean"/>.
    /// </summary>
    internal bool SetProperty(object target, string name, string value) {

      if (!HasSettableProperty(target.GetType(), name)) {
        return false;
      }

      var property = GetProperty(target.GetType(), name);

      object valueObject;

      if (property.PropertyType.Equals(typeof(bool))) {
        valueObject = value.Equals("1");
      }
      else if (property.PropertyType.Equals(typeof(int))) {
        Int32.TryParse(value, out int intValue);
        valueObject = intValue;
      }
      else if (property.PropertyType.Equals(typeof(string))) {
        valueObject = value;
      }
      else {
        return false;
      }

      property.SetValue(target, valueObject);
      return true;

    }

    /*==========================================================================================================================
    | PROPERTY: SETTABLE TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A list of types that are allowed to be set using <see cref="SetProperty(Object, String, String)"/>.
    /// </summary>
    internal List<Type> SettableTypes {
      get {
        if (_settableTypes == null) {
          _settableTypes = new List<Type> {
            typeof(bool),
            typeof(int),
            typeof(string)
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
    protected override Type GetKeyForItem(PropertyInfoCollection item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Type;
    }

  } //Class

} //Namespace