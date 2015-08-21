/*==============================================================================================================================
| Author        Katherine Trunkey, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING PROVIDER BASE CONTRACT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines code contracts for the <c>TopicMappingProviderBase</c> class' abstract methods <c>DeleteEventHandler()</c>,
  ///   <c>MoveEventHandler</c>, and <c>RenameEventHandler()</c>.
  /// </summary>
  [ContractClassFor(typeof(TopicMappingProviderBase))]
  public abstract class TopicMappingProviderBaseContract : TopicMappingProviderBase {

    /*==========================================================================================================================
    | METHOD: DELETE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstract definition of the <see cref="TopicMappingProviderBase.DeleteEventHandler(object, DeleteEventArgs)"/> that
    ///   inheriting objects must implement.
    /// </summary>
    /// <requires description="The Delete event arguments must not be null." exception="T:System.ArgumentNullException">
    ///   args != null
    /// </requires>
    public override void DeleteEventHandler(object sender, DeleteEventArgs args) {
	
    }

    /*==========================================================================================================================
    | METHOD: MOVE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstract definition of the <see cref="TopicMappingProviderBase.MoveEventHandler(object, MoveEventArgs)"/> that
    ///   inheriting objects must implement.
    /// </summary>
    /// <requires description="The Move event arguments must not be null." exception="T:System.ArgumentNullException">
    ///   args != null
    /// </requires>
    public override void MoveEventHandler(object sender, MoveEventArgs args) {
	
    }

    /*==========================================================================================================================
    | METHOD: RENAME EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Abstract definition of the <see cref="TopicMappingProviderBase.RenameEventHandler(object, RenameEventArgs)"/> that
    ///   inheriting objects must implement.
    /// </summary>
    /// <requires description="The Rename event arguments must not be null." exception="T:System.ArgumentNullException">
    ///   args != null
    /// </requires>
    public override void RenameEventHandler(object sender, RenameEventArgs args) {
	
    }

  } // Class

} // Namespace