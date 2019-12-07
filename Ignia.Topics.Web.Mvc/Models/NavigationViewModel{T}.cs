/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Models;

namespace Ignia.Topics.Web.Mvc.Models {

  /*============================================================================================================================
  | VIEW MODEL: NAVIGATION TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about a tier of navigation.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     No topics are expected to have a <c>Navigation</c> content type. Instead, this view model is expected to be manually
  ///     constructed by the <see cref="LayoutController"/>.
  ///   </para>
  ///   <para>
  ///     The <see cref="NavigationRoot"/> can be any view model that implements <see cref="INavigationTopicViewModel{T}"/>,
  ///     which provides a base level of support for properties associated with the typical <c>Page</c> content type as well as
  ///     a method for determining if a given <see cref="INavigationTopicViewModel{T}"/> instance is the currently-selected
  ///     topic. Implementations may support additional properties, as appropriate.
  ///   </para>
  /// </remarks>
  public class NavigationViewModel<T> where T: INavigationTopicViewModel<T> {

    public T NavigationRoot { get; set; }
    public string CurrentKey { get; set; }

  } //Class
} //Namespace