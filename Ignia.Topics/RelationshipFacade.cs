/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: RELATIONSHIP FACADE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple interface to accessing the underlying collection of topic relationships.
  /// </summary>
  public class RelationshipFacade {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    Dictionary<string, Topic>   _relationships                  = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);
    Topic                       _parent                         = null;
    bool                        _isIncoming                     = false;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="RelationshipFacade"/>.
    /// </summary>
    /// <remarks>
    ///   The constructor requires a reference to a <see cref="Topic"/> instance, which the related topics are to be associated
    ///   with. This will be used when setting incoming relationships. In addition, a <see cref="RelationshipFacade"/> may be
    ///   set as <paramref name="isIncoming"/> if it is specifically intended to track incoming relationships; if this is not
    ///   set, then it will not allow incoming relationships to be set via the internal <see cref="Set(string, Topic, bool)"/>
    ///   overload.
    /// </remarks>
    public RelationshipFacade(Topic parent, bool isIncoming = false) {
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
    public IEnumerable<string> Keys {
      get {
        return _relationships.Keys;
      }
    }

    /*==========================================================================================================================
    | METHOD: CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the specified <paramref name="scope"/> if currently established.
    /// </summary>
    /// <remarks>
    ///   Scopes will automatically be established when a new <see cref="Topic"/> instance is added via
    ///   <see cref="Set(string, Topic)"/>, assuming that scope was not previously established.
    /// </remarks>
    /// <param name="scope">The scope of the relationship to be evaluated.</param>
    /// <returns>
    ///   Returns true if the specified <paramref name="scope"/> is currently established.
    /// </returns>
    public bool Contains(string scope) {
      return _relationships.ContainsKey(scope);
    }

    /*==========================================================================================================================
    | METHOD: GET ALL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of scope.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public IEnumerable<Topic> GetAll() {
      var topics = new List<Topic>();
      foreach (Topic topic in _relationships.Values) {
        topics.Union<Topic>(topic);
      }
      return topics;
    }

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of scope, filtered by content type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public IEnumerable<Topic> GetAll(string contentType) {
      return GetAll().Where(t => t.ContentType == contentType);
    }

    /*==========================================================================================================================
    | METHOD: GET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific relationship scope.
    /// </summary>
    /// <param name="scope">The scope of the relationship to be returned.</param>
    public IEnumerable<Topic> Get(string scope) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      if (_relationships.ContainsKey(scope)) {
        return _relationships[scope];
      }
      return new List<Topic>();
    }

    /*==========================================================================================================================
    | METHOD: CLEAR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific relationship scope.
    /// </summary>
    /// <param name="scope">The scope of the relationship to be cleared.</param>
    public void Clear(string scope) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      if (_relationships.ContainsKey(scope)) {
        _relationships[scope].Clear();
      }
    }

    /*==========================================================================================================================
    | METHOD: REMOVE
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
    public bool Remove(string scope, string topicKey) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(topicKey));
      if (_relationships.ContainsKey(scope)) {
        var topics = _relationships[scope];
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
    public bool Remove(string scope, Topic topic) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(topic != null);
      if (_relationships.ContainsKey(scope)) {
        var topics = _relationships[scope];
        return topics.Remove(topic);
      }
      return false;
    }

    /*==========================================================================================================================
    | METHOD: SET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Ensures that a <see cref="Topic"/> is associated with the specified relationship scope.
    /// </summary>
    /// <remarks>
    ///   If a relationship by a given scope is not currently established, it will automatically be created.
    /// </remarks>
    /// <param name="scope">The scope of the relationship.</param>
    /// <param name="topic">The topic to be added, if it doesn't already exist.</param>
    public void Set(string scope, Topic topic) => Set(scope, topic, false);

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
    public void Set(string scope, Topic topic, bool isIncoming) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(topic != null);
      Topic.ValidateKey(scope);

      /*------------------------------------------------------------------------------------------------------------------------
      | Add relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_relationships.ContainsKey(scope)) {
        _relationships.Add(scope, Topic.Create(scope, "Container"));
      }
      var topics = _relationships[scope];
      topics.Add(topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Create reciprocal relationship, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isIncoming) {
        if (_isIncoming) {
          throw new ArgumentException("You are attempting to set an incoming relationship on a RelationshipFacade that is not flagged as IsIncoming", "isIncoming");
        }
        topic.IncomingRelationships.Set(scope, _parent, true);
      }

    }

  } //Class

} //Namespace