/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;
using OnTopic.Metadata;

namespace OnTopic.Repositories;

/*==============================================================================================================================
| INTERFACE: TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Defines the interface for reading and writing topics from a data storage repository.
/// </summary>
public interface ITopicRepository {

  /*============================================================================================================================
  | EVENT HANDLERS
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <summary>
  ///   Raised after a <see cref="Topic"/> is loaded from the <see cref="ITopicRepository"/> as part of a <see cref=
  ///   "ITopicRepository.Load(String, Topic?, Boolean, TopicPayload)"/> operation, or one of its overloads.
  /// </summary>
  /// <remarks>
  ///  <para>
  ///     The <see cref="TopicLoaded"/> event should only be raised when a new <see cref="Topic"/> is loaded from the underlying
  ///     persistence store—not, for example, when a value is returned from a cache, or a <c>topicId</c> is merely queried from
  ///     the database.
  ///   </para>
  ///   <para>
  ///     Raising this reliably is more than a courtesy. As a concrete example, the lazy-loading resolver stamping is driven by
  ///     this event, so an <see cref="ITopicRepository"/> that fails to raise it will leave loaded topics unable to lazy load
  ///     their own deferred payload.
  ///   </para>
  /// </remarks>
  event EventHandler<TopicLoadEventArgs> TopicLoaded;

  /// <summary>
  ///   Raised after a <see cref="Topic"/> is saved in the <see cref="ITopicRepository"/> as part of a <see cref=
  ///   "ITopicRepository.Save(Topic, Boolean)"/> operation.
  /// </summary>
  event EventHandler<TopicSaveEventArgs> TopicSaved;

  /// <summary>
  ///   Raised after a <see cref="Topic"/> is deleted from the <see cref="ITopicRepository"/> as part of a <see cref=
  ///   "ITopicRepository.Delete(Topic, Boolean)"/> operation.
  /// </summary>
  event EventHandler<TopicEventArgs> TopicDeleted;

  /// <summary>
  ///   Raised after a <see cref="Topic"/> is moved within the <see cref="ITopicRepository"/> as part of a <see cref=
  ///   "ITopicRepository.Move(Topic, Topic, Topic?)"/> operation.
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

  /*============================================================================================================================
  | GET CONTENT TYPE DESCRIPTORS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
  /// </summary>
  ContentTypeDescriptorCollection GetContentTypeDescriptors();

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <summary>
  ///   Loads the root <see cref="Topic"/>, using the same lazy defaults as <see cref=
  ///   "Load(Int32, Topic?, Boolean, TopicPayload)"/>.
  /// </summary>
  /// <returns>A topic object.</returns>
  public Task<Topic?> Load() => Load(-1);

  /// <summary>
  ///   Loads a <see cref="Topic"/> (and, optionally, all of its descendants) based on the specified <paramref name="topicId"/>.
  /// </summary>
  /// <param name="topicId">The topic identifier.</param>
  /// <param name="referenceTopic">
  ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
  ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
  /// </param>
  /// <param name="isRecursive">
  ///   Whether to load the full descendant subtree rooted at the seed topic. When <see langword="false"/>, only the seed topic
  ///   itself is loaded. Ancestor topics are always loaded when needed to place the seed topic within the graph.
  /// </param>
  /// <param name="payload">Specifies which data to include with each topic.</param>
  /// <returns>A topic object.</returns>
  Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  );

  /// <summary>
  ///   Loads a <see cref="Topic"/> (and, optionally, all of its descendants) based on a specified <paramref name="uniqueKey"/>.
  /// </summary>
  /// <param name="uniqueKey">The fully-qualified unique topic key.</param>
  /// <param name="referenceTopic">
  ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
  ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
  /// </param>
  /// <param name="isRecursive">
  ///   Whether to load the full descendant subtree. See <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/> for details.
  /// </param>
  /// <param name="payload">
  ///   Specifies which data to include with each topic. See <see cref="Load(Int32, Topic?, Boolean, TopicPayload)"/>
  ///   for details.
  /// </param>
  /// <returns>A topic object.</returns>
  Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    bool isRecursive            = false,
    TopicPayload payload        = TopicPayload.None
  );

  /// <inheritdoc cref="Load(Int32, Topic?, Boolean, TopicPayload)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete("This overload has  been removed in preference for Load(string, Topic, Boolean).")]
  Task<Topic?> Load(string? uniqueKey, bool isRecursive);

  /// <summary>
  ///   Loads a specific version of a <see cref="Topic"/> based on its <paramref name="topicId"/> and <paramref name="version"/>
  ///   as a detached topic, disconnected from any <see cref="Topic"/> graph, with no <see cref="Topic.Parent"/>, no <see cref=
  ///   "Topic.Children"/>, and no resolved relationships or references.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     This overload is suitable for previewing a historical version; merging one into a live <see cref="Topic"/> and
  ///     persisting the result is the responsibility of <see cref="Rollback(Topic, DateTime)"/>.
  ///   </para>
  ///   <para>
  ///     This overload does not accept an argument for recursion; it will only load a single instance of a version.
  ///   </para>
  ///   <para>
  ///     This overload also does not accept an argument for a reference topic and, as a result, which enforces the detachment:
  ///     Without that, an implementation has no resident graph to resolve relationships or references against, and so every
  ///     association the returned <see cref="Topic"/> carries is, by construction, left in either <see cref=
  ///     "TopicRelationshipMultiMap.Deferred"/> or <see cref="TopicReferenceCollection.Deferred"/> rather than resolved.
  ///   </para>
  /// </remarks>
  /// <param name="topicId">The topic identifier.</param>
  /// <param name="version">The version.</param>
  /// <returns>A detached topic object.</returns>
  Task<Topic?> Load(int topicId, DateTime version);

  /// <summary>
  ///   A convenience overload of <see cref="Load(Int32, DateTime)"/> for callers that already have a <see cref="Topic"/>
  ///   instance in hand.
  /// </summary>
  /// <remarks>
  ///   Returns the same detached preview graph <see cref="Load(Int32, DateTime)"/> does; it does not merge the result into
  ///   <paramref name="topic"/>, or otherwise mutate it. Callers that need to commit a historical version onto a live <see
  ///   cref="Topic"/> should use <see cref="Rollback(Topic, DateTime)"/> instead.
  /// </remarks>
  /// <param name="topic">The current version of the <see cref="Topic"/> whose history is being requested.</param>
  /// <param name="version">The version.</param>
  /// <returns>A detached topic object.</returns>
  Task<Topic?> Load(Topic topic, DateTime version);

  /*============================================================================================================================
  | METHOD: ROLLBACK
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Rolls back the supplied <paramref name="topic"/> to a particular point in its version history by merging the
  ///   historical version into it, and then saving the result as a new version.
  /// </summary>
  /// <remarks>
  ///   Unlike <see cref="Load(Int32, DateTime)"/> or <see cref="Load(Topic, DateTime)"/>, this mutates <paramref name="topic"/>
  ///   in place, immediately followed by <see cref="ITopicRepository.Save(Topic, Boolean)"/>. It is not appropriate for
  ///   previewing a historical version; use <see cref="Load(Topic, DateTime)"/> for that, as it only load the version, without
  ///   incporating it into any in-memory topic graph or committing the previous version to the persistence store.
  /// </remarks>
  /// <param name="topic">The current version of the <see cref="Topic"/> to rollback.</param>
  /// <param name="version">The selected Date/Time for the version to which to roll back.</param>
  /// <requires
  ///   description="The version requested for rollback does not exist in the version history."
  ///   exception="T:System.ArgumentNullException">
  ///   !VersionHistory.Contains(version)
  /// </requires>
  Task Rollback(Topic topic, DateTime version);

  /*============================================================================================================================
  | METHOD: REFRESH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Updates the topic graph represented by the <paramref name="referenceTopic"/> by loading any changes <paramref name=
  ///   "since"/> the specified <see cref="DateTime"/>.
  /// </summary>
  /// <remarks>
  ///   The <see cref="Refresh(Topic, DateTime)"/> method is intended to provide basic synchronization of core attributes,
  ///   indexed attributes, extended attributes, relationships, and topic references. It is not expected to handle deletes
  ///   or reordering of topics.
  /// </remarks>
  Task Refresh(Topic referenceTopic, DateTime since);

  /*============================================================================================================================
  | METHOD: SAVE
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  Task Save(Topic topic, bool isRecursive = false);

  /// <inheritdoc cref="Save(Topic, Boolean)"/>
  [ExcludeFromCodeCoverage]
  [Obsolete("The 'isDraft' argument of the Save() method has been removed.")]
  int Save(Topic topic, bool isRecursive, bool isDraft);

  /*============================================================================================================================
  | METHOD: MOVE
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  /// <requires
  ///   description="The target under which to move the topic must be provided."
  ///   exception="T:System.ArgumentNullException">
  ///   topic is not null
  /// </requires>
  Task Move(Topic topic, Topic  target, Topic? sibling = null);

  /*============================================================================================================================
  | METHOD: DELETE
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  Task Delete(Topic topic, bool isRecursive = false);

} //Interface