/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a named version of the <see cref="TopicCollection"/>, suitable for use in
  ///   <see cref="RelatedTopicCollection"/>, or other derivatives of <see cref="KeyedCollection{TKey, TItem}"/>.
  /// </summary>
  public class NamedTopicCollection: TopicCollection {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="NamedTopicCollection"/> class.
    /// </summary>
    /// <param name="name">Provides a name for the collection, used to identify different collections.</param>
    /// <param name="topics">Optionally seeds the collection with an optional list of topic references.</param>
    public NamedTopicCollection(string name = "", IEnumerable<Topic>? topics = null) : base() {
      Name = name;
      if (topics != null) {
        CopyTo(topics.ToArray(), 0);
      }
    }

    /*==========================================================================================================================
    | IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the collection has been modified. This value is set to <c>true</c> any time a new item is inserted or
    ///   removed from the collection.
    /// </summary>
    public bool IsDirty { get; set; }

    /*==========================================================================================================================
    | INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When inserting an item, determine if it will change the collection; if it will, mark the collection as <see
    ///   cref="IsDirty"/>.
    /// </summary>
    protected override void InsertItem(int index, Topic topic) {
      Contract.Requires(index, nameof(index));
      Contract.Requires(topic, nameof(topic));
      IsDirty = IsDirty || !Contains(topic.Key);
      base.InsertItem(index, topic);
    }

    /*==========================================================================================================================
    | SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When updating an existing item, determine if it will change the collection; if it will, mark the collection as <see
    ///   cref="IsDirty"/>.
    /// </summary>
    protected override void SetItem(int index, Topic topic) {
      Contract.Requires(index, nameof(index));
      Contract.Requires(topic, nameof(topic));
      IsDirty = IsDirty || !Contains(topic.Key);
      base.SetItem(index, topic);
    }

    /*==========================================================================================================================
    | REMOVE ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When removing an item from the collection, mark the collection as <see cref="IsDirty"/>.
    /// </summary>
    protected override void RemoveItem(int index) {
      Contract.Requires(index, nameof(index));
      IsDirty = IsDirty || index < Count;
      base.RemoveItem(index);
    }

    /*==========================================================================================================================
    | CLEAR ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When clearing the collection, mark the collection as <see cref="IsDirty"/> if it had items in it.
    /// </summary>
    protected override void ClearItems() {
      IsDirty = IsDirty || Count > 0;
      base.ClearItems();
    }

    /*==========================================================================================================================
    | PROPERTY: NAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides an optional name for the collection.
    /// </summary>
    /// <remarks>
    ///   The Name property is optional, and primary intended to differentiate multiple <see cref="TopicCollection{T}"/>
    ///   instances being referenced in a single collection, such as the <see cref="RelatedTopicCollection"/>.
    /// </remarks>
    public string Name { get; }

  } //Class
} //Namespace