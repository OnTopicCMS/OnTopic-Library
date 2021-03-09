/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ-ONLY KEYED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a read-only collection of topics.
  /// </summary>
  public class ReadOnlyKeyedTopicCollection<T> : ReadOnlyCollection<T> where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            KeyedTopicCollection<T>         _innerCollection;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyKeyedTopicCollection{T}"/> based on an existing <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection{T}"/>.</param>
    public ReadOnlyKeyedTopicCollection(IList<T>? innerCollection = null) : base(innerCollection?? new List<T>()) {
      _innerCollection = innerCollection as KeyedTopicCollection<T>?? new(innerCollection);
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
    [ExcludeFromCodeCoverage]
    [Obsolete("This is effectively satisfied by the related overload, and has been removed.", true)]
    public ReadOnlyTopicCollection<T> FromList(IList<T> innerCollection) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by <paramref name="key"/>.
    /// </summary>
    public T? GetValue(string key) {
      TopicFactory.ValidateKey(key);
      if (_innerCollection.Contains(key)) {
        return _innerCollection[key];
      }
      return null;
    }

    /// <inheritdoc cref="GetValue(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The GetTopic() method has been renamed to GetValue().", true)]
    public T? GetTopic(string key) => GetValue(key);

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