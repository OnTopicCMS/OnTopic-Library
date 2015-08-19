/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Xml;
using System.Diagnostics.Contracts;

namespace Ignia.Topics.Providers {

  /*============================================================================================================================
  | CLASS: SQL TOPIC DATA PROVIDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Implementation of the topic provider specific to SQL.
  /// </summary>
  public class SqlTopicDataProvider : TopicDataProviderBase {

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
    /// <requires description="Either the topicKey or the topicId are required." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(topicKey) || topicId > 0
    /// </requires>
    /// <exception cref="Exception">
    ///   The topic Ignia.Topics.<c>contentType</c> does not derive from Ignia.Topics.Topic.
    /// </exception>
    /// <exception cref="Exception">
    ///   Topics failed to load: <c>ex.Message</c>
    /// </exception>
    public override Topic Load(string topicKey, int topicId, int depth, DateTime? version = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(topicKey) || topicId > 0, 
        "Either the topicKey or the topicId are required."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() == null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Define assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        ConfigurationManager.ConnectionStrings != null,
        "The Load method assumes the connection strings are available from the configuration."
        );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      Dictionary<int, Topic>    topics          = new Dictionary<int, Topic>();
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = new SqlCommand("topics_GetTopics", connection);
      command.CommandType                       = CommandType.StoredProcedure;
      SqlDataReader             reader          = null;
      int                       sortOrder       = 0;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Open connection
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        AddSqlParameter(command, "TopicName",   topicKey,                                       SqlDbType.VarChar);
        AddSqlParameter(command, "Depth",       depth.ToString(CultureInfo.InvariantCulture),   SqlDbType.Int);
        AddSqlParameter(command, "TopicID",     topicId.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

        if (version != null) {
          AddSqlParameter(command, "Version",   version.ToString(),                             SqlDbType.DateTime);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query/reader
        \---------------------------------------------------------------------------------------------------------------------*/
        reader                                  = command.ExecuteReader();

