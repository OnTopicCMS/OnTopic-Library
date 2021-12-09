/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC MOVE EVENT ARGUMENTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicMoveEventArgs"/> object defines an event arguments relevant to a <see cref="ITopicRepository.Move(
  ///   Topic, Topic, Topic?)"/> operation.
  /// </summary>
  /// <remarks>
  ///   Allows tracking of the source and destination topics.
  /// </remarks>
  public class TopicMoveEventArgs : TopicEventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicMoveEventArgs"/> class and sets the <see cref="TopicEventArgs.Topic"
    ///   /> and <see cref="Target"/> properties based on the specified objects.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object being moved.</param>
    /// <param name="source">The original <see cref="Topic.Parent"/> of the <paramref name="topic"/>.</param>
    /// <param name="target">The new <see cref="Topic.Parent"/> of the <paramref name="topic"/>.</param>
    /// <param name="sibling">The optional <see cref="Topic"/> the <paramref name="topic"/> will be moved after.</param>
    /// <requires description="The topic to move must be provided." exception="T:System.ArgumentNullException">
    ///   <paramref name="topic"/> is not null
    /// </requires>
    /// <requires
    ///   description="The target topic under which to move the topic must be provided."
    ///   exception="T:System.ArgumentNullException">
    ///   <paramref name="target"/> is not null
    /// </requires>
    /// <requires description="The topic cannot be its own parent." exception="T:System.ArgumentException">
    ///   <paramref name="topic"/> != <paramref name="target"/>
    /// </requires>
    public TopicMoveEventArgs(Topic topic, Topic? source, Topic target, Topic? sibling = null): base(topic, true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Vaidate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, "target");
      Contract.Requires<ArgumentException>(topic != target, "The topic cannot be its own parent.");
      Contract.Requires<ArgumentException>(topic != source, "The topic cannot be its own parent.");
      Contract.Requires<ArgumentException>(topic != sibling, "The topic cannot be moved next to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Source                    = source;
      Target                    = target;
      Sibling                   = sibling;

    }

    /*==========================================================================================================================
    | PROPERTY: SOURCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the new parent that the <see cref="TopicEventArgs.Topic"/> is being moved from.
    /// </summary>
    public Topic? Source { get; set; }

    /*==========================================================================================================================
    | PROPERTY: TARGET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the new parent that the <see cref="TopicEventArgs.Topic"/> is being moved to.
    /// </summary>
    public Topic Target { get; set; }

    /*==========================================================================================================================
    | PROPERTY: SIBLING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the sibling that the <see cref="TopicEventArgs.Topic"/> is being moved after, if specified.
    /// </summary>
    public Topic? Sibling { get; set; }

  } //Class
} //Namespace