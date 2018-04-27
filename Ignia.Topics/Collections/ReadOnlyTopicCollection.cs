/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.Collections {

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
    public ReadOnlyTopicCollection(TopicCollection innerCollection) : base(innerCollection) {
    }

  }
}
