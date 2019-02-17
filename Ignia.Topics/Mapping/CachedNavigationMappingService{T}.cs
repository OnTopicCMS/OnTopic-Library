/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: CACHED NAVIGATION MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a wrapper to a <see cref="INavigationMappingService{T}"/> implementation with support for caching.
  /// </summary>
  /// <remarks>
  ///   By comparison to the <see cref="NavigationMappingService{T}"/>, the <see cref="CachedNavigationMappingService{T}"/> will
  ///   automatically cache the <see cref="INavigationTopicViewModel{T}"/> graph for each action that uses the protected <see
  ///   cref="INavigationMappingService.GetViewModelAsync(Topic, Boolean, Int32)"/> method to construct the graph. This is
  ///   preferable over using e.g. the <see cref="CachedTopicMappingService"/> since the <see cref="LayoutControllerBase{T}"/>
  ///   requires tight control over the shape of the <see cref="INavigationTopicViewModel{T}"/> graph. For instance, using a
  ///   generic caching decorator for the mapping might result in the edges of the <see cref="Menu"/> action being expanded due
  ///   to other actions reusing cached instances (e.g., for page-level navigation). To mitigate this, the <see
  ///   cref="CachedLayoutControllerBase{T}"/> handles top-level caching at the level of the navigation root.
  /// </remarks>
  public abstract class CachedNavigationMappingService<T> : INavigationMappingService<T>
    where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly ConcurrentDictionary<int, T> _cache = new ConcurrentDictionary<int, T>();
    private readonly INavigationMappingService<T> _navigationMappingService = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    protected CachedNavigationMappingService(
      INavigationMappingService<T> navigationMappingService
    ) {
      _navigationMappingService = navigationMappingService;
    }

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
    public Topic GetNavigationRoot(Topic currentTopic, int fromRoot = 2, string defaultRoot = "Web") {
      return _navigationMappingService.GetNavigationRoot(currentTopic, fromRoot, defaultRoot);
    }

    /*==========================================================================================================================
    | GET ROOT VIEW MODEL (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/>, maps a <typeparamref name="T"/>, as well as <paramref name="tiers"/> of
    ///   <see cref="INavigationTopicViewModel{T}.Children"/>. If the <see cref="INavigationTopicViewModel{T}"/> graph has been
    ///   mapped before, then a cached instance is returned. Optionally excludes <see cref="Topic"/> instance with the
    ///   <c>ContentType</c> of <c>PageGroup</c>.
    /// </summary>
    /// <param name="sourceTopic">The <see cref="Topic"/> to pull the values from.</param>
    /// <param name="allowPageGroups">Determines whether <see cref="PageGroupTopicViewModel"/>s should be crawled.</param>
    /// <param name="tiers">Determines how many tiers of children should be included in the graph.</param>
    public async Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceTopic == null) {
        return await Task<T>.FromResult<T>(null).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cache hits
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_cache.TryGetValue(sourceTopic.Id, out var dto)) {
        return await Task<T>.FromResult<T>(dto).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache and return new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModel = await GetViewModelAsync(sourceTopic, allowPageGroups, tiers).ConfigureAwait(false);
      return _cache.GetOrAdd(sourceTopic.Id, viewModel);

    }

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
    public async Task<T> GetViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups = true,
      int tiers = 1
    ) {
      return await _navigationMappingService.GetViewModelAsync(sourceTopic, allowPageGroups, tiers).ConfigureAwait(false);
    }

  } // Class
} // Namespace