/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0060 // Remove unused parameter

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ ONLY TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a read-only collection of topics.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Obsolete(
    $"The {nameof(ReadOnlyTopicCollection <T>)} has been renamed to {nameof(ReadOnlyKeyedTopicCollection<T>)}",
    true
  )]
  public class ReadOnlyTopicCollection<T> : ReadOnlyCollection<T> where T : Topic {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection{T}"/> based on an existing <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection{T}"/>.</param>
    public ReadOnlyTopicCollection(IList<T> innerCollection) : base(innerCollection) {
      throw new NotImplementedException();
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by key.
    /// </summary>
    public T? GetTopic(string key) => throw new NotImplementedException();

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
    public ReadOnlyTopicCollection<T> FromList(IList<T> innerCollection) => throw new NotImplementedException();

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an <see cref="Topic"/> by key.
    /// </summary>
    /// <param name="key">The topic key.</param>
    public Topic? this[string key] => null;

  } //Class
} //Namespace

#pragma warning restore IDE0060 // Remove unused parameter