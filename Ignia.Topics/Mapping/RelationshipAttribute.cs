/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | ATTRIBUTE: RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides the <see cref="ITopicMappingService"/> with instructions as to which key to follow for the relationship.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     By default, <see cref="ITopicMappingService"/> implementations will attempt to map a collection property to the first
  ///     relationship it finds with a key that matches the property name. For example, if a property is named "Categories" it
  ///     will first look in <see cref="Topic.Relationships"/> for a relationship named "Categories", then it will search <see
  ///     cref="Topic.Children"/> for a set of nested topics, and finally <see cref="Topic.IncomingRelationships"/>.
  ///   </para>
  ///   <para>
  ///     This attribute instructs the <see cref="ITopicMappingService"/> to instead look for a specified key. This allows the
  ///     target property name to be decoupled from the source's relationship key. In addition, this attribute can be used to
  ///     specify the type of relationship expected, which is useful if there might be ambiguity between relationship names (for
  ///     example, if there is a <see cref="Topic.Relationships"/> with the same key as an <see
  ///     cref="Topic.IncomingRelationships"/>).
  ///   </para>
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Property)]
  public sealed class RelationshipAttribute : System.Attribute {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing an <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key value of the relationships associated with the current property.</param>
    public RelationshipAttribute(string key) {
      TopicFactory.ValidateKey(key, false);
      Key = key;
    }

    /// <summary>
    ///   Annotates a property with the <see cref="RelationshipAttribute"/> by providing the <see cref="RelationshipType"/>.
    /// </summary>
    /// <param name="type">Optional. The type of collection the relationship is associated with.</param>
    public RelationshipAttribute(RelationshipType type = RelationshipType.Any) {
      Type = type;
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the relationship key.
    /// </summary>
    public string? Key { get; }

    /*==========================================================================================================================
    | PROPERTY: TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the value of the relationship type.
    /// </summary>
    public RelationshipType Type { get; set; }

  } //Class

} //Namespace
