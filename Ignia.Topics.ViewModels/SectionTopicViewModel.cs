/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace Ignia.Topics.ViewModels {

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
  public class SectionTopicViewModel : TopicViewModel {

    /*==========================================================================================================================
    | HEADER IMAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a header image which may be displayed at the top of a section.
    /// </summary>
    public string? HeaderImageUrl { get; set; }

  } //Class
} //Namespace