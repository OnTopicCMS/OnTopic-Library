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
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: NAVIGATION MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Implements a service that maps a limited-hierarchy of topics to a generic class representing the core properties
  ///   associated with a navigation item.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Ideally, this functionality would be baked directly into the <see cref="ITopicMappingService"/> implementations, but
  ///     this introduces a number of technical issues that make this unfeasible for now. Instead, the <see
  ///     cref="IHierarchicalTopicMappingService{T}"/> handles this functionality for special cases, such as navigation, where
  ///     e.g. the full recursion of the <see cref="ITopicMappingService"/> is not preferrable for functional or performance
  ///     reasons.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="HierarchicalTopicMappingService{T}"/> does not assume that a
  ///     particular view model will be used, and instead accepts a generic argument for any view model that implements the
  ///     interface <see cref="IHierarchicalTopicViewModel{T}"/>.
  ///   </para>
  /// </remarks>
  /// <typeparam name="T">A view model implementing the <see cref="IHierarchicalTopicViewModel{T}"/> interface.</typeparam>
  public class HierarchicalTopicMappingService<T>
    : IHierarchicalTopicMappingService<T>
    where T : class, IHierarchicalTopicViewModel<T>, new()
  {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicMappingService            _topicMappingService            = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="HierarchicalTopicMappingService{T}"/> with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    public HierarchicalTopicMappingService(
      ITopicRepository topicRepository,
      ITopicMappingService topicMappingService
    ) {
      TopicRepository           = topicRepository;
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
    | GET HIERARCHICAL ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdocs />
    public Topic GetHierarchicalRoot(Topic currentTopic, int fromRoot = 2, string defaultRoot = "Web") {

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
    /// <inheritdocs />
    public virtual async Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      int tiers = 1,
      Func<Topic, bool> validationDelegate = null
    ) => await GetViewModelAsync(sourceTopic, tiers, validationDelegate).ConfigureAwait(false);

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdocs />
    public async Task<T> GetViewModelAsync(
      Topic sourceTopic,
      int tiers = 1,
      Func<Topic, bool> validationDelegate = null
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
      | Establish default delegate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (validationDelegate == null) {
        validationDelegate = (Topic) => true;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map object
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel = await _topicMappingService.MapAsync<T>(sourceTopic, Relationships.None).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Request mapping of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (tiers >= 0 && validationDelegate(sourceTopic) && viewModel.Children.Count == 0) {
        foreach (var topic in sourceTopic.Children.Where(t => t.IsVisible())) {
          taskQueue.Add(GetViewModelAsync(topic, tiers, validationDelegate));
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