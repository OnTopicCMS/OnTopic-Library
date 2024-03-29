﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Associations;

#pragma warning disable IDE0060 // Remove unused parameter

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: RELATED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple interface for accessing collections of topic collections.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Obsolete($"{nameof(RelatedTopicCollection)} has been migrated to the new {nameof(TopicRelationshipMultiMap)}", true)]
  public class RelatedTopicCollection : KeyedCollection<string, NamedTopicCollection> {

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
    ///   set, then it will not allow incoming relationships to be set via the internal
    ///   <see cref="SetTopic(String, Topic, Boolean, Boolean?)"/> overload.
    /// </remarks>
    public RelatedTopicCollection(Topic parent, bool isIncoming = false) : base(StringComparer.OrdinalIgnoreCase) {
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
    public ReadOnlyTopicCollection GetAllTopics() => throw new NotImplementedException();

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of relationship key, filtered by content
    ///   type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection GetAllTopics(string contentType) => throw new NotImplementedException();

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
    public NamedTopicCollection GetTopics(string relationshipKey) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: CLEAR TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific relationship key.
    /// </summary>
    /// <param name="relationshipKey">The key of the relationship to be cleared.</param>
    public void ClearTopics(string relationshipKey) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: REMOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
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
    public bool RemoveTopic(string relationshipKey, string topicKey, bool isIncoming = false) =>
      throw new NotImplementedException();

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
    public bool RemoveTopic(string relationshipKey, Topic topic, bool isIncoming = false) => throw new NotImplementedException();

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
    public void SetTopic(string relationshipKey, Topic topic) => SetTopic(relationshipKey, topic, false);

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
    public void SetTopic(string relationshipKey, Topic topic, bool isIncoming, bool? isDirty = null) =>
      throw new NotImplementedException();

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
    protected override void InsertItem(int index, NamedTopicCollection item) => throw new NotImplementedException();

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

#pragma warning restore IDE0060 // Remove unused parameter