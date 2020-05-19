/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Data;
using System.Data.SqlTypes;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Repositories;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in Microsoft SQL Server.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="OnTopic.Repositories.IDataRepository"/> class.
  /// </remarks>
  public class SqlTopicRepository : TopicRepositoryBase, ITopicRepository {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            string                          _connectionString;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Instantiates a new instance of the SqlTopicRepository with a dependency on a connection string to provide necessary
    ///   access to a SQL database.
    /// </summary>
    /// <param name="connectionString">A connection string to a SQL server that contains the Topics database.</param>
    /// <returns>A new instance of the SqlTopicRepository.</returns>
    public SqlTopicRepository(string connectionString) : base() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(connectionString),
        "The name of the connection string must be provided in order to be validated."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Set private fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      _connectionString         = connectionString;

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic Load(string? topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty topic
      >-------------------------------------------------------------------------------------------------------------------------
      | If the topicKey is null, or does not contain a topic key, then assume the caller wants to return all data; in that case
      | call Load() with the special integer value of -1, which will load all topics from the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(topicKey)) {
        return Load(-1, isRecursive);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopicID", connection);
      int topicId;

      command.CommandType       = CommandType.StoredProcedure;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicKey", topicKey);
      command.AddOutputParameter();

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {

        connection.Open();
        command.ExecuteNonQuery();

        topicId                 = command.GetReturnCode();

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException($"Topic(s) failed to load: '{exception.Message}'", exception);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        command?.Dispose();
        connection.Close();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Load(topicId, isRecursive);

    }

    /// <inheritdoc />
    public override Topic Load(int topicId, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic                 = (Topic?)null;
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopics", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };
      var reader                = (SqlDataReader?)null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicID", topicId);
      command.AddParameter("DeepLoad", isRecursive);

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {
        connection.Open();
        reader                  = command.ExecuteReader();
        topic                   = reader.LoadTopicGraph();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        reader?.Dispose();
        command?.Dispose();
        connection.Close();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) {
        if (topicId == -1) {
          topic = TopicFactory.Create("Root", "Container");
        }
        else {
          throw new NullReferenceException($"Load() was unable to successfully load the topic with the TopicID '{topicId}'");
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <inheritdoc />
    public override Topic Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date >= new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic                 = (Topic?)null;
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopicVersion", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };
      var reader                = (SqlDataReader?)null;

      command.CommandType       = CommandType.StoredProcedure;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicID", topicId);
      command.AddParameter("Version", version);

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {
        connection.Open();
        reader                  = command.ExecuteReader();
        topic                   = reader.LoadTopicGraph(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        reader?.Dispose();
        command?.Dispose();
        connection.Close();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic?? throw new NullReferenceException("The specified Topic version could not be loaded");

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override int Save([NotNull]Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish attribute containers with schema
      \-----------------------------------------------------------------------------------------------------------------------*/
      var extendedAttributes    = new StringBuilder();
      var attributes            = new DataTable();

      attributes.Columns.Add(
        new DataColumn("AttributeKey") {
          MaxLength             = 128
        }
      );
      attributes.Columns.Add(
        new DataColumn("AttributeValue") {
          MaxLength             = 255
        }
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Add indexed attributes that are dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      //Exclude indexed attributes if the only dirty attributes are `LastModified` and/or `LastModifiedBy`, as these are
      //automatically generated and don't represent genuine changes to the topic.
      if (topic.Attributes.IsDirty(true)) {

        var indexedAttributes   = GetAttributes(topic, false, true);

        foreach (var attributeValue in indexedAttributes) {

          var record              = attributes.NewRow();
          record["AttributeKey"]  = attributeValue.Key;
          record["AttributeValue"]= attributeValue.Value;
          attributeValue.IsDirty  = false;

          attributes.Rows.Add(record);

        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add extended attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      extendedAttributes.Append("<attributes>");

      foreach (var attributeValue in GetAttributes(topic, true, null)) {

        extendedAttributes.Append(
          "<attribute key=\"" + attributeValue.Key + "\"><![CDATA[" + attributeValue.Value + "]]></attribute>"
        );
        attributeValue.IsDirty  = false;

        //###NOTE JJC20200502: By treating extended attributes as unmatched, we ensure that any indexed attributes with the same
        //value are overwritten with an empty attribute. This is useful for cases where an indexed attribute is moved to an
        //extended attribute, as it persists that version history, while removing ambiguity over which record is authoritative.
        //This is also useful for supporting arbitrary attribute values, since they may be moved from indexed to extended
        //attributes if their length exceeds 255.
        addUnmatchedAttribute(attributeValue.Key);

      }

      extendedAttributes.Append("</attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Add unmatched attributes
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it
      foreach (var attribute in GetUnmatchedAttributes(topic)) {
        addUnmatchedAttribute(attribute.Key);
      }

      void addUnmatchedAttribute(string key) {
        var record              = attributes.NewRow();
        record["AttributeKey"]  = key;
        record["AttributeValue"]= null;
        attributes.Rows.Add(record);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var isNew                 = topic.Id == -1;
      var connection            = new SqlConnection(_connectionString);
      var procedureName         = isNew? "CreateTopic" : "UpdateTopic";
      var command               = new SqlCommand(procedureName, connection) {
        CommandType             = CommandType.StoredProcedure
      };
      var version               = new SqlDateTime(DateTime.Now);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isNew) {
        command.AddParameter("TopicID", topic.Id);
        command.AddParameter("DeleteRelationships", true);
      }
      else if (topic.Parent != null) {
        command.AddParameter("ParentID", topic.Parent.Id);
      }
      command.AddParameter("Version", version.Value);
      command.AddParameter("ExtendedAttributes", extendedAttributes);
      command.Parameters.AddWithValue("@Attributes", attributes);
      command.AddOutputParameter();

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {

        connection.Open();
        command.ExecuteNonQuery();

        topic.Id                = command.GetReturnCode();

        Contract.Assume<InvalidOperationException>(
          topic.Id > 0,
          "The call to the CreateTopic stored procedure did not return the expected 'Id' parameter."
        );

        PersistRelations(topic, connection, true);

        topic.VersionHistory.Insert(0, version.Value);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException(
          $"Failed to save Topic '{topic.Key}' ({topic.Id}) via '{_connectionString}': '{exception.Message}'",
          exception
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        command?.Dispose();
        connection?.Dispose();
        attributes.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          childTopic.Attributes.SetValue("ParentID", topic.Id.ToString(CultureInfo.InvariantCulture));
          Save(childTopic, isRecursive, isDraft);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic.Id;

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
      "Microsoft.Contracts",
      "TestAlwaysEvaluatingToAConstant",
      Justification = "Sibling may be null from overloaded caller."
      )]
    public override void Move(Topic topic, Topic target, Topic? sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("MoveTopic", connection) {
        CommandType             = CommandType.StoredProcedure
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicID", topic.Id);
      command.AddParameter("ParentID", target.Id);

      // Append sibling ID if set
      if (sibling != null) {
        command.AddParameter("SiblingID", sibling.Id);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {
        connection.Open();
        command.ExecuteNonQuery();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException(
          $"Failed to move Topic '{topic.Key}' ({topic.Id}) to '{target.Key}' ({target.Id}): '{exception.Message}'",
          exception
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        command?.Dispose();
        connection.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset dirty status
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ParentId", target.Id.ToString(CultureInfo.InvariantCulture), false);

    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Delete(Topic topic, bool isRecursive = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Delete(topic, isRecursive);

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("DeleteTopic", connection) {
        CommandType             = CommandType.StoredProcedure
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicID", topic.Id);

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {
        connection.Open();
        command.ExecuteNonQuery();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException(
          $"Failed to delete Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
          exception
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        command?.Dispose();
        connection?.Dispose();
      }

    }

    /*==========================================================================================================================
    | METHOD: PERSIST RELATIONS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Internal method that saves topic relationships to the n:n mapping table in SQL, returns a XML-formatted string for
    ///   appending to the extended attributes unless <c>skipXml == true</c>.
    /// </summary>
    /// <param name="topic">The topic object whose relationships should be persisted.</param>
    /// <param name="connection">The SQL connection.</param>
    /// <param name="skipXml">
    ///   Boolean indicator noting whether attributes saved in the XML should be skipped as part of the operation.
    /// </param>
    /// <returns>
    ///   An XML-formatted string representing the <see cref="Topic.Relationships"/> XML content, or a blank string if
    ///   <c>skipXml == true</c>.
    /// </returns>
    private static string PersistRelations(Topic topic, SqlConnection connection, bool skipXml) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Return blank if the topic has no relations.
      \-----------------------------------------------------------------------------------------------------------------------*/
      // return "" if the topic has no relations
      if (!topic.Relationships.Keys.Any()) {
        return "";
      }
      var command               = (SqlCommand?)null;
      var targetIds             = new DataTable();

      targetIds.Columns.Add(
        new DataColumn("TopicID", typeof(int))
      );

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (var key in topic.Relationships.Keys) {

          var scope             = topic.Relationships.GetTopics(key);
          var topicId           = topic.Id.ToString(CultureInfo.InvariantCulture);

          command               = new SqlCommand("UpdateRelationships", connection) {
            CommandType         = CommandType.StoredProcedure
          };

          foreach (var targetTopicId in scope.Select<Topic, int>(m => m.Id)) {
            var record          = targetIds.NewRow();
            record["TopicID"]   = targetTopicId;
            targetIds.Rows.Add(record);
          }

          // Add Parameters
          command.Parameters.AddWithValue("TopicID", topicId);
          command.Parameters.AddWithValue("RelationshipKey", key);
          command.Parameters.AddWithValue("RelatedTopics", targetIds);

          command.ExecuteNonQuery();

        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException(
          $"Failed to persist relationships for Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
          exception
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        command?.Dispose();
        targetIds.Dispose();
        //Since the SQL connection is being passed in, do not close connection; this allows command pooling.
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the relationship attributes to append to the XML attributes (unless skipXml is set to true)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (skipXml) return "";
      else return CreateRelationshipsXml(topic);

    }

    /*==========================================================================================================================
    | METHOD: CREATE RELATIONSHIPS XML
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Internal helper function to build string of related XML nodes for each scope of related items in model.
    /// </summary>
    /// <param name="topic">The topic object for which to create the relationships.</param>
    /// <returns>The XML string.</returns>
    private static string CreateRelationshipsXml(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Create XML string container
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributesXml         = new StringBuilder("");

      /*------------------------------------------------------------------------------------------------------------------------
      | Add a related XML node for each scope
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var key in topic.Relationships.Keys) {

        var scope               = topic.Relationships.GetTopics(key);

        attributesXml.Append("<related scope=\"");
        attributesXml.Append(key);
        attributesXml.Append("\">");

        // Build out string array of related items in this scope
        var targetIds           = new string[scope.Count];
        var count               = 0;
        foreach (var relTopic in scope) {
          targetIds[count]      = relTopic.Id.ToString(CultureInfo.InvariantCulture);
          count++;
        }
        attributesXml.Append(String.Join(",", targetIds));
        attributesXml.Append("</related>");
      }

      return attributesXml.ToString();
    }

  } //Class
} //Namespace