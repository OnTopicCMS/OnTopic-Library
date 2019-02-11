/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using Ignia.Topics.Mapping;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: FLATTEN CHILDREN TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing view properties annotated with the <see
  ///   cref="FlattenAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class FlattenChildrenTopicViewModel {

    [Flatten]
    public List<FlattenChildrenTopicViewModel> Children { get; } = new List<FlattenChildrenTopicViewModel>();

  } //Class
} //Namespace
