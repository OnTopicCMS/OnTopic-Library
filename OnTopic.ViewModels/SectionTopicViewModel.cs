/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: SECTION TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a section topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record SectionTopicViewModel : TopicViewModel {

    /*==========================================================================================================================
    | HEADER IMAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a header image which may be displayed at the top of a section.
    /// </summary>
    public Uri? HeaderImageUrl { get; init; }

  } //Class
} //Namespace