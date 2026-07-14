/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
namespace OnTopic.Repositories;

/*==============================================================================================================================
| INTERFACE: TOPIC LAZY LOADABLE
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides the required members for the <see cref="Topic"/> class to fully support <see cref="LazyLoadingTopicRepository"/>,
///   including <c>LoadState</c> tracking and s reference to the <see cref="ITopicLazyLoader"/> that allows it to populate its
///   own deferred payload on demand.
/// </summary>
/// <remarks>
///   <see cref="Topic"/> is the sole implementer of this interface, and does so via explicit interface implementations; callers
///   must cast to <see cref="ITopicLazyLoadable"/> to access these members. This keeps the infrastructure off <see cref="Topic"
///   />'s public surface while still allowing repositories, the mapping layer, tests, and other infrastructure to reach it.
/// </remarks>
public interface ITopicLazyLoadable : ITopicBackingAccessor {

  /*============================================================================================================================
  | METHOD: IS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns <see langword="true"/> if every property flag in <paramref name="payload"/> has already been fetched from the
  ///   underlying persistence store; <see langword="false"/> if any one of them are <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  /// <remarks>
  ///   Reads each collection's <see cref="LoadState"/> directly without touching any autoloading getter, making it safe to use
  ///   in traversal and "gating" logic that should not trigger lazy loading.
  /// </remarks>
  /// <param name="payload">One or more <see cref="TopicPayload"/> flags to test.</param>
  bool IsLoaded(TopicPayload payload) {

    // Children
    if (payload.HasFlag(TopicPayload.Children) && Children.LoadState is not LoadState.Loaded) {
      return false;
    }

    // Extended Attributes
    if (payload.HasFlag(TopicPayload.ExtendedAttributes) && Attributes.LoadState is not LoadState.Loaded) {
      return false;
    }

    // Relationships
    if (payload.HasFlag(TopicPayload.Relationships) && Relationships.LoadState is not LoadState.Loaded) {
      return false;
    }

    // References
    if (payload.HasFlag(TopicPayload.References) && References.LoadState is not LoadState.Loaded) {
      return false;
    }

    // History
    if (payload.HasFlag(TopicPayload.VersionHistory) && VersionHistory.LoadState is not LoadState.Loaded) {
      return false;
    }

    // Unexpected
    return true;

  }

  /*============================================================================================================================
  | METHOD: SET LOAD STATE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Sets the <see cref="LoadState"/> for each boundary flag in <paramref name="payload"/> to <paramref name="state"/>.
  /// </summary>
  /// <remarks>
  ///   Mirrors <see cref="IsLoaded(TopicPayload)"/>: Sets each collection's <see cref="LoadState"/> directly without touching
  ///   any autoloading getter. Callers are responsible for passing only payload whose transition is safe.
  /// </remarks>
  /// <param name="payload">One or more <see cref="TopicPayload"/> flags identifying the payload to update.</param>
  /// <param name="state">The <see cref="LoadState"/> to assign to each matched boundary's collection.</param>
  void SetLoadState(TopicPayload payload, LoadState state) {

    // Children
    if (payload.HasFlag(TopicPayload.Children)) {
      Children.LoadState        = state;
    }

    // Extended Attributes
    if (payload.HasFlag(TopicPayload.ExtendedAttributes)) {
      Attributes.LoadState      = state;
    }

    // History
    if (payload.HasFlag(TopicPayload.VersionHistory)) {
      VersionHistory.LoadState  = state;
    }

  }

  /*============================================================================================================================
  | METHOD: FILTER PAYLOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns <paramref name="payload"/> with any already-<see cref="LoadState.Loaded"/> flags cleared, so callers skip
  ///   redundant round trips.
  /// </summary>
  /// <param name="payload">The requested <see cref="TopicPayload"/> flags to filter.</param>
  TopicPayload FilterPayload(TopicPayload payload) {

    // Strip already-loaded payload
    foreach (var flag in Enum.GetValues<TopicPayload>()) {
      if (flag is TopicPayload.None or TopicPayload.All) {
        continue;
      }
      if (IsLoaded(flag)) {
        payload                 &= ~flag;
      }
    }

    // Return filtered payload
    return payload;

  }

  /*============================================================================================================================
  | METHODS: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Ensures each requested <paramref name="payload"/> flag has been retrieved, while fetching and merging whichever of them
  ///   are not yet <see cref="LoadState.Loaded"/>, and skipping those that already are. Returns immediately if the <see cref=
  ///   "Loader"/> is absent.
  /// </summary>
  /// <remarks>
  ///   Callers such as a mapping or navigation service can await this to prepopulate one or more payloads before accessing
  ///   them, thus avoiding a synchronous block on a sparse topic. The autoloading property getters (e.g., <see cref=
  ///   "Topic.Children"/>) call this synchronously via <c>GetAwaiter().GetResult()</c> as an accepted sync-over-async boundary.
  /// </remarks>
  /// <param name="payload">
  ///   One or more <see cref="TopicPayload"/> flags identifying the payload that should be ensured to be loaded.
  /// </param>
  /// <param name="cancellationToken">An optional token that can be used to cancel the operation.</param>
  Task EnsureLoaded(TopicPayload payload, CancellationToken cancellationToken = default) {

    // Skip if the topic isn't "stamped" with the loader
    if (Loader is not { } loader) {
      return Task.CompletedTask;
    }

    // Filter to payload that are not yet loaded
    payload                     = FilterPayload(payload);
    if (payload is TopicPayload.None) {
      return Task.CompletedTask;
    }

    // Ensure the appropriate payload are loaded
    // (Topic)this is safe since Topic is the sole implementer of ITopicLazyLoadable
    return loader.EnsureLoaded((Topic)this, payload, cancellationToken);

  }

  /*============================================================================================================================
  | PROPERTY: LOADER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a reference to the <see cref="ITopicLazyLoader"/> used to lazy load collections on request. This is stamped
  ///   by <see cref="LazyLoadingTopicRepository.StampLoader"/> with whichever <see cref="ITopicRepository"/> most recently
  ///   loaded or saved this topic.
  /// </summary>
  ITopicLazyLoader? Loader { get; set; }

} //Interface