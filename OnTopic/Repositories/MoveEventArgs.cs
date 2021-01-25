/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: MOVE EVENT ARGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The MoveEventArgs object defines an event argument type specific to move events.
  /// </summary>
  /// <remarks>
  ///   Allows tracking of the source and destination topics.
  /// </remarks>
  public class MoveEventArgs : TopicEventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY MOVE EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MoveEventArgs"/> class and sets the <see cref="Topic"/> and
    ///   <see cref="Target"/> properties based on the specified objects.
    /// </summary>
    /// <param name="topic">The topic object associated with the move event.</param>
    /// <param name="target">The parent topic object targeted by the move event.</param>
    /// <requires description="The topic to move must be provided." exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    /// <requires
    ///   description="The target topic under which to move the topic must be provided."
    ///   exception="T:System.ArgumentNullException">
    ///   target is not null
    /// </requires>
    /// <requires description="The topic cannot be its own parent." exception="T:System.ArgumentException">
    ///   topic != target
    /// </requires>
    public MoveEventArgs(Topic topic, Topic target): base(topic) {
      Contract.Requires(topic, "topic");
      Contract.Requires(target, "target");
      Contract.Requires<ArgumentException>(topic != target, "The topic cannot be its own parent.");
      Topic = topic;
      Target = target;
    }

    /*==========================================================================================================================
    | PROPERTY: TARGET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the new parent that the topic will be moved to.
    /// </summary>
    public Topic Target { get; set; }

  } //Class
} //Namespace