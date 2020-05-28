/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
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
      var topicId               = -1;

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
      | Validate results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId < 0) {
        throw new TopicNotFoundException(topicKey);
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
          throw new TopicNotFoundException(topicId);
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
      return topic?? throw new TopicNotFoundException(topicId);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override int Save([NotNull]Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var unresolvedTopics      = new List<Topic>();
      var connection            = new SqlConnection(_connectionString);

      connection.Open();

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle first pass
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId = Save(topic, isRecursive, isDraft, connection, unresolvedTopics);

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to resolve outstanding relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var unresolvedTopic in unresolvedTopics) {
        Save(unresolvedTopic, false, isDraft, connection, new List<Topic>());
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      connection.Close();
      connection.Dispose();
      return topicId;

    }

    /// <summary>
    ///   The private overload of the <see cref="Save"/> method provides support for sharing the <see cref="SqlConnection"/>
    ///   between multiple requests, and maintaining a list of <paramref name="unresolvedRelationships"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     When recursively saving a topic graph, it is conceivable that references to other topics—such as <see
    ///     cref="Topic.Relationships"/> or <see cref="Topic.DerivedTopic"/>—can't yet be persisted because the target <see
    ///     cref="Topic"/> hasn't yet been saved, and thus the <see cref="Topic.Id"/> is still set to <c>-1</c>. To mitigate
    ///     this, the <paramref name="unresolvedRelationships"/> allows this private overload to keep track of unresolved
    ///     relationships. The public <see cref="Save(Topic, Boolean, Boolean)"/> overload uses this list to resave any topics
    ///     that include such references. This adds some overhead due to the duplicate <see cref="Save"/>, but helps avoid
    ///     potential data loss when working with complex topic graphs.
    ///   </para>
    ///   <para>
    ///     The connection sharing probably doesn't provide that much of a gain in that .NET does a good job of connection
    ///     pooling. Nevertheless, there is some overhead to opening a new connection, so sharing an open connection when we
    ///     doing a recursive save could potentially provide some performance benefit.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> to save.</param>
    /// <param name="isRecursive">Determines whether or not to recursively save <see cref="Topic.Children"/>.</param>
    /// <param name="isDraft">Determines if the <see cref="Topic"/> should be saved as a draft version.</param>
    /// <param name="connection">The open <see cref="SqlConnection"/> to use for executing <see cref="SqlCommand"/>s.</param>
    /// <param name="unresolvedRelationships">A list of <see cref="Topic"/>s with unresolved topic references.</param>
    private int Save(
      [NotNull]Topic topic,
      bool isRecursive,
      bool isDraft,
      SqlConnection connection,
      List<Topic> unresolvedRelationships
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var isNew                 = topic.Id == -1;
      var version               = new SqlDateTime(DateTime.Now);
      var areReferencesResolved = true;
      var areRelationshipsDirty = topic.Relationships.IsDirty();
      var areAttributesDirty    = topic.Attributes.IsDirty(excludeLastModified: true);
      var extendedAttributeList = GetAttributes(topic, isExtendedAttribute: true);
      var indexedAttributeList  = GetAttributes(
        topic                   : topic,
        isExtendedAttribute     : false,
        isDirty                 : true,
        excludeLastModified     : !areAttributesDirty
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Detect whether anything has changed
      >-------------------------------------------------------------------------------------------------------------------------
      | If no relationships have changed, and no attributes values have changed, and there aren't any mismatched attributes in
      | their respective lists, then there isn't anything new to persist to the database, and thus no benefit to executing the
      | current command. A more aggressive version of this would wrap much of the below logic in this, but this is just meant
      | as a quick fix to reduce the overhead of recursive saves.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var isDirty               =
        areRelationshipsDirty   ||
        areAttributesDirty      ||
        indexedAttributeList.Any() ||
        extendedAttributeList.Any(a => a.IsExtendedAttribute == false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass is not dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isDirty) {
        recurse();
        return topic.Id;
      }

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
      foreach (var attributeValue in indexedAttributeList) {

        var record              = attributes.NewRow();
        record["AttributeKey"]  = attributeValue.Key;
        record["AttributeValue"]= attributeValue.Value;
        attributeValue.IsDirty  = false;

        attributes.Rows.Add(record);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add extended attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      extendedAttributes.Append("<attributes>");

      foreach (var attributeValue in extendedAttributeList) {

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
      var procedureName         = isNew? "CreateTopic" : "UpdateTopic";
      var command               = new SqlCommand(procedureName, connection) {
        CommandType             = CommandType.StoredProcedure
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle unresolved references
      >-------------------------------------------------------------------------------------------------------------------------
      | If it's a recursive save and there are any unresolved relationships, come back to this after the topic graph has been
      | saved; that ensures that any relationships within the topic graph have been saved and can be properly persisted. The
      | same can be done for DerivedTopics references, which are effectively establish a 1:1 relationship.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive && (topic.DerivedTopic?.Id < 0 || topic.Relationships.Any(r => r.Any(t => t.Id < 0)))) {
        unresolvedRelationships.Add(topic);
        areReferencesResolved = false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isNew) {
        command.AddParameter("TopicID", topic.Id);
        command.AddParameter("DeleteRelationships", areReferencesResolved && areRelationshipsDirty);
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

        command.ExecuteNonQuery();

        topic.Id                = command.GetReturnCode();

        Contract.Assume<InvalidOperationException>(
          topic.Id > 0,
          "The call to the CreateTopic stored procedure did not return the expected 'Id' parameter."
        );

        if (areReferencesResolved && areRelationshipsDirty) {
          PersistRelations(topic, connection, true);
        }

        topic.VersionHistory.Insert(0, version.Value);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        connection?.Dispose();
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
        attributes.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      recurse();
      return topic.Id;

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse
      \-----------------------------------------------------------------------------------------------------------------------*/
      void recurse() {
        if (isRecursive) {
          foreach (var childTopic in topic.Children) {
            childTopic.Attributes.SetValue("ParentID", topic.Id.ToString(CultureInfo.InvariantCulture));
            Save(childTopic, isRecursive, isDraft, connection, unresolvedRelationships);
          }
        }
      }

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
          var relatedTopics     = scope.Where(t => t.Id > 0).Select<Topic, int>(m => m.Id);

          command               = new SqlCommand("UpdateRelationships", connection) {
            CommandType         = CommandType.StoredProcedure
          };

          foreach (var targetTopicId in relatedTopics) {
            var record          = targetIds.NewRow();
            record["TopicID"]   = targetTopicId;
            targetIds.Rows.Add(record);
          }

          // Add Parameters
          command.Parameters.AddWithValue("TopicID", topicId);
          command.Parameters.AddWithValue("RelationshipKey", key);
          command.Parameters.AddWithValue("RelatedTopics", targetIds);

          command.ExecuteNonQuery();

          // Clear rows
          targetIds.Clear();
          scope.IsDirty = relatedTopics.Count() < scope.Count;

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
        //Since the SQL connection is being passed in, do not close connection; this allows connection pooling.
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the relationship attributes to append to the XML attributes (unless skipXml is set to true)
      \-----------------------------------------------------------------------------------------------------------------------*/
      return skipXml? "" : CreateRelationshipsXml(topic);

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