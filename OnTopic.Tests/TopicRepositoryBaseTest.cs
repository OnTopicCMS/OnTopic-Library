/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Linq;
using OnTopic.Data.Caching;
using OnTopic.Metadata;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Metadata.AttributeTypes;
using OnTopic.Attributes;

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

      _topicRepository.GetContentTypeDescriptorsProxy((ContentTypeDescriptor)newContentType);

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

  } //Class
} //Namespace