/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: STAGGERED TOPIC LAZY LOADER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   An <see cref="ITopicLazyLoader"/> that suspends <see cref="ITopicLazyLoader.EnsureLoaded"/> for a fixed <see cref=
///   "TimeSpan"/> before completing, so a test can attach different instances to sibling topics and force their loads to
///   genuinely complete out of source order.
/// </summary>
/// <remarks>
///   <para>
///     This is similar to <see cref="StaggeredStubTopicRepository"/>, except that it staggers calls to <see cref=
///     "ITopicLazyLoader.EnsureLoaded"/>, not <see cref="ITopicRepository.Load()"/>.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
[ExcludeFromCodeCoverage]
internal sealed class StaggeredTopicLazyLoader(TimeSpan delay): ITopicLazyLoader {

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  async Task ITopicLazyLoader.EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {
    if (delay > TimeSpan.Zero) {
      await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
    }
    ((ITopicBackingAccessor)topic).Children.LoadState = LoadState.Loaded;
  }

} //Class