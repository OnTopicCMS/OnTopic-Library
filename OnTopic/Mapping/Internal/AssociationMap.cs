/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Internal;

/*==============================================================================================================================
| CLASS: ASSOCIATION MAP
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a mapping of the relationship between <see cref="CollectionType"/> and <see cref="AssociationTypes"/>.
/// </summary>
/// <remarks>
///   While the <see cref="CollectionType"/> and <see cref="AssociationTypes"/> enumerations are distinct, there are times
///   when a single <see cref="CollectionType"/> needs to be related to an item in the collection of <see cref="
///   AssociationTypes"/>. This mapping makes that feasible.
/// </remarks>
static internal class AssociationMap {

  /*============================================================================================================================
  | CONSTRUCTOR (STATIC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  static AssociationMap() {

    var mappings                = new Dictionary<CollectionType, AssociationTypes> {
      { CollectionType.Any, AssociationTypes.None },
      { CollectionType.Children, AssociationTypes.Children },
      { CollectionType.Relationship, AssociationTypes.Relationships },
      { CollectionType.NestedTopics, AssociationTypes.None },
      { CollectionType.MappedCollection, AssociationTypes.MappedCollections },
      { CollectionType.IncomingRelationship, AssociationTypes.IncomingRelationships }
    };

    // Any probes Relationships first, then Children (via NestedTopics); both must be warmed before probing
    // IncomingRelationship cannot be warmed for a single topic, and MappedCollection is property-based
    var payloadMappings         = new Dictionary<CollectionType, TopicPayload> {
      { CollectionType.Any, TopicPayload.Children | TopicPayload.Relationships },
      { CollectionType.Children, TopicPayload.Children },
      { CollectionType.Relationship, TopicPayload.Relationships },
      { CollectionType.NestedTopics, TopicPayload.Children },
      { CollectionType.MappedCollection, TopicPayload.None },
      { CollectionType.IncomingRelationship, TopicPayload.None }
    };

    Mappings                    = mappings;
    PayloadMappings             = payloadMappings;

  }

  /*============================================================================================================================
  | PROPERTY: MAPPINGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  static internal Dictionary<CollectionType, AssociationTypes> Mappings { get; }

  /*============================================================================================================================
  | PROPERTY: PAYLOAD MAPPINGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a mapping of the relationship between <see cref="CollectionType"/> and <see cref="TopicPayload"/>.
  /// </summary>
  /// <remarks>
  ///   Used by <see cref="TopicMappingService"/> to determine which lazy-load payloads to warm before probing collections.
  /// </remarks>
  static internal Dictionary<CollectionType, TopicPayload> PayloadMappings { get; }

} //Class