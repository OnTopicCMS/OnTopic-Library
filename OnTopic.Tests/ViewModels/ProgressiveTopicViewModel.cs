/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: REDUNDANT TOPIC (PROGRESSIVE)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with a redundant topic references with differerent
  ///   inclusions, which is useful for evaluating progressive cache handling.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class ProgressiveTopicViewModel {

    [Include(AssociationTypes.Parents)]
    public TopicAssociationsViewModel? FirstItem { get; set; }

    [Include(AssociationTypes.Parents)]
    public TopicAssociationsViewModel? SecondItem { get; set; }

  } //Class
} //Namespace