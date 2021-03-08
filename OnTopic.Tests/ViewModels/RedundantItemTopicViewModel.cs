/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.ViewModels;

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: REDUNDANT ITEM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a simple view model with two associations that will be included separately via the <see cref="
  ///   RedundantTopicViewModel"/>
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class RedundantItemTopicViewModel {

    public TopicViewModel? Parent { get; init; }
    public TopicViewModel? Reference { get; init; }

  } //Class
} //Namespace