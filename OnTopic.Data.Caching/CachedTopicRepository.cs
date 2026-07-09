/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;
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
///   Concrete implementation of the <see cref="OnTopic.Repositories.ITopicRepository"/> class, which provides a wrapper
///   for an actual data access class.
/// </remarks>

public class CachedTopicRepository : TopicRepositoryDecorator, ITopicLoadResolver {

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
  ///   Instantiates a new instance of the <see cref="CachedTopicRepository"/> with a dependency on an underlying <see cref="
  ///   ITopicRepository"/> in order to provide necessary data access.
  /// </summary>
  /// <param name="topicRepository">
  ///   A concrete instance of an <see cref="ITopicRepository"/>, which will be used for data access.
  /// </param>
  /// <returns>A new instance of a <see cref="CachedTopicRepository"/>.</returns>
  public CachedTopicRepository(ITopicRepository topicRepository) : base(topicRepository) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Seed root topic (without descendants)
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rootTopic               = TopicRepository.Load("Root", referenceTopic: null, isRecursive: false)
                                    .GetAwaiter().GetResult();

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
    TopicRepository.Load("Root:Configuration", referenceTopic: _cache, isRecursive: true, payload: TopicPayload.All)
      .GetAwaiter().GetResult();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Populate flat index from seeded topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var topic in _cache.FindAll()) {
      IndexTopic(topic);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Stamp resolver on seeded graph
    \-------------------------------------------------------------------------------------------------------------------------*/
    StampResolver(_cache);

  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  /// <remarks>
  ///   Returns the cached topic if present. On a miss, falls through to the underlying repository with <c>@LoadAscendants</c>
  ///   enabled so the full ancestor chain is fetched and merged into the live graph.
  ///   Missing IDs are recorded to prevent
  ///   redundant round-trips for topics that genuinely do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = true,
    TopicPayload payload        = TopicPayload.All
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle request for entire tree
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topicId < 0) {
      return _cache;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup by topic identifier; return immediately on a hit
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_topicIdIndex.TryGetValue(topicId, out var topic)) {
        return topic;
      }
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
    var loaded                  = await TopicRepository.Load(topicId, referenceTopic: null, isRecursive: false)
                                    .ConfigureAwait(false);

    // If it's missing, populate the appropriate index so we don't try loading it again
    if (loaded is null) {
      lock (_syncLock) {
        _absentTopicIdIndex.Add(topicId);
      }
      return null;
    }

    // Merge the returned ancestor chain into the cache, rewiring new topics to existing cache objects
    MergeIntoCache(loaded);

    // Return the topic from the cache
    lock (_syncLock) {
      _topicIdIndex.TryGetValue(topicId, out var result);
      return result;
    }

  }

  /// <inheritdoc />
  /// <remarks>
  ///   Returns the cached topic if present. On a miss, falls through to the underlying repository with <c>@LoadAscendants</c>
  ///   enabled so the full ancestor chain is fetched and merged into the live graph.
  ///   Missing IDs are recorded to prevent
  ///   redundant round-trips for topics that genuinely do not exist.
  /// </remarks>
  public override async Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    bool isRecursive            = true,
    TopicPayload payload        = TopicPayload.All
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
      uniqueKey = $"{_cache.Key}:{uniqueKey.TrimStart(':')}";
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup by unique key; return immediately on a hit
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_topicKeyIndex.TryGetValue(uniqueKey, out var topic)) {
        return topic;
      }
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
    var loaded = await TopicRepository.Load(uniqueKey, referenceTopic: null, isRecursive: false)
                   .ConfigureAwait(false);

    if (loaded is null) {
      lock (_syncLock) {
        _absentUniqueKeyIndex.Add(uniqueKey);
      }
      return null;
    }

    // Merge the returned ancestor chain into the cache, rewiring new topics to existing cache objects
    MergeIntoCache(loaded);

    lock (_syncLock) {
      _topicKeyIndex.TryGetValue(uniqueKey, out var result);
      return result;
    }

  }

  /// <inheritdoc />
  public override async Task<Topic?> Load(int topicId, DateTime version, Topic? referenceTopic = null) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Normalize parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    version                     = NormalizeToUtc(version);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(version.Date < DateTime.UtcNow, "The version requested must be a valid historical date.");
    Contract.Requires(
      version.Date >= new DateTime(2014, 12, 9),
      "The version is expected to have been created since version support was introduced into the topic library."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return appropriate topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = await TopicRepository.Load(topicId, version, referenceTopic ?? _cache)
                                    .ConfigureAwait(false);
    StampResolver(topic);
    return topic;

  }

  /*============================================================================================================================
  | METHODS: TOPIC LOAD RESOLVER
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
    payload                    = topic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Delegate to the inner resolver; captures missing targets in the Deferred collections
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (TopicRepository is ITopicLoadResolver resolver) {
      await resolver.EnsureLoaded(topic, payload, cancellationToken).ConfigureAwait(false);
    }

    // Resolve any deferred relationship/reference targets through the cache layer
    await ResolveDeferredAssociations(topic, payload, cancellationToken).ConfigureAwait(false);

    // Update flat index and stamp resolver for any newly loaded children
    if (payload.HasFlag(TopicPayload.Children)) {
      lock (_syncLock) {
        foreach (var child in topic.Children) {
          IndexTopic(child);
        }
      }
      StampResolver(topic);
    }

  }

  /*============================================================================================================================
  | METHODS: EVENT HANDLERS
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  /// <remarks>
  ///   Adds newly-created topics to the flat index. When the save is recursive, all resident descendants are indexed as well,
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
  | METHODS: PRIVATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Resolves any relationship and reference targets that were deferred by the underlying <see cref="ITopicRepository"/> by
  ///   loading each through the cache layer's own <c>Load()</c>, which checks the index before falling through to the
  ///   underlying persistence store. Targets that cannot be found are treated as stale references to deleted topics and
  ///   discarded; the getter clears any remaining <see cref="DeferredAssociation"/> entries after this method returns.
  /// </summary>
  /// <param name="topic">The topic whose deferred associations should be resolved.</param>
  /// <param name="payload">
  ///   The payload flags that were requested; only <see cref="TopicPayload.Relationships"/> and <see cref=
  ///   "TopicPayload.References"/> are acted upon.
  /// </param>
  private async Task ResolveDeferredAssociations(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {

    var rawTopic                = (ITopicBackingAccessor)topic;

    // Resolve deferred relationship targets; unresolvable targets are treated as stale and discarded
    if (payload.HasFlag(TopicPayload.Relationships) && rawTopic.Relationships.Deferred.Count > 0) {
      foreach (var deferred in rawTopic.Relationships.Deferred.ToArray()) {
        var target              = await Load(deferred.TopicId).ConfigureAwait(false);
        // SetValue removes the matching Deferred entry; stale entries are cleared by the Topic getter
        if (target is not null) {
          rawTopic.Relationships.SetValue(deferred.Key, target, markDirty: false);
        }
      }
    }

    // Resolve deferred reference targets; unresolvable targets are treated as stale and discarded
    if (payload.HasFlag(TopicPayload.References) && rawTopic.References.Deferred.Count > 0) {
      foreach (var deferred in rawTopic.References.Deferred.ToArray()) {
        var target              = await Load(deferred.TopicId).ConfigureAwait(false);
        // SetValue removes the matching Deferred entry; stale entries are cleared by the Topic getter
        if (target is not null) {
          rawTopic.References.SetValue(deferred.Key, target, markDirty: false);
        }
      }
    }

  }

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
  | METHOD: MERGE INTO CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Merges a freshly-loaded ancestor chain into the live graph by rewiring each new topic's <see cref="Topic.Parent"/> to
  ///   the corresponding cache object, then indexing and resolver-stamping any topics that were not previously resident.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Called by the <see cref="Load(Int32, Topic, Boolean, TopicPayload)"/> and <see cref=
  ///     "Load(String, Topic?, Boolean, TopicPayload)"/> overloads when a requested topic is not present in the flat index and
  ///     must be fetched from the underlying <see cref="ITopicRepository"/> with <c>@LoadAscendants = true</c>. The load
  ///     returns a freshly-built graph, including duplicate <see cref="Topic"/> objects for ancestors already in the cache.
  ///     This method replaces each duplicate ancestor with the resident cache object, keeps only genuinely new nodes, and
  ///     integrates them into the live graph.
  ///   </para>
  ///   <para>
  ///     The chain is walked from the leaf toward the root. At the first ancestor already present in <c>_topicById</c>
  ///     (typically <c>Root</c>), the new node above it is discarded and its child is reparented to the cached object, which
  ///     attaches it to the existing graph. All new nodes below that boundary are indexed and stamped with the resolver so
  ///     their own <see cref="Topic.Children"/> can lazy-load on demand.
  ///   </para>
  /// </remarks>
  /// <param name="loaded">The leaf topic returned from the underlying load, already part of an ancestor chain.</param>
  private void MergeIntoCache(Topic loaded) {

    // Build the ancestor chain from the leaf up to the root (leaf first)
    List<Topic> chain = [];
    for (var node = loaded; node is not null; node = node.Parent) {
      chain.Add(node);
    }

    // Walk the chain leaf-to-root, rewiring new topics onto the existing cache and indexing them
    foreach (var node in chain) {

      // Skip topics that are already present in the cache
      lock (_syncLock) {
        if (_topicIdIndex.ContainsKey(node.Id)) {
          continue;
        }
      }

      // Rewire to the existing cache parent to prevent duplicate Topic objects in the graph
      if (node.Parent is not null) {
        lock (_syncLock) {
          if (_topicIdIndex.TryGetValue(node.Parent.Id, out var cacheParent) && cacheParent != node.Parent) {
            node.Parent = cacheParent;
          }
        }
      }

      // Index the new topic and stamp it with the resolver for future lazy fills
      lock (_syncLock) {
        IndexTopic(node);
      }
      StampResolver(node);

    }

  }

} //Class