/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  public abstract class TopicDataProviderBase : ProviderBase {

    public event EventHandler<DeleteEventArgs>        DeleteEvent;
    public event EventHandler<MoveEventArgs>          MoveEvent;
    public event EventHandler<RenameEventArgs>        RenameEvent;

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface methods that loads topics to the specified depth.
    /// </summary>
    /// <remarks>
    ///   Optional overloads allow for a topic to be loaded based on its specified key or ID.
    /// </remarks>
    /// <param name="depth">Integer level to which to also load the topic's children.</param>
    /// <param name="version">DateTime identifier for the version of the topic.</param>
    /// <param name="topicId">Integer identifier for the topic.</param>
    /// <param name="topicKey">String identifier for the topic.</param>
    public Topic Load(int depth, DateTime? version = null) {
      return Load(null, -1, depth, version);
    }

    public Topic Load(int topicId, int depth, DateTime? version = null) {
      return Load(null, topicId, depth, version);
    }

    public Topic Load(string topicKey, int depth, DateTime? version = null) {
      return Load(topicKey, -1, depth, version);
    }

    public abstract Topic Load(string topicKey, int topicId, int depth, DateTime? version = null);

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that saves topic attributes; also used for renaming a topic since name is stored as an attribute.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's children and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual int Save(Topic topic, bool isRecursive, bool isDraft) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey != null && topic.OriginalKey != topic.Key) {
        RenameEventArgs       args    = new RenameEventArgs(topic);
        if (RenameEvent != null) {
          RenameEvent(this, args);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset originaal key
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.OriginalKey = null;
      return -1;

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows for specifying a sibling topic.
    /// </remarks>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    public abstract bool Move(Topic topic, Topic target, Topic sibling);

    public virtual bool Move(Topic topic, Topic target) {
      if (MoveEvent != null) {
        MoveEvent(this, new MoveEventArgs(topic, target));
      }
      return true;
    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's children and delete them as well.
    /// </param>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual void Delete(Topic topic, bool isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      DeleteEventArgs         args    = new DeleteEventArgs(topic);
      if (DeleteEvent != null) {
        DeleteEvent(this, args);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null) {
        topic.Parent.Remove(topic.Key);
      }

    }

  } //Class

} //Namespace
