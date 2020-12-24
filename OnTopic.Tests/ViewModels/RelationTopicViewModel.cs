/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: RELATION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a single property (<see cref="Cousins"/>) for mapping relationships.
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
  public class RelationTopicViewModel: KeyOnlyTopicViewModel {

    [Follow(Relationships.Children)]
    public Collection<RelationTopicViewModel> Cousins { get; } = new();

  } //Class
} //Namespace