        /*----------------------------------------------------------------------------------------------------------------------
        | Populate topics
        \---------------------------------------------------------------------------------------------------------------------*/
        while (reader.Read()) {

          // Identify attribute values
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                contentType     = reader["ContentType"].ToString();
          string                key             = reader["TopicKey"].ToString();
                                sortOrder       = Int32.Parse(reader["SortOrder"].ToString(), CultureInfo.InvariantCulture);
          int                   parentId        = -1;

          // Handle ParentID (could be null for root topic)
          Int32.TryParse(reader["ParentID"].ToString(), out parentId);

          dynamic current = Topic.Create(key, contentType);

          // Create new topic, if topic doesn't exist
          if (!topics.Keys.Contains(id)) {
            current.Id          = id;
            topics.Add(current.Id, current);
          }

          // Reference existing topic, if topic exists
          else {
            current             = topics[id];
          }

          // Assign sort order, based on database order
          if (current.SortOrder < 0) {
            current.SortOrder   = sortOrder++;
          }

          // Set Content Type
          if (!current.Attributes.Contains("ContentType")) {
            current.Attributes.Add(new AttributeValue("ContentType", contentType, false));
          }

          // Provide special handling for ParentId
          if (parentId == -1) {
            continue;
          }

          if (topics.Keys.Contains(parentId)) {
            current.Parent      = topics[parentId];
          }

          // Add Key, ContentType, and ParentID to Attributes (AttributesCollection) if not available
          // to ensure Attributes is populated
          if (!current.Attributes.Contains("Key")) {
            current.Attributes.Add(new AttributeValue("Key", key, false));
          }
          if (!current.Attributes.Contains("ContentType")) {
            current.Attributes.Add(new AttributeValue("ContentType", contentType, false));
          }
          if (!current.Attributes.Contains("ParentID")) {
            current.Attributes.Add(new AttributeValue("ParentID", parentId.ToString(), false));
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read attributes
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to TopicAttributes dataset
        reader.NextResult();

        while (reader.Read()) {

          // Identify attribute values
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                name            = reader["AttributeKey"].ToString();
          string                value           = reader["AttributeValue"].ToString();
          DateTime              versionDate     = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          Topic                 current         = topics[id];

          // Treat empty as null
          if (String.IsNullOrEmpty(value) || DBNull.Value.Equals(value)) continue;

          // Set attribute value
          if (!current.Attributes.Contains(name)) {
            current.Attributes.Add(new AttributeValue(name, value, false));
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read blob
        >-----------------------------------------------------------------------------------------------------------------------
        | Values of arbitrary length are stored in an XML blob. This makes them more efficient to store, but more difficult to
        | query; as such, it's ideal for content-oriented data. The blob values are returned as a separate data set.
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to blob dataset
        reader.NextResult();

        // Loop through each blob, each record associated with a specific record
        while (reader.Read()) {

          // Identify variables
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          DateTime              versionDate     = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          XmlDocument           blob            = new XmlDocument();

          // Load the blob into an XmlDocument object
          blob.LoadXml((string)reader["Blob"]);

          // This scenario should never occur.
          if (!topics.Keys.Contains(id)) continue;

          // Identify the current topic
          Topic                 current         = topics[id];

          // Loop through each node in the blob and associate with the current topic
          foreach (XmlNode attribute in blob.DocumentElement.GetElementsByTagName("attribute")) {
            string              name            = attribute.Attributes["key"].Value;
            string              value           = System.Web.HttpContext.Current.Server.HtmlDecode(attribute.InnerXml);

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

        /*----------------------------------------------------------------------------------------------------------------------
        | Read related items
        >-----------------------------------------------------------------------------------------------------------------------
        | Topics can be cross-referenced with each other via a many-to-many relationships. Once the topics are populated in
        | memory, loop through the data to create these associations.
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to the relationships dataset
        reader.NextResult();

        // Loop through each relationship; multiple records may exist per topic
        while (reader.Read()) {

          // Identify variables
          int                   sourceTopicId           = Int32.Parse(reader["Source_TopicID"].ToString(), CultureInfo.InvariantCulture);
          int                   targetTopicId           = Int32.Parse(reader["Target_TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                relationshipTypeId      = (string)reader["RelationshipTypeID"];
          Topic                 related                 = null;
          Topic                 current                 = null;

          // Fetch the source topic
          if (topics.Keys.Contains(sourceTopicId)) {
            current             = topics[sourceTopicId];
          }
          else {
            current             = TopicRepository.RootTopic.GetTopic(sourceTopicId);
          }

          // Fetch the related topic
          if (topics.Keys.Contains(targetTopicId)) {
            related             = topics[targetTopicId];
          }

          // Bypass if either of the objects are missing
          if (current == null || related == null) continue;

          // Set relationships on object
          current.SetRelationship(relationshipTypeId, related);

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Read version history
        >-----------------------------------------------------------------------------------------------------------------------
        | Every time a value changes for an attribute, a new version is created, represented by the date of the change. This
        | version history is aggregated per topic to allow topic information to be rolled back to a specific date. While version
        | content is not exposed directly via the Load() method, the metadata is.
        \---------------------------------------------------------------------------------------------------------------------*/

        // Move to the version history dataset
        reader.NextResult();

        // Loop through each version; multiple records may exist per topic
        while (reader.Read()) {

          // Identify variables
          int                   sourceTopicId           = Int32.Parse(reader["TopicId"].ToString(), CultureInfo.InvariantCulture);
          DateTime              dateTime                = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          Topic                 current                 = null;

          // Fetch the target topic
          if (topics.Keys.Contains(sourceTopicId)) {
            current                                     = topics[sourceTopicId];
          }

          // Set history
          if (!current.VersionHistory.Contains(dateTime)) {
            current.VersionHistory.Add(dateTime);
          }

        }

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
        if (reader != null) reader.Dispose();
        command.Dispose();
        connection.Dispose();
        connection.Close();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topics.Count == 0) return null;
      return topics[topics.Keys.ElementAt(0)];

    }

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
    /// <requires description="The topic to be saved must not be null." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    /// <exception cref="ArgumentNullException">topic</exception>
    /// <exception cref="Exception">
    ///   The Content Type <c>topic.GetAttribute(ContentType, Page)</c> referenced by <c>topic.Key</c> could not be found under 
    ///   Configuration:ContentTypes. There are <c>TopicRepository.ContentTypes.Count</c> ContentTypes in the Repository.
    /// </exception>
    /// <exception cref="Exception">
    ///   Failed to save Topic <c>topic.Key</c> (<c>topic.Id</c>) via 
    ///   <c>ConfigurationManager.ConnectionStrings[TopicsServer].ConnectionString</c>: <c>ex.Message</c>
    /// </exception>
    public override int Save(Topic topic, bool isRecursive, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");

      /*------------------------------------------------------------------------------------------------------------------------
      | Call base method - will trigger any events associated with the save
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!TopicRepository.ContentTypes.Contains(topic.GetAttribute("ContentType"))) {
        throw new Exception("The Content Type \"" + topic.GetAttribute("ContentType", "Page") + "\" referenced by \"" + topic.Key + "\" could not be found. under \"Configuration:ContentTypes\". There are " + TopicRepository.ContentTypes.Count + " ContentTypes in the Repository.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish attribute strings
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Strings are immutable, use a stringbuilder to save memory
      StringBuilder             attributes      = new StringBuilder();
      StringBuilder             nullAttributes  = new StringBuilder();
      StringBuilder             blob            = new StringBuilder();
      ContentType               contentType     = TopicRepository.ContentTypes[topic.GetAttribute("ContentType", "Page")];

      blob.Append("<attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the attributes, adding the names and values to the string builder
      \-----------------------------------------------------------------------------------------------------------------------*/
      // Process attributes not stored in the Blob
      foreach (AttributeValue attributeValue in topic.Attributes) {

        string                  key             = attributeValue.Key;
        Attribute               attribute       = null;

        if (contentType.SupportedAttributes.Keys.Contains(key)) {
          attribute                             = contentType.SupportedAttributes[key];
        }

        // For attributes not stored in the Blob, only add the AttributeValue item to store if it has changed
        if (attribute != null && !attribute.StoreInBlob && attributeValue.IsDirty) {
          attributes.Append(key + "~~" + topic.Attributes[key].Value + "``");
        }
        else if (attribute != null && attribute.StoreInBlob) {
          blob.Append("<attribute key=\"" + key + "\"><![CDATA[" + topic.Attributes[key].Value + "]]></attribute>");
        }

        // Reset IsDirty (changed) state
        attributeValue.IsDirty                  = false;

      }

      blob.Append("</attributes>");

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (string attributeKey in contentType.SupportedAttributes.Keys) {

        // Set preconditions
        Attribute attribute     = contentType.SupportedAttributes[attributeKey];
        bool topicHasAttribute  = (topic.Attributes.Contains(attributeKey) && topic.Attributes[attributeKey].Value != null);
        bool isPrimaryAttribute = (attributeKey == "Key" || attributeKey == "ContentType" || attributeKey == "ParentID");
        bool isRelationships    = (contentType.SupportedAttributes[attributeKey].Type == "Relationships.ascx");
        bool isNestedTopic      = (contentType.SupportedAttributes[attributeKey].Type == "TopicList.ascx");
        bool conditionsMet      = (!topicHasAttribute && !isPrimaryAttribute && !attribute.StoreInBlob && !isRelationships && !isNestedTopic && topic.Id != -1);

        if (conditionsMet) {
          nullAttributes.Append(attributeKey + ",");
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;
      int                       returnVal       = -1;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Update relations
        \---------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish command type (insert or update)
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          command               = new SqlCommand("topics_UpdateTopic", connection);
        }
        else {
          command               = new SqlCommand("topics_CreateTopic", connection);
        }

        command.CommandType     = CommandType.StoredProcedure;

        /*----------------------------------------------------------------------------------------------------------------------
        | SET VERSION DATETIME
        \---------------------------------------------------------------------------------------------------------------------*/
        DateTime version        = DateTime.Now;

        // NOTE: KLT031915: Commented out as Draft functionality is not fully implemented
        // if (isDraft) {
        //   version            = DateTime.MaxValue;
        // }

        /*----------------------------------------------------------------------------------------------------------------------
        | Establish query parameters
        \---------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          AddSqlParameter(command,      "TopicID",              topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        if (topic.Parent != null) {
          AddSqlParameter(command,      "ParentID",             topic.Parent.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        AddSqlParameter(command,        "Version",              version.ToString("yyyy-MM-dd HH:mm:ss.fff"), SqlDbType.DateTime);
        AddSqlParameter(command,        "Attributes",           attributes.ToString(),                  SqlDbType.VarChar);
        if (topic.Id != -1) {
          AddSqlParameter(command,      "NullAttributes",       nullAttributes.ToString(),              SqlDbType.VarChar);
          AddSqlParameter(command,      "DeleteRelationships",  "1",                                    SqlDbType.Bit);
        }
        AddSqlParameter(command,        "Blob",                 blob.ToString(),                        SqlDbType.Xml);
        AddSqlParameter(command,        "ReturnCode",           ParameterDirection.ReturnValue,         SqlDbType.Int);

        /*----------------------------------------------------------------------------------------------------------------------
        | Execute query
        \---------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

        /*----------------------------------------------------------------------------------------------------------------------
        | Process return value
        \---------------------------------------------------------------------------------------------------------------------*/
        returnVal               = Int32.Parse(command.Parameters["@ReturnCode"].Value.ToString(), CultureInfo.InvariantCulture);
        topic.Id                = returnVal;

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
          childTopic.Attributes["ParentID"].Value = returnVal.ToString();
          childTopic.Save(isRecursive, isDraft);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return returnVal;

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that moves the specified topic within the tree.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">The target (parent) topic object under which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    public override bool Move(Topic topic, Topic target) {
      return this.Move(topic, target, null);
    }

    /// <summary>
    ///   Interface method that moves the specified topic within the tree, usng the specified sibling topic object as a
    ///   secondary target for the move.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">The target (parent) topic object under which the topic should be moved.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    /// <requires description="The topic to be moved must not be null." exception="T:System.ArgumentNullException">
    ///   topic != null
    /// </requires>
    /// <requires description="The target parent topic must not be null." exception="T:System.ArgumentNullException">
    ///   target != null
    /// </requires>
    /// <requires description="The topic to be moved cannot be its own parent." exception="T:System.ArgumentException">
    ///   topic != target
    /// </requires>
    /// <requires description="The topic cannot be moved relative to itself." exception="T:System.ArgumentException">
    ///   topic != sibling
    /// </requires>
    public override bool Move(Topic topic, Topic target, Topic sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "topic");
      Contract.Requires<ArgumentNullException>(target != null, "target");
      Contract.Requires<ArgumentException>(topic != target, "A topic cannot be its own parent.");
      Contract.Requires<ArgumentException>(topic != sibling, "A topic cannot be moved relative to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from memory
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target);

      /*------------------------------------------------------------------------------------------------------------------------
      | Move in database
      \-----------------------------------------------------------------------------------------------------------------------*/
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;

      try {

        command                                 = new SqlCommand("topics_MoveTopic", connection);
        command.CommandType                     = CommandType.StoredProcedure;

        // Add Parameters
        AddSqlParameter(command, "TopicID",  topic.Id.ToString(CultureInfo.InvariantCulture),  SqlDbType.Int);
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

      return true;
    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Interface method that deletes the provided topic from the tree.
    /// </summary>
    /// <param name="topic">The topic object to be deleted.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and delete them as well.
    /// </param>
    /// <exception cref="ArgumentNullException">topic</exception>
    /// <exception cref="Exception">Failed to delete Topic <c>topic.Key</c> (<c>topic.Id</c>): <c>ex.Message</c></exception>
    public override void Delete(Topic topic, bool isRecursive) {

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
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;

      try {

        command                                 = new SqlCommand("topics_DeleteTopic", connection);
        command.CommandType                     = CommandType.StoredProcedure;

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

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
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
      SqlCommand                command         = null;

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (Topic scope in topic.Relationships) {

          command                               = new SqlCommand("topics_PersistRelations", connection);
          command.CommandType                   = CommandType.StoredProcedure;

          string[]      targetIds               = new string[topic.Relationships[scope.Key].Count];
          string        topicId                 = topic.Id.ToString(CultureInfo.InvariantCulture);
          int           count                   = 0;

          foreach (Topic relTopic in topic.Relationships[scope.Key]) {
            targetIds[count] = relTopic.Id.ToString(CultureInfo.InvariantCulture);
            count++;
          }

          // Add Parameters
          AddSqlParameter(command, "RelationshipTypeID",  scope.Key,                            SqlDbType.VarChar);
          AddSqlParameter(command, "Source_TopicID",      topicId,                              SqlDbType.Int);
          AddSqlParameter(command, "Target_TopicIDs",     String.Join(",", targetIds),          SqlDbType.VarChar);

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
        string[] targetIds = new string[topic.Relationships[scope.Key].Count];
        int count = 0;
        foreach (Topic relTopic in topic.Relationships[scope.Key]) {
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
    /// <requires
    ///   description="The SQL data type identifier must not be null." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(sqlDbType)
    /// </requires>
    private static SqlDbType ConvDbType (string sqlDbType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(sqlDbType),
        "The SQL data type identifier must be specified."
      );

      switch (sqlDbType.ToLower()) {
        case "int":               return SqlDbType.Int;
        case "tinyint":           return SqlDbType.TinyInt;
        case "smallint":          return SqlDbType.SmallInt;
        case "uniqueidentifier":  return SqlDbType.UniqueIdentifier;
        case "bit":               return SqlDbType.Bit;
        case "char":              return SqlDbType.Char;
        case "varchar":           return SqlDbType.VarChar;
        case "datetime":          return SqlDbType.DateTime;
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
    private static void AddSqlParameter(SqlCommand commandObject, String sqlParameter, String fieldValue, SqlDbType sqlDbType, ParameterDirection paramDirection, int sqlLength) {

      if (sqlLength > 0) {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType, sqlLength));
      }
      else {
        commandObject.Parameters.Add(new SqlParameter("@" + sqlParameter, sqlDbType));
      }
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

  } // Class

} // Namespace
