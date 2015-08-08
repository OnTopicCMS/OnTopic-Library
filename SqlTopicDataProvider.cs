/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC (casey.margell@ignia.com)
| Client        Ignia
| Project       Topics Editor
|
| Purpose       Implementation of the topic provider specific to SQL.
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               03.24.09        Casey Margell           Initial version template
|               07.26.10        Hedley Robertson        Added handling for relationships
|               11.12.10        Jeremy Caney            Fixed issue with connection pooling during Save().
|               08.25.13        Jeremy Caney            Updated Save() to correctly recurse, including setting of ParentID.
|               08.25.13        Jeremy Caney            Modified behavior of attribute lookup to handle arbitrary blob values.
|               09.28.13        Jeremy Caney            Added basic dependency injection (DI) to support Topic derivations.
|               09.30.13        Jeremy Caney            Updated to use TopicRepository.ContentTypes to lookup StoreInBlob on
                                                        Save().
|               08.06.14        Katherine Trunkey       Updated references to TopicAttribute to Attribute.
|               08.06.14        Katherine Trunkey       Updated all instances of Attributes[key] to Attributes[key].Value.
|               08.07.14        Katherine Trunkey       Updated Save() method correspondent to Versioning feature; added
|                                                       IsDraft parameter and corresponding logic.
|               08.13.14        Katherine Trunkey       Removed obsolete GetAttributes() property method.
|               08.14.14        Katherine Trunkey       Updated Save() method to use uncommon, multi-character delimiters rather
|                                                       than a colon and semicolon in the creation of the Attributes string in
|                                                       order to provide better escaping safety for the @Attributes parameter.
\-----------------------------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Xml;
using Ignia.Web.Tools;

namespace Ignia.Topics {

  /*==============================================================================================================================
  | CLASS
  \-----------------------------------------------------------------------------------------------------------------------------*/
  public class SqlTopicDataProvider : TopicDataProviderBase {

  /*============================================================================================================================
  | METHOD: LOAD
  >-----------------------------------------------------------------------------------------------------------------------------
  | Interface method that loads topics
  \---------------------------------------------------------------------------------------------------------------------------*/
    public override Topic Load(string topicKey, int topicId, int depth, DateTime? version = null) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | ESTABLISH DATABASE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      Dictionary<int, Topic>    topics          = new Dictionary<int, Topic>();
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = new SqlCommand("topics_GetTopics", connection);
      command.CommandType                       = CommandType.StoredProcedure;
      SqlDataReader             reader          = null;
      int                       sortOrder       = 0;

