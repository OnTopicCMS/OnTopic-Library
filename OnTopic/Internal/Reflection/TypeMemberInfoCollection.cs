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
  | CLASS: KEYED MEMBER INFO COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides keyed access to a collection of <see cref="MemberInfoCollection"/> instances.
  /// </summary>
  internal class TypeMemberInfoCollection: KeyedCollection<Type, MemberInfoCollection> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TypeMemberInfoCollection"/> class.
    /// </summary>
    internal TypeMemberInfoCollection() : base() {
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
        lock (Items) {
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
    internal MemberInfoCollection<T> GetMembers<T>(Type type) where T : MemberInfo =>
      new(type, GetMembers(type).Where(m => typeof(T).IsAssignableFrom(m.GetType())).Cast<T>());

    /*==========================================================================================================================
    | METHOD: GET MEMBER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify a local member by a given name, and returns the associated <see cref="MemberInfo"/>
    ///   instance.
    /// </summary>
    internal MemberInfo? GetMember(Type type, string name) {
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
    internal T? GetMember<T>(Type type, string name) where T : MemberInfo {
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
    internal bool HasMember(Type type, string name) => GetMember(type, name) is not null;

    /*==========================================================================================================================
    | METHOD: HAS MEMBER {T}
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Used reflection to identify if a local member of type <typeparamref name="T"/> is available.
    /// </summary>
    internal bool HasMember<T>(Type type, string name) where T : MemberInfo => GetMember<T>(type, name) is not null;

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