/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Ignia.Topics.Internal.Diagnostics;

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
    readonly                    Topic?                          _parent                         = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection{T}"/> class with a parent <see cref="Topic"/>.
    /// </summary>
    /// <param name="parent">A reference to the parent <see cref="Topic"/>.</param>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(Topic? parent, IEnumerable<T>? topics = null) : base(StringComparer.OrdinalIgnoreCase) {
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
    public TopicCollection(IEnumerable<T>? topics = null) : this(null, topics) {
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by key.
    /// </summary>
    public T? GetTopic(string key) {
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
    public ReadOnlyTopicCollection<T> AsReadOnly() => new ReadOnlyTopicCollection<T>(this);

    /*==========================================================================================================================
    | OVERRIDE: INSERT ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>Fires any time a <typeparamref name="T"/> is added to the collection.</summary>
    /// <remarks>
    ///   Compared to the base implementation, will throw a specific <see cref="ArgumentException"/> error if a duplicate key is
    ///   inserted. This conveniently provides the name of the <typeparamref name="T"/>'s <see cref="Topic.GetUniqueKey"/>, so
    ///   it's clear what key is being duplicated.
    /// </remarks>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The <typeparamref name="T"/> instance to insert.</param>
    /// <exception cref="ArgumentException">
    ///   A {typeof(T).Name} with the Key '{item.Key}' already exists. The UniqueKey of the existing {typeof(T).Name} is
    ///   '{GetUniqueKey()}'; the new item's is '{item.GetUniqueKey()}'.
    /// </exception>
    protected override void InsertItem(int index, T item) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(item, nameof(item));

      /*------------------------------------------------------------------------------------------------------------------------
      | Insert item, if not already present
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(item.Key)) {
        base.InsertItem(index, item);
      }
      else {
        throw new ArgumentException(
          $"A {typeof(T).Name} with the Key '{item.Key}' already exists. The UniqueKey of the existing {typeof(T).Name} is " +
          $"'{this[item.Key].GetUniqueKey()}'; the new item's is '{item.GetUniqueKey()}'."
        );
      }
    }

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
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key;
    }

  } //Class
} //Namespace