/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC SAVE EVENT ARGUMENTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicSaveEventArgs"/> object defines an event argument type specific to save events.
  /// </summary>
  public class TopicSaveEventArgs : TopicEventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="TopicSaveEventArgs"/> object defines the event arguments relevant to a <see cref="ITopicRepository.
    ///   Save(Topic, Boolean)"/> operation.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object associated with the rename event.</param>
    /// <param name="isRecursive">Whether or not descendants of the <see cref="Topic"/> were also saved.</param>
    /// <param name="isNew">Whether or not this was a newly created <see cref="Topic"/>.</param>
    public TopicSaveEventArgs(Topic topic, bool isRecursive, bool isNew): base(topic, isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      IsNew                     = isNew;

    }

    /*==========================================================================================================================
    | PROPERTY: IS NEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the <see cref="TopicEventArgs.Topic"/> was newly created, or if it was an existing <see cref="
    ///   Topic"/> that has been updated.
    /// </summary>
    public bool IsNew { get; set; }

  } //Class
} //Namespace