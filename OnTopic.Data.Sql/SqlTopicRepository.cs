﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using OnTopic.Data.Sql.Models;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Data.Sql {

  /*============================================================================================================================
  | CLASS: SQL TOPIC DATA REPOSITORY
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides data access to topics stored in Microsoft SQL Server.
  /// </summary>
  /// <remarks>
  ///   Concrete implementation of the <see cref="ITopicRepository"/> class.
  /// </remarks>
  public class SqlTopicRepository : TopicRepository, ITopicRepository {

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
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(connectionString), nameof(connectionString));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set private fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      _connectionString         = connectionString;

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic Load(string uniqueKey, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(uniqueKey, nameof(uniqueKey));
      Contract.Requires<TopicNotFoundException>(uniqueKey.Length > 0, nameof(uniqueKey));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("GetTopicID", connection);

      var topicId               = -1;

      command.CommandType       = CommandType.StoredProcedure;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("UniqueKey", uniqueKey);
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
      | Validate results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicId < 0) {
        throw new TopicNotFoundException(uniqueKey);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Load(topicId, referenceTopic, isRecursive);

    }

    /// <inheritdoc />
    public override Topic Load(int topicId, Topic? referenceTopic = null, bool isRecursive = true) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic                 = (Topic?)null;

      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("GetTopics", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };

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
        using var reader        = command.ExecuteReader();
        topic                   = reader.LoadTopicGraph(referenceTopic, false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null) {
        if (topicId == -1) {
          topic = TopicFactory.Create("Root", "Container");
        }
        else {
          throw new TopicNotFoundException(topicId);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish content type cache
      >-------------------------------------------------------------------------------------------------------------------------
      | If this load represents the entire topic graph, then relay the content type configuration to the TopicRepositoryBase in
      | order to either update or establish the content type cache. Not only does this prevent the need for a separate redundant
      | call later but, even more importantly, it helps ensure the same object references are maintained so that any updates to
      | subsequently cached content types are available.
      \-----------------------------------------------------------------------------------------------------------------------*/
      base.SetContentTypeDescriptors(topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      OnTopicLoaded(new(topic, isRecursive));

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <inheritdoc />
    public override Topic Load(int topicId, DateTime version, Topic? referenceTopic = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(version.Date < DateTime.Now, "The version requested must be a valid historical date.");
      Contract.Requires(
        version.Date >= new DateTime(2014, 12, 9),
        "The version is expected to have been created since version support was introduced into the topic library."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Clear associations
      >-------------------------------------------------------------------------------------------------------------------------
      | Because we don't (currently) track version as part of the .NET data model for relationships or topic references, there's
      | no easy way to determine if an association should be deleted when doing a rollback. As such, existing associations
      | should be deleted, assuming a `referenceTopic` is passed, and it contains the `topicId`.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic                 = (Topic?)null;

      if (referenceTopic?.Id == topicId) {
        topic                   = referenceTopic;
      }
      else if (referenceTopic is not null) {
        topic                   = referenceTopic.GetRootTopic().FindFirst(t => t.Id == topicId);
      }

      if (topic is not null) {
        foreach (var relationship in topic.Relationships) {
          topic.Relationships.Clear(relationship.Key);
        }
        topic.References.Clear();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("GetTopicVersion", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };

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
        using var reader        = command.ExecuteReader();
        topic                   = reader.LoadTopicGraph(referenceTopic, includeExternalReferences: referenceTopic is not null);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        if (topic is not null) {
          topic.Relationships.IsFullyLoaded = false;
          topic.References.IsFullyLoaded = false;
        }
        throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate result
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null) {
        throw new TopicNotFoundException(topicId);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete orphaned attributes
      >-------------------------------------------------------------------------------------------------------------------------
      | If a referenceTopic is passed, and it contains the `topicId`, then that instance will be updated with the previous
      | version. In that case, however, any attributes which were first introduced after that version won't be overwritten.
      | That's because there isn't a previous value associated with that key to overwrite the current value. In those cases,
      | those attributes must be manually removed.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var orphanedAttributes = topic.Attributes.Where(a => a.LastModified > version).ToList();
      foreach (var attribute in orphanedAttributes) {
        topic.Attributes.Remove(attribute.Key);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      OnTopicLoaded(new(topic, false, version));

      /*------------------------------------------------------------------------------------------------------------------------
      | Return objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public override void Refresh(Topic referenceTopic, DateTime since) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(referenceTopic, "A referenceTopic from the topic graph must be provided.");
      Contract.Requires(
        since.Date >= DateTime.Now.AddHours(-24),
        "The since date is expected to be within the last twenty four hours."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("GetTopicUpdates", connection) {
        CommandType             = CommandType.StoredProcedure,
        CommandTimeout          = 120
      };

      command.CommandType       = CommandType.StoredProcedure;

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("Since", since);

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {
        connection.Open();
        using var reader        = command.ExecuteReader();
        reader.LoadTopicGraph(referenceTopic.GetRootTopic(), false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException($"Topics failed to update: '{exception.Message}'", exception);
      }

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    protected override sealed void SaveTopic(
      [NotNull]Topic topic,
      DateTime version,
      bool persistRelationships
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var isTopicDirty          = topic.IsDirty();
      var areRelationshipsDirty = topic.Relationships.IsDirty();
      var areReferencesDirty    = topic.References.IsDirty();
      var areAttributesDirty    = topic.Attributes.IsDirty(true);
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
      | If no associations have changed, and no attributes values have changed, and there aren't any mismatched attributes in
      | their respective lists, then there isn't anything new to persist to the database, and thus no benefit to executing the
      | current command. A more aggressive version of this would wrap much of the below logic in this, but this is just meant
      | as a quick fix to reduce the overhead of recursive saves.
      \-----------------------------------------------------------------------------------------------------------------------*/
      areAttributesDirty        =
        areAttributesDirty      ||
        indexedAttributeList.Any(a => a.IsExtendedAttribute == true) ||
        extendedAttributeList.Any(a => a.IsExtendedAttribute == false);

      var isDirty               =
        isTopicDirty            ||
        areRelationshipsDirty   ||
        areReferencesDirty      ||
        areAttributesDirty;

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass is not dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isDirty) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add indexed attributes that are dirty
      >-------------------------------------------------------------------------------------------------------------------------
      | Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it.
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var attributeValues = new AttributeValuesDataTable();

      if (areAttributesDirty) {

        foreach (var attributeValue in indexedAttributeList) {
          attributeValues.AddRow(attributeValue.Key, attributeValue.Value);
        }

        foreach (var attribute in GetUnmatchedAttributes(topic)) {
          attributeValues.AddRow(attribute.Key);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add extended attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var extendedAttributes    = new StringBuilder();

      if (areAttributesDirty) {

        extendedAttributes.Append("<attributes>");

        foreach (var attributeValue in extendedAttributeList) {

          extendedAttributes.Append(
            "<attribute key=\"" + attributeValue.Key + "\"><![CDATA[" + attributeValue.Value + "]]></attribute>"
          );

          //###NOTE JJC20200502: By treating extended attributes as unmatched, we ensure that any indexed attributes with the same
          //value are overwritten with an empty attribute. This is useful for cases where an indexed attribute is moved to an
          //extended attribute, as it persists that version history, while removing ambiguity over which record is authoritative.
          //This is also useful for supporting arbitrary attribute values, since they may be moved from indexed to extended
          //attributes if their length exceeds 255.
          attributeValues.AddRow(attributeValue.Key);

        }

        extendedAttributes.Append("</attributes>");

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      var procedureName         = topic.IsNew? "CreateTopic" : "UpdateTopic";

      connection.Open();

      using var command         = new SqlCommand(procedureName, connection) {
        CommandType             = CommandType.StoredProcedure
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!topic.IsNew) {
        command.AddParameter("TopicID", topic.Id);
        command.AddParameter("DeleteUnmatched", false);
      }
      else if (topic.Parent is not null) {
        command.AddParameter("ParentID", topic.Parent.Id);
      }
      if (isTopicDirty || topic.IsNew) {
        command.AddParameter("Key", topic.Key);
        command.AddParameter("ContentType", topic.ContentType);
      }
      command.AddParameter("Version", version);
      if (areAttributesDirty) {
        command.AddParameter("Attributes", attributeValues);
        command.AddParameter("ExtendedAttributes", extendedAttributes);
      }
      command.AddOutputParameter();

      /*------------------------------------------------------------------------------------------------------------------------
      | Process database query
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {

        if (topic.IsNew || isTopicDirty || areAttributesDirty) {
          command.ExecuteNonQuery();
          topic.Id = command.GetReturnCode();
        }

        Contract.Assume(
          !topic.IsNew,
          "The call to the CreateTopic stored procedure did not return the expected 'Id' parameter."
        );

        if (persistRelationships && areRelationshipsDirty) {
          PersistRelationships(topic, version, connection);
        }

        if (persistRelationships && areReferencesDirty) {
          PersistReferences(topic, version, connection);
        }

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
        connection.Close();
      }

    }

    /*==========================================================================================================================
    | METHOD: MOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override sealed void MoveTopic(Topic topic, Topic target, Topic? sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(target, nameof(target));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish database connection
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("MoveTopic", connection) {
        CommandType             = CommandType.StoredProcedure
      };

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish query parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      command.AddParameter("TopicID", topic.Id);
      command.AddParameter("ParentID", target.Id);

      // Append sibling ID if set
      if (sibling is not null) {
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

    }

    /*==========================================================================================================================
    | METHOD: DELETE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    protected override sealed void DeleteTopic(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Delete from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      using var connection      = new SqlConnection(_connectionString);
      using var command         = new SqlCommand("DeleteTopic", connection) {
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

    }

    /*==========================================================================================================================
    | METHOD: PERSIST RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Internal method that saves topic relationships to the n:n mapping table in SQL.
    /// </summary>
    /// <param name="topic">The topic object whose relationships should be persisted.</param>
    /// <param name="version">The version that should be associated with the updated value.</param>
    /// <param name="connection">The SQL connection.</param>
    private static void PersistRelationships(Topic topic, DateTime version, SqlConnection connection) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Return blank if the topic has no relations.
      \-----------------------------------------------------------------------------------------------------------------------*/
      // return if the topic has no relations
      if (!topic.Relationships.Keys.Any()) {
        return;
      }

      try {

        /*----------------------------------------------------------------------------------------------------------------------
        | Iterate through each scope and persist to SQL
        \---------------------------------------------------------------------------------------------------------------------*/
        foreach (var key in topic.Relationships.Keys) {

          using var targetIds   = new TopicListDataTable();
          using var command     = new SqlCommand("UpdateRelationships", connection) {
            CommandType         = CommandType.StoredProcedure
          };

          foreach (var targetTopic in topic.Relationships.GetValues(key)) {
            if (!targetTopic.IsNew) {
              targetIds.AddRow(targetTopic.Id);
            }
          }

          // Add Parameters
          command.AddParameter("TopicID", topic.Id.ToString(CultureInfo.InvariantCulture));
          command.AddParameter("RelationshipKey", key);
          command.AddParameter("RelatedTopics", targetIds);
          command.AddParameter("Version", version);
          command.AddParameter("DeleteUnmatched", topic.Relationships.IsFullyLoaded);

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
      | Return
      \-----------------------------------------------------------------------------------------------------------------------*/
      return;

    }

    /*==========================================================================================================================
    | METHOD: PERSIST REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Internal method that saves topic references to the 1:n mapping table in SQL.
    /// </summary>
    /// <param name="topic">The topic object whose references should be persisted.</param>
    /// <param name="version">The version that should be associated with the updated value.</param>
    /// <param name="connection">The SQL connection.</param>
    private static void PersistReferences(Topic topic, DateTime version, SqlConnection connection) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Persist relations to database
      \-----------------------------------------------------------------------------------------------------------------------*/
      try {

        using var references    = new TopicReferencesDataTable();
        using var command       = new SqlCommand("UpdateReferences", connection) {
          CommandType           = CommandType.StoredProcedure
        };

        foreach (var relatedTopic in topic.References) {
          if (!relatedTopic.Value?.IsNew?? false) {
            references.AddRow(relatedTopic.Key, relatedTopic.Value!.Id);
          }
        }

        // Add Parameters
        command.AddParameter("TopicID", topic.Id.ToString(CultureInfo.InvariantCulture));
        command.AddParameter("ReferencedTopics", references);
        command.AddParameter("Version", version);
        command.AddParameter("DeleteUnmatched", topic.References.IsFullyLoaded);

        command.ExecuteNonQuery();

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Catch exception
      \-----------------------------------------------------------------------------------------------------------------------*/
      catch (SqlException exception) {
        throw new TopicRepositoryException(
          $"Failed to persist references for Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
          exception
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return
      \-----------------------------------------------------------------------------------------------------------------------*/
      return;

    }

  } //Class
} //Namespace