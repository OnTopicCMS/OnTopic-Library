/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels;

/*==============================================================================================================================
| VIEW MODEL: EXPANSION PARENT
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a view model whose two collections both map the same source topic to <see cref="ExpansionSharedTopicViewModel"/>,
///   but request disjoint associations so that mapping it can exercise an association expansion pass.
/// </summary>
/// <remarks>
///   <para>
///     The <see cref="Related"/> relationship and the <see cref="Children"/> collection are populated from the same source
///     topic and mapped to the same <see cref="ExpansionSharedTopicViewModel"/> instance, but request disjoint associations
///     (<see cref="AssociationTypes.Children"/> and <see cref="AssociationTypes.Relationships"/>). Neither association maps
///     anything on <see cref="ExpansionSharedTopicViewModel"/>, which has no association-typed members: The disjoint requests
///     exist only so the cache sees the second encounter as missing an association and runs an expansion pass, rather than
///     returning the cached instance unchanged. What that expansion pass must not do is redo the target's non-association work.
///   </para>
///   <para>
///     The disjointness is all this view model contributes, and only conditionally: If one encounter finds the instance the
///     other already cached, that encounter has a missing association. Whether the encounters actually resolve that way, as an
///     ordered initial pass followed by a cache-hit expansion pass rather than two concurrent initial passes, is a property
///     of the mapping runtime, not of this view model. The tests that depend on the ordered outcome document why it holds for
///     them.
///   </para>
///   <para>
///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
///   </para>
/// </remarks>
public class ExpansionParentTopicViewModel {

  /*============================================================================================================================
  | PROPERTY: RELATED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A relationship collection that reaches the shared topic while requesting only <see cref="AssociationTypes.Children"/>.
  /// </summary>
  [Collection("Related", Type = CollectionType.Relationship)]
  [Include(AssociationTypes.Children)]
  public Collection<ExpansionSharedTopicViewModel> Related { get; } = new();

  /*============================================================================================================================
  | PROPERTY: CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   A children collection that reaches the shared topic while requesting only <see cref="AssociationTypes.Relationships"/>.
  /// </summary>
  [Include(AssociationTypes.Relationships)]
  public Collection<ExpansionSharedTopicViewModel> Children { get; } = new();

} //Class