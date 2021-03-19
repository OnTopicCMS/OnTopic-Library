/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Data.Caching {

  /*============================================================================================================================
  | CLASS: CACHED TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in memory.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="OnTopic.Repositories.ITopicRepository"/> class, which provides a wrapper
  ///   for an actual data access class.
  /// </remarks>

  public class CachedTopicRepository : TopicRepositoryDecorator {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            Topic                           _cache;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="CachedTopicRepository"/> with a dependency on an underlying <see cref="
    ///   ITopicRepository"/> in order to provide necessary data access.
    /// </summary>
    /// <param name="topicRepository">
    ///   A concrete instance of an <see cref="ITopicRepository"/>, which will be used for data access.
    /// </param>
    /// <returns>A new instance of a <see cref="CachedTopicRepository"/>.</returns>
    public CachedTopicRepository(ITopicRepository topicRepository) : base(topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = TopicRepository.Load();

      Contract.Assume(
        rootTopic,
        $"The topic graph could not be successfully loaded from the {nameof(ITopicRepository)} instance. The " +
        $"{nameof(CachedTopicRepository)} is unable to establish the cache."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      _cache = rootTopic;

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle request for entire tree
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId < 0) {
        return _cache;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recursive search
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache.FindFirst(t => t.Id.Equals(topicId));

    }

    /// <inheritdoc />
    public override Topic? Load(string uniqueKey, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(uniqueKey)) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache.GetByUniqueKey(uniqueKey);

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.UtcNow, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Return appropriate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return TopicRepository.Load(topicId, version, referenceTopic?? _cache);

    }

  } //Class
} //Namespace