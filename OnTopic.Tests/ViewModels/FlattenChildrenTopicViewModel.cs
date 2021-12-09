/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

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
    public Collection<FlattenChildrenTopicViewModel> Children { get; } = new();

  } //Class
} //Namespace