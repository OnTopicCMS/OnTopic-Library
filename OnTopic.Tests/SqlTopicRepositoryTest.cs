/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnTopic.Data.Sql;
using OnTopic.References;
using OnTopic.Tests.Schemas;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: SQL TOPIC REPOSITORY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="SqlTopicRepository"/> class.
  /// </summary>
  [TestClass]
  public class SqlTopicRepositoryTest {

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH TOPIC: RETURNS TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicsDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithTopic_ReturnsTopic() {

      using var topics          = new TopicsDataTable();

      topics.AddRow(1, "Root", "Container", null);

      using var tableReader     = new DataTableReader(topics);

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH ATTRIBUTES: RETURNS ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with an <see cref="
    ///   AttributesDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithAttributes_ReturnsAttributes() {

      using var topics          = new TopicsDataTable();
      using var attributes      = new AttributesDataTable();

      topics.AddRow(1, "Root", "Container", null);
      attributes.AddRow(1, "Test", "Value");

      using var tableReader     = new DataTableReader(new DataTable[] { topics, attributes });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<string>("Value", topic.Attributes.GetValue("Test"));

    }

  } //Class
} //Namespace