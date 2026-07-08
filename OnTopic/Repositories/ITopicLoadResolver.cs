/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Repositories;

/*==============================================================================================================================
| INTERFACE: TOPIC LOAD RESOLVER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a narrow seam through which a <see cref="Topic"/> can populate one or more deferred payload on demand, without
///   taking a dependency on the full <see cref="ITopicRepository"/>. Instances are stamped onto topics by the repository as
///   they are loaded or saved; topics created in memory carry no resolver.
/// </summary>
public interface ITopicLoadResolver {

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Ensures each requested <paramref name="payload"/> flag has been retrieved for the supplied <paramref name="topic"/>,
  ///   fetching and merging whichever of them are not yet <see cref="LoadState.Loaded"/> and silently skipping those already
  ///   loaded. Invoked by the autoloading property getters, each with its own flag.
  /// </summary>
  Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default);

} //Interface