﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.Tests.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: FILTERED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for testing views properties annotated with the <see
  ///   cref="FilterByAttributeAttribute"/>.
  /// </summary>
  /// <remarks>
  ///   This is a sample class intended for test purposes only; it is not designed for use in a production environment.
  /// </remarks>
  public class FilteredTopicViewModel {

    [FilterByAttribute("SomeAttribute", "ValueA")]
    [FilterByAttribute("SomeOtherAttribute", "ValueB")]
    public TopicViewModelCollection<TopicViewModel> Children { get; } = new();

  } //Class
} //Namespace