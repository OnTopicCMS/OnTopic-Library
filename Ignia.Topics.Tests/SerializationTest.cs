/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Globalization;
using Ignia.Topics.Collections;
using Ignia.Topics.Data.Caching;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Repositories;
using Ignia.Topics.Serialization;
using Ignia.Topics.Tests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ignia.Topics.Tests {

  /*============================================================================================================================
  | CLASS: SERIALIZATION TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for JSON.NET's serialization—such as via the <see cref="AttributeValueCollectionJsonConverter"/>.
  /// </summary>
  [TestClass]
  public class SerializationTest {


    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    ITopicRepository            _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="ITopicRepositoryTest"/> with shared resources.
    /// </summary>
    /// <remarks>
    ///   This uses the <see cref="FakeTopicRepository"/> to provide data, and then <see cref="CachedTopicRepository"/> to
    ///   manage the in-memory representation of the data. While this introduces some overhead to the tests, the latter is a
    ///   relatively lightweight façade to any <see cref="ITopicRepository"/>, and prevents the need to duplicate logic for
    ///   crawling the object graph.
    /// </remarks>
    public SerializationTest() {
      _topicRepository = new CachedTopicRepository(new FakeTopicRepository());
    }

    /*==========================================================================================================================
    | TEST: WRITE ATTRIBUTE VALUE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the serialization of a <see cref="AttributeValueCollection"/> to confirm that it properly utilizes the <see
    ///   cref="AttributeValueCollectionJsonConverter"/>.
    /// </summary>
    [TestMethod]
    public void WriteAttributeValueCollection() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var json                  = JsonConvert.SerializeObject(topic);
      var jObject               = JObject.Parse(json);

      Assert.AreEqual<string>("Page", jObject["Attributes"]["ContentType"].ToString());

    }

    /*==========================================================================================================================
    | TEST: READ ATTRIBUTE VALUE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the deserialization of a <see cref="AttributeValueCollection"/> to confirm that it properly utilizes the <see
    ///   cref="AttributeValueCollectionJsonConverter"/>.
    /// </summary>
    [TestMethod]
    public void ReadAttributeValueCollection() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var json                  = JsonConvert.SerializeObject(topic);
      var result                = JsonConvert.DeserializeObject<Topic>(json);

      Assert.AreEqual<string>("Page", result.Attributes.GetValue("ContentType"));

    }

    /*==========================================================================================================================
    | TEST: WRITE RELATED TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the serialization of a <see cref="RelatedTopicCollection"/> to confirm that it properly utilizes the <see
    ///   cref="RelatedTopicCollectionJsonConverter"/>.
    /// </summary>
    [TestMethod]
    public void WriteRelatedTopicCollection() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var related               = TopicFactory.Create("Related", "Page");

      topic.Relationships.SetTopic("Related", related);

      var converter             = new RelatedTopicCollectionJsonConverter();
      var json                  = JsonConvert.SerializeObject(topic, converter);
      var jObject               = JObject.Parse(json);

      Assert.AreEqual<string>("Related", jObject["Relationships"]["Related"].First["title"].ToString());

    }

    /*==========================================================================================================================
    | TEST: READ RELATED TOPIC COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Tests the deserialization of a <see cref="RelatedTopicCollection"/> to confirm that it properly utilizes the <see
    ///   cref="RelatedTopicCollectionJsonConverter"/>.
    /// </summary>
    [TestMethod]
    public void ReadRelatedTopicCollection() {

      var topic                 = TopicFactory.Create("Test", "Page");
      var related               = _topicRepository.Load("Root:Configuration:ContentTypes:Page");

      topic.Relationships.SetTopic("Related", related);

      var converter             = new RelatedTopicCollectionJsonConverter(_topicRepository);
      var json                  = JsonConvert.SerializeObject(topic, converter);
      var result                = JsonConvert.DeserializeObject<Topic>(json, converter);

      Assert.AreEqual<string>("Page", result.Relationships.GetTopics("Related")[0].Title);

    }


  } //Class

} //Namespace
