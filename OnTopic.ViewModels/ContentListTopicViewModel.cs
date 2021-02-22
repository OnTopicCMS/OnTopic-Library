/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.ViewModels.Collections;
using OnTopic.ViewModels.Items;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONTENT LIST TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>ContentList</c> topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record ContentListTopicViewModel: PageTopicViewModel {

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

    /*==========================================================================================================================
    | IS INDEXED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a flag determining whether or not the content list should be indexed.
    /// </summary>
    /// <remarks>
    ///   The <see cref="IsIndexed"/> property, if set, requests that the view provides a list of <see cref="ContentItems"/> or
    ///   <see cref="Categories"/> at the top, with links to each one. This is entirely up to the discretion of the view as to
    ///   how to implement—or even whether it's appropriate. For instance, it might not make sense on views rendering as an
    ///   accordion or a searchable list. Nevertheless, it's a common enough feature that having it exposed as a first-party
    ///   option helps implementors account for common scenarios. As with other properties, it won't be set if there isn't a
    ///   corresponding attribute, and so this can easily be hidden or disabled globally via the editor.
    /// </remarks>
    /// <returns>True if the content list should be indexed; false otherwise.</returns>
    public bool IsIndexed { get; init; }

    /*==========================================================================================================================
    | INDEX LABEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assuming <see cref="IsIndexed"/>, determines the label to display. Defaults to <i>Contents</i>.
    /// </summary>
    /// <remarks>
    ///   Often, if an indexed list of links is provided on a view, it will be preceded with a lable. The <see cref=
    ///   "IndexLabel"/> allows that to be optionally set on a per topic basis. The default value is "Contents", though it is up
    ///   to view implementors and editor configurations as to whether this option is exposed or honored.
    /// </remarks>
    /// <returns>Returns the value init; defaults to "Contents".</returns>
    public string IndexLabel { get; init; } = "Contents";

  } //Class
} //Namespace