/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using OnTopic.Metadata;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  public abstract class ObservableTopicRepository : ITopicRepository {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     EventHandler<DeleteEventArgs>?  _deleteEvent;
    private                     EventHandler<MoveEventArgs>?    _moveEvent;
    private                     EventHandler<RenameEventArgs>?  _renameEvent;

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/

    /// <inheritdoc />
    public virtual event EventHandler<DeleteEventArgs>? DeleteEvent {
      add => _deleteEvent += value;
      remove => _deleteEvent -= value;
    }

    /// <inheritdoc />
    public virtual event EventHandler<MoveEventArgs>? MoveEvent {
      add => _moveEvent += value;
      remove => _moveEvent -= value;
    }

    /// <inheritdoc />
    public virtual event EventHandler<RenameEventArgs>? RenameEvent {
      add => _renameEvent += value;
      remove => _renameEvent -= value;
    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract ContentTypeDescriptorCollection GetContentTypeDescriptors();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true);

    /// <inheritdoc />
    public abstract Topic? Load(string? uniqueKey = null, Topic? referenceTopic = null, bool isRecursive = true);

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

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Move(Topic topic, Topic target, Topic? sibling = null);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract void Delete(Topic topic, bool isRecursive);

  } //Class
} //Namespace