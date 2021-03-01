/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: NAVIGABLE TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Ensures basic properties needed to treat a view model as a navigable entry.
  /// </summary>
  /// <remarks>
  ///   No topics are expected to have a <c>Navigable</c> content type. Instead, implementers of this view model are expected
  ///   to manually construct instances.
  /// </remarks>
  public interface INavigableTopicViewModel {

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ITopicViewModel.Title"/>
    [Required, NotNull, DisallowNull]
    string? Title { get; init; }

    /*==========================================================================================================================
    | PROPERTY: SHORT TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   In addition to the Title, a site may opt to define a Short Title used exclusively in the navigation. If present, this
    ///   value should be used instead of Title.
    /// </summary>
    string? ShortTitle { get; init; }

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ITopicViewModel.WebPath"/>
    [Required, NotNull, DisallowNull]
    string? WebPath { get; init; }

  } //Class
} //Namespace