/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ignia.Topics.Mapping;
using Ignia.Topics.Models;
using Ignia.Topics.AspNetCore.Mvc.Models;
using Ignia.Topics.Mapping.Hierarchical;

namespace Ignia.Topics.AspNetCore.Mvc.Components {

  /*============================================================================================================================
  | CLASS: NAVIGATION TOPIC VIEW COMPONENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a foundation for custom <see cref="ViewComponent"/> implementations that are based on the location of the
  ///   current <see cref="Topic"/> and require access to the <see cref="IHierarchicalTopicMappingService{T}"/> to populate the
  ///   model.
  /// </summary>
  /// <remarks>
  ///   This class is intended to provide a foundation for concrete implementations. It is not a fully formed implementation
  ///   itself. As a result, it is marked as <c>abstract</c>.
  /// </remarks>
  public abstract class NavigationTopicViewComponentBase<T> : ViewComponent where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ITopicRoutingService            _topicRoutingService            = null;
    private                     Topic                           _currentTopic                   = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="NavigationTopicViewComponentBase{T}"/> with necessary dependencies.
    /// </summary>
    /// <returns>A topic <see cref="NavigationTopicViewComponentBase{T}"/>.</returns>
    protected NavigationTopicViewComponentBase(
      ITopicRoutingService topicRoutingService,
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) {
      _topicRoutingService = topicRoutingService;
      HierarchicalTopicMappingService = hierarchicalTopicMappingService;
    }

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="IHierarchicalTopicMappingService{T}"/> in order to allow trees of topics to
    ///   be mapped.
    /// </summary>
    /// <returns>
    ///   The <see cref="IHierarchicalTopicMappingService{T}"/> associated with the <see cref="TopicViewComponentBase{T}"/>.
    /// </returns>
    protected IHierarchicalTopicMappingService<T> HierarchicalTopicMappingService { get; }

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    protected Topic CurrentTopic {
      get {
        if (_currentTopic == null) {
          _currentTopic = _topicRoutingService.GetCurrentTopic();
        }
        return _currentTopic;
      }
    }

  } // Class

} // Namespace