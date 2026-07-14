/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;

namespace OnTopic.Tests.TestDoubles;

/*==============================================================================================================================
| CLASS: TRACKING TOPIC LAZY LOADER
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   A minimal <see cref="ITopicLazyLoader"/> spy that records whether it was invoked, without performing any actual loading.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class TrackingTopicLazyLoader : ITopicLazyLoader {

  /*============================================================================================================================
  | PROPERTY: WAS CALLED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns <see langword="true"/> if <see cref="ITopicLazyLoader.EnsureLoaded"/> was invoked.
  /// </summary>
  public bool                   WasCalled                       { get; private set; }

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  Task ITopicLazyLoader.EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {
    WasCalled                   = true;
    return Task.CompletedTask;
  }

} //Class