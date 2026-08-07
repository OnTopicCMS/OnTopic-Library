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
  /// <param name="isDirty">Determines that the deferred entry is a modification yet to be saved.</param>
  public void SetValue(string key, int topicId, bool isDirty = false) {
    Remove(key, _singleValued? null : topicId);
    Add(new(key, topicId, isDirty));
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

  /*============================================================================================================================
  | METHOD: REPLACE ALL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Replaces every entry in the collection with the supplied <paramref name="source"/>, marking each as <see cref=
  ///   "DeferredAssociation.IsDirty"/> regardless of the source entry's own value.
  /// </summary>
  /// <remarks>
  ///   Intended for merging a detached topic's deferred associations onto a live topic's own <see cref=
  ///   "TopicRelationshipMultiMap.Deferred"/> or <see cref="TopicReferenceCollection.Deferred"/> wholesale, without requiring
  ///   the caller to individually clear and then <see cref="SetValue(String, Int32, Boolean)"/> for each entry. Every entry is
  ///   marked dirty because, by definition, a caller merging a new source into a resident collection is introducing a change
  ///   that isn't yet reflected in the persistence store, even though <paramref name="source"/>'s own entries are themselves
  ///   presumed clean in their original context (e.g., a detached historical version, freshly loaded from the persistence store
  ///   as-is). Because associations are saved wholesale, there's no attempt to differentiate between preexisting and genuine
  ///   changes as would be required for e.g., Indexed Attributes, which are only persisted if they are individually dirty.
  /// </remarks>
  /// <param name="source">The <see cref="DeferredAssociation"/> entries to populate the collection with.</param>
  internal void ReplaceAll(IEnumerable<DeferredAssociation> source) {
    Clear();
    foreach (var entry in source) {
      SetValue(entry.Key, entry.TopicId, isDirty: true);
    }
  }

} //Class