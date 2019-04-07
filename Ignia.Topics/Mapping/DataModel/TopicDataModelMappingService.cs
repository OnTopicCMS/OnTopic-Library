/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Generic;
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping.DataModel
{

  /*============================================================================================================================
  | CLASS: TOPIC DATA MODEL MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Implements a service that maps a <see cref="Topic"/> to a <see cref="TopicDataModel"/>.
  /// </summary>
  /// <remarks>
  ///   Compared to other concrete mapping service implementations, the <see cref="TopicDataModelMappingService"/> is quite
  ///   simple, as it doesn't need to deal with reflection. That said, it still helps centralize the logic for an otherwise
  ///   tedious and repetitive task.
  /// </remarks>
  public class TopicDataModelMappingService : ITopicDataModelMappingService {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TopicDataModelMappingService"/> with required dependencies.
    /// </summary>
    public TopicDataModelMappingService() {}

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/> and an (optional) instance of a <see cref="TopicDataModel"/>, will populate the <see
    ///   cref="TopicDataModel"/> with values from the <see cref="Topic"/>.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target <see cref="TopicDataModel"/> to map the data to.</param>
    /// <returns>
    ///   The target <see cref="TopicDataModel"/> with the properties appropriately mapped.
    /// </returns>
    public async Task<TopicDataModel> MapAsync(Topic source, TopicDataModel target = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source == null) {
        return target;
      }

      if (target == null) {
        target = new TopicDataModel();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<TopicDataModel>>();

      foreach (var childTopic in source.Children) {
        taskQueue.Add(MapAsync(childTopic));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0) {
        var mappingTask = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        taskQueue.Remove(mappingTask);
        var childTopic = await mappingTask.ConfigureAwait(false);
        target.Children.Add(childTopic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in source.Attributes) {
        target.Attributes.Add(attribute.Key, attribute.Value);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var scope in source.Relationships) {
        var relationships = new Dictionary<string, string>();
        target.Relationships.Add(scope.Name, relationships);
        foreach (var relationship in scope) {
          relationships.Add(relationship.GetUniqueKey(), relationship.Title);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

  } //Class
} //Namespace