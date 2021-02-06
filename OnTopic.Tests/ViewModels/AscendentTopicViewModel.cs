/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: ASCENDENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with a single property (<see cref="Parent"/>) for mapping ascendent associations.
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
  public class AscendentTopicViewModel: KeyOnlyTopicViewModel {

    [Include(AssociationTypes.Parents)]
    public AscendentTopicViewModel? Parent { get; set; }

  } //Class
} //Namespace