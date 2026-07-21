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
/// <param name="IsDirty">
///   Whether the association should be marked as dirty once resolved, so that <see cref="ITopicRepository.Save(Topic, Boolean)"
///   /> persists it. Defaults to <c>false</c>, which is appropriate for associations recorded during an ordinary load, where
///   the target is presumed to already reflect the persistence store and resolving it later shouldn't be treated as a pending
///   change. Associations representing a not-yet-persisted change are marked <c>true</c>, such as those transplanted by <see
///   cref="DeferredAssociationCollection.ReplaceAll(IEnumerable{DeferredAssociation})"/>.
/// </param>
public record DeferredAssociation(string Key, int TopicId, bool IsDirty = false);