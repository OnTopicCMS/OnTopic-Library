/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Mapping.Annotations {

  /*============================================================================================================================
  | ENUM: ASSOCIATION TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Allows one or more associations types to be specified.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="TopicMappingService"/> and <see cref="IncludeAttribute"/> use the <see cref="AssociationTypes"/> enum to
  ///     determine what associations should be mapped—or followed—as part of the mapping process. This helps constrain the
  ///     scope of the object graph to only include the data needed for a given view, or vice verse. That said, the <see cref="
  ///     AssociationTypes"/> enum can be used any place where the code needs to model multiple types of associations relevant
  ///     to the <see cref="Topic"/> class and its view models.
  ///   </para>
  ///   <para>
  ///     The<see cref="AssociationTypes"/> enum is <i>similar</i> to the<see cref="CollectionType"/> enum—and, in fact, they
  ///     share several members. They differ in that <see cref="CollectionType"/> <i>exclusively</i> models <i>collections</i>,
  ///     whereas <see cref="AssociationTypes"/> also models other types of associations, such as <see cref="Topic.Parent"/>,
  ///     and <see cref="AssociationTypes"/> allows <i>multiple</i> associations to be selected.
  ///   </para>
  /// </remarks>
  [Flags]
  public enum AssociationTypes {

    /*==========================================================================================================================
    | NONE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Do not follow any associations.
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
    ///   Map <see cref="Topic.Children"/> references, or properties marked as <see cref="CollectionType.Children"/>.
    /// </summary>
    Children                    = 1 << 1,

    /*==========================================================================================================================
    | RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.Relationships"/> references, or properties marked as <see cref="CollectionType.Relationship"/>.
    /// </summary>
    Relationships               = 1 << 2,

    /*==========================================================================================================================
    | INCOMING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map <see cref="Topic.IncomingRelationships"/> references, or properties marked as <see cref="CollectionType.
    ///   IncomingRelationship"/>.
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
    ///   Map topic pointer references, such as <see cref="Topic.BaseTopic"/>.
    /// </summary>
    /// <remarks>
    ///   By convention, <see cref="References"/> types refer to a <see cref="AttributeDescriptor"/>, <see cref="
    ///   AttributeKeyAttribute.Key"/>, or property identifier ending in <c>Id</c>.
    /// </remarks>
    References                  = 1 << 5,

    /*==========================================================================================================================
    | ALL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Map all association types.
    /// </summary>
    All = Parents | Children | Relationships | IncomingRelationships | MappedCollections | References

  } //Enum
} //Namespace