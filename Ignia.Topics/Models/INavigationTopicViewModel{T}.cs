/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/
using System.Collections.ObjectModel;

namespace Ignia.Topics.Models {

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
  public interface INavigationTopicViewModel<T> :
    ITopicViewModel, IHierarchicalTopicViewModel<T>
    where T: INavigationTopicViewModel<T>
  {

    /*==========================================================================================================================
    | PROPERTY: WEBPATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents the HTTP routing path for the corresponding <see cref="Topic"/>.
    /// </summary>
    string? WebPath { get; set; }

    /*==========================================================================================================================
    | PROPERTY: SHORT TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   In addition to the Title, a site may opt to define a Short Title used exclusively in the navigation. If present, this
    ///   value should be used instead of Title.
    /// </summary>
    string? ShortTitle { get; set; }

    /*==========================================================================================================================
    | METHOD: ISSELECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the current item is selected based on the provided <paramref name="uniqueKey"/>.
    /// </summary>
    /// <param name="uniqueKey">
    ///   The unique key to compare against the current <see cref="INavigationTopicViewModel{T}"/>
    /// </param>
    bool IsSelected(string uniqueKey);

  } // Class

} // Namespace