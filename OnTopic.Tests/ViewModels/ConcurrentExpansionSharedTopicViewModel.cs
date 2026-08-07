/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: CONCURRENT EXPANSION SHARED
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a shared target for two concurrent mapping passes with disjoint associations, exposing a single <see cref=
///   "CollectionType.Any"/> collection (<see cref="Related"/>) populated from two different sources.
/// </summary>
/// <remarks>
///   <para>
///     This is the referenced view model in the concurrency stress test: A single source topic is referenced twice from a
///     parent <see cref="ConcurrentExpansionRootTopicViewModel"/>, once with <see cref="AssociationTypes.Relationships"/> and
///     once with <see cref="AssociationTypes.IncomingRelationships"/>. Both passes populate the same <see cref="Related"/>
///     list: One from the source's outgoing relationships, the other from its incoming relationships, thus exercising the locks
///     on SetCollectionValueAsyn() and PopulateTargetCollectionAsync(). The property is left nullable and settable so the
///     mapper both creates the backing list (SetCollectionValueAsyn()) and adds to it (PopulateTargetCollectionAsync()).
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
[SuppressMessage(
  "Usage",
  "CA2227:Collection properties should be read only",
  Justification = "This view model intentionally exposes a settable, nullable collection property so the TopicMappingService's list-creation runs, which the concurrency test relies on to establish a creation race."
)]
public class ConcurrentExpansionSharedTopicViewModel {

  public string? Key { get; set; }

  [Collection("Related")]
  public Collection<KeyOnlyTopicViewModel>? Related { get; set; }

} //Class