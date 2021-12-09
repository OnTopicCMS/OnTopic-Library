/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CIRCULAR TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with a circular reference.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class CircularTopicViewModel {

    [Include(AssociationTypes.Parents)]
    public CircularTopicViewModel? Parent { get; set; }

    [Include(AssociationTypes.Children | AssociationTypes.Parents)]
    public Collection<CircularTopicViewModel> Children { get; } = new();

  } //Class
} //Namespace