/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: SPECIALIZED ASCENDENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a derived version of <see cref="AscendentTopicViewModel"/> with an additional property (<see cref="IsRoot"/>),
  ///   thus allowing tests to confirm that specialized content types are correctly mapped.
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
  public class AscendentSpecializedTopicViewModel: AscendentTopicViewModel {

    public bool IsRoot { get; set; }

  } //Class
} //Namespace