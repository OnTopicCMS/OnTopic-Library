/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Mapping;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     It is not strictly required that topic view models implement the <see cref="ITopicViewModel"/> interface for the
  ///     default <see cref="TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models.
  ///     That said, the interface does define properties that presentation infrastructure may expect. As a result, if it isn't
  ///     provided via the public interface then it will instead need to be defined in some other way.
  ///   </para>
  ///   <para>
  ///     For instance, in the default MVC library, the <c>TopicViewResult</c> class requires that the <see cref="Topic.
  ///     ContentType"/> and <see cref="Topic.View"/> be supplied separately if they're not provided as part of a <see cref="
  ///     ITopicViewModel"/>. The exact details of this will obviously vary based on the implementation of the presentation
  ///     layer and any supporting libraries.
  ///   </para>
  /// </remarks>
  public interface ITopicViewModel: ICoreTopicViewModel, IAssociatedTopicBindingModel, ITopicBindingModel {

    /*==========================================================================================================================
    | PROPERTY: ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's ID attribute, the primary unique identifier for the topic.
    /// </summary>
    int Id { get; init; }

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's <see cref="WebPath"/> attribute, which represents the <see cref="IAssociatedTopicBindingModel
    ///   .UniqueKey"/> in its URL format.
    /// </summary>
    [Required, NotNull, DisallowNull]
    string? WebPath { get; init; }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute, representing the default view to be used for the topic.
    /// </summary>
    /// <remarks>
    ///   This value can be set via the query string (via the <c>TopicViewResultExecutor</c> class), via the Accepts header
    ///   (also via the <c>TopicViewResultExecutor</c> class), on the topic itself (via this property), or via the <see cref="
    ///   ICoreTopicViewModel.ContentType"/>. By default, it will be set to the name of the <see cref="ICoreTopicViewModel.
    ///   ContentType"/>; e.g., if the Content Type is <c>Page</c>, then the view will be <c>Page</c>. This will cause the <c>
    ///   TopicViewResultExecutor</c> to look for a view at, for instance, <c>/Views/Page/Page.cshtml</c>.
    /// </remarks>
    string? View { get; init; }

    /*==========================================================================================================================
    | PROPERTY: IS HIDDEN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is hidden.
    /// </summary>
    [Obsolete("The IsHidden property is no longer supported by ITopicViewModel.", true)]
    bool IsHidden { get; init; }

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute, which represents the friendly name of the topic.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="ICoreTopicViewModel.Key"/> may not contain, for instance, spaces or symbols, there are no
    ///   restrictions on what characters can be used in the title. For this reason, it provides the default public value for
    ///   referencing topics.
    /// </remarks>
    [Required, NotNull, DisallowNull]
    string? Title { get; init; }

  } //Class
} //Namespace