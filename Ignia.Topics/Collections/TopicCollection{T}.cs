﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed collection of <see cref="Topic"/> instances, or a derived type.
  /// </summary>
  public class TopicCollection<T>: KeyedCollection<string, T>, IEnumerable<T> where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    Topic                       _parent                         = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection{T}"/> class with a parent <see cref="Topic"/>.
    /// </summary>
    /// <param name="parent">A reference to the parent <see cref="Topic"/>.</param>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(Topic parent, IEnumerable<T> topics = null) : base(StringComparer.OrdinalIgnoreCase) {
      _parent = parent;
      if (topics != null) {
        foreach (var topic in topics) {
          Add(topic);
        }
      }
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection{T}"/>; assumes no parent.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(IEnumerable<T> topics = null) : this(null, topics) {
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by key.
    /// </summary>
    public T GetTopic(string key) {
      TopicFactory.ValidateKey(key);
      if (Contains(key)) {
        return this[key];
      }
      return null;
    }

    /*==========================================================================================================================
    | METHOD: AS READ ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a read-only version of this <see cref="TopicCollection{T}"/>.
    /// </summary>
    public ReadOnlyTopicCollection<T> AsReadOnly() {
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<T>>() != null);
      return new ReadOnlyTopicCollection<T>(this);
    }

    /*==========================================================================================================================
    | PROPERTY: SORTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the values collection, sorted by the <see cref="Topic.SortOrder"/> property.
    /// </summary>
    /// <remarks>
    ///   Since Dictionaries do not guarantee sort order, this is necessary for any code that expects to honor the order of
    ///   topics in the database. Simply calling <see cref="Collection{T}.Items"/> may return topics in the correct order, but
    ///   this cannot be assumed.
    /// </remarks>
    public IEnumerable<T> Sorted {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<Topic>>() != null);
        return Items.OrderBy(topic => topic.SortOrder);
      }
    }

    /*==========================================================================================================================
    | METHOD: GET ENUMERATOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Returns an enumerable of the current <see cref="Topic"/> collection, sorted by <see cref="Topic.SortOrder"/>.
    /// </summary>
    /// <remarks>
    ///   By default, the enumerator will return <see cref="Topic"/> instances in the order they are stored internally. For
    ///   small collections loaded directly from the database, the sort order will be correct. If the collection has been
    ///   modified programmatically (e.g., via the editor), however, then the order will be incorrect since there is not a way
    ///   to set the order on a <see cref="KeyedCollection{TKey, TItem}"/>. As such, this overrides the
    ///   <see cref="IEnumerable{T}.GetEnumerator"/> method to ensure the correct sort order is honored.
    /// </remarks>
    public new IEnumerator<T> GetEnumerator() => Sorted.GetEnumerator();

    /*==========================================================================================================================
    | METHOD: CHANGE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the key associated with a topic to maintain referential integrity.
    /// </summary>
    /// <remarks>
    ///   By default, <see cref="KeyedCollection{TKey, TItem}"/> doesn't permit mutable keys; this mitigates that issue by
    ///   allowing the collection's lookup dictionary to be updated whenever the key is updated in the corresponding topic
    ///   object.
    /// </remarks>
    /// <param name="topic">The topic object for which the <see cref="Topic.Key"/> should be changed.</param>
    /// <param name="newKey">The string value for the new key.</param>
    internal void ChangeKey(T topic, string newKey) => base.ChangeItemKey(topic, newKey);

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(T item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

  } //Class

} //Namespace