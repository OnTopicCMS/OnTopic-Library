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
  public class ReadOnlyTopicCollection : ReadOnlyCollection<Topic> {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    TopicCollection _innerCollection;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection"/>.</param>
    public ReadOnlyTopicCollection(TopicCollection innerCollection) : base(innerCollection) {
      Contract.Requires(innerCollection != null, "innerCollection should not be null");
      _innerCollection = innerCollection;
    }

    /*==========================================================================================================================
    | FACTORY METHOD: FROM LIST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="List{Topic}"/>.
    /// </summary>
    /// <remarks>
    ///   The <paramref name="innerCollection"/> will be converted to a <see cref="TopicCollection"/>.
    /// </remarks>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection"/>.</param>
    public static ReadOnlyTopicCollection FromList(List<Topic> innerCollection) {
      Contract.Requires(innerCollection != null, "innerCollection should not be null");
      var topicCollection = new TopicCollection(innerCollection);
      return new ReadOnlyTopicCollection(topicCollection);
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
