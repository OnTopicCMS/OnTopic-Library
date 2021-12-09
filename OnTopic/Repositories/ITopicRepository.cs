/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
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
    ///   Raised after a <see cref="Topic"/> is loaded from the <see cref="ITopicRepository"/> as part of a <see cref="
    ///   ITopicRepository.Load(String?, Topic?, Boolean)"/> operation, or one of its overloads.
    /// </summary>
    /// <remarks>
    ///   The <see cref="TopicLoaded"/> event should only be raised when a new <see cref="Topic"/> is loaded from the underlying
    ///   persistence store. It should not be loaded, for example, if a value is loaded from the cache, or a <c>topicId</c> is
    ///   queried from the database. Given this, this event will need to be raised in actual implementations, since it is
    ///   specific to the business logic of each <see cref="ITopicRepository"/>.
    /// </remarks>
    event EventHandler<TopicLoadEventArgs> TopicLoaded;

    /// <summary>
    ///   Raised after a <see cref="Topic"/> is saved in the <see cref="ITopicRepository"/> as part of a <see cref="
    ///   ITopicRepository.Save(Topic, Boolean)"/> operation.
    /// </summary>
    event EventHandler<TopicSaveEventArgs> TopicSaved;

    /// <summary>
    ///   Raised after a <see cref="Topic"/> is deleted from the <see cref="ITopicRepository"/> as part of a <see cref="
    ///   ITopicRepository.Delete(Topic, Boolean)"/> operation.
    /// </summary>
    event EventHandler<TopicEventArgs> TopicDeleted;

    /// <summary>
    ///   Raised after a <see cref="Topic"/> is moved within the <see cref="ITopicRepository"/> as part of a <see cref="
    ///   ITopicRepository.Move(Topic, Topic, Topic?)"/> operation.
    /// </summary>
    event EventHandler<TopicMoveEventArgs> TopicMoved;

    /// <summary>
    ///   Raised after a <see cref="Topic"/> is renamed as ppart of a <see cref="ITopicRepository.Save(Topic, Boolean)"/>
    ///   operation.
    /// </summary>
    event EventHandler<TopicRenameEventArgs> TopicRenamed;

    /// <inheritdoc cref="TopicDeleted"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(DeleteEvent)} has been renamed to {nameof(TopicDeleted)}")]
    event EventHandler<DeleteEventArgs> DeleteEvent;

    /// <inheritdoc cref="TopicMoved"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(MoveEvent)} has been renamed to {nameof(TopicMoved)}")]
    event EventHandler<DeleteEventArgs> MoveEvent;

    /// <inheritdoc cref="TopicRenamed"/>
    [ExcludeFromCodeCoverage]
    [Obsolete($"The {nameof(RenameEvent)} has been renamed to {nameof(TopicRenamed)}")]
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
    ///   Loads the entire root topic graph, including all descendants.
    /// </summary>
    /// <returns>A topic object.</returns>
    public Topic? Load() => Load(-1);

    /// <summary>
    ///   Loads a <see cref="Topic"/> (and, optionally, all of its descendants) based on the specified <paramref name="topicId"
    ///   />.
    /// </summary>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="referenceTopic">
    ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
    ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
    /// </param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true);

    /// <summary>
    ///   Loads a <see cref="Topic"/> (and, optionally, all of its descendants) based on the specified <paramref name="uniqueKey
    ///   "/>.
    /// </summary>
    /// <param name="uniqueKey">The fully-qualified unique topic key.</param>
    /// <param name="referenceTopic">
    ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
    ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
    /// </param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    Topic? Load(string uniqueKey, Topic? referenceTopic = null, bool isRecursive = true);

    /// <inheritdoc cref="Load(Int32, Topic?, Boolean)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("This overload has been removed in preference for Load(string, Topic, Boolean).")]
    Topic? Load(string? uniqueKey, bool isRecursive);

    /// <summary>
    ///   Loads a specific version of a <see cref="Topic"/> based on its <paramref name="topicId"/> and <paramref name="version
    ///   "/>.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <param name="referenceTopic">
    ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
    ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
    /// </param>
    /// <returns>A topic object.</returns>
    Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null);

    /// <inheritdoc cref="Load(Int32, DateTime, Topic)"/>
    Topic? Load(Topic topic, DateTime version);

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Rolls back the supplied <paramref name="topic"/> to a particular point in its version history by reloading legacy
    ///   attributes and then saving the new version.
    /// </summary>
    /// <param name="topic">The current version of the <see cref="Topic"/> to rollback.</param>
    /// <param name="version">The selected Date/Time for the version to which to roll back.</param>
    /// <requires
    ///   description="The version requested for rollback does not exist in the version history."
    ///   exception="T:System.ArgumentNullException">
    ///   !VersionHistory.Contains(version)
    /// </requires>
    void Rollback(Topic topic, DateTime version);

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Updates the topic graph represented by the <paramref name="referenceTopic"/> by loading any changes <paramref name="
    ///   since"/> the specified <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="Refresh(Topic, DateTime)"/> method is intended to provide basic synchronization of core attributes,
    ///   indexed attributes, extended attributes, relationships, and topic references. It is not expected to handle deletes
    ///   or reordering of topics.
    /// </remarks>
    void Refresh(Topic referenceTopic, DateTime since);

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves the <paramref name="topic"/> (and, optionally, its descendants) to the persistence store.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <requires description="The topic to save must be specified." exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    void Save(Topic topic, bool isRecursive = false);

    /// <inheritdoc cref="Save(Topic, Boolean)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The 'isDraft' argument of the Save() method has been removed.")]
    int Save(Topic topic, bool isRecursive, bool isDraft);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves the <paramref name="topic"/> from its existing parent to a <paramref name="target"/> parent, optionally placing
    ///   it after a supplied <paramref name="sibling"/> topic.
    /// </summary>
    /// <remarks>
    ///   May optionally specify a <paramref name="sibling"/> topic. If specified, it is expected that the <see cref="Topic"/>
    ///   will be placed immediately after the <paramref name="sibling"/>.
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> object to be moved.</param>
    /// <param name="target">The target <see cref="Topic"/> object under which to move the source <see cref="Topic"/>.</param>
    /// <param name="sibling">
    ///   An optional <see cref="Topic"/> object representing a sibling adjacent to which the source <see cref="Topic"/> should
    ///   be moved.
    /// </param>
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
    ///   Deletes the provided <paramref name="topic"/> from the topic tree.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the <see cref="Topic"/>'s descendants and delete them as well. If set to false
    ///   and the topic has children, including any nested topics, an exception will be thrown. The default is false.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">
    ///   topic is not null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    void Delete(Topic topic, bool isRecursive = false);

  } //Interface
} //Namespace