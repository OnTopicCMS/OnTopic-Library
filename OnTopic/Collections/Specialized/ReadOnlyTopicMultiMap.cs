/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Collections.ObjectModel;

namespace OnTopic.Collections.Specialized;

/*==============================================================================================================================
| CLASS: READ-ONLY TOPIC MULTIMAP
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   The <see cref="ReadOnlyTopicMultiMap"/> provides a read-only façade to a <see cref="TopicMultiMap"/>.
/// </summary>
/// <remarks>
///   <see cref="GetValues(String)"/> and the indexer return live views over the underlying <see cref="TopicMultiMap"/>: Changes
///   to the source are reflected without re-querying. <see cref="GetAllValues()"/>, <see cref="GetAllValues(String)"/>, and
///   <see cref="Keys"/> return point-in-time snapshots instead, since they must flatten, deduplicate, or copy the data, rather
///   than wrapping a single underlying collection.
/// </remarks>
public class ReadOnlyTopicMultiMap: IEnumerable<KeyValuesPair<string, ReadOnlyTopicCollection>> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Constructs a new instance of a <see cref="ReadOnlyTopicMultiMap"/> class with a reference to an underlying <see cref=
  ///   "TopicMultiMap"/> instance.
  /// </summary>
  public ReadOnlyTopicMultiMap(TopicMultiMap source) {
    Contract.Requires(source, nameof(source));
    Source                      = source;
  }

  /*============================================================================================================================
  | PROPERTY: SOURCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to the underlying <see cref="TopicMultiMap"/> from which the <see cref="ReadOnlyTopicMultiMap"/> will
  ///   derive values.
  /// </summary>
  private protected TopicMultiMap Source { get; init; }

  /*============================================================================================================================
  | PROPERTY: KEYS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of keys available for the available collections.
  /// </summary>
  /// <returns>
  ///   Returns an enumerable list of keys, as a point-in-time snapshot; subsequent changes to the underlying <see cref=
  ///   "TopicMultiMap"/> aren't reflected in a previously retrieved <see cref="Keys"/> value.
  /// </returns>
  public ReadOnlyCollection<string> Keys => new([.. Source.Select(m => m.Key)]);

  /*============================================================================================================================
  | PROPERTY: COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a count of items in the source collection.
  /// </summary>
  /// <returns>
  ///   The number of collections in the underlying source collection.
  /// </returns>
  public int Count => Source.Count;

  /*============================================================================================================================
  | INDEXER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <see cref="ReadOnlyCollection{Topic}"/> collection from the source collection based on the <paramref
  ///   name="key"/>.
  /// </summary>
  /// <returns>
  ///   A live <see cref="ReadOnlyCollection{Topic}"/> view over the underlying <see cref="TopicMultiMap"/> collection;
  ///   changes to the source are reflected here.
  /// </returns>
  public ReadOnlyTopicCollection this[string key] => new(Source[key].Values);

  /*============================================================================================================================
  | METHOD: CONTAINS?
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc cref="KeyedCollection{TKey, TItem}.Contains(TKey)" />
  public bool Contains(string key) => Source.Contains(key);

  /// <inheritdoc cref="TopicMultiMap.Contains(String, Topic)" />
  public bool Contains(string key, Topic topic) => Source.Contains(key, topic);

  /*============================================================================================================================
  | METHOD: GET VALUES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific <paramref name="key"/>.
  /// </summary>
  /// <remarks>
  ///   For an existing <paramref name="key"/>, returns a live view over the underlying <see cref="TopicCollection"/>; changes
  ///   to the source are reflected here. For a key that doesn't exist, returns a new, disconnected, empty <see cref=
  ///   "ReadOnlyTopicCollection"/>, which isn't added to the underlying <see cref="TopicMultiMap"/>, and thus not maintained
  ///   should that key subsequently be added.
  /// </remarks>
  /// <param name="key">The key of the collection to be returned.</param>
  public ReadOnlyTopicCollection GetValues(string key) {
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
    if (Contains(key)) {
      return new(Source[key].Values);
    }
    return new([]);
  }

  /// <inheritdoc cref="GetValues(String)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(GetTopics)} method has been renamed to {nameof(GetValues)}.", true)]
  public ReadOnlyTopicCollection GetTopics(string key) => GetValues(key);

  /*============================================================================================================================
  | METHOD: GET ALL VALUES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of collection key.
  /// </summary>
  /// <returns>
  ///   Returns an enumerable list of <see cref="Topic"/> objects, as a point-in-time snapshot; subsequent changes to the
  ///   underlying <see cref="TopicMultiMap"/> aren't reflected in a previously retrieved result.
  /// </returns>
  public ReadOnlyTopicCollection GetAllValues() =>
    new([.. Source.SelectMany(list => list.Values).Distinct()]);

  /// <summary>
  ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of key, filtered by content
  ///   type.
  /// </summary>
  /// <returns>
  ///   Returns an enumerable list of <see cref="Topic"/> objects, as a point-in-time snapshot; subsequent changes to the
  ///   underlying <see cref="TopicMultiMap"/> aren't reflected in a previously retrieved result.
  /// </returns>
  public ReadOnlyTopicCollection GetAllValues(string contentType) =>
    new([.. GetAllValues().Where(t => t.ContentType == contentType)]);

  /// <inheritdoc cref="GetAllValues(String)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(GetAllTopics)} method has been renamed to {nameof(GetAllValues)}.", true)]
  public ReadOnlyTopicCollection GetAllTopics(string key) => GetAllValues(key);

  /// <inheritdoc cref="GetAllValues(String)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(GetAllTopics)} method has been renamed to {nameof(GetAllValues)}.", true)]
  public ReadOnlyTopicCollection GetAllTopics() => GetAllValues();

  /*============================================================================================================================
  | GET ENUMERATOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public IEnumerator<KeyValuesPair<string, ReadOnlyTopicCollection>> GetEnumerator() {
    foreach (var collection in  Source) {
      yield return new(collection.Key, new(collection.Values));
    }
  }

  /// <inheritdoc/>
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

} //Class