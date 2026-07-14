/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace OnTopic.Associations;

/*==============================================================================================================================
| CLASS: DEFERRED ASSOCIATION COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a collection of <see cref="DeferredAssociation"/> records pending resolution via lazy loading; i.e.,
///   relationships or references to targets that weren't available in the topic graph when first loaded.
/// </summary>
/// <remarks>
///   Deduplicates on add so that repeated loads don't accumulate redundant entries. Identity isn't uniform: References are
///   single-valued, so an entry's identity is its <see cref="DeferredAssociation.Key"/> alone; relationships are multivalued,
///   so an entry's identity is the full <see cref="DeferredAssociation.Key"/> and <see cref="DeferredAssociation.TopicId"/>
///   pair. This mirrors the asymmetry already represented in <see cref="TopicReferenceCollection"/> and <see cref=
///   "TopicRelationshipMultiMap"/>.
/// </remarks>
public class DeferredAssociationCollection: Collection<DeferredAssociation> {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      bool                            _singleValued;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="DeferredAssociationCollection"/> class.
  /// </summary>
  /// <param name="singleValued">
  ///   Determines whether entries are identified by <see cref="DeferredAssociation.Key"/> alone (<c>true</c> for references),
  ///   or by the full <see cref="DeferredAssociation.Key"/> and <see cref="DeferredAssociation.TopicId"/> pair (<c>false</c>
  ///   for relationships).
  /// </param>
  public DeferredAssociationCollection(bool singleValued = false) {
    _singleValued               = singleValued;
  }

  /*============================================================================================================================
  | METHOD: SET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Registers a deferred association, replacing any existing entry that shares the new entry's identity.
  /// </summary>
  /// <param name="key">The relationship or reference key under which the association is registered.</param>
  /// <param name="topicId">The <see cref="Topic.Id"/> of the target topic to be resolved.</param>
  public void SetValue(string key, int topicId) {
    Remove(key, _singleValued? null : topicId);
    Add(new(key, topicId));
  }

  /*============================================================================================================================
  | METHOD: REMOVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Removes the deferred association(s) matching the given identity.
  /// </summary>
  /// <remarks>
  ///   When <paramref name="topicId"/> is omitted, every entry registered under <paramref name="key"/> is removed; otherwise
  ///   only the exact <paramref name="key"/> and <paramref name="topicId"/> pair is removed.
  /// </remarks>
  /// <param name="key">The relationship or reference key of the association(s) to remove.</param>
  /// <param name="topicId">The <see cref="Topic.Id"/> of the target topic, if scoping the removal to a single entry.</param>
  /// <returns>Returns <c>true</c> if one or more entries were removed; otherwise, <c>false</c>.</returns>
  public bool Remove(string key, int? topicId = null) {
    var removed                 = false;
    for (var i                   = Count - 1; i >= 0; i--) {
      if (this[i].Key == key && (topicId is null || this[i].TopicId == topicId)) {
        RemoveAt(i);
        removed                 = true;
        if (topicId is not null) {
          break;
        }
      }
    }
    return removed;
  }

} //Class