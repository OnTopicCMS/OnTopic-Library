/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Mapping.Annotations;
using OnTopic.Models;

namespace OnTopic.ViewModels {

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

    /*==========================================================================================================================
    | ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public int Id { get; set; }

    /*==========================================================================================================================
    | KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? Key { get; set; }

    /*==========================================================================================================================
    | CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? ContentType { get; set; }

    /*==========================================================================================================================
    | UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? UniqueKey { get; set; }

    /*==========================================================================================================================
    | WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? WebPath { get; set; }

    /*==========================================================================================================================
    | VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? View { get; set; }

    /*==========================================================================================================================
    | TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public string? Title { get; set; }

    /*==========================================================================================================================
    | IS HIDDEN?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public bool IsHidden { get; set; }

    /*==========================================================================================================================
    | LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The date that the topic was last modified on.
    /// </summary>
    public DateTime LastModified { get; set; }

    /*==========================================================================================================================
    | PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the parent <see cref="TopicViewModel"/> in the topic hierarchy.
    /// </summary>
    /// <remarks>
    ///   If the current <see cref="TopicViewModel"/> is being mapped as part of another <see cref="TopicViewModel"/>, then the
    ///   <see cref="Parent"/> property will only be mapped if that relationship includes a <see cref="FollowAttribute"/>
    ///   with a value including <see cref="Relationships.Parents"/>. If it does, all <see cref="Parent"/> topics will be mapped
    ///   up to the root of the site. No other relationships on the <see cref="Parent"/> view models will be mapped, even if
    ///   they are annotated with a <see cref="FollowAttribute"/>.
    /// </remarks>
    [Follow(Relationships.Parents)]
    public TopicViewModel? Parent { get; set; }

  } //Class
} //Namespace