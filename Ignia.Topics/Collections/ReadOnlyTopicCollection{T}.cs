/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: READ ONLY TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a read-only keyed collection of topics.
  /// </summary>
  public class ReadOnlyTopicCollection<T> : ReadOnlyCollection<T> where T : Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    TopicCollection<T> _innerCollection;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection{T}"/> based on an existing <see cref="TopicCollection{T}"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection{T}"/>.</param>
    public ReadOnlyTopicCollection(TopicCollection<T> innerCollection) : base(innerCollection) {
      Contract.Requires(innerCollection != null, "innerCollection should not be null");
      _innerCollection = innerCollection;
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <typeparamref name="T"/> by key.
    /// </summary>
    public T GetTopic(string key) {
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
    public static ReadOnlyTopicCollection<T> FromList(List<T> innerCollection) {
      Contract.Requires(innerCollection != null, "innerCollection should not be null");
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<T>>() != null);
      var topicCollection = new TopicCollection<T>(innerCollection);
      return new ReadOnlyTopicCollection<T>(topicCollection);
    }

    /*==========================================================================================================================
    | INDEXER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an <see cref="Topic"/> by key.
    /// </summary>
    /// <param name="key">The topic key.</param>
    public Topic this[string key] {
      get {
        return _innerCollection[key];
      }
    }

  } //Class

} //Namespace
