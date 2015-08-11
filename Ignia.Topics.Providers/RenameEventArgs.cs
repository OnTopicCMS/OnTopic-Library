/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: RENAME EVENT ARGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The RenameEventArgs object defines an event argument type specific to rename events.
  /// </summary>
  public class RenameEventArgs : EventArgs {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private Topic   _topic = null;

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

    public RenameEventArgs(Topic topic) {
      _topic = topic;
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
    public Topic Topic {
      get {
        return _topic;
      }
      set {
        _topic = value;
      }
    }

  } //Class

} //Namespace
