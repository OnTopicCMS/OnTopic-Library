/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: DESCENDENT SPECIALIZED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a derived version of <see cref="DescendentTopicViewModel"/> with an additional property (<see cref="IsLeaf"/>),
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
  public record DescendentSpecializedTopicViewModel: DescendentTopicViewModel {

    public bool IsLeaf { get; set; }

  } //Class
} //Namespace