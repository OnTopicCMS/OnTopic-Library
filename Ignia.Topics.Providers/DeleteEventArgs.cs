/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: TAXONOMY DELETE EVENT ARGS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The DeleteEventArgs class defines an event argument type specific to deletion events
  /// </summary>
  public class DeleteEventArgs : EventArgs {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     Topic   _topic  = null;

    /*==========================================================================================================================
    | CONSTRUCTOR: TAXONOMY DELETE EVENT ARGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="DeleteEventArgs"/> class.
    /// </summary>
    /// <param name="topic">The topic.</param>
    public DeleteEventArgs(Topic topic) : base() {
      _topic = topic;
    }

    /*==========================================================================================================================
    | PROPERTY: TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Getter that returns the Topic object associated with the event
    /// </summary>
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