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
  ///   Defaults to <see cref="LoadState.Loaded"/>. The repository sets this to <see cref="LoadState.NotLoaded"/> when any
  ///   referenced topic cannot be resolved to an in-memory instance during load. The persistence store may optionally
  ///   provide an indicator of the count without returning the full data, thus allowing this to be set to <see cref=
  ///   "LoadState.Loaded"/> if, in fact, there are no topic references. While in that state, the <see cref="ITopicRepository"/>
  ///   will not delete unmatched references on save, preventing unintended data loss.
  /// </remarks>
  public LoadState LoadState {   get; set; } = LoadState.Loaded;

  /*============================================================================================================================
  | IS FULLY LOADED?
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Determines whether or not the collection was fully loaded from the persistence store.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     When loading an individual <see cref="Topic"/> or branch from the persistence store, it is possible that topic
  ///     references may not be fully available. In this scenario, updating topic references while e.g. deleting unmatched
  ///     relationships can result in unintended data loss. To account for this, the <see cref="IsFullyLoaded"/> property '
  ///     tracks whether a collection was fully loaded from the persistence store; if it wasn't, the <see cref="
  ///     ITopicRepository"/> should not deleted unmatched topic references.
  ///   </para>
  ///   <para>
  ///     The <see cref="IsFullyLoaded"/> property defaults to <c>true</c>. It should be set to <c>false</c> during the <see
  ///     cref="ITopicRepository.Load(String, Topic?, Boolean, TopicPayload)"/> method if any members of the collection cannot
  ///     be mapped back to a valid <see cref="Topic"/> reference in memory.
  ///   </para>
  /// </remarks>
  public bool IsFullyLoaded {
    get => LoadState is LoadState.Loaded;
    set => LoadState = value? LoadState.Loaded : LoadState.NotLoaded;
  }

  /*============================================================================================================================
  | PROPERTY: DEFERRED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Collects reference targets that were absent from the topic graph during an <see cref="ITopicRepository"/> load, pending
  ///   resolution via lazy-loading.
  /// </summary>
  /// <remarks>
  ///   Written to by the <see cref="ITopicRepository"/> when a reference target cannot be found in the current <see cref=
  ///   "TopicIndex"/>. The <see cref="Repositories.ITopicLoadResolver.EnsureLoaded(Topic, TopicPayload)"/> resolves each entry
  ///   by calling the <see cref="ITopicRepository"/>'s <c>Load()</c> method, assuming the topics haven't since been introduced
  ///   to the topic graph.
  /// </remarks>
  public Collection<DeferredAssociation> Deferred { get; } = new();

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
    for (var i = Deferred.Count - 1; i >= 0; i--) {
      if (Deferred[i].Key == item.Key) {
        Deferred.RemoveAt(i);
        break;
      }
    }

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
    for (var i = Deferred.Count - 1; i >= 0; i--) {
      if (Deferred[i].Key == item.Key) {
        Deferred.RemoveAt(i);
        break;
      }
    }

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