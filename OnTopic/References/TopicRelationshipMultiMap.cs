/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using OnTopic.Collections;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.References {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple interface for accessing collections of topic collections.
  /// </summary>
  public class RelatedTopicCollection : ReadOnlyTopicMultiMap {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    Topic                           _parent;
    readonly                    bool                            _isIncoming;
    readonly                    List<string>                    _isDirty                        = new();
    readonly                    TopicMultiMap                   _storage                        = new();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="RelatedTopicCollection"/>.
    /// </summary>
    /// <remarks>
    ///   The constructor requires a reference to a <see cref="Topic"/> instance, which the related topics are to be associated
    ///   with. This will be used when setting incoming relationships. In addition, a <see cref="RelatedTopicCollection"/> may
    ///   be set as <paramref name="isIncoming"/> if it is specifically intended to track incoming relationships; if this is not
    ///   set, then it will not allow incoming relationships to be set via the internal <see cref=
    ///   "SetTopic(String, Topic, Boolean?, Boolean)"/> overload.
    /// </remarks>
    public RelatedTopicCollection(Topic parent, bool isIncoming = false): base() {
      _parent                   = parent;
      _isIncoming               = isIncoming;
      base.Source               = _storage;
    }

    /*==========================================================================================================================
    | METHOD: CLEAR TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific relationship key.
    /// </summary>
    /// <remarks>
    ///   If there are any <see cref="Topic"/> objects in the specified <paramref name="relationshipKey"/>, then the <see cref="
    ///   RelatedTopicCollection"/> will be marked as <see cref="RelatedTopicCollection.IsDirty()"/>.
    /// </remarks>
    /// <param name="relationshipKey">The key of the relationship to be cleared.</param>
    public void ClearTopics(string relationshipKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      if (_storage.Contains(relationshipKey)) {
        var relationship = _storage.GetTopics(relationshipKey);
        if (relationship.Count > 0) {
          MarkDirty(relationshipKey);
        }
        _storage.Clear(relationshipKey);
      }
    }

    /*==========================================================================================================================
    | METHOD: REMOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topic">The <see cref="Topic"/> to be removed.</param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship key or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    public bool RemoveTopic(string relationshipKey, Topic topic) => RemoveTopic(relationshipKey, topic, false);

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
    internal bool RemoveTopic(string relationshipKey, Topic topic, bool isIncoming) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      Contract.Requires(topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove reciprocal relationship, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isIncoming) {
        if (_isIncoming) {
          throw new InvalidOperationException(
            "You are attempting to remove an incoming relationship on a RelatedTopicCollection that is not flagged as " +
            nameof(isIncoming)
          );
        }
        topic.IncomingRelationships.RemoveTopic(relationshipKey, _parent, true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate relationshipKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_storage.Contains(relationshipKey, topic)) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      MarkDirty(relationshipKey);
      _storage.Remove(relationshipKey, topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove true
      \-----------------------------------------------------------------------------------------------------------------------*/
      return true;

    }

    /*==========================================================================================================================
    | METHOD: SET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="Topic"/> is associated with the specified relationship key.
    /// </summary>
    /// <remarks>
    ///   If a relationship by a given key is not currently established, it will automatically be created.
    /// </remarks>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
    /// <param name="isDirty">
    ///   Optionally forces the collection to an <see cref="IsDirty()"/> state, assuming the topic was set.
    /// </param>
    public void SetTopic(string relationshipKey, Topic topic, bool? isDirty = null)
      => SetTopic(relationshipKey, topic, isDirty, false);

    /// <summary>
    ///   Ensures that an incoming <see cref="Topic"/> is associated with the specified relationship key.
    /// </summary>
    /// <remarks>
    ///   If a relationship by a given key is not currently established, it will automatically be c.
    /// </remarks>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
    /// <param name="isIncoming">
    ///   Notes that this is setting an internal relationship, and thus shouldn't set the reciprocal relationship.
    /// </param>
    /// <param name="isDirty">
    ///   Optionally forces the collection to an <see cref="IsDirty()"/> state, assuming the topic was set.
    /// </param>
    internal void SetTopic(string relationshipKey, Topic topic, bool? isDirty, bool isIncoming) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      Contract.Requires(topic);
      TopicFactory.ValidateKey(relationshipKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Add relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = _storage.GetTopics(relationshipKey);
      var wasDirty              = _isDirty.Contains(relationshipKey);
      if (!topics.Contains(topic)) {
        _storage.Add(relationshipKey, topic);
        if (isDirty.HasValue && !isDirty.Value && !wasDirty) {
          MarkClean(relationshipKey);
        }
        else {
          MarkDirty(relationshipKey);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create reciprocal relationship, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isIncoming) {
        if (_isIncoming) {
          throw new InvalidOperationException(
            "You are attempting to set an incoming relationship on a RelatedTopicCollection that is not flagged as " +
            nameof(isIncoming)
          );
        }
        topic.IncomingRelationships.SetTopic(relationshipKey, _parent, isDirty, true);
      }

    }

    /*==========================================================================================================================
    | METHOD: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if any of the relationships have been modified; if they have, returns <c>true</c>.
    /// </summary>
    public bool IsDirty() => _isDirty.Count > 0;

    /*==========================================================================================================================
    | METHOD: MARK DIRTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates each of the relationships to determine if any of them are set to <see cref="IsDirty()"/>. If they are,
    ///   returns <c>true</c>.
    /// </summary>
    private void MarkDirty(string relationshipKey) {
      if (!_isDirty.Contains(relationshipKey)) {
        _isDirty.Add(relationshipKey);
      }
    }

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Marks the relationships collections as clean.
    /// </summary>
    public void MarkClean() => _isDirty.Clear();

    /// <summary>
    ///   Removes the <paramref name="relationshipKey"/> from the <see cref="_isDirty"/> collection, if it exists.
    /// </summary>
    public void MarkClean(string relationshipKey) {
      if (_isDirty.Contains(relationshipKey)) {
        _isDirty.Remove(relationshipKey);
      }
    }

  } //Class
} //Namespace