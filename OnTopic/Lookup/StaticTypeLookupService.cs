/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Reflection;

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: STATIC TYPE LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="StaticTypeLookupService"/> can be configured to provide a lookup of <see cref="Type"/> classes based on
  ///   its name.
  /// </summary>
  public class StaticTypeLookupService: ITypeLookupService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            TypeCollection                  _typeCollection                 = new();

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
    public StaticTypeLookupService(
      IEnumerable<Type>? types = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Populate collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (types is not null) {
        foreach (var type in types) {
          if (!Contains(type)) {
            Add(type);
          }
        }
      }

    }

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
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"The {nameof(DefaultType)} property has been removed. Fallbacks types can now be added to {nameof(Lookup)} directly.",
      true
    )]
    public StaticTypeLookupService(
      IEnumerable<Type>? types,
      Type? defaultType
    ) {
      throw new NotImplementedException();
    }

    /*==========================================================================================================================
    | PROPERTY: DEFAULT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The default type to return in case <see cref="Lookup(String[])"/> cannot find a match.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"The {nameof(DefaultType)} property has been removed. Fallbacks types can now be added to {nameof(Lookup)} directly.",
      true
    )]
    public Type? DefaultType { get; }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public virtual Type? Lookup(params string[] typeNames) {
      if (typeNames is not null) {
        foreach (var typeName in typeNames) {
          if (Contains(typeName)) {
            return _typeCollection[typeName];
          }
        }
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a <see cref="Type"/> to the underlying collection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected void Add(Type type) => _typeCollection.Add(type);

    /*==========================================================================================================================
    | METHOD: TRY ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="Type"/> with the given <see cref="MemberInfo.Name"/> exists. If it does, returns <c>false
    ///   </c>; otherwise, adds the <see cref="Type"/> and returns <c>true</c>.
    /// </summary>
    protected bool TryAdd(Type type) {
      Contract.Requires(type, nameof(type));
      if (Contains(type.Name)) {
        return false;
      }
      Add(type);
      return true;
    }

    /*==========================================================================================================================
    | METHOD: ADD OR REPLACE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if a <see cref="Type"/> with the given <see cref="MemberInfo.Name"/> exists. If it does, it is replaced.
    ///   Otherwise, it is added.
    /// </summary>
    protected void AddOrReplace(Type type) {
      Contract.Requires(type, nameof(type));
      if (_typeCollection.Contains(type.Name)) {
        Remove(type.Name);
      }
      Add(type);
    }

    /*==========================================================================================================================
    | METHOD: CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the underlying collection has a <see cref="Type"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to located in the collection.</param>
    /// <returns><c>True</c> if the <see cref="Type"/> exists in the collection.</returns>
    [ExcludeFromCodeCoverage]
    protected bool Contains(Type type) => _typeCollection.Contains(type);

    /// <summary>
    ///   Determines if the underlying collection has a <see cref="Type"/> with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the <see cref="Type"/> to located in the collection.</param>
    /// <returns><c>True</c> if a <see cref="Type"/> with <paramref name="key"/> exists in the collection.</returns>
    [ExcludeFromCodeCoverage]
    protected bool Contains(string key) => _typeCollection.Contains(key);

    /*==========================================================================================================================
    | METHOD: REMOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a <see cref="Type"/> with the provided <paramref name="key"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected void Remove(string key) => _typeCollection.Remove(key);

  } //Class
} //Namespace