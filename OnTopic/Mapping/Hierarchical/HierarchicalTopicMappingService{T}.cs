/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping.Annotations;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Hierarchical {

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
    private readonly            ITopicMappingService            _topicMappingService;

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
    private ITopicRepository TopicRepository { get; }

    /*==========================================================================================================================
    | GET HIERARCHICAL ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public Topic? GetHierarchicalRoot(Topic? currentTopic, int fromRoot = 2, string defaultRoot = "Web") {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var navigationRootTopic = currentTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle default, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (navigationRootTopic is null) {
        if (String.IsNullOrEmpty(defaultRoot)) {
          throw new ArgumentOutOfRangeException(
            nameof(defaultRoot),
            "The current route could not be resolved to a topic and the defaultRoot was not set."
          );
        }
        navigationRootTopic = TopicRepository.Load(defaultRoot);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle error state
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (navigationRootTopic is null) {
        throw new ArgumentOutOfRangeException(
          nameof(defaultRoot),
          "Neither the current route nor the 'defaultRoot' parameter could be resolved to a topic."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Find navigation root
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (navigationRootTopic is not null && DistanceFromRoot(navigationRootTopic) > fromRoot) {
        navigationRootTopic = navigationRootTopic.Parent;
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
    private static int DistanceFromRoot(Topic? sourceTopic) {
      var distance = 1;
      while (sourceTopic?.Parent is not null) {
        sourceTopic = sourceTopic.Parent;
        distance++;
      }
      return distance;
    }

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual async Task<T?> GetRootViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    ) => await GetViewModelAsync(sourceTopic, tiers, validationDelegate).ConfigureAwait(false);

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> GetViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate preconditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      tiers--;
      if (sourceTopic is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue             = new List<Task<T?>>();
      var children              = new List<T>();
      var viewModel             = (T?)null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish default delegate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (validationDelegate is null) {
        validationDelegate = (Topic) => true;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map object
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel = await _topicMappingService.MapAsync<T>(sourceTopic, Relationships.None).ConfigureAwait(false);

      Contract.Assume(
        viewModel,
        $"The 'ITopicMappingService' failed to return a {typeof(T)} model for the '{sourceTopic.GetUniqueKey()}' topic."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Request mapping of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (tiers >= 0 && viewModel.Children.Count == 0) {
        foreach (var topic in sourceTopic.Children.Where(t => t.IsVisible() && validationDelegate(t))) {
          taskQueue.Add(GetViewModelAsync(topic, tiers, validationDelegate));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process children
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0 && viewModel.Children.Count == 0) {
        var dtoTask             = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        var dto                 = await dtoTask.ConfigureAwait(false);
        taskQueue.Remove(dtoTask);
        if (dto is not null) {
          children.Add(dto);
        }
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

  } //Class
} //Namespace