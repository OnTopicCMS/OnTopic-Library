/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections.Specialized;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Associations;

/*==============================================================================================================================
| CLASS: TOPIC RELATIONSHIP MULTIMAP
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a simple interface for accessing collections of topic collections.
/// </summary>
/// <remarks>
///   The <see cref="TopicRelationshipMultiMap"/> derives from <see cref="ReadOnlyTopicMultiMap"/> to provide read-only access
///   to the underlying <see cref="TopicMultiMap"/> collection, then acts as a façade for the write operations, thus not only
///   simplifying access to the <see cref="TopicMultiMap"/>, but also ensuring that business logic is enforced, such as local
///   state tracking and handling of reciprocal relationships.
/// </remarks>
public class TopicRelationshipMultiMap : ReadOnlyTopicMultiMap, ITrackDirtyKeys {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      Topic                           _parent;
  readonly                      bool                            _isIncoming;
  readonly                      DirtyKeyCollection              _dirtyKeys                      = new();
  readonly                      TopicMultiMap                   _storage;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicRelationshipMultiMap"/>.
  /// </summary>
  /// <remarks>
  ///   The constructor requires a reference to a <see cref="Topic"/> instance, which the related topics are to be associated
  ///   with. This will be used when setting incoming relationships. In addition, a <see cref="TopicRelationshipMultiMap"/>
  ///   may be set as <paramref name="isIncoming"/> if it is specifically intended to track incoming relationships; when set,
  ///   <see cref="SetValue(String, Topic, Boolean?)"/> and <see cref="Remove(String, Topic)"/> won't set the reciprocal
  ///   relationship, since <paramref name="parent"/> in this case represents that reciprocal.
  /// </remarks>
  internal TopicRelationshipMultiMap(Topic parent, bool isIncoming = false): base(new()) {
    _parent                     = parent;
    _isIncoming                 = isIncoming;
    _storage                    = base.Source;
  }

  /*============================================================================================================================
  | METHOD: CLEAR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Removes every <see cref="Topic"/> object across all relationship keys.
  /// </summary>
  /// <remarks>
  ///   Delegates to <see cref="Clear(String)"/> for each key, which handles both the <c>isDirty</c> as well as the removal of
  ///   reciprocal relationships in <see cref="Topic.IncomingRelationships"/>.
  /// </remarks>
  internal void Clear() {
    foreach (var key in Keys) {
      Clear(key);
    }
  }

  /// <summary>
  ///   Removes all <see cref="Topic"/> objects grouped by a specific <paramref name="relationshipKey"/>.
  /// </summary>
  /// <remarks>
  ///   If there are any <see cref="Topic"/> objects in the specified <paramref name="relationshipKey"/>, then the <see cref=
  ///   "TopicRelationshipMultiMap"/> will be marked as <see cref="TopicRelationshipMultiMap.IsDirty()"/>. Delegates to <see
  ///   cref="Remove(String, Topic)"/> for each entry so the reciprocal relationship is also removed from each target's <see
  ///   cref="Topic.IncomingRelationships"/>.
  /// </remarks>
  /// <param name="relationshipKey">The key of the relationship to be cleared.</param>
  public void Clear(string relationshipKey) {
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
    foreach (var topic in _storage.GetValues(relationshipKey).ToArray()) {
      Remove(relationshipKey, topic);
    }
  }

  /// <inheritdoc cref="Clear(String)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(ClearTopics)} method has been renamed to {nameof(Clear)}.", true)]
  public void ClearTopics(string relationshipKey) => Clear(relationshipKey);

  /*============================================================================================================================
  | METHOD: REMOVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Removes a specific <see cref="Topic"/> object associated with a specific <paramref name="relationshipKey"/>.
  /// </summary>
  /// <param name="relationshipKey">The key of the relationship.</param>
  /// <param name="topic">The <see cref="Topic"/> to be removed.</param>
  /// <returns>
  ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the specified <paramref name="
  ///   relationshipKey"/> or the <paramref name="topic"/> cannot be found.
  /// </returns>
  public bool Remove(string relationshipKey, Topic topic) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove reciprocal relationship, if appropriate
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!_isIncoming) {
      topic.IncomingRelationships.Remove(relationshipKey, _parent);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate relationshipKey
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!_storage.Contains(relationshipKey, topic)) {
      return false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove relationship
    \-------------------------------------------------------------------------------------------------------------------------*/
    _dirtyKeys.MarkAs(relationshipKey, markDirty: !_parent.IsNew);
    _storage.Remove(relationshipKey, topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove true
    \-------------------------------------------------------------------------------------------------------------------------*/
    return true;

  }

  /// <inheritdoc cref="Remove(String, Topic)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(RemoveTopic)} method has been renamed to {nameof(Remove)}.", true)]
  public bool RemoveTopic(string relationshipKey, Topic topic) => Remove(relationshipKey, topic);

  /*============================================================================================================================
  | METHOD: SET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Ensures that a <see cref="Topic"/> is associated with the specified <paramref name="relationshipKey"/>.
  /// </summary>
  /// <remarks>
  ///   If a relationship by a given <paramref name="relationshipKey"/> is not currently established, it will automatically be
  ///   created.
  /// </remarks>
  /// <param name="relationshipKey">The key of the relationship.</param>
  /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
  /// <param name="markDirty">
  ///   Optionally forces the collection to an <see cref="IsDirty()"/> state, assuming the topic was set.
  /// </param>
  public void SetValue(string relationshipKey, Topic topic, bool? markDirty = null) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
    Contract.Requires(topic);
    TopicFactory.ValidateKey(relationshipKey);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Add relationship
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topics                  = _storage.GetValues(relationshipKey);
    var wasDirty                = _dirtyKeys.IsDirty(relationshipKey);
    if (!topics.Contains(topic)) {
      _storage.Add(relationshipKey, topic);
      if (!_parent.IsNew && !topic.IsNew && markDirty.HasValue && !markDirty.Value && !wasDirty) {
        MarkClean(relationshipKey);
      }
      else {
        _dirtyKeys.MarkDirty(relationshipKey);
      }

      // Remove any pending deferred entry for this relationship/target pair
      Deferred.Remove(relationshipKey, topic.Id);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Create reciprocal relationship, if appropriate
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!_isIncoming) {
      topic.IncomingRelationships.SetValue(relationshipKey, _parent, markDirty);
    }

  }

  /// <inheritdoc cref="SetValue(String, Topic, Boolean?)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(SetTopic)} method has been renamed to {nameof(SetValue)}.", true)]
  public void SetTopic(string relationshipKey, Topic topic, bool? isDirty = null) => SetValue(relationshipKey, topic, isDirty);

  /*============================================================================================================================
  | PROPERTY: LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indicates whether the collection has been populated from the underlying <see cref="Repositories.ITopicRepository" />,
  ///   allowing callers to distinguish data that is present and authoritative from data that must still be fetched.
  /// </summary>
  /// <remarks>
  ///   Returns <see cref="LoadState.NotLoaded"/> when <see cref="Deferred"/> contains values, meaning one or more relationships
  ///   aren't yet available and must be lazy loaded. Returns <see cref="LoadState.Loaded"/> once <see cref="Deferred"/> is
  ///   empty, meaning all targets have been loaded. While <see cref="LoadState.NotLoaded"/>, the <see cref="ITopicRepository"/>
  ///   will not delete unmatched relationships on save, preventing unintended data loss.
  /// </remarks>
  public LoadState LoadState => Deferred.Count > 0 ? LoadState.NotLoaded : LoadState.Loaded;

  /*============================================================================================================================
  | PROPERTY: DEFERRED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Collects relationship targets that were absent from the topic graph during an <see cref="ITopicRepository"/> load,
  ///   pending resolution via lazy-loading.
  /// </summary>
  /// <remarks>
  ///   Written to by the <see cref="ITopicRepository"/> when a relationship target cannot be found in the current <see cref=
  ///   "TopicIndex"/>. The <see cref="Repositories.ITopicLazyLoader.EnsureLoaded(Topic, TopicPayload)"/> resolves each entry
  ///   by calling the <see cref="ITopicRepository"/>'s <c>Load()</c> method, assuming the topics haven't since been introduced
  ///   to the topic graph.
  /// </remarks>
  public DeferredAssociationCollection Deferred { get; } = new();

  /*============================================================================================================================
  | METHOD: IS DIRTY?
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public bool IsDirty() => _dirtyKeys.IsDirty();

  /// <inheritdoc/>
  public bool IsDirty(string key) => _dirtyKeys.IsDirty(key);

  /*============================================================================================================================
  | METHOD: MARK CLEAN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public void MarkClean() {
    if (_parent.IsNew) {
      return;
    }
    foreach (var relationship in _storage) {
      if (!relationship.Values.AnyNew()) {
        _dirtyKeys.MarkClean(relationship.Key);
      }
    }
  }

  /// <inheritdoc/>
  public void MarkClean(string  key) {
    if (_parent.IsNew) {
      return;
    }
    if (Contains(key) && !_storage[key].Values.AnyNew()) {
      _dirtyKeys.MarkClean(key);
    }
  }

} //Class