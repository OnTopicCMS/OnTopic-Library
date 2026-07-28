/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Data;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Data.Caching;
using OnTopic.Data.Sql;
using OnTopic.Metadata;
using OnTopic.Querying;
using OnTopic.Repositories;
using OnTopic.TestDoubles.LazyLoading;
using OnTopic.Tests.TestDoubles;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: CACHED TOPIC REPOSITORY TESTS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="CachedTopicRepository"/> class.
/// </summary>
/// <remarks>
///   These tests drive a <see cref="FakeSqlTopicRepository"/>, which is a minimal inner <see cref="ITopicRepository"/> that
///   calls the real, production <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> against in-memory <see cref="DataTable"/>
///   rows, exactly as <see cref="SqlTopicRepository"/> does against a live SQL data reader, rather than <see cref=
///   "StubLazyLoadingTopicRepository"/>, which deliberately ignores <c>referenceTopic</c>, and thus cannot distinguish a <see
///   langword="null"/> <c>referenceTopic</c> from <c>_cache</c>: The very two defects the <c>referenceTopic ?? _cache</c>
///   fallback resolves are intrinsic to <see cref="SqlDataReaderExtensions.LoadTopicGraph"/>'s <c>referenceTopic</c>-seeded
///   working index, which only <see cref="FakeSqlTopicRepository"/> exercises.
/// </remarks>
[ExcludeFromCodeCoverage]
public class CachedTopicRepositoryTest {

