/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Microsoft.Data.SqlClient;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Querying;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL DATA READER EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extension methods for the <see cref="IDataReader"/> class.
  /// </summary>
  /// <remarks>
  ///   Most of the extensions are optimized for reading the data returned from the <c>GetTopics</c> and <c>GetTopicVersion</c>
  ///   stored procedures, which require parsing multiple data sets in order to populate a topic graph. For that purpose, the
  ///   main entry point is <see cref="LoadTopicGraph"/>. It is supported by a number of <c>private</c> extensions which allow
  ///   it to handle individual records from particular data sets (e.g., the <see cref="SetExtendedAttributes"/> method maps to
  ///   data returned from the <c>ExtendedAttributeIndex</c> view). That said, the <c>Get</c> extensions (e.g., <see
  ///   cref="GetString(SqlDataReader, String)"/>) are not specific to this format, and remain useful for a variety of database
  ///   queries, should they be needed, and thus are marked as <c>internal</c>.
  /// </remarks>
  internal static class SqlDataReaderExtensions {

    /*==========================================================================================================================
    | METHOD: LOAD TOPIC GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="IDataReader"/> from a call to the <c>GetTopics</c> stored procedure, will extract a list of
    ///   topics and populate their attributes, relationships, and children.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="referenceTopic">
    ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic references
    ///   and relationships, including <see cref="Topic.Parent"/>, are integrated with existing entities.
    /// </param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    /// <param name="includeExternalReferences">
    ///   Optionally disables populating external references such as <see cref="Topic.Relationships"/> and <see
    ///   cref="Topic.BaseTopic"/>. This is useful for cases where it's known that a shallow copy is being retrieved, and
    ///   thus external references aren't likely to be available.
    /// </param>
    internal static Topic? LoadTopicGraph(
      this IDataReader reader,
      Topic? referenceTopic = null,
      bool? markDirty = null,
      bool includeExternalReferences = true
    ) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Establish topic index
      \---------------------------------------------------------------------------------------------------------------------*/
      var sqlDataReader         = reader as SqlDataReader;
      var topics                = referenceTopic is not null? referenceTopic.GetRootTopic().GetTopicIndex() : new();
      var rootTopicId           = -1;

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate topics
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): AddTopic() [" + DateTime.Now + "]");
      while (reader.Read()) {
        if (rootTopicId < 0) {
          rootTopicId           = reader.GetTopicId();
        }
        reader.AddTopic(topics, markDirty);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetIndexedAttributes() [" + DateTime.Now + "]");

      // Move to TopicAttributes dataset
      reader.NextResult();

      while (reader.Read()) {
        reader.SetIndexedAttributes(topics, markDirty);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read extended attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetExtendedAttributes() [" + DateTime.Now + "]");

      // Move to extened attributes dataset
      reader.NextResult();

      // Loop through each extended attribute record associated with a specific topic
      while (reader.Read()) {
        if (sqlDataReader is not null) {
          sqlDataReader.SetExtendedAttributes(topics, markDirty);
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read related items
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetRelationships() [" + DateTime.Now + "]");

      // Move to the relationships dataset
      reader.NextResult();

      // Loop through each relationship; multiple records may exist per topic
      if (includeExternalReferences) {
        while (reader.Read()) {
          reader.SetRelationships(topics, markDirty);
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read referenced items
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetReferences() [" + DateTime.Now + "]");

      // Move to the version history dataset
      reader.NextResult();

      // Loop through each version; multiple records may exist per topic
      while (reader.Read()) {
        reader.SetReferences(topics, markDirty);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read version history
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetVersionHistory() [" + DateTime.Now + "]");

      // Move to the version history dataset
      reader.NextResult();

      // Loop through each version; multiple records may exist per topic
      while (reader.Read()) {
        reader.SetVersionHistory(topics);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topics.ContainsKey(rootTopicId)) {
        return topics[rootTopicId];
      }
      return topics.Values.FirstOrDefault();

    }

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given the primary topic attributes from the <c>TopicIndex</c> view, establishes a barebones <see cref="Topic"/>
    ///   instance and adds it to the <paramref name="topics"/> collection.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    private static void AddTopic(this IDataReader reader, TopicIndex topics, bool? markDirty) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var key                   = reader.GetString("TopicKey");
      var contentType           = reader.GetString("ContentType");
      var parentId              = reader.GetInteger("ParentID");
      var wasDirty              = false;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!topics.TryGetValue(topicId, out var current)) {
        current = TopicFactory.Create(key, contentType, topicId);
        topics.Add(current.Id, current);
      }
      else {
        wasDirty                = current.IsDirty();
        current.Key             = key;
        current.ContentType     = contentType;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parentId >= 0 && current.Parent?.Id != parentId && topics.Keys.Contains(parentId)) {
        current.Parent = topics[parentId];
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark clean
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (wasDirty is false && markDirty is not null and false) {
        current.MarkClean();
      }

    }

    /*==========================================================================================================================
    | METHOD: SET INDEXED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given an attribute record from the <c>AttributeIndex</c> view, finds the associated <see cref="Topic"/> in the
    ///   <paramref name="topics"/> collection, and sets the corresponding <see cref="Topic.Attributes"/> value.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    private static void SetIndexedAttributes(this IDataReader reader, TopicIndex topics, bool? markDirty) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var attributeKey          = reader.GetString("AttributeKey");
      var attributeValue        = reader.GetString("AttributeValue");
      var version               = reader.GetVersion();

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle empty attributes (treat empty as null)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[topicId];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.Attributes.SetValue(attributeKey, attributeValue, markDirty, version, false);

    }

    /*==========================================================================================================================
    | METHOD: SET EXTENDED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given an XML record from the <c>ExtendedAttributeIndex</c> view, finds the associated <see cref="Topic"/> in the
    ///   <paramref name="topics"/> collection, and sets each of the the corresponding <see cref="Topic.Attributes"/> value.
    /// </summary>
    /// <remarks>
    ///   Values of arbitrary length are stored in an XML entry. This makes them more efficient to store, but more difficult to
    ///   query; as such, it's ideal for content-oriented data. The XML values are returned as a separate data set.
    /// </remarks>
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    private static void SetExtendedAttributes(this SqlDataReader reader, TopicIndex topics, bool? markDirty) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var version               = reader.GetVersion();

      /*------------------------------------------------------------------------------------------------------------------------
      | Load SQL XML into XmlDocument
      \-----------------------------------------------------------------------------------------------------------------------*/
      var xmlData               = reader.GetSqlXml(1);
      var xmlReader             = xmlData.CreateReader();

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify the current topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[topicId];

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
        var attributeKey        = (string?)xmlReader.GetAttribute("key");
        var attributeValue      = WebUtility.HtmlDecode(xmlReader.ReadInnerXml());

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate assumptions
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(
          attributeKey,
          $"The @key attribute of the <attribute /> element is missing for Topic '{topicId}'; the data is not in the " +
          $"expected format."
        );

        /*----------------------------------------------------------------------------------------------------------------------
        | Set attribute value
        \---------------------------------------------------------------------------------------------------------------------*/
        if (String.IsNullOrEmpty(attributeValue)) continue;
        current.Attributes.SetValue(attributeKey, attributeValue, markDirty, version, true);

      } while (xmlReader.Name is "attribute");

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
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    private static void SetRelationships(this IDataReader reader, TopicIndex topics, bool? isDirty = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = reader.GetTopicId("Source_TopicID");
      var targetTopicId         = reader.GetTopicId("Target_TopicID");
      var relationshipKey       = reader.GetString("RelationshipKey");
      var isDeleted             = reader.GetBoolean("IsDeleted");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];
      var related               = (Topic?)null;

      // Fetch the related topic
      if (topics.Keys.Contains(targetTopicId)) {
        related                 = topics[targetTopicId];
      }

      // Bypass if the target object is missing
      if (related is null) {
        current.Relationships.IsFullyLoaded = false;
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship on object
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isDeleted) {
        current.Relationships.SetTopic(relationshipKey, related, isDirty);
      }
      else if (current.Relationships.Contains(relationshipKey, related)) {
        current.Relationships.RemoveTopic(relationshipKey, related);
      }

    }

    /*==========================================================================================================================
    | METHOD: SET REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Adds topic references to their associated topics.
    /// </summary>
    /// <remarks>
    ///   Topics can be cross-referenced with each other topics via a one-to-one relationships. Once the topics are populated in
    ///   memory, loop through the data to create these associations.
    /// </remarks>
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    /// <param name="markDirty">
    ///   Specified whether the target collection value should be marked as dirty, assuming the value changes. By default, it
    ///   will be marked dirty if the value is new or has changed from a previous value. By setting this parameter, that
    ///   behavior is overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update
    ///   from being persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/>.
    /// </param>
    private static void SetReferences(this IDataReader reader, TopicIndex topics, bool? markDirty) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = reader.GetTopicId("Source_TopicID");
      var relationshipKey       = reader.GetString("ReferenceKey");
      var targetTopicId         = reader.GetNullableTopicId("Target_TopicID");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];
      var referenced            = (Topic?)null;

      // Fetch the related topic
      if (targetTopicId is not null && topics.Keys.Contains(targetTopicId.Value)) {
        referenced              = topics[targetTopicId.Value];
      }

      // Bypass if the target object is missing
      if (referenced is null) {
        current.References.IsFullyLoaded = false;
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship on object
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.References.SetValue(relationshipKey, referenced, markDirty);

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
    /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void SetVersionHistory(this IDataReader reader, TopicIndex topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var dateTime              = reader.GetVersion();

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[topicId];

      /*------------------------------------------------------------------------------------------------------------------------
      | Set history
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!current.VersionHistory.Contains(dateTime)) {
        current.VersionHistory.Add(dateTime);
      }

    }

    /*==========================================================================================================================
    | METHOD: GET STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a string value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    private static string GetString(this IDataReader reader, string columnName) =>
      reader.GetString(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET BOOLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a boolean value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    private static bool GetBoolean(this IDataReader reader, string columnName) =>
      reader.GetBoolean(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an integer value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    private static int GetInteger(this IDataReader reader, string columnName) =>
      Int32.TryParse(reader.GetValue(reader.GetOrdinal(columnName)).ToString(), out var output)? output : -1;

    /*==========================================================================================================================
    | METHOD: GET TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Topic.Id"/> value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    private static int GetTopicId(this IDataReader reader, string columnName = "TopicID") =>
      reader.GetInt32(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET NULLABLE TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Topic.Id"/> value by column name, while accepting null values.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    private static int? GetNullableTopicId(this IDataReader reader, string columnName = "TopicID") =>
      reader.IsDBNull(reader.GetOrdinal(columnName))? null : reader.GetInt32(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET VERSION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the version column, with precisions appropriate for setting the <see cref="Topic.VersionHistory"/>.
    /// </summary>
    /// <param name="reader">The <see cref="IDataReader"/> object.</param>
    private static DateTime GetVersion(this IDataReader reader) =>
      reader.GetDateTime(reader.GetOrdinal("Version"));

  } //Class
} //Namespace