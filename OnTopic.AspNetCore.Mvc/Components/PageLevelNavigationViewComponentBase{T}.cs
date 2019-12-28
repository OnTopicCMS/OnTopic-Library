/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using OnTopic.AspNetCore.Mvc.Models;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Models;
using OnTopic.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace OnTopic.AspNetCore.Mvc.Components {

  /*============================================================================================================================
  | CLASS: PAGE-LEVEL NAVIGATION VIEW COMPONENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a <see cref="ViewComponent"/> which provides access to a menu of <typeparamref name="NavigationTopicViewModel"/>
  ///   instances representing the nearest page-level navigation.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, global data required by the layout view are requested independent of the current page. This
  ///     allows each layout element to be provided with its own layout data, in the form of <see
  ///     cref="NavigationViewModel{T}"/>s, instead of needing to add this data to every view model returned by <see
  ///     cref="TopicController"/>.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="PageLevelNavigationViewComponentBase{T}"/> does not assume that
  ///     a particular view model will be used, and instead accepts a generic argument for any view model that implements the
  ///     interface <see cref="INavigationTopicViewModel{T}"/>. Since generic view components cannot be effectively routed to,
  ///     however, that means implementors must, at minimum, provide a local instance of <see
  ///     cref="PageLevelNavigationViewComponentBase{T}"/> which sets the generic value to the desired view model. To help
  ///     enforce this, while avoiding ambiguity, this class is marked as <c>abstract</c> and suffixed with <c>Base</c>.
  ///   </para>
  /// </remarks>
  public abstract class PageLevelNavigationViewComponentBase<T> :
    NavigationTopicViewComponentBase<T> where T : class, INavigationTopicViewModel<T>, new()
  {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="MenuViewComponentBase{T}"/> with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    protected PageLevelNavigationViewComponentBase(
      ITopicRepository topicRepository,
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) : base(
      topicRepository,
      hierarchicalTopicMappingService
    ) {}

    /*==========================================================================================================================
    | METHOD: INVOKE (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the pagel-level navigation menu for the current page, which exposes one tier of navigation from the nearest
    ///   page group.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = CurrentTopic;
      var navigationRootTopic   = (Topic?)currentTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify navigation root
      >-------------------------------------------------------------------------------------------------------------------------
      | The navigation root in the case of the page-level navigation any parent of content type "PageGroup".
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (navigationRootTopic != null) {
        while (
          navigationRootTopic.Parent != null &&
          !navigationRootTopic.ContentType.Equals("PageGroup", StringComparison.InvariantCulture)
        ) {
          navigationRootTopic = navigationRootTopic.Parent;
        }
      }

      if (navigationRootTopic?.Parent == null) navigationRootTopic = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate conditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(CurrentTopic, $"The current topic could not be identified for the page-level navigation.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationViewModel = new NavigationViewModel<T>() {
        NavigationRoot = await HierarchicalTopicMappingService.GetRootViewModelAsync(navigationRootTopic).ConfigureAwait(true),
        CurrentKey = CurrentTopic.GetUniqueKey()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the corresponding view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View(navigationViewModel);

    }

  } //Class
} //Namespace