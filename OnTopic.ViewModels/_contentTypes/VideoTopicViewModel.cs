/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: VIDEO TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed model for feeding views with information about a <c>Video</c> topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public record VideoTopicViewModel: PageTopicViewModel {

    /*==========================================================================================================================
    | VIDEO URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a URL reference to a video to display on the page.
    /// </summary>
    [Required]
    public Uri VideoUrl { get; init; } = default!;

    /*==========================================================================================================================
    | POSTER URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a URL reference to an image to display prior to playing the video.
    /// </summary>
    public Uri? PosterUrl { get; init; }

  } //Class
} //Namespace