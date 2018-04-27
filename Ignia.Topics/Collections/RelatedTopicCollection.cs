/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Ignia.Topics.Collections {

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
    Topic                       _parent                         = null;
    bool                        _isIncoming                     = false;

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
    ///   with. This will be used when setting incoming relationships. In addition, a <see cref="RelatedTopicCollection"/> may be
    ///   set as <paramref name="isIncoming"/> if it is specifically intended to track incoming relationships; if this is not
    ///   set, then it will not allow incoming relationships to be set via the internal
    ///   <see cref="SetTopic(String, Topic, Boolean)"/> overload.
    /// </remarks>
    public RelatedTopicCollection(Topic parent, bool isIncoming = false) : base(StringComparer.OrdinalIgnoreCase) {
      _parent = parent;
      _isIncoming = isIncoming;
    }

    /*==========================================================================================================================
    | PROPERTY: KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of relationship scopes available.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of relationship scopes.
    /// </returns>
    public ReadOnlyCollection<string> Keys {
      get {
        Contract.Ensures(Contract.Result<ReadOnlyCollection<string>>() != null);
        return new ReadOnlyCollection<string>(Items.Select(t => t.Name).ToList());
      }
    }

    /*==========================================================================================================================
    | METHOD: GET ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of scope.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection<Topic> GetAllTopics() {
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Topic>>() != null);
      var topics = new TopicCollection();
      foreach (var topicCollection in this) {
        foreach (var topic in topicCollection) {
          if (topicCollection.Contains(topic) && !topics.Contains(topic)) {
            topics.Add(topic);
          }
        }
      }
      return new ReadOnlyTopicCollection<Topic>(topics);
    }

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of scope, filtered by content type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection<Topic> GetAllTopics(string contentType) {
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Topic>>() != null);
      var topics = GetAllTopics().Where(t => t.ContentType == contentType);
      return ReadOnlyTopicCollection<Topic>.FromList(topics.ToList());
    }

    /*==========================================================================================================================
    | METHOD: GET TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific relationship scope.
    /// </summary>
    /// <remarks>
    ///   Returns a reference to the underlying <see cref="NamedTopicCollection"/>; modifications to this collection will modify
    ///   the <see cref="Topic"/>'s <see cref="Topic.Relationships"/>. As such, this should be used with care.
    /// </remarks>
    /// <param name="scope">The scope of the relationship to be returned.</param>
    public NamedTopicCollection GetTopics(string scope) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      if (Contains(scope)) {
        return this[scope];
      }
      return new NamedTopicCollection();
    }

    /*==========================================================================================================================
    | METHOD: CLEAR TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific relationship scope.
    /// </summary>
    /// <param name="scope">The scope of the relationship to be cleared.</param>
    public void ClearTopics(string scope) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      if (Contains(scope)) {
        this[scope].Clear();
      }
    }

    /*==========================================================================================================================
    | METHOD: REMOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship scope.
    /// </summary>
    /// <param name="scope">The scope of the relationship.</param>
    /// <param name="topicKey">The key of the topic to be removed.</param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship scope or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    public bool RemoveTopic(string scope, string topicKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(topicKey));
      if (Contains(scope)) {
        var topics = this[scope];
        return topics.Remove(topicKey);
      }
      return false;
    }

    /// <summary>
    ///   Removes a specific <see cref="Topic"/> object associated with a specific relationship scope.
    /// </summary>
    /// <param name="scope">The scope of the relationship.</param>
    /// <param name="topic">The topic to be removed.</param>
    /// <returns>
    ///   Returns true if the <see cref="Topic"/> is removed; returns false if either the relationship scope or the
    ///   <see cref="Topic"/> cannot be found.
    /// </returns>
    public bool RemoveTopic(string scope, Topic topic) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(topic != null);
      if (Contains(scope)) {
        var topics = this[scope];
        return topics.Remove(topic);
      }
      return false;
    }

    /*==========================================================================================================================
    | METHOD: SET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="Topic"/> is associated with the specified relationship scope.
    /// </summary>
    /// <remarks>
    ///   If a relationship by a given scope is not currently established, it will automatically be created.
    /// </remarks>
    /// <param name="scope">The scope of the relationship.</param>
    /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
    public void SetTopic(string scope, Topic topic) => SetTopic(scope, topic, false);

    /// <summary>
    ///   Ensures that an incoming <see cref="Topic"/> is associated with the specified relationship scope.
    /// </summary>
    /// <remarks>
    ///   If a relationship by a given scope is not currently established, it will automatically be c.
    /// </remarks>
    /// <param name="scope">The scope of the relationship.</param>
    /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
    /// <param name="isIncoming">
    ///   Notes that this is setting an internal relationship, and thus shouldn't set the reciprocal relationship.
    /// </param>
    public void SetTopic(string scope, Topic topic, bool isIncoming) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(topic != null);
      TopicFactory.ValidateKey(scope);

      /*------------------------------------------------------------------------------------------------------------------------
      | Add relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(scope)) {
        Add(new NamedTopicCollection(scope));
      }
      var topics = this[scope];
      if (!topics.Contains(topic.Key)) {
        topics.Add(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create reciprocal relationship, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isIncoming) {
        if (_isIncoming) {
          throw new ArgumentException("You are attempting to set an incoming relationship on a RelatedTopicCollection that is not flagged as IsIncoming", "isIncoming");
        }
        topic.IncomingRelationships.SetTopic(scope, _parent, true);
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
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Name;
    }

  } //Class

} //Namespace