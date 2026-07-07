/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Associations;

/*==============================================================================================================================
| RECORD: DEFERRED ASSOCIATION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a deferred association between a source <see cref="Topic"/> and a target topic that could not be resolved to an
///   in-memory instance when loaded.
/// </summary>
/// <remarks>
///   This is exposed via <see cref="TopicRelationshipMultiMap.Deferred"/> and <see cref="TopicReferenceCollection.Deferred"/>
///   so that the <see cref="ITopicRepository"/> can record missing associations during a load, and then can dynamically load
///   them later when the collection is called.

/// </remarks>
/// <param name="Key">The relationship or reference key under which the association is registered.</param>
/// <param name="TopicId">The <see cref="Topic.Id"/> of the target topic to be resolved.</param>
public record DeferredAssociation(string Key, int TopicId);