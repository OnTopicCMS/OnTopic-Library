/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC (casey.margell@ignia.com)
| Client:       Ignia
| Project:      Topics Editor
|
| Purpose:      The TopicDataProviderBase object defines a base abstract class for taxonomy data providers
|
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  >=============================================================================================================================
  | Defines a base abstract class for taxonomy data providers
  \---------------------------------------------------------------------------------------------------------------------------*/
  public abstract class TopicDataProviderBase : ProviderBase {

    public event EventHandler<DeleteEventArgs>        DeleteEvent;
    public event EventHandler<MoveEventArgs>          MoveEvent;
    public event EventHandler<RenameEventArgs>        RenameEvent;

  /*--------------------------------------------------------------------------------------------------------------------------
  | METHOD: LOAD
  >---------------------------------------------------------------------------------------------------------------------------
  | Interface methods that loads topics
  \-------------------------------------------------------------------------------------------------------------------------*/
    public Topic Load(int depth, DateTime? version = null) {
      return Load(null, -1, depth, version);
    }

    public Topic Load(int topicId, int depth, DateTime? version = null) {
      return Load(null, topicId, depth, version);
    }

    public Topic Load(string topicKey, int depth, DateTime? version = null) {
      return Load(topicKey, -1, depth, version);
    }

    public virtual Topic Load(string topicKey, int topicId, int depth, DateTime? version = null) {
      return null;
    }

  /*--------------------------------------------------------------------------------------------------------------------------
  | METHOD: SAVE
  >---------------------------------------------------------------------------------------------------------------------------
  | Interface method that saves topic attributes, also used for renaming a topic since name is stored as an attribute
  \-------------------------------------------------------------------------------------------------------------------------*/
    public virtual int Save(Topic topic, bool isRecursive, bool isDraft) {

    /*------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

    /*------------------------------------------------------------------------------------------------------------------------
    | TRIGGER EVENT
    \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey != null && topic.OriginalKey != topic.Key) {
        RenameEventArgs       args    = new RenameEventArgs(topic);
        if (RenameEvent != null) {
          RenameEvent(this, args);
        }
      }

    /*------------------------------------------------------------------------------------------------------------------------
    | RESET ORIGINAL KEY
    \-----------------------------------------------------------------------------------------------------------------------*/
      topic.OriginalKey = null;
      return -1;

    }

  /*--------------------------------------------------------------------------------------------------------------------------
  | METHOD: MOVE
  >---------------------------------------------------------------------------------------------------------------------------
  | Interface method that supports moving a topic from one position to another.
  \-------------------------------------------------------------------------------------------------------------------------*/
    public virtual bool Move(Topic topic, Topic target, Topic sibling) {
      return true;
    }

    public virtual bool Move(Topic topic, Topic target) {
      if (MoveEvent != null) {
        MoveEvent(this, new MoveEventArgs(topic, target));
      }
      return true;
    }

  /*--------------------------------------------------------------------------------------------------------------------------
  | METHOD: DELETE
  >---------------------------------------------------------------------------------------------------------------------------
  | Interface method that deletes the provided topic from the tree
  \-------------------------------------------------------------------------------------------------------------------------*/
    public virtual void Delete(Topic topic, bool isRecursive) {

    /*------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

    /*------------------------------------------------------------------------------------------------------------------------
    | TRIGGER EVENT
    \-----------------------------------------------------------------------------------------------------------------------*/
      DeleteEventArgs         args    = new DeleteEventArgs(topic);
      if (DeleteEvent != null) {
        DeleteEvent(this, args);
      }

    /*------------------------------------------------------------------------------------------------------------------------
    | REMOVE FROM PARENT
    \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null) {
        topic.Parent.Remove(topic.Key);
      }

    }

  } //Class

} //Namespace
