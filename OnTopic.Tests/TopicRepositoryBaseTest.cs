/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using OnTopic.Collections.Specialized;
using OnTopic.Data.Caching;
using OnTopic.Metadata;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.Metadata;
using Xunit;

namespace OnTopic.Tests;

/*==============================================================================================================================
| CLASS: TOPIC REPOSITORY BASE TESTS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides unit tests for the <see cref="TopicRepository"/> class.
/// </summary>
/// <remarks>
///   These tests evaluate features that are specific to the <see cref="TopicRepository"/> class.
/// </remarks>
[ExcludeFromCodeCoverage]
public class TopicRepositoryBaseTest {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      StubTopicRepository             _topicRepository;
  readonly                      CachedTopicRepository           _cachedTopicRepository;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Initializes a new instance of the <see cref="TopicRepositoryBaseTest"/> with shared resources.
  /// </summary>
  /// <remarks>
  ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
  ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
  ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
  ///   crawling the object graph.
  /// </remarks>
  public TopicRepositoryBaseTest() {
    _topicRepository            = new();
    _cachedTopicRepository      = new(_topicRepository);
  }

  /*============================================================================================================================
  | TEST: LOAD: VALID TOPIC ID: RETURNS EXPECTED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> with a valid <see cref="Topic.Id"/>
  ///   and confirms that the expected topic is returned.
  /// </summary>
  [Fact]
  public async Task Load_ValidTopicId_ReturnsExpectedTopic() {

    var topic                   = await _topicRepository.Load(11111);

    Assert.Equal(11111, topic?.Id);

  }

  /*============================================================================================================================
  | TEST: LOAD: INVALID TOPIC ID: RETURNS EXPECTED TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> with an invalid <see cref=
  ///   "Topic.Id"/> and confirms that no topic is returned.
  /// </summary>
  [Fact]
  public async Task Load_InvalidTopicId_ReturnsExpectedTopic() =>
    Assert.Null(await _topicRepository.Load(11113));

  /*============================================================================================================================
  | TEST: LOAD: NEGATIVE TOPIC ID: RETURNS ROOT TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> with a negative <see cref=
  ///   "Topic.Id"/> and confirms that the root topic is returned.
  /// </summary>
  [Fact]
  public async Task Load_NegativeTopicId_ReturnsRootTopic() =>
    Assert.Equal("Root", (await _cachedTopicRepository.Load(-2))?.GetUniqueKey());

  /*============================================================================================================================
  | TEST: LOAD: NARROW PAYLOAD: RETURNS TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="StubTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> with <c>payload</c> set to <see cref=
  ///   "TopicPayload.None"/> and confirms that a topic is still returned. The stub always returns fully-loaded topics
  ///   regardless of this parameter; the test simply verifies the signature is accepted.
  /// </summary>
  [Fact]
  public async Task Load_WithNarrowPayload_ReturnsTopic() {

    var topic                   = await _topicRepository.Load(11111, payload: TopicPayload.None);

    Assert.NotNull(topic);

  }

  /*============================================================================================================================
  | TEST: LOAD: NARROW PAYLOAD: EXTENDED ATTRIBUTES LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="StubTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> with <c>payload</c> set to <see cref=
  ///   "TopicPayload.None"/> and confirms the extended-attribute boundary is <see cref="LoadState.Loaded"/>. The stub does not
  ///   defer extended attributes; this simply confirms no regression for stub-backed tests.
  /// </summary>
  [Fact]
  public async Task Load_WithNarrowPayload_ExtendedAttributesLoaded() {

    var topic                   = await _topicRepository.Load(11111, payload: TopicPayload.None);

    Assert.NotNull(topic);
    Assert.Equal(LoadState.Loaded, topic.Attributes.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD: VALID DATE: RETURNS TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a valid date and ensures that topic
  ///   with that date is returned.
  /// </summary>
  [Fact]
  public async Task Load_ValidDate_ReturnsTopic() {

    var version                 = DateTime.UtcNow.AddDays(-1);
    var topic                   = await _cachedTopicRepository.Load(11111, version);

    Assert.True(topic?.VersionHistory.Contains(version));
    Assert.Equal(version.AddTicks(-(version.Ticks % TimeSpan.TicksPerSecond)), topic?.LastModified);

  }

  /*============================================================================================================================
  | TEST: ROLLBACK: TOPIC: UPDATES LAST MODIFIED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="TopicRepository.Rollback(Topic, DateTime)"/> with a valid date and ensures that the <see cref=
  ///   "Topic.LastModified"/> value is updated.
  /// </summary>
  [Fact]
  public async Task Rollback_Topic_UpdatesLastModified() {

    var version                 = DateTime.UtcNow.AddDays(-1);
    var topic                   = await _topicRepository.Load(11111);

    if (topic is not null) {
      topic.VersionHistory.Add(version);
      await _topicRepository.Rollback(topic, version);
    }

    Assert.True(topic?.VersionHistory.Contains(version));
    Assert.Equal(version.AddTicks(-(version.Ticks % TimeSpan.TicksPerSecond)), topic?.LastModified);

  }

  /*============================================================================================================================
  | TEST: LOAD: FUTURE DATE: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a future <see cref="DateTime"/> and
  ///   confirms that an exception is thrown.
  /// </summary>
  [Fact]
  public async Task Load_FutureDate_ThrowsException() =>
    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      _cachedTopicRepository.Load(1111, DateTime.UtcNow.AddDays(1))
    );

  /*============================================================================================================================
  | TEST: LOAD: OLD DATE: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a date prior to versioning being
  ///   introduced and ensures that an exception is thrown.
  /// </summary>
  [Fact]
  public async Task Load_OldDate_ThrowsException() =>
    await Assert.ThrowsAsync<InvalidOperationException>(() =>
      _cachedTopicRepository.Load(1111, new DateTime(2010, 10, 15))
    );

  /*============================================================================================================================
  | TEST: DELETE: BASE TOPIC: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic which other topics, outside the graph, derive from. Expects exception.
  /// </summary>
  [Fact]
  public async Task Delete_BaseTopic_ThrowsException() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    var child                   = new Topic("Child", "Page", topic);
    var derivedTopic            = new Topic("Derived", "Page", root) {
      BaseTopic                 = child
    };

    await Assert.ThrowsAsync<ReferentialIntegrityException>(() =>
      _topicRepository.Delete(topic, true)
    );

  }

  /*============================================================================================================================
  | TEST: DELETE: INTERNAL DERIVED TOPIC: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic which another topic within the graph derives from. Expects success.
  /// </summary>
  [Fact]
  public async Task Delete_InternallyDerivedTopic_Succeeds() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    var child                   = new Topic("Child", "Page", topic);
    _                           = new Topic("Derived", "Page", topic) {
      BaseTopic                 = child
    };

    await _topicRepository.Delete(topic, true);

    Assert.Empty(root.Children);

  }

  /*============================================================================================================================
  | TEST: DELETE: DESCENDANTS: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic with descendant topics. Expects exception if <c>isRecursive</c> is set to <c>false</c>.
  /// </summary>
  [Fact]
  public async Task Delete_Descendants_ThrowsException() {

    var topic                   = new Topic("Topic", "Page");
    _                           = new Topic("Child", "Page", topic);

    await Assert.ThrowsAsync<ReferentialIntegrityException>(() =>
      _topicRepository.Delete(topic)
    );

  }

  /*============================================================================================================================
  | TEST: DELETE: DESCENDANTS WITH RECURSIVE: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic with descendant topics. Expects no exception if <c>isRecursive</c> is set to <c>true</c>.
  /// </summary>
  [Fact]
  public async Task Delete_DescendantsWithRecursive_Succeeds() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    _                           = new Topic("Child", "Page", topic);

    await _topicRepository.Delete(topic, true);

    Assert.Empty(root.Children);

  }

  /*============================================================================================================================
  | TEST: DELETE: NESTED TOPICS: SUCCEEDS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic with nested topics. Expects no exception, even if <c>isRecursive</c> is set to <c>false</c>.
  /// </summary>
  [Fact]
  public async Task Delete_NestedTopics_Succeeds() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    _                           = new Topic("Child", "List", topic);

    await _topicRepository.Delete(topic);

    Assert.Empty(root.Children);

  }

  /*============================================================================================================================
  | TEST: DELETE: ASSOCIATIONS: DELETE ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic with outgoing relationships and topic references. Additionally, deletes those associations from the
  ///   target topics' <see cref="Topic.IncomingRelationships"/> collection.
  /// </summary>
  [Fact]
  public async Task Delete_Relationships_DeleteRelationships() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    var child                   = new Topic("Child", "Page", topic);
    var associated              = new Topic("Associated", "Page", root);

    child.Relationships.SetValue("Related", associated);
    child.References.SetValue("Referenced", associated);

    await _topicRepository.Delete(topic, true);

    Assert.Empty(associated.IncomingRelationships.GetValues("Related"));
    Assert.Empty(associated.IncomingRelationships.GetValues("Referenced"));

  }

  /*============================================================================================================================
  | TEST: DELETE: INCOMING RELATIONSHIPS: DELETE ASSOCIATIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Deletes a topic with incoming relationships. Deletes the relationships or references from the associated topic.
  /// </summary>
  [Fact]
  public async Task Delete_IncomingRelationships_DeleteAssociations() {

    var root                    = new Topic("Root", "Page");
    var topic                   = new Topic("Topic", "Page", root);
    var child                   = new Topic("Child", "Page", topic);
    var source1                 = new Topic("Source1", "Page", root);
    var source2                 = new Topic("Source2", "Page", root);

    source1.Relationships.SetValue("Associations", child);
    source2.References.SetValue("Associations", child);

    await _topicRepository.Delete(topic, true);

    Assert.Empty(source1.Relationships.GetValues("Associations"));

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: ANY ATTRIBUTES: RETURNS ALL ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, without any filtering by whether the attribute is an <see cref=
  ///   "AttributeDescriptor.IsExtendedAttribute"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_AnyAttributes_ReturnsAllAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("Title", "Title");

    var attributes              = _topicRepository.GetAttributesProxy(topic, null);

    Assert.Single(attributes);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: EMPTY ATTRIBUTES: SKIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, without any filtering by whether the attribute is an <see cref=
  ///   "AttributeDescriptor.IsExtendedAttribute"/>. Any <see cref="AttributeRecord"/>s with a null or empty value should be
  ///   skipped.
  /// </summary>
  [Fact]
  public void GetAttributes_EmptyAttributes_Skips() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("EmptyAttribute", "");
    topic.Attributes.SetValue("NullAttribute", null);

    var attributes              = _topicRepository.GetAttributesProxy(topic, null).ToList();

    Assert.DoesNotContain(attributes, a => a.Key is "EmptyAttribute");
    Assert.DoesNotContain(attributes, a => a.Key is "NullAttribute");

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: INDEXED ATTRIBUTES: RETURNS INDEXED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_IndexedAttributes_ReturnsIndexedAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("Title", "Title");

    var attributes              = _topicRepository.GetAttributesProxy(topic, false);

    Assert.Empty(attributes);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTES: RETURNS EXTENDED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_ExtendedAttributes_ReturnsExtendedAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("Title", "Title");

    var attributes              = _topicRepository.GetAttributesProxy(topic, true);

    Assert.Single(attributes);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTE MISMATCH: RETURNS EXTENDED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, filtering by <see cref="TrackedRecord{T}.IsDirty"/>. Expects an <see
  ///   cref="AttributeRecord"/> to be returned even if it's <i>not</i> <see cref="TrackedRecord{T}.IsDirty"/> <i>but</i> its
  ///   <see cref="AttributeRecord.IsExtendedAttribute"/> disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_ExtendedAttributeMismatch_ReturnsExtendedAttributes() {

    var topic                   = new Topic("Test", "Page", null, 1);

    topic.Attributes.SetValue("Title", "Title", markDirty: false, isExtendedAttribute: false);
    topic.Attributes.SetValue("IsHidden", "0", markDirty: false, isExtendedAttribute: true);
    topic.Attributes.SetValue("MetaTitle", "Metatitle", markDirty: false, isExtendedAttribute: null);
    topic.Attributes.SetValue("Arbitrary", "Value", markDirty: false, isExtendedAttribute: true);

    var dirtyExtended           = _topicRepository.GetAttributesProxy(topic, true, true).ToList();
    var dirtyIndexed            = _topicRepository.GetAttributesProxy(topic, false, true).ToList();
    var cleanExtended           = _topicRepository.GetAttributesProxy(topic, true, false).ToList();
    var cleanIndexed            = _topicRepository.GetAttributesProxy(topic, false, false).ToList();

    //Expect Title, even though it isn't IsDirty
    Assert.Single(dirtyExtended);
    Assert.Equal("Title", dirtyExtended.FirstOrDefault()?.Key);

    //Expect IsHidden, even though it isn't IsDirty
    Assert.Single(dirtyIndexed);
    Assert.Equal("IsHidden", dirtyIndexed.FirstOrDefault()?.Key);

    //Expect Metatitle, since it's clean, and not mismatched
    Assert.Single(cleanExtended);
    Assert.Equal("MetaTitle", cleanExtended.FirstOrDefault()?.Key);

    //Expect Arbitrary, since it's arbitrary and it's length is less than 255
    Assert.Single(cleanIndexed);
    Assert.Equal("Arbitrary", cleanIndexed.FirstOrDefault()?.Key);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTE MISMATCH: RETURNS NOTHING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, filtering by <see cref="TrackedRecord{T}.IsDirty"/>. Expects the <see
  ///   cref="AttributeRecord"/> to <i>not</i> be returned even though its <see cref="AttributeRecord.IsExtendedAttribute"/>
  ///   disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>, since it won't match the <see
  ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>'s <c>isExtendedAttribute</c> call.
  /// </summary>
  [Fact]
  public void GetAttributes_ExtendedAttributeMismatch_ReturnsNothing() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("Title", "Title", markDirty: false, isExtendedAttribute: false);

    var attributes              = _topicRepository.GetAttributesProxy(topic, false, true);

    Assert.Empty(attributes);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: EXCLUDE LAST MODIFIED: RETURNS OTHER ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of attributes from a topic, filtering by <c>excludeLastModified</c>. Confirms that <see
  ///   cref="AttributeRecord"/>s are not returned which start with <c>LastModified</c>.
  /// </summary>
  [Fact]
  public void GetAttributes_ExcludeLastModified_ReturnsOtherAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetDateTime("LastModified", DateTime.Now);
    topic.Attributes.SetValue("LastModifiedBy", "Unit Tests");

    var attributes              = _topicRepository.GetAttributesProxy(topic, null, excludeLastModified: true);

    Assert.Empty(attributes);

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH SHORT VALUE: RETURNS AS INDEXED ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value shorter than 255 characters, then
  ///   ensures that it is returned as an <i>indexed</i> <see cref="AttributeRecord"/> when calling <see
  ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_ArbitraryAttributeWithShortValue_ReturnsAsIndexedAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("ArbitraryAttribute", "Value");

    var attributes              = _topicRepository.GetAttributesProxy(topic, false);

    Assert.Contains(attributes, a => a.Key is "ArbitraryAttribute");

  }

  /*============================================================================================================================
  | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH LONG VALUE: RETURNS AS EXTENDED ATTRIBUTE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value longer than 255 characters, then
  ///   ensures that it is returned as an <see cref="AttributeDescriptor.IsExtendedAttribute"/> when calling <see
  ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
  /// </summary>
  [Fact]
  public void GetAttributes_ArbitraryAttributeWithLongValue_ReturnsAsExtendedAttributes() {

    var topic                   = new Topic("Test", "ContentTypes");

    topic.Attributes.SetValue("ArbitraryAttribute", new('x', 256));

    var attributes              = _topicRepository.GetAttributesProxy(topic, true);

    Assert.Contains(attributes, a => a.Key is "ArbitraryAttribute");

  }

  /*============================================================================================================================
  | TEST: GET UNMATCHED ATTRIBUTES: RETURNS ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Using <see cref="TopicRepository.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
  ///   <see cref="ContentTypeDescriptor"/> but not the <see cref="Topic"/> are returned.
  /// </summary>
  [Fact]
  public void GetUnmatchedAttributes_ReturnsAttributes() {

    var topic                   = new Topic("Test", "Page", null, 1);

    topic.Attributes.SetValue("Title", "Title");

    var attributes              = _topicRepository.GetUnmatchedAttributesProxy(topic).ToList();

    Assert.True(attributes.Count != 0);
    Assert.DoesNotContain(attributes, a => a.Key is "Title");

  }

  /*============================================================================================================================
  | TEST: GET UNMATCHED ATTRIBUTES: EMPTY ARBITRARY ATTRIBUTES: RETURNS ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Using <see cref="TopicRepository.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
  ///   <see cref="Topic"/> but not the <see cref="ContentTypeDescriptor"/> <i>and</i> are either <c>null</c> or empty are
  ///   returned. This ensures that arbitrary attributes can be deleted programmatically, instead of lingering as orphans in
  ///   the database.
  /// </summary>
  [Fact]
  public void GetUnmatchedAttributes_EmptyArbitraryAttributes_ReturnsAttributes() {

    var topic                   = new ContentTypeDescriptor("Test", "ContentTypeDescriptor", null, 1);

    topic.Attributes.SetValue("ArbitraryAttribute", "Value");
    topic.Attributes.SetValue("ArbitraryAttribute", "");
    topic.Attributes.SetValue("AnotherArbitraryAttribute", "Value");
    topic.Attributes.SetValue("YetAnotherArbitraryAttribute", "Value");
    topic.Attributes.SetValue("YetAnotherArbitraryAttribute", null);

    var attributes              = _topicRepository.GetUnmatchedAttributesProxy(topic).ToList();

    Assert.Contains(attributes, a => a.Key is "ArbitraryAttribute");
    Assert.Contains(attributes, a => a.Key is "YetAnotherArbitraryAttribute");
    Assert.DoesNotContain(attributes, a => a.Key is "AnotherArbitraryAttribute");

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTORS: RETURNS CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of <see cref="ContentTypeDescriptor"/>s from the <see cref="ITopicRepository"/> and ensures that
  ///   the expected number (2) are present.
  /// </summary>
  [Fact]
  public void GetContentTypeDescriptors_ReturnsContentTypes() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();

    Assert.Equal(15, contentTypes.Count);
    Assert.NotNull(contentTypes.GetValue("ContentTypeDescriptor"));
    Assert.NotNull(contentTypes.GetValue("Page"));
    Assert.NotNull(contentTypes.GetValue("LookupListItem"));

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTORS: WITH TOPIC GRAPH: RETURNS MERGED CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a list of <see cref="ContentTypeDescriptor"/>s from the <see cref="ITopicRepository"/> alongside a separate
  ///   topic graph and ensures the two are properly merged.
  /// </summary>
  [Fact]
  public void GetContentTypeDescriptors_WithTopicGraph_ReturnsMergedContentTypes() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var rootContentType         = contentTypes["ContentTypes"];
    var newContentType          = new ContentTypeDescriptor("NewContentType", "ContentTypeDescriptor", rootContentType);
    var contentTypeCount        = contentTypes.Count;

    _topicRepository.SetContentTypeDescriptorsProxy(rootContentType);

    Assert.NotEqual(contentTypeCount, contentTypes.Count);
    Assert.Contains(newContentType, contentTypes);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTOR: GET VALID CONTENT TYPE: RETURNS CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve a specific <see cref="ContentTypeDescriptor"/> by its <see cref="Topic.Key"/>.
  /// </summary>
  [Fact]
  public void GetContentTypeDescriptor_GetValidContentType_ReturnsContentType() {

    var topic                   = new Topic("Test", "Page");
    var contentType             = _topicRepository.GetContentTypeDescriptorProxy(topic);

    Assert.NotNull(contentType);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTOR: GET NEW CONTENT TYPE: RETURNS FROM TOPIC GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve a <see cref="ContentTypeDescriptor"/> that hasn't yet been persisted to the data store. Instead,
  ///   attempts to retrieve it from the <see cref="Topic"/>'s graph.
  /// </summary>
  [Fact]
  public async Task GetContentTypeDescriptor_GetNewContentType_ReturnsFromTopicGraph() {

    var rootTopic               = await _topicRepository.Load("Root");
    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var rootContentType         = contentTypes.GetValue("ContentTypes");
    var newContentType          = new ContentTypeDescriptor("NewContentType", "ContentTypeDescriptor", rootContentType);
    var topic                   = new Topic("Test", "NewContentType", rootTopic);

    var contentType             = _topicRepository.GetContentTypeDescriptorProxy(topic);

    Assert.NotNull(contentType);
    Assert.Equal(contentType, newContentType);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTOR: MISSING ROOT CONTENT ROOT: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve a <see cref="ContentTypeDescriptor"/> when the <see cref="ITopicRepository"/> doesn't contain a
  ///   root content type at <c>Root:Configuration:ContentTypes</c>. In this case, it should return <c>null</c>. This will
  ///   typically only occur when initializing a new database, and is an unexpected condition.
  /// </summary>
  [Fact]
  public async Task GetContentTypeDescriptor_MissingRootContentType_ReturnsNull() {

    var topicRepository         = new StubTopicRepository();
    var configuration           = await topicRepository.Load("Root:Configuration");
    var topic                   = new Topic("Test", "Page");

    await topicRepository.Delete(configuration!, true);

    var contentType             = topicRepository.GetContentTypeDescriptorProxy(topic);

    Assert.Null(contentType);

  }

  /*============================================================================================================================
  | TEST: GET CONTENT TYPE DESCRIPTOR: GET INVALID CONTENT TYPE: RETURNS NULL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve an invalid <see cref="ContentTypeDescriptor"/>.
  /// </summary>
  [Fact]
  public void GetContentTypeDescriptor_GetInvalidContentType_ReturnsNull() {

    var topic                   = new Topic("Test", "InvalidContentType");
    var contentType             = _topicRepository.GetContentTypeDescriptorProxy(topic);

    Assert.Null(contentType);

  }

  /*============================================================================================================================
  | TEST: SAVE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then saves a new <see
  ///   cref="ContentTypeDescriptor"/> via <see cref="TopicRepository.Save(Topic, Boolean)"/>, and ensures that it is
  ///   immediately reflected in the <see cref="TopicRepository"/> cache of <see cref="ContentTypeDescriptor"/>s.
  /// </summary>
  [Fact]
  public async Task Save_ContentTypeDescriptor_UpdatesContentTypeCache() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var topic                   = new ContentTypeDescriptor("NewContentType", "ContentTypeDescriptor");

    await _topicRepository.Save(topic);

    Assert.Contains(topic, contentTypes);

  }

  /*============================================================================================================================
  | TEST: SAVE: CONTENT TYPE DESCRIPTOR: UPDATES PERMITTED CONTENT TYPES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads the <see cref="TopicRepository.GetContentTypeDescriptors()"/>, then saves an existing <see cref=
  ///   "ContentTypeDescriptor"/> via <see cref="TopicRepository.Save(Topic, Boolean)"/>, and ensures that
  ///   it the <see cref="ContentTypeDescriptor.PermittedContentTypes"/> cache is updated.
  /// </summary>
  [Fact]
  public async Task Save_ContentTypeDescriptor_UpdatesPermittedContentTypes() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var contentTypesRoot        = contentTypes.GetValue("ContentTypes");
    var pageContentType         = contentTypes.GetValue("Page");
    var lookupContentType       = contentTypes.GetValue("Lookup");

    Contract.Assume(pageContentType);
    Contract.Assume(lookupContentType);
    Contract.Assume(contentTypesRoot);

    var initialCount            = pageContentType.PermittedContentTypes.Count;

    pageContentType.Relationships.SetValue("ContentTypes", lookupContentType);

    await _topicRepository.Save(contentTypesRoot, true);

    Assert.NotEqual(initialCount, pageContentType.PermittedContentTypes.Count);

  }

  /*============================================================================================================================
  | TEST: SAVE: NEW TOPIC: UPDATES VERSION HISTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> and confirms that the <see cref="Topic.VersionHistory"/> is correctly updated with a
  ///   new version.
  /// </summary>
  [Fact]
  public async Task Save_NewTopic_UpdatesVersionHistory() {

    var parent                  = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");
    var topic                   = new Topic("Test", "Page", parent);

    await _topicRepository.Save(topic);

    Assert.True(topic.VersionHistory.Count > 0);

  }

  /*============================================================================================================================
  | TEST: SAVE: IS RECURSIVE: SAVES CHILD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> with a child <see cref="Topic"/> and confirms that the <see cref="Topic.Id"/> of the
  ///   child <see cref="Topic"/> is correctly updated.
  /// </summary>
  [Fact]
  public async Task Save_IsRecursive_SavesChild() {

    var parent                  = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");
    var topic                   = new Topic("Test", "Page", parent);
    var child                   = new Topic("Child", "Page", topic);

    await _topicRepository.Save(topic, true);

    Assert.False(child.IsNew);

  }

  /*============================================================================================================================
  | TEST: SAVE: UNRESOLVED REFERENCE: RESOLVES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> with an unresolved <see cref="Topic.References"/> and confirms that it successfully
  ///   resolves it by marking the <see cref="Topic.References"/> collection as <see cref=
  ///   "TrackedRecordCollection{TItem,TValue, TAttribute}.IsDirty()"/> as <c>false</c>.
  /// </summary>
  [Fact]
  public async Task Save_UnresolvedReference_Resolves() {

    var parent                  = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");
    var topic                   = new Topic("Test", "Page", parent);
    var reference               = new Topic("Reference", "Page", topic);

    topic.References.SetValue("Test", reference);

    await _topicRepository.Save(topic, true);

  }

  /*============================================================================================================================
  | TEST: SAVE: UNRESOLVED REFERENCE: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> with an unresolved <see cref="Topic.References"/> and confirms that it throws the
  ///   expected <see cref="ReferentialIntegrityException"/> if that reference cannot be resolved.
  /// </summary>
  [Fact]
  public async Task Save_UnresolvedReference_ThrowsException() {

    var parent                  = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");
    var topic                   = new Topic("Test", "Page", parent);
    var reference               = new Topic("Reference", "Page", parent);

    topic.References.SetValue("Test", reference);

    await Assert.ThrowsAsync<ReferentialIntegrityException>(() =>
      _topicRepository.Save(topic, true)
    );

  }

  /*============================================================================================================================
  | TEST: SAVE: INVALID CONTENT TYPE: THROWS EXCEPTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> with an invalid <see cref="Topic.ContentType"/> and confirms that it throws the
  ///   expected <see cref="ReferentialIntegrityException"/>.
  /// </summary>
  [Fact]
  public async Task Save_InvalidContentType_ThrowsException() =>
    await Assert.ThrowsAsync<ReferentialIntegrityException>(() =>
      _topicRepository.Save(new("Test", "InvalidContentType"))
    );

  /*============================================================================================================================
  | TEST: DELETE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then deletes one of the
  ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepository.Delete(Topic, Boolean)"/>, and ensures that it
  ///   is immediately reflected in the <see cref="TopicRepository"/> cache of <see cref="ContentTypeDescriptor"/>s.
  /// </summary>
  [Fact]
  public async Task Delete_ContentTypeDescriptor_UpdatesContentTypeCache() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var contentType             = contentTypes.Contains("Page")? contentTypes["Page"] : null;

    Contract.Assume(contentType);

    await _topicRepository.Delete(contentType);

    Assert.DoesNotContain(contentType, contentTypes);

  }

  /*============================================================================================================================
  | TEST: MOVE: AFTER SIBLING: SET CORRECTLY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Moves a <see cref="Topic"/> after a sibling in another parent, and ensures it is set correctly.
  /// </summary>
  [Fact]
  public async Task Move_AfterSibling_SetCorrectly() {

    var source                  = new Topic("Source", "Page");
    var topic                   = new Topic("Test", "Page", source);
    var target                  = new Topic("Target", "Page");
    var sibling                 = new Topic("Sibling", "Page", target);
    var olderSibling            = new Topic("OlderSibling", "Page", target);

    await _topicRepository.Move(topic, target, sibling);

    Assert.Equal(target, topic.Parent);
    Assert.Equal(0, target.Children.IndexOf(sibling));
    Assert.Equal(1, target.Children.IndexOf(topic));
    Assert.Equal(2, target.Children.IndexOf(olderSibling));

  }

  /*============================================================================================================================
  | TEST: MOVE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then moves one of the
  ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepository.Move(Topic, Topic, Topic?)"/>, and ensures
  ///   that it is immediately reflected in the <see cref="TopicRepository"/> cache of <see
  ///   cref="ContentTypeDescriptor"/>s.
  /// </summary>
  [Fact]
  public async Task Move_ContentTypeDescriptor_UpdatesContentTypeCache() {

    var contentTypes            = _topicRepository.GetContentTypeDescriptors();
    var pageContentType         = contentTypes.Contains("Page")? contentTypes["Page"] : null;
    var contactContentType      = contentTypes.Contains("Contact")? contentTypes["Contact"] : null;
    var contactAttributeCount   = contactContentType?.AttributeDescriptors.Count;

    Contract.Assume(contactContentType);
    Contract.Assume(pageContentType);

    await _topicRepository.Move(contactContentType, pageContentType);

    Assert.NotEqual(contactContentType.AttributeDescriptors.Count, contactAttributeCount);

  }

  /*============================================================================================================================
  | TEST: SAVE: ATTRIBUTE DESCRIPTOR: UPDATES CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="ContentTypeDescriptor"/> with a child <see cref="ContentTypeDescriptor"/> . Adds a new <see
  ///   cref="AttributeDescriptor"/> to the parent. Ensures that the <see cref="ContentTypeDescriptor.AttributeDescriptors"/>
  ///   of the child reflects the change.
  /// </summary>
  [Fact]
  public async Task Save_AttributeDescriptor_UpdatesContentType() {

    var contentType             = new ContentTypeDescriptor("Parent", "ContentTypeDescriptor", null, 1);
    var attributeList           = new Topic("Attributes", "List", contentType, 2);
    var childContentType        = new ContentTypeDescriptor("Child", "ContentTypeDescriptor", contentType, 3);

    Contract.Assume(childContentType);

    var attributeCount          = childContentType.AttributeDescriptors.Count;

    var newAttribute            = new BooleanAttributeDescriptor("NewAttribute", "BooleanAttributeDescriptor", attributeList);

    Contract.Assume(newAttribute);

    await _topicRepository.Save(newAttribute);

    Assert.Equal(attributeCount+1, childContentType.AttributeDescriptors.Count);

  }

  /*============================================================================================================================
  | TEST: DELETE: ATTRIBUTE DESCRIPTOR: UPDATES CONTENT TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="ContentTypeDescriptor"/> with a child <see cref="ContentTypeDescriptor"/> . Adds a new <see
  ///   cref="AttributeDescriptor"/> to the parent, then immediately deletes it. Ensures that the <see
  ///   cref="ContentTypeDescriptor.AttributeDescriptors"/> of the child reflects the change.
  /// </summary>
  [Fact]
  public async Task Delete_AttributeDescriptor_UpdatesContentTypeCache() {

    var contentType             = new ContentTypeDescriptor("Parent", "ContentTypeDescriptor");
    var attributeList           = new Topic("Attributes", "List", contentType);
    var newAttribute            = new BooleanAttributeDescriptor("NewAttribute", "BooleanAttributeDescriptor", attributeList);
    var childContentType        = new ContentTypeDescriptor("Child", "ContentTypeDescriptor", contentType);

    Contract.Assume(childContentType);
    Contract.Assume(newAttribute);

    var attributeCount          = childContentType.AttributeDescriptors.Count;

    await _topicRepository.Delete(newAttribute);

    Assert.True(childContentType.AttributeDescriptors.Count < attributeCount);

  }

  /*============================================================================================================================
  | TEST: LOAD: TOPIC LOADED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic using <see cref="StubTopicRepository.Load(Int32, Topic?, Boolean, TopicPayload)"/> and ensures that the
  ///   <see cref="ITopicRepository.TopicLoaded"/> event is raised.
  /// </summary>
  [Fact]
  public async Task Load_TopicLoadedEvent_IsRaised() {

    var hasFired                = false;

    _cachedTopicRepository.TopicLoaded += eventHandler;

    var topic                   = await _topicRepository.Load("Root:Web");

    _cachedTopicRepository.TopicLoaded -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicLoadEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: LOAD: TOPIC LOADED EVENT: IS RAISED WITH VERSION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a topic using <see cref="StubTopicRepository.Load(Int32, DateTime, Topic?)"/> and ensures that the <see cref=
  ///   "ITopicRepository.TopicLoaded"/> event is raised.
  /// </summary>
  [Fact]
  public async Task Load_TopicLoadedEvent_IsRaisedWithVersion() {

    var hasFired                = false;
    var topicId                 = (await _topicRepository.Load("Root:Web"))?.Id;
    var version                 = DateTime.UtcNow;

    _cachedTopicRepository.TopicLoaded += eventHandler;

    var topic                   = await _topicRepository.Load(topicId?? -1, version);

    _cachedTopicRepository.TopicLoaded -= eventHandler;

    Assert.True(hasFired);
    Assert.Equal(topicId, topic?.Id);
    Assert.Equal(version, topic?.VersionHistory.LastOrDefault());

    void eventHandler(object? sender, TopicLoadEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: DELETE: TOPIC DELETED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/> and then immediately deletes it. Ensures that the <see cref=
  ///   "ITopicRepository.TopicDeleted"/> event is raised.
  /// </summary>
  [Fact]
  public async Task Delete_TopicDeletedEvent_IsRaised() {

    var topic                   = new Topic("Test", "Page");
    var hasFired                = false;

    await _cachedTopicRepository.Save(topic);
    _cachedTopicRepository.TopicDeleted += eventHandler;
    await _cachedTopicRepository.Delete(topic);
    _cachedTopicRepository.TopicDeleted -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: SAVE: TOPIC SAVED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/> and then immediately saves it. Ensures that the <see cref="ITopicRepository.TopicSaved"
  ///   /> event is raised.
  /// </summary>
  [Fact]
  public async Task Save_TopicSavedEvent_IsRaised() {

    var topic                   = new Topic("Test", "Page");
    var hasFired                = false;

    _cachedTopicRepository.TopicSaved += eventHandler;
    await _cachedTopicRepository.Save(topic);
    _cachedTopicRepository.TopicSaved -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicSaveEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: SAVE: TOPIC RENAMED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/> and then immediately saves it. Ensures that the <see cref="ITopicRepository.TopicRenamed"
  ///   /> event is raised.
  /// </summary>
  [Fact]
  public async Task Save_TopicRenamedEvent_IsRaised() {

    var topic                   = new Topic("Test", "Page", null, 1);
    var hasFired                = false;

    topic.Key                   = "New";

    _cachedTopicRepository.TopicRenamed += eventHandler;
    await _cachedTopicRepository.Save(topic);
    _cachedTopicRepository.TopicRenamed -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicRenameEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: SAVE: TOPIC MOVED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/>, changes its parent, and then saves it. Ensures that the <see cref=
  ///   "ITopicRepository.TopicMoved"/> event is raised.
  /// </summary>
  [Fact]
  public async Task Save_TopicMovedEvent_IsRaised() {

    var topic                   = new Topic("Test", "Page", null, 1);
    var parent                  = new Topic("Products", "Page", null, 2);
    var hasFired                = false;

    topic.Parent                = parent;

    _cachedTopicRepository.TopicMoved += eventHandler;
    await _cachedTopicRepository.Save(topic);
    _cachedTopicRepository.TopicMoved -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicMoveEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: SAVE: NEW TOPIC: STAMPS RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Saves a new <see cref="Topic"/> and confirms that the repository stamps a <see cref="ITopicLoadResolver"/> onto it so
  ///   that deferred boundaries can be populated on demand after the save.
  /// </summary>
  [Fact]
  public async Task Save_NewTopic_StampsResolver() {

    var parent                  = await _topicRepository.Load("Root:Web:Web_3:Web_3_0");
    var topic                   = new Topic("Test", "Page", parent);

    await _topicRepository.Save(topic);

    Assert.NotNull(topic.Resolver);

  }

  /*============================================================================================================================
  | TEST: SAVE: NOT LOADED CHILDREN: SKIPS RECURSIVE DESCENT
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a parent topic with a child, marks the parent's <see cref="Topic.Children"/> as <see cref="LoadState.NotLoaded"
  ///   />, then saves recursively. Verifies that the child is not saved; the recursive-save loop is gated on <see cref=
  ///   "Topic.IsLoaded(TopicPayload)"/>, so a not-loaded children collection prevents descent.
  /// </summary>
  [Fact]
  public async Task Save_NotLoadedChildren_SkipsRecursiveDescent() {

    var parent                  = new Topic("Parent", "Page");
    var child                   = new Topic("Child", "Page", parent);

    parent.Children.LoadState   = LoadState.NotLoaded;

    await _topicRepository.Save(parent, isRecursive: true);

    Assert.True(child.IsNew);

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: EXTENDED ATTRIBUTES NOT LOADED: MARKS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose extended-attribute boundary has been manually set to <see cref="LoadState.NotLoaded"/>
  ///   and confirms that <see cref="Topic.EnsureLoaded(TopicPayload)"/> promotes the boundary to <see cref="LoadState.Loaded"
  ///   /> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_ExtendedAttributesNotLoaded_MarksLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.Attributes.LoadState = LoadState.NotLoaded;
    topic.EnsureLoaded(TopicPayload.ExtendedAttributes);

    Assert.True(topic.IsLoaded(TopicPayload.ExtendedAttributes));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: MIXED BOUNDARIES: SKIPS LOADED BOUNDARIES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="Topic.EnsureLoaded(TopicPayload)"/> with a mixed set of flags, including one already set to <see
  ///   cref="LoadState.Loaded"/> and one <see cref="LoadState.NotLoaded"/>, and confirms that only the pending boundary is
  ///   forwarded to the resolver, leaving the already-loaded boundary unchanged.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_MixedBoundaries_SkipsLoadedBoundaries() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.Attributes.LoadState = LoadState.NotLoaded;
    Assert.True(topic.IsLoaded(TopicPayload.Children));
    topic.EnsureLoaded(TopicPayload.Children | TopicPayload.ExtendedAttributes);

    Assert.True(topic.IsLoaded(TopicPayload.ExtendedAttributes));
    Assert.True(topic.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: RELATIONSHIPS NOT LOADED: MARKS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose relationship boundary has been manually set to <see cref="LoadState.NotLoaded"/>
  ///   and confirms that <see cref="Topic.EnsureLoaded(TopicPayload)"/> promotes the boundary to <see cref="LoadState.Loaded"
  ///   /> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  /// <remarks>
  ///   On-demand fetching of non-resident relationship targets is deferred to <c>lazy-loading-plan.md</c> Task 6. A stub fill
  ///   simply marks the boundary <see cref="LoadState.Loaded"/>; the SQL resolver leaves it <see cref="LoadState.NotLoaded"/>
  ///   whenever any target is not resident in the single-node topic index it currently uses.
  /// </remarks>
  [Fact]
  public async Task EnsureLoaded_RelationshipsNotLoaded_MarksLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.Relationships.Deferred.Add(new("_stub", 11111));
    topic.EnsureLoaded(TopicPayload.Relationships);

    Assert.True(topic.IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: REFERENCES NOT LOADED: MARKS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose reference boundary has been manually set to <see cref="LoadState.NotLoaded"/> and
  ///   confirms that <see cref="Topic.EnsureLoaded(TopicPayload)"/> promotes the boundary to <see cref="LoadState.Loaded"/>
  ///   via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  /// <remarks>
  ///   On-demand fetching of non-resident reference targets is deferred to <c>lazy-loading-plan.md</c> Task 6. A stub fill
  ///   simply marks the boundary <see cref="LoadState.Loaded"/>; the SQL resolver leaves it <see cref="LoadState.NotLoaded"/>
  ///   whenever any target is not resident in the single-node topic index it currently uses.
  /// </remarks>
  [Fact]
  public async Task EnsureLoaded_ReferencesNotLoaded_MarksLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.References.Deferred.Add(new("_stub", 11111));
    topic.EnsureLoaded(TopicPayload.References);

    Assert.True(topic.IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: CHILDREN NOT LOADED STATE: TRIGGERS ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/>, marks its <see cref="Topic.Children"/> as <see cref="LoadState.NotLoaded"/>, then accesses
  ///   the <see cref="Topic.Children"/> getter. Verifies that the autoload fires, promoting the boundary to <see cref=
  ///   "LoadState.Loaded"/> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  [Fact]
  public async Task IsLoaded_ChildrenNotLoadedState_TriggersEnsureLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.SetLoadState(TopicPayload.Children, LoadState.NotLoaded);
    _                           = topic.Children;

    Assert.True(topic.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: CHILDREN LOADED STATE: DOES NOT CALL RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose <see cref="Topic.Children"/> is already <see cref="LoadState.Loaded"/> and accesses
  ///   the <see cref="Topic.Children"/> getter. Verifies that the boundary stays <see cref="LoadState.Loaded"/> without the
  ///   resolver being called redundantly.
  /// </summary>
  [Fact]
  public async Task IsLoaded_ChildrenLoadedState_DoesNotCallResolver() {

    var topic                   = await _topicRepository.Load(11111);

    Assert.True(topic!.IsLoaded(TopicPayload.Children));
    _                           = topic.Children;

    Assert.True(topic.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: RELATIONSHIPS NOT LOADED STATE: TRIGGERS ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/>, marks its <see cref="Topic.Relationships"/> as <see cref="LoadState.NotLoaded"/>, then
  ///   accesses the <see cref="Topic.Relationships"/> getter. Verifies that the autoload fires, promoting the boundary to
  ///   <see cref="LoadState.Loaded"/> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  [Fact]
  public async Task IsLoaded_RelationshipsNotLoadedState_TriggersEnsureLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.Relationships.Deferred.Add(new("_stub", 11111));
    _                           = topic.Relationships;

    Assert.True(topic.IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: RELATIONSHIPS LOADED STATE: DOES NOT CALL RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose <see cref="Topic.Relationships"/> is already <see cref="LoadState.Loaded"/> and
  ///   accesses the getter. Verifies that the boundary stays <see cref="LoadState.Loaded"/> without the resolver being called
  ///   redundantly.
  /// </summary>
  [Fact]
  public async Task IsLoaded_RelationshipsLoadedState_DoesNotCallResolver() {

    var topic                   = await _topicRepository.Load(11111);

    Assert.True(topic!.IsLoaded(TopicPayload.Relationships));
    _                           = topic.Relationships;

    Assert.True(topic.IsLoaded(TopicPayload.Relationships));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: REFERENCES NOT LOADED STATE: TRIGGERS ENSURE LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/>, marks its <see cref="Topic.References"/> as <see cref="LoadState.NotLoaded"/>, then
  ///   accesses the <see cref="Topic.References"/> getter. Verifies that the autoload fires, promoting the boundary to
  ///   <see cref="LoadState.Loaded"/> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  [Fact]
  public async Task IsLoaded_ReferencesNotLoadedState_TriggersEnsureLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.References.Deferred.Add(new("_stub", 11111));
    _                           = topic.References;

    Assert.True(topic.IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: REFERENCES LOADED STATE: DOES NOT CALL RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose <see cref="Topic.References"/> is already <see cref="LoadState.Loaded"/> and accesses
  ///   the getter. Verifies that the boundary stays <see cref="LoadState.Loaded"/> without the resolver being called
  ///   redundantly.
  /// </summary>
  [Fact]
  public async Task IsLoaded_ReferencesLoadedState_DoesNotCallResolver() {

    var topic                   = await _topicRepository.Load(11111);

    Assert.True(topic!.IsLoaded(TopicPayload.References));
    _                           = topic.References;

    Assert.True(topic.IsLoaded(TopicPayload.References));

  }

  /*============================================================================================================================
  | TEST: IS LOADED: CHILDREN NOT LOADED: MARKS LOADED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Loads a <see cref="Topic"/> whose <see cref="Topic.Children"/> has been manually set to <see cref="LoadState.NotLoaded"
  ///   /> and confirms that <see cref="Topic.EnsureLoaded(TopicPayload)"/> promotes the boundary to <see cref=
  ///   "LoadState.Loaded"/> via the <see cref="StubTopicRepository"/>'s fill.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_ChildrenNotLoaded_MarksLoaded() {

    var topic                   = await _topicRepository.Load(11111);

    topic!.SetLoadState(TopicPayload.Children, LoadState.NotLoaded);
    topic.EnsureLoaded(TopicPayload.Children);

    Assert.True(topic.IsLoaded(TopicPayload.Children));

  }

  /*============================================================================================================================
  | TEST: MOVE: TOPIC MOVED EVENT: IS RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/> and then immediately moves it. Ensures that the <see cref="ITopicRepository.TopicMoved"
  ///   /> event is raised.
  /// </summary>
  [Fact]
  public async Task Move_TopicMovedEvent_IsRaised() {

    var topic                   = new Topic("Test", "Page", null, 1);
    var parent                  = new Topic("Products", "Page", null, 2);
    var hasFired                = false;

    _cachedTopicRepository.TopicMoved += eventHandler;
    await _cachedTopicRepository.Move(topic, parent);
    _cachedTopicRepository.TopicMoved -= eventHandler;

    Assert.True(hasFired);

    void eventHandler(object? sender, TopicMoveEventArgs eventArgs) => hasFired = true;

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: WITH MISSING RELATIONSHIP TARGET: RESOLVES AND CONNECTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.EnsureLoaded"/> on a topic whose <c>Relationships.LoadState</c> is <c>NotLoaded</c>,
  ///   confirming that the resolver re-queries for the topic's relationships, loads any missing targets, and connects the
  ///   edges.
  /// </summary>
  /// <remarks>
  ///   The stub pre-seeds a relationship from the root topic to Root:Web (id 10000). The cache is initialized with only the
  ///   root topic, so the relationship target is initially absent. <c>EnsureLoaded</c> is expected to load it and connect the
  ///   edge through the full resolver stack.
  /// </remarks>
  [Fact]
  public async Task EnsureLoaded_WithMissingRelationshipTarget_ResolvesAndConnects() {

    // Get the root topic from cache; seed a deferred entry to simulate a pending relationship target
    var source                  = (await _cachedTopicRepository.Load(-1))!;
    source.Relationships.Deferred.Add(new("_stub", 11111));

    // Act: EnsureLoaded re-queries, finds the missing target, loads it, and connects the edge
    _cachedTopicRepository.EnsureLoaded(source, TopicPayload.Relationships);

    // Relationships are now Loaded and any pre-seeded edges are connected
    Assert.Equal(LoadState.Loaded, source.Relationships.LoadState);

  }

  /*============================================================================================================================
  | TEST: ENSURE LOADED: RELATIONSHIPS: ALREADY LOADED: SKIPS FILL
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="CachedTopicRepository.EnsureLoaded"/> on a topic whose relationships are already <see cref=
  ///   "LoadState.Loaded"/> and confirms it returns immediately without re-querying.
  /// </summary>
  [Fact]
  public async Task EnsureLoaded_Relationships_AlreadyLoaded_SkipsFill() {

    // Get the root topic; relationships start as Loaded (Deferred is empty) after initialization
    var source                  = (await _cachedTopicRepository.Load(-1))!;

    // Act
    _cachedTopicRepository.EnsureLoaded(source, TopicPayload.Relationships);

    // LoadState is unchanged; no fill was triggered
    Assert.Equal(LoadState.Loaded, source.Relationships.LoadState);

  }

  /*============================================================================================================================
  | TEST: LOAD: WITH ASCENDANTS: STAMPS ASCENDANT RESOLVERS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Calls <see cref="StubTopicRepository.Load(String, Topic?, Boolean, TopicPayload)"/> on a standalone instance—i.e., not
  ///   wrapped by <see cref="CachedTopicRepository"/>, and never itself passed to <c>Load()</c> before—for a deeply nested
  ///   topic, and confirms that an ascendant is nonetheless stamped with an <see cref="ITopicLoadResolver"/>, so its own
  ///   deferred payload can still be lazy-loaded.
  /// </summary>
  /// <remarks>
  ///   Uses a fresh <see cref="StubTopicRepository"/> rather than the shared <see cref="_topicRepository"/> field: The latter
  ///   is wrapped by <see cref="_cachedTopicRepository"/> in the constructor, whose own seeding recursively stamps the entire
  ///   (eagerly loaded) stub tree via <see cref="Topic.Children"/>, which would mask whether ascendant stamping actually comes
  ///   from this test's <c>Load()</c> call.
  /// </remarks>
  [Fact]
  public async Task Load_WithAscendants_StampsAscendantResolvers() {

    // Arrange: use a standalone repository, never wrapped by CachedTopicRepository
    var topicRepository         = new StubTopicRepository();

    // Act: load a deeply nested topic
    var topic                   = await topicRepository.Load("Root:Web:Web_3:Web_3_1:Web_3_1_0");

    // An ascendant that was never itself the target of a Load() call is still stamped
    var ascendant                = topic?.Parent?.Parent;

    Assert.NotNull(ascendant);
    Assert.NotNull(ascendant?.Resolver);

  }

  /*============================================================================================================================
  | TEST: MOVE: SAME LOCATION: EVENT NOT RAISED
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Creates a <see cref="Topic"/> and then moves it to the exact same location in the tree. Ensures that the <see cref=
  ///   "ITopicRepository.TopicMoved"/> event is not raised.
  /// </summary>
  [Fact]
  public async Task Move_SameLocation_EventNotRaised() {

    var parent                  = new Topic("Parent", "Page", null, 1);
    var sibling                 = new Topic("Sibling", "Page", parent, 2);
    var topic                   = new Topic("Test", "Page", parent, 3);
    var hasFired                = false;

    _cachedTopicRepository.TopicMoved += eventHandler;
    await _cachedTopicRepository.Move(topic, parent, sibling);
    _cachedTopicRepository.TopicMoved -= eventHandler;

    Assert.False(hasFired);

    void eventHandler(object? sender, TopicMoveEventArgs eventArgs) => hasFired = true;

  }

} //Class