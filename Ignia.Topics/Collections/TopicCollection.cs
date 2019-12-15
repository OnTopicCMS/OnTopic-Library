/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;

namespace Ignia.Topics.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class TopicCollection : TopicCollection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection{T}"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(IEnumerable<Topic>? topics = null) : base(topics) {
    }

  } //Class
} //Namespace