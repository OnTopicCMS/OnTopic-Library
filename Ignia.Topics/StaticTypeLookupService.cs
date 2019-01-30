/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Diagnostics;
using System.Reflection;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="StaticTypeLookupService"/> can be configured to provide a lookup of .
  /// </summary>
  public class StaticTypeLookupService: KeyedCollection<string, Type>, ITypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="StaticTypeLookupService"/>. Optionally accepts a list of <see cref="Type"/>
    ///   instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <remarks>
    ///   Any <see cref="Type"/> instances submitted via <paramref name="types"/> should be unique by <see
    ///   cref="MemberInfo.Name"/>; if they are not, they will be removed.
    /// </remarks>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    public StaticTypeLookupService(
      IEnumerable<Type> types = null,
      Type defaultType = null
    ): base(StringComparer.InvariantCultureIgnoreCase) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Set default type
      \---------------------------------------------------------------------------------------------------------------------*/
      DefaultType = defaultType;

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
    | PROPERTY: DEFAULT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The default type to return in case <see cref="GetType(String)"/> cannot find a match.
    /// </summary>
    public Type DefaultType { get; }

    /*==========================================================================================================================
    | METHOD: GET TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Type"/> from the class based on its string representation.
    /// </summary>
    /// <param name="typeName">A string representing the type.</param>
    /// <returns>A class type corresponding to the specified string.</returns>
    /// <requires description="The contentType key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    public Type GetType(string typeName) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached entry
      \---------------------------------------------------------------------------------------------------------------------*/
      if (Contains(typeName)) {
        return this[typeName];
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return default
      \---------------------------------------------------------------------------------------------------------------------*/
      return DefaultType;

    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Type"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(Type item) {
      Contract.Requires<ArgumentNullException>(item != null, "The item must be available in order to derive its key.");
      return item.Name;
    }

  } //Class
} //Namespace