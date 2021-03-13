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
using OnTopic.Associations;
using OnTopic.Data.Sql;
using OnTopic.Tests.Schemas;

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
    | TEST: LOAD TOPIC GRAPH: WITH NEW PARENT: UPDATES PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicsDataTable"/> record that represents a different parent than the existing <c>referenceTopic</c> and confirms that
    ///   the topic's parent is updated.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithNewParent_UpdatesParent() {

      using var topics          = new TopicsDataTable();

      var topic                 = new Topic("Root", "Container", null, 1);
      var parent1               = new Topic("Parent1", "Container", topic, 2);
      var parent2               = new Topic("Parent2", "Container", topic, 3);
      var child                 = new Topic("Child", "Page", parent1, 4);

      topics.AddRow(4, "Child", "Page", parent2.Id);

      using var tableReader     = new DataTableReader(topics);

      tableReader.LoadTopicGraph(topic);

      Assert.AreEqual<Topic?>(parent2, child.Parent);

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
    | TEST: LOAD TOPIC GRAPH: WITH NULL ATTRIBUTES: REMOVES ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with an <see cref="
    ///   AttributesDataTable"/> record representing a deleted attribute and confirms that an existing reference topic with that
    ///   attribute has the value removed.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithNullAttributes_RemovesAttribute() {

      using var topics          = new TopicsDataTable();
      using var attributes      = new AttributesDataTable();

      var topic                 = TopicFactory.Create("Root", "Container", 1);

      topic.Attributes.SetValue("Test", "Initial Value");

      topics.AddRow(1, "Root", "Container");
      attributes.AddRow(1, "Test", null);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, attributes });

      tableReader.LoadTopicGraph(topic);

      Assert.IsNull(topic.Attributes.GetValue("Test"));

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
      Assert.IsTrue(topic.Relationships.IsFullyLoaded);

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH MISSING RELATIONSHIP: NOT FULLY LOADED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   RelationshipsDataTable"/> record that is missing and confirms that <see cref="TopicRelationshipMultiMap.IsFullyLoaded"
    ///   /> returns <c>false</c>.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithMissingRelationship_NotFullyLoaded() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var relationships   = new RelationshipsDataTable();

      topics.AddRow(1, "Root", "Container", null);
      relationships.AddRow(1, "Test", 2, false);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, relationships });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int>(0, topic.Relationships.Count);
      Assert.IsFalse(topic.Relationships.IsFullyLoaded);

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
    | TEST: LOAD TOPIC GRAPH: WITH DELETED REFERENCE: REMOVES EXISTING REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicReferencesDataTable"/> record and confirms that existing references on a reference topic are deleted if they are
    ///   <c>null</c> in the <see cref="TopicReferencesDataTable"/>.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithDeletedReference_RemovesExistingReference() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var references      = new TopicReferencesDataTable();

      var referenceTopic        = TopicFactory.Create("Web", "Container", 1);

      referenceTopic.References.SetValue("Reference", referenceTopic);

      topics.AddRow(1, "Web", "Container", null);
      references.AddRow(1, "Reference", null);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, empty, references });

      tableReader.LoadTopicGraph(referenceTopic, false);

      Assert.IsNull(referenceTopic.References.GetValue("Reference"));
      Assert.IsTrue(referenceTopic.References.IsFullyLoaded);

    }

    /*==========================================================================================================================
    | TEST: LOAD TOPIC GRAPH: WITH MISSING REFERENCE: NOT FULLY LOADED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with a <see cref="
    ///   TopicReferencesDataTable"/> record that is missing and confirms that <see cref="TopicReferenceCollection.IsFullyLoaded
    ///   "/> returns <c>false</c>.
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
    | TEST: LOAD TOPIC GRAPH: WITH VERSION HISTORY: RETURNS VERSIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Calls <see cref="SqlDataReaderExtensions.LoadTopicGraph(IDataReader, Topic?, Boolean?, Boolean)"/> with an <see cref="
    ///   VersionHistoryDataTable"/> record and confirms that a topic with those values is returned.
    /// </summary>
    [TestMethod]
    public void LoadTopicGraph_WithVersionHistory_ReturnsVersions() {

      using var topics          = new TopicsDataTable();
      using var empty           = new AttributesDataTable();
      using var versions        = new VersionHistoryDataTable();

      topics.AddRow(1, "Root", "Container", null);
      versions.AddRow(1, DateTime.MinValue);

      using var tableReader     = new DataTableReader(new DataTable[] { topics, empty, empty, empty, empty, versions });

      var topic                 = tableReader.LoadTopicGraph();

      Assert.IsNotNull(topic);
      Assert.AreEqual<int>(1, topic.Id);
      Assert.AreEqual<int>(1, topic.VersionHistory.Count);
      Assert.IsTrue(topic.VersionHistory.Contains(DateTime.MinValue));

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
    | TEST: SQL COMMAND: ADD PARAMETER: NULL STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <c>null</c> parameter value to it using the <see cref="
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, String)"/> extension method.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddParameter_NullString() {

      var command               = new SqlCommand();

      command.AddParameter("TopicKey", (string?)null);

      var sqlParameter          = command.Parameters["@TopicKey"];

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@TopicKey"));
      Assert.AreEqual<string?>(null, (string?)sqlParameter?.Value);
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
      var dataTable             = new Data.Sql.Models.TopicListDataTable();

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
    ///   SqlCommandExtensions.AddParameter(SqlCommand, String, StringBuilder)"/> extension method.
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

      sqlParameter.Value        = 5;

      Assert.AreEqual<int>(1, command.Parameters.Count);
      Assert.IsTrue(command.Parameters.Contains("@TopicId"));
      Assert.AreEqual<int>(5, command.GetReturnCode("TopicId"));
      Assert.AreEqual<ParameterDirection?>(ParameterDirection.ReturnValue, sqlParameter?.Direction);
      Assert.AreEqual<SqlDbType?>(SqlDbType.Int, sqlParameter?.SqlDbType);

      command.Dispose();

    }

    /*==========================================================================================================================
    | TEST: SQL COMMAND: ADD OUTPUT PARAMETER: RETURN DEFAULT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Creates a <see cref="SqlCommand"/> object and adds a <see cref="String"/> parameter to it using the <see cref="
    ///   SqlCommandExtensions.AddOutputParameter(SqlCommand, String)"/> extension method. Ensures the default return code is
    ///   returned, if the value isn't explicitly set.
    /// </summary>
    [TestMethod]
    public void SqlCommand_AddOutputParameter_ReturnsDefault() {

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