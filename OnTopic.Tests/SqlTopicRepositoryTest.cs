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
using OnTopic.Associations;
using OnTopic.Tests.Schemas;
using System.Diagnostics.CodeAnalysis;

namespace OnTopic.Tests {

  /*============================================================================================================================
  | CLASS: SQL TOPIC REPOSITORY TEST
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides unit tests for the <see cref="SqlTopicRepository"/> class.
  /// </summary>
  [TestClass]
  [ExcludeFromCodeCoverage]
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
      Assert.AreEqual<string?>("Value", topic.Attributes.GetValue("Test"));

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH RELATIONSHIP: RETURNS RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   RelationshipsDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithRelationship_ReturnsRelationship() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var relationships   = new RelationshipsDataTable();

      topics.AddRow(1, "Root", "Container", null);
      topics.AddRow(2, "Web", "Container", 1);
      relationships.AddRow(1, "Test", 2, false);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, relationships });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int?>(2, topic.Relationships.GetValues("Test").FirstOrDefault()?.Id);

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH REFERENCE: RETURNS REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicReferencesDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithReference_ReturnsReference() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var references      = new TopicReferencesDataTable();

      topics.AddRow(1, "Root", "Container", null);
      topics.AddRow(2, "Web", "Container", 1);
      references.AddRow(1, "Test", 2);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, empty, references });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int?>(2, topic.References.GetValue("Test")?.Id);
      Assert.IsTrue(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH EXTERNAL REFERENCE: RETURNS REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicReferencesDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithExternalReference_ReturnsReference() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var references      = new TopicReferencesDataTable();

      var referenceTopic        = TopicFactory.Create("Web", "Container", 2);

      topics.AddRow(1, "Root", "Container", null);
      references.AddRow(1, "Test", 2);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, empty, references });

      var topic                 = tableReader.LoadTopicGraph(referenceTopic, false);

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int?>(2, topic.References.GetValue("Test")?.Id);
      Assert.IsTrue(topic.References.IsFullyLoaded);
      Assert.IsFalse(topic.References.IsDirty());

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH MISSING REFERENCE: NOT FULLY LOADED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicReferencesDataTable"/> record that is missing and confirms that <see cref="TopicRelationshipMultiMap.
    ///   IsFullyLoaded"/> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithMissingReference_NotFullyLoaded() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var references      = new TopicReferencesDataTable();

      topics.AddRow(1, "Root", "Container", null);
      references.AddRow(1, "Test", 2);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, empty, references });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int>(0, topic.References.Count);
      Assert.IsFalse(topic.References.IsFullyLoaded);

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH DELETED RELATIONSHIP: REMOVES RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a deleted <see
    ///   cref="RelationshipsDataTable"/> record and confirms that it is deleted from the <c>referenceTopic</c> graph.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithDeletedRelationship_RemovesRelationship() {

      var topic                 = TopicFactory.Create("Test", "Container", 1);
      var child                 = TopicFactory.Create("Child", "Container", topic, 2);
      var related               = TopicFactory.Create("Related", "Container", topic, 3);

      child.Relationships.SetValue("Test", related);

      using var empty           = new AttributesDataTable();
      using var relationships   = new RelationshipsDataTable();

      relationships.AddRow(2, "Test", 3, true);

      using var tableReader     = new DataTableReader(new DataTable[] { empty, empty, empty, relationships });

      tableReader.LoadTopicGraph(related);

      Assert.AreEqual<int>(0, topic.Relationships.GetValues("Test").Count);

    }

    /*==========================================================================================================================
    | TEST: TOPIC LIST DATA TABLE: ADD ROW: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="Data.Sql.Models.TopicListDataTable"/> and calls <see cref="Data.Sql.Models.TopicListDataTable.
    ///   AddRow(Int32)"/>. Confirms that a <see cref="DataRow"/> with the expected data is returned.
    /// </summary>
    [TestMethod]
    public void TopicListDataTable_AddRow_Succeeds() {

      var dataTable             = new Data.Sql.Models.TopicListDataTable();

      dataTable.AddRow(1);
      dataTable.AddRow(2);
      dataTable.AddRow(2);

      Assert.AreEqual<int>(3, dataTable.Rows.Count);
      Assert.AreEqual<int>(1, dataTable.Columns.Count);

      dataTable.Dispose();

    }

    /*==========================================================================================================================
    | TEST: ATTRIBUTE VALUES DATA TABLE: ADD ROW: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="Data.Sql.Models.AttributeValuesDataTable"/> and calls <see cref="Data.Sql.Models.
    ///   AttributeValuesDataTable.AddRow(String, String?)"/>. Confirms that a <see cref="DataRow"/> with the expected data is
    ///   returned.
    /// </summary>
    [TestMethod]
    public void AttributeValuesDataTable_AddRow_Succeeds() {

      var dataTable             = new Data.Sql.Models.AttributeValuesDataTable();

      dataTable.AddRow("Key", "Test");
      dataTable.AddRow("ContentType", "Page");
      dataTable.AddRow("ParentId", "4");

      Assert.AreEqual<int>(3, dataTable.Rows.Count);
      Assert.AreEqual<int>(2, dataTable.Columns.Count);

      dataTable.Dispose();

    }

    /*==========================================================================================================================
    | TEST: TOPIC REFERENCES DATA TABLE: ADD ROW: SUCCEEDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a <see cref="Data.Sql.Models.AttributeValuesDataTable"/> and calls <see cref="Data.Sql.Models.
    ///   TopicReferencesDataTable.AddRow(String, Int32)"/>. Confirms that a <see cref="DataRow"/> with the expected data is
    ///   returned.
    /// </summary>
    [TestMethod]
    public void TopicReferencesDataTable_AddRow_Succeeds() {

      var dataTable             = new Data.Sql.Models.TopicReferencesDataTable();

      dataTable.AddRow("BaseTopic", 1);
      dataTable.AddRow("Parent", 2);
      dataTable.AddRow("RootTopic", 3);

      Assert.AreEqual<int>(3, dataTable.Rows.Count);
      Assert.AreEqual<int>(2, dataTable.Columns.Count);

      dataTable.Dispose();

    }

  } //Class
} //Namespace