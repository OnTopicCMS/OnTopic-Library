/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using Ignia.Topics.Mapping;
using Ignia.Topics.Mapping.Annotations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable IDE0060 // Remove unused parameter

namespace Ignia.Topics.TestFixtures {

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

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning restore IDE0060 // Remove unused parameter