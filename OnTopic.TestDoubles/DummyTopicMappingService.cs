﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Mapping;
using OnTopic.Mapping.Annotations;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace OnTopic.TestDoubles {

  /*============================================================================================================================
  | DUMMY: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class DummyTopicMappingService : ITopicMappingService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DummyTopicMappingService"/> with required dependencies.
    /// </summary>
    public DummyTopicMappingService() {
    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    [return: NotNullIfNotNull("topic")]
    public async Task<object?> MapAsync(Topic? topic, AssociationTypes associations = AssociationTypes.All)
      => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> MapAsync<T>(Topic? topic, AssociationTypes associations = AssociationTypes.All) where T : class
      => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<object?> MapAsync(Topic? topic, object target, AssociationTypes associations = AssociationTypes.All)
      => throw new NotImplementedException();

  } //Class
} //Namespace

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously