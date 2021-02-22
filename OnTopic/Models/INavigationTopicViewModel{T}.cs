/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: NAVIGATION TOPIC VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views information about a navigation entry.
  /// </summary>
  /// <remarks>
  ///   No topics are expected to have a <c>Navigation</c> content type. Instead, implementers of this view model are expected
  ///   to manually construct instances.
  /// </remarks>
  public interface INavigationTopicViewModel<T> : IHierarchicalTopicViewModel<T> where T: INavigationTopicViewModel<T> {

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="ITopicViewModel.Title"/>
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
    string? WebPath { get; init; }

    /*==========================================================================================================================
    | METHOD: IS SELECTED?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the current item is selected based on the provided <paramref name="webPath"/>.
    /// </summary>
    /// <param name="webPath">
    ///   The path to compare against the current <see cref="INavigationTopicViewModel{T}"/>
    /// </param>
    bool IsSelected(string webPath);

  } //Class
} //Namespace