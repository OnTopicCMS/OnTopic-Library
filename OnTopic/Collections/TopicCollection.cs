/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class TopicCollection : Collection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    [ExcludeFromCodeCoverage]
    public TopicCollection(IEnumerable<Topic>? topics = null) : base(topics?.ToList()?? new()) {
    }

    /*==========================================================================================================================
    | METHOD: AS READ ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a read-only version of this <see cref="TopicCollection"/>.
    /// </summary>
    public ReadOnlyTopicCollection AsReadOnly() => new(this);

    /*==========================================================================================================================
    | METHOD: GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="KeyedTopicCollection{T}.GetTopic(String)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"The {nameof(GetValue)} method is not implemented on {nameof(TopicCollection)}. Use {nameof(KeyedTopicCollection)} " +
      $"instead.",
      true
    )]
    public Topic? GetValue(string key) => throw new NotImplementedException();

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="KeyedTopicCollection{T}"/>
    [ExcludeFromCodeCoverage]
    [Obsolete(
      $"Indexing by key is not implemented on the {nameof(TopicCollection)}. Use the {nameof(KeyedTopicCollection)} instead.",
      true
    )]
    public Topic this[string key] => throw new ArgumentOutOfRangeException(nameof(key));

  } //Class
} //Namespace