      try {

      /*------------------------------------------------------------------------------------------------------------------------
      | OPEN CONNECTION
      \-----------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

      /*------------------------------------------------------------------------------------------------------------------------
      | ESTABLISH QUERY PARAMETERS
      \-----------------------------------------------------------------------------------------------------------------------*/
        Utility.AddSqlParameter(command, "TopicName",   topicKey,                                       SqlDbType.VarChar);
        Utility.AddSqlParameter(command, "Depth",       depth.ToString(CultureInfo.InvariantCulture),   SqlDbType.Int);
        Utility.AddSqlParameter(command, "TopicID",     topicId.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

        if (version != null) {
          Utility.AddSqlParameter(command, "Version",   version.ToString(),                             SqlDbType.DateTime);
        }

      /*------------------------------------------------------------------------------------------------------------------------
      | EXECUTE QUERY/READER
      \-----------------------------------------------------------------------------------------------------------------------*/
        reader                                  = command.ExecuteReader();

      /*------------------------------------------------------------------------------------------------------------------------
      | POPULATE TOPICS
      \-----------------------------------------------------------------------------------------------------------------------*/
        while (reader.Read()) {

        //Identify attribute values
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                contentType     = reader["ContentType"].ToString();
          string                key             = reader["TopicKey"].ToString();
                                sortOrder       = Int32.Parse(reader["SortOrder"].ToString(), CultureInfo.InvariantCulture);
          int                   parentId        = -1;

        //Handle ParentID (could be null for root topic)
          Int32.TryParse(reader["ParentID"].ToString(), out parentId);

        //Determine target type
          Type                  baseType        = System.Type.GetType("Ignia.Topics.Topic");
          Type                  targetType      = System.Type.GetType("Ignia.Topics." + contentType);

        //Validate type
          if (targetType == null) {
            targetType          = baseType;
          }
          else if (!targetType.IsSubclassOf(baseType)) {
            targetType          = baseType;
            throw new Exception("The topic \"Ignia.Topics." + contentType + "\" does not derive from \"Ignia.Topics.Topic\".");
          }

        //Identify the appropriate topic
          dynamic               current         = Activator.CreateInstance(targetType);

        //Create new topic, if topic doesn't exist
          if (!topics.Keys.Contains(id)) {
            current.Key         = key;
            current.Id          = id;
            topics.Add(current.Id, current);
          }

        //Reference existing topic, if topic exists
          else {
            current             = topics[id];
          }

        //Assign sort order, based on database order
          if (current.SortOrder < 0) {
            current.SortOrder   = sortOrder++;
          }

        //Set Content Type
          if (!current.Attributes.Contains("ContentType")) {
            current.Attributes.Add(new AttributeValue("ContentType", contentType, false));
          }

        //Provide special handling for ParentId
          if (parentId == -1) {
            continue;
          }

          if (topics.Keys.Contains(parentId)) {
            current.Parent      = topics[parentId];
          }

        //Add Key, ContentType, and ParentID to Attributes (AttributesCollection) if not available
        //to ensure Attributes is populated
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

      /*------------------------------------------------------------------------------------------------------------------------
      | READ ATTRIBUTES
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Move to TopicAttributes dataset
        reader.NextResult();

        while (reader.Read()) {

        //Identify attribute values
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                name            = reader["AttributeKey"].ToString();
          string                value           = reader["AttributeValue"].ToString();
          DateTime              versionDate     = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          Topic                 current         = topics[id];

        //Treat empty as null
          if (String.IsNullOrEmpty(value) || DBNull.Value.Equals(value)) continue;

        //Set attribute value
          if (!current.Attributes.Contains(name)) {
            current.Attributes.Add(new AttributeValue(name, value, false));
          }

        }

      /*------------------------------------------------------------------------------------------------------------------------
      | READ BLOB
      >-------------------------------------------------------------------------------------------------------------------------
      | Values of arbitrary length are stored in an XML blob. This makes them more efficient to store, but more difficult to
      | query; as such, it's ideal for content-oriented data. The blob values are returned as a separate data set.
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Move to blob dataset
        reader.NextResult();

      //Loop through each blob, each record associated with a specific record
        while (reader.Read()) {

        //Identify variables
          int                   id              = Int32.Parse(reader["TopicID"].ToString(), CultureInfo.InvariantCulture);
          DateTime              versionDate     = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          XmlDocument           blob            = new XmlDocument();

        //Load the blob into an XmlDocument object
          blob.LoadXml((string)reader["Blob"]);

        //This scenario should never occur.
          if (!topics.Keys.Contains(id)) continue;

        //Identify the current topic
          Topic                 current         = topics[id];

        //Loop through each node in the blob and associate with the current topic
          foreach (XmlNode attribute in blob.DocumentElement.GetElementsByTagName("attribute")) {
            string              name            = attribute.Attributes["key"].Value;
            string              value           = System.Web.HttpContext.Current.Server.HtmlDecode(attribute.InnerXml);

          //Treat empty as null
            if (String.IsNullOrEmpty(value)) continue;

            if (!current.Attributes.Contains(name)) {
              current.Attributes.Add(new AttributeValue(name, value, false));
            }
            else {
            //System.Web.HttpContext.Current.Response.Write("Attribute '" + name + "(" + value + ") already exists. It was not added.");
            }

          }

        }

      /*------------------------------------------------------------------------------------------------------------------------
      | READ RELATED ITEMS
      >-------------------------------------------------------------------------------------------------------------------------
      | Topics can be cross-referenced with each other via a many-to-many relationships. Once the topics are populated in
      | memory, loop through the data to create these associations.
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Move to the relationships dataset
        reader.NextResult();

      //Loop through each relationship; multiple records may exist per topic
        while (reader.Read()) {

        //Identify variables
          int                   sourceTopicId           = Int32.Parse(reader["Source_TopicID"].ToString(), CultureInfo.InvariantCulture);
          int                   targetTopicId           = Int32.Parse(reader["Target_TopicID"].ToString(), CultureInfo.InvariantCulture);
          string                relationshipTypeId      = (string)reader["RelationshipTypeID"];
          Topic                 related                 = null;
          Topic                 current                 = null;

        //Fetch the source topic
          if (topics.Keys.Contains(sourceTopicId)) {
            current             = topics[sourceTopicId];
          }
          else {
            current             = TopicRepository.RootTopic.GetTopic(sourceTopicId);
          }

        //Fetch the related topic
          if (topics.Keys.Contains(targetTopicId)) {
            related             = topics[targetTopicId];
          }

        //Bypass if either of the objects are missing
          if (current == null || related == null) continue;

        //Set relationships on object
          current.SetRelationship(relationshipTypeId, related);

        }

      /*------------------------------------------------------------------------------------------------------------------------
      | READ VERSION HISTORY
      >-------------------------------------------------------------------------------------------------------------------------
      | Every time a value changes for an attribute, a new version is created, represented by the date of the change. This
      | version history is aggregated per topic to allow topic information to be rolled back to a specific date. While version
      | content is not exposed directly via the Load() method, the metadata is.
      \-----------------------------------------------------------------------------------------------------------------------*/

      //Move to the version history dataset
        reader.NextResult();

      //Loop through each version; multiple records may exist per topic
        while (reader.Read()) {

        //Identify variables
          int                   sourceTopicId           = Int32.Parse(reader["TopicId"].ToString(), CultureInfo.InvariantCulture);
          DateTime              dateTime                = Convert.ToDateTime(reader["Version"].ToString(), CultureInfo.InvariantCulture);
          Topic                 current                 = null;

        //Fetch the target topic
          if (topics.Keys.Contains(sourceTopicId)) {
            current                                     = topics[sourceTopicId];
          }

        //Set history
          if (!current.VersionHistory.Contains(dateTime)) {
            current.VersionHistory.Add(dateTime);
          }

        }

      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CATCH EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Topics failed to load: " + ex.Message);
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CLOSE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      finally {
        if (reader != null) reader.Dispose();
        command.Dispose();
        connection.Dispose();
        connection.Close();
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETURN OBJECTS
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (topics.Count == 0) return null;
      return topics[topics.Keys.ElementAt(0)];

    }

  /*============================================================================================================================
  | METHOD: SAVE
  >-----------------------------------------------------------------------------------------------------------------------------
  | Interface method that saves topic attributes, also used for renaming a topic since name is stored as an attribute
  \---------------------------------------------------------------------------------------------------------------------------*/
    public override int Save(Topic topic, bool isRecursive, bool isDraft = false) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

    /*--------------------------------------------------------------------------------------------------------------------------
    | CALL BASE METHOD - WILL TRIGGER ANY EVENTS ASSOCIATED WITH THE SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
      base.Save(topic, isRecursive, isDraft);

    /*--------------------------------------------------------------------------------------------------------------------------
    | VALIDATE CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (!TopicRepository.ContentTypes.Contains(topic.GetAttribute("ContentType"))) {
        throw new Exception("The Content Type \"" + topic.GetAttribute("ContentType", "Page") + "\" referenced by \"" + topic.Key + "\" could not be found. under \"Configuration:ContentTypes\". There are " + TopicRepository.ContentTypes.Count + " ContentTypes in the Repository.");
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | ESTABLISH ATTRIBUTE STRINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    //Strings are immutable, use a stringbuilder to save memory
      StringBuilder             attributes      = new StringBuilder();
      StringBuilder             nullAttributes  = new StringBuilder();
      StringBuilder             blob            = new StringBuilder();
      ContentType               contentType     = TopicRepository.ContentTypes[topic.GetAttribute("ContentType", "Page")];

      blob.Append("<attributes>");

    /*--------------------------------------------------------------------------------------------------------------------------
    | LOOP THROUGH THE ATTRIBUTES, ADDING THE NAMES AND VALUES TO THE STRING BUILDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    //Process attributes not stored in the Blob
      foreach (AttributeValue attributeValue in topic.Attributes) {

        string                  key             = attributeValue.Key;
        Attribute               attribute       = null;

        if (contentType.SupportedAttributes.Keys.Contains(key)) {
          attribute                             = contentType.SupportedAttributes[key];
        }

      //For attributes not stored in the Blob, only add the AttributeValue item to store if it has changed
        if (attribute != null && !attribute.StoreInBlob && attributeValue.IsDirty) {
          attributes.Append(key + "~~" + topic.Attributes[key].Value + "``");
        }
        else if (attribute != null && attribute.StoreInBlob) {
          blob.Append("<attribute key=\"" + key + "\"><![CDATA[" + topic.Attributes[key].Value + "]]></attribute>");
        }

      //Reset IsDirty (changed) state
        attributeValue.IsDirty                  = false;

      }

      blob.Append("</attributes>");

    /*--------------------------------------------------------------------------------------------------------------------------
    | LOOP THROUGH THE CONTENT TYPE'S SUPPORTED ATTRIBUTES AND ADD ATTRIBUTE TO NULL ATTRIBUTES IF TOPIC DOES NOT CONTAIN IT
    \-------------------------------------------------------------------------------------------------------------------------*/
      foreach (string attributeKey in contentType.SupportedAttributes.Keys) {

      //Set preconditions
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

    /*--------------------------------------------------------------------------------------------------------------------------
    | ESTABLISH DATABASE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;
      int                       returnVal       = -1;

      try {

      /*------------------------------------------------------------------------------------------------------------------------
      | UPDATE RELATIONS
      \-----------------------------------------------------------------------------------------------------------------------*/
        connection.Open();

      /*------------------------------------------------------------------------------------------------------------------------
      | ESTABLISH COMMAND TYPE (INSERT OR UPDATE)
      \-----------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          command               = new SqlCommand("topics_UpdateTopic", connection);
        }
        else {
          command               = new SqlCommand("topics_CreateTopic", connection);
        }

        command.CommandType     = CommandType.StoredProcedure;

      /*------------------------------------------------------------------------------------------------------------------------
      | SET VERSION DATETIME
      \-----------------------------------------------------------------------------------------------------------------------*/
        DateTime version        = DateTime.Now;

      //NOTE: KLT031915: Commented out as Draft functionality is not fully implemented
      /*
        if (isDraft) {
          version               = DateTime.MaxValue;
          }
      */

      /*------------------------------------------------------------------------------------------------------------------------
      | ESTABLISH QUERY PARAMETERS
      \-----------------------------------------------------------------------------------------------------------------------*/
        if (topic.Id != -1) {
          Utility.AddSqlParameter(command,      "TopicID",              topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        if (topic.Parent != null) {
          Utility.AddSqlParameter(command,      "ParentID",             topic.Parent.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }
        Utility.AddSqlParameter(command,        "Version",              version.ToString("yyyy-MM-dd HH:mm:ss.fff"), SqlDbType.DateTime);
        Utility.AddSqlParameter(command,        "Attributes",           attributes.ToString(),                  SqlDbType.VarChar);
        if (topic.Id != -1) {
          Utility.AddSqlParameter(command,      "NullAttributes",       nullAttributes.ToString(),              SqlDbType.VarChar);
          Utility.AddSqlParameter(command,      "DeleteRelationships",  "1",                                    SqlDbType.Bit);
        }
        Utility.AddSqlParameter(command,        "Blob",                 blob.ToString(),                        SqlDbType.Xml);
        Utility.AddSqlParameter(command,        "ReturnCode",           ParameterDirection.ReturnValue,         SqlDbType.Int);

      /*------------------------------------------------------------------------------------------------------------------------
      | EXECUTE QUERY
      \-----------------------------------------------------------------------------------------------------------------------*/
        command.ExecuteNonQuery();

      /*------------------------------------------------------------------------------------------------------------------------
      | PROCESS RETURN VALUE
      \-----------------------------------------------------------------------------------------------------------------------*/
        returnVal               = Int32.Parse(command.Parameters["@ReturnCode"].Value.ToString(), CultureInfo.InvariantCulture);
        topic.Id                = returnVal;

      /*------------------------------------------------------------------------------------------------------------------------
      | ADD VERSION TO VERSION HISTORY
      \-----------------------------------------------------------------------------------------------------------------------*/
        topic.VersionHistory.Insert(0, version);

      /*------------------------------------------------------------------------------------------------------------------------
      | UPDATE RELATIONS
      \-----------------------------------------------------------------------------------------------------------------------*/
        PersistRelations(topic, connection, true);

      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CATCH EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Failed to save Topic " + topic.Key + " (" + topic.Id + ") via " + ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString + ": " + ex.Message);
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CLOSE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      finally {
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | RECURSE
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (Topic childTopic in topic) {
          childTopic.Attributes["ParentID"].Value = returnVal.ToString();
          childTopic.Save(isRecursive, isDraft);
        }
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETURN VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
      return returnVal;

    }

  /*----------------------------------------------------------------------------------------------------------------------------
  | METHOD: MOVE
  >-----------------------------------------------------------------------------------------------------------------------------
  | Interface method that moves the provided topic from the tree
  \---------------------------------------------------------------------------------------------------------------------------*/
    public override bool Move(Topic topic, Topic target) {
      return this.Move(topic, target, null);
    }

    public override bool Move(Topic topic, Topic target, Topic sibling) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");
      if (target == null) throw new ArgumentNullException("target");

    /*--------------------------------------------------------------------------------------------------------------------------
    | DELETE FROM MEMORY
    \-------------------------------------------------------------------------------------------------------------------------*/
      base.Move(topic, target);

    /*--------------------------------------------------------------------------------------------------------------------------
    | MOVE IN DATABASE
    \-------------------------------------------------------------------------------------------------------------------------*/
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;

      try {

        command                                 = new SqlCommand("topics_MoveTopic", connection);
        command.CommandType                     = CommandType.StoredProcedure;

      //Add Parameters
        Utility.AddSqlParameter(command, "TopicID",  topic.Id.ToString(CultureInfo.InvariantCulture),  SqlDbType.Int);
        Utility.AddSqlParameter(command, "ParentID", target.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

      //Append sibling ID if set
        if (sibling != null) {
          Utility.AddSqlParameter(command, "SiblingID", sibling.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);
        }

      //Execute Query
        connection.Open();

        command.ExecuteNonQuery();

      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CATCH EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Failed to move Topic " + topic.Key + " (" + topic.Id + ") to " + target.Key + " (" + target.Id + "): " + ex.Message);
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CLOSE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      finally {
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
      }

      return true;
    }

  /*============================================================================================================================
  | METHOD: DELETE
  >-----------------------------------------------------------------------------------------------------------------------------
  | Interface method that deletes the provided topic from the tree
  \---------------------------------------------------------------------------------------------------------------------------*/
    public override void Delete(Topic topic, bool isRecursive) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) throw new ArgumentNullException("topic");

    /*--------------------------------------------------------------------------------------------------------------------------
    | DELETE FROM MEMORY
    \-------------------------------------------------------------------------------------------------------------------------*/
      base.Delete(topic, isRecursive);

    /*--------------------------------------------------------------------------------------------------------------------------
    | DELETE FROM DATABASE
    \-------------------------------------------------------------------------------------------------------------------------*/
      SqlConnection             connection      = new SqlConnection(ConfigurationManager.ConnectionStrings["TopicsServer"].ConnectionString);
      SqlCommand                command         = null;

      try {

        command                                 = new SqlCommand("topics_DeleteTopic", connection);
        command.CommandType                     = CommandType.StoredProcedure;

      //Add Parameters
        Utility.AddSqlParameter(command, "TopicID", topic.Id.ToString(CultureInfo.InvariantCulture), SqlDbType.Int);

      //Execute Query
        connection.Open();

        command.ExecuteNonQuery();

      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CATCH EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Failed to delete Topic " + topic.Key + " (" + topic.Id + "): " + ex.Message);
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CLOSE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      finally {
        if (command != null) command.Dispose();
        if (connection != null) connection.Dispose();
      }

    }

  /*============================================================================================================================
  | METHOD: PERSIST RELATIONS
  >-----------------------------------------------------------------------------------------------------------------------------
  | Internal method that saves topic relationships to the n:n mapping table in sql, returns a xml formatted string for
  | appending to the attribute 'blob' unless skipBlob == true (not really sure if we wanted related items in the blob?)
  \---------------------------------------------------------------------------------------------------------------------------*/
    private static string PersistRelations(Topic topic, SqlConnection connection) {
      return PersistRelations(topic, connection, false);
    }

    private static string PersistRelations(Topic topic, SqlConnection connection, bool skipBlob) {

      // return "" if the topic has no relations
      if (topic.Relationships.Count <= 0) {
        return "";
      }
      SqlCommand                command         = null;

      try {

      /*------------------------------------------------------------------------------------------------------------------------
      | ITERATE THROUGH EACH SCOPE AND PERSIST TO SQL
      \-----------------------------------------------------------------------------------------------------------------------*/
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

        //Add Parameters:
          Utility.AddSqlParameter(command, "RelationshipTypeID",  scope.Key,                            SqlDbType.VarChar);
          Utility.AddSqlParameter(command, "Source_TopicID",      topicId,                              SqlDbType.Int);
          Utility.AddSqlParameter(command, "Target_TopicIDs",     String.Join(",", targetIds),          SqlDbType.VarChar);

          command.ExecuteNonQuery();

        }

      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CATCH EXCEPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      catch (Exception ex) {
        throw new Exception("Failed to persist relationships for Topic " + topic.Key + " (" + topic.Id + "): " + ex.Message);
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | CLOSE CONNECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
      finally {
      //if (command != null) command.Dispose();
      //Since the connection string is being passed in, do not close connection.
      //if (connection != null) connection.Dispose();
      }

    /*--------------------------------------------------------------------------------------------------------------------------
    | RETURN THE RELATIONSHIP ATTRIBUTES TO APPEND TO THE XML BLOB (unless bool skipBlob)
    \-------------------------------------------------------------------------------------------------------------------------*/
      if (skipBlob) return "";
      else return CreateRelationshipsBlob(topic);

    }

  /*============================================================================================================================
  | METHOD: CREATE RELATIONSHIPS BLOB
  >-----------------------------------------------------------------------------------------------------------------------------
  | Internal helper function to build string of related xml nodes for each scope of related items in model.
  \---------------------------------------------------------------------------------------------------------------------------*/
    private static string CreateRelationshipsBlob(Topic topic) {
      StringBuilder blob = new StringBuilder("");
    // add a related xml node for each scope
      foreach (Topic scope in topic.Relationships) {
        blob.Append("<related scope=\"");
        blob.Append(scope.Key);
        blob.Append("\">");

        // build out string array of related items in this scope
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

  } //Class

} //Namespace
