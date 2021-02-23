/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC RENAME EVENT ARGUMENTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The RenameEventArgs object defines an event argument type specific to rename events.
  /// </summary>
  public class TopicRenameEventArgs : TopicEventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="TopicRenameEventArgs"/> object defines the event arguments relevant to a <see cref="ITopicRepository.
    ///   Save(Topic, Boolean)"/> operation where the <see cref="Topic.Key"/> has changed.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object associated with the rename event.</param>
    /// <param name="originalKey">The original key of the <see cref="Topic"/> prior to being renamed.</param>
    /// <param name="newKey">The new key of the <see cref="Topic"/> after being renamed.</param>
    public TopicRenameEventArgs(Topic topic, string originalKey, string newKey): base(topic, true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Vaidate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(originalKey, nameof(originalKey));
      Contract.Requires(newKey, nameof(newKey));
      Contract.Requires<ArgumentException>(originalKey != newKey, "The new key cannot be the same as the old key.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      OriginalKey               = originalKey;
      NewKey                    = newKey;

    }

    /*==========================================================================================================================
    | PROPERTY: ORIGINAL KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the original <see cref="Topic.Key"/> of the <see cref="TopicEventArgs.Topic"/> that is being renamed.
    /// </summary>
    public string OriginalKey { get; set; }

    /*==========================================================================================================================
    | PROPERTY: NEW KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the new <see cref="Topic.Key"/> of the <see cref="TopicEventArgs.Topic"/> that is being renamed.
    /// </summary>
    public string NewKey { get; set; }

  } //Class
} //Namespace