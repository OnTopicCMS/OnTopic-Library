﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ-ONLY TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class ReadOnlyTopicCollection : ReadOnlyCollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection"/>.</param>
    [ExcludeFromCodeCoverage]
    public ReadOnlyTopicCollection(IList<Topic>? innerCollection = null) : base(innerCollection?? new List<Topic>()) {
    }

    /*==========================================================================================================================
    | FACTORY METHOD: FROM LIST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="List{Topic}"/>.
    /// </summary>
    /// <remarks>
    ///   The <paramref name="innerCollection"/> will be converted to a <see cref="ReadOnlyTopicCollection"/>.
    /// </remarks>
    /// <param name="innerCollection">The underlying <see cref="List{Topic}"/>.</param>
    [ExcludeFromCodeCoverage]
    [Obsolete("This is effectively satisfied by the related overload, and has been removed.", true)]
    public ReadOnlyTopicCollection FromList(IList<Topic> innerCollection) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ReadOnlyKeyedTopicCollection{T}.GetTopic(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"The {nameof(GetValue)} method is not implemented on {nameof(ReadOnlyTopicCollection)}. Use " +
      $"{nameof(ReadOnlyKeyedTopicCollection)} instead.",
      true
    )]
    public Topic? GetValue(string key) => throw new NotImplementedException();

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ReadOnlyKeyedTopicCollection{T}"/>
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"Indexing by key is not implemented on {nameof(ReadOnlyTopicCollection)}. Use {nameof(ReadOnlyKeyedTopicCollection)} " +
      $"instead.",
      true
    )]
    public Topic this[string key] => throw new ArgumentOutOfRangeException(nameof(key));

  } //Class
} //Namespace