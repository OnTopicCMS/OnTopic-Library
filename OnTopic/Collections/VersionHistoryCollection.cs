/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace OnTopic.Collections;

/*==============================================================================================================================
| CLASS: VERSION HISTORY COLLECTION
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a collection of <see cref="DateTime"/> values representing past versions of a <see cref="Topic"/>.
/// </summary>
public class VersionHistoryCollection: Collection<DateTime> {

  /*============================================================================================================================
  | PROPERTY: LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Indicates whether the collection has been populated from the underlying <see cref="Repositories.ITopicRepository" />,
  ///   allowing callers to distinguish data that is present and authoritative from data that must still be loaded.
  /// </summary>
  /// <remarks>
  ///   Defaults to <see cref="LoadState.Loaded"/>, reflecting that a newly constructed, in-memory topic has nothing deferred.
  ///   When a topic is loaded from the persistence store without its version history, the repository sets this to <see
  ///   cref="LoadState.NotLoaded"/> to indicate that it has not yet been loaded.
  /// </remarks>
  public LoadState LoadState { get; set; } = LoadState.Loaded;

} //Class