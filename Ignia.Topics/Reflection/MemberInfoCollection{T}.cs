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
  | CLASS: MEMBER INFO COLLECTION {T}
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides keyed access to a collection of <see cref="MemberInfoCollection"/> instances.
  /// </summary>
  internal class MemberInfoCollection<T> : KeyedCollection<string, T> where T : MemberInfo {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    Type _type = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberInfoCollection"/> class associated with a <see cref="Type"/>
    ///   name.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> associated with the collection.</param>
    internal MemberInfoCollection(Type type) : base(StringComparer.OrdinalIgnoreCase) {
      Contract.Requires(type != null);
      _type = type;
      foreach (
        var member
        in type.GetMembers(
          BindingFlags.Instance |
          BindingFlags.FlattenHierarchy |
          BindingFlags.NonPublic |
          BindingFlags.Public
        ).Where(m => typeof(T).IsAssignableFrom(m.GetType()))
      ) {
        Add((T)member);
      }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="MemberInfoCollection"/> class associated with a <see cref="Type"/>
    ///   name and prepopulates it with a predetermined set of <typeparamref name="T"/> instances.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> associated with the collection.</param>
    /// <param name="members">
    ///   An <see cref="IEnumerable{T}"/> of <typeparamref name="T"/> instances to populate the collection.
    /// </param>
    internal MemberInfoCollection(Type type, IEnumerable<T> members) : base(StringComparer.OrdinalIgnoreCase) {
      Contract.Requires(type != null);
      Contract.Requires(members != null);
      _type = type;
      foreach (var member in members) {
        Add(member);
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
    protected override string GetKeyForItem(T item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Name;
    }

  } //Class

} //Namespace