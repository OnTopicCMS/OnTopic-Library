/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: KEYED TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class KeyedTopicCollection : KeyedTopicCollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="KeyedTopicCollection{T}"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public KeyedTopicCollection(IEnumerable<Topic>? topics = null) : base(topics) {
    }

  } //Class
} //Namespace