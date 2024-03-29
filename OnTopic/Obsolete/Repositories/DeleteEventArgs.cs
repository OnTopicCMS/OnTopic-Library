﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: DELETE EVENT ARGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The DeleteEventArgs class defines an event argument type specific to deletion events
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Obsolete($"The {nameof(DeleteEventArgs)} has been renamed to {nameof(TopicEventArgs)}", true)]
  public class DeleteEventArgs : EventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY DELETE EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="DeleteEventArgs"/> class.
    /// </summary>
    /// <param name="topic">The topic.</param>
    public DeleteEventArgs(Topic topic) : base() {
      Topic = topic;
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Getter that returns the Topic object associated with the event
    /// </summary>
    public Topic Topic { get; set; }

  } //Class
} //Namespace