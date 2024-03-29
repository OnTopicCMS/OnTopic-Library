﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: PAGE TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views based on the page content type.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     It is not strictly required that topic view models implement the <see cref="IPageTopicViewModel"/> interface for the
  ///     default <see cref="TopicMappingService"/> to correctly identify and map <see cref="Topic"/>s to topic view models.
  ///     That said, the interface does define properties that presentation infrastructure may expect. As a result, if it isn't
  ///     provided via the public interface then it will instead need to be defined in some other way.
  ///   </para>
  /// </remarks>
  [Obsolete($"The {nameof(IPageTopicViewModel)} is no longer utilized.", true)]
  public interface IPageTopicViewModel : ITopicViewModel, INavigableTopicViewModel {

    /*==========================================================================================================================
    | PROPERTY: META KEYWORDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Meta Keywords attribute, which represents the HTML metadata that will be presented alongside the
    ///   page.
    /// </summary>
    string? MetaKeywords { get; init; }

    /*==========================================================================================================================
    | PROPERTY: META DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Meta Description attribute, which represents the HTML metadata that will be presented alongside the
    ///   page.
    /// </summary>
    string? MetaDescription { get; init; }

  } //Class
} //Namespace