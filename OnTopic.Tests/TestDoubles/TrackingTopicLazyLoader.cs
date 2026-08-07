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
/// <param name="markLoaded">
///   By default, this doesn't mutate <see cref="LoadState"/>, so a stamped topic remains <see cref="LoadState.NotLoaded"/>
///   even after a call, letting tests assert that a specific code path either suppresses or triggers autoloading. Pass
///   <see langword="true"/> to instead simulate a real loader's fill, marking the requested payload <see
///   cref="LoadState.Loaded"/> on each call, when a test needs to confirm that a caller warms a payload <em>exactly once</em>
///   rather than relying on this spy's inertness to inflate the count.
/// </param>
[ExcludeFromCodeCoverage]
internal sealed class TrackingTopicLazyLoader(bool markLoaded = false) : ITopicLazyLoader {

  /*============================================================================================================================
  | PRIVATE FIELDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              List<TopicPayload>              _payloads                       = [];

  /*============================================================================================================================
  | PROPERTY: WAS CALLED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns <see langword="true"/> if <see cref="ITopicLazyLoader.EnsureLoaded"/> was invoked.
  /// </summary>
  public bool                   WasCalled                       => _payloads.Count > 0;

  /*============================================================================================================================
  | PROPERTY: CALL COUNT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the number of times <see cref="ITopicLazyLoader.EnsureLoaded"/> was invoked.
  /// </summary>
  public int                    CallCount                       => _payloads.Count;

  /*============================================================================================================================
  | PROPERTY: PAYLOADS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Returns the <see cref="TopicPayload"/> passed to each invocation of <see cref="ITopicLazyLoader.EnsureLoaded"/>, in
  ///   call order.
  /// </summary>
  public IReadOnlyList<TopicPayload> Payloads                   => _payloads;

  /*============================================================================================================================
  | METHOD: ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  Task ITopicLazyLoader.EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken) {
    _payloads.Add(payload);
    if (markLoaded) {
      ((ITopicLazyLoadable)topic).SetLoadState(payload, LoadState.Loaded);
    }
    return Task.CompletedTask;
  }

} //Class