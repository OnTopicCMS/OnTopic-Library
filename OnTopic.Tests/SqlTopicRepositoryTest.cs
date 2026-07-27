/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using OnTopic.Associations;
using OnTopic.Collections.Specialized;
using OnTopic.Data.Sql;
using OnTopic.Data.Sql.Models;
using OnTopic.Querying;
using OnTopic.Repositories;
using OnTopic.Tests.Schemas;
using Xunit;
using TopicReferencesDataTable  = OnTopic.Tests.Schemas.TopicReferencesDataTable;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: SQL TOPIC REPOSITORY TEST
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="SqlTopicRepository"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SqlTopicRepositoryTest {

  /*============================================================================================================================
  | PROPERTY: CANCELLATION TOKEN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Shorthand for <see cref="TestContext.Current"/>'s <see cref="TestContext.CancellationToken"/>.
  /// </summary>
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH TOPIC: RETURNS TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> record and confirms that
  ///   a topic with those values is returned.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithTopic_ReturnsTopic() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container");

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH NEW PARENT: UPDATES PARENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> record that represents a
  ///   different parent than the existing <c>referenceTopic</c> and confirms that the topic's parent is updated.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithNewParent_UpdatesParent() {

    using var topics            = new TopicsDataTable();

    var topic                   = new Topic("Root", "Container", null, 1);
    var parent1                 = new Topic("Parent1", "Container", topic, 2);
    var parent2                 = new Topic("Parent2", "Container", topic, 3);
    var child                   = new Topic("Child", "Page", parent1, 4);

    topics.AddRow(4, "Child", "Page", parent2.Id);

    using var tableReader       = new DataTableReader(topics);

    await tableReader.LoadTopicGraph(referenceTopic: topic, cancellationToken: CancellationToken);

    Assert.Equal(parent2, child.Parent);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH ATTRIBUTES: RETURNS ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with an <see cref="AttributesDataTable"/> record and confirms
  ///   that a topic with those values is returned.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithAttributes_ReturnsAttributes() {

    using var topics            = new TopicsDataTable();
    using var attributes        = new AttributesDataTable();

    topics.AddRow(1, "Root", "Container");
    attributes.AddRow(1, "Test", "Value");

    using var tableReader       = new DataTableReader([topics, attributes]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Equal("Value", topic.Attributes.GetValue("Test"));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH NULL ATTRIBUTES: REMOVES ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with an <see cref="AttributesDataTable"/> record representing
  ///   a deleted attribute and confirms that an existing reference topic with that attribute has the value removed.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithNullAttributes_RemovesAttribute() {

    using var topics            = new TopicsDataTable();
    using var attributes        = new AttributesDataTable();

    var topic                   = new Topic("Root", "Container", null, 1);

    topic.Attributes.SetValue("Test", "Initial Value");

    topics.AddRow(1, "Root", "Container");
    attributes.AddRow(1, "Test", null);

    using var tableReader       = new DataTableReader([topics, attributes]);

    await tableReader.LoadTopicGraph(referenceTopic: topic, cancellationToken: CancellationToken);

    Assert.Null(topic.Attributes.GetValue("Test"));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH RELATIONSHIP: RETURNS RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="RelationshipsDataTable"/> record and
  ///   confirms that a topic with those values is returned.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithRelationship_ReturnsRelationship() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    topics.AddRow(1, "Root", "Container");
    topics.AddRow(2, "Web", "Container", 1);
    relationships.AddRow(1, "Test", 2, false);

    using var tableReader       = new DataTableReader([topics, empty, empty, relationships]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Equal(2, topic.Relationships.GetValues("Test").FirstOrDefault()?.Id);
    Assert.True(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH MISSING RELATIONSHIP: NOT FULLY LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="RelationshipsDataTable"/> record that is
  ///   missing and confirms that <see cref="TopicRelationshipMultiMap.LoadState"/> returns <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithMissingRelationship_NotFullyLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    topics.AddRow(1, "Root", "Container");
    relationships.AddRow(1, "Test", 2, false);

    using var tableReader       = new DataTableReader([topics, empty, empty, relationships]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Empty(topic.Relationships);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH REFERENCE: RETURNS REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicReferencesDataTable"/> record and
  ///   confirms that a topic with those values is returned.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithReference_ReturnsReference() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    topics.AddRow(1, "Root", "Container");
    topics.AddRow(2, "Web", "Container", 1);
    references.AddRow(1, "Test", 2);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Equal(2, topic.References.GetValue("Test")?.Id);
    Assert.True(topic.References.IsDirty());

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH DELETED REFERENCE: REMOVES EXISTING REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicReferencesDataTable"/> record and
  ///   confirms that existing references on a reference topic are deleted if they are <c>null</c> in the <see cref=
  ///   "TopicReferencesDataTable"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithDeletedReference_RemovesExistingReference() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    var referenceTopic          = new Topic("Web", "Container", null, 1);

    referenceTopic.References.SetValue("Reference", referenceTopic);

    topics.AddRow(1, "Web", "Container");
    references.AddRow(1, "Reference", null);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    await tableReader.LoadTopicGraph(1, referenceTopic, false, cancellationToken: CancellationToken);

    Assert.Null(referenceTopic.References.GetValue("Reference"));
    Assert.Equal(LoadState.Loaded, referenceTopic.References.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH MISSING REFERENCE: NOT FULLY LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicReferencesDataTable"/> record that is
  ///   missing and confirms that <see cref="TopicReferenceCollection.LoadState"/> returns <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithMissingReference_NotFullyLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    topics.AddRow(1, "Root", "Container");
    references.AddRow(1, "Test", 2);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Empty(topic.References);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH DELETED RELATIONSHIP: REMOVES RELATIONSHIP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a deleted <see cref="RelationshipsDataTable"/> record
  ///   and confirms that it is deleted from the <c>referenceTopic</c> graph.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithDeletedRelationship_RemovesRelationship() {

    var topic                   = new Topic("Test", "Container", null, 1);
    var child                   = new Topic("Child", "Container", topic, 2);
    var related                 = new Topic("Related", "Container", topic, 3);

    child.Relationships.SetValue("Test", related);

    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    relationships.AddRow(2, "Test", 3, true);

    using var tableReader       = new DataTableReader([empty, empty, empty, relationships]);

    await tableReader.LoadTopicGraph(referenceTopic: related, cancellationToken: CancellationToken);

    Assert.Empty(topic.Relationships.GetValues("Test"));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: INDEXED-ONLY WITH RELATIONSHIP: RETURNS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> under an indexed-only load (i.e., extended attributes
  ///   deferred via <c>HasExtendedAttributes = true</c>) with a relationship whose target is resident, and confirms that
  ///   <see cref="TopicRelationshipMultiMap.LoadState"/> returns <see cref="LoadState.Loaded"/>.
  /// </summary>
  /// <remarks>
  ///   Confirms that the relationship result set is still returned and <see cref="LoadState"/> is correctly established even
  ///   when extended attributes are deferred.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_IndexedOnlyWithRelationship_ReturnsLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);
    topics.AddRow(2, "Web", "Container", 1, hasExtendedAttributes: false);
    relationships.AddRow(1, "Test", 2, false);

    using var tableReader       = new DataTableReader([topics, empty, empty, relationships]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.True(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: INDEXED-ONLY WITH MISSING RELATIONSHIP: RETURNS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> under an indexed-only load with a relationship whose target is
  ///   non-resident, and confirms that <see cref="TopicRelationshipMultiMap.LoadState"/> returns <see cref=
  ///   "LoadState.NotLoaded"/>.
  /// </summary>
  /// <remarks>
  ///   <c>LoadState.NotLoaded</c> blocks <c>DeleteUnmatched</c> on save; the missing edge is reconnected when the outer
  ///   resolver calls <c>EnsureLoaded(Relationships)</c> for the topic.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_IndexedOnlyWithMissingRelationship_ReturnsNotLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);
    relationships.AddRow(1, "Test", 99, false);

    using var tableReader       = new DataTableReader([topics, empty, empty, relationships]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: INDEXED-ONLY WITH REFERENCE: RETURNS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> under an indexed-only load with a reference whose target is
  ///   resident, and confirms that <see cref="TopicReferenceCollection.LoadState"/> returns <see cref="LoadState.Loaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_IndexedOnlyWithReference_ReturnsLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);
    topics.AddRow(2, "Web", "Container", 1, hasExtendedAttributes: false);
    references.AddRow(1, "Test", 2);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.True(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: INDEXED-ONLY WITH MISSING REFERENCE: RETURNS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> under an indexed-only load with a reference whose target is
  ///   non-resident, and confirms that <see cref="TopicReferenceCollection.LoadState"/> returns <see cref=
  ///   "LoadState.NotLoaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_IndexedOnlyWithMissingReference_ReturnsNotLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);
    references.AddRow(1, "Test", 99);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: PRE-EXISTING WITH DEFERRED EXTENDED ATTRIBUTES: PRESERVES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> against a pre-existing, resident <see cref="Topic"/> whose
  ///   <see cref="AttributeCollection.LoadState"/> is already <see cref="LoadState.Loaded"/>, with a row indicating extended
  ///   attributes exist but weren't requested this load (<c>HasExtendedAttributes = true</c>), and confirms the resident <see
  ///   cref="LoadState.Loaded"/> is preserved rather than downgraded.
  /// </summary>
  /// <remarks>
  ///   A load that doesn't request <see cref="TopicPayload.ExtendedAttributes"/> (e.g. a <see cref="TopicPayload.Children"/>
  ///   top-up) must not silently discard the fact that the extended attribute property is already fully loaded; doing so would
  ///   trigger a needless refetch, and could clobber an unsaved local edit the next time it's touched.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_PreExistingWithDeferredExtendedAttributes_PreservesLoaded() {

    var topic                   = new Topic("Root", "Container", null, 1);

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);

    using var tableReader       = new DataTableReader(topics);

    await tableReader.LoadTopicGraph(referenceTopic: topic, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, topic.Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: PRE-EXISTING SINGLE CHILD: PRESERVES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> against a pre-existing, resident <see cref="Topic"/> that has
  ///   exactly one child, already fully <see cref="LoadState.Loaded"/>, with a shallow reload of the seed that doesn't
  ///   re-return that child row, and confirms the resident <see cref="LoadState.Loaded"/> is preserved, rather than downgraded.
  /// </summary>
  /// <remarks>
  ///   Without this guard, a load's failure to re-return an already materialized single child (indistinguishable, by row count
  ///   alone, from a genuinely deferred boundary) would be misread as evidence the boundary was never loaded.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_PreExistingSingleChild_PreservesLoaded() {

    var topic                   = new Topic("Root", "Container", null, 1);
    var child                   = new Topic("Child", "Page", topic, 2);

    ((ITopicBackingAccessor)topic).Children.LoadState = LoadState.Loaded;

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);

    using var tableReader       = new DataTableReader(topics);

    await tableReader.LoadTopicGraph(1, referenceTopic: topic, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, ((ITopicBackingAccessor)topic).Children.LoadState);
    Assert.Equal(child, ((ITopicBackingAccessor)topic).Children.Single());

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: PRE-EXISTING ANCESTOR: PRESERVES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> for a deep seed whose ancestor is preexisting and already
  ///   fully <see cref="LoadState.Loaded"/>, and confirms the ancestor's <see cref="LoadState.Loaded"/> is preserved rather
  ///   than downgraded by the ancestor crawl.
  /// </summary>
  /// <remarks>
  ///   <c>@LoadAscendants</c> is passed for every <see cref="SqlTopicRepository.Load(Int32, Topic, Boolean, TopicPayload)"/>
  ///   call outside of the root, regardless of <c>isRecursive</c> or payload, so the ancestor crawl runs on essentially every
  ///   load of anything beneath an already loaded ancestor. Without this guard, an already complete ancestor would be
  ///   perpetually reset to <see cref="LoadState.NotLoaded"/>.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_PreExistingAncestor_PreservesLoaded() {

    var root                    = new Topic("Root", "Container", null, 1);
    var ancestor                = new Topic("Ancestor", "Container", root, 2);
    var seed                    = new Topic("Seed", "Page", ancestor, 3);

    ((ITopicBackingAccessor)ancestor).Children.LoadState = LoadState.Loaded;

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Ancestor", "Container", 1, hasChildren: true);
    topics.AddRow(3, "Seed", "Page", 2, hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    await tableReader.LoadTopicGraph(3, referenceTopic: seed, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.Loaded, ((ITopicBackingAccessor)ancestor).Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: FRESH ANCESTOR: SETS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> for a deep seed whose ancestor has a sibling not returned by
  ///   this load, and confirms the freshly introduced ancestor's <see cref="LoadState.NotLoaded"/> is still correctly set,
  ///   despite the seed's row naming the ancestor as its <c>ParentID</c> being processed after it.
  /// </summary>
  /// <remarks>
  ///   Guards against a defect variant in the ancestor classification: If the seed's own row were allowed to credit its parent
  ///   as having received a "loaded" child, the ancestor would be incorrectly marked <see cref="LoadState.Loaded"/> despite its
  ///   other child (the untouched sibling) having never been returned, thus risking <c>DeleteUnmatched</c> data loss on a
  ///   subsequent save.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_FreshAncestor_SetsNotLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Ancestor", "Container", 1, hasChildren: true);
    topics.AddRow(3, "Seed", "Page", 2, hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(3, cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(LoadState.NotLoaded, ((ITopicBackingAccessor)topic.Parent!).Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: DISCONNECTED BATCH: DOES NOT THROW
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a row whose <c>ParentID</c> names a topic that is
  ///   neither the first row nor otherwise resident, and confirms it completes without throwing.
  /// </summary>
  /// <remarks>
  ///   Approximates the shape of <c>GetTopicUpdates</c> (used by <see cref="SqlTopicRepository.Refresh"/>): An arbitrary,
  ///   possibly disconnected batch of individually modified topics, with <c>HasChildren</c> always <c>NULL</c> and no
  ///   guaranteed row order. A topic's parent may not be resolvable at all in that shape; <see cref="SqlDataReaderExtensions.
  ///   LoadTopicGraph"/> must derive completeness from the raw <c>ParentID</c> column, never by navigating <see cref=
  ///   "Topic.Parent"/> as an object, or this throws a <see cref="NullReferenceException"/>.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_DisconnectedBatch_DoesNotThrow() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container");
    topics.AddRow(99, "Orphan", "Page", 999);

    using var tableReader       = new DataTableReader(topics);

    var exception               = await Record.ExceptionAsync(
      async ()                  => await tableReader.LoadTopicGraph(cancellationToken: CancellationToken)
    );

    Assert.Null(exception);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: DISCONNECTED BATCH: PRESERVES NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <c>GetTopicUpdates</c>-shaped batch naming a existing,
  ///   <see cref="LoadState.NotLoaded"/> parent's child, and confirms the parent's <see cref="LoadState.NotLoaded"/> is
  ///   preserved, rather than being misread as a complete child listing.
  /// </summary>
  /// <remarks>
  ///   A <c>Refresh()</c> batch is an arbitrary, unordered set of individually modified topics, not a complete child listing
  ///   the way a <c>GetTopics</c> result is. Without gating on <c>HasChildren</c> (always <c>NULL</c> in this shape), any row
  ///   naming a resident parent, other than the batch's arbitrary first row, would be misread as proof the parent's full child
  ///   set was returned, silently preventing it from lazy loading.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_DisconnectedBatch_PreservesNotLoaded() {

    var root                    = new Topic("Root", "Container", null, 1);
    var parent                  = new Topic("Parent", "Container", root, 2);
    var rawParent               = (ITopicBackingAccessor)parent;

    rawParent.Children.LoadState = LoadState.NotLoaded;

    using var topics            = new TopicsDataTable();

    topics.AddRow(99, "Orphan", "Page", 999);
    topics.AddRow(3, "Child", "Page", 2);

    using var tableReader       = new DataTableReader(topics);

    await tableReader.LoadTopicGraph(referenceTopic: parent, cancellationToken: CancellationToken);

    Assert.Equal(LoadState.NotLoaded, rawParent.Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WHOLE TREE LOAD: CONVERGES NON-LEAF REGION NODES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with the default <c>seedTopicId</c> (<c>-1</c>, a whole-tree
  ///   load with no single seed) against a multi-level tree, and confirms a non-leaf node partway down the tree converges to
  ///   <see cref="LoadState.Loaded"/>, rather than being misclassified as an ancestor.
  /// </summary>
  /// <remarks>
  ///   The stored procedure resolves <c>-1</c> to the actual root internally, so no returned row's id ever equals the literal
  ///   <c>seedTopicId</c> passed to <see cref="SqlDataReaderExtensions.LoadTopicGraph"/>; the ancestor classification must not
  ///   mistake this for "no seed found yet" and misclassify the entire tree as ancestors.
  /// </remarks>
  [Fact]
  public async Task LoadTopicGraph_WholeTreeLoad_ConvergesNonLeafRegionNodes() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Branch", "Container", 1, hasChildren: true);
    topics.AddRow(3, "Leaf", "Page", 2, hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    var root                    = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);
    var branch                  = root!.Children["Branch"];

    Assert.Equal(LoadState.Loaded, ((ITopicBackingAccessor)branch).Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH MISSING RELATIONSHIP: SETS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a relationship whose target is not in the live graph,
  ///   and confirms that <see cref="TopicRelationshipMultiMap.LoadState"/> is set to <see cref="LoadState.NotLoaded"/> to
  ///   block <c>DeleteUnmatched</c> on save until the edge is resolved via <c>EnsureLoaded</c>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithMissingRelationship_SetsNotLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var relationships     = new RelationshipsDataTable();

    topics.AddRow(1, "Root", "Container");
    relationships.AddRow(1, "Test", 99, false);

    using var tableReader       = new DataTableReader([topics, empty, empty, relationships]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH MISSING REFERENCE: SETS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a reference whose target is not in the live graph, and
  ///   confirms that <see cref="TopicReferenceCollection.LoadState"/> is set to <see cref="LoadState.NotLoaded"/> to block
  ///   <c>DeleteUnmatched</c> on save until the reference is resolved via <c>EnsureLoaded</c>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithMissingReference_SetsNotLoaded() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var references        = new TopicReferencesDataTable();

    topics.AddRow(1, "Root", "Container");
    references.AddRow(1, "Test", 99);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, references]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH HISTORY DEFERRED: RETURNS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with no <see cref="VersionHistoryDataTable"/> records, as
  ///   would happen when <c>@IncludeHistory</c> is <c>0</c>, and confirms the history boundary is <see cref=
  ///   "LoadState.NotLoaded"/>, since every persisted topic is expected to have at least one version.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithHistoryDeferred_ReturnsNotLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container");

    using var tableReader = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.VersionHistory));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH VERSION HISTORY: RETURNS VERSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with an <see cref="VersionHistoryDataTable"/> record and
  ///   confirms that a topic with those values is returned.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithVersionHistory_ReturnsVersions() {

    using var topics            = new TopicsDataTable();
    using var empty             = new AttributesDataTable();
    using var versions          = new VersionHistoryDataTable();

    topics.AddRow(1, "Root", "Container");
    versions.AddRow(1, DateTime.MinValue);

    using var tableReader       = new DataTableReader([topics, empty, empty, empty, empty, versions]);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(1, topic.Id);
    Assert.Single(topic.VersionHistory);
    Assert.Contains(DateTime.MinValue, topic.VersionHistory);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH EXTENDED DEFERRED AND BLOB: RETURNS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> row where
  ///   <c>HasExtendedAttributes</c> is <see langword="true"/> (blob deferred), and confirms the extended-attribute boundary is
  ///   <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithExtendedDeferredAndBlob_ReturnsNotLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: true);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.False(((ITopicLazyLoadable)topic).IsLoaded(TopicPayload.ExtendedAttributes));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH EXTENDED DEFERRED AND NO BLOB: RETURNS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> row where
  ///   <c>HasExtendedAttributes</c> is <see langword="false"/> (blob deferred, but empty), and confirms the extended-attribute
  ///   boundary is <see cref="LoadState.Loaded"/>, thus avoiding a wasted round-trip.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithExtendedDeferredAndNoBlob_ReturnsLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasExtendedAttributes: false);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(LoadState.Loaded, topic.Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH EXTENDED INCLUDED AND BLOB: RETURNS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> row where
  ///   <c>HasExtendedAttributes</c> is <see langword="null"/> (extended included in result set), and confirms the
  ///   extended-attribute boundary is <see cref="LoadState.Loaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithExtendedIncludedAndBlob_ReturnsLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container");

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(cancellationToken: CancellationToken);

    Assert.NotNull(topic);
    Assert.Equal(LoadState.Loaded, topic.Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH HAS CHILDREN FALSE: RETURNS CHILDREN LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a <see cref="TopicsDataTable"/> row where <c>HasChildren
  ///   </c> is <see langword="false"/> and confirms that <see cref="Topic.Children"/> is <see cref="LoadState.Loaded"/> (the
  ///   topic is a leaf with nothing to lazy-load).
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithHasChildrenFalse_ReturnsChildrenLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(1, cancellationToken: CancellationToken);

    Assert.True(((ITopicLazyLoadable)topic)?.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH HAS CHILDREN AND LOADED CHILDREN: RETURNS CHILDREN LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a parent row where <c>HasChildren</c> is <see langword=
  ///   "true"/> and the child rows are present in the result set, confirming that <see cref="Topic.Children"/> is <see cref=
  ///    "LoadState.Loaded"/> (i.e., the subtree was loaded in full).
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithHasChildrenAndLoadedChildren_ReturnsChildrenLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Child", "Page", 1, hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(1, cancellationToken: CancellationToken);

    Assert.True(((ITopicLazyLoadable)topic)?.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH HAS CHILDREN AND NO RETURNED CHILDREN: SETS NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a shallow, single-row result set where <c>HasChildren
  ///   </c> is <see langword="true"/> but no child rows are returned, confirming <see cref="Topic.Children"/> is <see cref=
  ///   "LoadState.NotLoaded"/> (i.e., the seed itself is known to have children, but none were loaded).
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithHasChildrenAndNoReturnedChildren_SetsNotLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);

    using var tableReader       = new DataTableReader(topics);

    var topic                   = await tableReader.LoadTopicGraph(1, cancellationToken: CancellationToken);

    Assert.False(((ITopicLazyLoadable)topic)?.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH HAS CHILDREN ON ANCESTOR AND LOADED SUBTREE: SETS LOAD STATE CORRECTLY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a result set that includes both ancestor and a fully
  ///   loaded subtree, confirming that ancestor topics are stamped <see cref="LoadState.NotLoaded"/> (i.e., partial children)
  ///   while subtree topics are stamped <see cref="LoadState.Loaded"/> (i.e., all children present). This is the primary
  ///   scenario addressed by the <c>seedTopicId</c> parameter; i.e., loading ascendants and descendants.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithHasChildrenOnAncestorAndLoadedSubtree_SetsLoadStateCorrectly() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Child", "Container", 1, hasChildren: true);
    topics.AddRow(3, "Grandchild", "Page", 2, hasChildren: false);

    using var tableReader       = new DataTableReader(topics);

    // The seed topic is Child (2); Root (1) is on the ancestor chain and is NotLoaded.
    // The Child (seed) and Grandchild are in the fully loaded subtree and are Loaded.
    var seedTopic               = await tableReader.LoadTopicGraph(2, cancellationToken: CancellationToken);
    var rootTopic               = seedTopic?.Parent;

    Assert.Equal(LoadState.NotLoaded, rootTopic?.Children.LoadState);
    Assert.Equal(LoadState.Loaded, seedTopic?.Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD TOPIC GRAPH: WITH ONE LEVEL OF CHILDREN: CONVERGES SEED, LEAVES GRANDCHILDREN NOT LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph"/> with a result set shaped like a non-recursive <c>
  ///   @LoadChildren</c> call, with the seed's immediate child present, but that child's own children are not, and confirms the
  ///   seed converges to <see cref="LoadState.Loaded"/> while the child (which received no rows of its own) remains <see
  ///   cref="LoadState.NotLoaded"/>.
  /// </summary>
  [Fact]
  public async Task LoadTopicGraph_WithOneLevelOfChildren_ConvergesSeedLeavesGrandchildrenNotLoaded() {

    using var topics            = new TopicsDataTable();

    topics.AddRow(1, "Root", "Container", hasChildren: true);
    topics.AddRow(2, "Child", "Container", 1, hasChildren: true);

    using var tableReader       = new DataTableReader(topics);

    var seedTopic               = await tableReader.LoadTopicGraph(1, cancellationToken: CancellationToken);
    var childTopic              = ((ITopicBackingAccessor?)seedTopic)?.Children.FirstOrDefault();

    Assert.Equal(LoadState.Loaded, ((ITopicBackingAccessor?)seedTopic)?.Children.LoadState);
    Assert.Equal(LoadState.NotLoaded, ((ITopicBackingAccessor?)childTopic)?.Children.LoadState);

  }

  /*============================================================================================================================
  | TEST: TOPIC LIST DATA TABLE: ADD ROW: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Constructs a <see cref="TopicListDataTable"/> and calls <see cref="TopicListDataTable.AddRow(Int32)"/>. Confirms that a
  ///   <see cref="DataRow"/> with the expected data is returned.
  /// </summary>
  [Fact]
  public void TopicListDataTable_AddRow_Succeeds() {

    var dataTable               = new TopicListDataTable();

    dataTable.AddRow(1);
    dataTable.AddRow(2);
    dataTable.AddRow(2);

    Assert.Equal(3, dataTable.Rows.Count);
    Assert.Single(dataTable.Columns);

    dataTable.Dispose();

  }


  /*============================================================================================================================
  | TEST: FILL CHILDREN: PRE-EXISTING CHILD WITHOUT EXTENDED ATTRIBUTES: CONVERGES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.FillChildren"/> for a pre-existing, resident child topic whose <see cref=
  ///   "AttributeCollection.LoadState"/> is <see cref="LoadState.NotLoaded"/>, with a row indicating the topic genuinely has no
  ///   extended attributes (<c>HasExtendedAttributes = false</c>), and confirms the property converges to <see cref=
  ///   "LoadState.Loaded"/> rather than being left stuck.
  /// </summary>
  /// <remarks>
  ///   A children-only fill returns no extended attributes, but <c>false</c> is still definitive: There is nothing to defer, so
  ///   there's no reason to leave a pre-existing child's property <see cref="LoadState.NotLoaded"/> until something else
  ///   happens to touch it.
  /// </remarks>
  [Fact]
  public async Task FillChildren_PreExistingChildWithoutExtendedAttributes_ConvergesLoaded() {

    var parent                  = new Topic("Parent", "Container", null, 1);
    var child                   = new Topic("Child", "Page", parent, 2);

    ((ITopicBackingAccessor)child).Attributes.LoadState = LoadState.NotLoaded;

    using var topics             = new TopicsDataTable();

    topics.AddRow(1, "Parent", "Container", hasExtendedAttributes: false);
    topics.AddRow(2, "Child", "Page", 1, hasExtendedAttributes: false);

    using var tableReader        = new DataTableReader(topics);

    var topicIndex               = parent.GetTopicIndex();

    await tableReader.FillChildren(parent, topicIndex, CancellationToken);

    Assert.Equal(LoadState.Loaded, ((ITopicBackingAccessor)child).Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: FILL CHILDREN: FRESH CHILD WITH EXTENDED ATTRIBUTES INCLUDED: CONVERGES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="SqlDataReaderExtensions.FillChildren"/> for a fresh (not pre-existing) child topic with a row
  ///   indicating extended attributes were included with this fill (<c>HasExtendedAttributes = NULL</c>), and confirms the
  ///   boundary converges to <see cref="LoadState.Loaded"/> rather than <see cref="LoadState.NotLoaded"/>.
  /// </summary>
  /// <remarks>
  ///   Reproduces <see cref="Repositories.ITopicLazyLoader.EnsureLoaded(Topic,TopicPayload, CancellationToken)"/> being called
  ///   with <c>TopicPayload.Children | TopicPayload.ExtendedAttributes</c>: A single <c>IncludeExtended</c> parameter scopes
  ///   the whole <c>GetTopics</c> call, so children rows come back with <c>HasExtendedAttributes = NULL</c>, and their extended
  ///   attributes are delivered in the third result set, just like the seed's own row.
  /// </remarks>
  [Fact]
  public async Task FillChildren_FreshChildWithExtendedAttributesIncluded_ConvergesLoaded() {

    var parent                  = new Topic("Parent", "Container", null, 1);

    using var topics             = new TopicsDataTable();

    topics.AddRow(1, "Parent", "Container");
    topics.AddRow(2, "Child", "Page", 1);

    using var tableReader        = new DataTableReader(topics);

    // Attach-first: the fresh child is indexed via the attach hook into the live index, not the passed-in lookup index
    var topicIndex               = parent.GetLiveTopicIndex();

    await tableReader.FillChildren(parent, topicIndex, CancellationToken);

    var child                    = (ITopicBackingAccessor)topicIndex[2];

    Assert.Equal(LoadState.Loaded, child.Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: ATTRIBUTE VALUES DATA TABLE: ADD ROW: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Constructs a <see cref="AttributeValuesDataTable"/>, calls <see cref="AttributeValuesDataTable.AddRow(String, String?)"
  ///   />. Confirms that a <see cref="DataRow"/> with the expected data is returned.
  /// </summary>
  [Fact]
  public void AttributeValuesDataTable_AddRow_Succeeds() {

    var dataTable               = new AttributeValuesDataTable();

    dataTable.AddRow("Key", "Test");
    dataTable.AddRow("ContentType", "Page");
    dataTable.AddRow("ParentId", "4");

    Assert.Equal(3, dataTable.Rows.Count);
    Assert.Equal(2, dataTable.Columns.Count);

    dataTable.Dispose();

  }

  /*============================================================================================================================
  | TEST: TOPIC REFERENCES DATA TABLE: ADD ROW: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Constructs a <see cref="AttributeValuesDataTable"/> and calls <see cref=
  ///   "Data.Sql.Models.TopicReferencesDataTable.AddRow(String, Int32)"/>. Confirms that a <see cref="DataRow"/> with the
  ///   expected data is returned.
  /// </summary>
  [Fact]
  public void TopicReferencesDataTable_AddRow_Succeeds() {

    var dataTable               = new Data.Sql.Models.TopicReferencesDataTable();

    dataTable.AddRow("BaseTopic", 1);
    dataTable.AddRow("Parent",  2);
    dataTable.AddRow("RootTopic", 3);

    Assert.Equal(3, dataTable.Rows.Count);
    Assert.Equal(2, dataTable.Columns.Count);

    dataTable.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: STRING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, String)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_String() {

    var command                 = new SqlCommand();

    command.AddParameter("TopicKey", "Root");

    var sqlParameter            = command.Parameters["@TopicKey"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@TopicKey"));
    Assert.Equal("Root", (string?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.VarChar, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: NULL STRING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <c>null</c> parameter value to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, String)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_NullString() {

    var command                 = new SqlCommand();

    command.AddParameter("TopicKey", (string?)null);

    var sqlParameter            = command.Parameters["@TopicKey"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@TopicKey"));
    Assert.Null(sqlParameter?.Value);
    Assert.Equal(SqlDbType.VarChar, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: INT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="Int32"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, Int32)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_Int() {

    var command                 = new SqlCommand();

    command.AddParameter("TopicId", 5);

    var sqlParameter            = command.Parameters["@TopicId"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@TopicId"));
    Assert.Equal(5, (int?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.Int, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: BOOL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="Boolean"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, Boolean)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_Bool() {

    var command                 = new SqlCommand();

    command.AddParameter("IsHidden", true);

    var sqlParameter            = command.Parameters["@IsHidden"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@IsHidden"));
    Assert.Equal(true, (bool?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.Bit, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: DATE/TIME
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="DateTime"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, DateTime)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_DateTime() {

    var command                 = new SqlCommand();
    var lastModified            = DateTime.UtcNow;

    command.AddParameter("LastModified", lastModified);

    var sqlParameter            = command.Parameters["@LastModified"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@LastModified"));
    Assert.Equal(lastModified,  (DateTime?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.DateTime2, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: DATA TABLE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="DataTable"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, DataTable)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_DataTable() {

    var command                 = new SqlCommand();
    var dataTable               = new TopicListDataTable();

    command.AddParameter("Relationships", dataTable);

    var sqlParameter            = command.Parameters["@Relationships"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@Relationships"));
    Assert.Equal(dataTable, (DataTable?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.Structured, sqlParameter?.SqlDbType);

    command.Dispose();
    dataTable.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD PARAMETER: STRING BUILDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="StringBuilder"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddParameter(SqlCommand, String, StringBuilder)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddParameter_StringBuilder() {

    var command                 = new SqlCommand();
    var xml                     = new StringBuilder();

    command.AddParameter("AttributesXml", xml);

    var sqlParameter            = command.Parameters["@AttributesXml"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@AttributesXml"));
    Assert.Equal(xml.ToString(), (string?)sqlParameter?.Value);
    Assert.Equal(SqlDbType.Xml, sqlParameter?.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD OUTPUT PARAMETER: RETURN CODE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddOutputParameter(SqlCommand, String)"/> extension method.
  /// </summary>
  [Fact]
  public void SqlCommand_AddOutputParameter_ReturnCode() {

    var command                 = new SqlCommand();

    command.AddOutputParameter("TopicId");

    var sqlParameter            = command.Parameters["@TopicId"];

    sqlParameter.Value          = 5;

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@TopicId"));
    Assert.Equal(5, command.GetReturnCode("TopicId"));
    Assert.Equal(ParameterDirection.ReturnValue, sqlParameter.Direction);
    Assert.Equal(SqlDbType.Int, sqlParameter.SqlDbType);

    command.Dispose();

  }

  /*============================================================================================================================
  | TEST: SQL COMMAND: ADD OUTPUT PARAMETER: RETURN DEFAULT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref=
  ///   "SqlCommandExtensions.AddOutputParameter(SqlCommand, String)"/> extension method. Ensures the default return code is
  ///   returned, if the value isn't explicitly set.
  /// </summary>
  [Fact]
  public void SqlCommand_AddOutputParameter_ReturnsDefault() {

    var command                 = new SqlCommand();

    command.AddOutputParameter("TopicId");

    var sqlParameter            = command.Parameters["@TopicId"];

    Assert.Single(command.Parameters);
    Assert.True(command.Parameters.Contains("@TopicId"));
    Assert.Equal(-1, command.GetReturnCode("TopicId"));
    Assert.Equal(ParameterDirection.ReturnValue, sqlParameter?.Direction);
    Assert.Equal(SqlDbType.Int, sqlParameter?.SqlDbType);

    command.Dispose();

  }

} //Class