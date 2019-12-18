﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Models;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: PAGE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a page topic.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class PageTopicViewModel: TopicViewModel, IPageTopicViewModel {

    public string? Subtitle { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool? NoIndex { get; set; }
    public string? ShortTitle { get; set; }
    public string? Body { get; set; }

  } //Class
} //Namespace