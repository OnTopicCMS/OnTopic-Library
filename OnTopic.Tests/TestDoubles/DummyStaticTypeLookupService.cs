/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Lookup {

  /*============================================================================================================================
  | CLASS: DUMMY STATIC TYPE LOOKUP SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="DummyStaticTypeLookupService"/> provides a wrapper around the <see cref="StaticTypeLookupService"/> in
  ///   order to relay access to protected members for the purpose of testing. The <see cref="DummyStaticTypeLookupService"/>
  ///   doesn't implement any functionality of its own; it's just a pass-through wrapper.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class DummyStaticTypeLookupService: StaticTypeLookupService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DummyStaticTypeLookupService"/>. Optionally accepts a list of <see cref="
    ///   Type"/> instances and a default <see cref="Type"/> value.
    /// </summary>
    /// <param name="types">The list of <see cref="Type"/> instances to expose as part of this service.</param>
    public DummyStaticTypeLookupService(
      IEnumerable<Type>? types = null
    ): base(types) {
    }

    /*==========================================================================================================================
    | METHOD: LOOKUP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override Type? Lookup(params string[] typeNames) => base.Lookup(typeNames);

    /*==========================================================================================================================
    | METHOD: ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public new void Add(Type type) => base.Add(type);

    /*==========================================================================================================================
    | METHOD: TRY ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public new bool TryAdd(Type type) => base.TryAdd(type);

    /*==========================================================================================================================
    | METHOD: ADD OR REPLACE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public new void AddOrReplace(Type type) => base.AddOrReplace(type);

    /*==========================================================================================================================
    | METHOD: CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public new bool Contains(Type type) => base.Contains(type);

    /// <inheritdoc/>
    public new bool Contains(string key) => base.Contains(key);

    /*==========================================================================================================================
    | METHOD: REMOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public new void Remove(string key) => base.Remove(key);

  } //Class
} //Namespace