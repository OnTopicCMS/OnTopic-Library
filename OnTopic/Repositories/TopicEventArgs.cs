/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC EVENT ARGUMENTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicEventArgs"/> class defines an event argument type shared among <see cref="ITopicRepository"/>
  ///   events. It contains the <see cref="Topic"/> being operated against.
  /// </summary>
  /// <remarks>
  ///   All <see cref="ITopicRepository"/> events share at least one shared element: the <see cref="Topic"/> being operated
  ///   against. Some, such as the <see cref="ITopicRepository.TopicDeleted"/>, <i>only</i> relate to that information. Others,
  ///   such as <see cref="ITopicRepository.TopicMoved"/>, need additional information, and thus offer derived classes, such as
  ///   <see cref="TopicMoveEventArgs"/>, to capture additional information.
  /// </remarks>
  public class TopicEventArgs : EventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicEventArgs"/> class.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> being operated against.</param>
    /// <param name="isRecursive">Whether or not descendants of the <see cref="Topic"/> were also loaded.</param>
    public TopicEventArgs(Topic topic, bool isRecursive = true) : base() {
      Topic                     = topic;
      IsRecursive               = isRecursive;
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Getter that returns the Topic object associated with the event
    /// </summary>
    public Topic Topic { get; set; }

    /*==========================================================================================================================
    | PROPERTY: IS RECURSIVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the descendents of the <see cref="Topic"/> are also impacted by this operation.
    /// </summary>
    public bool IsRecursive { get; set; }

  } //Class
} //Namespace