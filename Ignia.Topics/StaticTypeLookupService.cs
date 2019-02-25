/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Internal.Collections;
using System.Reflection;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TYPE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="StaticTypeLookupService"/> can be configured to provide a lookup of .
  /// </summary>
  public class StaticTypeLookupService: ITypeLookupService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            TypeCollection                  _typeCollection                 = new TypeCollection();

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
    ) {

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
    ///   The default type to return in case <see cref="Lookup(String)"/> cannot find a match.
    /// </summary>
    public Type DefaultType { get; }

    /*==========================================================================================================================
    | METHOD: LOOKUP
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
    public Type Lookup(string typeName) => Contains(typeName) ? _typeCollection[typeName] : DefaultType;

    /*==========================================================================================================================
    | METHOD: ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a <see cref="Type"/> to the underlying collection.
    /// </summary>
    protected void Add(Type type) => _typeCollection.Add(type);

    /*==========================================================================================================================
    | METHOD: CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the underlying collection has a <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to located in the collection.</param>
    /// <returns><c>True</c> if the <see cref="Type"/> exists in the collection.</returns>
    protected bool Contains(Type type) => _typeCollection.Contains(type);

    /// <summary>
    ///   Determines if the underlying collection has a <see cref="Type"/> with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the <see cref="Type"/> to located in the collection.</param>
    /// <returns><c>True</c> if a <see cref="Type"/> with <paramref name="key"/> exists in the collection.</returns>
    protected bool Contains(string key) => _typeCollection.Contains(key);

    /*==========================================================================================================================
    | METHOD: REMOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a <see cref="Type"/> with the provided <paramref name="key"/>.
    /// </summary>
    protected void Remove(string key) => _typeCollection.Remove(key);

  } //Class
} //Namespace