/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
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
