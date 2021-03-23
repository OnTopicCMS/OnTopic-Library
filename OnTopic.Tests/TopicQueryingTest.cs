/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OnTopic.Collections;
using OnTopic.Data.Caching;
using OnTopic.Metadata;
using OnTopic.Querying;
using OnTopic.Repositories;
using OnTopic.TestDoubles;
using Xunit;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: TOPIC QUERYING TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="TopicExtensions"/> class.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
  public class TopicQueryingTest {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="TopicQueryingTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="StubTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public TopicQueryingTest() {
      _topicRepository = new CachedTopicRepository(new StubTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: FIND ALL BY ATTRIBUTE: RETURNS CORRECT TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks for a deeply nested child topic using only the attribute value.
    /// </summary>
    [Fact]
    public void FindAllByAttribute_ReturnsCorrectTopics() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", childTopic, 20);
      var grandNieceTopic       = TopicFactory.Create("GrandNieceTopic", "Page", childTopic, 3);
      var greatGrandChildTopic  = TopicFactory.Create("GreatGrandChildTopic", "Page", grandChildTopic, 7);

      grandChildTopic.Attributes.SetValue("Foo", "Baz");
      grandNieceTopic.Attributes.SetValue("Foo", "Bar");
      greatGrandChildTopic.Attributes.SetValue("Foo", "Bar");

      Assert.AreEqual<Topic?>(greatGrandChildTopic, parentTopic.FindAllByAttribute("Foo", "Bar").First());
      Assert.AreEqual<int>(2, parentTopic.FindAllByAttribute("Foo", "Bar").Count);
      Assert.AreEqual<Topic?>(grandChildTopic, parentTopic.FindAllByAttribute("Foo", "Baz").First());

    }

    /*==========================================================================================================================
    | TEST: FIND FIRST PARENT: RETURNS CORRECT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a deeply nested <see cref="Topic"/>, returns the first parent <see cref="Topic"/> which satisfies a delegate.
    /// </summary>
    [Fact]
    public void FindFirstParent_ReturnsCorrectTopic() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", childTopic, 20);
      var greatGrandChildTopic  = TopicFactory.Create("GreatGrandChildTopic", "Page", grandChildTopic, 7);

      var foundTopic            = greatGrandChildTopic.FindFirstParent(t => t.Id is 5);

      Assert.AreEqual<Topic?>(childTopic, foundTopic);

    }

    /*==========================================================================================================================
    | TEST: FIND FIRST PARENT: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Correctly returns null if the delegate cannot be satisfied.
    /// </summary>
    [Fact]
    public void FindFirstParent_ReturnsNull() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);

      var foundTopic            = childTopic.FindFirstParent(t => t.Id is 10);

      Assert.IsNull(foundTopic);

    }

    /*==========================================================================================================================
    | TEST: GET ROOT TOPIC: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a deeply nested <see cref="Topic"/>, returns the root <see cref="Topic"/>.
    /// </summary>
    [Fact]
    public void GetRootTopic_ReturnsRootTopic() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", childTopic, 20);
      var greatGrandChildTopic  = TopicFactory.Create("GreatGrandChildTopic", "Page", grandChildTopic, 7);

      var rootTopic             = greatGrandChildTopic.GetRootTopic();

      Assert.AreEqual<Topic?>(parentTopic, rootTopic);

    }

    /*==========================================================================================================================
    | TEST: GET ROOT TOPIC: RETURNS CURRENT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root <see cref="Topic"/>, returns the current <see cref="Topic"/>.
    /// </summary>
    [Fact]
    public void GetRootTopic_ReturnsCurrentTopic() {

      var topic                 = TopicFactory.Create("ParentTopic", "Page", 1);

      var rootTopic             = topic.GetRootTopic();

      Assert.AreEqual<Topic?>(topic, rootTopic);

    }

    /*==========================================================================================================================
    | TEST: GET BY UNIQUE KEY: ROOT KEY: RETURNS ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a root <see cref="Topic"/>, returns the root <see cref="Topic"/>.
    /// </summary>
    [Fact]
    public void GetByUniqueKey_RootKey_ReturnsRootTopic() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      _                         = TopicFactory.Create("ChildTopic", "Page", parentTopic, 2);

      var foundTopic = parentTopic.GetByUniqueKey("ParentTopic");

      Assert.IsNotNull(foundTopic);
      Assert.AreEqual<Topic?>(parentTopic, foundTopic);

    }

    /*==========================================================================================================================
    | TEST: GET BY UNIQUE KEY: VALID KEY: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a deeply nested <see cref="Topic"/>, returns the expected <see cref="Topic"/>.
    /// </summary>
    [Fact]
    public void GetByUniqueKey_ValidKey_ReturnsTopic() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", childTopic, 20);
      var greatGrandChildTopic1 = TopicFactory.Create("GreatGrandChildTopic1", "Page", grandChildTopic, 7);
      var greatGrandChildTopic2 = TopicFactory.Create("GreatGrandChildTopic2", "Page", grandChildTopic, 7);

      var foundTopic = greatGrandChildTopic1.GetByUniqueKey("ParentTopic:ChildTopic:GrandChildTopic:GreatGrandChildTopic2");

      Assert.AreEqual<Topic?>(greatGrandChildTopic2, foundTopic);

    }

    /*==========================================================================================================================
    | TEST: GET BY UNIQUE KEY: INVALID KEY: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given an invalid <c>UniqueKey</c>, the <see cref="TopicExtensions.GetByUniqueKey(Topic, String)"/> returns
    ///   <c>null</c>.
    /// </summary>
    [Fact]
    public void GetByUniqueKey_InvalidKey_ReturnsNull() {

      var parentTopic           = TopicFactory.Create("ParentTopic", "Page", 1);
      var childTopic            = TopicFactory.Create("ChildTopic", "Page", parentTopic, 5);
      var grandChildTopic       = TopicFactory.Create("GrandChildTopic", "Page", childTopic, 20);
      var greatGrandChildTopic  = TopicFactory.Create("GreatGrandChildTopic", "Page", grandChildTopic, 7);

      var foundTopic = greatGrandChildTopic.GetByUniqueKey("ParentTopic:ChildTopic:GrandChildTopic:GreatGrandChildTopic2");

      Assert.IsNull(foundTopic);

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE: VALID CONTENT TYPE: RETURNS CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a deeply nested <see cref="Topic"/>, returns the expected <see cref="ContentTypeDescriptor"/>.
    /// </summary>
    [Fact]
    public void GetContentType_ValidContentType_ReturnsContentType() {

      var topic                 = _topicRepository.Load(11111);
      var contentTypeDescriptor = topic?.GetContentTypeDescriptor();

      Assert.IsNotNull(contentTypeDescriptor);
      Assert.AreEqual<string>("Page", contentTypeDescriptor.Key);

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE: INVALID CONTENT TYPE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given an invalid <see cref="ContentTypeDescriptor"/>, the <see cref="TopicExtensions.GetContentTypeDescriptor(Topic)"
    ///   /> returns <c>null</c>.
    /// </summary>
    [Fact]
    public void GetContentType_InvalidContentType_ReturnsNull() {

      var parentTopic           = _topicRepository.Load(11111);
      var topic                 = TopicFactory.Create("Test", "NonExistent", parentTopic);
      var contentTypeDescriptor = topic.GetContentTypeDescriptor();

      Assert.IsNull(contentTypeDescriptor);

    }

    /*==========================================================================================================================
    | TEST: GET CONTENT TYPE: INVALID TYPE: RETURNS NULL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given an invalid <see cref="ContentTypeDescriptor"/>, the <see cref="TopicExtensions.GetContentTypeDescriptor(Topic)"
    ///   /> returns <c>null</c>.
    /// </summary>
    /// <remarks>
    ///   This varies from <see cref="GetContentType_InvalidContentType_ReturnsNull()"/> in that it returns a valid <see cref="
    ///   Topic"/> which doesn't derive from <see cref="ContentTypeDescriptor"/>.
    /// </remarks>
    [Fact]
    public void GetContentType_InvalidType_ReturnsNull() {

      var parentTopic           = _topicRepository.Load(11111);
      var topic                 = TopicFactory.Create("Test", "Title", parentTopic);
      var contentTypeDescriptor = topic.GetContentTypeDescriptor();

      Assert.IsNull(contentTypeDescriptor);

    }

    /*==========================================================================================================================
    | TEST: ANY DIRTY: DIRTY COLLECTION: RETURN TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="TopicCollection"/> with at least one <see cref="Topic"/> that <see cref="Topic.IsDirty(String)"/>,
    ///   returns <c>true</c>.
    /// </summary>
    [Fact]
    public void AnyDirty_DirtyCollection_ReturnTrue() {

      var topics = new TopicCollection {
        new Topic("Test", "Page")
      };

      Assert.IsTrue(topics.AnyDirty());

    }

    /*==========================================================================================================================
    | TEST: ANY DIRTY: CLEAN COLLECTION: RETURN FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="TopicCollection"/> with no <see cref="Topic"/>s that are <see cref="Topic.IsDirty(String)"/>,
    ///   returns <c>false</c>.
    /// </summary>
    [Fact]
    public void AnyDirty_CleanCollection_ReturnFalse() {

      var topics = new TopicCollection {
        new Topic("Test", "Page", null, 1)
      };

      Assert.IsFalse(topics.AnyDirty());

    }

    /*==========================================================================================================================
    | TEST: ANY NEW: CONTAINS NEW: RETURN TRUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="TopicCollection"/> with at least one <see cref="Topic"/> that <see cref="Topic.IsNew"/>, returns
    ///   <c>true</c>.
    /// </summary>
    [Fact]
    public void AnyNew_ContainsNew_ReturnTrue() {

      var topics = new TopicCollection {
        new Topic("Test", "Page")
      };

      Assert.IsTrue(topics.AnyNew());

    }

    /*==========================================================================================================================
    | TEST: ANY NEW: CONTAINS EXISTING: RETURN FALSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="TopicCollection"/> with no <see cref="Topic"/>s that are <see cref="Topic.IsNew"/>, returns <c>
    ///   false</c>.
    /// </summary>
    [Fact]
    public void AnyNew_ContainsExisting_ReturnFalse() {

      var topics = new TopicCollection {
        new Topic("Test", "Page", null, 1)
      };

      Assert.IsFalse(topics.AnyNew());

    }

  } //Class
} //Namespace