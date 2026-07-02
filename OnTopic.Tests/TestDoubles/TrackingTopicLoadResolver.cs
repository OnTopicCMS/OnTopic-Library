/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: TRACKING TOPIC LOAD RESOLVER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   A minimal <see cref="ITopicLoadResolver"/> spy that records whether it was invoked, without performing any actual loading.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class TrackingTopicLoadResolver : ITopicLoadResolver {

  /*============================================================================================================================
  | PROPERTY: WAS CALLED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns <see langword="true"/> if either <see cref="ITopicLoadResolver.EnsureLoaded"/> or <see cref="
  ///   ITopicLoadResolver.EnsureLoadedAsync"/> was invoked.
  /// </summary>
  public bool                     WasCalled                       { get; private set; }

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  void ITopicLoadResolver.EnsureLoaded(Topic topic, LoadBoundaries boundaries) => WasCalled = true;

  /*============================================================================================================================
  | METHOD: ENSURE LOADED (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  Task ITopicLoadResolver.EnsureLoadedAsync(
    Topic topic,
    LoadBoundaries boundaries,
    CancellationToken cancellationToken
  ) {
    WasCalled                     = true;
    return Task.CompletedTask;
  }

} //Class
