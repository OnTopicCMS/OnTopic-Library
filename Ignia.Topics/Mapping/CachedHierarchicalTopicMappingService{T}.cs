/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: CACHED HIERARCHICAL TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a wrapper to a <see cref="IHierarchicalTopicMappingService{T}"/> implementation with support for caching.
  /// </summary>
  /// <remarks>
  ///   By comparison to the <see cref="HierarchicalTopicMappingService{T}"/>, the <see
  ///   cref="CachedHierarchicalTopicMappingService{T}"/> will automatically cache the <see
  ///   cref="IHierarchicalTopicViewModel{T}"/> graph for each action that uses the protected <see
  ///   cref="IHierarchicalTopicMappingService{T}.GetViewModelAsync(Topic, Int32, Func{Topic, Boolean})"/> method to construct
  ///   the graph. This is preferable over using e.g. the <see cref="CachedTopicMappingService"/> since the <see
  ///   cref="IHierarchicalTopicMappingService{T}"/> requires tight control over the shape of the <see
  ///   cref="IHierarchicalTopicViewModel{T}"/> graph. For instance, using a generic caching decorator for the mapping might
  ///   result in the edges of the graph being expanded due to other actions reusing cached instances (e.g., for page-level
  ///   navigation). To mitigate this, the <see cref="CachedHierarchicalTopicMappingService{T}"/> handles top-level caching at
  ///   the level of the hierarchy's root.
  /// </remarks>
  public class CachedHierarchicalTopicMappingService<T> : IHierarchicalTopicMappingService<T>
    where T : class, IHierarchicalTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly ConcurrentDictionary<int, T?> _cache = new ConcurrentDictionary<int, T?>();
    private readonly IHierarchicalTopicMappingService<T> _hierarchicalTopicMappingService;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="CachedHierarchicalTopicMappingService{T}"/> based on an underlying
    ///   implementation of a <see cref="HierarchicalTopicMappingService{T}"/>.
    /// </summary>
    /// <returns>An instance of a <see cref="CachedHierarchicalTopicMappingService{T}"/>.</returns>
    public CachedHierarchicalTopicMappingService(
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) {
      _hierarchicalTopicMappingService = hierarchicalTopicMappingService;
    }

    /*==========================================================================================================================
    | GET HIERARCHICAL ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdocs />
    public Topic? GetHierarchicalRoot(Topic? currentTopic, int fromRoot = 2, string defaultRoot = "Web") =>
      _hierarchicalTopicMappingService.GetHierarchicalRoot(currentTopic, fromRoot, defaultRoot);

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdocs />
    public async Task<T?> GetRootViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceTopic == null) {
        return await Task<T?>.FromResult<T?>(null).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cache hits
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_cache.TryGetValue(sourceTopic.Id, out var dto)) {
        return await Task<T?>.FromResult<T?>(dto).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache and return new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel = await GetViewModelAsync(sourceTopic, tiers, validationDelegate).ConfigureAwait(false);
      return _cache.GetOrAdd(sourceTopic.Id, viewModel);

    }

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdocs />
    public async Task<T?> GetViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    ) =>
      await _hierarchicalTopicMappingService.GetViewModelAsync(sourceTopic, tiers, validationDelegate).ConfigureAwait(false);

  } //Class
} //Namespace