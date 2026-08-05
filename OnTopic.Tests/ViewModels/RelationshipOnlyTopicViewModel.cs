/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: RELATIONSHIP ONLY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a simple view model with a single, explicitly typed <see cref="CollectionType.Relationship"/> property (<see cref
///   ="Related"/>).
/// </summary>
/// <remarks>
///   <para>
///     Intended as a stand-in for cases where a very simple view model is required for test purposes, without introducing other
///     mapping scenarios that might introduce errors, even though they've not part of the test. Unlike <see cref=
///     "RelationTopicViewModel"/>, whose <see cref="RelationTopicViewModel.Cousins"/> maps <see cref=
///     "AssociationTypes.Children"/>, <see cref="Related"/> is explicitly typed as a relationship, so it exercises only the
///     relationship probe in <c>TopicMappingService.GetSourceCollectionAsync</c>.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
public class RelationshipOnlyTopicViewModel: KeyOnlyTopicViewModel {

  [Collection("Related", Type = CollectionType.Relationship)]
  public Collection<KeyOnlyTopicViewModel> Related { get; } = new();

} //Class