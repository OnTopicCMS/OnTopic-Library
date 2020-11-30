/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Models;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Models;
using OnTopic.Repositories;

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
    ///   Initializes a new instance of a <see cref="PageLevelNavigationViewComponentBase{T}"/> with necessary dependencies.
    /// </summary>
    protected PageLevelNavigationViewComponentBase(
      ITopicRepository topicRepository,
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) : base(
      topicRepository,
      hierarchicalTopicMappingService
    ) {}

    /*==========================================================================================================================
    | METHOD: GET NAVIGATION ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the root <see cref="Topic"/> from which to map the <typeparamref name="T"/> objects.
    /// </summary>
    /// <remarks>
    ///   The navigation root in the case of the page-level navigation any parent of content type <c>PageGroup</c>.
    /// </remarks>
    protected Topic? GetNavigationRoot() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = CurrentTopic;
      var navigationRootTopic   = (Topic?)currentTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify navigation root
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (navigationRootTopic is not null) {
        while (
          navigationRootTopic is not null and not ({ Parent: null } or { ContentType: "PageGroup" })
        ) {
          navigationRootTopic = navigationRootTopic.Parent;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return root
      \-----------------------------------------------------------------------------------------------------------------------*/
      return navigationRootTopic?.Parent is null? null : navigationRootTopic;

    }

    /*==========================================================================================================================
    | METHOD: MAP NAVIGATION TOPIC VIEW MODELS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Maps a list of <typeparamref name="T"/> instances based on the <paramref name="navigationRootTopic"/>.
    /// </summary>
    protected async Task<T?> MapNavigationTopicViewModels(Topic? navigationRootTopic) =>
      await HierarchicalTopicMappingService.GetRootViewModelAsync(navigationRootTopic).ConfigureAwait(true);

    /*==========================================================================================================================
    | METHOD: INVOKE (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the pagel-level navigation menu for the current page, which exposes one tier of navigation from the nearest
    ///   page group.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve root topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationRootTopic = GetNavigationRoot();

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationViewModel = new NavigationViewModel<T>() {
        NavigationRoot = await MapNavigationTopicViewModels(navigationRootTopic).ConfigureAwait(true),
        CurrentKey = CurrentTopic?.GetUniqueKey()?? HttpContext.Request.Path
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the corresponding view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View(navigationViewModel);

    }

  } //Class
} //Namespace