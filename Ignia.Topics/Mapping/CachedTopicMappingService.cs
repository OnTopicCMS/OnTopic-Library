/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Mapping.Annotations;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: CACHED TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a caching interface for an underlying <see cref="ITopicMappingService"/>, by caching entire mapped object
  ///   graphs based on the <see cref="Topic.Id"/> of the root topic.
  /// </summary>
  public class CachedTopicMappingService : ITopicMappingService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicMappingService            _topicMappingService;

    /*==========================================================================================================================
    | ESTABLISH CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly ConcurrentDictionary<(int, Type?, Relationships), object> _cache =
      new ConcurrentDictionary<(int, Type?, Relationships), object>();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="CachedTopicMappingService"/> with required dependencies.
    /// </summary>
    public CachedTopicMappingService(ITopicMappingService topicMappingService) {
      Contract.Requires(topicMappingService, "An instance of topicMappingService is required.");
      _topicMappingService = topicMappingService;
    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<object?> MapAsync(Topic? topic, Relationships relationships = Relationships.All) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, (Type?)null, relationships);
      if(_cache.TryGetValue(cacheKey, out object? viewModel)) {
        return viewModel;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process result
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel = await _topicMappingService.MapAsync(topic, relationships).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return (cached) result
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (viewModel == null) {
        return null;
      }
      return CacheViewModel(
        topic.ContentType,
        viewModel,
        cacheKey
      );

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> MapAsync<T>(Topic? topic, Relationships relationships = Relationships.All) where T : class, new() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, typeof(T), relationships);
      if (_cache.TryGetValue(cacheKey, out object? viewModel)) {
        return (T)viewModel;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process result
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel = await _topicMappingService.MapAsync<T>(topic, relationships).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return (cached) result
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (viewModel == null) {
        return null;
      }
      return CacheViewModel(
        topic.ContentType,
        viewModel,
        cacheKey
      ) as T;

    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<object?> MapAsync(Topic? topic, object target, Relationships relationships = Relationships.All) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, target.GetType(), relationships);
      if (_cache.TryGetValue(cacheKey, out object? viewModel)) {
        return viewModel;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process result
      \-----------------------------------------------------------------------------------------------------------------------*/
      viewModel = await _topicMappingService.MapAsync(topic, relationships).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return (cached) result
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (viewModel == null) {
        return null;
      }
      return CacheViewModel(
        topic.ContentType,
        viewModel,
        cacheKey
      );

    }

    /*==========================================================================================================================
    | METHOD: CACHE VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a view model, determines if it is appropriate to cache and, if so, adds it to the cache. Regardless, returns the
    ///   view model back to the consumer.
    /// </summary>
    /// <remarks>
    ///   The internal will potentially add two entries to the cache for every view model.
    ///   <list type="number">
    ///     <item>
    ///       The first will be bound to the <see cref="Topic.Id"/>, view model <see cref="Type"/>, and the <see
    ///       cref="Relationships"/> mapped.
    ///     </item>
    ///     <item>
    ///       The second will assume a null <see cref="Type"/>, and can be used for scenarios where the <see cref="Type"/> is
    ///       not known—and, thus, assumed to be the default mapping.
    ///     </item>
    ///   </list>
    ///   In all cases, the <see cref="Topic.Id"/> must be greater than zero, to ensure that it's a saved entity (otherwise,
    ///   multiple distinct entities will have the default <see cref="Topic.Id"/> of <c>-1</c>). In addition, the following
    ///   conditions apply, respectively, to each of the caches:
    ///   <list type="number">
    ///     <item>
    ///       The first must have a view model type that is not an <see cref="Object"/>, since it is meant to map to a specific
    ///       view model type.
    ///     </item>
    ///     <item>
    ///       The second will assume that the view model type of the naming convention <c>{ContentType}TopicViewModel</c>—i.e.,
    ///       it is the default implementation for the given view model type, and not a specially cast version such as e.g.,
    ///       <c>NavigationTopicViewModel</c> or <c>TopicViewModel</c>.
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <param name="contentType">The content type associated with the associated <see cref="Topic"/>.</param>
    /// <param name="viewModel">The view model object to cache; can be any POCO object.</param>
    /// <param name="cacheKey">A Tuple{T1, T2, T3} representing the cache key.</param>
    /// <returns>The <paramref name="viewModel"/>.</returns>
    private object? CacheViewModel(string contentType, object viewModel, (int, Type?, Relationships) cacheKey) {
      if (cacheKey.Item1 > 0 && cacheKey.Item2 != null && !viewModel.GetType().Equals(typeof(object))) {
        _cache.TryAdd(cacheKey, viewModel);
      }
      if (cacheKey.Item2 != null) {
        cacheKey = (cacheKey.Item1, null, cacheKey.Item3);
      }
      if (cacheKey.Item1 > 0 && viewModel.GetType().Name == $"{contentType}TopicViewModel") {
        _cache.TryAdd(cacheKey, viewModel);
      }
      return viewModel;
    }

  } //Class
} //Namespace