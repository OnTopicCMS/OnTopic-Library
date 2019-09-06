/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Concurrent;
using Ignia.Topics.Internal.Diagnostics;
using System.Threading.Tasks;

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
    private readonly ConcurrentDictionary<(int, Type, Relationships), object> _cache =
      new ConcurrentDictionary<(int, Type, Relationships), object>();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="CachedTopicMappingService"/> with required dependencies.
    /// </summary>
    public CachedTopicMappingService(ITopicMappingService topicMappingService) {
      Contract.Requires<ArgumentNullException>(topicMappingService != null, "An instance of topicMappingService is required.");
      _topicMappingService = topicMappingService;
    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the underlying implementation is using reflection to determine the target View Models, the return type is
    ///     <see cref="Object"/>. These results may need to be cast to a specific type, depending on the context. That said,
    ///     strongly-typed views should be able to cast the object to the appropriate View Model type. If the type of the View
    ///     Model is known upfront, and it is imperative that it be strongly-typed, then prefer <see
    ///     cref="MapAsync{T}(Topic, Relationships)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed by the underlying implementation, it must implement a
    ///     default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    public async Task<object> MapAsync(Topic topic, Relationships relationships = Relationships.All) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, (Type)null, relationships);
      if(_cache.TryGetValue(cacheKey, out var viewModel)) {
        return viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(
        topic.ContentType,
        await _topicMappingService.MapAsync(topic, relationships).ConfigureAwait(false),
        cacheKey
      );

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and a generic type, will instantiate a new instance of the generic type and populate it according to the
    ///   rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the target object is being dynamically constructed by the underlying implementation, it must implement a
    ///     default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>
    ///   An instance of the requested View Model <typeparamref name="T"/> with properties appropriately mapped.
    /// </returns>
    public async Task<T> MapAsync<T>(Topic topic, Relationships relationships = Relationships.All) where T : class, new() {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, typeof(T), relationships);
      if (_cache.TryGetValue(cacheKey, out var viewModel)) {
        return (T)viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(
        topic.ContentType,
        await _topicMappingService.MapAsync<T>(topic, relationships).ConfigureAwait(false),
        cacheKey
      ) as T;

    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    public async Task<object> MapAsync(Topic topic, object target, Relationships relationships = Relationships.All) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = (topic.Id, target.GetType(), relationships);
      if (_cache.TryGetValue(cacheKey, out var viewModel)) {
        return viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(
        topic.ContentType,
        await _topicMappingService.MapAsync(topic, relationships).ConfigureAwait(false),
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
    private object CacheViewModel(string contentType, object viewModel, (int, Type, Relationships) cacheKey) {
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
