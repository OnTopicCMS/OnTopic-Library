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
  | CLASS: PROPERTY INFO COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides keyed access to a collection of <see cref="PropertyInfo"/> instances.
  /// </summary>
  public class PropertyInfoCollection : KeyedCollection<string, PropertyInfo> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    Type                        _type                           = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="PropertyInfoCollection"/> class associated with a <see cref="Type"/>
    ///   name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> associated with the collection.</param>
    public PropertyInfoCollection(Type type) : base(StringComparer.OrdinalIgnoreCase) {
      Contract.Requires(type != null);
      _type = type;
      foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public)) {
        Add(property);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns the type associated with this collection.
    /// </summary>
    internal Type Type => _type;

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(PropertyInfo item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Name;
    }

  } //Class

} //Namespace