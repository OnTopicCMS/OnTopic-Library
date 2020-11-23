/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: RELATION WITH CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a specialized version of <see cref="RelationTopicViewModel"/> for a relation with <see cref="Children"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Intended as a stand-in for cases where a very simple view model is required for test purposes, without introducing
  ///     other mapping scenarios that might introduce errors, even though they've not part of the test.
  ///   </para>
  ///   <para>
  ///     This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  ///   </para>
  /// </remarks>
  public class RelationWithChildrenTopicViewModel: RelationTopicViewModel {

    [Follow(Relationships.Relationships)]
    public List<RelationWithChildrenTopicViewModel> Children { get; } = new();

  } //Class
} //Namespace