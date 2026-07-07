/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
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
  ///   may be set as <paramref name="isIncoming"/> if it is specifically intended to track incoming relationships; if this is
  ///   not set, then it will not allow incoming relationships to be set via the internal <see cref=
  ///   "SetValue(String, Topic, Boolean?, Boolean)"/> overload.
  /// </remarks>
  public TopicRelationshipMultiMap(Topic parent, bool isIncoming = false): base(new()) {
    _parent                     = parent;
    _isIncoming                 = isIncoming;
    _storage                    = base.Source;
  }

  /*============================================================================================================================
  | METHOD: CLEAR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Removes all <see cref="Topic"/> objects grouped by a specific <paramref name="relationshipKey"/>.
  /// </summary>
  /// <remarks>
  ///   If there are any <see cref="Topic"/> objects in the specified <paramref name="relationshipKey"/>, then the <see cref="
  ///   TopicRelationshipMultiMap"/> will be marked as <see cref="TopicRelationshipMultiMap.IsDirty()"/>.
  /// </remarks>
  /// <param name="relationshipKey">The key of the relationship to be cleared.</param>
  public void Clear(string relationshipKey) {
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
    if (_storage.Contains(relationshipKey)) {
      var relationship = _storage.GetValues(relationshipKey);
      if (relationship.Count >  0) {
        _dirtyKeys.MarkAs(relationshipKey, markDirty: !_parent.IsNew);
      }
      _storage.Clear(relationshipKey);
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
  public bool Remove(string relationshipKey, Topic topic) => Remove(relationshipKey, topic, false);

  /// <summary>
  ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship key.
  /// </summary>
  /// <param name="relationshipKey">The key of the relationship.</param>
  /// <param name="topic">The topic to be removed.</param>
  /// <param name="isIncoming">
  ///   Notes that this is setting an internal relationship, and thus shouldn't set the reciprocal relationship.
  /// </param>
  /// <returns>
  ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship key or the
  ///   <see cref="Topic"/> cannot be found.
  /// </returns>
  internal bool Remove(string relationshipKey, Topic topic, bool isIncoming) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove reciprocal relationship, if appropriate
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!isIncoming) {
      if (_isIncoming) {
        throw new InvalidOperationException(
          "You are attempting to remove an incoming relationship on a TopicRelationshipMultiMap that is not flagged as " +
          nameof(isIncoming)
        );
      }
      topic.IncomingRelationships.Remove(relationshipKey, _parent, true);
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

  /// <inheritdoc cref="Remove(String, Topic, Boolean)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(RemoveTopic)} method has been renamed to {nameof(Remove)}.", true)]
  public bool RemoveTopic(string relationshipKey, Topic topic, bool isIncoming) =>
    Remove(relationshipKey, topic, isIncoming);

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
  public void SetValue(string relationshipKey, Topic topic, bool? markDirty = null)
    => SetValue(relationshipKey, topic, markDirty, false);

  /// <summary>
  ///   Ensures that an incoming <see cref="Topic"/> is associated with the specified <paramref name="relationshipKey"/>.
  /// </summary>
  /// <remarks>
  ///   If a relationship by a given <paramref name="relationshipKey"/> is not currently established, it will automatically be
  ///   created.
  /// </remarks>
  /// <param name="relationshipKey">The key of the relationship.</param>
  /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
  /// <param name="isIncoming">
  ///   Notes that this is setting an internal relationship, and thus shouldn't set the reciprocal relationship.
  /// </param>
  /// <param name="markDirty">
  ///   Optionally forces the collection to an <see cref="IsDirty()"/> state, assuming the topic was set.
  /// </param>
  internal void SetValue(string relationshipKey, Topic topic, bool? markDirty, bool isIncoming) {

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
      for (var i = Deferred.Count - 1; i >= 0; i--) {
        if (Deferred[i].Key == relationshipKey && Deferred[i].TopicId == topic.Id) {
          Deferred.RemoveAt(i);
          break;
        }
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Create reciprocal relationship, if appropriate
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!isIncoming) {
      if (_isIncoming) {
        throw new InvalidOperationException(
          "You are attempting to set an incoming relationship on a TopicRelationshipMultiMap that is not flagged as " +
          nameof(isIncoming)
        );
      }
      topic.IncomingRelationships.SetValue(relationshipKey, _parent, markDirty, true);
    }

  }

  /// <inheritdoc cref="SetValue(String, Topic, Boolean?)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(SetTopic)} method has been renamed to {nameof(SetValue)}.", true)]
  public void SetTopic(string relationshipKey, Topic topic, bool? isDirty = null) => SetValue(relationshipKey, topic, isDirty);

  /// <inheritdoc cref="SetValue(String, Topic, Boolean?, Boolean)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(SetTopic)} method has been renamed to {nameof(SetValue)}.", true)]
  public void SetTopic(string relationshipKey, Topic topic, bool? isDirty, bool isIncoming) =>
    SetValue(relationshipKey, topic, isDirty, isIncoming);

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
  ///   "TopicIndex"/>. The <see cref="Repositories.ITopicLoadResolver.EnsureLoaded(Topic, TopicPayload)"/> resolves each entry
  ///   by calling the <see cref="ITopicRepository"/>'s <c>Load()</c> method, assuming the topics haven't since been introduced
  ///   to the topic graph.
  /// </remarks>
  public Collection<DeferredAssociation> Deferred { get; } = new();

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