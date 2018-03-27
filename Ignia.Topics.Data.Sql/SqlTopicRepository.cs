/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ignia.Topics.Repositories;
using System.Diagnostics.Contracts;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.Web;
using System.Globalization;
using Ignia.Topics.Collections;
using System.Diagnostics;
using Ignia.Topics.Querying;

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
    private                     ContentTypeDescriptorCollection _contentTypeDescriptors         = null;
    private readonly            string                          _connectionString               = null;

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
    private static void AddTopic(SqlDataReader reader, Dictionary<int, Topic> topics, out int sortOrder) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var parentId              = -1;
      var id                    = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      var contentType           = reader?["ContentType"]?.ToString();
      var key                   = reader?["TopicKey"]?.ToString();

      sortOrder                 = Int32.Parse(reader?["SortOrder"]?.ToString(), CultureInfo.InvariantCulture);

      // Handle ParentID (could be null for root topic)
      Int32.TryParse(reader?["ParentID"]?.ToString(), out parentId);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current = TopicFactory.Create(key, contentType, id);
      topics.Add(current.Id, current);

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign sort order, based on database order
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (current.SortOrder < 0) {
        current.SortOrder = sortOrder++;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parentId >= 0 && topics.Keys.Contains(parentId)) {
        current.Attributes.SetValue("ParentID", parentId.ToString(), false);
        current.Parent = topics[parentId];
      }

    }

    /*==========================================================================================================================
    | METHOD: SET INDEXED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private static void SetIndexedAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      var name                  = reader?["AttributeKey"]?.ToString();
      var value                 = reader?["AttributeValue"]?.ToString();

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
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var id                    = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);

      /*------------------------------------------------------------------------------------------------------------------------
      | Load blob into XmlDocument
      \-----------------------------------------------------------------------------------------------------------------------*/
      var blob = new XmlDocument();
      blob.LoadXml((string)reader?["Blob"]);

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify the current topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[id];

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through nodes to set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (XmlNode attribute in blob.DocumentElement.GetElementsByTagName("attribute")) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Identify attributes
        \---------------------------------------------------------------------------------------------------------------------*/
        var name                = attribute.Attributes?["key"]?.Value;
        var value               = HttpUtility.HtmlDecode(attribute.InnerXml);

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate assumptions
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(name != null, "Assumes the blob AttributeValue collection is available and has a 'key' attribute.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Set attribute value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (String.IsNullOrEmpty(value)) continue;
        current.Attributes.SetValue(name, value, false);

      }
    }

    /*==========================================================================================================================
    | METHOD: SET RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds relationships retrieved from an individual relationship record to their associated topics.
    /// </summary>
    /// <remarks>
    ///   Topics can be cross-referenced with each other via a many-to-many relationships.Once the topics are populated in
    ///   memory, loop through the data to create these associations.
    /// </remarks>
    /// <param name="reader">The <see cref="System.Data.SqlClient.SqlDataReader"/> that representing the current record.</param>
    /// <param name="topics">The index of topics currently being loaded.</param>
    private static void SetRelationships(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = Int32.Parse(reader?["Source_TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      var targetTopicId         = Int32.Parse(reader?["Target_TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      var relationshipTypeId    = (string)reader?["RelationshipTypeID"];

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];
      Topic related             = null;

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
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in topics.Values) {
        var success = Int32.TryParse(topic.Attributes.GetValue("TopicId", "-1", false, false), out var derivedTopicId);
        if (!success || derivedTopicId < 0) continue;
        if (topics.Keys.Contains(derivedTopicId)) {
          topic.DerivedTopic = topics[derivedTopicId];
        }

      }

    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of Content Type Descriptor objects from the configuration section of the data provider.
    /// </summary>
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize content types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypeDescriptors == null) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Load configuration data
        \---------------------------------------------------------------------------------------------------------------------*/
        var configuration = Load("Configuration");

        /*--------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \-------------------------------------------------------------------------------------------------------------------*/
        _contentTypeDescriptors = new ContentTypeDescriptorCollection();

        /*--------------------------------------------------------------------------------------------------------------------
        | Ensure the parent ContentTypes topic is available to iterate over
        \-------------------------------------------------------------------------------------------------------------------*/
        if (configuration.Children.GetTopic("ContentTypes") == null) {
          throw new Exception("Unable to load section Configuration:ContentTypes.");
        }

        /*--------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \-------------------------------------------------------------------------------------------------------------------*/
        foreach (var topic in configuration.Children.GetTopic("ContentTypes").FindAllByAttribute("ContentType", "ContentType")) {
          // Ensure the Topic is used as the strongly-typed ContentType
          // Add ContentType Topic to collection if not already added
          if (
            topic is ContentTypeDescriptor contentTypeDescriptor &&
            !_contentTypeDescriptors.Contains(contentTypeDescriptor.Key)
          ) {
            _contentTypeDescriptors.Add(contentTypeDescriptor);
          }
        }

      }

      return _contentTypeDescriptors;

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
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

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
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified topic key.
    /// </summary>
    /// <param name="topicKey">The topic key.</param>
    /// <param name="isRecursive">Determines whether or not to recurse through and load a topic's children.</param>
    /// <returns>A topic object.</returns>
    public override Topic Load(string topicKey = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty topic
      >-------------------------------------------------------------------------------------------------------------------------
      | If the topicKey is null, or does not contain a topic key, then assume the caller wants to return all data; in that case
      | call Load() with the special integer value of -1, which will load all topics from the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrWhiteSpace(topicKey)) {
        return Load(-1, isRecursive);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection = new SqlConnection(_connectionString);
      var command = new SqlCommand("topics_GetTopicID", connection);
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
        Contract.Assume(command.Parameters != null, "Assumes the command object parameters collection is available.");
        Contract.Assume(command.Parameters["@ReturnCode"] != null, "Assumes the return code parameter is available on query.");
        topicId = Int32.Parse(command.Parameters["@ReturnCode"].Value.ToString(), CultureInfo.InvariantCulture);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Topic(s) failed to load: " + ex.Message);
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
    ///   Loads a topic (and, optionally, all of its descendents) based on the specified unique identifier.
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
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("topics_GetTopics", connection);
      var sortOrder             = 0;

      command.CommandType       = CommandType.StoredProcedure;
      command.CommandTimeout    = 120;
      SqlDataReader reader      = null;

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
          AddTopic(reader, topics, out sortOrder);
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
      catch (Exception ex) {
        throw new Exception("Topics failed to load: " + ex.Message);
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
      if (topics.Count == 0) return new Topic();
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
      | Validate contracts
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topics                = new Dictionary<int, Topic>();
      var connection            = new SqlConnection(_connectionString);
      var command               = new SqlCommand("topics_GetVersion", connection);
      var sortOrder             = 0;

      command.CommandType       = CommandType.StoredProcedure;
      command.CommandTimeout    = 120;
      SqlDataReader reader      = null;

      command.CommandType = CommandType.StoredProcedure;

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
          AddTopic(reader, topics, out sortOrder);
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
      catch (Exception ex) {
        throw new Exception("Topics failed to load: " + ex.Message);
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
      if (topics.Count == 0) return null;
      return topics[topics.Keys.ElementAt(0)];

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
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentTypes = GetContentTypeDescriptors();
      if (!contentTypes.Contains(topic.Attributes.GetValue("ContentType"))) {
        throw new Exception(
          "The Content Type \"" + topic.Attributes.GetValue("ContentType", "Page") + "\" referenced by \"" + topic.Key +
          "\" could not be found. under \"Configuration:ContentTypes\". There are " + contentTypes.Count +
          " ContentTypes in the Repository."
        );
      }
      var contentType = contentTypes[topic.Attributes.GetValue("ContentType", "Page")];

      /*------------------------------------------------------------------------------------------------------------------------
      | Update content types collection, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is ContentTypeDescriptor && !contentTypes.Contains(topic.Key)) {
        _contentTypeDescriptors.Add(topic as ContentTypeDescriptor);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish attribute strings
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Strings are immutable, use a stringbuilder to save memory
      var attributes            = new StringBuilder();
      var nullAttributes        = new StringBuilder();
      var blob                  = new StringBuilder();

      Contract.Assume(contentType != null, "Assumes the Content Type is available.");

      blob.Append("<attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the attributes, adding the names and values to the string builder
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Process attributes not stored in the Blob
      foreach (var attributeValue in topic.Attributes) {

        var key = attributeValue.Key;
        var attribute = (AttributeDescriptor)null;

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
        var isPrimaryAttribute  = (attribute.Key.Equals("Key") || attribute.Key.Equals("ContentType") || attribute.Key.Equals("ParentID"));
        var isRelationships     = (attribute.Type.Equals("Relationships.ascx"));
        var isNestedTopic       = (attribute.Type.Equals("TopicList.ascx"));
        var conditionsMet       = (!topicHasAttribute && !isPrimaryAttribute && !attribute.StoreInBlob && !isRelationships && !isNestedTopic && topic.Id != -1);

        if (conditionsMet) {
          nullAttributes.Append(attribute.Key + ",");
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection = new SqlConnection(_connectionString);
      SqlCommand command = null;
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
          AddSqlParameter(command,      "TopicID",              topic.Id.ToString(CultureInfo.InvariantCulture),  SqlDbType.Int);
        }
        if (topic.Parent != null) {
          AddSqlParameter(command,      "ParentID",             topic.Parent.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        AddSqlParameter(command,        "Version",              version.ToString("yyyy-MM-dd HH:mm:ss.fff"),      SqlDbType.DateTime);
        AddSqlParameter(command,        "Attributes",           attributes.ToString(),                            SqlDbType.VarChar);
        if (topic.Id != -1) {
          AddSqlParameter(command,      "NullAttributes",       nullAttributes.ToString(),                        SqlDbType.VarChar);
          AddSqlParameter(command,      "DeleteRelationships",  "1", SqlDbType.Bit);
        }
        AddSqlParameter(command,        "Blob",                 blob.ToString(),                                  SqlDbType.Xml);
        AddSqlParameter(command,        "ReturnCode",           ParameterDirection.ReturnValue,                   SqlDbType.Int);

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(command.Parameters != null, "Assumes the command object parameters collection is available.");
        Contract.Assume(command.Parameters["@ReturnCode"] != null, "Assumes the return code parameter is available on query.");
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
      | Catch excewption
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Failed to save Topic " + topic.Key + " (" + topic.Id + ") via " + _connectionString + ": " + ex.Message);
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
          Contract.Assume(childTopic.Attributes.GetValue("ParentID") != null, "Assumes the Parent ID AttributeValue is available.");
          childTopic.Attributes.SetValue("ParentID", returnVal.ToString());
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
    public override void Move(Topic topic, Topic target, Topic sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | Move in database
      \-----------------------------------------------------------------------------------------------------------------------*/
      var connection = new SqlConnection(_connectionString);
      SqlCommand command = null;

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
      catch (Exception ex) {
        throw new Exception("Failed to move Topic " + topic.Key + " (" + topic.Id + ") to " + target.Key + " (" + target.Id + "): " + ex.Message);
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
      topic.Attributes.SetValue("ParentId", target.Id.ToString(), false);

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
    /// <requires description="The topic to delete must be provided." exception="T:System.ArgumentNullException">topic != null</requires>
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
      SqlCommand command = null;

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
      catch (Exception ex) {
        throw new Exception("Failed to delete Topic " + topic.Key + " (" + topic.Id + "): " + ex.Message);
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
    ///   appending to the attribute 'blob'.
    /// </summary>
    /// <param name="topic">The topic object whose relationships should be persisted.</param>
    /// <param name="connection">The SQL connection.</param>
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string PersistRelations(Topic topic, SqlConnection connection) {
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must not be null.");
      return PersistRelations(topic, connection, false);
    }

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
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Return blank if the topic has no relations.
      \-----------------------------------------------------------------------------------------------------------------------*/
      // return "" if the topic has no relations
      if (topic.Relationships.Keys.Count() <= 0) {
        return "";
      }
      SqlCommand command = null;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (var key in topic.Relationships.Keys) {

          var scope = topic.Relationships.GetTopics(key);

          command = new SqlCommand("topics_PersistRelations", connection) {
            CommandType = CommandType.StoredProcedure
          };

          var targetIds         = new string[scope.Count()];
          var topicId           = topic.Id.ToString(CultureInfo.InvariantCulture);
          var count             = 0;

          foreach (var relTopic in scope) {
            targetIds[count] = relTopic.Id.ToString(CultureInfo.InvariantCulture);
            count++;
          }

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
      catch (Exception ex) {
        throw new Exception("Failed to persist relationships for Topic " + topic.Key + " (" + topic.Id + "): " + ex.Message);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Close connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      finally {
        //if (command != null) command.Dispose();
        //Since the connection string is being passed in, do not close connection.
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
    ///   Internal helper function to build string of related xml nodes for each scope of related items in model.
    /// </summary>
    /// <param name="topic">The topic object for which to create the relationsihps.</param>
    /// <returns>The blob string.</returns>
    /// <requires description="The topic must not be null." exception="T:System.ArgumentNullException">topic != null</requires>
    private static string CreateRelationshipsBlob(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Create blog string container
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
    | FUNCTION: CONV DB TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Converts a string into the appropriate database type object
    /// </summary>
    /// <param name="sqlDbType">The string specified to be converted to the appropriate SQL data type.</param>
    /// <returns>The converted SQL data type.</returns>
    /// <requires description="The sqlDbType must not be null." exception="T:System.ArgumentNullException">
    ///   sqlDbType != null
    /// </requires>
    private static SqlDbType ConvDbType(string sqlDbType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sqlDbType != null, "The sqlDbType must not be null.");

      switch (sqlDbType.ToLower()) {
        case "int"              : return SqlDbType.Int;
        case "tinyint"          : return SqlDbType.TinyInt;
        case "smallint"         : return SqlDbType.SmallInt;
        case "uniqueidentifier" : return SqlDbType.UniqueIdentifier;
        case "bit"              : return SqlDbType.Bit;
        case "char"             : return SqlDbType.Char;
        case "varchar"          : return SqlDbType.VarChar;
        case "datetime"         : return SqlDbType.DateTime;
        default                 : return SqlDbType.VarChar;
      }

    }

    /*==========================================================================================================================
    | METHOD: ADD SQL PARAMETER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Wrapper function that adds a SQL paramter to a command object.
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
    ///   Adds a SQL paramter to a command object, additionally setting the specified parameter direction.
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
    ///   Adds a SQL paramter to a command object, additionally setting the specified SQL data length for the field.
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
      string fieldValue,
      SqlDbType sqlDbType,
      ParameterDirection paramDirection,
      int sqlLength
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(commandObject != null, "The SQL command object must be specified.");
      Contract.Requires(commandObject.Parameters != null, "The SQL command object's parameters collection must be available");

      /*------------------------------------------------------------------------------------------------------------------------
      | Define primary assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sqlLength > 0) {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType, sqlLength));
      }
      else {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType));
      }

      Contract.Assume(commandObject.Parameters["@" + sqlParameter] != null, "Assumes the parameter is valid.");

      commandObject.Parameters["@" + sqlParameter].Direction = paramDirection;

      if (paramDirection != ParameterDirection.Output & paramDirection != ParameterDirection.ReturnValue) {
        if (String.IsNullOrEmpty(fieldValue)) {
          commandObject.Parameters["@" + sqlParameter].Value = null;
        }
        else if (sqlDbType == SqlDbType.Int || sqlDbType == SqlDbType.BigInt || sqlDbType == SqlDbType.TinyInt || sqlDbType == SqlDbType.SmallInt) {
          commandObject.Parameters["@" + sqlParameter].Value = Int64.Parse(fieldValue);
        }
        else if (sqlDbType == SqlDbType.UniqueIdentifier) {
          commandObject.Parameters["@" + sqlParameter].Value = new Guid(fieldValue);
        }
        else if (sqlDbType == SqlDbType.Bit) {
          if (fieldValue == "1" || fieldValue.ToLower() == "true") {
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
