/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple interface for accessing collections of topic collections.
  /// </summary>
  public class RelatedTopicCollection : KeyedCollection<string, NamedTopicCollection> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    Topic                           _parent;
    readonly                    bool                            _isIncoming;

    /*==========================================================================================================================
    | DATA STORE
    \-------------------------------------------------------------------------------------------------------------------------*/

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
    public RelatedTopicCollection(Topic parent, bool isIncoming = false) : base(StringComparer.OrdinalIgnoreCase) {
      _parent = parent;
      _isIncoming = isIncoming;
    }

    /*==========================================================================================================================
    | PROPERTY: KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of relationship key available.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of relationship keys.
    /// </returns>
    public ReadOnlyCollection<string> Keys => new(Items.Select(t => t.Name).ToList());

    /*==========================================================================================================================
    | METHOD: GET ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of relationship key.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public IEnumerable<Topic> GetAllTopics() {
      var topics = new List<Topic>();
      foreach (var topicCollection in this) {
        foreach (var topic in topicCollection) {
          if (!topics.Contains(topic)) {
            topics.Add(topic);
          }
        }
      }
      return topics;
    }

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of relationship key, filtered by content
    ///   type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public IEnumerable<Topic> GetAllTopics(string contentType) => GetAllTopics().Where(t => t.ContentType == contentType);

    /*==========================================================================================================================
    | METHOD: GET TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific relationship key.
    /// </summary>
    /// <remarks>
    ///   Returns a reference to the underlying <see cref="NamedTopicCollection"/>; modifications to this collection will modify
    ///   the <see cref="Topic"/>'s <see cref="Topic.Relationships"/>. As such, this should be used with care.
    /// </remarks>
    /// <param name="relationshipKey">The key of the relationship to be returned.</param>
    public NamedTopicCollection GetTopics(string relationshipKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      if (Contains(relationshipKey)) {
        return this[relationshipKey];
      }
      return new();
    }

    /*==========================================================================================================================
    | METHOD: CLEAR TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship to be cleared.</param>
    public void ClearTopics(string relationshipKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      if (Contains(relationshipKey)) {
        this[relationshipKey].Clear();
      }
    }

    /*==========================================================================================================================
    | METHOD: REMOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topicKey">The key of the topic to be removed.</param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship key or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    public bool RemoveTopic(string relationshipKey, string topicKey) => RemoveTopic(relationshipKey, topicKey);

    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topicKey">The key of the topic to be removed.</param>
    /// <param name="isIncoming">
    ///   Notes that this is setting an internal relationship, and thus shouldn't set the reciprocal relationship.
    /// </param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship key or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    internal bool RemoveTopic(string relationshipKey, string topicKey, bool isIncoming = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(relationshipKey), nameof(relationshipKey));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(topicKey), nameof(topicKey));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate topic key
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = Contains(relationshipKey)? this[relationshipKey] : null;
      var topic                 = topics?.Contains(topicKey)?? false? topics[topicKey] : null;

      if (topics is null || topic is null) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Call overload
      \-----------------------------------------------------------------------------------------------------------------------*/
      return RemoveTopic(relationshipKey, topic, isIncoming);

    }

    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship.</param>
    /// <param name="topic">The topic to be removed.</param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship key or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    public bool RemoveTopic(string relationshipKey, Topic topic) => RemoveTopic(relationshipKey, topic);


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
    internal bool RemoveTopic(string relationshipKey, Topic topic, bool isIncoming = false) {

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
            "IsIncoming"
          );
        }
        topic.IncomingRelationships.RemoveTopic(relationshipKey, _parent, true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate relationshipKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = Contains(relationshipKey)? this[relationshipKey] : null;

      if (topics is null || !topics.Contains(topic)) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      topics.Remove(topic);

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
    ///   Optionally forces the collection to a <see cref="NamedTopicCollection.IsDirty"/> state, assuming the topic was set.
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
    ///   Optionally forces the collection to a <see cref="NamedTopicCollection.IsDirty"/> state, assuming the topic was set.
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
      if (!Contains(relationshipKey)) {
        Add(new(relationshipKey));
      }
      var topics = this[relationshipKey];
      if (!topics.Contains(topic.Key)) {
        topics.Add(topic);
        topics.IsDirty = isDirty?? topics.IsDirty;
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
        topic.IncomingRelationships.SetTopic(relationshipKey, _parent, true);
      }

    }

    /*==========================================================================================================================
    | METHOD: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Evaluates each of the child <see cref="NamedTopicCollection"/>s to determine if any of them are set to <see
    ///   cref="NamedTopicCollection.IsDirty"/>. If they are, returns <c>true</c>.
    /// </summary>
    public bool IsDirty() => Items.Any(r => r.IsDirty);

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Fires any time a <see cref="NamedTopicCollection"/> is added to the collection.</summary>
    /// <remarks>
    ///   Compared to the base implementation, will throw a specific <see cref="ArgumentException"/> error if a duplicate key is
    ///   inserted. This conveniently provides the name of the <see cref="NamedTopicCollection"/>, so it's clear what key is
    ///   being duplicated.
    /// </remarks>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The <see cref="NamedTopicCollection"/> instance to insert.</param>
    /// <exception cref="ArgumentException">
    ///   A NamedTopicCollection with the Name '{item.Name}' already exists in this RelatedTopicCollection. The existing key is
    ///   {this[item.Name].Name}'; the new item's is '{item.Name}'. This collection is associated with the '{GetUniqueKey()}'
    ///   Topic.
    /// </exception>
    protected override void InsertItem(int index, NamedTopicCollection item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(item, nameof(item));

      /*------------------------------------------------------------------------------------------------------------------------
      | Insert item, if it doesn't already exist
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(item.Name)) {
        base.InsertItem(index, item);
      }
      else {
        throw new ArgumentException(
          $"A {nameof(NamedTopicCollection)} with the Name '{item.Name}' already exists in this " +
          $"{nameof(RelatedTopicCollection)}. The existing key is '{this[item.Name].Name}'; the new item's is '{item.Name}'. " +
          $"This collection is associated with the '{_parent.GetUniqueKey()}' Topic.",
          nameof(item)
        );
      }
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a method for the <see cref="KeyedCollection{TKey, TItem}"/> to retrieve the key from the underlying
    ///   collection of objects, in this case <see cref="NamedTopicCollection"/>s.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(NamedTopicCollection item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Name;
    }

  } //Class
} //Namespace