/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Attributes;
using OnTopic.Data.Caching;
using OnTopic.Metadata;
using OnTopic.Metadata.AttributeTypes;
using OnTopic.Repositories;
using OnTopic.TestDoubles;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC REPOSITORY BASE TESTS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicRepositoryBase"/> class.
  /// </summary>
  /// <remarks>
  ///   These tests evaluate features that are specific to the <see cref="TopicRepositoryBase"/> class.
  /// </remarks>
  [TestClass]
  public class TopicRepositoryBaseTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    StubTopicRepository             _topicRepository;

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
      _topicRepository = new StubTopicRepository();
    }

    /*==========================================================================================================================
    | TEST: DELETE: DERIVED TOPIC: THROWS EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a topic which other topics, outside of the graph, derive from. Expects exception.
    /// </summary>
    [TestMethod]
    [ExpectedException(typeof(ReferentialIntegrityException))]
    public void Delete_DerivedTopic_ThrowsException() {

      var root                  = TopicFactory.Create("Root", "Page");
      var topic                 = TopicFactory.Create("Topic", "Page", root);
      var child                 = TopicFactory.Create("Child", "Page", topic);
      var derived               = TopicFactory.Create("Derived", "Page", root);

      derived.DerivedTopic      = child;

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

      derived.DerivedTopic      = child;

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
      var child                 = TopicFactory.Create("Child", "Page", topic);

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
      var child                 = TopicFactory.Create("Child", "Page", topic);

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
      var child                 = TopicFactory.Create("Child", "List", topic);

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

      child.Relationships.SetTopic("Related", related);

      _topicRepository.Delete(topic, true);

      Assert.AreEqual<int>(0, related.IncomingRelationships.GetTopics("Related").Count);

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

      Assert.AreEqual<int>(3, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EMPTY ATTRIBUTES: SKIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, without any filtering by whether or not the attribute is an <see
    ///   cref="AttributeDescriptor.IsExtendedAttribute"/>. Any <see cref="AttributeValue"/>s with a null or empty value should
    ///   be skipped.
    /// </summary>
    [TestMethod]
    public void GetAttributes_EmptyAttributes_Skips() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("EmptyAttribute", "");
      topic.Attributes.SetValue("NullAttribute", null);

      var attributes            = _topicRepository.GetAttributesProxy(topic, null);

      Assert.IsFalse(attributes.Any(a => a.Key == "EmptyAttribute"));
      Assert.IsFalse(attributes.Any(a => a.Key == "NullAttribute"));

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

      Assert.AreEqual<int>(2, attributes.Count());

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
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeValue.IsDirty"/>. Expects an <see
    ///   cref="AttributeValue"/> to be returned even if it's <i>not</i> <see cref="AttributeValue.IsDirty"/> <i>but</i> its
    ///   <see cref="AttributeValue.IsExtendedAttribute"/> disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExtendedAttributeMismatch_ReturnsExtendedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title", isDirty:false, isExtendedAttribute:false);

      var attributes            = _topicRepository.GetAttributesProxy(topic, true, true);

      //Expect Title, even though it isn't IsDirty
      Assert.AreEqual<int>(1, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXTENDED ATTRIBUTE MISMATCH: RETURNS NOTHING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <see cref="AttributeValue.IsDirty"/>. Expects the <see
    ///   cref="AttributeValue"/> to <i>not</i> be returned even though its <see cref="AttributeValue.IsExtendedAttribute"/>
    ///   disagrees with <see cref="AttributeDescriptor.IsExtendedAttribute"/>, since it won't match the <see
    ///   cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>'s <c>isExtendedAttribute</c> call.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExtendedAttributeMismatch_ReturnsNothing() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("Title", "Title", isDirty: false, isExtendedAttribute: false);

      var attributes            = _topicRepository.GetAttributesProxy(topic, false, true);

      //Expect Key and ContentType, but not Title
      Assert.AreEqual<int>(2, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: EXCLUDE LAST MODIFIED: RETURNS OTHER ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a list of attributes from a topic, filtering by <c>excludeLastModified</c>. Confirms that <see
    ///   cref="AttributeValue"/>s are not returned which start with <c>LastModified</c>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ExcludeLastModified_ReturnsOtherAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetDateTime("LastModified", DateTime.Now);
      topic.Attributes.SetValue("LastModifiedBy", "Unit Tests");

      var attributes            = _topicRepository.GetAttributesProxy(topic, null, excludeLastModified: true);

      //Expected to return Key and ContentType, butnot LastModified or LastModifiedBy
      Assert.AreEqual<int>(2, attributes.Count());

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH SHORT VALUE: RETURNS AS INDEXED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value shorter than 255 characters, then
    ///   ensures that it is returned as an an <i>indexed</i> <see cref="AttributeValue"/> when calling <see
    ///   cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ArbitraryAttributeWithShortValue_ReturnsAsIndexedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("ArbitraryAttribute", "Value");

      var attributes            = _topicRepository.GetAttributesProxy(topic, false);

      Assert.IsTrue(attributes.Any(a => a.Key.Equals("ArbitraryAttribute", StringComparison.InvariantCultureIgnoreCase)));

    }

    /*==========================================================================================================================
    | TEST: GET ATTRIBUTES: ARBITRARY ATTRIBUTE WITH LONG VALUE: RETURNS AS EXTENDED ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an arbitrary (unmatched) attribute on a <see cref="Topic"/> with a value longer than 255 characters, then
    ///   ensures that it is returned as an an <see cref="AttributeDescriptor.IsExtendedAttribute"/> when calling <see
    ///   cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>.
    /// </summary>
    [TestMethod]
    public void GetAttributes_ArbitraryAttributeWithLongValue_ReturnsAsExtendedAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypes");

      topic.Attributes.SetValue("ArbitraryAttribute", new string('x', 256));

      var attributes            = _topicRepository.GetAttributesProxy(topic, true);

      Assert.IsTrue(attributes.Any(a => a.Key.Equals("ArbitraryAttribute", StringComparison.InvariantCultureIgnoreCase)));

    }

    /*==========================================================================================================================
    | TEST: GET UNMATCHED ATTRIBUTES: RETURNS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Using <see cref="TopicRepositoryBase.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
    ///   <see cref="ContentTypeDescriptor"/> but not the <see cref="Topic"/> are returned.
    /// </summary>
    [TestMethod]
    public void GetUnmatchedAttributes_ReturnsAttributes() {

      var topic                 = TopicFactory.Create("Test", "ContentTypeDescriptor", 1);

      topic.Attributes.SetValue("Title", "Title");

      var attributes            = _topicRepository.GetUnmatchedAttributesProxy(topic);

      Assert.IsTrue(attributes.Any());
      Assert.IsFalse(attributes.Any(a => a.Key.Equals("Title", StringComparison.InvariantCultureIgnoreCase)));

    }

    /*==========================================================================================================================
    | TEST: GET UNMATCHED ATTRIBUTES: EMPTY ARBITRARY ATTRIBUTES: RETURNS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Using <see cref="TopicRepositoryBase.GetUnmatchedAttributes(Topic)"/>, ensures that any attributes that exist on the
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

      Assert.IsTrue(attributes.Any(a => a.Key.Equals("ArbitraryAttribute", StringComparison.InvariantCulture)));
      Assert.IsTrue(attributes.Any(a => a.Key.Equals("YetAnotherArbitraryAttribute", StringComparison.InvariantCulture)));
      Assert.IsFalse(attributes.Any(a => a.Key.Equals("AnotherArbitraryAttribute", StringComparison.InvariantCulture)));

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
      Assert.IsNotNull(contentTypes.GetTopic("ContentTypeDescriptor"));
      Assert.IsNotNull(contentTypes.GetTopic("Page"));
      Assert.IsNotNull(contentTypes.GetTopic("LookupListItem"));

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
    ///   Loads the <see cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then saves a new <see
    ///   cref="ContentTypeDescriptor"/> via <see cref="TopicRepositoryBase.Save(Topic, Boolean, Boolean)"/>, and ensures that
    ///   it is immediately reflected in the <see cref="TopicRepositoryBase"/> cache of <see cref="ContentTypeDescriptor"/>s.
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
    ///   Loads the <see cref="TopicRepositoryBase.GetContentTypeDescriptors()"/>, then saves an existing <see cref=
    ///   "ContentTypeDescriptor"/> via <see cref="TopicRepositoryBase.Save(Topic, Boolean, Boolean)"/>, and ensures that
    ///   it the <see cref="ContentTypeDescriptor.PermittedContentTypes"/> cache is updated.
    /// </summary>
    [TestMethod]
    public void Save_ContentTypeDescriptor_UpdatesPermittedContentTypes() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var pageContentType       = contentTypes.GetTopic("Page");
      var lookupContentType     = contentTypes.GetTopic("Lookup");
      var initialCount          = pageContentType.PermittedContentTypes.Count;

      pageContentType.Relationships.SetTopic("ContentTypes", lookupContentType);

      _topicRepository.Save(pageContentType);

      Assert.AreNotEqual<int>(initialCount, pageContentType.PermittedContentTypes.Count);

    }

    /*==========================================================================================================================
    | TEST: DELETE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then deletes one of the
    ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepositoryBase.Delete(Topic, Boolean)"/>, and ensures that it
    ///   is immediately reflected in the <see cref="TopicRepositoryBase"/> cache of <see cref="ContentTypeDescriptor"/>s.
    /// </summary>
    [TestMethod]
    public void Delete_ContentTypeDescriptor_UpdatesContentTypeCache() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var contentType           = contentTypes.Contains("Page")? contentTypes["Page"] : null;

      _topicRepository.Delete(contentType);

      Assert.IsFalse(contentTypes.Contains(contentType));

    }

    /*==========================================================================================================================
    | TEST: MOVE: CONTENT TYPE DESCRIPTOR: UPDATES CONTENT TYPE CACHE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Loads the <see cref="TopicRepositoryBase.GetAttributes(Topic, Boolean?, Boolean?, Boolean)"/>, then moves one of the
    ///   <see cref="ContentTypeDescriptor"/>s via <see cref="TopicRepositoryBase.Move(Topic, Topic, Topic?)"/>, and ensures
    ///   that it is immediately reflected in the <see cref="TopicRepositoryBase"/> cache of <see
    ///   cref="ContentTypeDescriptor"/>s.
    /// </summary>
    [TestMethod]
    public void Move_ContentTypeDescriptor_UpdatesContentTypeCache() {

      var contentTypes          = _topicRepository.GetContentTypeDescriptors();
      var pageContentType       = contentTypes.Contains("Page")? contentTypes["Page"] : null;
      var contactContentType    = contentTypes.Contains("Contact")? contentTypes["Contact"] : null;
      var contactAttributeCount = contactContentType?.AttributeDescriptors.Count;

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
      var attributeCount        = childContentType.AttributeDescriptors.Count;

      var newAttribute          = TopicFactory.Create("NewAttribute", "BooleanAttribute", attributeList) as BooleanAttribute;

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
      var newAttribute          = TopicFactory.Create("NewAttribute", "BooleanAttribute", attributeList) as BooleanAttribute;
      var childContentType      = TopicFactory.Create("Child", "ContentTypeDescriptor", contentType) as ContentTypeDescriptor;
      var attributeCount        = childContentType.AttributeDescriptors.Count;

      _topicRepository.Delete(newAttribute);

      Assert.IsTrue(childContentType.AttributeDescriptors.Count < attributeCount);

    }

  } //Class
} //Namespace