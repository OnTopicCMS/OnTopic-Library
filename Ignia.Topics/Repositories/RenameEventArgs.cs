/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;

namespace Ignia.Topics.Repositories {

  /*============================================================================================================================
  | CLASS: RENAME EVENT ARGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The RenameEventArgs object defines an event argument type specific to rename events.
  /// </summary>
  public class RenameEventArgs : EventArgs {

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY RENAME EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="RenameEventArgs"/> class.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows for specifying a topic object.
    /// </remarks>
    public RenameEventArgs() { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="RenameEventArgs"/> class and sets the <see cref="Topic"/> property based
    ///   on the specified object.
    /// </summary>
    /// <param name="topic">The topic object associated with the rename event.</param>
    public RenameEventArgs(Topic topic) {
      Topic = topic;
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    /// Gets or sets the Topic object associated with the event.
    /// </summary>
    /// <value>
    /// The topic.
    /// </value>
    [DisallowNull]
    public Topic Topic { get; }

  } // Class

} // Namespace