  /*============================================================================================================================
  | PROPERTY: CANCELLATION TOKEN
  \---------------------------------------------------------------------------------------------------------------------------*/
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  /*============================================================================================================================
  | TEST: LOAD: COLD MISS WITH RESIDENT PARENT: ATTACHES TO RESIDENT INSTANCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic ("Web") non-recursively, leaving <see cref="Topic.Children"/> <see cref="LoadState.NotLoaded"/>, then
  ///   loads a direct child ("Web_0") by ID, confirming the child attaches to the existing "Web" instance, populated via its
  ///   <see cref="Topic.Children"/>, with a matching <see cref="Topic.Parent"/>, rather than dangling off a disconnected
  ///   duplicate.
  /// </summary>
  [Fact]
  public async Task Load_ColdMissWithResidentParent_AttachesToResidentInstance() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");
    var web                     = await cache.Load("Web");

    Assert.False(((ITopicLazyLoadable)web!).IsLoaded(TopicPayload.Children));

    var web0                    = await cache.Load(3);

    Assert.NotNull(web0);
    Assert.Same(web, web0.Parent);
    Assert.Contains(web!.Children, child => child.Id == web0.Id);
    Assert.Same(root, web0.GetRootTopic());

  }

  /*============================================================================================================================
  | TEST: LOAD: COLD MISS TWO LEVELS BELOW RESIDENT ANCESTOR: ATTACHES WHOLE CHAIN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Cold-loads a topic ("Web_0_0") two levels below the deepest resident ancestor ("Web") and confirms the entire new
  ///   intermediate chain ("Web_0") attaches under the resident ancestor rather than duplicating it.
  /// </summary>
  [Fact]
  public async Task Load_ColdMissTwoLevelsBelowResidentAncestor_AttachesWholeChain() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2)
      .AddTopic(4, "Web_0_0", "Page", 3);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");
    var web                     = await cache.Load("Web");

    var web00                   = await cache.Load(4);

    Assert.NotNull(web00);
    Assert.Equal("Web_0", web00.Parent?.Key);
    Assert.Same(web, web00.Parent?.Parent);
    Assert.Contains(web!.Children, child => child.Key == "Web_0");
    Assert.Same(root, web00.GetRootTopic());

  }

  /*============================================================================================================================
  | TEST: LOAD: COLD MISS TWO LEVELS BELOW RESIDENT ANCESTOR: INDEXES INTERMEDIATE FOR KEY HIT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Cold-loads a topic ("Web_0_0") two levels below the deepest loaded ancestor ("Web"), pulling the intermediate ("Web_0")
  ///   into the graph, then requests that intermediate by its unique key and confirms it resolves from the cache's flat key
  ///   index as a pure hit, with no fall-through to the inner repository. This exercises the ancestor crawl implemented in <see
  ///   cref="CachedTopicRepository.OnTopicLoaded(TopicLoadEventArgs)"/>, which indexes ascendants that the downward-only <see
  ///   cref="TopicExtensions.FindAll(Topic)"/> cannot reach.
  /// </summary>
  [Fact]
  public async Task Load_ColdMissTwoLevelsBelowResidentAncestor_IndexesIntermediateForKeyHit() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2)
      .AddTopic(4, "Web_0_0", "Page", 3);

    var cache                   = new CachedTopicRepository(inner);
    await cache.Load("Web");

    // Cold-load the grandchild, pulling the not-yet-loaded intermediate "Web_0" in as an ancestor
    var leaf                    = await cache.Load(4);
    var intermediate            = leaf!.Parent;

    Assert.Equal("Web_0", intermediate?.Key);

    // Count loads against the inner repository; an index hit for the intermediate makes no such round-trip
    var innerLoads              = 0;
    inner.TopicLoaded           += (_, _) => innerLoads++;

    var resolved                = await cache.Load("Root:Web:Web_0");

    Assert.Same(intermediate, resolved);
    Assert.Equal(0, innerLoads);

  }

  /*============================================================================================================================
  | TEST: LOAD: AFTER ATTACHED-BUT-UNINDEXED SUBTREE: RETURNS ATTACHED INSTANCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Simulates the shape produced by <see cref="ITopicRepository.Refresh(Topic, DateTime)"/>, which attaches new topics to an
  ///   existing parent without raising <see cref="ITopicRepository.TopicLoaded"/> (and thus without indexing them), then calls
  ///   <see cref="CachedTopicRepository.Load(int, Topic?, TopicPayload, int)"/> for the leaf and confirms the cache returns
  ///   the existing attached instances rather than duplicating them. The merge-aware underlying load reuses the loaded topics
  ///   via the reference graph, and <see cref="CachedTopicRepository.OnTopicLoaded(TopicLoadEventArgs)"/> indexes both the leaf
  ///   and any previously unindexed intermediate ancestor ("Web_0") by walking up the parent chain.
  /// </summary>
  [Fact]
  public async Task Load_AfterAttachedButUnindexedSubtree_ReturnsAttachedInstance() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2)
      .AddTopic(4, "Web_0_0", "Page", 3);

    var cache                   = new CachedTopicRepository(inner);
    var web                     = await cache.Load("Web");

    // Attach a two-level subtree directly, bypassing Load()/TopicLoaded—mirroring Refresh()'s attach-without-index shape
    var newWeb0                 = new Topic("Web_0", "Page", web, 3);
    var newWeb00                = new Topic("Web_0_0", "Page", newWeb0, 4);

    var loaded                  = await cache.Load(4);

    Assert.Same(newWeb00, loaded);
    Assert.Same(newWeb0, loaded?.Parent);
    Assert.Same(web, loaded?.Parent?.Parent);

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: DEFERRED ASSOCIATION FALLBACK WITH RESIDENT TARGET PARENT: ATTACHES RESOLVED TARGET
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Confirms that a deferred-association fallback (<see cref="LazyLoadingTopicRepository.LoadDeferredAssociations"/>
  ///   calling <c>Load(targetId)</c> with default, <see langword="null"/> parameters) resolves a target whose direct parent
  ///   is already resident—the same shape as Defect 1—so the resolved instance attaches to the live cache graph rather than
  ///   arriving dangling.
  /// </summary>
  [Fact]
  public async Task TaskEnsureLoaded_DeferredAssociationFallbackWithResidentTargetParent_AttachesToCacheGraph() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2)
      .AddTopic(4, "Web_0_0", "Page", 3)
      .AddTopic(5, "Web_1", "Page", 2);

    inner.AddRelationship(5, "Related", 4);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");

    // Establish "Web_0" (id 3) as resident—the direct parent of the deferred target
    var web0                    = await cache.Load("Web:Web_0");

    var web1                    = await cache.Load("Web:Web_1");
    var rawWeb1                 = (ITopicBackingAccessor)web1!;

    Assert.NotEmpty(rawWeb1.Relationships.Deferred);

    await ((ITopicLazyLoadable)web1!).EnsureLoaded(TopicPayload.Relationships, cancellationToken: CancellationToken);

    var related                 = web1.Relationships.GetValues("Related").Single();

    Assert.Same(web0, related.Parent);
    Assert.Contains(web0!.Children, child => child.Id == related.Id);
    Assert.Same(root, related.GetRootTopic());

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: CONCURRENT CHILDREN REQUESTS: FETCHES ONCE WITHOUT CORRUPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Reproduces a concurrent-read race on a shared <see cref="LoadState.NotLoaded"/> <see cref="Topic.Children"/> property:
  ///   Two concurrent <see cref="ITopicLazyLoadable.EnsureLoaded"/> requests for the same cached topic must merge exactly once,
  ///   not twice, into the shared <see cref="KeyedTopicCollection"/>.
  /// </summary>
  /// <remarks>
  ///   Uses <see cref="BlockingStubLazyLoadingTopicRepository"/>, which suspends inside its own <c>EnsureLoaded</c> until
  ///   released, to interleave both requests without any <see cref="Thread.Sleep(int)"/> or other timing hack: The first
  ///   request is proven in flight because it is the one suspended on the gate; the second is proven in flight because calling
  ///   it synchronously (i.e., without <see langword="await"/>) before releasing the gate runs against to its own suspension
  ///   before the test proceeds.
  /// </remarks>
  [Fact]
  public async Task EnsureLoaded_ConcurrentChildrenRequests_FetchesOnceWithoutCorruption() {

    var inner                   = new BlockingStubLazyLoadingTopicRepository();
    var cache                   = new CachedTopicRepository(inner);
    var web                     = await cache.Load("Web");
    var rawWeb                  = (ITopicLazyLoadable)web!;

    Assert.False(rawWeb.IsLoaded(TopicPayload.Children));

    // "Arm" the gate so the first request suspends mid-fetch, then launch both requests without awaiting either
    inner.ArmGate();

    var firstRequest            = rawWeb.EnsureLoaded(TopicPayload.Children, CancellationToken);
    var secondRequest           = rawWeb.EnsureLoaded(TopicPayload.Children, CancellationToken);

    // Release the gate and let both requests run to completion
    inner.ReleaseGate();

    await Task.WhenAll(firstRequest, secondRequest);

    // A single inner fetch, no duplicate children, and a fully loaded boundary confirm the race did not corrupt the merge
    Assert.Equal(1, inner.FetchCount);
    Assert.True(rawWeb.IsLoaded(TopicPayload.Children));
    Assert.Equal(2, web.Children.Count);
    Assert.Equal(2, web.Children.Select(child => child.Id).Distinct().Count());

  }

  /*============================================================================================================================
  | TEST: DELETE: LOADED TOPIC: SUBSEQUENT LOAD DOES NOT RETURN DETACHED INSTANCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a loaded topic and confirms it is absent from <see cref="TopicExtensions.GetLiveTopicIndex(Topic)"/>; i.e., the
  ///   detach hook in <see cref="TopicIndexRegistry"/> pruned it, since <see cref="CachedTopicRepository.OnTopicDeleted"/> no
  ///   longer does so.
  /// </summary>
  /// <remarks>
  ///   <see cref="FakeSqlTopicRepository"/>'s <c>DeleteTopic</c> doesn't actually do anything: The row survives in its store,
  ///   unlike a real deletion. That's exploited for the follow-up assertions: A subsequent <see cref="CachedTopicRepository"/>
  ///   <c>Load()</c> by ID and then by unique key, both still find a row and reattach a fresh instance. If either index had
  ///   retained a stale entry instead of being pruned by e.g., the detach hook <see cref="CachedTopicRepository.OnTopicDeleted"
  ///   />, that lookup would have returned the old, now-detached instance directly, without ever falling through to the inner
  ///   repository, so the fresh instances are themselves evidence both stale entries are gone, even though this isn't how a
  ///   real repository handles a delete.
  /// </remarks>
  [Fact]
  public async Task Delete_LoadedTopic_SubsequentLoadDoesNotReturnDetachedInstance() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null)
      .AddTopic(2, "Web", "Page", 1)
      .AddTopic(3, "Web_0", "Page", 2);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");
    var web                     = await cache.Load("Web");
    var web0                    = await cache.Load("Web:Web_0");

    Assert.Contains(web!.Children, child => child.Id == web0!.Id);

    await cache.Delete(web0!, isRecursive: false);

    Assert.DoesNotContain(web.Children, child => child.Id == web0!.Id);
    Assert.False(root!.GetLiveTopicIndex().ContainsKey(web0!.Id));

    var reloadedById            = await cache.Load(web0!.Id);

    Assert.NotSame(web0, reloadedById);

    var reloadedByKey           = await cache.Load("Web:Web_0");

    Assert.NotSame(web0, reloadedByKey);

  }

  /*============================================================================================================================
  | TEST: SAVE: NEW TOPIC: RESOLVES VIA LOAD WITHOUT FALLBACK
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a newly created, unsaved <see cref="Topic"/> under a resident parent, then calls <see cref=
  ///   "CachedTopicRepository.Load(int, Topic?, TopicPayload, int)"/> for its newly assigned ID and confirms the same
  ///   instance is returned directly from the live index, with no fall-through to the inner repository. Ensures the <see cref=
  ///   "Topic.Id"/> setter's indexing hook from <see cref="TopicIndexRegistry"/> makes a freshly saved topic resolvable by ID.
  /// </summary>
  [Fact]
  public async Task Save_NewTopic_ResolvesViaLoadWithoutFallback() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");

    // Establish a minimal content type graph directly on the live root, required by Save()'s content type validation
    var configuration           = new Topic("Configuration", "Container", root);
    var contentTypes            = new ContentTypeDescriptor("ContentTypes", "ContentTypeDescriptor", configuration);

    _                           = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", contentTypes);

    var newChild                = new Topic("NewChild", "Page", root);

    Assert.True(newChild.IsNew);

    await cache.Save(newChild);

    Assert.False(newChild.IsNew);

    var loaded                  = await cache.Load(newChild.Id);

    Assert.Same(newChild, loaded);

  }

  /*============================================================================================================================
  | TEST: SAVE: RECURSIVE NEW TOPICS: RESOLVE VIA LOAD WITHOUT FALLBACK
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Recursively saves a newly created parent with a newly created child underneath it, then calls <see cref=
  ///   "CachedTopicRepository.Load(int, Topic?, TopicPayload, int)"/> for each of their newly assigned IDs and confirms both
  ///   resolve directly from the live index, with no fall-through to the inner repository.
  /// </summary>
  /// <remarks>
  ///   Unlike <see cref="Save_NewTopic_ResolvesViaLoadWithoutFallback"/>, which saves a single topic, this ensures that the
  ///   <see cref="Topic.Id"/> setter's indexing hook also fires correctly for a child of a parent that was itself just assigned
  ///   an ID moments earlier in the same recursive save; i.e., <see cref="TopicExtensions.GetRootTopic(Topic)"/>, walked from
  ///   the child at the moment its own ID is assigned, correctly reaches <c>_cache</c> through the freshly attached parent.
  /// </remarks>
  [Fact]
  public async Task Save_RecursiveNewTopics_ResolveViaLoadWithoutFallback() {

    var inner                   = new FakeSqlTopicRepository()
      .AddTopic(1, "Root", "Container", null);

    var cache                   = new CachedTopicRepository(inner);
    var root                    = await cache.Load("Root");

    // Establish a minimal content type graph directly on the live root, required by Save()'s content type validation
    var configuration           = new Topic("Configuration", "Container", root);
    var contentTypes            = new ContentTypeDescriptor("ContentTypes", "ContentTypeDescriptor", configuration);

    _                           = new ContentTypeDescriptor("Page", "ContentTypeDescriptor", contentTypes);

    var newParent               = new Topic("NewParent", "Page", root);
    var newChild                = new Topic("NewChild", "Page", newParent);

    await cache.Save(newParent, isRecursive: true);

    Assert.False(newParent.IsNew);
    Assert.False(newChild.IsNew);

    var loadedParent            = await cache.Load(newParent.Id);
    var loadedChild             = await cache.Load(newChild.Id);

    Assert.Same(newParent, loadedParent);
    Assert.Same(newChild, loadedChild);

  }

} //Class