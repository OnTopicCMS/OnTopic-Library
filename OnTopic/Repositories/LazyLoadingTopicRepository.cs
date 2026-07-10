/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;

namespace OnTopic.Repositories;

/*==============================================================================================================================
| CLASS: LAZY LOADING TOPIC REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides an abstract base class for centralizing infrastructure for implementations of <see cref="ITopicRepository"/> that
///   support lazy-loading, independent of the underlying persistence store.
/// </summary>
/// <remarks>
///   This sits between <see cref="ObservableTopicRepository"/>, which offers only event handling, and the two families of
///   concrete <see cref="ITopicRepository"/> base classes: <see cref="TopicRepository"/>, for implementations that persist
///   directly to a data store, and <see cref="TopicRepositoryDecorator"/>, for implementations that wrap another <see cref=
///   "ITopicRepository"/>. Both need to stamp topics with an <see cref="ITopicLoadResolver"/> and resolve deferred
///   associations, but neither should be coupled to the other's specific concerns (e.g., <see cref="TopicRepository"/>'s
///   sealed <c>Save()</c>, <c>Move()</c>, and <c>Delete()</c> template methods, which <see cref="TopicRepositoryDecorator"/>
///   must remain free to override for delegation).
/// </remarks>
public abstract class LazyLoadingTopicRepository : ObservableTopicRepository {

  /*============================================================================================================================
  | METHOD: STAMP RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Stamps the supplied <paramref name="topic"/> and its entire loaded graph with this repository as the <see cref=
  ///   "ITopicLoadResolver"/>, enabling each topic to populate deferred portions of itself on demand.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Only stamps when the current repository implements <see cref="ITopicLoadResolver"/>. A passthrough decorator that is
  ///     not itself a resolver leaves any existing inner stamp intact, rather than overwriting it.
  ///   </para>
  ///   <para>
  ///     Recursion is gated on <see cref="Topic.IsLoaded(TopicPayload)"/> so that unloaded branches are not forced to load.
  ///     Since <see cref="Topic.Children"/> is an autoloading getter, recursing into it unconditionally would trigger a load
  ///     for every <see cref="LoadState.NotLoaded"/> branch just to stamp it; the gate keeps this confined to what's already
  ///     present.
  ///   </para>
  ///   <para>
  ///     Call this method once on the root of a recently loaded or saved graph; it stamps every present topic in one pass.
  ///   </para>
  /// </remarks>
  /// <param name="topic">The root of the topic graph to stamp.</param>
  protected void StampResolver(Topic? topic) {

    // Skip if the TopicRepository is not an ITopicLoadResolver, or if the topic doesn't exist
    if (this is not ITopicLoadResolver resolver || topic is null) {
      return;
    }

    // Stamp the resolver on the topic
    topic.Resolver              = resolver;

    // If the children aren't yet loaded, don't bother with them yet
    if (!topic.IsLoaded(TopicPayload.Children)) {
      return;
    }

    // Stamp any children (this is recursive, obviously!)
    foreach (var child in topic.Children) {
      StampResolver(child);
    }

  }

} //Class