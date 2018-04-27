/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Collections;
using Ignia.Topics.Repositories;

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
    private readonly            ITopicMappingService            _topicMappingService            = null;

    /*==========================================================================================================================
    | ESTABLISH CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly ConcurrentDictionary<Tuple<int, Type, Relationships>, object> _cache =
      new ConcurrentDictionary<Tuple<int, Type, Relationships>, object>();

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
    ///     cref="Map{T}(Topic, Relationships)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed by the underlying implementation, it must implement a
    ///     default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    public object Map(Topic topic, Relationships relationships = Relationships.All) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = GetCacheKey(topic.Id, null, relationships);
      if(_cache.TryGetValue(cacheKey, out var viewModel)) {
        return viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(topic.ContentType, _topicMappingService.Map(topic, relationships), cacheKey);

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
    public T Map<T>(Topic topic, Relationships relationships = Relationships.All) where T : class, new() {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = GetCacheKey(topic.Id, typeof(T), relationships);
      if (_cache.TryGetValue(cacheKey, out var viewModel)) {
        return (T)viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(topic.ContentType, _topicMappingService.Map<T>(topic, relationships), cacheKey) as T;

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
    public object Map(Topic topic, object target, Relationships relationships = Relationships.All) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = GetCacheKey(topic.Id, target.GetType(), relationships);
      if (_cache.TryGetValue(cacheKey, out var viewModel)) {
        return viewModel;
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached result
      \---------------------------------------------------------------------------------------------------------------------*/
      return CacheViewModel(topic.ContentType, _topicMappingService.Map(topic, relationships), cacheKey);

    }

    /*==========================================================================================================================
    | METHOD: CACHE VIEW MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a view model, determines if it is appropriate to cache and, if so, adds it to the cache. Regardless, returns the
    ///   view model back to the consumer.
    /// </summary>
    /// <param name="contentType">The content type associated with the associated <see cref="Topic"/>.</param>
    /// <param name="viewModel">The view model object to cache; can be any POCO object.</param>
    /// <param name="cacheKey">A Tuple{T1, T2, T3} representing the cache key.</param>
    /// <returns>The <paramref name="viewModel"/>.</returns>
    private object CacheViewModel(string contentType, object viewModel, Tuple<int, Type, Relationships> cacheKey) {
      if (cacheKey.Item1 > 0 && cacheKey.Item2 != null && !viewModel.GetType().Equals(typeof(object))) {
        _cache.TryAdd(cacheKey, viewModel);
      }
      if (cacheKey.Item1 > 0 && viewModel.GetType().Name.Equals(contentType + "TopicViewModel")) {
        _cache.TryAdd(cacheKey, viewModel);
      }
      return viewModel;
    }

    /*==========================================================================================================================
    | METHOD: GET CACHE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic.Id"/>, <see cref="Type"/>, and <see cref="Relationships"/> reference, produces a cache key of
    ///   type <see cref="Tuple{T1, T2, T3}"/>
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> of the entity to derive the data from.</param>
    /// <param name="type">The type of the target object that the <see cref="Topic"/> will be mapped to.</param>
    /// <param name="relationships">The relationships the mapping will follow, if any.</param>
    /// <returns>A <see cref="Tuple{Int32, Type, Relationships}"/> representing the unique cache key.</returns>
    private static Tuple<int, Type, Relationships> GetCacheKey(int topicId, Type type, Relationships relationships) {
      return new Tuple<int, Type, Relationships>(topicId, type, relationships);
    }

  } //Class
} //Namespace
