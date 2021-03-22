/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Concurrent;
using OnTopic.Internal.Diagnostics;
using OnTopic.Mapping.Annotations;

namespace OnTopic.Mapping.Internal {

  /*============================================================================================================================
  | CLASS: MAPPED TOPIC CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a collection intended to track local caching of objects mapped using the <see cref="TopicMappingService"/>.
  /// </summary>
  internal class MappedTopicCache {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ConcurrentDictionary<(int, Type), MappedTopicCacheEntry>        _cache = new();

    /*==========================================================================================================================
    | METHOD: TRY GET VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Overload of the <see cref="ConcurrentDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/> which accepts a
    ///   <paramref name="topicId"/> and <paramref name="type"/> to construct a cache key, and then outputs a <paramref name="
    ///   cacheEntry"/>, if available.
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> associated with the cache entry.</param>
    /// <param name="type">The <see cref="Type"/> that the <see cref="Topic"/> has been mapped to.</param>
    /// <param name="cacheEntry">The <see cref="MappedTopicCacheEntry"/> containing the cached instance and metadata.</param>
    /// <returns>Returns <c>true</c> if a cached entry could be found, and otherwise <c>false</c>.</returns>
    internal bool TryGetValue(int topicId, Type type, out MappedTopicCacheEntry cacheEntry) =>
      _cache.TryGetValue(GetCacheKey(topicId, type), out cacheEntry);

    /*==========================================================================================================================
    | METHOD: GET OR ADD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to add a new <see cref="MappedTopicCacheEntry"/> to the collection based on the <paramref name="topicId"/>,
    ///   <paramref name="associations"/>, and <paramref name="viewModel"/>. If one already exists, it is returned.
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> associated with the cache entry.</param>
    /// <param name="associations">The <see cref="AssociationTypes"/> used to map the current view model.</param>
    /// <param name="viewModel">The mapped view model associated with the <paramref name="topicId"/>.</param>
    internal MappedTopicCacheEntry GetOrAdd(int topicId, AssociationTypes associations, object viewModel) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(associations, nameof(associations));
      Contract.Requires(viewModel, nameof(viewModel));

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct cache entry
      \-----------------------------------------------------------------------------------------------------------------------*/
      var type                  = viewModel.GetType();
      var cacheKey              = GetCacheKey(topicId, type);
      var cacheEntry            = new MappedTopicCacheEntry() {
        MappedTopic             = viewModel,
        Associations            = associations
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Get or add entry
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId > 0 && !type.Equals(typeof(object))) {
        cacheEntry = _cache.GetOrAdd(cacheKey, cacheEntry);
        if (cacheEntry.IsInitializing) {
          cacheEntry.IsInitializing = false;
          cacheEntry.MappedTopic = viewModel;
          cacheEntry.Associations = associations;
        }
      }
      return cacheEntry;

    }

    /*==========================================================================================================================
    | METHOD: PREREGISTER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to preregister a <see cref="MappedTopicCacheEntry"/> for a <see cref="Topic"/> that is in the process of
    ///   being mapped to <paramref name="type"/>.
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> associated with the cache entry.</param>
    /// <param name="type">The <see cref="Type"/> that the <see cref="Topic"/> is being mapped to.</param>
    internal MappedTopicCacheEntry Preregister(int topicId, Type type) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicId, nameof(topicId));
      Contract.Requires(type, nameof(type));

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct cache entry
      \-----------------------------------------------------------------------------------------------------------------------*/
      var cacheKey = GetCacheKey(topicId, type);
      var cacheEntry = new MappedTopicCacheEntry() {
        IsInitializing = true
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Get or add entry
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId > 0 && !type.Equals(typeof(object))) {
        var existingCacheEntry  = _cache.GetOrAdd(cacheKey, cacheEntry);
        if (existingCacheEntry != cacheEntry) {
          throw new TopicMappingException(
            $"An attempt has been made to map '{topicId}' to a {type.Name} has resulted in a circular reference during the " +
            $"construction of the {type.Name} instance. This is not allowed. Circular must be be mapped as properties, not " +
            $"as constructor parameters, so that cached entries can be returned."
          );
        }
      }
      return cacheEntry;

    }

    /*==========================================================================================================================
    | METHOD: GET CACHE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new cache key from the supplied <paramref name="topicId"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="topicId">The <see cref="Topic.Id"/> associated with the source topic.</param>
    /// <param name="type">The <see cref="Type"/> that the <see cref="Topic"/> has been mapped to.</param>
    /// <returns></returns>
    internal static (int, Type) GetCacheKey(int topicId, Type type) => new(topicId, type);

  } //Class
} //Namespace