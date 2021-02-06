/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE ENTRY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an entry to tracking an object mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   In addition to the actual <see cref="MappedTopic"/>, this also includes a <see cref="Relationships"/> property for
  ///   tracking what relationships were mapped to the <see cref="MappedTopic"/>. This allows the <see cref=
  ///   "TopicMappingService"/> to be update the cached object with any missing relationships, which can be identified using the
  ///   <see cref="GetMissingRelationships(AssociationTypes)"/> method. In turn, the cache can then be updated to reflect those
  ///   new relationships by using <see cref="AddMissingRelationships(AssociationTypes)"/>. This ensures that even if a topic
  ///   has already been mapped, its scope can be expanded without duplicating effort.
  /// </remarks>
  public class MappedTopicCacheEntry {

    /*==========================================================================================================================
    | PROPERTY: MAPPED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the mapped object.
    /// </summary>
    public object MappedTopic { get; set; } = null!;

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the relationships that the <see cref="MappedTopic"/> was mapped with.
    /// </summary>
    public AssociationTypes Relationships { get; set; } = AssociationTypes.None;

    /*==========================================================================================================================
    | METHOD: GET MISSING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target <paramref name="relationships"/>, identifies any relationships not covered by <see cref="Relationships"
    ///   /> and returns them as a new <see cref="OnTopic.Mapping.Annotations.AssociationTypes"/> instance.
    /// </summary>
    public AssociationTypes GetMissingRelationships(AssociationTypes relationships) => Relationships ^ (relationships | Relationships);

    /*==========================================================================================================================
    | METHOD: ADD MISSING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target <paramref name="relationships"/>, adds any missing <see cref="OnTopic.Mapping.Annotations.
    ///   AssociationTypes"/> to the <see cref="Relationships"/> property.
    /// </summary>
    public void AddMissingRelationships(AssociationTypes relationships) => Relationships = relationships | Relationships;

  } //Class
} //Namespace