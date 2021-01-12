/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OnTopic.Collections {

  /*============================================================================================================================
  | CLASS: TOPIC COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Represents a collection of <see cref="Topic"/> objects.
  /// </summary>
  public class TopicCollection : Collection<Topic> {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection"/>.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(IEnumerable<Topic>? topics = null) : base(topics.ToList()) {
    }

    /*==========================================================================================================================
    | METHOD: AS READ ONLY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a read-only version of this <see cref="TopicCollection"/>.
    /// </summary>
    public ReadOnlyTopicCollection AsReadOnly() => new(this);

  } //Class
} //Namespace