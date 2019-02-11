/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ignia.Topics.Mapping;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.AspNetCore.Mvc.Controllers {

  /*============================================================================================================================
  | CLASS: CACHED LAYOUT CONTROLLER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides access to views for populating specific layout dependencies, such as the <see cref="Menu"/>, while caching
  ///   the <see cref="INavigationTopicViewModel{T}"/> graphs for performance.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     As a best practice, global data required by the layout view are requested independently of the current page. This
  ///     allows each layout element to be provided with its own layout data, in the form of <see
  ///     cref="NavigationViewModel{T}"/>s, instead of needing to add this data to every view model returned by <see
  ///     cref="TopicController"/>. The <see cref="LayoutController{T}"/> facilitates this by not only providing a default
  ///     implementation for <see cref="Menu"/>, but additionally providing protected helper methods that aid in locating and
  ///     assembling <see cref="Topic"/> and <see cref="INavigationTopicViewModelCore"/> references that are relevant to
  ///     specific layout elements.
  ///   </para>
  ///   <para>
  ///     In order to remain view model agnostic, the <see cref="LayoutController{T}"/> does not assume that a particular view
  ///     model will be used, and instead accepts a generic argument for any view model that implements the interface <see
  ///     cref="INavigationTopicViewModelCore"/>. Since generic controllers cannot be effectively routed to, however, that means
  ///     implementors must, at minimum, provide a local instance of <see cref="LayoutController{T}"/> which sets the generic
  ///     value to the desired view model. To help enforce this, while avoiding ambiguity, this class is marked as
  ///     <c>abstract</c> and suffixed with <c>Base</c>.
  ///   </para>
  ///   <para>
  ///     By comparison to the <see cref="LayoutControllerBase{T}"/>, the <see cref="CachedLayoutControllerBase{T}"/> will
  ///     automatically cache the <see cref="INavigationTopicViewModel{T}"/> graph for each action that uses the protected <see
  ///     cref="GetViewModelAsync(Topic, Boolean, Int32)"/> method to construct the graph. This is preferable over using e.g.
  ///     the <see cref="CachedTopicMappingService"/> since the <see cref="LayoutControllerBase{T}"/> requires tight control
  ///     over the shape of the <see cref="INavigationTopicViewModel{T}"/> graph. For instance, using a generic caching
  ///     decorator for the mapping might result in the edges of the <see cref="Menu"/> action being expanded due to other
  ///     actions reusing cached instances (e.g., for page-level navigation). To mitigate this, the <see
  ///     cref="CachedLayoutControllerBase{T}"/> handles top-level caching at the level of the navigation root.
  ///   </para>
  /// </remarks>
  public abstract class CachedLayoutControllerBase<T> : LayoutControllerBase<T>
    where T : class, INavigationTopicViewModel<T>, new() {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static ConcurrentDictionary<int, T> _cache = new ConcurrentDictionary<int, T>();

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a Topic Controller with necessary dependencies.
    /// </summary>
    /// <returns>A topic controller for loading OnTopic views.</returns>
    protected CachedLayoutControllerBase(
      ITopicRepository topicRepository,
      ITopicRoutingService topicRoutingService,
      ITopicMappingService topicMappingService
    ) : base(topicRepository, topicRoutingService, topicMappingService) {}

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
    protected override async Task<T> GetRootViewModelAsync(
      Topic sourceTopic,
      bool allowPageGroups      = true,
      int tiers                 = 1
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

  } // Class

} // Namespace