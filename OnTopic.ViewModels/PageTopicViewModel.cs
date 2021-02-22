/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Models;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: PAGE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>Page</c> topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record PageTopicViewModel: TopicViewModel, IPageTopicViewModel {

    /*==========================================================================================================================
    | SUBTITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides an optional subtitle which will typically be displayed under the title.
    /// </summary>
    public string? Subtitle { get; init; }

    /*==========================================================================================================================
    | META TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides an optional title to be used in page's metadata, if it differs from the <see cref="TopicViewModel.Title"/>.
    /// </summary>
    public string? MetaTitle { get; init; }

    /*==========================================================================================================================
    | META DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? MetaDescription { get; init; }

    /*==========================================================================================================================
    | META KEYWORDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? MetaKeywords { get; init; }

    /*==========================================================================================================================
    | META KEYWORDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not search engines are expected to index the page.
    /// </summary>
    public bool? NoIndex { get; init; }

    /*==========================================================================================================================
    | SHORT TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a short title to be used in the navigation, for cases where the normal title is too long.
    /// </summary>
    public string? ShortTitle { get; init; }

    /*==========================================================================================================================
    | BODY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the primary content for the page, which is typically in HTML format.
    /// </summary>
    public string? Body { get; init; }

  } //Class
} //Namespace