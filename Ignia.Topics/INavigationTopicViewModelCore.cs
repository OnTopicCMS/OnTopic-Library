/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using Ignia.Topics;

namespace Ignia.Topics {

  /*============================================================================================================================
  | INTERFACE: PAGE TOPIC VIEW MODEL CORE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a generic data transfer topic for feeding views information about a navigation entry.
  /// </summary>
  /// <remarks>
  ///   No topics are expected to have a <c>Navigation</c> content type. Instead, implementers of this view model are expected
  ///   to manually construct instances.
  /// </remarks>
  public interface INavigationTopicViewModelCore<T> : IPageTopicViewModelCore where T: INavigationTopicViewModelCore<T> {

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents a collection of child <see cref="INavigationTopicViewModelCore{T}"/> instances.
    /// </summary>
    Collection<T> Children { get; set; }

    /*==========================================================================================================================
    | METHOD: ISSELECTED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the current item is selected based on the provided <paramref name="uniqueKey"/>.
    /// </summary>
    /// <param name="uniqueKey">
    ///   The unique key to compare against the current <see cref="INavigationTopicViewModelCore{T}"/>
    /// </param>
    bool IsSelected(string uniqueKey);

  } // Class

} // Namespace