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
    | Ensure topics are loaded
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rootTopic               = TopicRepository.Load();

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
    | Populate flat index from loaded graph
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var topic in _cache.FindAll()) {
      IndexTopic(topic);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Stamp resolver on loaded graph
    \-------------------------------------------------------------------------------------------------------------------------*/
    StampResolver(_cache);

  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override Topic? Load(
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
    | Lookup by topic identifier
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_topicIdIndex.TryGetValue(topicId, out var topic)) {
        return topic;
      }
    }

  }

  /// <inheritdoc />
  public override Topic? Load(
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
    | Lookup by unique key
    \-------------------------------------------------------------------------------------------------------------------------*/
    lock (_syncLock) {
      if (_topicKeyIndex.TryGetValue(uniqueKey, out var topic)) {
        return topic;
      }
    }

  }

  /// <inheritdoc />
  public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) {

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
    var topic                   = TopicRepository.Load(topicId, version, referenceTopic?? _cache);
    StampResolver(topic);
    return topic;

  }

  /*============================================================================================================================
  | METHODS: TOPIC LOAD RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  public virtual void EnsureLoaded(Topic topic, TopicPayload payload) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Filter to pending (not yet Loaded) payload
    \-------------------------------------------------------------------------------------------------------------------------*/
    payload                    = topic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return;
    }

    }

    }

  }

  /// <inheritdoc />
  public virtual Task EnsureLoadedAsync(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Filter to pending (i.e., not yet Loaded) payload
    \-------------------------------------------------------------------------------------------------------------------------*/
    payload                    = topic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return Task.CompletedTask;
    }

    }

    }

    return Task.CompletedTask;

  }

  /*============================================================================================================================
  | METHODS: EVENT HANDLERS
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  /// <remarks>
  ///   Adds newly-created topics to the flat index. When the save is recursive, all resident descendants are indexed as well,
  ///   since only one <see cref="ITopicRepository.TopicSaved"/> event fires for the root of a recursive save.
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
  ///   Removes stale <see cref="_topicByKey"/> entries for <paramref name="topic"/> and its descendants by swapping the
  ///   <paramref name="oldRootUniqueKey"/> prefix for the current one, then re-indexes the subtree under its current unique
  ///   keys.
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

  }

} //Class