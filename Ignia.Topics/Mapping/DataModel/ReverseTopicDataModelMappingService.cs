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
using Ignia.Topics.Repositories;
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping.DataModel {

  /*============================================================================================================================
  | CLASS: REVERSE TOPIC DATA MODEL MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an interface for a service that maps a <see cref="TopicDataModel"/> to a <see cref="Topic"/>. This is the
  ///   inverse of the <see cref="ITopicDataModelMappingService"/>.
  /// </summary>
  /// <remarks>
  ///   Compared to other concrete mapping service implementations, the <see cref="ReverseTopicDataModelMappingService"/> is
  ///   quite simple, as it doesn't need to deal with reflection. That said, it still helps centralize the logic for an otherwise
  ///   tedious and repetitive task.
  /// </remarks>
  public class ReverseTopicDataModelMappingService : IReverseTopicDataModelMappingService {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="ReverseTopicDataModelMappingService"/> with required dependencies.
    /// </summary>
    public ReverseTopicDataModelMappingService(
      ITopicRepository topicRepository
    ) {
      _topicRepository = topicRepository;
    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="TopicDataModel"/> and an (optional) instance of a <see cref="Topic"/>, will populate the <see
    ///   cref="Topic"/> with values from the <see cref="TopicDataModel"/>.
    /// </summary>
    /// <param name="source">The <see cref="TopicDataModel"/> to derive the data from.</param>
    /// <param name="target">The target <see cref="Topic"/> entity to map the data to.</param>
    /// <returns>
    ///   The target <see cref="Topic"/> with the properties appropriately mapped.
    /// </returns>
    public async Task<Topic> MapAsync(TopicDataModel source, Topic target = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source == null) {
        return target;
      }

      if (target == null) {
        target = _topicRepository.Load(source.UniqueKey);
      }
      if (target == null) {
        target = TopicFactory.Create(source.Key, source.ContentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<Topic>>();

      foreach (var childTopicDataModel in source.Children) {
        var childTopic = (Topic)null;
        if (target.Children.Contains(childTopicDataModel.Key)) {
          childTopic = target.Children[childTopicDataModel.Key];
        }
        taskQueue.Add(MapAsync(childTopicDataModel, childTopic));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0) {
        var mappingTask = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        taskQueue.Remove(mappingTask);
        var childTopic = await mappingTask.ConfigureAwait(false);
        childTopic.Parent = target;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in source.Attributes) {
        target.Attributes.SetValue(attribute.Key, attribute.Value);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      target.Relationships.Clear();
      foreach (var scope in source.Relationships) {
        foreach (var relationship in scope.Value) {
          var relatedTopic = _topicRepository.Load(relationship.Key);
          target.Relationships.SetTopic(scope.Key, relatedTopic);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

  } //Class
} //Namespace