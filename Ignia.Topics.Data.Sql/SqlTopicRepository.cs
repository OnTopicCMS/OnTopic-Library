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
using System.Configuration;
using System.Data;
using System.Xml;
using System.Web;
using System.Globalization;

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
    private             ContentTypeCollection           _contentTypes           = null;

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    private void AddTopic(SqlDataReader reader, Dictionary<int, Topic> topics, out int sortOrder) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int parentId = -1;
      int id = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      string contentType = reader?["ContentType"]?.ToString();
      string key = reader?["TopicKey"]?.ToString();
      sortOrder = Int32.Parse(reader?["SortOrder"]?.ToString(), CultureInfo.InvariantCulture);

      // Handle ParentID (could be null for root topic)
      Int32.TryParse(reader?["ParentID"]?.ToString(), out parentId);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic current = Topic.Create(key, contentType, id);
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
        current.Parent = topics[parentId];
      }

    }

    /*==========================================================================================================================
    | METHOD: SET INDEXED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private void SetIndexedAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int id = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      string name = reader?["AttributeKey"]?.ToString();
      string value = reader?["AttributeValue"]?.ToString();
      DateTime versionDate = Convert.ToDateTime(reader?["Version"]?.ToString(), CultureInfo.InvariantCulture);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty attributes (treat empty as null)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) || DBNull.Value.Equals(value)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic current = topics[id];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!current.Attributes.Contains(name)) {
        current.Attributes.Add(new AttributeValue(name, value, false));
      }

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
    private void SetBlobAttributes(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int id = Int32.Parse(reader?["TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      DateTime versionDate = Convert.ToDateTime(reader?["Version"]?.ToString(), CultureInfo.InvariantCulture);

      /*------------------------------------------------------------------------------------------------------------------------
      | Load blob into XmlDocument
      \-----------------------------------------------------------------------------------------------------------------------*/
      XmlDocument blob = new XmlDocument();
      blob.LoadXml((string)reader?["Blob"]);

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify the current topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic current = topics[id];

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through nodes to set attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (XmlNode attribute in blob.DocumentElement.GetElementsByTagName("attribute")) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Identify attributes
        \---------------------------------------------------------------------------------------------------------------------*/
        string name = attribute.Attributes?["key"]?.Value;
        string value = HttpUtility.HtmlDecode(attribute.InnerXml);

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate assumptions
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(name != null, "Assumes the blob AttributeValue collection is available and has a 'key' attribute.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Set attribute value
        \---------------------------------------------------------------------------------------------------------------------*/
        // Treat empty as null
        if (String.IsNullOrEmpty(value)) continue;

        if (!current.Attributes.Contains(name)) {
          current.Attributes.Add(new AttributeValue(name, value, false));
        }
        else {
          // System.Web.HttpContext.Current.Response.Write("Attribute '" + name + "(" + value + ") already exists. It was not added.");
        }

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
    private void SetRelationships(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int sourceTopicId = Int32.Parse(reader?["Source_TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      int targetTopicId = Int32.Parse(reader?["Target_TopicID"]?.ToString(), CultureInfo.InvariantCulture);
      string relationshipTypeId = (string)reader?["RelationshipTypeID"];

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic current = topics[sourceTopicId];
      Topic related = null;

      // Fetch the related topic
      if (topics.Keys.Contains(targetTopicId)) {
        related = topics[targetTopicId];
      }

      // Bypass if either of the objects are missing
      if (related == null) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship on object
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.SetRelationship(relationshipTypeId, related);

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
    private void SetDerivedTopics(Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (Topic topic in topics.Values) {
        int derivedTopicId = 0;
        bool success = Int32.TryParse(topic.Attributes.Get("TopicId", "-1", false, false), out derivedTopicId);
        if (!success || derivedTopicId < 0) continue;
        if (topics.Keys.Contains(derivedTopicId)) {
          topic.DerivedTopic = topics[derivedTopicId];
        }

      }

    }

    /*==========================================================================================================================
    | GET CONTENT TYPES
    >===========================================================================================================================
    | ###TODO JJC092813: Need to identify a way of handling cache dependencies and/or recycling of ContentTypes based on
    | changes.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the Configuration namespace from the database, and sets the <see cref="ContentType"/> objects from there.
    /// </summary>
    public ContentTypeCollection GetContentTypes() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<ContentTypeCollection>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize content types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypes == null) {

        _contentTypes = new ContentTypeCollection();

        /*----------------------------------------------------------------------------------------------------------------------
        | Load configuration data
        \---------------------------------------------------------------------------------------------------------------------*/
        var configuration = this.Load("Configuration");

        /*--------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \-------------------------------------------------------------------------------------------------------------------*/
        _contentTypes = new ContentTypeCollection();

        /*--------------------------------------------------------------------------------------------------------------------
        | Ensure the parent ContentTypes topic is available to iterate over
        \-------------------------------------------------------------------------------------------------------------------*/
        if (configuration.GetTopic("ContentTypes") == null) {
          throw new Exception("Unable to load section Configuration:ContentTypes.");
        }

        /*--------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \-------------------------------------------------------------------------------------------------------------------*/
        foreach (Topic topic in configuration.GetTopic("ContentTypes").FindAllByAttribute("ContentType", "ContentType")) {
          // Ensure the Topic is used as the strongly-typed ContentType
          ContentType contentType = topic as ContentType;
          // Add ContentType Topic to collection if not already added
          if (contentType != null && !_contentTypes.Contains(contentType.Key)) {
            _contentTypes.Add(contentType);
          }
        }

      }

      return _contentTypes;

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
    private void SetVersionHistory(SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topics != null, "The topics Dictionary must not be null.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      int sourceTopicId = Int32.Parse(reader?["TopicId"]?.ToString(), CultureInfo.InvariantCulture);
      DateTime dateTime = Convert.ToDateTime(reader?["Version"]?.ToString(), CultureInfo.InvariantCulture);

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic current = topics[sourceTopicId];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set history
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.VersionHistory.Add(dateTime);

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    >---------------------------------------------------------------------------------------------------------------------------
    | ### NOTE JJC081115: This method should be broken down into private helper functions to better separate functionality, 
    | avoid lengthy nested blocks, and enhance code readability. 
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that loads topics into memory.
    /// </summary>
    /// <param name="topicKey">The string identifier for the topic.</param>
    /// <param name="topicId">The integer identifier for the topic.</param>
    /// <param name="depth">The level to which to recurse through and load a topic's children.</param>
    /// <param name="version">The DateTime stamp signifying when the topic was saved.</param>
    /// <returns>A topic object.</returns>
    /// <exception cref="Exception">
    ///   The topic Ignia.Topics.<c>contentType</c> does not derive from Ignia.Topics.Topic.
    /// </exception>
    /// <exception cref="Exception">
    ///   Topics failed to load: <c>ex.Message</c>
    /// </exception>
    protected override Topic Load(string topicKey, int topicId, int depth, DateTime? version = null) {

    /*------------------------------------------------------------------------------------------------------------------------
    | Validate contracts
    \-----------------------------------------------------------------------------------------------------------------------*/
    Contract.Ensures(Contract.Result<Topic>() != null);
      ValidateConnectionStrings("TopicsServer");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      Dictionary<int, Topic> topics = new Dictionary<int, Topic>();
      SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings?["TopicsServer"]?.ConnectionString);
      SqlCommand command = new SqlCommand("topics_GetTopics", connection);
      command.CommandType = CommandType.StoredProcedure;
      SqlDataReader reader = null;
      int sortOrder = 0;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Open connection
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        AddSqlParameter(command, "TopicName", topicKey, SqlDbType.VarChar);
        AddSqlParameter(command, "Depth", depth.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        AddSqlParameter(command, "TopicID", topicId.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

        if (version != null) {
          AddSqlParameter(command, "Version", version.ToString(), SqlDbType.DateTime);
        }

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
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to the relationships dataset
        reader.NextResult();

        // Loop through each relationship; multiple records may exist per topic
        while (reader.Read()) {
          SetRelationships(reader, topics);
        }

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
        \---------------------------------------------------------------------------------------------------------------------*/
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
    ///   The Content Type <c>topic.Attributes.Get(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found under 
    ///   Configuration:ContentTypes. There are <c>ContentTypes.Count</c> ContentTypes in cached in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via 
    ///   <c>ConfigurationManager.ConnectionStrings[TopicsServer].ConnectionString</c>: <c>ex.Message</c>
    /// </exception>
    public override int Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentTypes = GetContentTypes();
      if (!contentTypes.Contains(topic.Attributes.Get("ContentType"))) {
        throw new Exception("The Content Type \"" + topic.Attributes.Get("ContentType", "Page") + "\" referenced by \"" + topic.Key + "\" could not be found. under \"Configuration:ContentTypes\". There are " + contentTypes.Count + " ContentTypes in the Repository.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish attribute strings
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Strings are immutable, use a stringbuilder to save memory
      StringBuilder attributes = new StringBuilder();
      StringBuilder nullAttributes = new StringBuilder();
      StringBuilder blob = new StringBuilder();
      ContentType contentType = contentTypes[topic.Attributes.Get("ContentType", "Page")];

      Contract.Assume(contentType != null, "Assumes the Content Type is available.");

      blob.Append("<attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the attributes, adding the names and values to the string builder
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Process attributes not stored in the Blob
      foreach (AttributeValue attributeValue in topic.Attributes) {

        string key = attributeValue.Key;
        Attribute attribute = null;

        if (contentType.SupportedAttributes.Keys.Contains(key)) {
          attribute = contentType.SupportedAttributes[key];
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
      foreach (string attributeKey in contentType.SupportedAttributes.Keys) {

        // Set preconditions
        Attribute attribute = contentType.SupportedAttributes[attributeKey];
        bool topicHasAttribute = (topic.Attributes.Contains(attributeKey) && !String.IsNullOrEmpty(topic.Attributes[attributeKey].Value));
        bool isPrimaryAttribute = (attributeKey == "Key" || attributeKey == "ContentType" || attributeKey == "ParentID");
        bool isRelationships = (contentType.SupportedAttributes[attributeKey].Type == "Relationships.ascx");
        bool isNestedTopic = (contentType.SupportedAttributes[attributeKey].Type == "TopicList.ascx");
        bool conditionsMet = (!topicHasAttribute && !isPrimaryAttribute && !attribute.StoreInBlob && !isRelationships && !isNestedTopic && topic.Id != -1);

        if (conditionsMet) {
          nullAttributes.Append(attributeKey + ",");
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(ConfigurationManager.ConnectionStrings != null, "Assumes the connection strings are available in the configuration.");
      Contract.Assume(ConfigurationManager.ConnectionStrings["TopicsServer"] != null, "Assumes the connection string is available.");
      SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand command = null;
      int returnVal = -1;

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
        DateTime version = DateTime.Now;

        // NOTE: KLT031915: Commented out as Draft functionality is not fully implemented
        // if (isDraft) {
        //   version            = DateTime.MaxValue;
        // }

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          AddSqlParameter(command, "TopicID", topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        if (topic.Parent != null) {
          AddSqlParameter(command, "ParentID", topic.Parent.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        AddSqlParameter(command, "Version", version.ToString("yyyy-MM-dd HH:mm:ss.fff"), SqlDbType.DateTime);
        AddSqlParameter(command, "Attributes", attributes.ToString(), SqlDbType.VarChar);
        if (topic.Id != -1) {
          AddSqlParameter(command, "NullAttributes", nullAttributes.ToString(), SqlDbType.VarChar);
          AddSqlParameter(command, "DeleteRelationships", "1", SqlDbType.Bit);
        }
        AddSqlParameter(command, "Blob", blob.ToString(), SqlDbType.Xml);
        AddSqlParameter(command, "ReturnCode", ParameterDirection.ReturnValue, SqlDbType.Int);

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
        throw new Exception("Failed to save Topic " + topic.Key + " (" + topic.Id + ") via " + ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString + ": " + ex.Message);
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
        foreach (Topic childTopic in topic) {
          Contract.Assume(childTopic.Attributes["ParentID"] != null, "Assumes the Parent ID AttributeValue is available.");
          childTopic.Attributes["ParentID"].Value = returnVal.ToString();
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
      base.Move(topic, target);

      /*------------------------------------------------------------------------------------------------------------------------
      | Move in database
      \-----------------------------------------------------------------------------------------------------------------------*/
      SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand command = null;

      try {

        command = new SqlCommand("topics_MoveTopic", connection);
        command.CommandType = CommandType.StoredProcedure;

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
      | Define assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        ConfigurationManager.ConnectionStrings != null,
        "The Delete method assumes the database connection strings are available from the configuration."
        );

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Delete(topic, isRecursive);

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assert(ConfigurationManager.ConnectionStrings != null, "Assumes the connection strings are available in the configuration.");
      Contract.Assume(ConfigurationManager.ConnectionStrings["TopicsServer"] != null, "Assumes the connection string is available.");
      SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand command = null;

      try {

        command = new SqlCommand("topics_DeleteTopic", connection);
        command.CommandType = CommandType.StoredProcedure;

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
      if (topic.Relationships.Count <= 0) {
        return "";
      }
      SqlCommand command = null;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (Topic scope in topic.Relationships) {

          command = new SqlCommand("topics_PersistRelations", connection);
          command.CommandType = CommandType.StoredProcedure;

          string[] targetIds = new string[scope.Count];
          string topicId = topic.Id.ToString(CultureInfo.InvariantCulture);
          int count = 0;

          foreach (Topic relTopic in scope) {
            targetIds[count] = relTopic.Id.ToString(CultureInfo.InvariantCulture);
            count++;
          }

          // Add Parameters
          AddSqlParameter(command, "RelationshipTypeID", scope.Key, SqlDbType.VarChar);
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
      StringBuilder blob = new StringBuilder("");

      /*------------------------------------------------------------------------------------------------------------------------
      | Add a related XML node for each scope
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (Topic scope in topic.Relationships) {
        blob.Append("<related scope=\"");
        blob.Append(scope.Key);
        blob.Append("\">");

        // Build out string array of related items in this scope
        string[] targetIds = new string[scope.Count];
        int count = 0;
        foreach (Topic relTopic in scope) {
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
        case "int": return SqlDbType.Int;
        case "tinyint": return SqlDbType.TinyInt;
        case "smallint": return SqlDbType.SmallInt;
        case "uniqueidentifier": return SqlDbType.UniqueIdentifier;
        case "bit": return SqlDbType.Bit;
        case "char": return SqlDbType.Char;
        case "varchar": return SqlDbType.VarChar;
        case "datetime": return SqlDbType.DateTime;
      }

      return SqlDbType.VarChar;

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
    private static void AddSqlParameter(SqlCommand commandObject, String sqlParameter, String fieldValue, SqlDbType sqlDbType) {
      AddSqlParameter(commandObject, sqlParameter, fieldValue, sqlDbType, ParameterDirection.Input, -1);
    }

    /// <summary>
    ///   Adds a SQL paramter to a command object, additionally setting the specified parameter direction.
    /// </summary>
    /// <param name="commandObject">The SQL command object.</param>
    /// <param name="sqlParameter">The SQL parameter.</param>
    /// <param name="paramDirection">The SQL parameter's directional setting (input-only, output-only, etc.).</param>
    /// <param name="sqlDbType">The SQL field data type.</param>
    private static void AddSqlParameter(SqlCommand commandObject, String sqlParameter, ParameterDirection paramDirection, SqlDbType sqlDbType) {
      AddSqlParameter(commandObject, sqlParameter, null, sqlDbType, paramDirection, -1);
    }

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
    private static void AddSqlParameter(SqlCommand commandObject, String sqlParameter, String fieldValue, SqlDbType sqlDbType, ParameterDirection paramDirection, int sqlLength) {

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
        if (fieldValue == null || fieldValue == "") {
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

    /*==========================================================================================================================
    | METHOD: VALIDATE CONNECTION STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Confirms that the SQL connection as defined in the application configuration is available/valid.
    /// </summary>
    /// <param name="connectionString">The expected name of the SQL connection to validate in the configuration.</param>
    [Pure]
    public static void ValidateConnectionStrings(string connectionString) {

      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(connectionString),
        "The name of the connection string must be provided in order to be validated."
      );
      Contract.Assert(
        ConfigurationManager.ConnectionStrings != null,
        "Confirms the connection strings are available from the configuration."
      );

      if (ConfigurationManager.ConnectionStrings?[connectionString] == null) {
        throw new ArgumentException(
          "Required connection string '" + connectionString + "' is missing from the web.config's <connectionStrings /> element."
        );
      }

    }

  } //Class

} //Namespace
