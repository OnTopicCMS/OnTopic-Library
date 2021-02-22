/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using Microsoft.AspNetCore.Mvc;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.AspNetCore.Mvc.Components {

  /*============================================================================================================================
  | CLASS: NAVIGATION TOPIC VIEW COMPONENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a foundation for custom <see cref="ViewComponent"/> implementations that are based on the location of the
  ///   current <see cref="Topic"/> and require access to the <see cref="IHierarchicalTopicMappingService{T}"/> to populate the
  ///   model.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   This class is intended to provide a foundation for concrete implementations. It is not a fully formed implementation
  ///   itself. As a result, it is marked as <c>abstract</c>.
  ///   </para>
  ///   <para>
  ///     While the <see cref="NavigationTopicViewComponentBase{T}"/> only requires that the <typeparamref name="T"/> implement
  ///     <see cref="IHierarchicalTopicViewModel{T}"/>, views will require additional properties. These can be determined on a
  ///     per-case basis, as required by the implementation. Implementaters, however, should consider implementing the <see cref
  ///     ="INavigationTopicViewModel{T}"/> interface, which provides the standard properties that most views will likely need,
  ///     as well as a <see cref="INavigationTopicViewModel{T}.IsSelected(String)"/> method for determining if the navigation
  ///     item is currently selected.
  ///   </para>
  /// </remarks>
  public abstract class NavigationTopicViewComponentBase<T> : ViewComponent where T : class, IHierarchicalTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     Topic?                          _currentTopic;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="NavigationTopicViewComponentBase{T}"/> with necessary dependencies.
    /// </summary>
    /// <returns>A topic <see cref="NavigationTopicViewComponentBase{T}"/>.</returns>
    protected NavigationTopicViewComponentBase(
      ITopicRepository topicRepository,
      IHierarchicalTopicMappingService<T> hierarchicalTopicMappingService
    ) {
      TopicRepository = topicRepository;
      HierarchicalTopicMappingService = hierarchicalTopicMappingService;
    }

    /*==========================================================================================================================
    | TOPIC REPOSITORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="ITopicRepository"/> in order to allow the current topic to be identified based
    ///   on the route data.
    /// </summary>
    /// <returns>
    ///   The <see cref="ITopicRepository"/> associated with the <see cref="NavigationTopicViewComponentBase{T}"/>.
    /// </returns>
    protected ITopicRepository TopicRepository { get; }

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the <see cref="IHierarchicalTopicMappingService{T}"/> in order to allow trees of topics to
    ///   be mapped.
    /// </summary>
    /// <returns>
    ///   The <see cref="IHierarchicalTopicMappingService{T}"/> associated with the <see cref="
    ///   NavigationTopicViewComponentBase{T}"/>.
    /// </returns>
    protected IHierarchicalTopicMappingService<T> HierarchicalTopicMappingService { get; }

    /*==========================================================================================================================
    | CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the current topic associated with the request.
    /// </summary>
    /// <returns>The Topic associated with the current request.</returns>
    protected Topic? CurrentTopic {
      get {
        if (_currentTopic is null) {
          _currentTopic = TopicRepository.Load(RouteData);
        }
        return _currentTopic;
      }
    }

  } //Class
} //Namespace