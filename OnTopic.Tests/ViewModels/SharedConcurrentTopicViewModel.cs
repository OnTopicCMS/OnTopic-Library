/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: SHARED CONCURRENT TOPIC
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a strongly typed data transfer object, implemented as a positional <c>record</c>, for testing that two concurrent
///   branches mapping the same topic to the same view model type share a single instance.
/// </summary>
/// <remarks>
///   <para>
///     The <see cref="Related"/> collection is mapped through a constructor parameter, so constructing this model requires the
///     source topic's payload to be loaded. When paired with a repository that suspends inside its lazy load, this lets a test
///     hold one branch mid-construction while a second branch reaches the same still-initializing cache entry, evaluating the
///     <see cref="TopicMappingService"/>'s support of sibling concurrency.
///   </para>
///   <para>
///     The actual sibling references are set up in the accompanying <see cref="ConcurrentReferenceTopicViewModel"/>.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
/// <param name="Key">The key of the mapped topic.</param>
/// <param name="Related">A collection mapped from a constructor parameter, forcing the source payload to be loaded.</param>
public record SharedConcurrentTopicViewModel(
  string Key,
  [Collection("Related")] Collection<KeyOnlyTopicViewModel>? Related
);