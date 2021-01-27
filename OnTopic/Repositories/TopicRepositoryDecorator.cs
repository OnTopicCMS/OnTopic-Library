/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY DECORATOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for establishing a decorator of an <see cref="ITopicRepository"/>.
  /// </summary>
  /// <remarks>
  ///   Decorators allow implementors to operate off of an underlying version of an <see cref="ITopicRepository"/>, while
  ///   intercepting calls in order to extend their functionality. They do this by establishing passthrough members to each of
  ///   the underlying members. The <see cref="TopicRepositoryDecorator"/> offers a base class for implementing decorators by
  ///   accepting a <see cref="ITopicRepository"/> via its constructor, and then wiring up each of the members as a passthrough.
  ///   It also subscribes to the underlying <see cref="ITopicRepository"/>'s events so that they will correctly bubble up
  ///   through the decorator. This way, derived decorators must only implement the specific methods they wish to override, and
  ///   can leave everything else as is.
  /// </remarks>
  public abstract class TopicRepositoryDecorator : ObservableTopicRepository {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="TopicRepositoryDecorator"/> with a dependency on an underlying <see cref
    ///   ="ITopicRepository"/> in order to provide necessary data access.
    /// </summary>
    /// <param name="topicRepository">
    ///   A concrete instance of an <see cref="ITopicRepository"/>, which will be used for data access.
    /// </param>
    /// <returns>A new instance of the <see cref="TopicRepositoryDecorator"/>.</returns>
    protected TopicRepositoryDecorator(ITopicRepository topicRepository) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, "A concrete implementation of an ITopicRepository is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set values locally
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository = topicRepository;

      /*------------------------------------------------------------------------------------------------------------------------
      | Subscribe to underlying events
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository.TopicDeleted += (object sender, TopicEventArgs args)                       => OnTopicDeleted(args);
      TopicRepository.TopicMoved   += (object sender, TopicMoveEventArgs args)                   => OnTopicMoved(args);
      TopicRepository.TopicRenamed += (object sender, TopicRenameEventArgs args)                 => OnTopicRenamed(args);

    }

    /*==========================================================================================================================
    | DATA PROVIDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to the underlying <see cref="ITopicRepository"/> that this decorates.
    /// </summary>
    protected ITopicRepository TopicRepository { get; set; }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() => TopicRepository.GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) =>
      TopicRepository.Load(topicId, referenceTopic, isRecursive);

    /// <inheritdoc />
    public override Topic? Load(string? uniqueKey = null, Topic? referenceTopic = null, bool isRecursive = true) =>
      TopicRepository.Load(uniqueKey, referenceTopic, isRecursive);

    /// <inheritdoc />
    public override Topic? Load(Topic topic, DateTime version)
      => TopicRepository.Load(topic, version);

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) =>
      TopicRepository.Load(topicId, version, referenceTopic);

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Refresh(Topic referenceTopic, DateTime since) => TopicRepository.Refresh(referenceTopic, since);

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Rollback(Topic topic, DateTime version) => TopicRepository.Rollback(topic, version);

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Save(Topic topic, bool isRecursive = false) => TopicRepository.Save(topic, isRecursive);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Move(Topic topic, Topic target, Topic? sibling = null) => TopicRepository.Move(topic, target, sibling);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive) => TopicRepository.Delete(topic, isRecursive);

  } //Class
} //Namespace