/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.TestDoubles.LazyLoading;

/*==============================================================================================================================
| CLASS: STUB LAZY TOPIC DATA REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a lazy-loading implementation of an <see cref="ITopicRepository"/>, serving partially loaded topics with the
///   ability to be dynamically filled by e.g., <see cref="ITopicLoadResolver.EnsureLoaded"/>.
/// </summary>
/// <remarks>
///   <para>
///     Unlike <see cref="StubTopicRepository"/>, which serves a fully materialized graph from the outset, this double
///     builds each <see cref="Topic"/> shallow: Its own <see cref="Repositories.TopicPayload.Children"/>, <see cref=
///     "Repositories.TopicPayload.ExtendedAttributes"/>, and association properties are absent until something fills them, one
///     at a time, from the record store.
///   </para>
///   <para>
///     Filling happens two ways, matching how a real, e.g., SQL-backed repository distinguishes a batch <c>Load()</c> from an
///     on-demand fill. <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/> and its overloads only connect association
///     targets already present in the graph being built, leaving the rest deferred; it never issues an additional fetch to
///     resolve a missing target. <see cref="EnsureLoaded(Topic, TopicPayload, CancellationToken)"/>, invoked either explicitly
///     or via one of <see cref="Topic"/>'s autoloading getters, goes further: It recursively loads whatever deferred targets it
///     can find and discards the rest as stale.
///   </para>
///   <para>
///     The <c>Root:Configuration</c> subtree (required by <see cref="Data.Caching.CachedTopicRepository"/> for content type
///     resolution) is the one exception to lazy service: It is built eagerly, with every property <see cref="LoadState.Loaded"
///     />, exactly as production seeds it. Everything else is served lazily from the record store supplied to the constructor,
///     or from <see cref="CreateDefaultRecords"/> if none is supplied.
///   </para>
///   <para>
///     A per-topic, per-property fetch-count spy (<see cref="GetFetchCount(Int32, TopicPayload)"/> and <see cref="TotalFetches"
///     />) records every fill, letting tests assert fetch-once behavior and, critically, that stamping the resolver doesn't
///     trigger lazy loading.
///   </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public class StubLazyLoadingTopicRepository : TopicRepository, ITopicRepository, ITopicLoadResolver {

  /*============================================================================================================================
  | VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Topic                           _root;
  private readonly              Dictionary<int, Topic>          _served                         = [];
  private readonly              Dictionary<string, int>         _keyIndex                       = new(StringComparer.OrdinalIgnoreCase);
  private                       int                             _identity                       = 90000;

  private readonly              IReadOnlyDictionary<int, TopicRecord> _store;
  private readonly              Dictionary<(int TopicId, TopicPayload Boundary), int> _fetchCounts = [];

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instantiates a new instance of the <see cref="StubLazyLoadingTopicRepository"/> using the default, built-in seed dataset.
  /// </summary>
  public StubLazyLoadingTopicRepository() : this(CreateDefaultRecords()) { }

  /// <summary>
  ///   Instantiates a new instance of the <see cref="StubLazyLoadingTopicRepository"/> using a custom set of <see cref=
  ///   "TopicRecord"/>s, allowing downstream consumers (e.g., the OnTopic Editor) to seed their own lazy-capable content graph
  ///   without rebuilding this scaffolding. See <see cref="StubLazyLoadingTopicRepositoryBuilder"/> for a convenient way to construct
  ///   <paramref name="records"/>.
  /// </summary>
  /// <param name="records">The flat, SQL-free record store from which the "content" subtree is lazily served.</param>
  public StubLazyLoadingTopicRepository(IEnumerable<TopicRecord> records) {

    Contract.Requires(records, nameof(records));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Eagerly build the Root:Configuration scaffold; every boundary defaults to Loaded, since it is never marked otherwise
    \-------------------------------------------------------------------------------------------------------------------------*/
    _root                       = BuildEagerScaffold();

    foreach (var topic in _root.FindAll()) {
      _served[topic.Id]         = topic;
      _keyIndex[topic.GetUniqueKey()] = topic.Id;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Index the lazy content record store
    \-------------------------------------------------------------------------------------------------------------------------*/
    _store                      = records.ToDictionary(record => record.Id);

    foreach (var record in _store.Values) {
      _keyIndex[GetUniqueKey(record)] = record.Id;
    }

  }

  /*============================================================================================================================
  | METHOD: BUILD EAGER SCAFFOLD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Builds the minimal <c>Root:Configuration</c> scaffold that <see cref="Data.Caching.CachedTopicRepository"/> requires
  ///   for construction (content-type resolution) and that <see cref="TopicRepository"/> requires for <c>Save()</c>
  ///   (content-type validation).
  /// </summary>
  private static Topic BuildEagerScaffold() {

    var root                    = new Topic("Root", "Container", null, 1);
    var configuration           = new Topic("Configuration", "Container", root, 2);
    var contentTypes            = new ContentTypeDescriptor("ContentTypes", "ContentTypeDescriptor", configuration, 3);
    _                           = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", contentTypes, 4);

    return root;

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

    // Validate unique key
    if (String.IsNullOrEmpty(uniqueKey)) {
      return null;
    }

    // Normalize unique key by ensuring "root:" prefix
    if (!uniqueKey.StartsWith(_root.Key, StringComparison.OrdinalIgnoreCase)) {
      uniqueKey                 = $"{_root.Key}:{uniqueKey.TrimStart(':')}";
    }

    // If the root is requested, hardcode the topicId at -1
    if (uniqueKey.Equals(_root.Key, StringComparison.OrdinalIgnoreCase)) {
      return await Load(-1, referenceTopic, isRecursive, payload).ConfigureAwait(false);
    }

    // If the unique key isn't in the data store, return null
    if (!_keyIndex.TryGetValue(uniqueKey, out var topicId)) {
      return null;
    }

    // Otherwise, use the store's topicId to call the base overload
    return await Load(topicId, referenceTopic, isRecursive, payload).ConfigureAwait(false);

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Returns an already-<see cref="_served"/> topic, without raising <see cref="ObservableTopicRepository.OnTopicLoaded"/>
  ///   again: The event fires only when a topic is genuinely built for the first time, mirroring how a real repository only
  ///   fires when something is actually pulled from the persistence store. On a miss against the record store, builds the
  ///   requested topic and its ancestor chain as shallow, sparse topics, and raises the event for the requested topic. Either
  ///   way, if <paramref name="payload"/> requests anything not yet loaded, it connects whatever it can from the graph already
  ///   built so far via <see cref="FillRequestedPayload"/>; targets that aren't yet resident stay deferred.
  /// </remarks>
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  ) {

    // Setup
    var topic                   = (Topic?)null;
    var isNewlyBuilt            = false;

    // Attempt to retrieve topic, falling back to the topic store if needed
    if (topicId < 0) {
      topic                     = _root;
    }
    else if (_served.TryGetValue(topicId, out var existing)) {
      topic                     = existing;
    }
    else if (_store.TryGetValue(topicId, out var record)) {
      topic                     = BuildTopicWithAncestors(record);
      isNewlyBuilt              = true;
    }
    else {
      return null;
    }

    // Preload the topic with the requested payload
    await FillRequestedPayload(topic, payload, resolveDeferredTargets: false, CancellationToken.None).ConfigureAwait(false);

    // Fire the TopicLoaded event, if newly built
    if (isNewlyBuilt) {
      OnTopicLoaded(new(topic, isRecursive));
    }

    // Return the requested topic
    return topic;

  }

  /// <inheritdoc />
  public override async Task<Topic?> Load(int topicId, DateTime version, Topic? referenceTopic = null) {

    // Setup
    Contract.Requires(version.Date < DateTime.UtcNow, "The version requested must be a valid historical date.");
    Contract.Requires(
      version.Date > new DateTime(2014, 12, 9),
      "The version is expected to have been created since version support was introduced into the topic library."
    );

    // Load the topic requested
    var topic                   = await Load(topicId, referenceTopic).ConfigureAwait(false);

    // Throw an exception if the topic doesn't exist
    if (topic is null) {
      throw new TopicNotFoundException(topicId);
    }

    // For the stub, accept whatever version is provided
    if (!topic.VersionHistory.Contains(version)) {
      topic.VersionHistory.Add(version);
    }
    topic.LastModified          = version;

    // Fire the TopicLoaded event; this is always assumed to be freshly loaded
    OnTopicLoaded(new(topic, false, version));

    // Return the topic version
    return topic;

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
  protected override Task SaveTopic([NotNull]Topic topic, DateTime version, bool persistRelationships) {

    // For saving topics, just assign an identity
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

  /*============================================================================================================================
  | METHODS: TOPIC LOAD RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  /// <remarks>
  ///   The on-demand fill: Unlike <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/>, this recursively resolves deferred
  ///   relationship and reference targets via the inherited <c>ResolveDeferredAssociations</c>, discarding whatever remains
  ///   unresolved as stale, assuming either <see cref="TopicPayload.Relationships"/> or <see cref="TopicPayload.References"/>.
  /// </remarks>
  public virtual async Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default) {

    // Validate the input
    Contract.Requires(topic, nameof(topic));

    // There's nothing to load on a new topic
    if (topic.IsNew) {
      return;
    }

    // Call the centralized private helper to fulfill the request
    await FillRequestedPayload(topic, payload, resolveDeferredTargets: true, cancellationToken).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHOD: FILL REQUESTED PAYLOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   On a plain <c>Load()</c>, connects resident relationship and reference targets unconditionally, regardless of
  ///   <paramref name="payload"/>. Either way, loads the data requested in <paramref name="payload"/>, after filtering out
  ///   any already <see cref="LoadState.Loaded"/> flags, recording a fetch in the spy for each property filled. Children are
  ///   fetched from the record store one level at a time; a child already present in <see cref="_served"/> (e.g., attached
  ///   while building an ancestor chain for a deeper <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/> call) is
  ///   reused rather than rebuilt, to avoid colliding with the existing instance already attached to the graph.
  /// </summary>
  /// <param name="topic">The topic whose requested payload should be filled.</param>
  /// <param name="payload">The requested <see cref="TopicPayload"/> flags.</param>
  /// <param name="resolveDeferredTargets">
  ///   Whether unresolved relationships and references targets should be recursively loaded, via the inherited <c>
  ///   ResolveDeferredAssociations</c>. Set by <see cref="EnsureLoaded(Topic, TopicPayload, CancellationToken)"/>'s on-demand
  ///   fill; left <see langword="false"/> by a plain <c>Load()</c>, which only connects targets already present in the graph,
  ///   via <see cref="ConnectResidentAssociations"/>.
  /// </param>
  /// <param name="cancellationToken">An optional token used only when resolving deferred targets.</param>
  private async Task FillRequestedPayload(
    Topic topic,
    TopicPayload payload,
    bool resolveDeferredTargets,
    CancellationToken cancellationToken
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Setup
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rawTopic                = (ITopicBackingAccessor)topic;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Relationships and references: Connect resident targets
    >-------------------------------------------------------------------------------------------------------------------------
    | Unconditionally connects targets already present in the graph, mirroring how LoadTopicGraph() reads relationship and
    | reference rows alongside every topic row and links whatever's already resident, regardless of the requested payload.
    | Runs ahead of the payload/store checks below, since it isn't gated by them in production either.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!resolveDeferredTargets) {
      ConnectResidentAssociations(rawTopic);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Filter out any already loaded payloads
    \-------------------------------------------------------------------------------------------------------------------------*/
    payload                     = topic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return;
    }

    // Filters out just the association payloads, if present, for a later gate
    var associationPayload      = payload & (TopicPayload.Relationships | TopicPayload.References);

    /*--------------------------------------------------------------------------------------------------------------------------
    | A topic with pending payload must be backed by a record
    >---------------------------------------------------------------------------------------------------------------------------
    | A topic that has no corresponding record means it was attached to the graph without ever being built from the store, which
    | this stub has no way to fulfill; this represents test setup error, not a legitimate state
    \-------------------------------------------------------------------------------------------------------------------------*/
    _store.TryGetValue(topic.Id, out var record);
    Contract.Assume(
      record,
      $"{nameof(StubLazyLoadingTopicRepository)} can only lazily fill topics that are defined in the record store supplied to its " +
      $"constructor. Topic {topic.Id} was attached to the graph with a pending {payload} payload, but has no corresponding " +
      $"record to fill it from. This is an invalid configuration."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Children
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (payload.HasFlag(TopicPayload.Children)) {

      // Loop through each child record and build the topic from the topic store
      foreach (var childRecord in _store.Values.Where(r => r.ParentId == topic.Id).OrderBy(r => r.Id)) {

        // Build the child record, assuming it hasn't already been served
        if (_served.ContainsKey(childRecord.Id)) {
          continue;
        }
        var child               = BuildTopic(childRecord, topic);

        // Load the rest of the requested payload for the child, mirroring how a Children fetch also pulls in whatever else was
        // requested (e.g., ExtendedAttributes, VersionHistory) for the whole scope, while relationships and references always
        // ride along for free; the child's own Children are left deferred
        var childPayload        = (payload & ~TopicPayload.Children) | TopicPayload.Relationships | TopicPayload.References;
        await FillRequestedPayload(child, childPayload, resolveDeferredTargets: false, cancellationToken).ConfigureAwait(false);

        // Fire the TopicLoaded event
        OnTopicLoaded(new(child, isRecursive: false));

      }

      // Mark the children as fetched and loaded
      RecordFetch(topic.Id, TopicPayload.Children);
      topic.SetLoadState(TopicPayload.Children, LoadState.Loaded);

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Extended attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (payload.HasFlag(TopicPayload.ExtendedAttributes)) {

      // Load each of the extended attributes from the data store
      foreach (var attribute in record.ExtendedAttributes) {
        rawTopic.Attributes.SetValue(attribute.Key, attribute.Value, markDirty: false, isExtendedAttribute: true);
      }

      // Mark the extended attributes as fetched and loaded
      RecordFetch(topic.Id, TopicPayload.ExtendedAttributes);
      topic.SetLoadState(TopicPayload.ExtendedAttributes, LoadState.Loaded);

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Relationships and references: Resolve deferred targets
    >-------------------------------------------------------------------------------------------------------------------------
    | Gated on what was actually requested, and delegates to the inherited ResolveDeferredAssociations, so this double
    | utilizes that shared infrastructure. Resident targets were already connected above, regardless of this gate.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (resolveDeferredTargets && associationPayload is not TopicPayload.None) {

      // Load any deferred relationships or references
      await ResolveDeferredAssociations(topic, associationPayload, cancellationToken).ConfigureAwait(false);

      // Mark the associations requested as fetched
      if (associationPayload.HasFlag(TopicPayload.Relationships)) {
        RecordFetch(topic.Id, TopicPayload.Relationships);
      }
      if (associationPayload.HasFlag(TopicPayload.References)) {
        RecordFetch(topic.Id, TopicPayload.References);
      }

    }

  }

  /*============================================================================================================================
  | METHOD: CONNECT RESIDENT ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Connects each deferred relationship or reference entry whose target is already present in <see cref="_served"/>, without
  ///   recursively loading anything. Mirrors how a real, SQL-backed repository connects association targets found within the
  ///   same result set as the requested topic, leaving out-of-scope targets deferred for a later, explicit fill via <see cref=
  ///   "EnsureLoaded(Topic, TopicPayload, CancellationToken)"/>.
  /// </summary>
  /// <param name="rawTopic">The requesting topic's backing accessor.</param>
  private void ConnectResidentAssociations(ITopicBackingAccessor rawTopic) {

    // Attempts to resolve each deferred relationship
    foreach (var entry in rawTopic.Relationships.Deferred.ToArray()) {
      if (_served.TryGetValue(entry.TopicId, out var target)) {
        rawTopic.Relationships.SetValue(entry.Key, target, markDirty: false);
      }
    }

    // Attempts to resolve each deferred reference
    foreach (var entry in rawTopic.References.Deferred.ToArray()) {
      if (_served.TryGetValue(entry.TopicId, out var target)) {
        rawTopic.References.SetValue(entry.Key, target, markDirty: false);
      }
    }

  }

  /*============================================================================================================================
  | METHOD: BUILD TOPIC WITH ANCESTORS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Builds the requested <paramref name="record"/> as a shallow, served <see cref="Topic"/>, first building (or reusing)
  ///   its entire ancestor chain up to <see cref="_root"/>.
  /// </summary>
  private Topic BuildTopicWithAncestors(TopicRecord record) {
    var parent                  = record.ParentId is { } parentId ? GetOrBuildAncestor(parentId) : _root;
    return BuildTopic(record, parent);
  }

  /*============================================================================================================================
  | METHOD: GET OR BUILD ANCESTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the already-<see cref="_served"/> ancestor for <paramref name="topicId"/>, or builds it and, recursively, its
  ///   own ancestors, as a shallow topic. Never raises <see cref="ObservableTopicRepository.OnTopicLoaded"/>: Only the
  ///   originally requested topic does that; ascendants are stamped separately, via <c>StampAscendants</c>.
  /// </summary>
  private Topic GetOrBuildAncestor(int topicId) {
    if (_served.TryGetValue(topicId, out var existing)) {
      return existing;
    }
    var record                  = _store[topicId];
    var parent                  = record.ParentId is { } parentId ? GetOrBuildAncestor(parentId) : _root;
    return BuildTopic(record, parent);
  }

  /*============================================================================================================================
  | METHOD: BUILD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Constructs a single shallow <see cref="Topic"/> for <paramref name="record"/> under <paramref name="parent"/>, populates
  ///   its indexed attributes, marks its extended attribute, children, and association properties as <see cref=
  ///   "LoadState.NotLoaded"/>, and registers it in <see cref="_served"/>.
  /// </summary>
  private Topic BuildTopic(TopicRecord record, Topic parent) {

    // Setup
    var topic                   = new Topic(record.Key, record.ContentType, parent, record.Id);
    var rawTopic                = (ITopicBackingAccessor)topic;

    // Set indexed attributes
    foreach (var attribute in record.IndexedAttributes) {
      rawTopic.Attributes.SetValue(attribute.Key, attribute.Value, markDirty: false, isExtendedAttribute: false);
    }

    // Set extended attributes
    topic.SetLoadState(TopicPayload.ExtendedAttributes, LoadState.NotLoaded);

    // Set children
    topic.SetLoadState(TopicPayload.Children, LoadState.NotLoaded);

    // Add relationships to Deferred
    foreach (var (key, targetId) in record.Relationships) {
      rawTopic.Relationships.Deferred.Add(new(key, targetId));
    }

    // Add references to Deferred
    foreach (var (key, targetId) in record.References) {
      rawTopic.References.Deferred.Add(new(key, targetId));
    }

    // Mark record as served so it doesn't trigger OnTopicLoaded() again
    _served[record.Id]          = topic;

    // Return the built topic
    return topic;

  }

  /*============================================================================================================================
  | METHOD: GET UNIQUE KEY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Computes the unique key for <paramref name="record"/> by walking its <see cref="TopicRecord.ParentId"/> chain through
  ///   the record store, without constructing or touching any <see cref="Topic"/> instance.
  /// </summary>
  private string GetUniqueKey(TopicRecord record) {
    List<string> segments       = [record.Key];
    var current                 = record;
    while (current.ParentId is { } parentId) {
      current                   = _store[parentId];
      segments.Insert(0, current.Key);
    }
    segments.Insert(0, _root.Key);
    return String.Join(":", segments);
  }

  /*============================================================================================================================
  | METHODS: FETCH SPY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the total number of genuine boundary fetches recorded across every topic.
  /// </summary>
  public int TotalFetches => _fetchCounts.Values.Sum();

  /// <summary>
  ///   Returns the number of times <paramref name="boundary"/> was genuinely fetched for <paramref name="topicId"/>.
  /// </summary>
  public int GetFetchCount(int topicId, TopicPayload boundary) => _fetchCounts.GetValueOrDefault((topicId, boundary), 0);

  /// <summary>
  ///   Increments the fetch-count spy for <paramref name="boundary"/> on <paramref name="topicId"/>.
  /// </summary>
  private void RecordFetch(int topicId, TopicPayload boundary) {
    var key                     = (topicId, boundary);
    _fetchCounts[key]           = _fetchCounts.GetValueOrDefault(key) + 1;
  }

  /*============================================================================================================================
  | METHOD: CREATE DEFAULT RECORDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates the built-in seed dataset used by the parameterless constructor: A four-level "Web" content subtree with an
  ///   extended-attribute topic, a resolvable relationship and reference pair, and a stale (dangling) relationship and
  ///   reference pair.
  /// </summary>
  private static IReadOnlyList<TopicRecord> CreateDefaultRecords() =>
    new StubLazyLoadingTopicRepositoryBuilder()
      .AddTopic(10000, "Web", "Page", null, indexedAttributes: new Dictionary<string, string> { ["Title"] = "Web" })
      .AddTopic(10001, "Web_0", "Page", 10000, indexedAttributes: new Dictionary<string, string> { ["Title"] = "Web_0" })
      .AddTopic(
        10002,
        "Web_0_0",
        "Page",
        10001,
        indexedAttributes: new Dictionary<string, string> { ["Title"] = "Web_0_0" },
        extendedAttributes: new Dictionary<string, string> { ["Body"] = "Extended body content for Web_0_0." }
      )
      .AddTopic(10003, "Web_0_0_0", "Page", 10002, indexedAttributes: new Dictionary<string, string> { ["Title"] = "Web_0_0_0" })
      .AddTopic(10004, "Web_1", "Page", 10000, indexedAttributes: new Dictionary<string, string> { ["Title"] = "Web_1" })
      .AddRelationship(10004, "Related", 10002)
      .AddRelationship(10001, "Related", 99999)
      .AddReference(10004, "BaseTopic", 10001)
      .AddReference(10001, "BaseTopic", 99998)
      .Build();

} //Class