/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;

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

  } //Class
} //Namespace