/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Models;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views.
  /// </summary>
  /// <remarks>
  ///   Typically, view models should be created as part of the presentation layer. The <see cref="Models"/> namespace contains
  ///   default implementations that can be used directly, used as base classes, or overwritten at the presentation level. They
  ///   are supplied for convenience to model factory default settings for out-of-the-box content types.
  /// </remarks>
  public class TopicViewModel: ITopicViewModel {

    /// <inheritdoc />
    public int Id { get; set; }

    /// <inheritdoc />
    public string? Key { get; set; }

    /// <inheritdoc />
    public string? ContentType { get; set; }

    /// <inheritdoc />
    public string? UniqueKey { get; set; }

    /// <inheritdoc />
    public string? WebPath { get; set; }

    /// <inheritdoc />
    public string? View { get; set; }

    /// <inheritdoc />
    public string? Title { get; set; }

    /// <inheritdoc />
    public bool IsHidden { get; set; }

    public DateTime LastModified { get; set; }

    [Follow(Relationships.Parents)]
    public TopicViewModel? Parent { get; set; }

  } //Class
} //Namespace