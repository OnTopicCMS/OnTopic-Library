/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: KEYED TOPIC COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a mutable collection of <see cref="Topic"/> objects that can be looked up by <see cref="Topic.Key"/>.
/// </summary>
public class KeyedTopicCollection : KeyedTopicCollection<Topic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="KeyedTopicCollection{T}"/>.
  /// </summary>
  /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
  public KeyedTopicCollection(IEnumerable<Topic>? topics = null) : base(topics) {
  }

} //Class