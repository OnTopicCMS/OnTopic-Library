/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONTENT LIST TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a content list topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class ContentListTopicViewModel: PageTopicViewModel {

    /*==========================================================================================================================
    | CONTENT ITEMS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of <see cref="ContentItemTopicViewModel"/>, representing the contents of the <see
    ///   cref="ContentListTopicViewModel"/>.
    /// </summary>
    public TopicViewModelCollection<ContentItemTopicViewModel> ContentItems { get; } = new();

    /*==========================================================================================================================
    | CATEGORIES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a list of valid categories that each of the <see cref="ContentItems"/> may optionally be associated with.
    /// </summary>
    public TopicViewModelCollection<TopicViewModel> Categories { get; } = new();

  } //Class
} //Namespace