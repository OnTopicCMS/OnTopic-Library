/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/

using System.Collections.ObjectModel;

namespace Ignia.Topics.ViewModels {

  /*============================================================================================================================
  | VIEW MODEL: NAVIGATION TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a strongly-typed data transfer object for feeding views with information about the navigation.
  /// </summary>
  /// <remarks>
  ///   No topics are expected to have a <c>Navigation</c> content type. Instead, this view model is expected to be manually
  ///   constructed by e.g. a <c>LayoutController</c>.
  /// </remarks>
  public class NavigationTopicViewModel : PageTopicViewModel, INavigationTopicViewModelCore {

    public Collection<INavigationTopicViewModelCore> Children { get; set; }
    public bool IsSelected(string uniqueKey) => uniqueKey?.StartsWith(this.UniqueKey) ?? false;

  } // Class

} // Namespace