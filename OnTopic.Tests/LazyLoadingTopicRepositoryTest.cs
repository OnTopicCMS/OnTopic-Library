/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Associations;
using OnTopic.Data.Caching;
using OnTopic.Repositories;
using OnTopic.TestDoubles.LazyLoading;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: LAZY LOADING TOPIC REPOSITORY TESTS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the store-independent, lazy-loading <see cref="LazyLoadingTopicRepository"/>, evaluated through
///   the <see cref="StubLazyLoadingTopicRepository"/>, a lazy test double that serves shallow topics from a flat, SQL-free
///   record store and loads each property on demand.
/// </summary>
/// <remarks>
///   <para>
///     <see cref="_loadingTopicRepository"/> is a standalone <see cref="StubLazyLoadingTopicRepository"/>, never wrapped by a
///     <see cref="CachedTopicRepository"/>. This matters: Were it wrapped in e.g., <see cref="TopicRepositoryBaseTest"/>'s
///     shared field pattern, then <see cref="ITopicRepository.TopicLoaded"/>, which is raised whenever the double builds a
///     topic for the first time, whether requested directly against the inner repository or through the decorator, would
///     synchronously re-enter the outer cache's handler, restamping that topic's <see cref="ITopicLazyLoadable.Loader"/> as the
///     cache rather than the double itself. That would silently reroute every autoloading getter to <see cref=
///     "CachedTopicRepository.EnsureLoaded"/>, which delegates children and extended attributes to the inner resolver but
///     withholds relationships and references, resolving them itself via <c>LoadDeferredAssociations</c> instead. The double's
///     own fetch-count spy would then never see association fetches. So the standalone-mechanism tests (groups A through G, and
///     J) use <see cref="_loadingTopicRepository"/>, while the decorator-specific tests (groups H and I) use <see cref=
///     "_cachedTopicRepository"/>, which wraps its own, separate <see cref="StubLazyLoadingTopicRepository"/> instance.
///   </para>
///   <para>
///     Both repositories share the same built-in seed dataset (see <see cref="StubLazyLoadingTopicRepository"/>'s default
///     constructor): A four-level <c>Root:Web</c> content subtree of <c>Web</c>, <c>Web_0</c>, <c>Web_0_0</c> (carrying an
///     extended attribute), <c>Web_0_0_0</c>, plus a sibling <c>Web_1</c>, with a resolvable relationship and reference pair
///     (<c>Web_1</c>, <c>Web_0_0</c> / <c>Web_0</c>) and a stale, unresolvable pair (<c>Web_0</c>, a nonexistent target).
///   </para>
/// </remarks>
[ExcludeFromCodeCoverage]
public class LazyLoadingTopicRepositoryTest {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      StubLazyLoadingTopicRepository  _loadingTopicRepository;
  readonly                      CachedTopicRepository           _cachedTopicRepository;

