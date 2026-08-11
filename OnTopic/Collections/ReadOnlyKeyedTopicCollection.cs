/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: READ-ONLY KEYED TOPIC COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a read-only collection of <see cref="Topic"/> objects that can be looked up by <see cref="Topic.Key"/>.
/// </summary>
public class ReadOnlyKeyedTopicCollection : ReadOnlyKeyedTopicCollection<Topic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a new <see cref="ReadOnlyKeyedTopicCollection"/> based on an existing <see cref="KeyedTopicCollection"/>.
  /// </summary>
  /// <param name="innerCollection">The underlying <see cref="KeyedTopicCollection"/>.</param>
  public ReadOnlyKeyedTopicCollection(KeyedTopicCollection<Topic>? innerCollection = null) : base(innerCollection) {
  }

} //Class