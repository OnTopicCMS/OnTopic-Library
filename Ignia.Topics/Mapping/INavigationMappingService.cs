/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Threading.Tasks;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Mapping {

  /// <summary>
  ///   Provides an interface for a service that maps a limited-hierarchy of topics to a generic class representing the core
  ///   properties associated with a navigation item.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public interface INavigationMappingService<T> where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | GET NAVIGATION ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A helper function that will crawl up the current tree and retrieve the topic that is <paramref name="fromRoot"/> tiers
    ///   down from the root of the topic graph.
    /// </summary>
    /// <remarks>
    ///   Often, an action of a <see cref="LayoutController{T}"/> will need a reference to a topic at a certain level, which
    ///   represents the navigation for the site. For instance, if the primary navigation is at <c>Root:Web</c>, then the
    ///   navigation is one level from the root (i.e., <paramref name="fromRoot"/>=1). This, however, should not be hard-coded
    ///   in case a site has multiple roots. For instance, if a page is under <c>Root:Library</c> then <i>that</i> should be the
    ///   navigation root. This method provides support for these scenarios.
    /// </remarks>
    /// <param name="currentTopic">The <see cref="Topic"/> to start from.</param>
    /// <param name="fromRoot">The distance that the navigation root should be from the root of the topic graph.</param>
    /// <param name="defaultRoot">If a root cannot be identified, the default root that should be returned.</param>
    Topic GetNavigationRoot(Topic currentTopic, int fromRoot = 2, string defaultRoot = "Web");

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <remarks>
    ///   In the out-of-the-box implementation, <see cref="GetRootViewModelAsync(Topic, Boolean, Int32)"/> and <see
    ///   cref="GetViewModelAsync(Topic, Boolean, Int32)"/> provide the same functionality. It is recommended that actions call
    ///   <see cref="GetRootViewModelAsync(Topic, Boolean, Int32)"/>, however, as it allows implementers the flexibility to
    ///   differentiate between the root view model (which the client application will be binding to) and any child view models
    ///   (which the client application may optionally iterate over).
    /// </remarks>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    );

    /*==========================================================================================================================
    | GET VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    Task<T> GetViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    );


    } //Interface
  } //Class