/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Repositories;
using OnTopic.Tests.TestDoubles;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: TOPIC LAZY LOADABLE TEST
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="ITopicLazyLoadable"/> interface, with a particular emphasis on the recursive <see
///   cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> overload.
/// </summary>
[ExcludeFromCodeCoverage]
public class ITopicLazyLoadableTest {

  /*============================================================================================================================
  | TEST: IS LOADED: NON-RECURSIVE: IGNORES UNLOADED CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic with fully loaded extended attributes but a <see cref="LoadState.NotLoaded"/> children collection.
  ///   Verifies that a non-recursive <see cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> query returns <c>true</c>
  ///   once the requested payload is satisfied, regardless of the state of <see cref="Topic.Children"/>.
  /// </summary>
  [Fact]
  public void IsLoaded_NonRecursive_IgnoresUnloadedChildren() {

    var topic                   = (ITopicLazyLoadable)new Topic("Test", "Page", null, 1) {
      Children                  = {
        LoadState               = LoadState.NotLoaded
      }
    };

    var result                  = topic.IsLoaded(TopicPayload.ExtendedAttributes, depth: 0);

    Assert.True(result);

  }

  /*============================================================================================================================
  | TEST: IS LOADED: SHALLOW SEED: RECURSIVE RETURNS FALSE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic with a single, unloaded child collection. Verifies that <see cref=
  ///   "ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> returns <c>false</c> for a recursive query, since the seed's <see
  ///   cref="Topic.Children"/> are not yet loaded.
  /// </summary>
  [Fact]
  public void IsLoaded_ShallowSeed_Recursive_ReturnsFalse() {

    var topic                   = (ITopicLazyLoadable)new Topic("Test", "Page", null, 1) {
      Children                  = {
        LoadState               = LoadState.NotLoaded
      }
    };

    Assert.False(topic.IsLoaded(TopicPayload.All, depth: -1));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: FULLY RESIDENT SUBTREE: RECURSIVE RETURNS TRUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a three-level topic hierarchy with every collection fully loaded. Verifies that <see
  ///   cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> returns <c>true</c> once the whole subtree is loaded.
  /// </summary>
  [Fact]
  public void IsLoaded_FullyResidentSubtree_Recursive_ReturnsTrue() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    var child                   = new Topic("Child", "Page", parent, 2);
    _                           = new Topic("Grandchild", "Page", child, 3);

    Assert.True(((ITopicLazyLoadable)parent).IsLoaded(TopicPayload.All, depth: -1));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: NOT LOADED DESCENDANT: RECURSIVE RETURNS FALSE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a three-level topic hierarchy where the middle topic's extended attributes are <see cref=
  ///   "LoadState.NotLoaded"/>. Verifies that <see cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> returns
  ///   <c>false</c> for a recursive query, even though the seed and its <see cref="Topic.Children"/> collection are fully
  ///   loaded.
  /// </summary>
  [Fact]
  public void IsLoaded_NotLoadedDescendant_Recursive_ReturnsFalse() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    var child                   = new Topic("Child", "Page", parent, 2);
    _                           = new Topic("Grandchild", "Page", child, 3);

    child.Attributes.LoadState  = LoadState.NotLoaded;

    Assert.False(((ITopicLazyLoadable)parent).IsLoaded(TopicPayload.ExtendedAttributes, depth: -1));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: NOT LOADED CHILDREN: EXCLUDED PAYLOAD: RECURSIVE RETURNS FALSE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a two-level topic hierarchy where the seed's <see cref="Topic.Children"/> collection is <see cref=
  ///   "LoadState.NotLoaded"/>, and queries a <c>payload</c> parameter that excludes <see cref="TopicPayload.Children"/>.
  ///   Verifies that <see cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> still returns <c>false</c>, confirming
  ///   that the children gate is evaluated independently of the requested payload before recursing.
  /// </summary>
  [Fact]
  public void IsLoaded_NotLoadedChildren_ExcludedPayload_Recursive_ReturnsFalse() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    _                           = new Topic("Child", "Page", parent, 2);

    parent.Children.LoadState   = LoadState.NotLoaded;

    var result                  = ((ITopicLazyLoadable)parent).IsLoaded(TopicPayload.ExtendedAttributes, depth: -1);

    Assert.False(result);

  }

  /*============================================================================================================================
  | TEST: IS LOADED: NOT LOADED CHILDREN: NEVER TRIGGERS A LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic stamped with a <see cref="TrackingTopicLazyLoader"/> and a <see cref="LoadState.NotLoaded"/> children
  ///   collection. Verifies that <see cref="ITopicLazyLoadable.IsLoaded(TopicPayload, Int32)"/> reads <see cref="LoadState"/>
  ///   directly and returns <c>false</c> without triggering a lazy load of <see cref="Topic.Children"/>.
  /// </summary>
  [Fact]
  public void IsLoaded_NotLoadedChildren_NeverTriggersLoad() {

    var topic                   = new Topic("Test", "Page", null, 1);
    var rawTopic                = (ITopicLazyLoadable)topic;
    var loader                  = new TrackingTopicLazyLoader();

    rawTopic.Loader             = loader;
    topic.Children.LoadState    = LoadState.NotLoaded;

    var result                  = rawTopic.IsLoaded(TopicPayload.All, depth: -1);

    Assert.False(result);
    Assert.False(loader.WasCalled);

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: NULL RESOLVER: DOES NOT THROW
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="ITopicLazyLoadable.EnsureLoaded(TopicPayload, CancellationToken)"/> on an in-memory topic with no
  ///   loader and confirms it completes without throwing.
  /// </summary>
  [Fact]
  public void EnsureLoaded_NullResolver_DoesNotThrow() {
    var topic                   = new Topic("Topic", "Page");
    ((ITopicLazyLoadable)topic).EnsureLoaded(TopicPayload.All);
  }

  /*============================================================================================================================
  | TEST: IS NEW: NEW TOPIC: HAS NULL LOADER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Confirms that a newly constructed, unsaved <see cref="Topic"/> carries a null <see cref="ITopicLazyLoadable.Loader"/>.
  /// </summary>
  /// <remarks>
  ///   Ensures that <see cref="Repositories.LazyLoadingTopicRepository"/> only stamps <see cref="ITopicLazyLoadable.Loader"/>
  ///   once a topic has been loaded or saved (and thus has a stable <see cref="Topic.Id"/>), so an in-memory, unsaved topic can
  ///   never carry one.
  /// </remarks>
  [Fact]
  public void IsNew_NewTopic_HasNullLoader() {
    var topic                   = new Topic("Topic", "Page"); // ID = -1, IsNew = true
    Assert.True(topic.IsNew);
    Assert.Null(((ITopicLazyLoadable)topic).Loader);
  }

} //Class