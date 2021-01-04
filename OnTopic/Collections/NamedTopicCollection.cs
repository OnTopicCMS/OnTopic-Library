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
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     bool                            _isDirty;

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
      if (topics is not null) {
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
    public bool IsDirty() => _isDirty;

    /*==========================================================================================================================
    | MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Resets the <see cref="_isDirty"/> status of the <see cref="NamedTopicCollection"/>.
    /// </summary>
    public void MarkClean() => _isDirty = false;

    /*==========================================================================================================================
    | INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When inserting an item, determine if it will change the collection; if it will, mark the collection as <see
    ///   cref="_isDirty"/>.
    /// </summary>
    protected override void InsertItem(int index, Topic item) {
      Contract.Requires(index, nameof(index));
      Contract.Requires(item, nameof(item));
      _isDirty = _isDirty || !Contains(item.Key);
      base.InsertItem(index, item);
    }

    /*==========================================================================================================================
    | SET ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When updating an existing item, determine if it will change the collection; if it will, mark the collection as <see
    ///   cref="_isDirty"/>.
    /// </summary>
    protected override void SetItem(int index, Topic item) {
      Contract.Requires(index, nameof(index));
      Contract.Requires(item, nameof(item));
      _isDirty = _isDirty || !Contains(item.Key);
      base.SetItem(index, item);
    }

    /*==========================================================================================================================
    | REMOVE ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When removing an item from the collection, mark the collection as <see cref="_isDirty"/>.
    /// </summary>
    protected override void RemoveItem(int index) {
      Contract.Requires(index, nameof(index));
      _isDirty = _isDirty || index < Count;
      base.RemoveItem(index);
    }

    /*==========================================================================================================================
    | CLEAR ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When clearing the collection, mark the collection as <see cref="_isDirty"/> if it had items in it.
    /// </summary>
    protected override void ClearItems() {
      _isDirty = _isDirty || Count > 0;
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