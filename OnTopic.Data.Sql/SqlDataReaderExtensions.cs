/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.Data.SqlClient;
using OnTopic.Attributes;
using OnTopic.Internal.Diagnostics;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL DATA READER EXTENSIONS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Extension methods for the <see cref="SqlDataReader"/> class.
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
    ///   Given a <see cref="SqlDataReader"/> from a call to the <c>GetTopics</c> stored procedure, will extract a list of
    ///   topics and populate their attributes, relationships, and children.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="includeExternalReferences">
    ///   Optionally disables populating external references such as <see cref="Topic.Relationships"/> and <see
    ///   cref="Topic.DerivedTopic"/>. This is useful for cases where it's known that a shallow copy is being retrieved, and
    ///   thus external references aren't likely to be available.
    /// </param>
    internal static Topic LoadTopicGraph(this SqlDataReader reader, bool includeExternalReferences = true) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Establish topic index
      \---------------------------------------------------------------------------------------------------------------------*/
      var topics                = new Dictionary<int, Topic>();

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate topics
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): AddTopic() [" + DateTime.Now + "]");
      while (reader.Read()) {
        reader.AddTopic(topics);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetIndexedAttributes() [" + DateTime.Now + "]");

      // Move to TopicAttributes dataset
      reader.NextResult();

      while (reader.Read()) {
        reader.SetIndexedAttributes(topics);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Read extended attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetExtendedAttributes() [" + DateTime.Now + "]");

      // Move to extened attributes dataset
      reader.NextResult();

      // Loop through each extended attribute record associated with a specific topic
      while (reader.Read()) {
        reader.SetExtendedAttributes(topics);
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
          reader.SetRelationships(topics);
        }
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

      /*----------------------------------------------------------------------------------------------------------------------
      | Populate strongly typed references
      \---------------------------------------------------------------------------------------------------------------------*/
      Debug.WriteLine("SqlTopicRepository.Load(): SetDerivedTopics() [" + DateTime.Now + "]");

      if (includeExternalReferences) {
        SetDerivedTopics(topics);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topics.Values.FirstOrDefault();

    }

    /*==========================================================================================================================
    | METHOD: ADD TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given the primary topic attributes from the <c>TopicIndex</c> view, establishes a barebones <see cref="Topic"/>
    ///   instance and adds it to the <paramref name="topics"/> collection.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void AddTopic(this SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var key                   = reader.GetString("TopicKey");
      var contentType           = reader.GetString("ContentType");
      var parentId              = reader.GetInteger("ParentID");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = TopicFactory.Create(key, contentType, topicId);

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
    /// <summary>
    ///   Given an attribute record from the <c>AttributeIndex</c> view, finds the associated <see cref="Topic"/> in the
    ///   <paramref name="topics"/> collection, and sets the corresponding <see cref="Topic.Attributes"/> value.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void SetIndexedAttributes(this SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var attributeKey          = reader.GetString("AttributeKey");
      var attributeValue        = reader.GetString("AttributeValue");
      var version               = DateTime.Now;

      //Check field count to avoid breaking changes with the 4.0.0 release, which didn't include a "Version" column
      //### TODO JJC20200221: This condition can be removed and accepted as a breaking change in v5.0.
      if (reader.FieldCount > 3) {
        version                 = reader.GetVersion();
      }

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
      current.Attributes.SetValue(attributeKey, attributeValue, false, version, false);

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
    private static void SetExtendedAttributes(this SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicId               = reader.GetTopicId();
      var version               = DateTime.Now;

      //Check field count to avoid breaking changes with the 4.0.0 release, which didn't include a "Version" column
      //### TODO JJC20200221: This condition can be removed and accepted as a breaking change in v5.0.
      if (reader.FieldCount > 2) {
        version                 = reader.GetVersion();
      }

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
        var attributeKey        = (string)xmlReader.GetAttribute("key");
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
        current.Attributes.SetValue(attributeKey, attributeValue, false, version, true);

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
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void SetRelationships(this SqlDataReader reader, Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceTopicId         = reader.GetTopicId("Source_TopicID");
      var targetTopicId         = reader.GetTopicId("Target_TopicID");
      var relationshipKey       = reader.GetString("RelationshipKey");

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify affected topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var current               = topics[sourceTopicId];
      var related               = (Topic?)null;

      // Fetch the related topic
      if (topics.Keys.Contains(targetTopicId)) {
        related                 = topics[targetTopicId];
      }

      // Bypass if either of the objects are missing
      if (related is null) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship on object
      \-----------------------------------------------------------------------------------------------------------------------*/
      current.Relationships.SetTopic(relationshipKey, related, isIncoming: false, isDirty: false);

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
    /// <param name="reader">The <see cref="SqlDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void SetVersionHistory(this SqlDataReader reader, Dictionary<int, Topic> topics) {

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
      current.VersionHistory.Add(dateTime);

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
    /// <param name="topics">A <see cref="Dictionary{Int32, Topic}"/> of topics to be loaded.</param>
    private static void SetDerivedTopics(Dictionary<int, Topic> topics) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in topics.Values) {
        var derivedTopicId      = topic.Attributes.GetInteger("TopicId", -1, false, false);
        if (derivedTopicId < 0) continue;
        if (topics.Keys.Contains(derivedTopicId)) {
          topic.DerivedTopic    = topics[derivedTopicId];
        }

      }

    }

    /*==========================================================================================================================
    | METHOD: GET INTEGER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves an integer value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static int GetInteger(this SqlDataReader reader, string columnName) =>
      Int32.TryParse(reader.GetValue(reader.GetOrdinal(columnName)).ToString(), out var output)? output : -1;

    /*==========================================================================================================================
    | METHOD: GET TOPIC ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a <see cref="Topic.Id"/> value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static int GetTopicId(this SqlDataReader reader, string columnName = "TopicID") =>
      reader.GetInt32(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET STRING
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a string value by column name.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    /// <param name="columnName">The name of the column to retrieve the value from.</param>
    internal static string GetString(this SqlDataReader reader, string columnName) =>
      reader.GetString(reader.GetOrdinal(columnName));

    /*==========================================================================================================================
    | METHOD: GET VERSION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves the version column, with precisions appropriate for setting the <see cref="Topic.VersionHistory"/>.
    /// </summary>
    /// <param name="reader">The <see cref="SqlDataReader"/> object.</param>
    internal static DateTime GetVersion(this SqlDataReader reader) =>
      reader.GetDateTime(reader.GetOrdinal("Version"));

  } //Class
} //Namespace