/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
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
    public ReadOnlyTopicCollection(IList<Topic>? innerCollection = null) : base(innerCollection?? new List<Topic>()) {
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ReadOnlyKeyedTopicCollection{T}.GetTopic(String)"/>
    [Obsolete(
      "The GetTopic() method is not implemented on ReadOnlyTopicCollection. Use ReadOnlyKeyedTopicCollection instead.",
      true
    )]
    public Topic? GetValue(string key) => throw new NotImplementedException();

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ReadOnlyKeyedTopicCollection{T}"/>
    [Obsolete(
      "Indexing by key is not implemented on ReadOnlyTopicCollection. Use ReadOnlyKeyedTopicCollection instead.",
      true
    )]
    public Topic this[string key] => throw new ArgumentOutOfRangeException(nameof(key));

  } //Class
} //Namespace