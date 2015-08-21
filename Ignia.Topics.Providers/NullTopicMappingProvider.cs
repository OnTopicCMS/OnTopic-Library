/*==============================================================================================================================
| Author      Jeremy Caney, Ignia LLC
| Client      Ignia, LLC
| Project     Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: NULL TOPIC MAPPING PROVIDER
  >-----------------------------------------------------------------------------------------------------------------------------
  | ### TODO JJC 081115: Are the concrete implementations for the event handlers required? This seems to add unnecessary logic 
  | for scenarios where no mapping provider is called for.
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a topic mapping provider that allows the topics system to work independent of a third-party system.
  /// </summary>
  /// <remarks>
  ///   Originally, the topic library was intended to work in conjunction with third-party CMSs as a means of extending the 
  ///   metadata content types. For instance, it could be used to add tagging data to SharePoint objects. The topic library may
  ///   instead be used as a stand-alone CMS, however, in which case the null topic mapping provider disables all third-party 
  ///   integration.
  /// </remarks>
  public class NullTopicMappingProvider : TopicMappingProviderBase {

    /*==========================================================================================================================
    | METHOD: DELETE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Definition of the DeleteEventHandler override from the base; deletes the event handler.
    /// </summary>
    /// <remarks>
    ///   Contract preconditions are defined in the <see cref="TopicMappingProviderBaseContract"/> contract class.
    /// </remarks>
    /// <param name="sender">The topic object.</param>
    /// <param name="args">The <see cref="DeleteEventArgs"/> instance containing the event data.</param>
    public override void DeleteEventHandler(object sender, DeleteEventArgs args) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(args.Topic != null, "The topic object to be deleted is assumed to be available.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Relay to method
      \-----------------------------------------------------------------------------------------------------------------------*/
      Delete(args.Topic.UniqueKey);

    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Core delete method, which permits deleting based on a string.
    /// </summary>
    /// <param name="originalTag">The string identifier for the topic to be deleted.</param>
    public void Delete(string originalTag) {
      // Do nothing
    }

    /*==========================================================================================================================
    | METHOD: MOVE EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Definition of the RenameEventHandler override from the base; moves the event handler.
    /// </summary>
    /// <remarks>
    ///   Contract preconditions are defined in the <see cref="TopicMappingProviderBaseContract"/> contract class.
    /// </remarks>
    /// <param name="sender">The topic object.</param>
    /// <param name="args">The <see cref="MoveEventArgs"/> instance containing the event data.</param>
    public override void MoveEventHandler(object sender, MoveEventArgs args) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(args.Topic != null, "The topic object to be moved is assumed to be available.");
      Contract.Assume(args.Target != null, "The target topic under which the topic should be moved is assumed to be available.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set arguments
      \-----------------------------------------------------------------------------------------------------------------------*/
      string originalTag = args.Topic.UniqueKey;
      string newTag      = args.Target.UniqueKey + ":" + args.Topic.Key;

      /*------------------------------------------------------------------------------------------------------------------------
      | Relay to method
      \-----------------------------------------------------------------------------------------------------------------------*/
      Move(originalTag, newTag);

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Core move method, which permits moving based on a string
    /// </summary>
    /// <param name="originalTag">The string identifier for the topic to be renamed.</param>
    /// <param name="newTag">The new string identifier for the topic.</param>
    public void Move(string originalTag, string newTag) {
      Rename(originalTag, newTag);
    }

    /*==========================================================================================================================
    | METHOD: RENAME EVENT HANDLER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Definition of the RenameEventHandler override from the base; renames the event handler.
    /// </summary>
    /// <remarks>
    ///   Contract preconditions are defined in the <see cref="TopicMappingProviderBaseContract"/> contract class.
    /// </remarks>
    /// <param name="sender">The topic object.</param>
    /// <param name="args">The <see cref="RenameEventArgs"/> instance containing the event data.</param>
    public override void RenameEventHandler(object sender, RenameEventArgs args) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(args.Topic != null, "The topic object to be renamed is assumed to be available.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set arguments
      \-----------------------------------------------------------------------------------------------------------------------*/
      string tagRoot     = "";

      if (args.Topic.UniqueKey.LastIndexOf(":", StringComparison.Ordinal) >= 0) {
        tagRoot = args.Topic.UniqueKey.Substring(0, args.Topic.UniqueKey.LastIndexOf(":", StringComparison.Ordinal));
      }

      string originalTag = tagRoot + ":" + args.Topic.OriginalKey;
      string newTag      = tagRoot + ":" + args.Topic.Key;

      /*------------------------------------------------------------------------------------------------------------------------
      | Relay to method
      \-----------------------------------------------------------------------------------------------------------------------*/
      Rename(originalTag, newTag);

    }

    /*==========================================================================================================================
    | METHOD: RENAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Core rename method, which permits renaming based on a string
    /// </summary>
    /// <param name="originalTag">The string identifier for the topic to be renamed.</param>
    /// <param name="newTag">The new string identifier for the topic.</param>
    public void Rename(string originalTag, string newTag) {
      // Do nothing
    }

  } // Class

} // Namespace

