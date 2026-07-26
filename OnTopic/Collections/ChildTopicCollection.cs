/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: CHILD TOPIC COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a collection of <see cref="Topic"/> objects representing the immediate children of a <see cref="Topic"/>.
/// </summary>
/// <remarks>
///   The <see cref="ChildTopicCollection"/> is intended exclusively for providing access to children via the <see cref=
///   "Topic.Children"/> property. For this reason, the constructor is marked as internal.
/// </remarks>
public class ChildTopicCollection : KeyedTopicCollection {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              Topic                           _parent;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="ChildTopicCollection"/> class.
  /// </summary>
  /// <param name="parent">A reference to the topic that the current child collection is bound to.</param>
  internal ChildTopicCollection(Topic parent) {
    _parent                     = parent;
  }

  /*============================================================================================================================
  | PROPERTY: LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indicates whether the collection has been populated from the underlying <see cref="Repositories.ITopicRepository" />,
  ///   allowing callers to distinguish data that is present and authoritative from data that must still be fetched.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Defaults to <see cref="LoadState.Loaded"/>, reflecting that a newly constructed, in-memory collection has nothing
  ///     deferred. When a topic is loaded shallowly from the persistence store, the repository conditionally sets this to <see
  ///     cref="LoadState.NotLoaded"/> to indicate that the immediate children have not yet been fetched. The persistence store
  ///     may optionally provide an indicator of the count without returning the full data, thus allowing this to be set to <see
  ///     cref="LoadState.Loaded"/> if, in fact, there are no relevant topics.
  ///   </para>
  ///   <para>
  ///     This setter exists for <see cref="Repositories.ITopicRepository"/> implementations populating or converging load state
  ///     during <see cref="ITopicRepository.Load(Int32, Topic?, bool, TopicPayload)"/> or <see cref=
  ///     "ITopicLazyLoader.EnsureLoaded(Topic, TopicPayload, CancellationToken)"/>. Setting <see cref="LoadState.Loaded"/>
  ///     while children remain unfetched masks the deferral from subsequent readers; setting <see cref="LoadState.NotLoaded"/>
  ///     on already-resident children induces a spurious synchronous load on next access.
  ///   </para>
  /// </remarks>
  public LoadState LoadState { get; set; } = LoadState.Loaded;

} //Class