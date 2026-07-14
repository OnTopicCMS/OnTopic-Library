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
///   "ITopicRepository"/>. Both need to stamp topics with an <see cref="ITopicLazyLoader"/> and resolve deferred
///   associations, but neither should be coupled to the other's specific concerns (e.g., <see cref="TopicRepository"/>'s
///   sealed <c>Save()</c>, <c>Move()</c>, and <c>Delete()</c> template methods, which <see cref="TopicRepositoryDecorator"/>
///   must remain free to override for delegation).
/// </remarks>
public abstract class LazyLoadingTopicRepository : ObservableTopicRepository {

  /*============================================================================================================================
  | METHOD: ON TOPIC LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  /// <remarks>
  ///   <para>
  ///     Stamps the <see cref="TopicEventArgs.Topic"/> from the <paramref name="args"/>, and any descendants attached to it,
  ///     via <see cref="StampLoader(Topic?)"/> and <see cref="StampAscendants(Topic?)"/> before raising the event, so the
  ///     loader gets stamped without the loaders needing to be aware of it.
  ///   </para>
  ///   <para>
  ///     <see cref="ITopicRepository.TopicLoaded"/> fires only for the requested topic, never individually for any descendants
  ///     pulled in alongside it, so this handles both.
  ///   </para>
  ///   <para>
  ///     Stamping ahead of the base call is load-bearing: <see cref="TopicRepositoryDecorator"/> subscribes to its inner <see
  ///     cref="ITopicRepository.TopicLoaded"/> in its constructor, so raising the event here synchronously re-enters this
  ///     method on any outer decorators before this call returns, letting the outer's stamp take precedence over inner ones.
  ///   </para>
  /// </remarks>
  protected override void OnTopicLoaded(TopicLoadEventArgs args) {
    Contract.Requires(args, nameof(args));
    StampLoader(args.Topic);
    StampAscendants(args.Topic.Parent);
    base.OnTopicLoaded(args);
  }

  /*============================================================================================================================
  | METHOD: ON TOPIC SAVED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  /// <remarks>
  ///   Stamps the <see cref="TopicEventArgs.Topic"/> from the <paramref name="args"/> via <see cref="StampLoader(Topic?)"/>
  ///   before raising the event for the same reason and via the same method as <see cref="OnTopicLoaded(TopicLoadEventArgs)"/>.
  /// </remarks>
  protected override void OnTopicSaved(TopicSaveEventArgs args) {
    Contract.Requires(args, nameof(args));
    StampLoader(args.Topic);
    base.OnTopicSaved(args);
  }

  /*============================================================================================================================
  | METHOD: LOAD DEFERRED ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Resolves any relationship and reference targets that were deferred when loading each through this repository's own
  ///   <see cref="ITopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/>.
  /// </summary>
  /// <remarks>
  ///   Targets that cannot be found after this are treated as stale references to deleted topics; this completed by clearing
  ///   the <see cref="DeferredAssociation"/>, resulting in the corresponding association collection to <see cref=
  ///   "LoadState.Loaded"/>.
  /// </remarks>
  /// <param name="topic">The topic whose deferred associations should be resolved.</param>
  /// <param name="payload">
  ///   The payload flags that were requested; only <see cref="TopicPayload.Relationships"/> and <see cref=
  ///   "TopicPayload.References"/> are acted upon.
  /// </param>
  /// <param name="cancellationToken">An optional token that can be used to cancel the operation.</param>
  protected async Task LoadDeferredAssociations(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {

    // Validate input
    Contract.Requires(topic, nameof(topic));

    // Cast topic to safely access backing fields
    var rawTopic                = (ITopicBackingAccessor)topic;

    // Resolve deferred relationship targets; unresolvable targets are treated as stale and discarded
    if (payload.HasFlag(TopicPayload.Relationships)) {
      foreach (var deferred in rawTopic.Relationships.Deferred.ToArray()) {
        var target              = await Load(deferred.TopicId).ConfigureAwait(false);
        // SetValue removes the matching Deferred entry; any left unresolved are cleared below
        if (target is not null) {
          rawTopic.Relationships.SetValue(deferred.Key, target, markDirty: false);
        }
      }
      rawTopic.Relationships.Deferred.Clear();
    }

    // Resolve deferred reference targets; unresolvable targets are treated as stale and discarded
    if (payload.HasFlag(TopicPayload.References)) {
      foreach (var deferred in rawTopic.References.Deferred.ToArray()) {
        var target              = await Load(deferred.TopicId).ConfigureAwait(false);
        // SetValue removes the matching Deferred entry; any left unresolved are cleared below
        if (target is not null) {
          rawTopic.References.SetValue(deferred.Key, target, markDirty: false);
        }
      }
      rawTopic.References.Deferred.Clear();
    }

  }

  /*============================================================================================================================
  | METHOD: STAMP LOADER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Stamps the supplied <paramref name="topic"/> and its entire loaded graph with this repository as the <see cref=
  ///   "ITopicLazyLoader"/>, enabling each topic to populate deferred portions of itself on demand.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Only stamps when the current repository implements <see cref="ITopicLazyLoader"/>. A passthrough decorator that is
  ///     not itself a loader leaves any existing inner stamp intact, rather than overwriting it.
  ///   </para>
  ///   <para>
  ///     Recursion is gated on <see cref="ITopicLazyLoadable.IsLoaded(TopicPayload)"/> so that unloaded branches are not forced
  ///     to load. Since <see cref="Topic.Children"/> is an autoloading getter, recursing into it unconditionally would trigger
  ///     a load for every <see cref="LoadState.NotLoaded"/> branch just to stamp it; the gate keeps this confined to what's
  ///     already present.
  ///   </para>
  ///   <para>
  ///     Call this method once on the root of a recently loaded or saved graph; it stamps every present topic in one pass.
  ///   </para>
  /// </remarks>
  /// <param name="topic">The root of the topic graph to stamp.</param>
  private void StampLoader(Topic? topic) {

    // Skip if the TopicRepository is not an ITopicLazyLoader, or if the topic doesn't exist
    if (this is not ITopicLazyLoader loader || topic is null) {
      return;
    }

    // Stamp the loader on the topic
    ((ITopicLazyLoadable)topic).Loader = loader;

    // If the children aren't yet loaded, don't bother with them yet
    if (!((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Children)) {
      return;
    }

    // Stamp any children (this is recursive, obviously!)
    foreach (var child in topic.Children) {
      StampLoader(child);
    }

  }

  /*============================================================================================================================
  | METHOD: STAMP ASCENDANTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Walks a <paramref name="topic"/>'s <see cref="Topic.Parent"/> chain, stamping each with this repository as the <see
  ///   cref="ITopicLazyLoader"/>, so an ascendant that was never individually loaded can still lazy-load its own deferred
  ///   payload.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Stops as soon as it reaches an ascendant already stamped by this exact loader instance, on the assumption that its
  ///     own ascendants were already walked and stamped at that time. Comparing by instance, rather than merely checking for a
  ///     non-null <see cref="ITopicLazyLoadable.Loader"/>, matters when this method runs as part of a decorated stack: An outer
  ///     decorator's pass must not stop early just because an inner repository already stamped the chain with itself.
  ///   </para>
  ///   <para>
  ///     In practice, this only short-circuits repeat calls against the same, undecorated loader instance (e.g., a bare
  ///     <see cref="TopicRepository"/> loading many topics over its lifetime that share ascendant branches). When wrapped by a
  ///     <see cref="TopicRepositoryDecorator"/>, the inner and outer passes stamp with different instances on every call, so
  ///     neither ever finds a match from the other, and thus each pass walks the full chain to the root every time. That's
  ///     harmless, just not a savings there.
  ///   </para>
  /// </remarks>
  /// <param name="topic">
  ///   The topic at which to start walking (typically a loaded topic's <see cref="Topic.Parent"/>).
  /// </param>
  private void StampAscendants(Topic? topic) {

    // Skip if the current repository is not an ITopicLazyLoader
    if (this is not ITopicLazyLoader loader) {
      return;
    }

    // Walk and stamp each ascendant, stopping once this loader has already stamped one
    for (var ascendant = topic; ascendant is not null && ((ITopicLazyLoadable)ascendant).Loader != loader; ascendant = ascendant.Parent) {
      ((ITopicLazyLoadable)ascendant).Loader = loader;
    }

  }

} //Class