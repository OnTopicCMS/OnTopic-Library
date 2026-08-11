/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: READ-ONLY KEYED TOPIC COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a read-only collection of topics.
/// </summary>
public class ReadOnlyKeyedTopicCollection<T> : ReadOnlyCollection<T> where T : Topic {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              KeyedTopicCollection<T>         _innerCollection;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a new <see cref="ReadOnlyKeyedTopicCollection{T}"/> based on an existing <see cref=
  ///   "KeyedTopicCollection{T}"/>.
  /// </summary>
  /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection{T}"/>.</param>
  public ReadOnlyKeyedTopicCollection(KeyedTopicCollection<T>? innerCollection = null) : base(innerCollection ?? new()) {
    _innerCollection            = innerCollection ?? new();
  }

  /*============================================================================================================================
  | FACTORY METHOD: FROM LIST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a new <see cref="ReadOnlyTopicCollection{T}"/> based on an existing <see cref="List{T}"/>.
  /// </summary>
  /// <remarks>
  ///   The <paramref name="innerCollection"/> will be converted to a <see cref="TopicCollection{T}"/>.
  /// </remarks>
  /// <param name="innerCollection">The underlying <see cref="TopicCollection{T}"/>.</param>
  [ExcludeFromCodeCoverage]
  [Obsolete("This is effectively satisfied by the related overload, and has been removed.", true)]
  public ReadOnlyTopicCollection<T> FromList(IList<T> innerCollection) => throw new NotImplementedException();

  /*============================================================================================================================
  | METHOD: GET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <typeparamref name="T"/> by <paramref name="key"/>.
  /// </summary>
  public T? GetValue(string key) {
    TopicFactory.ValidateKey(key);
    return TryGetValue(key, out var value)? value : null;
  }

  /// <inheritdoc cref="GetValue(String)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(GetTopic)} method has been renamed to {nameof(GetValue)}.", true)]
  public T? GetTopic(string key) => GetValue(key);

  /*============================================================================================================================
  | METHOD: TRY GET VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve a <typeparamref name="T"/> by <paramref name="key"/>, returning whether it was found.
  /// </summary>
  /// <param name="key">The topic key.</param>
  /// <param name="value">The <typeparamref name="T"/> associated with the <paramref name="key"/>, if found.</param>
  public bool TryGetValue(string key, [NotNullWhen(true)] out T? value) => _innerCollection.TryGetValue(key, out value);

  /*============================================================================================================================
  | INDEXER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <typeparamref name="T"/> by key.
  /// </summary>
  /// <param name="key">The topic key.</param>
  public T this[string key] => _innerCollection[key];

} //Class