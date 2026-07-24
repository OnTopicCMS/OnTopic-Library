/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
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
  private readonly              Dictionary<int, Topic>          _topicIdIndex                   = new();
  private readonly              Dictionary<string, Topic>       _topicKeyIndex                  = new(StringComparer.OrdinalIgnoreCase);
  private readonly              HashSet<int>                    _absentTopicIdIndex             = new();
  private readonly              HashSet<string>                 _absentUniqueKeyIndex           = new(StringComparer.OrdinalIgnoreCase);
  private readonly              object                          _syncLock                       = new();

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
      .Load("Root", referenceTopic: null, isRecursive: false, payload: TopicPayload.All)
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
      .Load("Root:Configuration", referenceTopic: _cache, isRecursive: true, payload: TopicPayload.All)
      .GetAwaiter()
      .GetResult();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Populate flat index from seeded topics
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
  ///   Returns a cached topic if it satisfies the requested <paramref name="payload"/> and <paramref name="isRecursive"/>; an
  ///   insufficient hit is topped up via <see cref="EnsureLoaded(Topic, TopicPayload, Boolean)"/> before being returned. On a
  ///   miss, falls through to the underlying repository with <c>@LoadAscendants</c> enabled so the full ancestor chain is
  ///   fetched and merged into the live graph, using <paramref name="referenceTopic"/> if supplied, or the cache root
  ///   otherwise, so the underlying load seeds its working index from, and can attach directly to, the resident graph. Missing
  ///   IDs are recorded to prevent redundant round-trips for topics that do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle request for entire tree
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topicId < 0) {
      await EnsureLoaded(_cache, payload, isRecursive).ConfigureAwait(false);
      return _cache;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup by topic identifier; top up and return on a hit
    \-------------------------------------------------------------------------------------------------------------------------*/
    Topic? topic;
    lock (_syncLock) {
      _topicIdIndex.TryGetValue(topicId, out topic);
    }
    if (topic is not null) {
      await EnsureLoaded(topic, payload, isRecursive).ConfigureAwait(false);
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
      .Load(topicId, referenceTopic?? _cache, isRecursive, payload)
      .ConfigureAwait(false);

    // If it's missing, populate the appropriate index so we don't try loading it again
    if (loaded is null) {
      lock (_syncLock) {
        _absentTopicIdIndex.Add(topicId);
      }
      return null;
    }

    // Return the topic from the cache
    lock (_syncLock) {
      _topicIdIndex.TryGetValue(topicId, out var result);
      return result;
    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Returns a cached topic if it satisfies the requested <paramref name="payload"/> and <paramref name="isRecursive"/>; an
  ///   insufficient hit is topped up via <see cref="EnsureLoaded(Topic, TopicPayload, Boolean)"/> before being returned. On a
  ///   miss, falls through to the underlying repository with <c>@LoadAscendants</c> enabled so the full ancestor chain is
  ///   fetched and merged into the live graph, using <paramref name="referenceTopic"/> if supplied, or the cache root
  ///   otherwise, so the underlying load seeds its working index from, and can attach directly to, the resident graph. Missing
  ///   IDs are recorded to prevent redundant round-trips for topics that do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
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
      await EnsureLoaded(resident, payload, isRecursive).ConfigureAwait(false);
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
      .Load(uniqueKey, referenceTopic?? _cache, isRecursive, payload)
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
    payload                     = ((ITopicLazyLoadable)topic).FilterPayload(payload);

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
        await loader.EnsureLoaded(topic, innerPayload, cancellationToken).ConfigureAwait(false);
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
  ///   for cases where the <c>isRecursive</c> parameter was specified on <see cref="Load(int, Topic?, bool, TopicPayload)"/>.
  ///   Ancestors pulled in via <c>@LoadAscendants</c> sit above the topic, so <see cref="TopicExtensions.FindAll(Topic)"/>,
  ///   which only walks downward, never reaches them; they are indexed by walking up the parent chain instead.
  /// </remarks>
  protected override void OnTopicLoaded(TopicLoadEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicLoaded(args);

    lock (_syncLock) {

      // Index the loaded topic and any descendants that came back attached; FindAll() is lazy-safe and naturally returns just
      // the topic itself when nothing further is present, so this is correct whether or not the load was recursive
      foreach (var topic in args.Topic.FindAll()) {
        if (_topicIdIndex.ContainsKey(topic.Id)) {
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
        if (_topicIdIndex.ContainsKey(ancestor.Id)) {
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
  ///   lookups.
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
  ///   "TopicExtensions.FindAll(Topic)"/> on the deleted topic still returns the full subtree.
  /// </remarks>
  protected override void OnTopicDeleted(TopicEventArgs args) {

    // Setup
    Contract.Requires(args);
    base.OnTopicDeleted(args);

    // Remove the deleted subtree from both indices
    lock (_syncLock) {
      foreach (var topic in args.Topic.FindAll()) {
        _topicIdIndex.Remove(topic.Id);
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
  ///   Adds or updates <paramref name="topic"/> in both flat indexes.
  /// </summary>
  /// <remarks>
  ///   Callers are responsible for holding <see cref="_syncLock"/> before invoking this method, except during construction
  ///   where single-threaded access is guaranteed.
  /// </remarks>
  private void IndexTopic(Topic topic) {
    _topicIdIndex[topic.Id]     = topic;
    _topicKeyIndex[topic.GetUniqueKey()] = topic;
  }

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Confirms that <paramref name="topic"/> already satisfies the requested <paramref name="payload"/> and <paramref name=
  ///   "isRecursive"/> scope and, if not, tops it up in place; the caller's own reference to <paramref name="topic"/> reflects
  ///   whatever is added.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Relationships and references are excluded from the sufficiency gate since <c>Load()</c> never guarantees a fully
  ///     resolved target graph, so gating on them would prevent convergence and force a reload on every hit.
  ///   </para>
  ///   <para>
  ///     A single-topic shortfall is topped up via <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload, CancellationToken)"
  ///     />, which converges <c>LoadState</c> in a single batched round-trip. A recursive shortfall, including a whole-tree
  ///     request, performs one deep <see cref="ITopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> against the
  ///     underlying repository, using <paramref name="topic"/> itself as the reference into the topic graph, so the underlying
  ///     load merges the result directly into it. It then looks up any in-graph associations (<see cref=
  ///     "LazyLoadingTopicRepository.ResolveAssociations(Topic, TopicPayload)"/>), so any relationship or reference targets
  ///     that just became resident are connected without a further trip.
  ///   </para>
  /// </remarks>
  /// <param name="topic">The already-resident topic to confirm or top up.</param>
  /// <param name="payload">The <see cref="TopicPayload"/> flags the caller requires to be loaded.</param>
  /// <param name="isRecursive">Whether the caller requires the full subtree, not merely <paramref name="topic"/> itself.</param>
  private async Task EnsureLoaded(
    Topic topic,
    TopicPayload payload,
    bool isRecursive
  ) {

    // Narrow the sufficiency gate to exclude relationships and references, which Load() never guarantees are fully resolved
    var gate                    = payload & ~(TopicPayload.Relationships | TopicPayload.References);

    // Return immediately if the resident topic already satisfies the requested scope
    if (((ITopicLazyLoadable)topic).IsLoaded(gate, isRecursive)) {
      return;
    }

    // Top up a non-recursive shortfall via the loader, which converges LoadState in a single round-trip
    if (!isRecursive) {
      await ((ITopicLazyLoadable)topic).EnsureLoaded(gate).ConfigureAwait(false);
      return;
    }

    // Top up a recursive shortfall via one deep load, merged into the live graph
    var loaded                  = await TopicRepository
      .Load(topic.Id, topic, isRecursive, payload)
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