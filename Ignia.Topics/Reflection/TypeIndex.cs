/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Ignia.Topics.Reflection {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TypeIndex"/> will search all assemblies for <see cref="Type"/> instances that match a predicate.
  /// </summary>
  internal class TypeIndex: KeyedCollection<string, Type> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TypeIndex"/> based on a <paramref name="predicate"/> and, optionally, a
    ///   default <see cref="Type"/> object to return if none is specified.
    /// </summary>
    /// <param name="predicate">The search condition to use to identify target classes.</param>
    /// <param name="defaultType">The default type to return if no match can be found. Defaults to object.</param>
    internal TypeIndex(Func<Type, bool> predicate, Type defaultType = null) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Set default type
      \---------------------------------------------------------------------------------------------------------------------*/
      DefaultType = defaultType;

      /*----------------------------------------------------------------------------------------------------------------------
      | Find target classes
      \---------------------------------------------------------------------------------------------------------------------*/
      var matchedTypes = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(t => t.GetTypes())
        .Where(t => t.IsClass && predicate(t))
        .OrderBy(t => t.Namespace.StartsWith("Ignia.Topics"))
        .ToList();

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate collection
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var type in matchedTypes) {
        if (!Contains(type)) {
          Add(type);
        }
      }

    }

    /*==========================================================================================================================
    | PROPERTY: DEFAULT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The default type to return in case <see cref="GetType(String)"/> cannot find a match.
    /// </summary>
    internal Type DefaultType { get; }

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
    internal Type GetType(string typeName) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(typeName));
      Contract.Ensures(Contract.Result<Type>() != null);

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
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Name;
    }

  } //Class
} //Namespace