/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping.Annotations;
using OnTopic.ViewModels;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: TOPIC ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with two associations. Useful for testing the <see cref="IncludeAttribute"/> as well as
  ///   caching via e.g. <see cref="RedundantTopicViewModel"/> and <see cref="ProgressiveTopicViewModel"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class TopicAssociationsViewModel {

    public TopicViewModel? Parent { get; init; }
    public TopicViewModel? Reference { get; init; }

  } //Class
} //Namespace