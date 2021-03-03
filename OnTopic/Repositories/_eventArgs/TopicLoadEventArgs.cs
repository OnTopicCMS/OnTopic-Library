/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC LOAD EVENT ARGUMENTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="TopicLoadEventArgs"/> object defines an event argument type specific to load events.
  /// </summary>
  public class TopicLoadEventArgs : TopicEventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The <see cref="TopicLoadEventArgs"/> object defines the event arguments relevant to a <see cref="ITopicRepository.
    ///   Load(Int32, Topic?, Boolean)"/> operation and its overloads.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object associated with the rename event.</param>
    /// <param name="isRecursive">Whether or not descendants of the <see cref="Topic"/> were also loaded.</param>
    /// <param name="version">If a specific version was loaded, specified that version.</param>
    public TopicLoadEventArgs(Topic topic, bool isRecursive, DateTime? version = null): base(topic, isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Version                   = version;

    }

    /*==========================================================================================================================
    | PROPERTY: VERSION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the specific version of the <see cref="TopicEventArgs.Topic"/> that has been loaded.
    /// </summary>
    public DateTime? Version { get; set; }

  } //Class
} //Namespace