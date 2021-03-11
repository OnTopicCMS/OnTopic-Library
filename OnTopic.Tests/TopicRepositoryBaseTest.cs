/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Collections.Specialized;
using OnTopic.Data.Caching;
using OnTopic.Data.Sql;
using OnTopic.Data.Sql.Models;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using OnTopic.TestDoubles.Metadata;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY BASE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRepository"/> class.
  /// </summary>
  /// <remarks>
  ///   These tests evaluate features that are specific to the <see cref="TopicRepository"/> class.
  /// </remarks>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class TopicRepositoryBaseTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    StubTopicRepository             _topicRepository;
    readonly                    CachedTopicRepository           _cachedTopicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
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
      _topicRepository          = new StubTopicRepository();
      _cachedTopicRepository    = new CachedTopicRepository(_topicRepository);
    }

    /*==========================================================================================================================
    | TEST: LOAD: VALID TOPIC ID: RETURNS EXPECTED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean)"/> with a valid <see cref="Topic.Id"/> and
    ///   confirms that the expected topic is returned.
    /// </summary>
    [TestMethod]
    public void Load_ValidTopicId_ReturnsExpectedTopic() {

      var topic                 = _topicRepository.Load(11111);

      Assert.AreEqual<int?>(11111, topic?.Id);

    }

    /*==========================================================================================================================
    | TEST: LOAD: INVALID TOPIC ID: RETURNS EXPECTED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean)"/> with an invalid <see cref="Topic.Id"/> and
    ///   confirms that no topic is returned.
    /// </summary>
    [TestMethod]
    public void Load_InvalidTopicId_ReturnsExpectedTopic() {

      var topic                 = _topicRepository.Load(11113);

      Assert.IsNull(topic);

    }

    /*==========================================================================================================================
    | TEST: LOAD: NEGATIVE TOPIC ID: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, Topic?, Boolean)"/> with a negative <see cref="Topic.Id"/> and
    ///   confirms that the root topic is returned.
    /// </summary>
    [TestMethod]
    public void Load_NegativeTopicId_ReturnsRootTopic() {

      var topic                 = _cachedTopicRepository.Load(-2);

      Assert.AreEqual<string?>("Root", topic?.GetUniqueKey());

    }

    /*==========================================================================================================================
    | TEST: LOAD: VALID DATE: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a valid data and ensures that topic
    ///   with that date is returned.
    /// </summary>
    [TestMethod]
    public void Load_ValidDate_ReturnsTopic() {

      var version               = DateTime.UtcNow.AddDays(-1);
      var topic                 = _topicRepository.Load(11111, version);

      Assert.IsTrue(topic?.VersionHistory.Contains(version));
      Assert.AreEqual<DateTime?>(version.AddTicks(-(version.Ticks % TimeSpan.TicksPerSecond)), topic?.LastModified);

    }

    /*==========================================================================================================================
    | TEST: LOAD: FUTURE DATE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a future <see cref="DateTime"/> and
    ///   confirms that an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Load_FutureDate_ThrowsException() =>
      _cachedTopicRepository.Load(1111, DateTime.UtcNow.AddDays(1));

    /*==========================================================================================================================
    | TEST: LOAD: OLD DATE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="CachedTopicRepository.Load(Int32, DateTime, Topic?)"/> with a date prior to versioning being
    ///   introduced and ensures that an exception is thrown.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Load_OldDate_ThrowsException() =>
      _cachedTopicRepository.Load(1111, new DateTime(2010, 10, 15));

    /*==========================================================================================================================
    | TEST: DELETE: BASE TOPIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic which other topics, outside of the graph, derive from. Expects exception.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ReferentialIntegrityException))]
    public void Delete_BaseTopic_ThrowsException() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      var child                 = TopicFactory.Create("Child", "Page", topic);
      var derivedTopic          = TopicFactory.Create("Derived", "Page", root);

      derivedTopic.BaseTopic    = child;

      _topicRepository.Delete(topic, true);

    }

    /*==========================================================================================================================
    | TEST: DELETE: INTERNAL DERIVED TOPIC: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic which another topic within the graph derives from. Expects success.
    /// </summary>
    [TestMethod]
    public void Delete_InternallyDerivedTopic_Succeeds() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      var child                 = TopicFactory.Create("Child", "Page", topic);
      var derived               = TopicFactory.Create("Derived", "Page", topic);

      derived.BaseTopic         = child;

      _topicRepository.Delete(topic, true);

      Assert.AreEqual<int>(0, root.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DELETE: DESCENDANTS: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic with descendant topics. Expects exception if <c>isRecursive</c> is set to <c>false</c>.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ReferentialIntegrityException))]
    public void Delete_Descendants_ThrowsException() {

      var topic                 = TopicFactory.Create("Topic", "Page");
      _                         = TopicFactory.Create("Child", "Page", topic);

      _topicRepository.Delete(topic, false);

    }

    /*==========================================================================================================================
    | TEST: DELETE: DESCENDANTS WITH RECURSIVE: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic with descendant topics. Expects no exception if <c>isRecursive</c> is set to <c>true</c>.
    /// </summary>
    [TestMethod]
    public void Delete_DescendantsWithRecursive_Succeeds() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      _                         = TopicFactory.Create("Child", "Page", topic);

      _topicRepository.Delete(topic, true);

      Assert.AreEqual<int>(0, root.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DELETE: NESTED TOPICS: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic with nested topics. Expects no exception, even if <c>isRecursive</c> is set to <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Delete_NestedTopics_Succeeds() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      _                         = TopicFactory.Create("Child", "List", topic);

      _topicRepository.Delete(topic, false);

      Assert.AreEqual<int>(0, root.Children.Count);

    }

    /*==========================================================================================================================
    | TEST: DELETE: RELATIONSHIPS: DELETE RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic with outgoing relationships. Deletes those relationships from that topic's <see cref=
    ///   "Topic.IncomingRelationships"/> collection.
    /// </summary>
    [TestMethod]
    public void Delete_Relationships_DeleteRelationships() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      var child                 = TopicFactory.Create("Child", "Page", topic);
      var related               = TopicFactory.Create("Related", "Page", root);

      child.Relationships.SetValue("Related", related);

      _topicRepository.Delete(topic, true);

      Assert.AreEqual<int>(0, related.IncomingRelationships.GetValues("Related").Count);

    }

    /*==========================================================================================================================
    | TEST: DELETE: INCOMING RELATIONSHIPS: DELETE RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic with incoming relationships. Deletes those relationships from that topic's <see cref=
    ///   "Topic.Relationships"/> collection.
    /// </summary>
    [TestMethod]
    public void Delete_IncomingRelationships_DeleteRelationships() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      var child                 = TopicFactory.Create("Child", "Page", topic);
      var related               = TopicFactory.Create("Related", "Page", root);

      related.Relationships.SetValue("Related", child);

      _topicRepository.Delete(topic, true);

      Assert.AreEqual<int>(0, related.Relationships.GetValues("Related").Count);

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: ANY ATTRIBUTES: RETURNS ALL ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, without any filtering by whether or not the attribute is an <see
    ///   cref="AttributeDescriptor.IsExtendedAttribute"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_AnyAttributes_ReturnsAllAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title");

      var attributes            = _topicRepository.GetAttributesProxy(topic, null);

      Assert.AreEqual<int>(1, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EMPTY ATTRIBUTES: SKIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, without any filtering by whether or not the attribute is an <see
    ///   cref="AttributeDescriptor.IsExtendedAttribute"/>. Any <see cref="AttributeRecord"/>s with a null or empty value should
    ///   be skipped.
    /// </summary>
    [TestMethod]
    public void GetAttributes_EmptyAttributes_Skips() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("EmptyAttribute", "");
      topic.Attributes.SetValue("NullAttribute", null);

      var attributes            = _topicRepository.GetAttributesProxy(topic, null);

      Assert.IsFalse(attributes.Any(a => a.Key is "EmptyAttribute"));
      Assert.IsFalse(attributes.Any(a => a.Key is "NullAttribute"));

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: INDEXED ATTRIBUTES: RETURNS INDEXED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_IndexedAttributes_ReturnsIndexedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title");

      var attributes            = _topicRepository.GetAttributesProxy(topic, false);

      Assert.AreEqual<int>(0, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTES: RETURNS EXTENDED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExtendedAttributes_ReturnsExtendedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title");

      var attributes            = _topicRepository.GetAttributesProxy(topic, true);

      Assert.AreEqual<int>(1, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTE MISMATCH: RETURNS EXTENDED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="TrackedRecord{T}.IsDirty"/>. Expects an <see
    ///   cref="AttributeRecord"/> to be returned even if it's <i>not</i> <see cref="TrackedRecord{T}.IsDirty"/> <i>but</i> its
    ///   <see cref="AttributeRecord.IsExtendedAttribute"/> disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExtendedAttributeMismatch_ReturnsExtendedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title", markDirty:false, isExtendedAttribute:false);

      var attributes            = _topicRepository.GetAttributesProxy(topic, true, true);

      //Expect Title, even though it isn't IsDirty
      Assert.AreEqual<int>(1, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTE MISMATCH: RETURNS NOTHING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="TrackedRecord{T}.IsDirty"/>. Expects the <see
    ///   cref="AttributeRecord"/> to <i>not</i> be returned even though its <see cref="AttributeRecord.IsExtendedAttribute"/>
    ///   disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>, since it won't match the <see
    ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>'s <c>isExtendedAttribute</c> call.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExtendedAttributeMismatch_ReturnsNothing() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title", markDirty: false, isExtendedAttribute: false);

      var attributes            = _topicRepository.GetAttributesProxy(topic, false, true);

      Assert.AreEqual<int>(0, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXCLUDE LAST MODIFIED: RETURNS OTHER ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <c>excludeLastModified</c>. Confirms that <see
    ///   cref="AttributeRecord"/>s are not returned which start with <c>LastModified</c>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExcludeLastModified_ReturnsOtherAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetDateTime("LastModified", DateTime.Now);
      topic.Attributes.SetValue("LastModifiedBy", "Unit Tests");

      var attributes            = _topicRepository.GetAttributesProxy(topic, null, excludeLastModified: true);

      Assert.AreEqual<int>(0, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH SHORT VALUE: RETURNS AS INDEXED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value shorter than 255 characters, then
    ///   ensures that it is returned as an an <i>indexed</i> <see cref="AttributeRecord"/> when calling <see
    ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ArbitraryAttributeWithShortValue_ReturnsAsIndexedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("ArbitraryAttribute", "Value");

      var attributes            = _topicRepository.GetAttributesProxy(topic, false);

      Assert.IsTrue(attributes.Any(a => a.Key is "ArbitraryAttribute"));

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH LONG VALUE: RETURNS AS EXTENDED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value longer than 255 characters, then
    ///   ensures that it is returned as an an <see cref="AttributeDescriptor.IsExtendedAttribute"/> when calling <see
    ///   cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ArbitraryAttributeWithLongValue_ReturnsAsExtendedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("ArbitraryAttribute", new string('x', 256));

      var attributes            = _topicRepository.GetAttributesProxy(topic, true);

      Assert.IsTrue(attributes.Any(a => a.Key is "ArbitraryAttribute"));

    }

    /*==========================================================================================================================
    | TEST: GET UNMATCHED ATTRIBUTES: RETURNS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Using <see cref="TopicRepository.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
    ///   <see cref="ContentTypeDescriptor"/> but not the <see cref="Topic"/> are returned.
    /// </summary>
    [TestMethod]
    public void GetUnmatchedAttributes_ReturnsAttributes() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);

      topic.Attributes.SetValue("Title", "Title");

      var attributes            = _topicRepository.GetUnmatchedAttributesProxy(topic);

      Assert.IsTrue(attributes.Any());
      Assert.IsFalse(attributes.Any(a => a.Key is "Title"));

    }

    /*==========================================================================================================================
    | TEST: GET UNMATCHED ATTRIBUTES: EMPTY ARBITRARY ATTRIBUTES: RETURNS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Using <see cref="TopicRepository.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
    ///   <see cref="Topic"/> but not the <see cref="ContentTypeDescriptor"/> <i>and</i> are either <c>null</c> or empty are
    ///   returned. This ensures that arbitrary attributes can be deleted programmatically, instead of lingering as orphans in
    ///   the database.
    /// </summary>
    [TestMethod]
    public void GetUnmatchedAttributes_EmptyArbitraryAttributes_ReturnsAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor", 1);

      topic.Attributes.SetValue("ArbitraryAttribute", "Value");
      topic.Attributes.SetValue("ArbitraryAttribute", "");
      topic.Attributes.SetValue("AnotherArbitraryAttribute", "Value");
      topic.Attributes.SetValue("YetAnotherArbitraryAttribute", "Value");
      topic.Attributes.SetValue("YetAnotherArbitraryAttribute", null);

      var attributes            = _topicRepository.GetUnmatchedAttributesProxy(topic);

      Assert.IsTrue(attributes.Any(a => a.Key is "ArbitraryAttribute"));
      Assert.IsTrue(attributes.Any(a => a.Key is "YetAnotherArbitraryAttribute"));
      Assert.IsFalse(attributes.Any(a => a.Key is "AnotherArbitraryAttribute"));

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE DESCRIPTORS: RETURNS CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="ContentTypeDescriptor"/>s from the <see cref="ITopicRepository"/> and ensures that
    ///   the expected number (2) are present.
    /// </summary>
    [TestMethod]
    public void GetContentTypeDescriptors_ReturnsContentTypes() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();

      Assert.AreEqual<int>(15, contentTypes.Count);
      Assert.IsNotNull(contentTypes.GetValue("ContentTypeDescriptor"));
      Assert.IsNotNull(contentTypes.GetValue("Page"));
      Assert.IsNotNull(contentTypes.GetValue("LookupListItem"));

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE DESCRIPTORS: WITH TOPIC GRAPH: RETURNS MERGED CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of <see cref="ContentTypeDescriptor"/>s from the <see cref="ITopicRepository"/> alongside a separate
    ///   topic graph and ensures the two are properly merged.
    /// </summary>
    [TestMethod]
    public void GetContentTypeDescriptors_WithTopicGraph_ReturnsMergedContentTypes() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var rootContentType       = contentTypes["ContentTypes"];
      var newContentType        = TopicFactory.Create("NewContentType", "ContentTypeDescriptor", rootContentType);
      var contentTypeCount      = contentTypes.Count;

      _topicRepository.SetContentTypeDescriptorsProxy(rootContentType);

      Assert.AreNotEqual<int>(contentTypeCount, contentTypes.Count);
      Assert.IsNotNull(contentTypes.Contains(newContentType));

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE DESCRIPTOR: GET VALID CONTENT TYPE: RETURNS CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to retrieve a specific <see cref="ContentTypeDescriptor"/> by its <see cref="Topic.Key"/>.
    /// </summary>
    [TestMethod]
    public void GetContentTypeDescriptor_GetValidContentType_ReturnsContentType() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var contentType           = _topicRepository.GetContentTypeDescriptorProxy(topic);

      Assert.IsNotNull(contentType);

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE DESCRIPTOR: GET NEW CONTENT TYPE: RETURNS FROM TOPIC GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to retrieve a <see cref="ContentTypeDescriptor"/> that hasn't yet been persisted to the data store. Instead,
    ///   attempts to retrieve it from the <see cref="Topic"/>'s graph.
    /// </summary>
    [TestMethod]
    public void GetContentTypeDescriptor_GetNewContentType_ReturnsFromTopicGraph() {

      var rootTopic             = _topicRepository.Load("Root");
      var contentTypes          = _topicRepository.Load("Root:Configuration:ContentTypes");
      var newContentType        = TopicFactory.Create("NewContentType", "ContentTypeDescriptor", contentTypes);
      var topic                 = TopicFactory.Create("Test", "NewContentType", rootTopic);

      var contentType           = _topicRepository.GetContentTypeDescriptorProxy(topic);

      Assert.IsNotNull(contentType);
      Assert.ReferenceEquals(contentType, newContentType);

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE DESCRIPTOR: GET INVALID CONTENT TYPE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to retrieve an invalid <see cref="ContentTypeDescriptor"/>.
    /// </summary>
    [TestMethod]
    public void GetContentTypeDescriptor_GetInvalidContentType_ReturnsNull() {

      var topic                 = TopicFactory.Create("Test", "InvalidContentType");
      var contentType           = _topicRepository.GetContentTypeDescriptorProxy(topic);

      Assert.IsNull(contentType);

    }

    /*==========================================================================================================================
    | TEST: SAVE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then saves a new <see
    ///   cref="ContentTypeDescriptor"/> via <see cref="TopicRepository.Save(Topic, Boolean)"/>, and ensures that it is
    ///   immediately reflected in the <see cref="TopicRepository"/> cache of <see cref="ContentTypeDescriptor"/>s.
    /// </summary>
    [TestMethod]
    public void Save_ContentTypeDescriptor_UpdatesContentTypeCache() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var topic                 = TopicFactory.Create("NewContentType", "ContentTypeDescriptor");

      _topicRepository.Save(topic);

      Assert.IsTrue(contentTypes.Contains(topic));

    }

    /*==========================================================================================================================
    | TEST: SAVE: CONTENT TYPE DESCRIPTOR: UPDATES PERMITTED CONTENT TYPES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepository.GetContentTypeDescriptors()"/>, then saves an existing <see cref=
    ///   "ContentTypeDescriptor"/> via <see cref="TopicRepository.Save(Topic, Boolean)"/>, and ensures that
    ///   it the <see cref="ContentTypeDescriptor.PermittedContentTypes"/> cache is updated.
    /// </summary>
    [TestMethod]
    public void Save_ContentTypeDescriptor_UpdatesPermittedContentTypes() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var contentTypesRoot      = contentTypes.GetValue("ContentTypes");
      var pageContentType       = contentTypes.GetValue("Page");
      var lookupContentType     = contentTypes.GetValue("Lookup");

      Contract.Assume(pageContentType);
      Contract.Assume(lookupContentType);
      Contract.Assume(contentTypesRoot);

      var initialCount          = pageContentType.PermittedContentTypes.Count;

      pageContentType.Relationships.SetValue("ContentTypes", lookupContentType);

      _topicRepository.Save(contentTypesRoot, true);

      Assert.AreNotEqual<int>(initialCount, pageContentType.PermittedContentTypes.Count);

    }

    /*==========================================================================================================================
    | TEST: SAVE: NEW TOPIC: UPDATES VERSION HISTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves a new <see cref="Topic"/> and confirms that the <see cref="Topic.VersionHistory"/> is correctly updated with a
    ///   new version.
    /// </summary>
    [TestMethod]
    public void Save_NewTopic_UpdatesVersionHistory() {

      var parent                = _topicRepository.Load("Root:Web:Web_3:Web_3_0");
      var topic                 = TopicFactory.Create("Test", "Page", parent);

      _topicRepository.Save(topic);

      Assert.IsTrue(topic.VersionHistory.Count > 0);

    }

    /*==========================================================================================================================
    | TEST: SAVE: IS RECURSIVE: SAVES CHILD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves a new <see cref="Topic"/> with a child <see cref="Topic"/> and confirms that the <see cref="Topic.Id"/> of the
    ///   child <see cref="Topic"/> is correctly updated.
    /// </summary>
    [TestMethod]
    public void Save_IsRecursive_SavesChild() {

      var parent                = _topicRepository.Load("Root:Web:Web_3:Web_3_0");
      var topic                 = TopicFactory.Create("Test", "Page", parent);
      var child                 = TopicFactory.Create("Child", "Page", topic);

      _topicRepository.Save(topic, true);

      Assert.IsFalse(child.IsNew);

    }

    /*==========================================================================================================================
    | TEST: SAVE: UNRESOLVED REFERENCE: RESOLVES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves a new <see cref="Topic"/> with an unresolved <see cref="Topic.References"/> and confirms that it successfully
    ///   resolves it by marking the <see cref="Topic.References"/> collection as <see cref="TrackedRecordCollection{TItem,
    ///   TValue, TAttribute}.IsDirty()"/> as <c>false</c>.
    /// </summary>
    [TestMethod]
    public void Save_UnresolvedReference_Resolves() {

      var parent                = _topicRepository.Load("Root:Web:Web_3:Web_3_0");
      var topic                 = TopicFactory.Create("Test", "Page", parent);
      var reference             = TopicFactory.Create("Reference", "Page", topic);

      topic.References.SetValue("Test", reference);

      _topicRepository.Save(topic, true);

    }

    /*==========================================================================================================================
    | TEST: SAVE: UNRESOLVED REFERENCE: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves a new <see cref="Topic"/> with an unresolved <see cref="Topic.References"/> and confirms that it throws an
    ///   exception if that reference cannot be resolved.
    /// </summary>
    [TestMethod]
    [ExpectedException(
      typeof(ReferentialIntegrityException),
      "TopicRepository.Save() failed to throw an exception despite an unresolved topic reference."
    )]
    public void Save_UnresolvedReference_ThrowsException() {

      var parent                = _topicRepository.Load("Root:Web:Web_3:Web_3_0");
      var topic                 = TopicFactory.Create("Test", "Page", parent);
      var reference             = TopicFactory.Create("Reference", "Page", parent);

      topic.References.SetValue("Test", reference);

      _topicRepository.Save(topic, true);

    }

    /*==========================================================================================================================
    | TEST: DELETE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then deletes one of the
    ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepository.Delete(Topic, Boolean)"/>, and ensures that it
    ///   is immediately reflected in the <see cref="TopicRepository"/> cache of <see cref="ContentTypeDescriptor"/>s.
    /// </summary>
    [TestMethod]
    public void Delete_ContentTypeDescriptor_UpdatesContentTypeCache() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var contentType           = contentTypes.Contains("Page")? contentTypes["Page"] : null;

      Contract.Assume(contentType);

      _topicRepository.Delete(contentType);

      Assert.IsFalse(contentTypes.Contains(contentType));

    }

    /*==========================================================================================================================
    | TEST: MOVE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepository.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then moves one of the
    ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepository.Move(Topic, Topic, Topic?)"/>, and ensures
    ///   that it is immediately reflected in the <see cref="TopicRepository"/> cache of <see
    ///   cref="ContentTypeDescriptor"/>s.
    /// </summary>
    [TestMethod]
    public void Move_ContentTypeDescriptor_UpdatesContentTypeCache() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var pageContentType       = contentTypes.Contains("Page")? contentTypes["Page"] : null;
      var contactContentType    = contentTypes.Contains("Contact")? contentTypes["Contact"] : null;
      var contactAttributeCount = contactContentType?.AttributeDescriptors.Count;

      Contract.Assume(contactContentType);
      Contract.Assume(pageContentType);

      _topicRepository.Move(contactContentType, pageContentType);

      Assert.AreNotEqual<int?>(contactContentType?.AttributeDescriptors.Count, contactAttributeCount);

    }

    /*==========================================================================================================================
    | TEST: SAVE: ATTRIBUTE DESCRIPTOR: UPDATES CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="ContentTypeDescriptor"/> with a child <see cref="ContentTypeDescriptor"/> . Adds a new <see
    ///   cref="AttributeDescriptor"/> to the parent. Ensures that the <see cref="ContentTypeDescriptor.AttributeDescriptors"/>
    ///   of the child reflects the change.
    /// </summary>
    [TestMethod]
    public void Save_AttributeDescriptor_UpdatesContentType() {

      var contentType           = TopicFactory.Create("Parent", "ContentTypeDescriptor") as ContentTypeDescriptor;
      var attributeList         = TopicFactory.Create("Attributes", "List", contentType);
      var childContentType      = TopicFactory.Create("Child", "ContentTypeDescriptor", contentType) as ContentTypeDescriptor;

      Contract.Assume(childContentType);

      var attributeCount        = childContentType.AttributeDescriptors.Count;

      var newAttribute          = TopicFactory.Create("NewAttribute", "BooleanAttributeDescriptor", attributeList) as BooleanAttributeDescriptor;

      Contract.Assume(newAttribute);

      _topicRepository.Save(newAttribute);

      Assert.IsTrue(childContentType.AttributeDescriptors.Count > attributeCount);

    }

    /*==========================================================================================================================
    | TEST: DELETE: ATTRIBUTE DESCRIPTOR: UPDATES CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="ContentTypeDescriptor"/> with a child <see cref="ContentTypeDescriptor"/> . Adds a new <see
    ///   cref="AttributeDescriptor"/> to the parent, then immediately deletes it. Ensures that the <see
    ///   cref="ContentTypeDescriptor.AttributeDescriptors"/> of the child reflects the change.
    /// </summary>
    [TestMethod]
    public void Delete_AttributeDescriptor_UpdatesContentTypeCache() {

      var contentType           = TopicFactory.Create("Parent", "ContentTypeDescriptor") as ContentTypeDescriptor;
      var attributeList         = TopicFactory.Create("Attributes", "List", contentType);
      var newAttribute          = TopicFactory.Create("NewAttribute", "BooleanAttributeDescriptor", attributeList) as BooleanAttributeDescriptor;
      var childContentType      = TopicFactory.Create("Child", "ContentTypeDescriptor", contentType) as ContentTypeDescriptor;

      Contract.Assume(childContentType);
      Contract.Assume(newAttribute);

      var attributeCount        = childContentType.AttributeDescriptors.Count;

      _topicRepository.Delete(newAttribute);

      Assert.IsTrue(childContentType.AttributeDescriptors.Count < attributeCount);

    }

    /*==========================================================================================================================
    | TEST: DELETE: TOPIC DELETED EVENT: IS RAISED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> and then immediately deletes it. Ensures that the <see cref="ITopicRepository.
    ///   TopicDeleted"/> is raised.
    /// </summary>
    [TestMethod]
    public void Delete_TopicDeletedEvent_IsRaised() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var hasFired              = false;

      _cachedTopicRepository.Save(topic);
      _cachedTopicRepository.TopicDeleted += eventHandler;
      _cachedTopicRepository.Delete(topic);
      _cachedTopicRepository.TopicDeleted -= eventHandler;

      Assert.IsTrue(hasFired);

      void eventHandler(object? sender, TopicEventArgs eventArgs) => hasFired = true;

    }

    /*==========================================================================================================================
    | TEST: SAVE: TOPIC SAVED EVENT: IS FIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> and then immediately saves it. Ensures that the <see cref="ITopicRepository.TopicSaved"
    ///   /> is raised.
    /// </summary>
    [TestMethod]
    public void Save_TopicSavedEvent_IsRaised() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var hasFired              = false;

      _cachedTopicRepository.TopicSaved += eventHandler;
      _cachedTopicRepository.Save(topic);
      _cachedTopicRepository.TopicSaved -= eventHandler;

      Assert.IsTrue(hasFired);

      void eventHandler(object? sender, TopicSaveEventArgs eventArgs) => hasFired = true;

    }

    /*==========================================================================================================================
    | TEST: SAVE: TOPIC RENAMED EVENT: IS FIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> and then immediately saves it. Ensures that the <see cref="ITopicRepository.TopicRenamed
    ///   "/> is raised.
    /// </summary>
    [TestMethod]
    public void Save_TopicRenamedEvent_IsRaised() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var hasFired              = false;

      topic.Key                 = "New";

      _cachedTopicRepository.TopicRenamed += eventHandler;
      _cachedTopicRepository.Save(topic);
      _cachedTopicRepository.TopicRenamed -= eventHandler;

      Assert.IsTrue(hasFired);

      void eventHandler(object? sender, TopicRenameEventArgs eventArgs) => hasFired = true;

    }

    /*==========================================================================================================================
    | TEST: SAVE: TOPIC MOVED EVENT: IS FIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/>, changes its parent, and then saves it. Ensures that the <see cref="ITopicRepository.
    ///   TopicMoved"/> is raised.
    /// </summary>
    [TestMethod]
    public void Save_TopicMovedEvent_IsRaised() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var parent                = TopicFactory.Create("Products", "Page", 2);
      var hasFired              = false;

      topic.Parent              = parent;

      _cachedTopicRepository.TopicMoved += eventHandler;
      _cachedTopicRepository.Save(topic);
      _cachedTopicRepository.TopicMoved -= eventHandler;

      Assert.IsTrue(hasFired);

      void eventHandler(object? sender, TopicMoveEventArgs eventArgs) => hasFired = true;

    }

    /*==========================================================================================================================
    | TEST: MOVE: TOPIC MOVED EVENT: IS FIRED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="Topic"/> and then immediately moves it. Ensures that the <see cref="ITopicRepository.TopicMoved"
    ///   /> is raised.
    /// </summary>
    [TestMethod]
    public void Move_TopicMovedEvent_IsRaised() {

      var topic                 = TopicFactory.Create("Test", "Page", 1);
      var parent                = TopicFactory.Create("Products", "Page", 2);
      var hasFired              = false;

      _cachedTopicRepository.TopicMoved += eventHandler;
      _cachedTopicRepository.Move(topic, parent);
      _cachedTopicRepository.TopicMoved -= eventHandler;

      Assert.IsTrue(hasFired);

      void eventHandler(object? sender, TopicMoveEventArgs eventArgs) => hasFired = true;

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, String)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_String() {

      var command               = new SqlCommand();

      command.AddParameter("TopicKey", "Root");

      var sqlParameter          = command.Parameters["@TopicKey"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@TopicKey"));
      Assert.AreEqual<string?>("Root", (string?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.VarChar, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: INT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="Int32"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, Int32)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_Int() {

      var command               = new SqlCommand();

      command.AddParameter("TopicId", 5);

      var sqlParameter          = command.Parameters["@TopicId"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@TopicId"));
      Assert.AreEqual<int?>(5, (int?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Int, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: BOOL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="Boolean"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, Boolean)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_Bool() {

      var command               = new SqlCommand();

      command.AddParameter("IsHidden", true);

      var sqlParameter          = command.Parameters["@IsHidden"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@IsHidden"));
      Assert.AreEqual<bool?>(true, (bool?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Bit, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: DATE/TIME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="DateTime"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, DateTime)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_DateTime() {

      var command               = new SqlCommand();
      var lastModified          = DateTime.UtcNow;

      command.AddParameter("LastModified", lastModified);

      var sqlParameter          = command.Parameters["@LastModified"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@LastModified"));
      Assert.AreEqual<DateTime?>(lastModified, (DateTime?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.DateTime2, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: DATA TABLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="DataTable"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, DataTable)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_DataTable() {

      var command               = new SqlCommand();
      var dataTable             = new TopicListDataTable();

      command.AddParameter("Relationships", dataTable);

      var sqlParameter          = command.Parameters["@Relationships"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@Relationships"));
      Assert.AreEqual<DataTable?>(dataTable, (DataTable?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Structured, sqlParameter?.SqlDbType);

      command.Dispose();
      dataTable.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD PARAMETER: STRING BUILDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="StringBuilder"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, string, StringBuilder)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_StringBuilder() {

      var command               = new SqlCommand();
      var xml                   = new StringBuilder();

      command.AddParameter("AttributesXml", xml);

      var sqlParameter          = command.Parameters["@AttributesXml"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@AttributesXml"));
      Assert.AreEqual<string?>(xml.ToString(), (string?)sqlParameter?.Value);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Xml, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD OUTPUT PARAMETER: RETURN CODE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddOutputParameter(SqlCommand, String)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddOutputParameter_ReturnCode() {

      var command               = new SqlCommand();

      command.AddOutputParameter("TopicId");

      var sqlParameter          = command.Parameters["@TopicId"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@TopicId"));
      Assert.AreEqual<int>(-1, command.GetReturnCode("TopicId"));
      Assert.AreEqual<ParameterDirection?>(ParameterDirection.ReturnValue, sqlParameter?.Direction);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Int, sqlParameter?.SqlDbType);

      command.Dispose();

    }

  } //Class
} //Namespace