/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Threading.Tasks;
using OnTopic.Models;

namespace OnTopic.Mapping.Hierarchical {

  /*============================================================================================================================
  | INTERFACE: HIERARCHICAL TOPIC MAPPING SERVICE
  \===========================================================================================================================*/
  /// <summary>
  ///   Provides an interface for a service that maps a limited-hierarchy of topics to a generic class representing the core
  ///   properties associated with a navigation item.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Ideally, this functionality would be baked directly into the <see cref="ITopicMappingService"/> implementations, but
  ///     this introduces a number of technical issues that make this unfeasible for now. Instead, the <see
  ///     cref="IHierarchicalTopicMappingService{T}"/> handles this functionality for special cases, such as navigation, where
  ///     e.g. the full recursion of the <see cref="ITopicMappingService"/> is not preferrable for functional or performance
  ///     reasons.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="HierarchicalTopicMappingService{T}"/> does not assume that a
  ///     particular view model will be used, and instead accepts a generic argument for any view model that implements the
  ///     interface <see cref="IHierarchicalTopicViewModel{T}"/>.
  ///   </para>
  /// </remarks>
  /// <typeparam name="T">A view model implementing the <see cref="IHierarchicalTopicViewModel{T}"/> interface.</typeparam>
  public interface IHierarchicalTopicMappingService<T> where T : class, IHierarchicalTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | GET HIERARCHICAL ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="currentTopic"/>, will crawl up the current tree and retrieve the topic that is <paramref
    ///   name="fromRoot"/> tiers down from the root of the topic graph.
    /// </summary>
    /// <remarks>
    ///   Often, a <see cref="IHierarchicalTopicMappingService{T}"/> will need a reference to a topic at a certain level, which
    ///   represents the root of the the site. For instance, if the hierarchy represents a site's navigation, and the navigation
    ///   starts at <c>Root:Web</c>, then the navigation is one level from the root (i.e., <paramref name="fromRoot"/>=1). This,
    ///   however, should not be hard-coded in case a site has multiple roots. For instance, if a page is under <c>Root:Library
    ///   </c> then <i>that</i> should be the navigation root. This method provides support for these scenarios.
    /// </remarks>
    /// <param name="currentTopic">The <see cref="Topic"/> to start from.</param>
    /// <param name="fromRoot">The distance that the navigation root should be from the root of the topic graph.</param>
    /// <param name="defaultRoot">If a root cannot be identified, the default root that should be returned.</param>
    Topic? GetHierarchicalRoot(Topic? currentTopic, int fromRoot = 2, string defaultRoot = "Web");

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="IHierarchicalTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <remarks>
    ///   In the out-of-the-box implementation, <see cref="GetRootViewModelAsync(Topic, Int32, Func{Topic, Boolean})"/> and <see
    ///   cref="GetViewModelAsync(Topic, Int32, Func{Topic, Boolean})"/> provide the same functionality. It is recommended that
    ///   actions call <see cref="GetRootViewModelAsync(Topic, Int32, Func{Topic, Boolean})"/>, however, as it allows
    ///   implementers the flexibility to differentiate between the root view model (which the client application will be
    ///   binding to) and any child view models (which the client application may optionally iterate over).
    /// </remarks>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    /// <param name="validationDelegate">
    ///   A delegate for validating whether a <see cref="Topic"/> (and it's descendants) should be included in the returned
    ///   hierarchy.
    /// </param>
    Task<T?> GetRootViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    );

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="IHierarchicalTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    /// <param name="validationDelegate">
    ///   A delegate for validating whether a <see cref="Topic"/> (and it's descendants) should be included in the returned
    ///   hierarchy.
    /// </param>
    Task<T?> GetViewModelAsync(
      Topic? sourceTopic,
      int tiers = 1,
      Func<Topic, bool>? validationDelegate = null
    );


  } //Interface
} //Class