/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ ONLY TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class ReadOnlyTopicCollection : ReadOnlyTopicCollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection"/>.</param>
    public ReadOnlyTopicCollection(IList<Topic> innerCollection) : base(innerCollection) {
    }

    /*==========================================================================================================================
    | FACTORY METHOD: FROM LIST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyTopicCollection"/> based on an existing <see cref="List{Topic}"/>.
    /// </summary>
    /// <remarks>
    ///   The <paramref name="innerCollection"/> will be converted to a <see cref="TopicCollection{Topic}"/>.
    /// </remarks>
    /// <param name="innerCollection">The underlying <see cref="TopicCollection{Topic}"/>.</param>
    public new static ReadOnlyTopicCollection FromList(IList<Topic> innerCollection) {
      Contract.Requires(innerCollection, "innerCollection should not be null");
      return new(innerCollection);
    }

  } //Class
} //Namespace