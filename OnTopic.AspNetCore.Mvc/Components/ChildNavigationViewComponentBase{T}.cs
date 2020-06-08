/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Models;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.AspNetCore.Mvc.Components {

  /*============================================================================================================================
  | CLASS: CHILD NAVIGATION VIEW COMPONENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a <see cref="ViewComponent"/> which provides access to a menu of <typeparamref name="NavigationTopicViewModel"/>
  ///   instances representing the direct descendents of the current <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   In order to remain view model agnostic, the <see cref="ChildNavigationViewComponentBase{T}"/> does not assume that a
  ///   particular view model will be used, and instead accepts a generic argument for any view model that implements the
  ///   interface <see cref="INavigationTopicViewModel{T}"/>. Since generic view components cannot be effectively routed to,
  ///   however, that means implementors must, at minimum, provide a local instance of <see
  ///   cref="ChildNavigationViewComponentBase{T}"/> which sets the generic value to the desired view model. To help
  ///   enforce this, while avoiding ambiguity, this class is marked as <c>abstract</c> and suffixed with <c>Base</c>.
  /// </remarks>
  public abstract class ChildNavigationViewComponentBase<T> :
    NavigationTopicViewComponentBase<T> where T : class, INavigationTopicViewModel<T>, new()
  {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="ChildNavigationViewComponentBase{T}"/> with necessary dependencies.
    /// </summary>
    protected ChildNavigationViewComponentBase(
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
    ///   The navigation root in the case of the child navigation is simply the <see cref="CurrentTopic"/>.
    /// </remarks>
    protected Topic? GetNavigationRoot() => CurrentTopic;

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

      Contract.Assume(CurrentTopic, $"The current topic could not be identified for the page-level navigation.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationViewModel = new NavigationViewModel<T>() {
        NavigationRoot = await MapNavigationTopicViewModels(navigationRootTopic).ConfigureAwait(true),
        CurrentKey = CurrentTopic.GetUniqueKey()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the corresponding view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return View(navigationViewModel);

    }

  } //Class
} //Namespace