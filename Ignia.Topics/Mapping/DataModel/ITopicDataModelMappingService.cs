/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Ignia.Topics.Models;

namespace Ignia.Topics.Mapping.DataModel {

  /*==============================================================================================================================
  | INTERFACE: TOPIC DATA MODEL MAPPING SERVICE
  \=============================================================================================================================*/
  /// <summary>
  ///   Provides an interface for a service that maps a <see cref="Topic"/> to a <see cref="TopicDataModel"/>.
  /// </summary>
  public interface ITopicDataModelMappingService {

    /*==========================================================================================================================
    | MAP (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="source"/>, maps a <paramref name="target"/>.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="target">The <see cref="TopicDataModel"/> to set the values on.</param>
    Task<TopicDataModel> MapAsync(Topic source, TopicDataModel target = null);

    } //Interface
  } //Class