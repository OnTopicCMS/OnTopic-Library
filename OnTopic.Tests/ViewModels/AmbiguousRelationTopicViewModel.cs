/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: AMBIGUOUS RELATION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a an ambiguously mapped relationship (<see cref="RelationshipAlias"/>).
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="RelationshipAlias"/> uses <see cref="CollectionAttribute"/> to set the relationship key to <c>
  ///     AmbiguousRelationship</c> and the <see cref="CollectionType"/> to <see cref="CollectionType.IncomingRelationship"/>.
  ///     <c>AmbiguousRelationship</c> refers to a relationship that is both outgoing and incoming.
  ///   </para>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  /// </remarks>
  public class AmbiguousRelationTopicViewModel: KeyOnlyTopicViewModel {

    [Collection("AmbiguousRelationship", Type= CollectionType.IncomingRelationship)]
    public Collection<KeyOnlyTopicViewModel> RelationshipAlias { get; } = new();

  } //Class
} //Namespace