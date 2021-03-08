/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: REDUNDANT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views with a redundant topic references, which are useful for
  ///   evaluating cache handling.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class RedundantTopicViewModel {

    [Include(AssociationTypes.Parents)]
    public TopicAssociationsViewModel? FirstItem { get; set; }

    [Include(AssociationTypes.References)]
    public TopicAssociationsViewModel? SecondItem { get; set; }

  } //Class
} //Namespace