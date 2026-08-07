/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections.Specialized;
using OnTopic.Querying;
using OnTopic.Repositories;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: TOPIC INDEX REGISTRY TEST
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the live, incrementally maintained <see cref="TopicIndexRegistry"/>, accessed via <see cref=
///   "TopicExtensions.GetLiveTopicIndex(Topic)"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class TopicIndexRegistryTest {

  /*============================================================================================================================
  | TEST: GET LIVE TOPIC INDEX: SAME GRAPH: RETURNS SAME INSTANCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="TopicExtensions.GetLiveTopicIndex(Topic)"/> twice on the same graph, confirming the same instance is
  ///   returned both times, and that a subsequently attached child is present without a rebuild.
  /// </summary>
  [Fact]
  public void GetLiveTopicIndex_SameGraph_ReturnsSameInstance() {

    var root                    = new Topic("Root", "Container", null, 1);

    var firstIndex              = root.GetLiveTopicIndex();
    var secondIndex             = root.GetLiveTopicIndex();

    Assert.Same(firstIndex, secondIndex);

    var child                   = new Topic("Child", "Page", root, 2);

    Assert.Same(child, firstIndex[2]);

  }

  /*============================================================================================================================
  | TEST: ON ATTACHED: SUBTREE: INDEXES DESCENDANTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attaches a subtree of already populated <see cref="Topic"/> instances under a graph with a live index and confirms every
  ///   non-<see cref="Topic.IsNew"/> descendant is indexed.
  /// </summary>
  [Fact]
  public void OnAttached_Subtree_IndexesDescendants() {

    var root                    = new Topic("Root", "Container", null, 1);
    var index                   = root.GetLiveTopicIndex();

    var branch                  = new Topic("Branch", "Container", null, 2);
    var leaf                    = new Topic("Leaf", "Page", branch, 3);
    var newLeaf                 = new Topic("NewLeaf", "Page", branch);

    branch.Parent               = root;

    Assert.Same(branch, index[2]);
    Assert.Same(leaf, index[3]);
    Assert.False(index.ContainsKey(newLeaf.Id));

  }

  /*============================================================================================================================
  | TEST: ON DETACHED: SUBTREE: REMOVES DESCENDANTS AND SUPPORTS MOVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Detaches a subtree from a graph with a live index and confirms its descendants are removed; then re-attaches it under a
  ///   different parent in the same graph and confirms they're restored.
  /// </summary>
  [Fact]
  public void OnDetached_Subtree_RemovesDescendants_AndSupportsMove() {

    var root                    = new Topic("Root", "Container", null, 1);
    var branchA                 = new Topic("BranchA", "Container", root, 2);
    var branchB                 = new Topic("BranchB", "Container", root, 3);
    var leaf                    = new Topic("Leaf", "Page", branchA, 4);

    var index                   = root.GetLiveTopicIndex();

    Assert.True(index.ContainsKey(4));

    branchA.Children.Remove(leaf.Key);

    Assert.False(index.ContainsKey(4));

    leaf.Parent                 = branchB;

    Assert.True(index.ContainsKey(4));
    Assert.Same(branchB, index[4].Parent);

  }

  /*============================================================================================================================
  | TEST: ON ID ASSIGNED: MATERIALIZED INDEX: INDEXES TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attaches a new, unsaved <see cref="Topic"/> to a graph with a live index, then assigns its <see cref="Topic.Id"/>, thus
  ///   simulating <see cref="ITopicRepository.Save(Topic, Boolean)"/>, and confirms it then appears in the index.
  /// </summary>
  [Fact]
  public void OnIdAssigned_MaterializedIndex_IndexesTopic() {

    var root                    = new Topic("Root", "Container", null, 1);
    var index                   = root.GetLiveTopicIndex();
    var newTopic                = new Topic("New", "Page", root);

    Assert.True(newTopic.IsNew);
    Assert.False(index.ContainsKey(newTopic.Id));

    newTopic.Id                 = 42;

    Assert.Same(newTopic, index[42]);

  }

  /*============================================================================================================================
  | TEST: CLEAR ITEMS: MATERIALIZED INDEX: INVALIDATES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Clears a topic's children and confirms the next <see cref="TopicExtensions.GetLiveTopicIndex(Topic)"/> call returns a
  ///   rebuilt index that no longer contains the cleared topics, at any depth.
  /// </summary>
  [Fact]
  public void ClearItems_MaterializedIndex_Invalidates() {

    var root                    = new Topic("Root", "Container", null, 1);
    var child                   = new Topic("Child", "Page", root, 2);

    _                           = new Topic("Grandchild", "Page", child, 3);

    var firstIndex              = root.GetLiveTopicIndex();

    Assert.True(firstIndex.ContainsKey(2));
    Assert.True(firstIndex.ContainsKey(3));

    root.Children.Clear();

    var secondIndex             = root.GetLiveTopicIndex();

    Assert.False(secondIndex.ContainsKey(2));
    Assert.False(secondIndex.ContainsKey(3));

  }

  /*============================================================================================================================
  | TEST: GET LIVE TOPIC INDEX: SEPARATE GRAPHS: RETURNS INDEPENDENT INDEXES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes two separate topic graphs and confirms their live indexes are independent of one another.
  /// </summary>
  [Fact]
  public void GetLiveTopicIndex_SeparateGraphs_ReturnsIndependentIndexes() {

    var rootA                   = new Topic("RootA", "Container", null, 1);
    var rootB                   = new Topic("RootB", "Container", null, 2);

    _                           = new Topic("ChildA", "Page", rootA, 3);

    var indexA                  = rootA.GetLiveTopicIndex();
    var indexB                  = rootB.GetLiveTopicIndex();

    Assert.NotSame(indexA, indexB);
    Assert.True(indexA.ContainsKey(3));
    Assert.False(indexB.ContainsKey(3));

  }

  /*============================================================================================================================
  | TEST: ON ATTACHED: RE-ROOTED GRAPH: DOES NOT RESURRECT STALE INDEX
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Builds the live index of a standalone graph, attaches its root under another graph (merging it in), detaches it again,
  ///   and confirms a subsequent <see cref="TopicExtensions.GetLiveTopicIndex"/> call returns a freshly rebuilt index
  ///   rather than the stale, pre-merge one.
  /// </summary>
  /// <remarks>
  ///   Evaluates both <see cref="TopicIndexRegistry.OnAttached"/> and <see cref="TopicIndexRegistry.OnDetached"/> directly,
  ///   since a topic that has been fully detached from its parent has no public path to being a root again (<see cref=
  ///   "Topic.Parent"/>'s back-reference isn't cleared by removal alone, only by <see cref="Topic.SetParent(Topic, Topic)"/>
  ///   assigning a new one). This test evaluates the internal hook contract, not a publicly reachable sequence.
  /// </remarks>
  [Fact]
  public void OnAttached_ReRootedGraph_DoesNotResurrectStaleIndex() {

    var standaloneRoot          = new Topic("Standalone", "Container", null, 5);

    _                           = new Topic("StandaloneChild", "Page", standaloneRoot, 6);

    var staleIndex              = standaloneRoot.GetLiveTopicIndex();

    Assert.True(staleIndex.ContainsKey(6));

    var mainRoot                = new Topic("Main", "Container", null, 1);

    _                           = mainRoot.GetLiveTopicIndex();

    TopicIndexRegistry.OnAttached(mainRoot, standaloneRoot);
    TopicIndexRegistry.OnDetached(mainRoot, standaloneRoot);

    var rebuiltIndex            = standaloneRoot.GetLiveTopicIndex();

    Assert.NotSame(staleIndex, rebuiltIndex);
    Assert.True(rebuiltIndex.ContainsKey(6));

  }

  /*============================================================================================================================
  | TEST: ON ATTACHED: NOT LOADED INTERMEDIATE: INDEXES PHYSICAL DESCENDANTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attaches a subtree whose intermediate node has <see cref="LoadState.NotLoaded"/> <see cref="Topic.Children"/> but also
  ///   present children, and confirms every physical descendant is indexed regardless; detaches it and confirms every physical
  ///   descendant is pruned regardless.
  /// </summary>
  /// <remarks>
  ///   <see cref="TopicExtensions.FindAll(Topic)"/> traverses raw, bypassing the autoloading getter, and the registry's hooks
  ///   must not reintroduce a <c>LoadState</c> gate.
  /// </remarks>
  [Fact]
  public void OnAttached_NotLoadedIntermediate_IndexesPhysicalDescendants() {

    var root                    = new Topic("Root", "Container", null, 1);
    var index                   = root.GetLiveTopicIndex();

    var intermediate            = new Topic("Intermediate", "Container", null, 2);

    _                           = new Topic("PhysicalChild", "Page", intermediate, 3);

    ((ITopicBackingAccessor)intermediate).Children.LoadState = LoadState.NotLoaded;

    intermediate.Parent         = root;

    Assert.True(index.ContainsKey(2));
    Assert.True(index.ContainsKey(3));

    root.Children.Remove(intermediate.Key);

    Assert.False(index.ContainsKey(2));
    Assert.False(index.ContainsKey(3));

  }

} //Class