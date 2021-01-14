/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects indexed by <see cref="Topic.Id"/>.
  /// </summary>
  public class TopicIndex : Dictionary<int, Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicIndex(IEnumerable<Topic>? topics = null) : base() {
      if (topics is not null) {
        foreach(var topic in topics) {
          Add(topic.Id, topic);
        }
      }
    }

  } //Class
} //Namespace