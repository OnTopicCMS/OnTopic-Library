/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Configuration.Provider;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  public interface ITopicRepository {

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates the <see cref="DeleteEventArgs"/> event handler.
    /// </summary>
    event EventHandler<DeleteEventArgs> DeleteEvent;

    /// <summary>
    ///   Instantiates the <see cref="MoveEventArgs"/> event handler.
    /// </summary>
    event EventHandler<MoveEventArgs> MoveEvent;

    /// <summary>
    ///   Instantiates the <see cref="RenameEventArgs"/> event handler.
    /// </summary>
    event EventHandler<RenameEventArgs> RenameEvent;

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///   Loads topics to the specified depth, based on the specified string and integer identifiers for the topic, and
    ///   optionally based on its DateTime version.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="depth">The number of levels of topics to load.</param>
    /// <param name="version">The specific version number, in DateTime format.</param>
    /// <returns>A topic object.</returns>
    Topic Load(int topicId, int depth = -1, DateTime? version = null);

    /// <summary>
    ///   Loads topics to the specified depth, based on the specified string and integer identifiers for the topic, and
    ///   optionally based on its DateTime version.
    /// </summary>
    /// <param name="topicKey">The unique topic key .</param>
    /// <param name="depth">The depth.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    Topic Load(string topicKey = null, int depth = -1, DateTime? version = null);

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
    int Save(Topic topic, bool isRecursive = false, bool isDraft = false);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <remarks>
    ///   May optionally specify a sibling. If specified, it is expected that the topic will be placed immediately after the 
    ///   topic. 
    /// </remarks>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    /// <requires description="The target under which to move the topic must be provided." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    void Move(Topic topic, Topic target, Topic sibling = null);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well. If set to false
    ///   (the default) and the topic has children, including any nested topics, an exception will be thrown.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    void Delete(Topic topic, bool isRecursive = false);

  } // Interface

} // Namespace
