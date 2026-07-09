/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Mapping.Annotations;

/*==============================================================================================================================
| ATTRIBUTE: RELATIONSHIP
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <inheritdoc cref="CollectionAttribute"/>
[ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Property)]
[Obsolete($"The {nameof(RelationshipAttribute)} has been renamed to {nameof(CollectionAttribute)}.", true)]
public sealed class RelationshipAttribute : Attribute {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing an <paramref name="key"/>.
  /// </summary>
  /// <param name="key">The key value of the collection associated with the current property.</param>
  public RelationshipAttribute(string key) {
    TopicFactory.ValidateKey(key);
    Key                         = key;
  }

  /// <summary>
  ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing the <see cref="RelationshipType"/>.
  /// </summary>
  /// <param name="type">Optional. The type of collection the collection is associated with.</param>
  public RelationshipAttribute(RelationshipType type = RelationshipType.Any) {
    Type                        = type;
  }

  /*============================================================================================================================
  | PROPERTY: KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets the value of the collection key.
  /// </summary>
  public string? Key { get; }

  /*============================================================================================================================
  | PROPERTY: TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets the value of the <see cref="CollectionType"/>.
  /// </summary>
  [SuppressMessage("Design", "CA1019:Define accessors for attribute arguments", Justification = "Retained for symmetry with CollectionAttribute; this class is marked [Obsolete(error: true)] and cannot be used in compiled code.")]
  public RelationshipType Type  { get; set; }

} //Class