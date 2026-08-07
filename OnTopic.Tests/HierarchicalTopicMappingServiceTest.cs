/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Data.Caching;
using OnTopic.Lookup;
using OnTopic.Mapping;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.LazyLoading;
using OnTopic.Tests.Fixtures;
using OnTopic.Tests.TestDoubles;
using OnTopic.ViewModels;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: HIERARCHICAL TOPIC MAPPING SERVICE TEST
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="HierarchicalTopicMappingService{T}"/>.
/// </summary>
[ExcludeFromCodeCoverage]
[Xunit.Collection("Shared Repository")]
public class HierarchicalTopicMappingServiceTest: IClassFixture<TopicInfrastructureFixture<StubTopicRepository>> {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      ITopicRepository                _topicRepository;
  readonly                      Topic                           _topic;

  /*============================================================================================================================
  | HIERARCHICAL TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalMappingService;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="HierarchicalTopicMappingServiceTest"/> with shared resources.
  /// </summary>
  /// <remarks>
  ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
  ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
  ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
  ///   crawling the object graph. In addition, it initializes a shared <see cref="Topic"/> reference to use for the various
  ///   tests.
  /// </remarks>
  public HierarchicalTopicMappingServiceTest(TopicInfrastructureFixture<StubTopicRepository> fixture) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(fixture,  nameof(fixture));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish dependencies
    \-------------------------------------------------------------------------------------------------------------------------*/
    _topicRepository            = fixture.CachedTopicRepository;
    _topic                      =  _topicRepository.Load("Root:Web:Web_3:Web_3_0").GetAwaiter().GetResult()!;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish hierarchical topic mapping service
    \-------------------------------------------------------------------------------------------------------------------------*/
    _hierarchicalMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
      new HierarchicalTopicMappingService<NavigationTopicViewModel>(
        fixture.TopicRepository,
        fixture.MappingService
      )
    );

  }

  /*============================================================================================================================
  | TEST: GET HIERARCHICAL ROOT: WITH NULL TOPIC: RETURNS DEFAULT ROOT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetHierarchicalRoot(Topic?, Int32, String)"/> method with no
  ///   <c>currentTopic</c> and ensures it falls back to the <c>defaultRoot</c>.
  /// </summary>
  [Fact]
  public void GetHierarchicalRoot_WithNullTopic_ReturnsDefaultRoot() {

    var rootTopic               = _hierarchicalMappingService.GetHierarchicalRoot(null, 2, "Configuration");

    Assert.NotNull(rootTopic);
    Assert.Equal("Configuration", rootTopic?.Key);

  }

  /*============================================================================================================================
  | TEST: GET HIERARCHICAL ROOT: WITH NULL TOPIC: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetHierarchicalRoot(Topic?, Int32, String)"/> method with no
  ///   <c>currentTopic</c> or <c>defaultRoot</c> and ensures it throws an <see cref="ArgumentNullException"/>.
  /// </summary>
  [Fact]
  public void GetHierarchicalRoot_WithNullTopic_ThrowsException() =>
    Assert.Throws<ArgumentNullException>(() =>
      _hierarchicalMappingService.GetHierarchicalRoot(null, 2, "")
    );

  /*============================================================================================================================
  | TEST: GET HIERARCHICAL ROOT: WITH INVALID DEFAULT ROOT: TRHOWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetHierarchicalRoot(Topic?, Int32, String)"/> method with no
  ///   <c>currentTopic</c> and throws an <see cref="ArgumentException"/> when it cannot identify the <c>defaultRoot</c>.
  /// </summary>
  [Fact]
  public void GetHierarchicalRoot_WithInvalidDefaultRoot_ThrowsException() =>
    Assert.Throws<ArgumentOutOfRangeException>(() =>
      _hierarchicalMappingService.GetHierarchicalRoot(null, 2, "InvalidDefaultRoot")
    );

  /*============================================================================================================================
  | TEST: GET HIERARCHICAL ROOT: WITH DEEP TOPIC: RETURNS ROOT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetHierarchicalRoot(Topic?, Int32, String)"/> method with a deeply
  ///   nested topic and ensures that it returns the expected root.
  /// </summary>
  [Fact]
  public void GetHierarchicalRoot_WithDeepTopic_ReturnsRoot() {

    var rootTopic               = _hierarchicalMappingService.GetHierarchicalRoot(_topic, 2, "Configuration");

    Assert.NotNull(rootTopic);
    Assert.Equal("Web", rootTopic?.Key);

  }

  /*============================================================================================================================
  | TEST: GET VIEW MODEL: WITH TWO LEVELS: RETURNS GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync(Topic?, Int32, Func{Topic, Boolean}?)"/> method
  ///   and ensures that the expected data is returned, with children landing in the same order as <see cref="Topic.Children"/>
  ///   (i.e., source order), confirming the sequential <c>foreach</c> fan-out never reorders on completion.
  /// </summary>
  [Fact]
  public async Task GetViewModel_WithTwoLevels_ReturnsGraph() {

    var rootTopic               = await _topicRepository.Load("Root:Web");
    var expectedOrder            = rootTopic!.Children.Select(t => t.GetWebPath()).ToList();
    var viewModel                = await _hierarchicalMappingService.GetViewModelAsync(rootTopic, 1);

    Assert.NotNull(viewModel);
    Assert.Equal(3, viewModel.Children.Count);
    Assert.Empty(viewModel.Children[0].Children);
    Assert.Equal(expectedOrder, viewModel.Children.Select(c => c.WebPath));

  }

  /*============================================================================================================================
  | TEST: GET VIEW MODEL: WITH VALIDATION DELEGATE: EXCLUDES TOPICS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync(Topic?, Int32, Func{Topic, Boolean}?)"/> method
  ///   with a <c>validationDelegate</c> and ensures that it correctly trims the topic graph.
  /// </summary>
  [Fact]
  public async Task GetViewModel_WithValidationDelegate_ExcludesTopics() {

    var rootTopic               = await _topicRepository.Load("Root:Web");
    var viewModel               = await _hierarchicalMappingService
      .GetViewModelAsync(rootTopic, 2, (t) => t.Key.EndsWith('1'));

    Assert.NotNull(viewModel);
    Assert.Single(viewModel.Children);
    Assert.Single(viewModel.Children[0].Children);

  }

  /*============================================================================================================================
  | TEST: GET VIEW MODEL: WITH DISABLED: EXCLUDES DISABLED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync(Topic?, Int32, Func{Topic, Boolean}?)"/> method
  ///   with a <see cref="Topic.IsDisabled"/> topic in the graph, and ensures it is not returned.
  /// </summary>
  [Fact]
  public async Task GetViewModel_WithDisabled_ExcludesDisabled() {

    var rootTopic               = (await _topicRepository.Load("Root:Web:Web_3"))!;
    var disabledTopic           = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");

    Contract.Assume(disabledTopic);

    rootTopic.IsDisabled        = true;
    disabledTopic.IsDisabled    = true;

    var viewModel               = await _hierarchicalMappingService.GetViewModelAsync(rootTopic, 1);

    Assert.NotNull(viewModel);
    Assert.Single(viewModel.Children);

    //Revert state
    rootTopic.IsDisabled        = false;
    disabledTopic.IsDisabled    = false;

  }

  /*============================================================================================================================
  | TEST: GET VIEW MODEL: DEPTH TWO: WARMS ONCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync"/> with <c>tiers: 2</c> against a fresh <see cref=
  ///   "StubLazyLoadingTopicRepository"/>, and confirms the region is warmed in a single round-trip: An identical second call
  ///   is a clean, converged hit that issues no further fetches, proving the recursive descent never falls back to per-node
  ///   lazy loads.
  /// </summary>
  [Fact]
  public async Task GetViewModel_DepthTwo_WarmsOnce() {

    var stub                    = new StubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(stub);
    var typeLookupService       = new CompositeTypeLookupService(new TopicViewModelLookupService(), new FakeViewModelLookupService());
    var mappingService          = new TopicMappingService(cache, typeLookupService);
    var hierarchicalService     = new HierarchicalTopicMappingService<NavigationTopicViewModel>(cache, mappingService);

    var webTopic                = await cache.Load("Root:Web");

    var viewModel               = await hierarchicalService.GetViewModelAsync(webTopic, 2);
    var fetchesAfterFirstMap    = stub.TotalFetches;

    Assert.NotNull(viewModel);

    _                           = await hierarchicalService.GetViewModelAsync(webTopic, 2);

    Assert.Equal(fetchesAfterFirstMap, stub.TotalFetches);

  }

  /*============================================================================================================================
  | TEST: GET VIEW MODEL: NEW SOURCE TOPIC: ISSUES NO LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync"/> against an unsaved <see cref="Topic"/> (<see
  ///   cref="Topic.IsNew"/>) and confirms the warm-up load is skipped: An unsaved topic's <c>Id</c> of <c>-1</c> would
  ///   otherwise route <see cref="ITopicRepository.Load(Int32, Topic?, TopicPayload, Int32)"/>, loading the root node, rather
  ///   than warming the intended region.
  /// </summary>
  [Fact]
  public async Task GetViewModel_NewSourceTopic_IssuesNoLoad() {

    var stub                    = new StubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(stub);
    var typeLookupService       = new CompositeTypeLookupService(new TopicViewModelLookupService(), new FakeViewModelLookupService());
    var mappingService          = new TopicMappingService(cache, typeLookupService);
    var hierarchicalService     = new HierarchicalTopicMappingService<NavigationTopicViewModel>(cache, mappingService);

    var newTopic                = new Topic("Test", "Page");
    var fetchesBeforeMap        = stub.TotalFetches;

    var viewModel               = await hierarchicalService.GetViewModelAsync(newTopic, 2);

    Assert.NotNull(viewModel);
    Assert.Equal(fetchesBeforeMap, stub.TotalFetches);

  }

} //Class