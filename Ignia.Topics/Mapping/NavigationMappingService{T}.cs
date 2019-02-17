/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: NAVIGATION MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides methods for mapping multiple levels of a topic tree to a <see cref="INavigationTopicViewModel{T}"/> tree.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, global data required by the layout view are requested independently of the current page. This
  ///     allows each layout element to be provided with its own layout data, in the form of <see
  ///     cref="INavigationTopicViewModel{T}"/>s, instead of needing to add this data to every view model returned by <see
  ///     cref="TopicController"/>. The <see cref="LayoutController{T}"/> facilitates this by not only providing a default
  ///     implementation for <see cref="Menu"/>, but additionally providing protected helper methods that aid in locating and
  ///     assembling <see cref="Topic"/> and <see cref="INavigationTopicViewModel{T}"/> references that are relevant to
  ///     specific layout elements.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="NavigationMappingService{T}"/> does not assume that a
  ///     particular view model will be used, and instead accepts a generic argument for any view model that implements the
  ///     interface <see cref="INavigationTopicViewModel{T}"/>.
  ///   </para>
  /// </remarks>
  public class NavigationMappingService<T> : INavigationMappingService<T> where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRoutingService            _topicRoutingService            = null;
    private readonly            ITopicMappingService            _topicMappingService            = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public NavigationMappingService(
      ITopicRepository topicRepository,
      ITopicRoutingService topicRoutingService,
      ITopicMappingService topicMappingService
    ) {
      TopicRepository           = topicRepository;
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
    private ITopicRepository TopicRepository { get; } = null;

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
    public Topic GetNavigationRoot(Topic currentTopic, int fromRoot = 2, string defaultRoot = "Web") {

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
    public virtual async Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    ) => await GetViewModelAsync(sourceTopic, allowPageGroups, tiers).ConfigureAwait(false);

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    public async Task<T> GetViewModelAsync(
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
      viewModel                 = await _topicMappingService.MapAsync<T>(sourceTopic, Relationships.None).ConfigureAwait(false);

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
        var dtoTask = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        taskQueue.Remove(dtoTask);
        children.Add(await dtoTask.ConfigureAwait(false));
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