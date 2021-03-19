/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Metadata;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: OBSERVABLE TOPIC REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an abstract base class for <see cref="ITopicRepository"/> implementations which implements the event handling
  ///   logic.
  /// </summary>
  /// <remarks>
  ///   All implementations of <see cref="ITopicRepository"/> are expected to need the following logic at minimum.
  ///   Concrete implementations that are working directly with an underlying data source should prefer to instead derive from
  ///   the more opinionated <see cref="TopicRepository"/>, which provides more built-in business logic.
  /// </remarks>
  public abstract class ObservableTopicRepository : ITopicRepository {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     EventHandler<TopicLoadEventArgs>?                               _topicLoaded;
    private                     EventHandler<TopicSaveEventArgs>?                               _topicSaved;
    private                     EventHandler<TopicEventArgs>?                                   _topicDeleted;
    private                     EventHandler<TopicMoveEventArgs>?                               _topicMoved;
    private                     EventHandler<TopicRenameEventArgs>?                             _topicRenamed;

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <inheritdoc />
    public event EventHandler<TopicLoadEventArgs>? TopicLoaded {
      add => _topicLoaded += value;
      remove => _topicLoaded -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TopicSaveEventArgs>? TopicSaved {
      add => _topicSaved += value;
      remove => _topicSaved -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TopicEventArgs>? TopicDeleted {
      add => _topicDeleted += value;
      remove => _topicDeleted -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TopicMoveEventArgs>? TopicMoved {
      add => _topicMoved += value;
      remove => _topicMoved -= value;
    }

    /// <inheritdoc />
    public event EventHandler<TopicRenameEventArgs>? TopicRenamed {
      add => _topicRenamed += value;
      remove => _topicRenamed -= value;
    }

    /// <inheritdoc cref="TopicDeleted"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The DeleteEvent has been renamed to TopicDeleted", true)]
    public event EventHandler<DeleteEventArgs>? DeleteEvent;

    /// <inheritdoc cref="TopicMoved"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The MoveEvent has been renamed to TopicMoved", true)]
    public event EventHandler<DeleteEventArgs>? MoveEvent;

    /// <inheritdoc cref="TopicRenamed"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The RenameEvent has been renamed to TopicRenamed", true)]
    public event EventHandler<RenameEventArgs>? RenameEvent;

    /*==========================================================================================================================
    | ON TOPIC LOADED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Raises the <see cref="TopicLoaded"/> event.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Raising an event invokes the event handler through a delegate. For more information, see <seealso href="
    ///     https://docs.microsoft.com/en-us/dotnet/standard/events/">Handling and Raising Events</seealso>.
    ///   </para>
    ///   <para>
    ///     The <see cref="OnTopicLoaded(TopicLoadEventArgs)"/> method also allows derived classes to handle the event without
    ///     attaching a delegate. This is the preferred technique for handling the event in a derived class.
    ///   </para>
    ///   <para>
    ///     When overriding the <see cref="OnTopicLoaded(TopicLoadEventArgs)"/> method in a derived class, be sure to call the
    ///     base class's <see cref="OnTopicLoaded(TopicLoadEventArgs)"/> method so that registered delegates receive the event.
    ///   </para>
    /// </remarks>
    /// <param name="args">An instance of the <see cref="TopicLoadEventArgs"/> associated with the event.</param>
    protected virtual void OnTopicLoaded(TopicLoadEventArgs args) => _topicLoaded?.Invoke(this, args);

    /*==========================================================================================================================
    | ON TOPIC SAVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Raises the <see cref="TopicSaved"/> event.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Raising an event invokes the event handler through a delegate. For more information, see <seealso href="
    ///     https://docs.microsoft.com/en-us/dotnet/standard/events/">Handling and Raising Events</seealso>.
    ///   </para>
    ///   <para>
    ///     The <see cref="OnTopicSaved(TopicSaveEventArgs)"/> method also allows derived classes to handle the event without
    ///     attaching a delegate. This is the preferred technique for handling the event in a derived class.
    ///   </para>
    ///   <para>
    ///     When overriding the <see cref="OnTopicSaved(TopicSaveEventArgs)"/> method in a derived class, be sure to call the
    ///     base class's <see cref="OnTopicSaved(TopicSaveEventArgs)"/> method so that registered delegates receive the event.
    ///   </para>
    /// </remarks>
    /// <param name="args">An instance of the <see cref="TopicSaveEventArgs"/> associated with the event.</param>
    protected virtual void OnTopicSaved(TopicSaveEventArgs args) => _topicSaved?.Invoke(this, args);

    /*==========================================================================================================================
    | ON TOPIC DELETED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Raises the <see cref="TopicDeleted"/> event.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Raising an event invokes the event handler through a delegate. For more information, see <seealso href="
    ///     https://docs.microsoft.com/en-us/dotnet/standard/events/">Handling and Raising Events</seealso>.
    ///   </para>
    ///   <para>
    ///     The <see cref="OnTopicDeleted(TopicEventArgs)"/> method also allows derived classes to handle the event without
    ///     attaching a delegate. This is the preferred technique for handling the event in a derived class.
    ///   </para>
    ///   <para>
    ///     When overriding the <see cref="OnTopicDeleted(TopicEventArgs)"/> method in a derived class, be sure to call the
    ///     base class's <see cref="OnTopicDeleted(TopicEventArgs)"/> method so that registered delegates receive the event.
    ///   </para>
    /// </remarks>
    /// <param name="args">An instance of the <see cref="TopicEventArgs"/> associated with the event.</param>
    protected virtual void OnTopicDeleted(TopicEventArgs args) => _topicDeleted?.Invoke(this, args);

    /*==========================================================================================================================
    | ON TOPIC MOVED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Raises the <see cref="TopicMoved"/> event.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Raising an event invokes the event handler through a delegate. For more information, see <seealso href="
    ///     https://docs.microsoft.com/en-us/dotnet/standard/events/">Handling and Raising Events</seealso>.
    ///   </para>
    ///   <para>
    ///     The <see cref="OnTopicMoved(TopicMoveEventArgs)"/> method also allows derived classes to handle the event without
    ///     attaching a delegate. This is the preferred technique for handling the event in a derived class.
    ///   </para>
    ///   <para>
    ///     When overriding the <see cref="OnTopicMoved(TopicMoveEventArgs)"/> method in a derived class, be sure to call the
    ///     base class's <see cref="OnTopicMoved(TopicMoveEventArgs)"/> method so that registered delegates receive the event.
    ///   </para>
    /// </remarks>
    /// <param name="args">An instance of the <see cref="TopicMoveEventArgs"/> associated with the event.</param>
    protected virtual void OnTopicMoved(TopicMoveEventArgs args) => _topicMoved?.Invoke(this, args);

    /*==========================================================================================================================
    | ON TOPIC RENAMED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Raises the <see cref="TopicRenamed"/> event.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Raising an event invokes the event handler through a delegate. For more information, see <seealso href="
    ///     https://docs.microsoft.com/en-us/dotnet/standard/events/">Handling and Raising Events</seealso>.
    ///   </para>
    ///   <para>
    ///     The <see cref="OnTopicRenamed(TopicRenameEventArgs)"/> method also allows derived classes to handle the event
    ///     without attaching a delegate. This is the preferred technique for handling the event in a derived class.
    ///   </para>
    ///   <para>
    ///     When overriding the <see cref="OnTopicRenamed(TopicRenameEventArgs)"/> method in a derived class, be sure to call
    ///     the base class's <see cref="OnTopicRenamed(TopicRenameEventArgs)"/> method so that registered delegates receive the
    ///     event.
    ///   </para>
    /// </remarks>
    /// <param name="args">An instance of the <see cref="TopicRenameEventArgs"/> associated with the event.</param>
    protected virtual void OnTopicRenamed(TopicRenameEventArgs args) => _topicRenamed?.Invoke(this, args);

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract ContentTypeDescriptorCollection GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual Topic? Load() => Load(-1);

    /// <inheritdoc />
    public abstract Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true);

    /// <inheritdoc />
    public abstract Topic? Load(string? uniqueKey = null, Topic? referenceTopic = null, bool isRecursive = true);

    /// <inheritdoc cref="Load(Int32, Topic?, Boolean)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("This overload has been removed in preference for Load(string, Topic, Boolean).")]
    public Topic? Load(string? uniqueKey, bool isRecursive) => throw new NotImplementedException();

    /// <inheritdoc />
    public abstract Topic? Load(Topic topic, DateTime version);

    /// <inheritdoc />
    public abstract Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null);

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Refresh(Topic referenceTopic, DateTime since);

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Rollback(Topic topic, DateTime version);

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Save(Topic topic, bool isRecursive = false);

    /// <inheritdoc cref="Save(Topic, Boolean)"/>
    [ExcludeFromCodeCoverage]
    [Obsolete("The 'isDraft' argument of the Save() method has been removed.")]
    public int Save(Topic topic, bool isRecursive, bool isDraft) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Move(Topic topic, Topic target, Topic? sibling = null);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Delete(Topic topic, bool isRecursive = false);

  } //Class
} //Namespace