/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
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