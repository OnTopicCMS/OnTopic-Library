/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.TestDoubles.LazyLoading;

/*==============================================================================================================================
| CLASS: STUB LAZY LOADING TOPIC REPOSITORY BUILDER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a fluent builder for constructing a custom <see cref="TopicRecord"/> set to seed a <see cref=
///   "StubLazyLoadingTopicRepository"/>, sparing downstream consumers from manually authoring records or rebuilding this
///   scaffolding.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class StubLazyLoadingTopicRepositoryBuilder {

  /*============================================================================================================================
  | CLASS: STAGING RECORD
  \---------------------------------------------------------------------------------------------------------------------------*/
  private sealed class StagingRecord(string key, string contentType, int? parentId) {
    public string Key { get; } = key;
    public string ContentType { get; } = contentType;
    public int? ParentId { get; } = parentId;
    public Dictionary<string, string> IndexedAttributes { get; } = [];
    public Dictionary<string, string> ExtendedAttributes { get; } = [];
    public List<(string Key, int TargetId)> Relationships { get; } = [];
    public List<(string Key, int TargetId)> References { get; } = [];
  }

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Dictionary<int, StagingRecord>  _staging                        = [];
  private readonly              List<int>                       _order                          = [];

  /*============================================================================================================================
  | METHOD: ADD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Adds a new <see cref="TopicRecord"/> to the set being built.
  /// </summary>
  /// <param name="id">The topic's unique identifier.</param>
  /// <param name="key">The topic's key.</param>
  /// <param name="contentType">The topic's content type.</param>
  /// <param name="parentId">The identifier of the topic's parent, or <c>null</c> to attach directly under <c>Root</c>.</param>
  /// <param name="indexedAttributes">Attribute values always present on the topic.</param>
  /// <param name="extendedAttributes">Attribute values only present once the extended attributes are loaded.</param>
  public StubLazyLoadingTopicRepositoryBuilder AddTopic(
    int id,
    string key,
    string contentType,
    int? parentId,
    IReadOnlyDictionary<string, string>? indexedAttributes = null,
    IReadOnlyDictionary<string, string>? extendedAttributes = null
  ) {
    var staging                 = new StagingRecord(key, contentType, parentId);
    if (indexedAttributes is not null) {
      foreach (var attribute in indexedAttributes) {
        staging.IndexedAttributes[attribute.Key] = attribute.Value;
      }
    }
    if (extendedAttributes is not null) {
      foreach (var attribute in extendedAttributes) {
        staging.ExtendedAttributes[attribute.Key] = attribute.Value;
      }
    }
    _staging[id]                = staging;
    _order.Add(id);
    return this;
  }

  /*============================================================================================================================
  | METHOD: ADD RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Adds a relationship from the topic previously added via <see cref="AddTopic"/> under <paramref name="sourceId"/> to
  ///   <paramref name="targetId"/>, under <paramref name="key"/>. The target need not itself be present in the record store:
  ///   An absent target id produces a stale, unresolvable relationship, useful for exercising discard behavior.
  /// </summary>
  public StubLazyLoadingTopicRepositoryBuilder AddRelationship(int sourceId, string key, int targetId) {
    _staging[sourceId].Relationships.Add((key, targetId));
    return this;
  }

  /*============================================================================================================================
  | METHOD: ADD REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Adds a reference from the topic previously added via <see cref="AddTopic"/> under <paramref name="sourceId"/> to
  ///   <paramref name="targetId"/>, under <paramref name="key"/>. The target need not itself be present in the record store:
  ///   An absent target id produces a stale, unresolvable reference, useful for exercising discard behavior.
  /// </summary>
  public StubLazyLoadingTopicRepositoryBuilder AddReference(int sourceId, string key, int targetId) {
    _staging[sourceId].References.Add((key, targetId));
    return this;
  }

  /*============================================================================================================================
  | METHOD: BUILD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Finalizes the set of <see cref="TopicRecord"/>s built so far into an immutable list, suitable for passing to <see cref=
  ///   "StubLazyLoadingTopicRepository(System.Collections.Generic.IEnumerable{OnTopic.TestDoubles.LazyLoading.TopicRecord})"/>.
  /// </summary>
  public IReadOnlyList<TopicRecord> Build() =>
    _order.Select(id         => {
      var staging               = _staging[id];
      return new TopicRecord(
        id,
        staging.Key,
        staging.ContentType,
        staging.ParentId,
        new Dictionary<string, string>(staging.IndexedAttributes),
        new Dictionary<string, string>(staging.ExtendedAttributes),
        staging.Relationships.ToArray(),
        staging.References.ToArray()
      );
    }).ToList();

} //Class