/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | CLASS: TOPIC MULTIMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicMultiMap"/> offers support for a keyed collection where each key is mapped to a collection of <see
  ///   cref="Topic"/> instances, thus supporting a 1:n relationship with zero or more topics, organized by key.
  /// </summary>
  public class TopicMultiMap: KeyedCollection<string, KeyValuesPair<string, TopicCollection>> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instance of a <see cref="TopicMultiMap"/> class.
    /// </summary>
    public TopicMultiMap(): base(StringComparer.OrdinalIgnoreCase) {

    }

    /*==========================================================================================================================
    | METHOD: CONTAINS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the <paramref name="topic"/> exists in a collection with the supplied <paramref name="key"/>.
    /// </summary>
    /// <returns>
    ///   Returns <c>true</c> if the <see cref="Topic"/> exists in the specified collection. Returns <c>false</c> if the
    ///   collection doesn't exist.
    /// </returns>
    public bool Contains(string key, Topic topic) => Contains(key) && this[key].Values.Contains(topic);

    /*==========================================================================================================================
    | METHOD: GET VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    ///   Returns a reference to the underlying <see cref="Collection{Topic}"/> collection.
    /// </remarks>
    /// <param name="key">The key of the collection to be returned.</param>
    public TopicCollection GetValues(string key) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      if (Contains(key)) {
        return this[key].Values;
      }
      return new();
    }

    /// <inheritdoc cref="GetValues(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(GetTopics)} method has been renamed to {nameof(GetValues)}.", true)]
    public TopicCollection GetTopics(string key) => GetValues(key);

    /*==========================================================================================================================
    | METHOD: ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds a <paramref name="topic"/> to a collection with the supplied <paramref name="key"/>. If the collection with
    ///   <paramref name="key"/> doesn't exist, it will be established.
    /// </summary>
    public void Add(string key, Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(key, nameof(key));
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure collection is established
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(key)) {
        Add(new(key, new()));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add topic, if it hasn't already been added
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Contains(key, topic)) {
        this[key].Values.Add(topic);
      }

    }

    /*==========================================================================================================================
    | METHOD: REMOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes a <paramref name="topic"/> from a collection with the supplied <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the collection.</param>
    /// <param name="topic">The <see cref="Topic"/> to be removed.</param>
    /// <returns>
    ///   Returns <c>true</c> if the <see cref="Topic"/> is removed; returns <c>false</c> if either the <paramref name="key"/>
    ///   or the <paramref name="topic"/> cannot be found.
    /// </returns>
    public bool Remove(string key, Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate key
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = GetValues(key);

      if (topics is null || !topics.Contains(topic)) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      topics.Remove(topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove true
      \-----------------------------------------------------------------------------------------------------------------------*/
      return true;

    }

    /*==========================================================================================================================
    | METHOD: CLEAR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Removes all <see cref="Topic"/> objects grouped by a specific <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the collection to be cleared.</param>
    public void Clear(string key) => GetValues(key).Clear();

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the <see cref="TopicMultiMap"/> to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="KeyValuesPair{TKey, TValue}"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    [ExcludeFromCodeCoverage]
    protected override sealed string GetKeyForItem(KeyValuesPair<string, TopicCollection> item) {
      Contract.Requires(item, "The item must be available in order to derive its key.");
      return item.Key;
    }

  } //Class
} //Namespace