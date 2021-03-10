/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Collections.Specialized;
using OnTopic.Repositories;

namespace OnTopic.Associations {

  /*============================================================================================================================
  | CLASS: TOPIC REFERENCE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects associated with particular reference keys.
  /// </summary>
  public class TopicReferenceCollection : TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicReferenceCollection"/>.
    /// </summary>
    /// <param name="parentTopic">A reference to the topic that the current collection is bound to.</param>
    public TopicReferenceCollection(Topic parentTopic) : base(parentTopic) { }

    /*==========================================================================================================================
    | PROPERTY: PARENT COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute>? ParentCollection =>
      AssociatedTopic.Parent?.References;

    /*==========================================================================================================================
    | PROPERTY: BASE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override TrackedRecordCollection<TopicReferenceRecord, Topic, ReferenceSetterAttribute>? BaseCollection =>
      AssociatedTopic.BaseTopic?.References;

    /*==========================================================================================================================
    | IS FULLY LOADED?
    \-------------------------------------------------------------------------------------------------------------------------*/
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
    ///     cref="ITopicRepository.Load(String?, Topic?, Boolean)"/> method if any members of the collection cannot be mapped
    ///     back to a valid <see cref="Topic"/> reference in memory.
    ///   </para>
    /// </remarks>
    public bool IsFullyLoaded { get; set; } = true;

    /*==========================================================================================================================
    | INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override void InsertItem(int index, TopicReferenceRecord item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.InsertItem(index, item);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      item?.Value?.IncomingRelationships.SetValue(item.Key, AssociatedTopic, null, true);

    }

    /*==========================================================================================================================
    | OVERRIDE: SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override void SetItem(int index, TopicReferenceRecord item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Get existing reference
      \-----------------------------------------------------------------------------------------------------------------------*/
      var existingItem          = this[index];

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.SetItem(index, item);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      existingItem?.Value?.IncomingRelationships.Remove(existingItem.Key, AssociatedTopic, true);
      item?.Value?.IncomingRelationships.SetValue(item.Key, AssociatedTopic, null, true);

    }

    /*==========================================================================================================================
    | OVERRIDE: REMOVE ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override sealed void RemoveItem(int index) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      var existing              = this[index];

      existing.Value?.IncomingRelationships.Remove(existing.Key, AssociatedTopic, true);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.RemoveItem(index);

    }

    /*==========================================================================================================================
    | OVERRIDE: CLEAR ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override sealed void ClearItems() {


      /*------------------------------------------------------------------------------------------------------------------------
      | Provide base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.ClearItems();

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle recipricol references
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var item in Items) {
        item.Value?.IncomingRelationships.Remove(item.Key, AssociatedTopic, true);
      }

    }

  } //Class
} //Namespace