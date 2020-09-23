/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ ONLY TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a read-only collection of topics.
  /// </summary>
  public class ReadOnlyTopicCollection<T> : ReadOnlyCollection<T> where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            TopicCollection<T>              _innerCollection;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection{T}"/> based on an existing <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection{T}"/>.</param>
    public ReadOnlyTopicCollection(IList<T> innerCollection) : base(innerCollection) {
      Contract.Requires(innerCollection, "innerCollection should not be null");
      _innerCollection = innerCollection as TopicCollection<T>?? new TopicCollection<T>(innerCollection);
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by key.
    /// </summary>
    public T? GetTopic(string key) {
      TopicFactory.ValidateKey(key);
      if (_innerCollection.Contains(key)) {
        return _innerCollection[key];
      }
      return null;
    }

    /*==========================================================================================================================
    | FACTORY METHOD: FROM LIST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection{T}"/> based on an existing <see cref="List{T}"/>.
    /// </summary>
    /// <remarks>
    ///   The <paramref name="innerCollection"/> will be converted to a <see cref="TopicCollection{T}"/>.
    /// </remarks>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection{T}"/>.</param>
    [Obsolete("This is effectively satisfied by the related overload, and will be removed in OnTopic 5.0.0.", false)]
    public ReadOnlyTopicCollection<T> FromList(IList<T> innerCollection) {
      Contract.Requires(innerCollection, "innerCollection should not be null");
      return new ReadOnlyTopicCollection<T>(innerCollection);
    }

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an <see cref="Topic"/> by key.
    /// </summary>
    /// <param name="key">The topic key.</param>
    public Topic this[string key] => _innerCollection[key];

  } //Class
} //Namespace