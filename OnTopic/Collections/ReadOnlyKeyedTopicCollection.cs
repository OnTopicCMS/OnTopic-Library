/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: READ-ONLY KEYED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class ReadOnlyKeyedTopicCollection : ReadOnlyKeyedTopicCollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyKeyedTopicCollection"/> based on an existing <see cref="KeyedTopicCollection"/>.
    /// </summary>
    /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection"/>.</param>
    public ReadOnlyKeyedTopicCollection(IList<Topic>? innerCollection = null) : base(innerCollection) {
    }

    /*==========================================================================================================================
    | FACTORY METHOD: FROM LIST
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new <see cref="ReadOnlyKeyedTopicCollection"/> based on an existing <see cref="List{Topic}"/>.
    /// </summary>
    /// <remarks>
    ///   The <paramref name="innerCollection"/> will be converted to a <see cref="KeyedTopicCollection{Topic}"/>.
    /// </remarks>
    /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection{Topic}"/>.</param>
    public new static ReadOnlyKeyedTopicCollection FromList(IList<Topic> innerCollection) {
      Contract.Requires(innerCollection, "innerCollection should not be null");
      return new(innerCollection);
    }

  } //Class
} //Namespace