/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Collections.Specialized;
using OnTopic.Repositories;

namespace OnTopic.Associations;

/*==============================================================================================================================
| CLASS: TOPIC REFERENCE COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a collection of <see cref="Topic"/> objects associated with particular reference keys.
/// </summary>
public class TopicReferenceCollection : TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicReferenceCollection"/>.
  /// </summary>
  /// <param name="parentTopic">A reference to the topic that the current collection is bound to.</param>
  public TopicReferenceCollection(Topic parentTopic) : base(parentTopic) { }

  /*============================================================================================================================
  | PROPERTY: PARENT COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute>? ParentCollection =>
    AssociatedTopic.Parent?.References;

  /*============================================================================================================================
  | PROPERTY: BASE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute>? BaseCollection =>
    AssociatedTopic.BaseTopic?.References;

  /*============================================================================================================================
  | PROPERTY: LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indicates whether the collection has been populated from the underlying <see cref="Repositories.ITopicRepository" />,
  ///   allowing callers to distinguish data that is present and authoritative from data that must still be fetched.
  /// </summary>
  /// <remarks>
  ///   Returns <see cref="LoadState.NotLoaded"/> when <see cref="Deferred"/> contains values, meaning one or more references
  ///   aren't yet available and must be lazy loaded. Returns <see cref="LoadState.Loaded"/> once <see cref="Deferred"/> is
  ///   empty, meaning all targets have been loaded. While <see cref="LoadState.NotLoaded"/>, the <see cref="ITopicRepository"/>
  ///   will not delete unmatched references on save, preventing unintended data loss.
  /// </remarks>
  public LoadState LoadState => Deferred.Count > 0 ? LoadState.NotLoaded : LoadState.Loaded;

  /*============================================================================================================================
  | PROPERTY: DEFERRED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Collects reference targets that were absent from the topic graph during an <see cref="ITopicRepository"/> load, pending
  ///   resolution via lazy-loading.
  /// </summary>
  /// <remarks>
  ///   Written to by the <see cref="ITopicRepository"/> when a reference target cannot be found in the current <see cref=
  ///   "TopicIndex"/>. The <see cref="Repositories.ITopicLazyLoader.EnsureLoaded(Topic, TopicPayload)"/> resolves each entry
  ///   by calling the <see cref="ITopicRepository"/>'s <c>Load()</c> method, assuming the topics haven't since been introduced
  ///   to the topic graph.
  /// </remarks>
  public DeferredAssociationCollection Deferred { get; } = new(singleValued: true);

  /*============================================================================================================================
  | INSERT ITEM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override void InsertItem(int index, TopicReferenceRecord item) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(item, nameof(item));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide base logic
    \-------------------------------------------------------------------------------------------------------------------------*/
    base.InsertItem(index, item);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove any pending deferred entry for this reference key
    \-------------------------------------------------------------------------------------------------------------------------*/
    Deferred.Remove(item.Key);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle recipricol references
    \-------------------------------------------------------------------------------------------------------------------------*/
    item.Value?.IncomingRelationships.SetValue(item.Key, AssociatedTopic, null, true);

  }

  /*============================================================================================================================
  | OVERRIDE: SET ITEM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override void SetItem(int index, TopicReferenceRecord item) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(item, nameof(item));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Get existing reference
    \-------------------------------------------------------------------------------------------------------------------------*/
    var existingItem            = this[index];

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide base logic
    \-------------------------------------------------------------------------------------------------------------------------*/
    base.SetItem(index, item);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove any pending deferred entry for this reference key
    \-------------------------------------------------------------------------------------------------------------------------*/
    Deferred.Remove(item.Key);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle recipricol references
    \-------------------------------------------------------------------------------------------------------------------------*/
    existingItem.Value?.IncomingRelationships.Remove(existingItem.Key, AssociatedTopic, true);
    item?.Value?.IncomingRelationships.SetValue(item.Key, AssociatedTopic, null, true);

  }

  /*============================================================================================================================
  | OVERRIDE: REMOVE ITEM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override sealed void RemoveItem(int index) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle recipricol references
    \-------------------------------------------------------------------------------------------------------------------------*/
    var existing                = this[index];

    existing.Value?.IncomingRelationships.Remove(existing.Key, AssociatedTopic, true);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide base logic
    \-------------------------------------------------------------------------------------------------------------------------*/
    base.RemoveItem(index);

  }

} //Class