/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Repositories;
using System;

namespace Ignia.Topics.TestDoubles {

  /*============================================================================================================================
  | DUMMY: TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a basic, non-functional version of a <see cref="ITopicRepository"/> which satisfies the interface requirements,
  ///   but is not intended to be called.
  /// </summary>
  public class DummyTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="DummyTopicRepository"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="DummyTopicRepository"/>.</returns>
    public DummyTopicRepository() : base() {}

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override Topic? Load(int topicId, bool isRecursive = true) => null;

    /// <inheritdoc/>
    public override Topic? Load(string? topicKey = null, bool isRecursive = true) => null;

    /// <inheritdoc/>
    public override Topic? Load(int topicId, DateTime version) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Move(Topic topic, Topic target, Topic? sibling) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Delete(Topic topic, bool isRecursive = false) => throw new NotImplementedException();

  } //Class
} //Namespace