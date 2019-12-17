/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Ignia.Topics.Internal.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignia.Topics.Internal.Mapping;
using Ignia.Topics.Internal.Reflection;
using Ignia.Topics.Mapping.Annotations;
using Ignia.Topics.Repositories;
using Ignia.Topics.Models;
using System.Diagnostics.CodeAnalysis;
using Ignia.Topics.Attributes;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | DUMMY: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  public class DummyTopicMappingService : ITopicMappingService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="DummmyTopicMappingService"/> with required dependencies.
    /// </summary>
    public DummyTopicMappingService() {
    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    [return: NotNullIfNotNull("topic")]
    public async Task<object?> MapAsync(Topic? topic, Relationships relationships = Relationships.All)
      => throw new NotImplementedException();

    /// <inheritdoc/>
    private async Task<object?> MapAsync(Topic? topic, Relationships relationships, ConcurrentDictionary<int, object> cache)
      => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<T?> MapAsync<T>(Topic? topic, Relationships relationships = Relationships.All) where T : class, new()
      => throw new NotImplementedException();

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<object?> MapAsync(Topic? topic, object target, Relationships relationships = Relationships.All)
      => throw new NotImplementedException();

  } //Class
} //Namespace