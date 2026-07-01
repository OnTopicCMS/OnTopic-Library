/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic;

/*==============================================================================================================================
| ENUM: LOAD STATE
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Indicates the state to which a topic collection has been populated from the underlying <see cref="ITopicRepository"/>,
///   allowing callers to distinguish data that is present and authoritative from data that must still be loaded.
/// </summary>
public enum LoadState {

  /*----------------------------------------------------------------------------------------------------------------------------
  | NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The collection has not been retrieved from the persistence store. Its current contents, typically empty, are not
  ///   authoritative, and accessing the collection should trigger an on-demand load of its immediate members.
  /// </summary>
  NotLoaded,

  /*----------------------------------------------------------------------------------------------------------------------------
  | LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The collection has been fully retrieved and is authoritative. This is the default for a newly constructed, in-memory
  ///   topic, which has nothing deferred in the persistence store.
  /// </summary>
  Loaded,

} //Enum