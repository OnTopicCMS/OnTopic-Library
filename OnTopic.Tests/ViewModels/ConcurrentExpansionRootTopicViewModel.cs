/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: CONCURRENT EXPANSION ROOT
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a parent view model that references the same source topic twice with disjoint associations, forcing two
///   concurrent mapping passes over the shared target.
/// </summary>
/// <remarks>
///   <para>
///     Both <see cref="RelationshipsView"/> and <see cref="IncomingView"/> resolve to the same source topic, but request
///     disjoint associations via <see cref="IncludeAttribute"/>. Because both reference properties are mapped concurrently (via
///     the property-level <c>Task.WhenAll</c>), one pass constructs the shared instance while the other expands it, each
///     populating the target's <see cref="ConcurrentExpansionSharedTopicViewModel.Related"/> list from a different source; this
///     is the scenario that the SetCollectionValueAsync() and PopulateTargetCollectionAsync() locks protect against.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
public class ConcurrentExpansionRootTopicViewModel {

  [Include(AssociationTypes.Relationships)]
  public ConcurrentExpansionSharedTopicViewModel? RelationshipsView { get; set; }

  [Include(AssociationTypes.IncomingRelationships)]
  public ConcurrentExpansionSharedTopicViewModel? IncomingView { get; set; }

} //Class