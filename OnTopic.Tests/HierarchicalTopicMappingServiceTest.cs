/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Data.Caching;
using OnTopic.Mapping;
using OnTopic.Mapping.Hierarchical;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.ViewModels;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: HIERARCHICAL TOPIC MAPPING SERVICE TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="HierarchicalTopicMappingService{T}"/>.
  /// </summary>
  [TestClass]
  public class HierarchicalTopicMappingServiceTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITopicMappingService            _topicMappingService;
    readonly                    Topic                           _topic;

    /*==========================================================================================================================
    | HIERARCHICAL TOPIC MAPPING SERVICE
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly IHierarchicalTopicMappingService<NavigationTopicViewModel> _hierarchicalMappingService;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
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
    public HierarchicalTopicMappingServiceTest() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = new CachedTopicRepository(new StubTopicRepository());
      _topic                    = _topicRepository.Load("Root:Web:Web_3:Web_3_0")!;
      _topicMappingService      = new TopicMappingService(_topicRepository, new TopicViewModelLookupService());

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish hierarchical topic mapping service
      \-----------------------------------------------------------------------------------------------------------------------*/
      _hierarchicalMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
        new HierarchicalTopicMappingService<NavigationTopicViewModel>(
          _topicRepository,
          _topicMappingService
        )
      );

    }

    /*==========================================================================================================================
    | TEST: GET HIERARCHICAL ROOT: WITH DEEP TOPIC: RETURNS ROOT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetHierarchicalRoot(Topic?, Int32, String)"/> method with a deeply
    ///   nested topic and ensures that it returns the expected root.
    /// </summary>
    [TestMethod]
    public void GetHierarchicalRoot_WithDeepTopic_ReturnsRoot() {

      var rootTopic             = _hierarchicalMappingService.GetHierarchicalRoot(_topic, 2, "Configuration");

      Assert.IsNotNull(rootTopic);
      Assert.AreEqual<string>("Web", rootTopic.Key);

    }

    /*==========================================================================================================================
    | TEST: GET VIEW MODEL: WITH TWO LEVELS: RETURNS GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync(Topic?, Int32, Func{Topic, Boolean}?)"/> method
    ///   and ensures that the expected data is returned.
    /// </summary>
    [TestMethod]
    public async Task GetViewModel_WithTwoLevels_ReturnsGraph() {

      var rootTopic             = _topicRepository.Load("Root:Web");
      var viewModel             = await _hierarchicalMappingService.GetViewModelAsync(rootTopic, 1).ConfigureAwait(false);

      Assert.IsNotNull(viewModel);
      Assert.AreEqual<int>(3, viewModel.Children.Count);
      Assert.AreEqual<int>(0, viewModel.Children[0].Children.Count);

    }

    /*==========================================================================================================================
    | TEST: GET VIEW MODEL: WITH VALIDATION DELEGATE: EXCLUDES TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="HierarchicalTopicMappingService{T}.GetViewModelAsync(Topic?, Int32, Func{Topic, Boolean}?)"/> method
    ///   with a <c>validationDelegate</c> and ensures that it correctly trims the topic graph.
    /// </summary>
    [TestMethod]
    public async Task GetViewModel_WithValidationDelegate_ExcludesTopics() {

      var rootTopic             = _topicRepository.Load("Root:Web");
      var viewModel             = await _hierarchicalMappingService
        .GetViewModelAsync(rootTopic, 2, (t) => t.Key.EndsWith("1", StringComparison.Ordinal))
        .ConfigureAwait(false);

      Assert.IsNotNull(viewModel);
      Assert.AreEqual<int>(1, viewModel.Children.Count);
      Assert.AreEqual<int>(1, viewModel.Children[0].Children.Count);

    }


  } //Class
} //Namespace