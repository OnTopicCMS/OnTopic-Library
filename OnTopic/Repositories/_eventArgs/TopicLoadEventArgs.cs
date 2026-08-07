/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories;

/*==============================================================================================================================
| CLASS: TOPIC LOAD EVENT ARGUMENTS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   The <see cref="TopicLoadEventArgs"/> object defines an event argument type specific to load events.
/// </summary>
public class TopicLoadEventArgs : TopicEventArgs {

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicLoadEventArgs"/> object defines the event arguments relevant to a <see cref="ITopicRepository.
  ///   Load(Int32, Topic?, TopicPayload, Int32)"/> operation and its overloads.
  /// </summary>
  /// <param name="topic">The <see cref="Topic"/> object associated with the rename event.</param>
  /// <param name="depth">The number of tiers of descendants that were also loaded. See <see cref="Depth"/> for details.</param>
  /// <param name="version">If a specific version was loaded, specified that version.</param>
  public TopicLoadEventArgs(Topic topic, int depth, DateTime? version = null): base(topic, depth != 0) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Initialize properties
    \-------------------------------------------------------------------------------------------------------------------------*/
    Depth                       = depth;
    Version                     = version;

  }

  /*============================================================================================================================
  | PROPERTY: DEPTH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets or sets the number of tiers of descendants that were loaded below the <see cref="TopicEventArgs.Topic"/>.
  /// </summary>
  /// <remarks>
  ///   <c>-1</c> indicates the full subtree was loaded; <c>0</c> indicates only the seed <see cref="TopicEventArgs.Topic"/>
  ///   itself was loaded; <c>N</c> indicates <c>N</c> tiers of descendants were loaded.
  /// </remarks>
  public int Depth { get; set; }

  /*============================================================================================================================
  | PROPERTY: VERSION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets or sets the specific version of the <see cref="TopicEventArgs.Topic"/> that has been loaded.
  /// </summary>
  public DateTime? Version { get; set; }

} //Class