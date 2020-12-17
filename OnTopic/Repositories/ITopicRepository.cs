/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | INTERFACE: TOPIC REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines the interface for reading and writing topics from a data storage repository.
  /// </summary>
  public interface ITopicRepository {

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates the <see cref="DeleteEventArgs"/> event handler.
    /// </summary>
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    event EventHandler<DeleteEventArgs> DeleteEvent;

    /// <summary>
    ///   Instantiates the <see cref="MoveEventArgs"/> event handler.
    /// </summary>
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    event EventHandler<MoveEventArgs> MoveEvent;

    /// <summary>
    ///   Instantiates the <see cref="RenameEventArgs"/> event handler.
    /// </summary>
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    event EventHandler<RenameEventArgs> RenameEvent;

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
    /// </summary>
    ContentTypeDescriptorCollection GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    Topic? Load(int topicId, bool isRecursive = true);

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified key name.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    Topic? Load(string? topicKey = null, bool isRecursive = true);

    /// <summary>
    ///   Loads a specific version of a topic based on its version.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    Topic? Load(int topicId, DateTime version);

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Rolls back the current topic to a particular point in its version history by reloading legacy attributes and then
    ///   saving the new version.
    /// </summary>
    /// <param name="topic">The current version of the topic to rollback.</param>
    /// <param name="version">The selected Date/Time for the version to which to roll back.</param>
    /// <requires
    ///   description="The version requested for rollback does not exist in the version history."
    ///   exception="T:System.ArgumentNullException">
    ///   !VersionHistory.Contains(version)
    /// </requires>
    void Rollback(Topic topic, DateTime version);

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
    /// <returns>The integer return value from the execution of the <c>topics_UpdateTopic</c> stored procedure.</returns>
    /// <requires description="The topic to save must be specified." exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    int Save(Topic topic, bool isRecursive = false);

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
    /// <requires
    ///   description="The target under which to move the topic must be provided."
    ///   exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    void Move(Topic topic, Topic target, Topic? sibling = null);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <remarks>
    ///   Prior to OnTopic 4.5.0, the <paramref name="isRecursive"/> defaulted to <c>false</c>. Unfortunately, a bug in the
    ///   implementation of <see cref="TopicRepositoryBase.Delete(Topic, Boolean)"/> resulted in this not being validated, and
    ///   thus it operated <i>as though</i> it were <c>true</c>. This was fixed in OnTopic 4.5.0. As this bug fix potentially
    ///   breaks prior implementations, however, the default for <paramref name="isRecursive"/> was changed to <c>true</c> in
    ///   order to maintain backward compatibility. In OnTopic 5.0.0, this will be changed back to <c>false</c>.
    /// </remarks>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well. If set to false
    ///   and the topic has children, including any nested topics, an exception will be thrown. The default is false.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    void Delete(Topic topic, bool isRecursive = false);

  } //Interface
} //Namespace