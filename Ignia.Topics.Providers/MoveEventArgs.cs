/*==============================================================================================================================
| Author        Jeremy Caney, Ignia LLC
| Client        Ignia
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The MoveEventArgs object defines an event argument type specific to move events.
  /// </summary>
  /// <remarks>
  ///   Allows tracking of the source and destination topics.
  /// </remarks>
  public class MoveEventArgs : EventArgs {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private Topic   _topic = null;
    private Topic   _target = null;

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY MOVE EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="MoveEventArgs"/> class.
    /// </summary>
    public MoveEventArgs() { }

    public MoveEventArgs(Topic topic, Topic target) {
      _topic = topic;
      _target = target;
    }

    /*==========================================================================================================================
    | PROPERTY: EVENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Topic object associated with the event.
    /// </summary>
    public Topic Topic {
      get {
        return _topic;
      }
      set {
        _topic = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TARGET
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the new parent that the topic will be moved to.
    /// </summary>
    public Topic Target {
      get {
        return _target;
      }
      set {
        _target = value;
      }
    }

  } //Class

} //Namespace