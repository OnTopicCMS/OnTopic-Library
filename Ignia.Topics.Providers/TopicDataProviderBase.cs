/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  [ContractClass(typeof(TopicDataProviderBaseContract))]
  public abstract class TopicDataProviderBase : ProviderBase {

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates the <see cref="DeleteEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<DeleteEventArgs>        DeleteEvent;

    /// <summary>
    ///   Instantiates the <see cref="MoveEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<MoveEventArgs>          MoveEvent;

    /// <summary>
    ///   Instantiates the <see cref="RenameEventArgs"/> event handler.
    /// </summary>
    public event EventHandler<RenameEventArgs>        RenameEvent;

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the root topic and, optionally, all of its descendants.
    /// </summary>
    /// <remarks>
    ///   If the deepLoad flag is set to true, all available descendants are also loaded.
    /// </remarks>
    /// <param name="deepLoad">
    ///   Boolean indicator signifying whether to load all of a topic's children along with the topic.
    /// </param>
    /// <returns>A topic object and its descendants, if <c>deepLoad = true</c>.</returns>
    public Topic Load(bool deepLoad) {
      return Load("", deepLoad ? -1 : 0);
    }

    /// <summary>
    ///   Loads a specific topic using its unique <paramref name="topicKey"/> value and, optionally, all of its descendants 
    ///   (see <see cref="Load(bool)"/>).
    /// </summary>
    /// <param name="topicKey">The unique key for the topic.</param>
    /// <param name="deepLoad">
    ///   Boolean indicator signifying whether to load all of a topic's children along with the topic.
    /// </param>
    /// <returns>A topic object and its descendants, if <c>deepLoad = true</c>.</returns>
    public Topic Load(string topicKey = "", bool deepLoad = false) {
      return Load(topicKey, deepLoad ? -1 : 0);
    }

    /// <summary>
    ///   Loads a specific topic using its unique id and, optionally, all of its descendants (see <see cref="Load(bool)"/>).
    /// </summary>
    /// <param name="topicId">The integer identifier for the topic.</param>
    /// <param name="deepLoad">
    ///   Boolean indicator signifying whether to load all of a topic's children along with the topic.
    /// </param>
    /// <returns>A topic object and its descendants, if <c>deepLoad = true</c>.</returns>
    public Topic Load(int topicId, bool deepLoad = false) {
      return Load(topicId, deepLoad ? -1 : 0);
    }

    /// <summary>
    ///   Loads a specific version of a specific topic based on the topic id.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">DateTime identifier for the version of the topic.</param>
    /// <returns>A topic object.</returns>
    public Topic Load(int topicId, DateTime version) {
      return Load(null, topicId, -1, version);
    }

    /// <summary>
    ///   Loads a specific version of a specific topic based on the topic key.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="version">DateTime identifier for the version of the topic.</param>
    /// <returns>A topic object.</returns>
    public Topic Load(string topicKey, DateTime version) {
      return Load(topicKey, -1, -1, version);
    }

    /// <summary>
    ///   Loads topics to the specified depth, based on the root topic.
    /// </summary>
    /// <param name="depth">Integer level to which to also load the topic's children.</param>
    /// <returns>A topic object.</returns>
    public Topic Load(int depth) {
      return Load(null, -1, depth, null);
    }

    /// <summary>
    ///   Loads topics to the specified depth, based on the specified integer identifier for the topic.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="depth">The depth.</param>
    /// <returns>A topic object.</returns>
    public Topic Load(int topicId, int depth) {
      return Load(null, topicId, depth, null);
    }

    /// <summary>
    ///   Loads topics to the specified depth, based on the specified string identifier for the topic.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="depth">The depth.</param>
    /// <returns>A topic object.</returns>
    public Topic Load(string topicKey, int depth) {
      return Load(topicKey, -1, depth, null);
    }

    /// <summary>
    ///   Loads topics to the specified depth, based on the specified string and integer identifiers for the topic, and
    ///   optionally based on its DateTime version.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="depth">The depth.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    internal abstract Topic Load(string topicKey, int topicId, int depth, DateTime? version = null);

    /*==========================================================================================================================
    | ###TODO JJC080314: An overload to Load() should be created to accept an XmlDocument or XmlNode based on the proposed
    | Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: If the topic already exists, return the existing node, by calling its Merge() function. Otherwise,
    | construct a new node using its XmlNode constructor.
    >---------------------------------------------------------------------------------------------------------------------------
      public static Topic Load(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects
      //are already created and available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that saves topic attributes; also used for renaming a topic since name is stored as an attribute.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>The integer return value from the execution of the <c>topics_UpdateTopic</c> stored procedure.</returns>
    /// <requires description="The topic to save must be specified." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual int Save(Topic topic, bool isRecursive, bool isDraft) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey != null && topic.OriginalKey != topic.Key) {
        RenameEventArgs       args    = new RenameEventArgs(topic);
        RenameEvent?.Invoke(this, args);
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
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    public abstract bool Move(Topic topic, Topic target, Topic sibling);

    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    /// <requires description="The topic to move must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <requires description="The target under which to move the topic must be provided." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    public virtual bool Move(Topic topic, Topic target) {
      Contract.Requires<ArgumentNullException>(topic != null, "topic");
      Contract.Requires<ArgumentNullException>(target != null, "target");
      MoveEvent?.Invoke(this, new MoveEventArgs(topic, target));
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
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    public virtual void Delete(Topic topic, bool isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      DeleteEventArgs         args    = new DeleteEventArgs(topic);
      DeleteEvent?.Invoke(this, args);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null) {
        topic.Parent.Remove(topic.Key);
      }

    }

  } // Class

} // Namespace
