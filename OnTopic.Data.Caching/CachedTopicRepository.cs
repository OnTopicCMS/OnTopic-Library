/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Data.Caching;

/*==============================================================================================================================
| CLASS: CACHED TOPIC DATA REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides data access to topics stored in memory.
/// </summary>
/// <remarks>
///   Concrete implementation of the <see cref="ITopicRepository"/> class, which provides a wrapper for an actual data access
///   class.
/// </remarks>

public class CachedTopicRepository : TopicRepositoryDecorator, ITopicLazyLoader {

  /*============================================================================================================================
  | VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Topic                           _cache;
  private readonly              Dictionary<string, Topic>       _topicKeyIndex                  = new(StringComparer.OrdinalIgnoreCase);
  private readonly              HashSet<int>                    _absentTopicIdIndex             = new();
  private readonly              HashSet<string>                 _absentUniqueKeyIndex           = new(StringComparer.OrdinalIgnoreCase);
  private readonly              object                          _syncLock                       = new();
  private readonly              ConcurrentDictionary<int, SemaphoreSlim> _loadGates             = new();

  /*============================================================================================================================
  | CONSTANTS
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <summary>
  ///   Payload whose full load lets a per-topic gate be reclaimed.
  /// </summary>
  /// <remarks>
  ///   This excludes <see cref="TopicPayload.VersionHistory"/>, which rarely loads outside the editor, so most gates reclaim as
  ///   soon as <see cref="TopicPayload.Children"/> and <see cref="TopicPayload.ExtendedAttributes"/> converge, rather than
  ///   persisting indefinitely. A gate recreated later for a <see cref="TopicPayload.VersionHistory"/>-only fetch is reclaimed
  ///   immediately once that fetch completes.
  /// </remarks>
  private const                 TopicPayload                    _reclaimPayload                  = TopicPayload.Children | TopicPayload.ExtendedAttributes;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instantiates a new instance of the <see cref="CachedTopicRepository"/> with a dependency on an underlying <see cref=
  ///   "ITopicRepository"/> in order to provide necessary data access.
  /// </summary>
  /// <param name="topicRepository">
  ///   A concrete instance of an <see cref="ITopicRepository"/>, which will be used for data access.
  /// </param>
  /// <returns>A new instance of a <see cref="CachedTopicRepository"/>.</returns>
  public CachedTopicRepository(ITopicRepository topicRepository) : base(topicRepository) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Seed root topic and its immediate children (without grandchildren)
    >-------------------------------------------------------------------------------------------------------------------------
    | The top-level topics under Root typically represent distinct content buckets (e.g., Web, Configuration) that are commonly
    | referenced individually via e.g., relationships used to delegate navigation, so fully loading this shallow tier up front
    | avoids the predictable, immediate lazy-load of Root.Children that would otherwise follow. Each child's own Children remain
    | deferred, preserving the benefits of lazy loading below this boundary.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rootTopic               = TopicRepository
      .Load("Root", referenceTopic: null, payload: TopicPayload.All)
      .GetAwaiter()
      .GetResult();

    Contract.Assume(
      rootTopic,
      $"The topic graph could not be successfully loaded from the {nameof(ITopicRepository)} instance. The " +
      $"{nameof(CachedTopicRepository)} is unable to establish the cache."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish cache
    \-------------------------------------------------------------------------------------------------------------------------*/
    _cache                      = rootTopic;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Eager-load Root:Configuration subtree (required for content-type descriptor resolution)
    \-------------------------------------------------------------------------------------------------------------------------*/
    TopicRepository
      .Load("Root:Configuration", referenceTopic: _cache, payload: TopicPayload.All, depth: -1)
      .GetAwaiter()
      .GetResult();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Populate key index from seeded topics
    >---------------------------------------------------------------------------------------------------------------------------
    | The live id index needs no seeding here: Any ITopicRepository.Load() call attaches its results directly into the graph
    | of the referenceTopic it's given, which builds that topic's live index; the Root:Configuration load above did so via
    | _cache, and every topic attached since keeps it current.
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var topic in _cache.FindAll()) {
      IndexTopic(topic);
    }

  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  /// <remarks>
  ///   Returns a cached topic if it satisfies the requested <paramref name="payload"/> and <paramref name="depth"/>; an
  ///   insufficient hit is topped up via <see cref="EnsureLoaded(Topic, TopicPayload, Int32)"/> before being returned. On a
  ///   miss, falls through to the underlying repository with <c>@LoadAscendants</c> enabled so the full ancestor chain is
  ///   fetched and merged into the live graph, using <paramref name="referenceTopic"/> if supplied, or the cache root
  ///   otherwise, so the underlying load seeds its working index from, and can attach directly to, the resident graph. Missing
  ///   IDs are recorded to prevent redundant round-trips for topics that do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    TopicPayload payload        = TopicPayload.None,
    int depth                   = 0
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle request for entire tree
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topicId < 0) {
      await EnsureLoaded(_cache, payload, depth).ConfigureAwait(false);
      return _cache;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup by topic identifier; top up and return on a hit
    \-------------------------------------------------------------------------------------------------------------------------*/
    _cache.GetLiveTopicIndex().TryGetValue(topicId, out var topic);
    if (topic is not null) {
      await EnsureLoaded(topic, payload, depth).ConfigureAwait(false);
      return topic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Skip IDs that are known to be missing to avoid redundant round-trips
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_absentTopicIdIndex.Contains(topicId)) {
        return null;
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | On miss: Load with ancestors and merge result into the live graph
    \-------------------------------------------------------------------------------------------------------------------------*/
    var loaded                  = await TopicRepository
      .Load(topicId, referenceTopic?? _cache, payload, depth)
      .ConfigureAwait(false);

    // If it's missing, populate the appropriate index so we don't try loading it again
    if (loaded is null) {
      lock (_syncLock) {
        _absentTopicIdIndex.Add(topicId);
      }
      return null;
    }

    // Return the topic from the cache; the TopicIndexRegistry hooks indexed it as it was merged above
    _cache.GetLiveTopicIndex().TryGetValue(topicId, out var result);
    return result;

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Returns a cached topic if it satisfies the requested <paramref name="payload"/> and <paramref name="depth"/>; an
  ///   insufficient hit is topped up via <see cref="EnsureLoaded(Topic, TopicPayload, Int32)"/> before being returned. On a
  ///   miss, falls through to the underlying repository with <c>@LoadAscendants</c> enabled so the full ancestor chain is
  ///   fetched and merged into the live graph, using <paramref name="referenceTopic"/> if supplied, or the cache root
  ///   otherwise, so the underlying load seeds its working index from, and can attach directly to, the resident graph. Missing
  ///   IDs are recorded to prevent redundant round-trips for topics that do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    TopicPayload payload        = TopicPayload.None,
    int depth                   = 0
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (String.IsNullOrEmpty(uniqueKey)) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Normalize key: Accept partial paths such as "Web:Valid:Child" in addition to the canonical "Root:Web:Valid:Child"
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (
      !uniqueKey.StartsWith(_cache.Key + ":", StringComparison.OrdinalIgnoreCase) &&
      !uniqueKey.Equals(_cache.Key, StringComparison.OrdinalIgnoreCase)
    ) {
      uniqueKey                 = $"{_cache.Key}:{uniqueKey.TrimStart(':')}";
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup by unique key; top up and return on a hit
    \-------------------------------------------------------------------------------------------------------------------------*/
    Topic? resident;
    lock (_syncLock) {
      _topicKeyIndex.TryGetValue(uniqueKey, out resident);
    }
    if (resident is not null) {
      await EnsureLoaded(resident, payload, depth).ConfigureAwait(false);
      return resident;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Skip IDs that are known to be missing to avoid redundant round-trips
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_absentUniqueKeyIndex.Contains(uniqueKey)) {
        return null;
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | On miss: Load with ancestors and merge result into the live graph
    \-------------------------------------------------------------------------------------------------------------------------*/
    var loaded                  = await TopicRepository
      .Load(uniqueKey, referenceTopic?? _cache, payload, depth)
      .ConfigureAwait(false);

    if (loaded is null) {
      lock (_syncLock) {
        _absentUniqueKeyIndex.Add(uniqueKey);
      }
      return null;
    }

    // Return the topic from the cache
    lock (_syncLock) {
      _topicKeyIndex.TryGetValue(uniqueKey, out var result);
      return result;
    }

  }

  /*============================================================================================================================
  | METHODS: TOPIC LAZY LOADER
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  public virtual async Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Filter to pending (i.e., not yet Loaded) payload
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rawTopic                = (ITopicLazyLoadable)topic;
    payload                     = rawTopic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Delegate to the inner resolver; captures missing targets in the Deferred collections
    >-------------------------------------------------------------------------------------------------------------------------
    | Relationships and References are withheld from the inner delegation: This cache is the outermost resolver, so it alone
    | is responsible for resolving deferred association targets, via its own Load()—which checks the flat index before
    | falling through to the inner repository. If the inner repository (e.g., SqlTopicRepository) were also asked to resolve
    | them, it would do so via its own, non-cache-aware Load(), producing a duplicate Topic instance for any target that's
    | already present in this cache.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (TopicRepository is ITopicLazyLoader loader) {
      var innerPayload          = payload & ~(TopicPayload.Relationships | TopicPayload.References);
      if (innerPayload is not TopicPayload.None) {

        // Serialize fetches and merges (children, extended attributes, version history) per topic
        var gate                = _loadGates.GetOrAdd(topic.Id, _ => new(1, 1));
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {

          // Second escape hatch: Re-filter under the gate, since a prior holder may have merged some or all of this payload
          innerPayload          = rawTopic.FilterPayload(innerPayload);
          if (innerPayload is not TopicPayload.None) {
            await loader.EnsureLoaded(topic, innerPayload, cancellationToken).ConfigureAwait(false);
          }
        }
        finally {
          gate.Release();
        }

        // Reclaim: Once ReclaimPayload is loaded, the gate is dead weight, so we can drop the cached instance
        if (rawTopic.IsLoaded(_reclaimPayload)) {
          _loadGates.TryRemove(new(topic.Id, gate));
        }

      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Resolve any relationship and reference targets via the cache layer
    \-------------------------------------------------------------------------------------------------------------------------*/
    await LoadDeferredAssociations(topic, payload, cancellationToken).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHODS: EVENT HANDLERS
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  /// <remarks>
  ///   Adds the newly loaded topic to the index and clears any entries previously known to be missing, so a topic that was
  ///   missing on an earlier lookup can be found now. This automatically indexes any descendants loaded alongside the topic,
  ///   for cases where a non-zero <c>depth</c> was specified on <see cref="Load(int, Topic?, TopicPayload, int)"/>.
  ///   Ancestors pulled in via <c>@LoadAscendants</c> sit above the topic, so <see cref="TopicExtensions.FindAll(Topic)"/>,
  ///   which only walks downward, never reaches them; they are indexed by walking up the parent chain instead.
  ///   <para>
  ///     The live id index needs no attention here, as it is managed via the <see cref="TopicIndexRegistry"/> before this event
  ///     even fires. A historical version load (<see cref="TopicLoadEventArgs.Version"/> not <see langword="null"/>) is skipped
  ///     entirely: Its topic is never attached to the resident graph, so it must not enter the key index either.
  ///   </para>
  /// </remarks>
  protected override void OnTopicLoaded(TopicLoadEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicLoaded(args);

    // Historical version loads are not part of the resident graph; neither index should reflect them
    if (args.Version is not null) {
      return;
    }

    lock (_syncLock) {

      // Index the loaded topic and any descendants that came back attached; FindAll() is lazy-safe and naturally returns just
      // the topic itself when nothing further is present, so this is correct whether or not the load was recursive.
      foreach (var topic in args.Topic.FindAll()) {
        if (_topicKeyIndex.ContainsKey(topic.GetUniqueKey())) {
          continue;
        }
        IndexTopic(topic);
        _absentTopicIdIndex.Remove(topic.Id);
        _absentUniqueKeyIndex.Remove(topic.GetUniqueKey());
      }

      // Index any ancestors pulled in via @LoadAscendants, which sit above the requested topic and so are missed by FindAll().
      // Walk up from the parent, stopping at the first already indexed ancestor: The cache is always rooted, so everything
      // above an existing ancestor is itself already loaded and indexed.
      for (var ancestor = args.Topic.Parent; ancestor is not null; ancestor = ancestor.Parent) {
        if (_topicKeyIndex.ContainsKey(ancestor.GetUniqueKey())) {
          break;
        }
        IndexTopic(ancestor);
        _absentTopicIdIndex.Remove(ancestor.Id);
        _absentUniqueKeyIndex.Remove(ancestor.GetUniqueKey());
      }

    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Adds newly created topics to the flat index. When the save is recursive, all present descendants are indexed as well,
  ///   since only one <see cref="ITopicRepository.TopicSaved"/> event fires for the root of a recursive save. Also clears any
  ///   entries known to be missing so that a previously missing ID or key that is now created can be found on subsequent
  ///   lookups. The live id index needs no attention here: <see cref="Topic.Id"/>'s setter already indexed each newly created
  ///   topic, via the registry's hooks, at the moment its persisted identifier was assigned.
  /// </remarks>
  protected override void OnTopicSaved(TopicSaveEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicSaved(args);

    // Index newly created topics and, when saved recursively, any new descendants
    if (args.IsNew) {
      lock (_syncLock) {
        foreach (var topic in args.Topic.FindAll()) {
          IndexTopic(topic);
          _absentTopicIdIndex.Remove(topic.Id);
          _absentUniqueKeyIndex.Remove(topic.GetUniqueKey());
        }
      }
    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Removes the deleted topic and all of its descendants from the flat index. Called after the topic has been detached from
  ///   its parent's <see cref="Topic.Children"/> collection but before the topic graph is torn down, so <see cref=
  ///   "TopicExtensions.FindAll(Topic)"/> on the deleted topic still returns the full subtree. The live id index needs no
  ///   attention here: <see cref="ITopicRepository.Delete(Topic, Boolean)"/> detaches the topic from its parent before raising
  ///   this event, so the registry's detach hook has already pruned the subtree from it.
  /// </remarks>
  protected override void OnTopicDeleted(TopicEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicDeleted(args);

    // Remove the deleted subtree from the key index
    lock (_syncLock) {
      foreach (var topic in args.Topic.FindAll()) {
        _topicKeyIndex.Remove(topic.GetUniqueKey());
      }
    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Rebuilds the unique-key index entries for the moved topic and all of its descendants. The move has already completed by
  ///   the time this fires, so the old root key is reconstructed from <see cref="TopicMoveEventArgs.Source"/> and the topic's
  ///   (unchanged) <see cref="Topic.Key"/>.
  /// </remarks>
  protected override void OnTopicMoved(TopicMoveEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicMoved(args);

    // Reconstruct the old root unique key from the source parent and the (unchanged) topic key
    var oldRootUniqueKey        = args.Source is null
      ? args.Topic.Key
      : $"{args.Source.GetUniqueKey()}:{args.Topic.Key}";

    // Reindex topic and children
    RekeyTopicSubtree(args.Topic, oldRootUniqueKey);

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Rebuilds the unique-key index entries for the renamed topic and all of its descendants. The rename has already been
  ///   applied to <see cref="Topic.Key"/> by the time this fires, so the old root key is reconstructed from the (unchanged)
  ///   parent path and <see cref="TopicRenameEventArgs.OriginalKey"/>.
  /// </remarks>
  protected override void OnTopicRenamed(TopicRenameEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicRenamed(args);

    // Reconstruct the old root unique key from the (unchanged) parent path and the original key
    var oldRootUniqueKey        = args.Topic.Parent is null
      ? args.OriginalKey
      : $"{args.Topic.Parent.GetUniqueKey()}:{args.OriginalKey}";

    // Reindex topic and children
    RekeyTopicSubtree(args.Topic, oldRootUniqueKey);

  }

  /*============================================================================================================================
  | METHOD: REKEY TOPIC SUBTREE
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <summary>
  ///   Removes stale <c>_topicByKey</c> entries for <paramref name="topic"/> and its descendants by swapping the <paramref
  ///   name="oldRootUniqueKey"/> prefix for the current one, then reindexes the subtree under its current unique keys.
  /// </summary>
  private void RekeyTopicSubtree(Topic topic, string oldRootUniqueKey) {

    // Establish variables
    var newRootUniqueKey        = topic.GetUniqueKey();

    // Remove each stale unique-key entry and replace it with the current unique key
    lock (_syncLock) {
      foreach (var subtopic in topic.FindAll()) {
        var currentKey          = subtopic.GetUniqueKey();
        var oldKey              = oldRootUniqueKey + currentKey[newRootUniqueKey.Length..];
        _topicKeyIndex.Remove(oldKey);
        _topicKeyIndex[currentKey] = subtopic;
      }
    }

  }

  /*============================================================================================================================
  | METHOD: INDEX TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Adds or updates <paramref name="topic"/> in the unique-key index.
  /// </summary>
  /// <remarks>
  ///   The live id index (<see cref="TopicExtensions.GetLiveTopicIndex(Topic)"/>) is maintained separately by the <see cref=
  ///   "TopicIndexRegistry"/> hooks, so needs no counterpart here. Callers are responsible for holding <see cref="_syncLock"/>
  ///   before invoking this method, except during construction where single-threaded access is guaranteed.
  /// </remarks>
  private void IndexTopic(Topic topic) => _topicKeyIndex[topic.GetUniqueKey()] = topic;

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Confirms that <paramref name="topic"/> already satisfies the requested <paramref name="payload"/> and <paramref name=
  ///   "depth"/> scope and, if not, tops it up in place; the caller's own reference to <paramref name="topic"/> reflects
  ///   whatever is added.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Relationships and references are excluded from the sufficiency gate since <c>Load()</c> never guarantees a fully
  ///     resolved target graph, so gating on them would prevent convergence and force a reload on every hit.
  ///   </para>
  ///   <para>
  ///     A single-topic shortfall is topped up via <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload, CancellationToken)"
  ///     />, which converges <c>LoadState</c> in a single batched round-trip. Any other shortfall, including a whole-tree
  ///     request, performs one deep <see cref="ITopicRepository.Load(Int32, Topic?, TopicPayload, Int32)"/> against the
  ///     underlying repository, using <paramref name="topic"/> itself as the reference into the topic graph, so the underlying
  ///     load merges the result directly into it. It then looks up any in-graph associations (<see cref=
  ///     "LazyLoadingTopicRepository.ResolveAssociations(Topic, TopicPayload)"/>), so any relationship or reference targets
  ///     that just became resident are connected without a further trip.
  ///   </para>
  ///   <para>
  ///     Interim gate semantics (Stage 1): <paramref name="depth"/> is only distinguished from zero here—any non-zero value is
  ///     treated as the old <c>isRecursive: true</c>, a documented superset until Stage 3 tightens the gate to honor the
  ///     requested depth precisely.
  ///   </para>
  /// </remarks>
  /// <param name="topic">The already-resident topic to confirm or top up.</param>
  /// <param name="payload">The <see cref="TopicPayload"/> flags the caller requires to be loaded.</param>
  /// <param name="depth">The number of tiers of descendants the caller requires, not merely <paramref name="topic"/> itself.</param>
  private async Task EnsureLoaded(
    Topic topic,
    TopicPayload payload,
    int depth
  ) {

    // Narrow the sufficiency gate to exclude relationships and references, which Load() never guarantees are fully resolved
    var gate                    = payload & ~(TopicPayload.Relationships | TopicPayload.References);

    // Return immediately if the resident topic already satisfies the requested scope
    if (((ITopicLazyLoadable)topic).IsLoaded(gate, depth != 0)) {
      return;
    }

    // Top up a single-topic shortfall via the loader, which converges LoadState in a single round-trip
    if (depth is 0) {
      await ((ITopicLazyLoadable)topic).EnsureLoaded(gate).ConfigureAwait(false);
      return;
    }

    // Top up any other shortfall via one deep load, merged into the live graph
    var loaded                  = await TopicRepository
      .Load(topic.Id, topic, payload, depth)
      .ConfigureAwait(false);

    if (loaded is not null) {

      // Opportunistically connect any relationship or reference targets that are now resident in the merged region, regardless
      // of whether relationships or references were themselves part of the requested payload
      foreach (var descendant in loaded.FindAll()) {
        await ResolveAssociations(descendant, TopicPayload.Relationships | TopicPayload.References).ConfigureAwait(false);
      }

    }

  }

} //Class