/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Collections.Specialized;

/*==============================================================================================================================
| CLASS: TOPIC INDEX
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Represents a collection of <see cref="Topic"/> objects indexed by <see cref="Topic.Id"/>.
/// </summary>
public class TopicIndex : Dictionary<int, Topic> {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicCollection"/>.
  /// </summary>
  /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
  /// <remarks>
  ///   Unsaved <see cref="Topic"/> instances (<see cref="Topic.IsNew"/>) are skipped, since their <see cref="Topic.Id"/> is a
  ///   placeholder shared by every other unsaved topic, not a real identity, and so isn't a genuine collision. Any other
  ///   colliding <see cref="Topic.Id"/> reflects corrupt data and continues to throw.
  /// </remarks>
  public TopicIndex(IEnumerable<Topic>? topics = null) {
    if (topics is not null) {
      foreach (var topic in topics) {
        if (topic.IsNew) {
          continue;
        }
        Add(topic.Id, topic);
      }
    }
  }

} //Class