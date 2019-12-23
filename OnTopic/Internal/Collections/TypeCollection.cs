/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Internal.Collections {

  /*============================================================================================================================
  | CLASS: TYPE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a <see cref="KeyedCollection{TKey, TItem}"/> of <see cref="Type"/> instances indexed by <see
  ///   cref="String"/>.
  /// </summary>
  public class TypeCollection : KeyedCollection<string, Type> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new <see cref="TypeCollection"/>. Optionally accepts an <see cref="IEnumerable"/> of <see
    ///   cref="Type" /> instances to prepopulate the collection.
    /// </summary>
    public TypeCollection(IEnumerable<Type>? types = null) : base(StringComparer.InvariantCultureIgnoreCase) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate collection
      \---------------------------------------------------------------------------------------------------------------------*/
      if (types != null) {
        foreach (var type in types) {
          if (!Contains(type)) {
            Add(type);
          }
        }
      }

    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Type"/>, returns the value that will act as the key—in this case, the <see cref="MemberInfo.Name"/>
    ///   property.
    /// </summary>
    /// <param name="item">The <see cref="Type"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(Type item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Name;
    }

  } //Class
} //Namespace