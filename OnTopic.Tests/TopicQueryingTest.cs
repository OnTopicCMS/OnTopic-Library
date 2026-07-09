/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections;
using OnTopic.Data.Caching;
using OnTopic.Metadata;
using OnTopic.Querying;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.Tests.Fixtures;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: TOPIC QUERYING TEST
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="TopicExtensions"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
[Xunit.Collection("Shared Repository")]
public class TopicQueryingTest  {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      ITopicRepository                _topicRepository;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicQueryingTest"/> with shared resources.
  /// </summary>
  /// <remarks>
  ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
  ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
  ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
  ///   crawling the object graph.
  /// </remarks>
  public TopicQueryingTest(TopicInfrastructureFixture<StubTopicRepository> fixture) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(fixture,  nameof(fixture));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish dependencies
    \-------------------------------------------------------------------------------------------------------------------------*/
    _topicRepository = fixture.CachedTopicRepository;

  }

  /*============================================================================================================================
  | TEST: FIND ALL BY ATTRIBUTE: RETURNS CORRECT TOPICS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Looks for a deeply nested child topic using only the attribute value.
  /// </summary>
  [Fact]
  public void FindAllByAttribute_ReturnsCorrectTopics() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);
    var grandChildTopic         = new Topic("GrandChildTopic", "Page", childTopic, 20);
    var grandNieceTopic         = new Topic("GrandNieceTopic", "Page", childTopic, 3);
    var greatGrandChildTopic    = new Topic("GreatGrandChildTopic", "Page", grandChildTopic, 7);

    grandChildTopic.Attributes.SetValue("Foo", "Baz");
    grandNieceTopic.Attributes.SetValue("Foo", "Bar");
    greatGrandChildTopic.Attributes.SetValue("Foo", "Bar");

    Assert.Equal(greatGrandChildTopic, parentTopic.FindAllByAttribute("Foo", "Bar").First());
    Assert.Equal(2, parentTopic.FindAllByAttribute("Foo", "Bar").Count);
    Assert.Equal(grandChildTopic, parentTopic.FindAllByAttribute("Foo", "Baz").First());

  }

  /*============================================================================================================================
  | TEST: FIND FIRST PARENT: RETURNS CORRECT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a deeply nested <see cref="Topic"/>, returns the first parent <see cref="Topic"/> which satisfies a delegate.
  /// </summary>
  [Fact]
  public void FindFirstParent_ReturnsCorrectTopic() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);
    var grandChildTopic         = new Topic("GrandChildTopic", "Page", childTopic, 20);
    var greatGrandChildTopic    = new Topic("GreatGrandChildTopic", "Page", grandChildTopic, 7);

    var foundTopic              = greatGrandChildTopic.FindFirstParent(t => t.Id is 5);

    Assert.Equal(childTopic, foundTopic);

  }

  /*============================================================================================================================
  | TEST: FIND FIRST PARENT: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Correctly returns null if the delegate cannot be satisfied.
  /// </summary>
  [Fact]
  public void FindFirstParent_ReturnsNull() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);

    var foundTopic              = childTopic.FindFirstParent(t => t.Id is 10);

    Assert.Null(foundTopic);

  }

  /*============================================================================================================================
  | TEST: GET ROOT TOPIC: RETURNS ROOT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a deeply nested <see cref="Topic"/>, returns the root <see cref="Topic"/>.
  /// </summary>
  [Fact]
  public void GetRootTopic_ReturnsRootTopic() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);
    var grandChildTopic         = new Topic("GrandChildTopic", "Page", childTopic, 20);
    var greatGrandChildTopic    = new Topic("GreatGrandChildTopic", "Page", grandChildTopic, 7);

    var rootTopic               = greatGrandChildTopic.GetRootTopic();

    Assert.Equal(parentTopic, rootTopic);

  }

  /*============================================================================================================================
  | TEST: GET ROOT TOPIC: RETURNS CURRENT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a root <see cref="Topic"/>, returns the current <see cref="Topic"/>.
  /// </summary>
  [Fact]
  public void GetRootTopic_ReturnsCurrentTopic() {

    var topic                   = new Topic("ParentTopic", "Page", null, 1);

    var rootTopic               = topic.GetRootTopic();

    Assert.Equal(topic, rootTopic);

  }

  /*============================================================================================================================
  | TEST: GET BY UNIQUE KEY: ROOT KEY: RETURNS ROOT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a root <see cref="Topic"/>, returns the root <see cref="Topic"/>.
  /// </summary>
  [Fact]
  public void GetByUniqueKey_RootKey_ReturnsRootTopic() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    _                           = new Topic("ChildTopic", "Page", parentTopic, 2);

    var foundTopic = parentTopic.GetByUniqueKey("ParentTopic");

    Assert.NotNull(foundTopic);
    Assert.Equal(parentTopic, foundTopic);

  }

  /*============================================================================================================================
  | TEST: GET BY UNIQUE KEY: VALID KEY: RETURNS TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a deeply nested <see cref="Topic"/>, returns the expected <see cref="Topic"/>.
  /// </summary>
  [Fact]
  public void GetByUniqueKey_ValidKey_ReturnsTopic() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);
    var grandChildTopic         = new Topic("GrandChildTopic", "Page", childTopic, 20);
    var greatGrandChildTopic1 = new Topic("GreatGrandChildTopic1", "Page", grandChildTopic, 7);
    var greatGrandChildTopic2 = new Topic("GreatGrandChildTopic2", "Page", grandChildTopic, 7);

    var foundTopic = greatGrandChildTopic1.GetByUniqueKey("ParentTopic:ChildTopic:GrandChildTopic:GreatGrandChildTopic2");

    Assert.Equal(greatGrandChildTopic2, foundTopic);

  }

  /*============================================================================================================================
  | TEST: GET BY UNIQUE KEY: INVALID KEY: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given an invalid <c>UniqueKey</c>, the <see cref="TopicExtensions.GetByUniqueKey(Topic, String)"/> returns
  ///   <c>null</c>.
  /// </summary>
  [Fact]
  public void GetByUniqueKey_InvalidKey_ReturnsNull() {

    var parentTopic             = new Topic("ParentTopic", "Page", null, 1);
    var childTopic              = new Topic("ChildTopic", "Page", parentTopic, 5);
    var grandChildTopic         = new Topic("GrandChildTopic", "Page", childTopic, 20);
    var greatGrandChildTopic    = new Topic("GreatGrandChildTopic", "Page", grandChildTopic, 7);

    var foundTopic = greatGrandChildTopic.GetByUniqueKey("ParentTopic:ChildTopic:GrandChildTopic:GreatGrandChildTopic2");

    Assert.Null(foundTopic);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE: VALID CONTENT TYPE: RETURNS CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a deeply nested <see cref="Topic"/>, returns the expected <see cref="ContentTypeDescriptor"/>.
  /// </summary>
  [Fact]
  public async Task GetContentType_ValidContentType_ReturnsContentType() {

    var topic                   = await _topicRepository.Load(11111);
    var contentTypeDescriptor = topic?.GetContentTypeDescriptor();

    Assert.NotNull(contentTypeDescriptor);
    Assert.Equal("Page", contentTypeDescriptor?.Key);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE: INVALID CONTENT TYPE: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given an invalid <see cref="ContentTypeDescriptor"/>, the <see cref="TopicExtensions.GetContentTypeDescriptor(Topic)"
  ///   /> returns <c>null</c>.
  /// </summary>
  [Fact]
  public async Task GetContentType_InvalidContentType_ReturnsNull() {

    var parentTopic             = await _topicRepository.Load(11111);
    var topic                   = new Topic("Test", "NonExistent", parentTopic);
    var contentTypeDescriptor = topic.GetContentTypeDescriptor();

    Assert.Null(contentTypeDescriptor);

    //Revert state
    await _topicRepository.Delete(topic);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE: INVALID TYPE: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given an invalid <see cref="ContentTypeDescriptor"/>, the <see cref="TopicExtensions.GetContentTypeDescriptor(Topic)"
  ///   /> returns <c>null</c>.
  /// </summary>
  /// <remarks>
  ///   This varies from <see cref="GetContentType_InvalidContentType_ReturnsNull()"/> in that it returns a valid <see cref="
  ///   Topic"/> which doesn't derive from <see cref="ContentTypeDescriptor"/>.
  /// </remarks>
  [Fact]
  public async Task GetContentType_InvalidType_ReturnsNull() {

    var parentTopic             = await _topicRepository.Load(11111);
    var topic                   = new Topic("Test", "Title", parentTopic);
    var contentTypeDescriptor = topic.GetContentTypeDescriptor();

    Assert.Null(contentTypeDescriptor);

    //Revert state
    await _topicRepository.Delete(topic);

  }

  /*============================================================================================================================
  | TEST: ANY DIRTY: DIRTY COLLECTION: RETURN TRUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="TopicCollection"/> with at least one <see cref="Topic"/> that <see cref="Topic.IsDirty(String)"/>,
  ///   returns <c>true</c>.
  /// </summary>
  [Fact]
  public void AnyDirty_DirtyCollection_ReturnTrue() {

    var topics = new TopicCollection {
      new Topic("Test", "Page")
    };

    Assert.True(topics.AnyDirty());

  }

  /*============================================================================================================================
  | TEST: ANY DIRTY: CLEAN COLLECTION: RETURN FALSE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="TopicCollection"/> with no <see cref="Topic"/>s that are <see cref="Topic.IsDirty(String)"/>,
  ///   returns <c>false</c>.
  /// </summary>
  [Fact]
  public void AnyDirty_CleanCollection_ReturnFalse() {

    var topics = new TopicCollection {
      new Topic("Test", "Page", null, 1)
    };

    Assert.False(topics.AnyDirty());

  }

  /*============================================================================================================================
  | TEST: ANY NEW: CONTAINS NEW: RETURN TRUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="TopicCollection"/> with at least one <see cref="Topic"/> that <see cref="Topic.IsNew"/>, returns
  ///   <c>true</c>.
  /// </summary>
  [Fact]
  public void AnyNew_ContainsNew_ReturnTrue() {

    var topics = new TopicCollection {
      new Topic("Test", "Page")
    };

    Assert.True(topics.AnyNew());

  }

  /*============================================================================================================================
  | TEST: ANY NEW: CONTAINS EXISTING: RETURN FALSE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="TopicCollection"/> with no <see cref="Topic"/>s that are <see cref="Topic.IsNew"/>, returns <c>
  ///   false</c>.
  /// </summary>
  [Fact]
  public void AnyNew_ContainsExisting_ReturnFalse() {

    var topics = new TopicCollection {
      new Topic("Test", "Page", null, 1)
    };

    Assert.False(topics.AnyNew());

  }

  /*============================================================================================================================
  | TEST: FIND FIRST: NOT LOADED CHILD: DOES NOT DESCEND
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a three-level topic hierarchy and manually sets the middle topic's <see cref="Topic.Children"/> to <see cref=
  ///   "LoadState.NotLoaded"/>. Verifies that <see cref="TopicExtensions.FindFirst"/> stops at that node and does not return
  ///   the grandchild, which would only be reachable by descending into the not-loaded subtree.
  /// </summary>
  [Fact]
  public void FindFirst_WithNotLoadedChild_DoesNotDescend() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    var child                   = new Topic("Child", "Page", parent, 2);
    var grandchild              = new Topic("Grandchild", "Page", child, 3);

    child.Children.LoadState    = LoadState.NotLoaded;

    var result                  = parent.FindFirst(t => t == grandchild);

    Assert.Null(result);

  }

  /*============================================================================================================================
  | TEST: FIND ALL: PARTIALLY LOADED GRAPH: EXCLUDES NOT LOADED SUBTREES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a topic graph where one branch has a <see cref="LoadState.NotLoaded"/> children collection. Verifies that <see
  ///   cref="TopicExtensions.FindAll(Topic)"/> includes the not-loaded node itself (it is resident) but excludes its
  ///   descendants, which are unreachable without triggering a load.
  /// </summary>
  [Fact]
  public void FindAll_WithPartiallyLoadedGraph_ExcludesNotLoadedSubtrees() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    var childA                  = new Topic("ChildA", "Page", parent, 2);
    var childB                  = new Topic("ChildB", "Page", parent, 3);
    var grandchildA             = new Topic("GrandchildA", "Page", childA, 4);
    var grandchildB             = new Topic("GrandchildB", "Page", childB, 5);

    childB.Children.LoadState   = LoadState.NotLoaded;

    var results                 = parent.FindAll();

    Assert.Contains(parent, results);
    Assert.Contains(childA, results);
    Assert.Contains(grandchildA, results);
    Assert.Contains(childB, results);
    Assert.DoesNotContain(grandchildB, results);

  }

} //Class