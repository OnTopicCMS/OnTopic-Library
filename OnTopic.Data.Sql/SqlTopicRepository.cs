/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Data.SqlClient;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;
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
      _connectionString = connectionString;

    }

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static void AddTopic(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(reader, nameof(reader));
      Contract.Requires(topics, nameof(topics));

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = reader.GetInteger("TopicID");
      var key                   = reader.GetString("TopicKey");
      var contentType           = reader.GetString("ContentType");
      var parentId              = reader.GetInteger("ParentID");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current = TopicFactory.Create(key, contentType, id);
      topics.Add(current.Id, current);

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parentId >= 0 && topics.Keys.Contains(parentId)) {
        current.Attributes.SetValue("ParentID", parentId.ToString(CultureInfo.InvariantCulture), false);
        current.Parent = topics[parentId];
      }

    }

    /*==========================================================================================================================
    | METHOD: SET INDEXED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static void SetIndexedAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(reader, nameof(reader));
      Contract.Requires(topics, nameof(topics));

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = reader.GetInteger("TopicID");
      var name                  = reader.GetString("AttributeKey");
      var value                 = reader.GetString("AttributeValue");
      var version               = DateTime.Now;

      //Check field count to avoid breaking changes with the 4.0.0 release, which didn't include a "Version" column
      //### TODO JJC20200221: This condition can be removed and accepted as a breaking change in v5.0.
      if (reader.FieldCount > 3) {
        version                 = reader.GetDateTime("Version");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate conditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(id, $"The 'TopicID' field is missing from the topic. This is an unexpected condition.");
      Contract.Assume(name, $"The 'AttributeKey' field is missing from the topic. This is an unexpected condition.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty attributes (treat empty as null)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) || DBNull.Value.Equals(value)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current = topics[id];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.Attributes.SetValue(name, value, false, version);

    }

    /*==========================================================================================================================
    | METHOD: SET EXTENDED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds attributes retrieved from an individual XML record to their associated topic.
    /// </summary>
    /// <remarks>
    ///   Values of arbitrary length are stored in an XML entry. This makes them more efficient to store, but more difficult to
    ///   query; as such, it's ideal for content-oriented data. The XML values are returned as a separate data set.
    /// </remarks>
    /// <param name="reader">The <see cref="System.Data.SqlClient.SqlDataReader"/> that representing the current record.</param>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetExtendedAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topics, "The topics Dictionary must not be null.");
      Contract.Requires(reader, nameof(reader));

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = reader.GetInteger("TopicID");
      var version               = DateTime.Now;

      //Check field count to avoid breaking changes with the 4.0.0 release, which didn't include a "Version" column
      //### TODO JJC20200221: This condition can be removed and accepted as a breaking change in v5.0.
      if (reader.FieldCount > 2) {
        version                 = reader.GetDateTime("Version");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate conditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(id, $"The 'TopicID' field is missing from the topic. This is an unexpected condition.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Load SQL XML into XmlDocument
      \-----------------------------------------------------------------------------------------------------------------------*/
      var xmlData               = reader.GetSqlXml(1);
      var xmlReader             = xmlData.CreateReader();

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify the current topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[id];

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle scenario where there isn't an <attribute /> element
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!xmlReader.ReadToFollowing("attribute")) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through nodes to set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      do {

        /*----------------------------------------------------------------------------------------------------------------------
        | Identify attributes
        \---------------------------------------------------------------------------------------------------------------------*/
        var name = (string)xmlReader.GetAttribute("key");
        var value = WebUtility.HtmlDecode(xmlReader.ReadInnerXml());

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate assumptions
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(
          name,
          $"The @key attribute of the <attribute /> element is missing for Topic '{id}'; the data is not in the expected format."
        );

        /*----------------------------------------------------------------------------------------------------------------------
        | Set attribute value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (String.IsNullOrEmpty(value)) continue;
        current.Attributes.SetValue(name, value, false, version);

      } while (xmlReader.Name == "attribute");

    }

    /*==========================================================================================================================
    | METHOD: SET RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds relationships retrieved from an individual relationship record to their associated topics.
    /// </summary>
    /// <remarks>
    ///   Topics can be cross-referenced with each other via a many-to-many relationships. Once the topics are populated in
    ///   memory, loop through the data to create these associations.
    /// </remarks>
    /// <param name="reader">The <see cref="System.Data.SqlClient.SqlDataReader"/> that representing the current record.</param>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetRelationships(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topics, "The topics Dictionary must not be null.");
      Contract.Requires(reader, "The reader must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = reader.GetInteger("Source_TopicID");
      var targetTopicId         = reader.GetInteger("Target_TopicID");
      var relationshipKey       = reader.GetString("RelationshipKey");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];
      var related               = (Topic?)null;

      // Fetch the related topic
      if (topics.Keys.Contains(targetTopicId)) {
        related = topics[targetTopicId];
      }

      // Bypass if either of the objects are missing
      if (related == null) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship on object
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.Relationships.SetTopic(relationshipKey, related);

    }

    /*==========================================================================================================================
    | METHOD: SET DERIVED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets references to <see cref="OnTopic.Topic.DerivedTopic"/>.
    /// </summary>
    /// <remarks>
    ///   Topics can be cross-referenced with each other via <see cref="OnTopic.Topic.DerivedTopic"/>. Once the topics are
    ///   populated in memory, loop through the data to create these associations. By handling this in the repository, we avoid
    ///   needing to rely on lazy-loading, which would complicate dependency injection.
    /// </remarks>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetDerivedTopics(Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topics, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in topics.Values) {
        var derivedTopicId = topic.Attributes.GetInteger("TopicId", -1, false, false);
        if (derivedTopicId < 0) continue;
        if (topics.Keys.Contains(derivedTopicId)) {
          topic.DerivedTopic = topics[derivedTopicId];
        }

      }

    }

    /*==========================================================================================================================
    | METHOD: SET VERSION HISTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds versions retrieved from an individual topic record to its associated topics.
    /// </summary>
    /// <remarks>
    ///   Every time a value changes for an attribute, a new version is created, represented by the date of the change.This
    ///   version history is aggregated per topic to allow topic information to be rolled back to a specific date.While version
    ///   content is not exposed directly via the Load() method, the metadata is.
    /// </remarks>
    /// <param name="reader">The <see cref="System.Data.SqlClient.SqlDataReader"/> that representing the current record.</param>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetVersionHistory(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topics, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = reader.GetInteger("TopicID");
      var dateTime              = reader.GetDateTime("Version");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set history
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.VersionHistory.Add(dateTime);

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
      if (topicKey is null || topicKey.Trim().Length == 0) {
        return Load(-1, isRecursive);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopicID", connection);
      int topicId;

      command.CommandType = CommandType.StoredProcedure;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Open connection
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        command.AddParameter("TopicKey", topicKey);
        command.AddOutputParameter("ReturnCode");

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topics
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume<InvalidOperationException>(
          command.Parameters["@ReturnCode"] != null,
          "The call to the GetTopicID stored procedure did not return the expected 'ReturnCode' parameter."
        );
        topicId = Int32.Parse(command.Parameters["@ReturnCode"].Value.ToString(), CultureInfo.InvariantCulture);

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
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopics", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };
      var reader                = (SqlDataReader?)null;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Open connection
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        command.AddParameter("TopicID", topicId);
        command.AddParameter("DeepLoad", isRecursive);

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query/reader
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): ExecuteNonQuery [" + DateTime.Now + "]");
        reader = command.ExecuteReader();

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topics
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): AddTopic() [" + DateTime.Now + "]");
        while (reader.Read()) {
          AddTopic(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read attributes
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetIndexedAttributes() [" + DateTime.Now + "]");

        // Move to TopicAttributes dataset
        reader.NextResult();

        while (reader.Read()) {
          SetIndexedAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read extended attributes
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetExtendedAttributes() [" + DateTime.Now + "]");

        // Move to extened attributes dataset
        reader.NextResult();

        // Loop through each extended attribute record associated with a specific topic
        while (reader.Read()) {
          SetExtendedAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read related items
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetRelationships() [" + DateTime.Now + "]");

        // Move to the relationships dataset
        reader.NextResult();

        // Loop through each relationship; multiple records may exist per topic
        while (reader.Read()) {
          SetRelationships(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read version history
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetVersionHistory() [" + DateTime.Now + "]");

        // Move to the version history dataset
        reader.NextResult();

        // Loop through each version; multiple records may exist per topic
        while (reader.Read()) {
          SetVersionHistory(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate strongly typed references
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetDerivedTopics() [" + DateTime.Now + "]");

        SetDerivedTopics(topics);

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
      if (topics.Count == 0) {
        throw new NullReferenceException($"Load() was unable to successfully load the topic with the TopicID '{topicId}'");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topics[topics.Keys.ElementAt(0)];

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
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("GetTopicVersion", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };
      var reader                = (SqlDataReader?)null;

      command.CommandType       = CommandType.StoredProcedure;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Open connection
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        command.AddParameter("TopicID", topicId);
        command.AddParameter("Version", version);

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query/reader
        \---------------------------------------------------------------------------------------------------------------------*/
        reader = command.ExecuteReader();

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topic
        \---------------------------------------------------------------------------------------------------------------------*/
        while (reader.Read()) {
          AddTopic(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate topic
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(
          topics.Count == 1,
          "The version requested does not exist in the SQL database."
        );

        /*----------------------------------------------------------------------------------------------------------------------
        | Read attributes
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to TopicAttributes dataset
        reader.NextResult();

        while (reader.Read()) {
          SetIndexedAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read extended attributes
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to extended attributes dataset
        reader.NextResult();

        // Loop through each extended attribute set, each record associated with a specific record
        while (reader.Read()) {
          SetExtendedAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read related items
        >-----------------------------------------------------------------------------------------------------------------------
        | ### NOTE JJC072617: While GetTopicVersion correctly returns relationships, they cannot be correctly set because this
        | overload doesn't maintain a full set of topics to create relationships to. This shouldn't be an issue since
        | relationships are not currently versioned.
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to the relationships dataset
        reader.NextResult();

        /*----------------------------------------------------------------------------------------------------------------------
        | Read version history
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to the version history dataset
        reader.NextResult();

        // Loop through each version; multiple records may exist per topic
        while (reader.Read()) {
          SetVersionHistory(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate strongly typed references
        >-----------------------------------------------------------------------------------------------------------------------
        | ### NOTE JJC072617: While getTopicVersion correctly returns the derived topic, it cannot be correctly set because this
        | overload doesn't maintain a full set of topics to create relationships to.
        \---------------------------------------------------------------------------------------------------------------------*/
        //SetDerivedTopics(topics);

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
      return topics[topics.Keys.ElementAt(0)]?? throw new NullReferenceException("The specified Topic version could not be loaded");

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
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType = GetContentTypeDescriptors()[topic.Attributes.GetValue("ContentType", "Page")?? "Page"];

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

      Contract.Assume(
        contentType,
        "The Topics repository or database does not contain a ContentTypeDescriptor for the Page content type."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Add indexed attributes that are dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attributeValue in GetAttributes(topic, false, true)) {

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

      foreach (var attributeValue in GetAttributes(topic, true, null)) {
        extendedAttributes.Append(
          "<attribute key=\"" + attributeValue.Key + "\"><![CDATA[" + attributeValue.Value + "]]></attribute>"
        );
        attributeValue.IsDirty = false;
      }

      extendedAttributes.Append("</attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in contentType.AttributeDescriptors) {

        // Set preconditions
        var topicHasAttribute   = (topic.Attributes.Contains(attribute.Key) && !String.IsNullOrEmpty(topic.Attributes.GetValue(attribute.Key, null, false, false)));
        var isPrimaryAttribute  = (attribute.Key == "Key" || attribute.Key == "ContentType" || attribute.Key == "ParentID");
        var isRelationships     = (attribute.EditorType == "Relationships.ascx");
        var isNestedTopic       = (attribute.EditorType == "TopicList.ascx");
        var conditionsMet       = (!topicHasAttribute && !isPrimaryAttribute && !attribute.IsExtendedAttribute && !isRelationships && !isNestedTopic && topic.Id != -1);

        if (conditionsMet) {
          var record = attributes.NewRow();
          record["AttributeKey"] = attribute.Key;
          record["AttributeValue"] = null;
          attributes.Rows.Add(record);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection = new SqlConnection(_connectionString);
      var command = (SqlCommand?)null;
      var returnVal = -1;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Update relations
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish command type (insert or update)
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          command = new SqlCommand("UpdateTopic", connection);
        }
        else {
          command = new SqlCommand("CreateTopic", connection);
        }

        command.CommandType = CommandType.StoredProcedure;

        /*----------------------------------------------------------------------------------------------------------------------
        | SET VERSION DATETIME
        \---------------------------------------------------------------------------------------------------------------------*/
        var version = DateTime.Now;

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          command.AddParameter("TopicID", topic.Id);
        }
        else if (topic.Parent != null) {
          command.AddParameter("ParentID", topic.Parent.Id);
        }
        command.AddParameter("Version", version);
        command.Parameters.AddWithValue("@Attributes", attributes);
        if (topic.Id != -1) {
          command.AddParameter("DeleteRelationships", true);
        }
        command.AddParameter("ExtendedAttributes", extendedAttributes);
        command.AddOutputParameter("ReturnCode");

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume<InvalidOperationException>(
          command.Parameters["@ReturnCode"] != null,
          "The call to the CreateTopic stored procedure did not return the expected 'ReturnCode' parameter."
        );
        returnVal = Int32.Parse(command.Parameters["@ReturnCode"].Value.ToString(), CultureInfo.InvariantCulture);

        topic.Id = returnVal;

        /*----------------------------------------------------------------------------------------------------------------------
        | Add version to version history
        \---------------------------------------------------------------------------------------------------------------------*/
        topic.VersionHistory.Insert(0, version);

        /*----------------------------------------------------------------------------------------------------------------------
        | Update relations
        \---------------------------------------------------------------------------------------------------------------------*/
        PersistRelations(topic, connection, true);

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
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
        if (attributes != null) attributes.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          Contract.Assume<InvalidOperationException>(
            childTopic.Attributes.GetInteger("ParentID", -1) > 0,
            "The call to the CreateTopic stored procedure did not return the expected 'ParentID' parameter."
          );
          childTopic.Attributes.SetValue("ParentID", returnVal.ToString(CultureInfo.InvariantCulture));
          Save(childTopic, isRecursive, isDraft);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return returnVal;

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
      | Move in database
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection = new SqlConnection(_connectionString);
      var command = (SqlCommand?)null;

      try {

        command = new SqlCommand("MoveTopic", connection) {
          CommandType = CommandType.StoredProcedure
        };

        // Add Parameters
        command.AddParameter("TopicID", topic.Id);
        command.AddParameter("ParentID", target.Id);

        // Append sibling ID if set
        if (sibling != null) {
          command.AddParameter("SiblingID", sibling.Id);
        }

        // Execute Query
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
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset dirty status
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ParentId", target.Id.ToString(CultureInfo.InvariantCulture), false);

      //return true;

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
      var connection = new SqlConnection(_connectionString);
      SqlCommand? command = null;

      try {

        command = new SqlCommand("DeleteTopic", connection) {
          CommandType = CommandType.StoredProcedure
        };

        // Add Parameters
        command.AddParameter("TopicID", topic.Id);

        // Execute Query
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
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
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
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string PersistRelations(Topic topic, SqlConnection connection, bool skipXml) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, "The topic must not be null.");

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
            var record = targetIds.NewRow();
            record["TopicID"] = targetTopicId;
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
        if (command != null) command.Dispose();
        if (targetIds != null) targetIds.Dispose();
        //Since the SQL connection is being passed in, do not close connection; this allows command pooling.
        //if (connection != null) connection.Dispose();
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
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string CreateRelationshipsXml(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, "The topic must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Create XML string container
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributesXml = new StringBuilder("");

      /*------------------------------------------------------------------------------------------------------------------------
      | Add a related XML node for each scope
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var key in topic.Relationships.Keys) {
        var scope = topic.Relationships.GetTopics(key);
        attributesXml.Append("<related scope=\"");
        attributesXml.Append(key);
        attributesXml.Append("\">");

        // Build out string array of related items in this scope
        var targetIds = new string[scope.Count];
        var count = 0;
        foreach (var relTopic in scope) {
          targetIds[count] = relTopic.Id.ToString(CultureInfo.InvariantCulture);
          count++;
        }
        attributesXml.Append(String.Join(",", targetIds));
        attributesXml.Append("</related>");
      }

      return attributesXml.ToString();
    }



  } //Class
} //Namespace