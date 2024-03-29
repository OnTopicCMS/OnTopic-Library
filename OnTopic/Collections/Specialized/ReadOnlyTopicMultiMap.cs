/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Collections.ObjectModel;

namespace OnTopic.Collections.Specialized {

  /*============================================================================================================================
  | CLASS: READ-ONLY TOPIC MULTIMAP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ReadOnlyTopicMultiMap"/> provides a read-only fa�ade to a <see cref="TopicMultiMap"/>.
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
    ///   ReadOnlyTopicMultiMap(TopicMultiMap)"/> constructor.
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
    | METHOD: GET VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="Topic"/> objects grouped by a specific <paramref name="key"/>.
    /// </summary>
    /// <remarks>
    ///   Returns a reference to the underlying <see cref="Collection{Topic}"/> collection.
    /// </remarks>
    /// <param name="key">The key of the collection to be returned.</param>
    public ReadOnlyTopicCollection GetValues(string key) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), nameof(key));
      if (Contains(key)) {
        return new(Source[key].Values);
      }
      return new(new List<Topic>());
    }

    /// <inheritdoc cref="GetValues(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(GetTopics)} method has been renamed to {nameof(GetValues)}.", true)]
    public ReadOnlyTopicCollection GetTopics(string key) => GetValues(key);

    /*==========================================================================================================================
    | METHOD: GET ALL VALUES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of collection key.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection GetAllValues() =>
      new(Source.SelectMany(list => list.Values).Distinct().ToList());

    /// <summary>
    ///   Retrieves a list of all related <see cref="Topic"/> objects, independent of key, filtered by content
    ///   type.
    /// </summary>
    /// <returns>
    ///   Returns an enumerable list of <see cref="Topic"/> objects.
    /// </returns>
    public ReadOnlyTopicCollection GetAllValues(string contentType) =>
      new(GetAllValues().Where(t => t.ContentType == contentType).ToList());

    /// <inheritdoc cref="GetAllValues(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(GetAllTopics)} method has been renamed to {nameof(GetAllValues)}.", true)]
    public ReadOnlyTopicCollection GetAllTopics(string key) => GetAllValues(key);

    /// <inheritdoc cref="GetAllValues(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(GetAllTopics)} method has been renamed to {nameof(GetAllValues)}.", true)]
    public ReadOnlyTopicCollection GetAllTopics() => GetAllValues();

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
    [ExcludeFromCodeCoverage]
    IEnumerator IEnumerable.GetEnumerator() => Source.GetEnumerator();

  } //Class
} //Namespace