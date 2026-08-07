/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.TestDoubles.LazyLoading;

/*==============================================================================================================================
| RECORD: TOPIC RECORD
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a single topic within <see cref="StubLazyLoadingTopicRepository"/>'s flat, SQL-free record store, from which
///   shallow <see cref="Topic"/> instances are built and lazily materialized.
/// </summary>
/// <param name="Id">The topic's unique identifier.</param>
/// <param name="Key">The topic's key.</param>
/// <param name="ContentType">The topic's content type.</param>
/// <param name="ParentId">
///   The identifier of the topic's parent, or <c>null</c> if the topic is a top-level "content" topic, attached directly under
///   <c>Root</c>.
/// </param>
/// <param name="IndexedAttributes">Attribute values always present on the topic, regardless of requested payload.</param>
/// <param name="ExtendedAttributes">Attribute values only merged when the extended-attribute property is materialized.</param>
/// <param name="Relationships">Relationship key/id pairs, deferred until the relationship property is materialized.</param>
/// <param name="References">Reference key/id pairs, deferred until the reference property is materialized.</param>
[ExcludeFromCodeCoverage]
public sealed record TopicRecord(
  int Id,
  string Key,
  string ContentType,
  int? ParentId,
  IReadOnlyDictionary<string, string> IndexedAttributes,
  IReadOnlyDictionary<string, string> ExtendedAttributes,
  IReadOnlyList<(string Key, int TargetId)> Relationships,
  IReadOnlyList<(string Key, int TargetId)> References
);