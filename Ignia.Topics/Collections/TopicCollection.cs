/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    ///   Initializes a new instance of the <see cref="TopicCollection"/> class with a parent <see cref="Topic"/>.
    /// </summary>
    /// <param name="parent">A reference to the parent <see cref="Topic"/>.</param>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(Topic parent, IEnumerable<Topic> topics = null) : base(parent, topics) {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicCollection{T}"/>; assumes no parent.
    /// </summary>
    /// <param name="topics">Seeds the collection with an optional list of topic references.</param>
    public TopicCollection(IEnumerable<Topic> topics = null) : this(new Topic(), topics) {
    }

  }
}
