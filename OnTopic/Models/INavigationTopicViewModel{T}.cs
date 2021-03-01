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
  ///   Provides a generic model for feeding views information about a navigation entry.
  /// </summary>
  /// <remarks>
  ///   No topics are expected to have a <c>Navigation</c> content type. Instead, implementers of this view model are expected
  ///   to manually construct instances.
  /// </remarks>
  public interface INavigationTopicViewModel<T> :
    INavigableTopicViewModel,
    IHierarchicalTopicViewModel<T> where T: INavigationTopicViewModel<T>
  {

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