/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Data;
using OnTopic.Data.Sql;
using OnTopic.Repositories;
using OnTopic.TestDoubles.LazyLoading;
using OnTopic.Tests.Schemas;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: FAKE SQL TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   A fake for <see cref="SqlTopicRepository"/> that serves an in-memory row set through the real, production <see cref=
///   "SqlDataReaderExtensions.LoadTopicGraph"/>, exactly as <see cref="SqlTopicRepository"/> feeds it from a live SQL data
///   reader. Every request loads the full ascendant chain from the requested topic to the root, matching production's
///   <c>@LoadAscendants = (topicId &gt;= 0)</c> behavior.
/// </summary>
/// <remarks>
///   Unlike <see cref="OnTopic.TestDoubles.StubTopicRepository"/> and <see cref="StubLazyLoadingTopicRepository"/> stubs that
///   each maintain a single, persistent, already materialized <see cref="Topic"/> graph and simply return existing instances
///   from it, this is a fake: It rebuilds a fresh <see cref="Topic"/> subgraph from its row store on every call, via the same
///   <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> entry point <see cref="SqlTopicRepository"/> uses, and relies on the
///   caller's <c>referenceTopic</c> to reconcile new rows against an already resident graph, exactly as production does. That
///   is the specific mechanism under test in <see cref="CachedTopicRepositoryTest"/>'s <c>referenceTopic ?? _cache</c>
///   regression tests: Neither of the other two doubles can distinguish a <see langword="null"/> <c>referenceTopic</c> from a
///   resident one, since neither ever produces a duplicate instance to reconcile in the first place.
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class FakeSqlTopicRepository : TopicRepository {

  /*============================================================================================================================
  | VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Dictionary<int, (string Key, string ContentType, int? ParentId)> _rows = [];
  private readonly              List<(int SourceId, string Key, int TargetId)> _relationships   = [];
  private readonly              List<(int SourceId, string Key, int TargetId)> _historicalRelationships = [];
  private readonly              Dictionary<string, int>         _keyIndex                       = new(StringComparer.OrdinalIgnoreCase);
  private                       int                             _identity                       = 90000;

  /*============================================================================================================================
  | METHOD: ADD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Registers a row in the fake row store, keyed by <paramref name="id"/>, and indexes its unique key for <see cref=
  ///   "Load(String, Topic?, Boolean, TopicPayload)"/> lookups.
  /// </summary>
  public FakeSqlTopicRepository AddTopic(int id, string key, string contentType, int? parentId) {
    _rows[id]                   = (key, contentType, parentId);
    _keyIndex[GetUniqueKey(id)] = id;
    return this;
  }

  /*============================================================================================================================
  | METHOD: ADD RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Registers a relationship row, returned alongside its source topic's ascendant chain on a subsequent <see cref=
  ///   "Load(Int32, Topic?, Boolean, TopicPayload)"/>.
  /// </summary>
  public void AddRelationship(int sourceId, string key, int targetId) => _relationships.Add((sourceId, key, targetId));

  /*============================================================================================================================
  | METHOD: ADD HISTORICAL RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Registers a relationship row belonging to a historical version, returned independently of the current relationships
  ///   added by <see cref="AddRelationship"/>, and instead returned by a call to <see cref="Load(Int32, DateTime)"/>.
  /// </summary>
  public void AddHistoricalRelationship(int sourceId, string key, int targetId) =>
    _historicalRelationships.Add((sourceId, key, targetId));

  /*============================================================================================================================
  | METHOD: GET UNIQUE KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Computes the unique key for <paramref name="id"/> by walking its <c>ParentId</c> chain through the row store.
  /// </summary>
  private string GetUniqueKey(int id) {
    List<string> segments       = [];
    var current                 = (int?)id;
    while (current is { } currentId) {
      segments.Insert(0, _rows[currentId].Key);
      current                   = _rows[currentId].ParentId;
    }
    return String.Join(":", segments);
  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override async Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  ) {
    if (!_keyIndex.TryGetValue(uniqueKey, out var topicId)) {
      return null;
    }
    return await Load(topicId, referenceTopic, isRecursive, payload).ConfigureAwait(false);
  }

  /// <inheritdoc />
  /// <remarks>
  ///   Builds the requested topic's ascendant chain into fresh <see cref="TopicsDataTable"/>/<see cref="RelationshipsDataTable"
  ///   /> rows on every call, then feeds them through the real <see cref="SqlDataReaderExtensions.LoadTopicGraph"/>—reproducing
  ///   the new instance per call, reconciled against the <c>referenceTopic</c> behavior of <see cref=
  ///   "SqlTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/>.
  /// </remarks>
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  ) {

    // Bypass for rowstore misses
    if (!_rows.ContainsKey(topicId)) {
      return null;
    }

    // Build the ascendant chain, root-first, mirroring production's @LoadAscendants = (topicId >= 0)
    List<int> chain             = [];
    var current                 = (int?)topicId;
    while (current is { } id) {
      chain.Insert(0, id);
      current                   = _rows[id].ParentId;
    }

    // Delegate to the shared graph builder, seeded with the ascendant chain and current relationships
    var topic                   = await LoadTopicGraph(
      topicId,
      referenceTopic,
      populateTopics,
      relationshipRows          : _relationships.Where(r => chain.Contains(r.SourceId))
    ).ConfigureAwait(false);

    // Raise the TopicLoaded event
    OnTopicLoaded(new(topic!, isRecursive));

    // Finally, return the seed topic
    return topic;

    // Populates the ascendant chain's rows into the source data table
    void populateTopics(TopicsDataTable topics) {
      foreach (var id in chain) {
        var (key, contentType, parentId) = _rows[id];
        var hasChildren         = _rows.Values.Any(row => row.ParentId == id);
        topics.AddRow(id, key, contentType, parentId, hasChildren: hasChildren);
      }
    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Unlike <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/>, this builds a single-row <see cref="TopicsDataTable"/>,
  ///   without the ascendant chain, and populated from <see cref="_historicalRelationships"/> rather than <see cref=
  ///   "_relationships"/>, then feeds it through <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with no<c>referenceTopic
  ///   </c>, mirroring production's detached <c>GetTopicVersion</c>: The returned <see cref="Topic"/> has no <see cref=
  ///   "Topic.Parent"/> and no resolved associations, only <c>Deferred</c> entries.
  /// </remarks>
  public override async Task<Topic?> Load(int topicId, DateTime version) {

    // Bypass for rowstore misses
    if (!_rows.TryGetValue(topicId, out var row)) {
      return null;
    }

    var (key, contentType, _)   = row;

    // Delegate to the shared graph builder, seeded with a single disconnected row and no referenceTopic, per the detached
    // contract, alongside historical rather than current relationships
    var topic                   = await LoadTopicGraph(
      topicId,
      referenceTopic            : null,
      populateTopics            : topics => topics.AddRow(topicId, key, contentType),
      relationshipRows          : _historicalRelationships.Where(r => r.SourceId == topicId)
    ).ConfigureAwait(false);

    // Raise the TopicLoaded event
    OnTopicLoaded(new(topic!, false, version));

    // Finally, return the detached topic
    return topic;

  }

  /*============================================================================================================================
  | METHOD: LOAD TOPIC GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Shared core behind <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/> and <see cref="Load(Int32, DateTime)"/>:
  ///   Builds a fresh set of <see cref="TopicsDataTable"/> and <see cref="RelationshipsDataTable"/> rows, then feeds them
  ///   through the real <see cref="SqlDataReaderExtensions.LoadTopicGraph"/>, exactly as <see cref="SqlTopicRepository"/> does
  ///   from a live reader.
  /// </summary>
  /// <param name="topicId">The <see cref="Topic.Id"/> to seed the load with.</param>
  /// <param name="referenceTopic">The reference topic graph to reconcile new rows against, if any.</param>
  /// <param name="populateTopics">Adds whatever topic row(s) the caller's scenario requires to the source data table.</param>
  /// <param name="relationshipRows">The relationship rows to feed alongside the topic row(s).</param>
  private static async Task<Topic?> LoadTopicGraph(
    int topicId,
    Topic? referenceTopic,
    Action<TopicsDataTable> populateTopics,
    IEnumerable<(int SourceId, string Key, int TargetId)> relationshipRows
  ) {

    // Define source data tables
    using var topics            = new TopicsDataTable();
    using var attributes        = new AttributesDataTable();
    using var extendedAttributes = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    // Build the topic data
    populateTopics(topics);

    // Build the relationship data
    foreach (var (sourceId, key, targetId) in relationshipRows) {
      relationships.AddRow(sourceId, key, targetId, isDeleted: false);
    }

    // Establish data table reader, which simulates the return from the GetTopics stored procedure
    using var tableReader       = new DataTableReader([topics, attributes, extendedAttributes, relationships]);

    // Delegate to the standard LoadTopicGraph from the SQL provider
    return await tableReader.LoadTopicGraph(
      topicId,
      referenceTopic,
      cancellationToken         : CancellationToken.None
    ).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHOD: REFRESH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public override Task Refresh(Topic referenceTopic, DateTime since) => Task.CompletedTask;

  /*============================================================================================================================
  | METHOD: SAVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override Task SaveTopic(Topic topic, DateTime version, bool persistRelationships) {
    if (topic.IsNew) {
      topic.Id                  = _identity++;
    }
    return Task.CompletedTask;
  }

  /*============================================================================================================================
  | METHOD: MOVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override Task MoveTopic(Topic topic, Topic target, Topic? sibling = null) => Task.CompletedTask;

  /*============================================================================================================================
  | METHOD: DELETE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override Task DeleteTopic(Topic topic) => Task.CompletedTask;

} //Class