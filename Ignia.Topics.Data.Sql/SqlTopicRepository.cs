/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Metadata;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using Ignia.Topics.Repositories;
using System.Diagnostics.CodeAnalysis;

namespace Ignia.Topics.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in Microsoft SQL Server.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="Ignia.Topics.Repositories.IDataRepository"/> class.
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

      Contract.Requires(reader["ParentID"], "The SqlDataReader is expected to include a 'ParentID' element.");
      Contract.Requires(reader["TopicID"], "The SqlDataReader is expected to include a 'ParentID' element.");
      Contract.Requires(reader["ContentType"], "The SqlDataReader is expected to include a 'ContentType' element.");
      Contract.Requires(reader["TopicKey"], "The SqlDataReader is expected to include a 'TopicKey' element.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int parentId              = Int32.TryParse(reader["ParentID"].ToString(), out parentId)? parentId : -1;
      var id                    = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
      var contentType           = reader["ContentType"].ToString();
      var key                   = reader["TopicKey"].ToString();

      // Handle ParentID (could be null for root topic)

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
      var id                    = Int32.Parse(reader["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      var name                  = reader["AttributeKey"]?.ToString();
      var value                 = reader["AttributeValue"]?.ToString();

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
      current.Attributes.SetValue(name, value, false);

    }

    /*==========================================================================================================================
    | METHOD: SET BLOB ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds attributes retrieved from an individual blob record to their associated topic.
    /// </summary>
    /// <remarks>
    ///   Values of arbitrary length are stored in an XML blob. This makes them more efficient to store, but more difficult to
    ///   query; as such, it's ideal for content-oriented data. The blob values are returned as a separate data set.
    /// </remarks>
    /// <param name="reader">The <see cref="System.Data.SqlClient.SqlDataReader"/> that representing the current record.</param>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetBlobAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topics, "The topics Dictionary must not be null.");
      Contract.Requires(reader, nameof(reader));

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = Int32.Parse(reader["TopicID"]?.ToString(), CultureInfo.InvariantCulture);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate conditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(id, $"The 'TopicID' field is missing from the topic. This is an unexpected condition.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Load blob into XmlDocument
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
        current.Attributes.SetValue(name, value, false);

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

      Contract.Requires(reader["Source_TopicID"], "The Source_TopicID record must not be null.");
      Contract.Requires(reader["Target_TopicID"], "The Target_TopicID record must not be null.");
      Contract.Requires(reader["RelationshipTypeID"], "The RelationshipTypeID record must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = Int32.Parse(reader["Source_TopicID"].ToString(), CultureInfo.InvariantCulture);
      var targetTopicId         = Int32.Parse(reader["Target_TopicID"].ToString(), CultureInfo.InvariantCulture);
      var relationshipTypeId    = (string)reader["RelationshipTypeID"];

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
      current.Relationships.SetTopic(relationshipTypeId, related);

    }

    /*==========================================================================================================================
    | METHOD: SET DERIVED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets references to <see cref="Ignia.Topics.Topic.DerivedTopic"/>.
    /// </summary>
    /// <remarks>
    ///   Topics can be cross-referenced with each other via <see cref="Ignia.Topics.Topic.DerivedTopic"/>. Once the topics are
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
      var sourceTopicId         = Int32.Parse(reader?["TopicId"]?.ToString(), CultureInfo.InvariantCulture);
      var dateTime              = Convert.ToDateTime(reader?["Version"]?.ToString(), CultureInfo.InvariantCulture);

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

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified topic key.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
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
      var command               = new SqlCommand("topics_GetTopicID", connection);
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
        AddSqlParameter(command,        "TopicKey",             topicKey,                               SqlDbType.VarChar);
        AddSqlParameter(command,        "ReturnCode",           ParameterDirection.ReturnValue,         SqlDbType.Int);

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topics
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume<InvalidOperationException>(
          command.Parameters["@ReturnCode"] != null,
          "The call to the topics_GetTopicID stored procedure did not return the expected 'ReturnCode' parameter."
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

    /// <summary>
    ///   Loads a topic (and, optionally, all of its descendants) based on the specified unique identifier.
    /// </summary>
    /// <param name="topicId">The topic's unique identifier.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    /// <exception cref="Exception">
    ///   The topic Ignia.Topics.<c>contentType</c> does not derive from Ignia.Topics.Topic.
    /// </exception>
    /// <exception cref="Exception">
    ///   Topics failed to load: <c>ex.Message</c>
    /// </exception>
    public override Topic Load(int topicId, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("topics_GetTopics", connection) {
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
        AddSqlParameter(command, "TopicID",      topicId.ToString(CultureInfo.InvariantCulture),       SqlDbType.Int);
        AddSqlParameter(command, "DeepLoad",     isRecursive.ToString(CultureInfo.InvariantCulture),   SqlDbType.Bit);

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
        | Read blob
        \---------------------------------------------------------------------------------------------------------------------*/
        Debug.WriteLine("SqlTopicRepository.Load(): SetBlobAttributes() [" + DateTime.Now + "]");

        // Move to blob dataset
        reader.NextResult();

        // Loop through each blob, each record associated with a specific record
        while (reader.Read()) {
          SetBlobAttributes(reader, topics);
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

    /// <summary>
    ///   Loads a specific version of a topic based on its version.
    /// </summary>
    /// <remarks>
    ///   This overload does not accept an argument for recursion; it will only load a single instance of a version. Further,
    ///   it will only load versions for which the unique identifier is known.
    /// </remarks>
    /// <param name="topicId">The topic identifier.</param>
    /// <param name="version">The version.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(int topicId, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date > new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("topics_GetVersion", connection) {
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
        AddSqlParameter(command, "TopicID",      topicId.ToString(CultureInfo.InvariantCulture),       SqlDbType.Int);
        AddSqlParameter(command, "Version",      version.ToString(CultureInfo.InvariantCulture),       SqlDbType.DateTime);

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query/reader
        \---------------------------------------------------------------------------------------------------------------------*/
        reader = command.ExecuteReader();

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topics
        \---------------------------------------------------------------------------------------------------------------------*/
        while (reader.Read()) {
          AddTopic(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read attributes
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to TopicAttributes dataset
        reader.NextResult();

        while (reader.Read()) {
          SetIndexedAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read blob
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to blob dataset
        reader.NextResult();

        // Loop through each blob, each record associated with a specific record
        while (reader.Read()) {
          SetBlobAttributes(reader, topics);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read related items
        >-----------------------------------------------------------------------------------------------------------------------
        | ### NOTE JJC072617: While getVersion correctly returns relationships, they cannot be correctly set because this
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
        | ### NOTE JJC072617: While getVersion correctly returns the derived topic, it cannot be correctly set because this
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
    | ###TODO JJC080314: An overload to Load() should be created to accept an XmlDocument or XmlNode based on the proposed
    | Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: If the topic already exists, return the existing node, by calling its Merge() function. Otherwise,
    | construct a new node using its XmlNode constructor.
    >---------------------------------------------------------------------------------------------------------------------------
      public static Topic Load(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() to validate against whatever objects are already created and
      //available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that saves topic attributes; also used for renaming a topic since name is stored as an attribute.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>The integer return value from the execution of the <c>topics_UpdateTopic</c> stored procedure.</returns>
    /// <exception cref="Exception">
    ///   The Content Type <c>topic.Attributes.GetValue(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found under
    ///   Configuration:ContentTypes. There are <c>ContentTypes.Count</c> ContentTypes in cached in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via connection string: <c>ex.Message</c>
    /// </exception>
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
      | Establish attribute strings
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Strings are immutable, use a StringBuilder to save memory
      var attributes            = new StringBuilder();
      var nullAttributes        = new StringBuilder();
      var blob                  = new StringBuilder();

      Contract.Assume(
        contentType,
        "The Topics repository or database does not contain a ContentTypeDescriptor for the Page content type."
      );

      blob.Append("<attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the attributes, adding the names and values to the string builder
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Process attributes not stored in the Blob
      foreach (var attributeValue in topic.Attributes) {

        var key = attributeValue.Key;
        var attribute = (AttributeDescriptor?)null;

        if (contentType.AttributeDescriptors.Contains(key)) {
          attribute = contentType.AttributeDescriptors[key];
        }

        // For attributes not stored in the Blob, only add the AttributeValue item to store if it has changed
        if (attribute != null && !attribute.StoreInBlob && attributeValue.IsDirty) {
          attributes.Append(key + "~~" + attributeValue.Value + "``");
        }
        else if (attribute != null && attribute.StoreInBlob) {
          blob.Append("<attribute key=\"" + key + "\"><![CDATA[" + attributeValue.Value + "]]></attribute>");
        }

        // Reset IsDirty (changed) state
        attributeValue.IsDirty = false;

      }

      blob.Append("</attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in contentType.AttributeDescriptors) {

        // Set preconditions
        var topicHasAttribute   = (topic.Attributes.Contains(attribute.Key) && !String.IsNullOrEmpty(topic.Attributes.GetValue(attribute.Key, null, false, false)));
        var isPrimaryAttribute  = (attribute.Key == "Key" || attribute.Key == "ContentType" || attribute.Key == "ParentID");
        var isRelationships     = (attribute.EditorType == "Relationships.ascx");
        var isNestedTopic       = (attribute.EditorType == "TopicList.ascx");
        var conditionsMet       = (!topicHasAttribute && !isPrimaryAttribute && !attribute.StoreInBlob && !isRelationships && !isNestedTopic && topic.Id != -1);

        if (conditionsMet) {
          nullAttributes.Append(attribute.Key + ",");
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
          command = new SqlCommand("topics_UpdateTopic", connection);
        }
        else {
          command = new SqlCommand("topics_CreateTopic", connection);
        }

        command.CommandType = CommandType.StoredProcedure;

        /*----------------------------------------------------------------------------------------------------------------------
        | SET VERSION DATETIME
        \---------------------------------------------------------------------------------------------------------------------*/
        var version = DateTime.Now;

        // NOTE: KLT031915: Commented out as Draft functionality is not fully implemented
        // if (isDraft) {
        //   version            = DateTime.MaxValue;
        // }

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          AddSqlParameter(
            command,
            "TopicID",
            topic.Id.ToString(CultureInfo.InvariantCulture),
            SqlDbType.Int
          );
        }
        if (topic.Parent != null) {
          AddSqlParameter(
            command,
            "ParentID",
            topic.Parent.Id.ToString(CultureInfo.InvariantCulture),
            SqlDbType.Int
          );
        }
        AddSqlParameter(
          command,
          "Version",
          version.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
          SqlDbType.DateTime
        );
        AddSqlParameter(
          command,
          "Attributes",
          attributes.ToString(),
          SqlDbType.VarChar
        );
        if (topic.Id != -1) {
          AddSqlParameter(
            command,
            "NullAttributes",
            nullAttributes.ToString(),
            SqlDbType.VarChar
          );
          AddSqlParameter(
            command,
            "DeleteRelationships",
            "1",
            SqlDbType.Bit
          );
        }
        AddSqlParameter(
          command,
          "Blob",
          blob.ToString(),
          SqlDbType.Xml
        );
        AddSqlParameter(
          command,
          "ReturnCode",
          ParameterDirection.ReturnValue,
          SqlDbType.Int
        );

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume<InvalidOperationException>(
          command.Parameters["@ReturnCode"] != null,
          "The call to the topics_CreateTopic stored procedure did not return the expected 'ReturnCode' parameter."
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
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          Contract.Assume<InvalidOperationException>(
            childTopic.Attributes.GetInteger("ParentID", -1) > 0,
            "The call to the topics_CreateTopic stored procedure did not return the expected 'ParentID' parameter."
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
    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <remarks>
    ///   May optionally specify a sibling. If specified, it is expected that the topic will be placed immediately after the
    ///   topic.
    /// </remarks>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">The target (parent) topic object under which the topic should be moved.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
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

        command = new SqlCommand("topics_MoveTopic", connection) {
          CommandType = CommandType.StoredProcedure
        };

        // Add Parameters
        AddSqlParameter(command, "TopicID", topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        AddSqlParameter(command, "ParentID", target.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

        // Append sibling ID if set
        if (sibling != null) {
          AddSqlParameter(command, "SiblingID", sibling.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
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
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree
    /// </summary>
    /// <param name="topic">The topic object to delete.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well. If set to false
    ///   (the default) and the topic has children, including any nested topics, an exception will be thrown.
    /// </param>
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    /// <exception cref="Exception">Failed to delete Topic <c>topic.Key</c> (<c>topic.Id</c>): <c>ex.Message</c></exception>
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

        command = new SqlCommand("topics_DeleteTopic", connection) {
          CommandType = CommandType.StoredProcedure
        };

        // Add Parameters
        AddSqlParameter(command, "TopicID", topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

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
    ///   appending to the attribute 'blob' unless <c>skipBlob == true</c>.
    /// </summary>
    /// <param name="topic">The topic object whose relationships should be persisted.</param>
    /// <param name="connection">The SQL connection.</param>
    /// <param name="skipBlob">
    ///   Boolean indicator noting whether attributes saved in the blob should be skipped as part of the operation.
    /// </param>
    /// <returns>
    ///   An XML-formatted string representing the <see cref="Topic.Relationships"/> blob content, or a blank string if
    ///   <c>skipBlob == true</c>.
    /// </returns>
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string PersistRelations(Topic topic, SqlConnection connection, bool skipBlob) {

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
      var command = (SqlCommand?)null;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (var key in topic.Relationships.Keys) {

          var scope             = topic.Relationships.GetTopics(key);
          var topicId           = topic.Id.ToString(CultureInfo.InvariantCulture);
          var targetIds         = scope.Select<Topic, int>(m => m.Id).ToArray();

          command               = new SqlCommand("topics_PersistRelations", connection) {
            CommandType         = CommandType.StoredProcedure
          };

          // Add Parameters
          AddSqlParameter(command, "RelationshipTypeID", key, SqlDbType.VarChar);
          AddSqlParameter(command, "Source_TopicID", topicId, SqlDbType.Int);
          AddSqlParameter(command, "Target_TopicIDs", String.Join(",", targetIds), SqlDbType.VarChar);

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
        //Since the SQL connection is being passed in, do not close connection; this allows command pooling.
        //if (connection != null) connection.Dispose();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the relationship attributes to append to the XML blob (unless skipBlob is set to true)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (skipBlob) return "";
      else return CreateRelationshipsBlob(topic);

    }

    /*==========================================================================================================================
    | METHOD: CREATE RELATIONSHIPS BLOB
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Internal helper function to build string of related XML nodes for each scope of related items in model.
    /// </summary>
    /// <param name="topic">The topic object for which to create the relationships.</param>
    /// <returns>The blob string.</returns>
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string CreateRelationshipsBlob(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, "The topic must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Create blob string container
      \-----------------------------------------------------------------------------------------------------------------------*/
      var blob = new StringBuilder("");

      /*------------------------------------------------------------------------------------------------------------------------
      | Add a related XML node for each scope
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var key in topic.Relationships.Keys) {
        var scope = topic.Relationships.GetTopics(key);
        blob.Append("<related scope=\"");
        blob.Append(key);
        blob.Append("\">");

        // Build out string array of related items in this scope
        var targetIds = new string[scope.Count()];
        var count = 0;
        foreach (var relTopic in scope) {
          targetIds[count] = relTopic.Id.ToString(CultureInfo.InvariantCulture);
          count++;
        }
        blob.Append(String.Join(",", targetIds));
        blob.Append("</related>");
      }

      return blob.ToString();
    }

    /*==========================================================================================================================
    | METHOD: ADD SQL PARAMETER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Wrapper function that adds a SQL parameter to a command object.
    /// </summary>
    /// <param name="commandObject">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    private static void AddSqlParameter(
      SqlCommand commandObject,
      string sqlParameter,
      string fieldValue,
      SqlDbType sqlDbType
    ) => AddSqlParameter(commandObject, sqlParameter, fieldValue, sqlDbType, ParameterDirection.Input, -1);

    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified parameter direction.
    /// </summary>
    /// <param name="commandObject">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    private static void AddSqlParameter(
      SqlCommand commandObject,
      string sqlParameter,
      ParameterDirection paramDirection,
      SqlDbType sqlDbType
    ) => AddSqlParameter(commandObject, sqlParameter, null, sqlDbType, paramDirection, -1);

    /// <summary>
    ///   Adds a SQL parameter to a command object, additionally setting the specified SQL data length for the field.
    /// </summary>
    /// <param name="commandObject">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="fieldValue">The SQL field value.</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <param name="sqlLength">Length limit for the SQL field.</param>
    /// <requires description="The SQL command object must be specified." exception="T:System.ArgumentNullException">
    ///   commandObject != null
    /// </requires>
    private static void AddSqlParameter(
      SqlCommand commandObject,
      string sqlParameter,
      string? fieldValue,
      SqlDbType sqlDbType,
      ParameterDirection paramDirection,
      int sqlLength
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(commandObject, "The SQL command object must be specified.");
      Contract.Requires(commandObject.Parameters, "The SQL command object's parameters collection must be available");

      /*------------------------------------------------------------------------------------------------------------------------
      | Define primary assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sqlLength > 0) {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType, sqlLength));
      }
      else {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType));
      }

      Contract.Assume<InvalidOperationException>(
        commandObject.Parameters["@" + sqlParameter] != null,
        $"The {commandObject.CommandText} stored procedure does not contain a parameter named {sqlParameter}."
      );

      commandObject.Parameters["@" + sqlParameter].Direction = paramDirection;

      if (paramDirection != ParameterDirection.Output & paramDirection != ParameterDirection.ReturnValue) {
        if (fieldValue is null ||fieldValue.Length.Equals(0)) {
          commandObject.Parameters["@" + sqlParameter].Value = null;
        }
        else if (sqlDbType == SqlDbType.Int || sqlDbType == SqlDbType.BigInt || sqlDbType == SqlDbType.TinyInt || sqlDbType == SqlDbType.SmallInt) {
          commandObject.Parameters["@" + sqlParameter].Value = Int64.Parse(fieldValue, CultureInfo.InvariantCulture);
        }
        else if (sqlDbType == SqlDbType.UniqueIdentifier) {
          commandObject.Parameters["@" + sqlParameter].Value = new Guid(fieldValue);
        }
        else if (sqlDbType == SqlDbType.Bit) {
          if (fieldValue == "1" || fieldValue.ToUpperInvariant() == "TRUE") {
            commandObject.Parameters["@" + sqlParameter].Value = true;
          }
          else {
            commandObject.Parameters["@" + sqlParameter].Value = false;
          }
        }
        else {
          commandObject.Parameters["@" + sqlParameter].Value = fieldValue;
        }
      }

    }

  } //Class
} //Namespace