  /*============================================================================================================================
  | PROPERTY: CANCELLATION TOKEN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Shorthand for <see cref="TestContext.Current"/>'s <see cref="TestContext.CancellationToken"/>.
  /// </summary>
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="LazyLoadingTopicRepositoryTest"/> with two independent repositories: A
  ///   standalone <see cref="StubLazyLoadingTopicRepository"/> for evaluating the repository directly, and a second, separate
  ///   instance wrapped by a <see cref="CachedTopicRepository"/> for evaluating decorator-specific behavior.
  /// </summary>
  public LazyLoadingTopicRepositoryTest() {
    _loadingTopicRepository     = new();
    _cachedTopicRepository      = new(new StubLazyLoadingTopicRepository());
  }

  #region A: Genuine Deferral on Load

  /*============================================================================================================================
  | TEST: LOAD: DEFAULT PAYLOAD: CHILDREN NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with the default payload and confirms its <see cref="Topic.Children"/> property is genuinely absent: Not
  ///   merely flagged <see cref="LoadState.NotLoaded"/>, but backed by an empty collection.
  /// </summary>
  [Fact]
  public async Task Load_DefaultPayload_ChildrenNotLoaded() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");

    Assert.False(((ITopicLazyLoadable)topic!).IsLoaded(TopicPayload.Children));
    Assert.Empty(((ITopicBackingAccessor)topic).Children);

  }

  /*============================================================================================================================
  | TEST: LOAD: DEFAULT PAYLOAD: EXTENDED ATTRIBUTES NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with an extended attribute and confirms the extended-attribute boundary is genuinely absent: The attribute
  ///   value itself is missing from the backing collection, not merely flagged.
  /// </summary>
  [Fact]
  public async Task Load_DefaultPayload_ExtendedAttributesNotLoaded() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");

    Assert.False(((ITopicLazyLoadable)topic!).IsLoaded(TopicPayload.ExtendedAttributes));
    Assert.False(topic.Attributes.Contains("Body"));

  }

  /*============================================================================================================================
  | TEST: LOAD: DEFAULT PAYLOAD: ASSOCIATIONS DEFERRED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with both a relationship and a reference and confirms both association properties are <see cref=
  ///   "LoadState.NotLoaded"/>, with genuine <c>DeferredAssociation</c> entries recorded, not resolved targets.
  /// </summary>
  [Fact]
  public async Task Load_DefaultPayload_AssociationsDeferred() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");
    var rawTopic                = (ITopicBackingAccessor)topic!;
    var lazyTopic               = (ITopicLazyLoadable)topic!;

    Assert.False(lazyTopic.IsLoaded(TopicPayload.Relationships));
    Assert.False(lazyTopic.IsLoaded(TopicPayload.References));
    Assert.NotEmpty(rawTopic.Relationships.Deferred);
    Assert.NotEmpty(rawTopic.References.Deferred);

  }

  #endregion

  #region B: On-Demand Materialization via the Autoloading Getters

  /*============================================================================================================================
  | TEST: CHILDREN: NOT LOADED: MATERIALIZES REAL CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Touches <see cref="Topic.Children"/> on a topic whose children are <see cref="LoadState.NotLoaded"/> and confirms the
  ///   getter returns the actual child topics from the record store, not an empty collection with a flipped flag.
  /// </summary>
  [Fact]
  public async Task Children_NotLoaded_MaterializesRealChildren() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");
    var children                = topic!.Children;

    Assert.Equal(2, children.Count);
    Assert.Contains(children, child => child.Key == "Web_0");
    Assert.Contains(children, child => child.Key == "Web_1");

  }

  /*============================================================================================================================
  | TEST: RELATIONSHIPS: NOT LOADED: MATERIALIZES TARGETS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Touches <see cref="Topic.Relationships"/> on a topic with a resolvable relationship target and confirms the getter
  ///   connects the real target object.
  /// </summary>
  [Fact]
  public async Task Relationships_NotLoaded_MaterializesTargets() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");
    var related                 = topic!.Relationships.GetValues("Related");

    Assert.Single(related);
    Assert.Equal("Web_0_0", related[0].Key);

  }

  /*============================================================================================================================
  | TEST: REFERENCES: NOT LOADED: MATERIALIZES TARGETS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Touches <see cref="Topic.References"/> on a topic with a resolvable reference target and confirms the getter lazy loads
  ///   the target object.
  /// </summary>
  [Fact]
  public async Task References_NotLoaded_MaterializesTargets() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");

    Assert.True(topic!.References.Contains("BaseTopic"));
    Assert.Equal("Web_0", topic.References["BaseTopic"].Value?.Key);

  }

  #endregion

  #region C: On-Demand Materialization via Async Ensure Loaded

  /*============================================================================================================================
  | TEST: ENSURE LOADED: CHILDREN: MATERIALIZES BEFORE ACCESS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Awaits <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload, System.Threading.CancellationToken)"/> for <see cref="TopicPayload.Children"/>, then
  ///   confirms the boundary is already <see cref="LoadState.Loaded"/> and backed by real data before the getter is touched.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_Children_MaterializesBeforeAccess() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");
    var rawTopic                = (ITopicLazyLoadable)topic!;

    await rawTopic.EnsureLoaded(TopicPayload.Children, cancellationToken: CancellationToken);

    Assert.True(rawTopic.IsLoaded(TopicPayload.Children));
    Assert.Equal(2, ((ITopicBackingAccessor)topic).Children.Count);

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: EXTENDED ATTRIBUTES: MATERIALIZES REAL VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Awaits <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload,System.Threading.CancellationToken)"/> for <see cref=
  ///   "TopicPayload.ExtendedAttributes"/> and confirms the extended attribute getter returns the real value from the record
  ///   store, not merely a flipped <see cref="LoadState"/>. This is a distinct autoload seam from <see cref="Topic.Children"/>
  ///   and the association getters: It lives in <c>AttributeCollection.GetValue</c>, not directly on a <see cref="Topic"/>
  ///   property getter.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_ExtendedAttributes_MaterializesRealValue() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");
    var rawTopic                = (ITopicLazyLoadable)topic!;

    await rawTopic.EnsureLoaded(TopicPayload.ExtendedAttributes, cancellationToken: CancellationToken);

    Assert.True(rawTopic.IsLoaded(TopicPayload.ExtendedAttributes));
    Assert.Equal("Extended body content for Web_0_0.", topic.Attributes.GetValue("Body"));

  }

  #endregion

  #region D: Fetch-Once (Spy)

  /*============================================================================================================================
  | TEST: CHILDREN: ACCESSED TWICE: FETCHES ONCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Touches <see cref="Topic.Children"/> twice and confirms the record store is only fetched once, via the double's
  ///   per-topic, per-property fetch-count spy.
  /// </summary>
  [Fact]
  public async Task Children_AccessedTwice_FetchesOnce() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");

    _                           = topic!.Children;
    _                           = topic.Children;

    Assert.Equal(1, _loadingTopicRepository.GetFetchCount(topic.Id, TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: ALREADY LOADED: DOES NOT FETCH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with children already requested, then calls <see cref=
  ///   "ITopicLazyLoadable.EnsureLoaded(TopicPayload,System.Threading.CancellationToken)"/> again for the property payload, and
  ///   confirms no additional fetch is recorded.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_AlreadyLoaded_DoesNotFetch() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web", payload: TopicPayload.Children);
    var fetchesAfterLoad        = _loadingTopicRepository.TotalFetches;

    await ((ITopicLazyLoadable)topic!).EnsureLoaded(TopicPayload.Children, cancellationToken: CancellationToken);

    Assert.Equal(fetchesAfterLoad, _loadingTopicRepository.TotalFetches);

  }

  #endregion

  #region E: Recursive Lazy Descent

  /*============================================================================================================================
  | TEST: CHILDREN: MATERIALIZED CHILD: IS ITSELF LAZY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Materializes the first level of a topic's children and confirms a materialized child reports its own children as <see
  ///   cref="LoadState.NotLoaded"/>, with no fetch yet recorded for that child, then touches the child's own <see cref=
  ///   "Topic.Children"/> and confirms a separate, later fetch materializes the next level. Proves that nothing trickles past
  ///   the level actually accessed.
  /// </summary>
  [Fact]
  public async Task Children_MaterializedChild_IsItselfLazy() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");
    var web0                    = topic!.Children["Web_0"];

    Assert.False(((ITopicLazyLoadable)web0).IsLoaded(TopicPayload.Children));
    Assert.Equal(0, _loadingTopicRepository.GetFetchCount(web0.Id, TopicPayload.Children));

    _                           = web0.Children;

    Assert.Equal(1, _loadingTopicRepository.GetFetchCount(web0.Id, TopicPayload.Children));

  }

  #endregion

  #region F: Resolver Stamping through the Public Path

  /*============================================================================================================================
  | TEST: LOAD: SERVED NODE: IS STAMPED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic and confirms it carries a non-null <see cref="ITopicLazyLoadable.Loader"/>, stamped through the public
  ///   <see cref="ITopicRepository.Load(String, Topic?, Boolean, TopicPayload)"/>/event path.
  /// </summary>
  [Fact]
  public async Task Load_ServedNode_IsStamped() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");

    Assert.NotNull(((ITopicLazyLoadable)topic!).Loader);

  }

  /*============================================================================================================================
  | TEST: CHILDREN: MATERIALIZED CHILDREN: ARE STAMPED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads children via <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload,System.Threading.CancellationToken)"/> and
  ///   confirms each child carries a non-null <see cref="ITopicLazyLoadable.Loader"/>, which is what enables recursive lazy
  ///   loading (see <see cref="Children_MaterializedChild_IsItselfLazy"/>): The per-child <c>OnTopicLoaded</c> event raised
  ///   during materialization is what stamps them.
  /// </summary>
  [Fact]
  public async Task Children_MaterializedChildren_AreStamped() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");

    await ((ITopicLazyLoadable)topic!).EnsureLoaded(TopicPayload.Children, cancellationToken: CancellationToken);

    var children                = ((ITopicBackingAccessor)topic).Children;

    Assert.Equal(2, children.Count);

    foreach (var child in children) {
      Assert.NotNull(((ITopicLazyLoadable)child).Loader);
    }

  }

  /*============================================================================================================================
  | TEST: LOAD: DEEP NODE: ASCENDANTS ARE STAMPED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a deeply nested topic from a standalone repository and confirms that an ascendant, never itself the target of a
  ///   <c>Load()</c> call, is nonetheless stamped with an <see cref="ITopicLazyLoader"/>.
  /// </summary>
  [Fact]
  public async Task Load_DeepNode_AscendantsAreStamped() {

    var topicRepository         = new StubLazyLoadingTopicRepository();
    var topic                   = await topicRepository.Load("Root:Web:Web_0:Web_0_0:Web_0_0_0");
    var ascendant               = topic?.Parent?.Parent;

    Assert.NotNull(ascendant);
    Assert.NotNull((ascendant as ITopicLazyLoadable)?.Loader);

  }

  /*============================================================================================================================
  | TEST: LOAD: DEEP NODE: RELOAD IS IDEMPOTENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Reloads the same deep topic twice and confirms ascendants remain correctly stamped, with no spurious fetches triggered
  ///   by the second load, an indirect check of the ascendant-stamping's short-circuit correctness.
  /// </summary>
  [Fact]
  public async Task Load_DeepNode_ReloadIsIdempotent() {

    var topicRepository         = new StubLazyLoadingTopicRepository();
    var uniqueKey               = "Root:Web:Web_0:Web_0_0:Web_0_0_0";

    _                           = await topicRepository.Load(uniqueKey);

    var fetchesAfterFirstLoad   = topicRepository.TotalFetches;
    var reloaded                = await topicRepository.Load(uniqueKey);

    Assert.Equal(fetchesAfterFirstLoad, topicRepository.TotalFetches);
    Assert.NotNull((reloaded?.Parent?.Parent as ITopicLazyLoadable)?.Loader);

  }

  /*============================================================================================================================
  | TEST: SAVE: NEW TOPIC: STAMPS RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> and confirms the repository stamps a <see cref="ITopicLazyLoader"/> onto it so that
  ///   deferred boundaries can be populated on demand after the save.
  /// </summary>
  [Fact]
  public async Task Save_NewTopic_StampsResolver() {

    var parent                  = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");
    var topic                   = new Topic("Test", "Page", parent);

    await _loadingTopicRepository.Save(topic);

    Assert.NotNull(((ITopicLazyLoadable)topic).Loader);

  }

  #endregion

  #region G: Force-Load Gate (Stamping Must Not Fill)

  /*============================================================================================================================
  | TEST: CHILDREN: MATERIALIZED: STAMPING DOES NOT LOAD GRANDCHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads one level of children and confirms the loader-stamping pass triggered by each child's <c>OnTopicLoaded</c> event
  ///   does not, in turn, force-load its own children. Each child's own <see cref="Topic.Children"/> property is <see cref=
  ///   "LoadState.NotLoaded"/>, so the gate at <c>LazyLoadingTopicRepository.StampLoader</c>, which only recurses into a
  ///   topic's <i>already-loaded</i> children, stamps the child without descending. Were the gate removed, <c>StampLoader</c>'s
  ///   recursion would autoload every child's children, and the spy would show fetches for them; instead it shows none.
  /// </summary>
  [Fact]
  public async Task Children_Materialized_StampingDoesNotLoadGrandchildren() {

    var topic                   = await _loadingTopicRepository.Load("Root:Web");
    var children                = topic!.Children;
    var web0                    = children["Web_0"];
    var web1                    = children["Web_1"];

    Assert.Equal(1, _loadingTopicRepository.GetFetchCount(topic.Id, TopicPayload.Children));
    Assert.Equal(0, _loadingTopicRepository.GetFetchCount(web0.Id, TopicPayload.Children));
    Assert.Equal(0, _loadingTopicRepository.GetFetchCount(web1.Id, TopicPayload.Children));

  }

  #endregion

  #region H: Deferred-Association Resolution through the Cache Decorator

  /*============================================================================================================================
  | TEST: ENSURE LOADED: STALE RELATIONSHIP TARGET: IS DISCARDED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.EnsureLoaded"/> on a topic with a deferred relationship whose target is absent
  ///   from the underlying record store, and confirms the association resolves to nothing: The deferred entry is dropped, not
  ///   left dangling, while the property still ends up <see cref="LoadState.Loaded"/>.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_StaleRelationshipTarget_IsDiscarded() {

    var source                  = await _cachedTopicRepository.Load("Root:Web:Web_0");

    await _cachedTopicRepository.EnsureLoaded(source!, TopicPayload.Relationships, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, source!.Relationships.LoadState);
    Assert.Empty(source.Relationships.GetValues("Related"));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: STALE REFERENCE TARGET: IS DISCARDED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.EnsureLoaded"/> on a topic with a deferred reference whose target is absent from
  ///   the underlying record store, and confirms the association resolves to nothing: The deferred entry is dropped, not left
  ///   dangling, while the property still ends up <see cref="LoadState.Loaded"/>.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_StaleReferenceTarget_IsDiscarded() {

    var source                  = await _cachedTopicRepository.Load("Root:Web:Web_0");

    await _cachedTopicRepository.EnsureLoaded(source!, TopicPayload.References, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, source!.References.LoadState);
    Assert.False(source.References.Contains("BaseTopic"));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: MISSING REFERENCE TARGET: RESOLVES AND CONNECTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.EnsureLoaded"/> on a topic whose <c>References.LoadState</c> is <c>NotLoaded</c>,
  ///   confirming that the loader re-queries for the topic's references, loads a target initially absent from the cache, and
  ///   connects the edge. The reference-target complement to <see cref=
  ///   "TopicRepositoryBaseTest.EnsureLoaded_WithMissingRelationshipTarget_ResolvesAndConnects"/>.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_MissingReferenceTarget_ResolvesAndConnects() {

    // The cache seeds only Root and Root:Configuration; "Web" (id 10000) is initially absent from the cache
    var root                    = (await _cachedTopicRepository.Load(-1))!;
    ((ITopicBackingAccessor)root).References.Deferred.Add(new("_stub", 10000));

    await _cachedTopicRepository.EnsureLoaded(root, TopicPayload.References, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, root.References.LoadState);
    Assert.Equal(10000, root.References["_stub"].Value?.Id);

  }

  #endregion

  #region I: Decorator Stamp Precedence

  /*============================================================================================================================
  | TEST: LOAD: DECORATED: OUTER RESOLVER WINS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic through a <see cref="CachedTopicRepository"/> wrapping the lazy double and confirms the loaded topic's
  ///   <see cref="ITopicLazyLoadable.Loader"/> is the outer cache instance, not the inner double, verifying the synchronous
  ///   re-entry described on <c>LazyLoadingTopicRepository.OnTopicLoaded</c>.
  /// </summary>
  [Fact]
  public async Task Load_Decorated_OuterResolverWins() {

    var topic                   = await _cachedTopicRepository.Load("Root:Web");

    Assert.Same(_cachedTopicRepository, ((ITopicLazyLoadable)topic!).Loader);

  }

  #endregion

  #region J: Edge Cases

  /*============================================================================================================================
  | TEST: ENSURE LOADED: NEW TOPIC: DOES NOT FETCH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="StubLazyLoadingTopicRepository.EnsureLoaded"/> directly against an in-memory <see cref="Topic"/>
  ///   attached to nothing, and confirms no fetch is recorded. Distinct from the gating case covered in <c>TopicTest</c>, this
  ///   is asserted through the repository's own spy, not merely the absence of a resolver call.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_NewTopic_DoesNotFetch() {

    var topic                   = new Topic("Test", "Page");

    await _loadingTopicRepository.EnsureLoaded(topic, TopicPayload.Children, cancellationToken: CancellationToken);

    Assert.Equal(0, _loadingTopicRepository.TotalFetches);

  }

  #endregion

  #region K: In-Graph Association Resolution

  /*============================================================================================================================
  | TEST: ENSURE LOADED: TARGETS RESIDENT IN GRAPH: RESOLVE AND CLEAR DEFERRED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a target topic into the repository's graph ahead of time, then loads a second topic whose deferred relationship
  ///   and reference entries point at it, and confirms that <see cref=
  ///   "ITopicLazyLoadable.EnsureLoaded(TopicPayload,System.Threading.CancellationToken)"/> connects both associations to the
  ///   in-graph instance and clears their <c>Deferred</c> entries, without either target needing to be (re)built from the
  ///   record store.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_TargetsResidentInGraph_ResolveAndClearDeferred() {

    var web00                   = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");
    var web0                    = web00!.Parent;
    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");
    var rawTopic                = (ITopicBackingAccessor)topic!;

    await ((ITopicLazyLoadable)topic!).EnsureLoaded(
      TopicPayload.Relationships | TopicPayload.References,
      cancellationToken: CancellationToken
    );

    var related                 = topic.Relationships.GetValues("Related");

    Assert.Single(related);
    Assert.Same(web00, related[0]);
    Assert.Same(web0, topic.References["BaseTopic"].Value);
    Assert.Empty(rawTopic.Relationships.Deferred);
    Assert.Empty(rawTopic.References.Deferred);

  }

  #endregion

  #region L: Sufficiency-Gated Cache Hits

  /*============================================================================================================================
  | TEST: LOAD: NARROW PAYLOAD HIT: TOPS UP AND CONVERGES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with the default payload, then loads it again, non-recursively, requesting <see cref=
  ///   "TopicPayload.ExtendedAttributes"/>, and confirms the second call returns the exact same, resident instance, but now
  ///   filled with the extended attribute value, and not merely a flipped <see cref="LoadState"/>.
  /// </summary>
  [Fact]
  public async Task Load_NarrowPayloadHit_TopsUpAndConverges() {

    var topic                   = await _cachedTopicRepository.Load("Root:Web:Web_0:Web_0_0");

    Assert.False(((ITopicLazyLoadable)topic!).IsLoaded(TopicPayload.ExtendedAttributes));

    var reloaded                 = await _cachedTopicRepository.Load(
      "Root:Web:Web_0:Web_0_0",
      topic,
      false,
      TopicPayload.ExtendedAttributes
    );

    Assert.Same(topic, reloaded);
    Assert.True(((ITopicLazyLoadable)reloaded!).IsLoaded(TopicPayload.ExtendedAttributes));
    Assert.Equal("Extended body content for Web_0_0.", reloaded.Attributes.GetValue("Body"));

  }

  /*============================================================================================================================
  | TEST: LOAD: RECURSIVE HIT: CONVERGES SUBTREE THEN CLEAN HIT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a subtree recursively with the full payload, then repeats the identical call, and confirms the second call is a
  ///   genuine, converged hit: The same instance is returned and no further fetches are recorded against the underlying <see
  ///   cref="StubLazyLoadingTopicRepository"/>, proving <see cref="LoadState"/> converged on every resident descendant rather
  ///   than merely on the seed.
  /// </summary>
  [Fact]
  public async Task Load_RecursiveHit_ConvergesSubtreeThenCleanHit() {

    var stub                    = new StubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(stub);
    var gate                    = TopicPayload.All & ~(TopicPayload.Relationships | TopicPayload.References);

    var seed                    = await cache.Load("Root:Web:Web_0", null, true, TopicPayload.All);

    Assert.True(((ITopicLazyLoadable)seed!).IsLoaded(gate, isRecursive: true));

    var fetchesAfterFirstLoad   = stub.TotalFetches;
    var reloaded                = await cache.Load("Root:Web:Web_0", null, true, TopicPayload.All);

    Assert.Same(seed, reloaded);
    Assert.Equal(fetchesAfterFirstLoad, stub.TotalFetches);

  }

  /*============================================================================================================================
  | TEST: LOAD: RECURSIVE TOP UP ON RESIDENT SEED: ANCESTORS STAY NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a deep, shallow seed, then tops it up recursively for <see cref="TopicPayload.Children"/> and <see cref=
  ///   "TopicPayload.ExtendedAttributes"/>, and confirms the seed and its descendants converge while the seed's ascendant
  ///   remains <see cref="LoadState.NotLoaded"/>, matching the parent plan distinction of ascendants vs. seed graph.
  /// </summary>
  [Fact]
  public async Task Load_RecursiveTopUpOnResidentSeed_AncestorsStayNotLoaded() {

    var seed                    = await _cachedTopicRepository.Load("Root:Web:Web_0:Web_0_0");

    Assert.False(((ITopicLazyLoadable)seed!).IsLoaded(TopicPayload.Children));

    var deep                    = await _cachedTopicRepository.Load(
      "Root:Web:Web_0:Web_0_0",
      seed,
      true,
      TopicPayload.Children | TopicPayload.ExtendedAttributes
    );

    var ancestor                 = deep!.Parent;

    Assert.Same(seed, deep);
    Assert.True(
      ((ITopicLazyLoadable)deep).IsLoaded(TopicPayload.Children | TopicPayload.ExtendedAttributes, isRecursive: true)
    );
    Assert.Equal("Extended body content for Web_0_0.", deep.Attributes.GetValue("Body"));
    Assert.False(((ITopicLazyLoadable)ancestor!).IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: LOAD: RECURSIVE TOP UP: IN-GRAPH CORE CONNECTS MERGED REGION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic with deferred relationship and reference targets that are not yet loaded, then recursively tops up its
  ///   shared ancestor so both targets are pulled into the same merged graph, and confirms the in-graph association are
  ///   correctly connected and clearing <c>Deferred</c> without a further round-trip dedicated to associations.
  /// </summary>
  [Fact]
  public async Task Load_RecursiveTopUp_InGraphCoreConnectsMergedRegion() {

    var stub                    = new StubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(stub);

    var topic                   = await cache.Load("Root:Web:Web_1");
    var rawTopic                = (ITopicBackingAccessor)topic!;

    Assert.NotEmpty(rawTopic.Relationships.Deferred);
    Assert.NotEmpty(rawTopic.References.Deferred);

    var web                     = await cache.Load("Root:Web", null, true, TopicPayload.Children);
    var web00                   = web!.Children["Web_0"].Children["Web_0_0"];
    var related                 = topic!.Relationships.GetValues("Related");

    Assert.Single(related);
    Assert.Same(web00, related[0]);
    Assert.Equal("Web_0", topic.References["BaseTopic"].Value?.Key);
    Assert.Empty(rawTopic.Relationships.Deferred);
    Assert.Empty(rawTopic.References.Deferred);

  }

  /*============================================================================================================================
  | TEST: LOAD: WHOLE TREE TOP UP: MATERIALIZES THEN CLEAN HIT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Seeds the cache with the default <c>Root</c> seed established by the <see cref="CachedTopicRepository"/> constructor
  ///   (<c>Root</c> plus its immediate children, per its eager top-tier load, but not their descendants), then requests the
  ///   whole tree recursively via the <c>topicId &lt; 0</c> branch, and confirms every descendant is materialized and <see
  ///   cref="LoadState.Loaded"/>, and that a third, identical call is a genuine, converged hit against the same instance with
  ///   no further fetches, thus exercising the <see cref="StubLazyLoadingTopicRepository"/>'s own lazy <c>Root</c> boundary,
  ///   as per <see cref="ITopicRepository.Load()"/>'s documented lazy defaults, alongside <c>EnsureLoaded</c>'s whole-tree
  ///   branch.
  /// </summary>
  [Fact]
  public async Task Load_WholeTreeTopUp_MaterializesThenCleanHit() {

    var stub                    = new StubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(stub);
    var gate                    = TopicPayload.All & ~(TopicPayload.Relationships | TopicPayload.References);

    var seed                    = await cache.Load(-1, null, false, TopicPayload.None);

    Assert.True(((ITopicLazyLoadable)seed!).IsLoaded(TopicPayload.Children));

    var loaded                  = await cache.Load(-1, seed, true, TopicPayload.All);

    Assert.Same(seed, loaded);
    Assert.True(((ITopicLazyLoadable)loaded!).IsLoaded(gate, isRecursive: true));

    var web                     = loaded.Children["Web"];

    Assert.True(web.Children.Contains("Web_0"));
    Assert.True(web.Children.Contains("Web_1"));
    Assert.True(web.Children["Web_0"].Children.Contains("Web_0_0"));
    Assert.True(web.Children["Web_0"].Children["Web_0_0"].Children.Contains("Web_0_0_0"));
    Assert.Equal(
      "Extended body content for Web_0_0.",
      web.Children["Web_0"].Children["Web_0_0"].Attributes.GetValue("Body")
    );

    var fetchesAfterLoad         = stub.TotalFetches;
    var reloaded                 = await cache.Load(-1, null, true, TopicPayload.All);

    Assert.Same(loaded, reloaded);
    Assert.Equal(fetchesAfterLoad, stub.TotalFetches);

  }

  #endregion

  #region M: Deferred Dirty-State Propagation

  /*============================================================================================================================
  | TEST: ENSURE LOADED: DIRTY DEFERRED TARGET: RESOLVES AS DIRTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Marks a loaded topic's deferred relationship entry as dirty, as <see cref="TopicRepository.Rollback(Topic, DateTime)"
  ///   />'s merge does, via <see cref="DeferredAssociationCollection.ReplaceAll"/>, then resolves it via <see cref=
  ///   "ITopicLazyLoader.EnsureLoaded"/> and confirms the resolved relationship is itself marked dirty, so that a subsequent
  ///   <see cref="ITopicRepository.Save(Topic, Boolean)"/> would persist it.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_DirtyDeferredTarget_ResolvesAsDirty() {

    // The target must not yet be loaded when "Web_1" loads; otherwise Load()'s own resolution (i.e., FillRequestedPayload's
    // resolveDeferredTargets) would resolve "Related" immediately, with the default, non-dirty flag, before this test ever gets
    // a chance to restamp the entry as dirty
    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");
    var rawTopic                = (ITopicBackingAccessor)topic!;
    var targetId                = rawTopic.Relationships.Deferred.Single(d => d.Key == "Related").TopicId;

    rawTopic.Relationships.Deferred.SetValue("Related", targetId, isDirty: true);

    var target                  = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");

    await _loadingTopicRepository.EnsureLoaded(topic!, TopicPayload.Relationships, cancellationToken: CancellationToken);

    Assert.Contains(target, topic!.Relationships.GetValues("Related"));
    Assert.True(topic.Relationships.IsDirty());

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: CLEAN DEFERRED TARGET: RESOLVES AS NOT DIRTY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Resolves a loaded topic's non-dirty deferred relationship entry via <see cref="ITopicLazyLoader.EnsureLoaded"/> and
  ///   confirms the resolved relationship is not marked dirty, since it merely reflects data already present in the persistence
  ///   store, thus the counterpart to <see cref="EnsureLoaded_DirtyDeferredTarget_ResolvesAsDirty"/>.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_CleanDeferredTarget_ResolvesAsNotDirty() {

    // As in EnsureLoaded_DirtyDeferredTarget_ResolvesAsDirty, "Web_1" must load before its target, so "Related" stays deferred
    // until EnsureLoaded resolves it, rather than being eagerly resolved by Load()'s own resolution
    var topic                   = await _loadingTopicRepository.Load("Root:Web:Web_1");

    var target                  = await _loadingTopicRepository.Load("Root:Web:Web_0:Web_0_0");

    await _loadingTopicRepository.EnsureLoaded(topic!, TopicPayload.Relationships, cancellationToken: CancellationToken);

    Assert.Contains(target, topic!.Relationships.GetValues("Related"));
    Assert.False(topic.Relationships.IsDirty());

  }

  #endregion

} //Class