/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Allows one or more relationship types to be specified.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="TopicMappingService"/> and <see cref="FollowAttribute"/> use the <see cref="Relationships"/> enum to
  ///     determine what relationships should be mapped—or followed as part of the mapping process. This helps constrain the
  ///     scope of the object graph to only include the data needed for a given view, or vice verse. That said, the <see
  ///     cref="Relationships"/> enum can be used any place where the code needs to model multiple types of relationships
  ///     relevant to the <see cref="Topic"/> class and its view models.
  ///   </para>
  ///   <para>
  ///     This differs from <see cref="RelationshipType"/>, which only allows <i>one</i> relationship to be specified.
  ///   </para>
  /// </remarks>
  [Flags]
  public enum Relationships {

    /*==========================================================================================================================
    | NONE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Do not follow any relationships.
    /// </summary>
    None                        = 0,

    /*==========================================================================================================================
    | PARENTS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.Parent"/> references.
    /// </summary>
    Parents                     = 1,

    /*==========================================================================================================================
    | CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.Children"/> references, or properties marked as <see cref="RelationshipType.Children"/>.
    /// </summary>
    Children                    = 1 << 1,

    /*==========================================================================================================================
    | RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.Relationships"/> references, or properties marked as <see cref="RelationshipType.Relationship"/>.
    /// </summary>
    Relationships               = 1 << 2,

    /*==========================================================================================================================
    | INCOMING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.IncomingRelationships"/> references, or properties marked as <see
    ///   cref="RelationshipType.IncomingRelationship"/>.
    /// </summary>
    IncomingRelationships       = 1 << 3,

    /*==========================================================================================================================
    | MAPPED COLLECTIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map to any collection on the <see cref="Topic"/> with either the same property name, or which corresponds to the <see
    ///   cref="AttributeKeyAttribute.Key"/>.
    /// </summary>
    /// <remarks>
    ///   This allows mapping of custom collection, such as <see cref="ContentTypeDescriptor.AttributeDescriptors"/>.
    /// </remarks>
    MappedCollections           = 1 << 4,

    /*==========================================================================================================================
    | REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map topic pointer references, such as <see cref="Topic.DerivedTopic"/>.
    /// </summary>
    /// <remarks>
    ///   By convention, <see cref="References"/> types refer to a <see cref="AttributeDescriptor"/>, <see
    ///   cref="AttributeKeyAttribute.Key"/>, or property identifier ending in <c>Id</c>.
    /// </remarks>
    References                  = 1 << 5,

    /*==========================================================================================================================
    | ALL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map all relationship types.
    /// </summary>
    All = Parents | Children | Relationships | IncomingRelationships | MappedCollections | References

  } //Enum
} //Namespace