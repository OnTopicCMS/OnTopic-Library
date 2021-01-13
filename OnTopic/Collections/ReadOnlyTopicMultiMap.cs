/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ-ONLY TOPIC MULTIMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ReadOnlyTopicMultiMap"/> provides a read-only façade to a <see cref="TopicMultiMap"/>.
  /// </summary>
  public class ReadOnlyTopicMultiMap: IEnumerable<KeyValuesPair<string, ReadOnlyTopicCollection>> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instance of a <see cref="ReadOnlyTopicMultiMap"/> class with a reference to an underlying <see cref=
    ///   "TopicMultiMap"/> instance.
    /// </summary>
    public ReadOnlyTopicMultiMap(TopicMultiMap source) {
      Contract.Requires(source, nameof(source));
      Source = source;
    }

    /// <summary>
    ///   Constructs a new instance of a <see cref="ReadOnlyTopicMultiMap"/> class.
    /// </summary>
    /// <remarks>
    ///   The <see cref="ReadOnlyTopicMultiMap"/> requires an underlying <see cref="Source"/> <see cref="TopicMultiMap"/> to
    ///   derive values from. It's normally expected that callers will pass that via the public <see cref="
    ///   ReadOnlyTopicMultiMap(TopicMultiMap)"/> constructor. Derived classes, however, cannot pass instance parameters to a
    ///   base class. As such, the protected <see cref="ReadOnlyTopicMultiMap()"/> constructor allows the derived class to
    ///   intialize the <see cref="ReadOnlyTopicMultiMap"/> without a <see cref="Source"/>—but expects that it will immediately
    ///   set one via its constructor.
    /// </remarks>
    protected ReadOnlyTopicMultiMap() {}

    /*==========================================================================================================================
    | PROPERTY: SOURCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to the underlying <see cref="TopicMultiMap"/> from which the <see cref="ReadOnlyTopicMultiMap"/> will
    ///   derive values.
    /// </summary>
    /// <returns>
    ///   The <see cref="Source"/> must be passed in via either the public <see cref="ReadOnlyTopicMultiMap(TopicMultiMap)"/>
    ///   constructor, or must be set manually from the constructor of a derived class when using the protected <see cref="
    ///   ReadOnlyTopicMultiMap()"/> constructor.
    /// </returns>
    [NotNull, DisallowNull]
    protected TopicMultiMap? Source { get; init; }

    /*==========================================================================================================================
    | PROPERTY: KEYS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of keys available for the available collections.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of keys.
    /// </returns>
    public ReadOnlyCollection<string> Keys => new(Source.Select(m => m.Key).ToList());

    /*==========================================================================================================================
    | PROPERTY: COUNT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a count of items in the source collection.
    /// </summary>
    /// <returns>
    ///   The number of collections in the underlying source collection.
    /// </returns>
    public int Count => Source.Count;

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="ReadOnlyCollection{Topic}"/> collection from the source collection based on the <paramref
    ///   name="key"/>.
    /// </summary>
    /// <returns>
    ///   A <see cref="ReadOnlyCollection{Topic}"/> collection.
    /// </returns>
    public ReadOnlyTopicCollection this[string key] => new(Source[key].Values);

    /*==========================================================================================================================
    | METHOD: CONTAINS?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="KeyedCollection{TKey, TItem}.Contains(TKey)" />
    public bool Contains(string key) => Source.Contains(key);

    /// <inheritdoc cref="TopicMultiMap.Contains(String, Topic)" />
    public bool Contains(string key, Topic topic) => Source.Contains(key, topic);

    /*==========================================================================================================================
    | METHOD: GET TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    ///   Returns a reference to the underlying <see cref="Collection{Topic}"/> collection.
    /// </remarks>
    /// <param name="key">The key of the collection to be returned.</param>
    public ReadOnlyTopicCollection GetTopics(string key) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      if (Contains(key)) {
        return new(Source[key].Values);
      }
      return new(new List<Topic>());
    }

    /*==========================================================================================================================
    | METHOD: GET ALL TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of collection key.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection GetAllTopics() =>
      new(Source.SelectMany(list => list.Values).Distinct().ToList());

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of relationship key, filtered by content
    ///   type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection GetAllTopics(string contentType) =>
      new(GetAllTopics().Where(t => t.ContentType == contentType).ToList());

    /*==========================================================================================================================
    | GET ENUMERATOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public IEnumerator<KeyValuesPair<string, ReadOnlyTopicCollection>> GetEnumerator() {
      foreach (var collection in Source) {
        yield return new(collection.Key, new(collection.Values));
      }
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => Source.GetEnumerator();

  } //Class
} //Namespace