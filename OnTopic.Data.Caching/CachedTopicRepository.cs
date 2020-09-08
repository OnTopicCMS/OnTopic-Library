/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
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

  public class CachedTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRepository                _dataProvider;
    private readonly            Topic                           _cache;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the CachedTopicRepository with a dependency on an underlying ITopicRepository in order
    ///   to provide necessary data access.
    /// </summary>
    /// <param name="dataProvider">A concrete instance of an ITopicRepository, which will be used for data access.</param>
    /// <returns>A new instance of the CachedTopicRepository.</returns>
    public CachedTopicRepository(ITopicRepository dataProvider) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(dataProvider, "A concrete implementation of an ITopicRepository is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      _dataProvider = dataProvider;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure topics are loaded
      \-----------------------------------------------------------------------------------------------------------------------*/
      var rootTopic = _dataProvider.Load();

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
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() => _dataProvider.GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, bool isRecursive = true) {

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
    public override Topic? Load(string? topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup by TopicKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicKey != null && !topicKey.Length.Equals(0)) {
        return _cache.GetByUniqueKey(topicKey);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return entire cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _cache;

    }

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Return appropriate topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _dataProvider.Load(topicId, version);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) =>
      _dataProvider.Save(topic, isRecursive, isDraft);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Move(Topic topic, Topic target, Topic? sibling) => _dataProvider.Move(topic, target, sibling);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive = true) => _dataProvider.Delete(topic, isRecursive);

  } //Class
} //Namespace