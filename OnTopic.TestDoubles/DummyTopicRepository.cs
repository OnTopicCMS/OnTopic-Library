/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using OnTopic.Metadata;
using OnTopic.Repositories;

namespace OnTopic.TestDoubles {

  /*============================================================================================================================
  | DUMMY: TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a basic, non-functional version of a <see cref="ITopicRepository"/> which satisfies the interface requirements,
  ///   but is not intended to be called.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class DummyTopicRepository : ObservableTopicRepository {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the <see cref="DummyTopicRepository"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="DummyTopicRepository"/>.</returns>
    public DummyTopicRepository() : base() { }

    /*==========================================================================================================================
    | METHOD: GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() => new();

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) => null;

    /// <inheritdoc />
    public override Topic? Load(string uniqueKey, Topic? referenceTopic = null, bool isRecursive = true) => null;

    /// <inheritdoc />
    public override Topic? Load(Topic? topic, DateTime version) => throw new NotImplementedException();

    /// <inheritdoc />
    public override Topic? Load(int topicId, DateTime version, Topic? referenceTopic = null) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Rollback(Topic topic, DateTime version) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Refresh(Topic referenceTopic, DateTime since) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Save(Topic topic, bool isRecursive = false) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Move(Topic topic, Topic target, Topic? sibling = null) => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive = false) => throw new NotImplementedException();

  } //Class
} //Namespace