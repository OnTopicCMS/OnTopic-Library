/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia
| Project       Website
\=============================================================================================================================*/
using System.Collections.ObjectModel;
using OnTopic.Mapping.Hierarchical;

namespace OnTopic.Models {

  /*============================================================================================================================
  | INTERFACE: HIERARCHICAL VIEW MODEL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides an interface for a view model that is hierarchical—i.e., supports populating child objects.
  /// </summary>
  /// <remarks>
  ///   Using the <see cref="IHierarchicalTopicViewModel{T}"/> interface allows the <see
  ///   cref="IHierarchicalTopicMappingService{T}"/> to remain view model agnostic, thus working with lightly-decorated POCOs.
  ///   It makes no other assumptions about the interface outside of those absolutely necessary to support a hierarchy.
  /// </remarks>
  public interface IHierarchicalTopicViewModel<T> where T : IHierarchicalTopicViewModel<T> {

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Represents a collection of child <see cref="IHierarchicalTopicViewModel{T}"/> instances.
    /// </summary>
    Collection<T> Children { get; }

  } //Class
} //Namespace