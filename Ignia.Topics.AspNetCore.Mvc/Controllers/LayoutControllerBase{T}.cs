/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;
using Ignia.Topics.AspNetCore.Mvc.Models;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: LAYOUT CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to views for populating specific layout dependencies, such as the <see cref="Menu"/>.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, global data required by the layout view are requested independently of the current page. This
  ///     allows each layout element to be provided with its own layout data, in the form of <see
  ///     cref="NavigationViewModel{T}"/>s, instead of needing to add this data to every view model returned by <see
  ///     cref="TopicController"/>. The <see cref="LayoutController{T}"/> facilitates this by not only providing a default
  ///     implementation for <see cref="Menu"/>, but additionally providing protected helper methods that aid in locating and
  ///     assembling <see cref="Topic"/> and <see cref="INavigationTopicViewModelCore"/> references that are relevant to
  ///     specific layout elements.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="LayoutController{T}"/> does not assume that a particular view
  ///     model will be used, and instead accepts a generic argument for any view model that implements the interface <see
  ///     cref="INavigationTopicViewModelCore"/>. Since generic controllers cannot be effectively routed to, however, that means
  ///     implementors must, at minimum, provide a local instance of <see cref="LayoutController{T}"/> which sets the generic
  ///     value to the desired view model. To help enforce this, while avoiding ambiguity, this class is marked as
  ///     <c>abstract</c> and suffixed with <c>Base</c>.
  ///   </para>
  /// </remarks>
  public abstract class LayoutControllerBase<T> : Controller where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _topicRepository                = null;
    private readonly            ITopicRoutingService            _topicRoutingService            = null;
    private readonly            ITopicMappingService            _topicMappingService            = null;
    private                     Topic                           _currentTopic                   = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    protected LayoutControllerBase(
      ITopicRepository topicRepository,
      ITopicRoutingService topicRoutingService,
      ITopicMappingService topicMappingService
    ) : base() {
      _topicRepository          = topicRepository;
      _topicRoutingService      = topicRoutingService;
      _topicMappingService      = topicMappingService;
    }

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the Topic Repository in order to gain arbitrary access to the entire topic graph.
    /// </summary>
    /// <returns>The TopicRepository associated with the controller.</returns>
    protected ITopicRepository TopicRepository => _topicRepository;

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    protected Topic CurrentTopic {
      get {
        if (_currentTopic == null) {
          _currentTopic = _topicRoutingService.GetCurrentTopic();
        }
        return _currentTopic;
      }
    }

    /*==========================================================================================================================
    | MENU
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides the global menu for the site layout, which exposes the top two tiers of navigation.
    /// </summary>
    public async virtual Task<PartialViewResult> Menu() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var currentTopic          = CurrentTopic;
      var navigationRootTopic   = (Topic)null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify navigation root
      >-------------------------------------------------------------------------------------------------------------------------
      | The navigation root in the case of the main menu is the namespace; i.e., the first topic underneath the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      navigationRootTopic = GetNavigationRoot(currentTopic, 2, "Web");

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationViewModel   = new NavigationViewModel<T>() {
        NavigationRoot          = await GetRootViewModelAsync(navigationRootTopic, false, 3),
        CurrentKey              = CurrentTopic?.GetUniqueKey()
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the corresponding view
      \-----------------------------------------------------------------------------------------------------------------------*/
      return PartialView(navigationViewModel);

    }

    /*==========================================================================================================================
    | GET NAVIGATION ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A helper function that will crawl up the current tree and retrieve the topic that is <paramref name="fromRoot"/> tiers
    ///   down from the root of the topic graph.
    /// </summary>
    /// <remarks>
    ///   Often, an action of a <see cref="LayoutController{T}"/> will need a reference to a topic at a certain level, which
    ///   represents the navigation for the site. For instance, if the primary navigation is at <c>Root:Web</c>, then the
    ///   navigation is one level from the root (i.e., <paramref name="fromRoot"/>=1). This, however, should not be hard-coded
    ///   in case a site has multiple roots. For instance, if a page is under <c>Root:Library</c> then <i>that</i> should be the
    ///   navigation root. This method provides support for these scenarios.
    /// </remarks>
    /// <param name="currentTopic">The <see cref="Topic"/> to start from.</param>
    /// <param name="fromRoot">The distance that the navigation root should be from the root of the topic graph.</param>
    /// <param name="defaultRoot">If a root cannot be identified, the default root that should be returned.</param>
    protected Topic GetNavigationRoot(Topic currentTopic, int fromRoot = 2, string defaultRoot = "Web") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Find navigation root
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationRootTopic = currentTopic;
      while (DistanceFromRoot(navigationRootTopic) > fromRoot) {
        navigationRootTopic = navigationRootTopic.Parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle default, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (navigationRootTopic == null && !String.IsNullOrWhiteSpace(defaultRoot)) {
        navigationRootTopic = TopicRepository.Load(defaultRoot);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return navigation root
      \-----------------------------------------------------------------------------------------------------------------------*/
      return navigationRootTopic;

    }

    /*==========================================================================================================================
    | DISTANCE FROM ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A helper function that will determine how far a given topic is from the root of a tree.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    private static int DistanceFromRoot(Topic sourceTopic) {
      var distance = 1;
      while (sourceTopic?.Parent != null) {
        sourceTopic = sourceTopic.Parent;
        distance++;
      }
      return distance;
    }

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <remarks>
    ///   In the out-of-the-box implementation, <see cref="GetRootViewModelAsync(Topic, Boolean, Int32)"/> and <see
    ///   cref="GetViewModelAsync(Topic, Boolean, Int32)"/> provide the same functionality. It is recommended that actions call
    ///   <see cref="GetRootViewModelAsync(Topic, Boolean, Int32)"/>, however, as it allows implementers the flexibility to
    ///   differentiate between the root view model (which the client application will be binding to) and any child view models
    ///   (which the client application may optionally iterate over).
    /// </remarks>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    protected virtual async Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    ) => await GetViewModelAsync(sourceTopic, allowPageGroups, tiers);

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    protected async Task<T> GetViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups      = true,
      int tiers                 = 1
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate preconditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      tiers--;
      if (sourceTopic == null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue             = new List<Task<T>>();
      var children              = new List<T>();
      var viewModel             = (T)null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Map object
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel                 = await _topicMappingService.MapAsync<T>(sourceTopic, Relationships.None);

      /*------------------------------------------------------------------------------------------------------------------------
      | Request mapping of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (tiers >= 0 && (allowPageGroups || sourceTopic.ContentType != "PageGroup") && viewModel.Children.Count == 0) {
        foreach (var topic in sourceTopic.Children.Where(t => t.IsVisible())) {
          taskQueue.Add(GetViewModelAsync(topic, allowPageGroups, tiers));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process children
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0 && viewModel.Children.Count == 0) {
        var dtoTask = await Task.WhenAny(taskQueue);
        taskQueue.Remove(dtoTask);
        children.Add(await dtoTask);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add children to view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (viewModel.Children.Count == 0) {
        lock (viewModel) {
          if (viewModel.Children.Count == 0) {
            children.ForEach(c => viewModel.Children.Add(c));
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return view model
      \-----------------------------------------------------------------------------------------------------------------------*/
      return viewModel;
    }

  } // Class

} // Namespace