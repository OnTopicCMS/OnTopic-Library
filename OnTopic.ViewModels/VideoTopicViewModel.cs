/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/

using System;

namespace OnTopic.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: VIDEO TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a video topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class VideoTopicViewModel: PageTopicViewModel {

    /*==========================================================================================================================
    | VIDEO URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a URL reference to a video to display on the page.
    /// </summary>
    public Uri? VideoUrl { get; set; }

    /*==========================================================================================================================
    | POSTER URL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a URL reference to an image to display prior to playing the video.
    /// </summary>
    public Uri? PosterUrl { get; set; }

  } //Class
} //Namespace