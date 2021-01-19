/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: CONTENT ITEM TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a content item topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record ContentItemTopicViewModel: ItemTopicViewModel {

    /*==========================================================================================================================
    | DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the description; for Content Items, this is effectively the body.
    /// </summary>
    public string Description { get; init; } = default!;

    /*==========================================================================================================================
    | LEARN MORE URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an optional URL for additional information that should be linked to.
    /// </summary>
    public Uri? LearnMoreUrl { get; init; }

    /*==========================================================================================================================
    | THUMBNAIL IMAGE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets an optional path to a thumbnail image that should accompany the content item.
    /// </summary>
    public Uri? ThumbnailImage { get; init; }

    /*==========================================================================================================================
    | CATEGORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the category that the content item should be grouped under.
    /// </summary>
    public string? Category { get; init; }

  } //Class
} //Namespace