/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping.DataModel {

  /*==============================================================================================================================
  | INTERFACE: REVERSE TOPIC DATA MODEL MAPPING SERVICE
  \=============================================================================================================================*/
  /// <summary>
  ///   Provides an interface for a service that maps a <see cref="TopicDataModel"/> to a <see cref="Topic"/>. This is the
  ///   inverse of the <see cref="ITopicDataModelMappingService"/>.
  /// </summary>
  public interface IReverseTopicDataModelMappingService {

    /*==========================================================================================================================
    | MAP (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="source"/>, maps a <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The <see cref="TopicDataModel"/> to pull the values from.</param>
    /// <param name="target">The <see cref="Topic"/> to set the values on.</param>
    Task<Topic> MapAsync(TopicDataModel source, Topic target = null);

    } //Interface
  } //Class