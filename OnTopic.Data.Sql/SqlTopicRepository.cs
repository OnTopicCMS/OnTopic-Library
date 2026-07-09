/*==============================================================================================================================
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

namespace OnTopic.Data.Sql;

/*==============================================================================================================================
| CLASS: SQL TOPIC DATA REPOSITORY
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides data access to topics stored in Microsoft SQL Server.
/// </summary>
/// <remarks>
///   Concrete implementation of the <see cref="ITopicRepository"/> class.
/// </remarks>
public class SqlTopicRepository : TopicRepository, ITopicRepository, ITopicLoadResolver {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  private readonly              string                          _connectionString;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Instantiates a new instance of the SqlTopicRepository with a dependency on a connection string to provide necessary
  ///   access to a SQL database.
  /// </summary>
  /// <param name="connectionString">A connection string to a SQL server that contains the Topics database.</param>
  /// <returns>A new instance of the SqlTopicRepository.</returns>
  public SqlTopicRepository(string connectionString) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(connectionString), nameof(connectionString));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set private fields
    \-------------------------------------------------------------------------------------------------------------------------*/
    _connectionString           = connectionString;

  }

  /*============================================================================================================================
  | METHOD: LOAD
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public override async Task<Topic?> Load(
    string uniqueKey,
    Topic? referenceTopic       = null,
    bool isRecursive            = true,
    TopicPayload payload        = TopicPayload.All
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(uniqueKey, nameof(uniqueKey));
    Contract.Requires<TopicNotFoundException>(uniqueKey.Length > 0, nameof(uniqueKey));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetTopicID", connection);

    var topicId                 = -1;

    command.CommandType         = CommandType.StoredProcedure;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("UniqueKey", uniqueKey);
    command.AddOutputParameter();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {

      await connection.OpenAsync().ConfigureAwait(false);
      await command.ExecuteNonQueryAsync().ConfigureAwait(false);

      topicId                   = command.GetReturnCode();

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topic(s) failed to load: '{exception.Message}'", exception);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate results
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topicId < 0) {
      throw new TopicNotFoundException(uniqueKey);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    return await Load(topicId, referenceTopic, isRecursive, payload).ConfigureAwait(false);

  }

  /// <inheritdoc />
  public override async Task<Topic?> Load(
    int topicId,
    Topic? referenceTopic       = null,
    bool isRecursive            = true,
    TopicPayload payload        = TopicPayload.All
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = (Topic?)null;

    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetTopics", connection) {
      CommandType               = CommandType.StoredProcedure,
      CommandTimeout            = 120
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("TopicID", topicId);
    command.AddParameter("LoadDescendants", isRecursive);
    command.AddParameter("LoadAscendants", topicId >= 0);
    command.AddParameter("IncludeExtended", payload.HasFlag(TopicPayload.ExtendedAttributes));
    command.AddParameter("IncludeRelationships", true);
    command.AddParameter("IncludeReferences", true);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      using var reader          = (SqlDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
      topic                     = await reader.LoadTopicGraphAsync(topicId, referenceTopic, false).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate results
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null) {
      if (topicId == -1) {
        topic                   = TopicFactory.Create("Root", "Container");
      }
      else {
        throw new TopicNotFoundException(topicId);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish content type cache
    >-------------------------------------------------------------------------------------------------------------------------
    | If this load represents the entire topic graph, then relay the content type configuration to the TopicRepositoryBase in
    | order to either update or establish the content type cache. Not only does this prevent the need for a separate redundant
    | call later but, even more importantly, it helps ensure the same object references are maintained so that any updates to
    | subsequently cached content types are available.
    \-------------------------------------------------------------------------------------------------------------------------*/
    base.SetContentTypeDescriptors(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Stamp resolver
    \-------------------------------------------------------------------------------------------------------------------------*/
    StampResolver(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Raise event
    \-------------------------------------------------------------------------------------------------------------------------*/
    OnTopicLoaded(new(topic, isRecursive));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return objects
    \-------------------------------------------------------------------------------------------------------------------------*/
    return topic;

  }

  /// <inheritdoc />
  public override async Task<Topic?> Load(int topicId, DateTime version, Topic? referenceTopic = null) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Normalize parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    version                     = NormalizeToUtc(version);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(version.Date < DateTime.UtcNow, "The version requested must be a valid historical date.");
    Contract.Requires(
      version.Date >= new DateTime(2014, 12, 9),
      "The version is expected to have been created since version support was introduced into the topic library."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Clear associations
    >-------------------------------------------------------------------------------------------------------------------------
    | Because we don't (currently) track version as part of the .NET data model for relationships or topic references, there's
    | no easy way to determine if an association should be deleted when doing a rollback. As such, existing associations
    | should be deleted, assuming a `referenceTopic` is passed, and it contains the `topicId`.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = (Topic?)null;

    if (referenceTopic?.Id == topicId) {
      topic                     = referenceTopic;
    }
    else if (referenceTopic is  not null) {
      topic                     = referenceTopic.GetRootTopic().FindFirst(t => t.Id == topicId);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetTopicVersion", connection) {
      CommandType               = CommandType.StoredProcedure,
      CommandTimeout            = 120
    };

    command.CommandType         = CommandType.StoredProcedure;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("TopicID", topicId);
    command.AddParameter("Version", version);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      using var reader          = (SqlDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);

      // Clear existing associations before repopulating from the historical version
      if (topic is not null) {
        var rawExisting         = (ITopicBackingAccessor)topic;
        foreach (var relationship in rawExisting.Relationships) {
          rawExisting.Relationships.Clear(relationship.Key);
        }
        rawExisting.Relationships.Deferred.Clear();
        rawExisting.References.Deferred.Clear();
        rawExisting.References.Clear();
      }

      // Load the historical version into the current topic graph
      topic                     = await reader.LoadTopicGraphAsync(
        topicId,
        referenceTopic,
        includeExternalReferences: referenceTopic is not null
      ).ConfigureAwait(false);

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topics failed to load: '{exception.Message}'", exception);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate result
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null) {
      throw new TopicNotFoundException(topicId);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Delete orphaned attributes
    >-------------------------------------------------------------------------------------------------------------------------
    | If a referenceTopic is passed, and it contains the `topicId`, then that instance will be updated with the previous
    | version. In that case, however, any attributes which were first introduced after that version won't be overwritten.
    | That's because there isn't a previous value associated with that key to overwrite the current value. In those cases,
    | those attributes must be manually removed.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rawTopic                = (ITopicBackingAccessor)topic;
    var orphanedAttributes      = rawTopic.Attributes.Where(a => a.LastModified > version).ToList();

    foreach (var attribute in orphanedAttributes) {
      rawTopic.Attributes.Remove(attribute.Key);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Stamp resolver
    \-------------------------------------------------------------------------------------------------------------------------*/
    StampResolver(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Raise event
    \-------------------------------------------------------------------------------------------------------------------------*/
    OnTopicLoaded(new(topic, false, version));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return objects
    \-------------------------------------------------------------------------------------------------------------------------*/
    return topic;

  }

  /*============================================================================================================================
  | METHOD: REFRESH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public override async Task Refresh(Topic referenceTopic, DateTime since) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Normalize parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    since                       = NormalizeToUtc(since);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(referenceTopic, "A referenceTopic from the topic graph must be provided.");
    Contract.Requires(
      since.Date >= DateTime.UtcNow.AddHours(-24),
      "The since date is expected to be within the last twenty four hours."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetTopicUpdates", connection) {
      CommandType               = CommandType.StoredProcedure,
      CommandTimeout            = 120
    };

    command.CommandType         = CommandType.StoredProcedure;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("Since", since);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      using var reader          = (SqlDataReader)await command.ExecuteReaderAsync().ConfigureAwait(false);
      await reader.LoadTopicGraphAsync(-1, referenceTopic.GetRootTopic(), false).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topics failed to update: '{exception.Message}'", exception);
    }

  }

  /*============================================================================================================================
  | METHODS: TOPIC LOAD RESOLVER
  \---------------------------------------------------------------------------------------------------------------------------*/

  /// <inheritdoc />
  public virtual async Task EnsureLoaded(Topic topic, TopicPayload payload, CancellationToken cancellationToken = default) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Skip for new topics, as there's no persistent data to fetch
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic.IsNew) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Filter to pending (not yet Loaded) payload
    \-------------------------------------------------------------------------------------------------------------------------*/
    payload                     = topic.FilterPayload(payload);

    if (payload is TopicPayload.None) {
      return;
    }

    // Relationships and References themselves not by SqlTopicRepository; exit early if that's all that's pending so we don't
    // open a database connection unnecessarily
    if (!payload.HasFlag(TopicPayload.Children) && !payload.HasFlag(TopicPayload.ExtendedAttributes)) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("GetTopics", connection) {
      CommandType               = CommandType.StoredProcedure
    };

    // Set the stored procedure parameters based on the TopicPayload enum values
    AddEnsureLoadedParameters(command, topic.Id, payload);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    >---------------------------------------------------------------------------------------------------------------------------
    | Use the full live graph as the topic index so already-resident relationship targets are found without extra round-trips.
    | When filling Children, associations for the parent/seed topic are re-fetched alongside the children's; stale deferred
    | entries are cleared before processing to prevent duplicates from accumulating in the Deferred collection.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topics                  = topic.GetRootTopic().GetTopicIndex();
    var rawTopic                = (ITopicBackingAccessor)topic;

    try {

      // Setup
      await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
      using var reader          = (SqlDataReader)await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

      // Children: Fill first result set; FillChildrenAsync() sets each child's Children.LoadState and marks the parent Loaded
      if (payload.HasFlag(TopicPayload.Children)) {
        await reader.FillChildrenAsync(topic, topics, cancellationToken).ConfigureAwait(false);
      }

      // Otherwise, skip the first result set since the topic is already resident
      else {
        await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
      }

      // Indexed attributes
      await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
        reader.SetIndexedAttributes(topics, markDirty: false);
      }

      // Extended attributes
      await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
        reader.SetExtendedAttributes(topics, markDirty: false, preserveDirty: true);
      }

      // Clear stale deferred entries on the parent/seed topic before its associations are re-processed alongside children
      if (payload.HasFlag(TopicPayload.Children)) {
        rawTopic.Relationships.Deferred.Clear();
        rawTopic.References.Deferred.Clear();
      }

      // Relationships
      await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
        reader.SetRelationships(topics, markDirty: false);
      }

      // References
      await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
        reader.SetReferences(topics, markDirty: false);
      }

    }
    catch (SqlException exception) {
      throw new TopicRepositoryException($"Topic payload failed to load: '{exception.Message}'", exception);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Mark confirmed payload as Loaded
    >---------------------------------------------------------------------------------------------------------------------------
    | Children is excluded: Its LoadState is set inside FillChildren() after a successful fill. Relationships and References
    | are computed from Deferred.Count and require no explicit assignment here. Only Extended Attributes needs to be set.
    \-------------------------------------------------------------------------------------------------------------------------*/
    topic.SetLoadState(payload & TopicPayload.ExtendedAttributes, LoadState.Loaded);

  }

  /*============================================================================================================================
  | METHOD: SAVE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  protected override sealed async Task SaveTopic(
    [NotNull]Topic topic,
    DateTime version,
    bool persistRelationships
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Define variables
    \-------------------------------------------------------------------------------------------------------------------------*/
    var rawTopic                = (ITopicBackingAccessor)topic;
    var isTopicDirty            = topic.IsDirty();
    var areRelationshipsDirty   = rawTopic.Relationships.IsDirty();
    var areReferencesDirty      = rawTopic.References.IsDirty();
    var areAttributesDirty      = rawTopic.Attributes.IsDirty(true);
    var extendedBoundaryLoaded  = rawTopic.Attributes.LoadState is LoadState.Loaded;
    var extendedAttributeList   = GetAttributes(topic, isExtendedAttribute: true).ToList();
    var indexedAttributeList    = GetAttributes(
      topic                     : topic,
      isExtendedAttribute       : false,
      isDirty                   : true,
      excludeLastModified       : !areAttributesDirty
    ).ToList();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Ensure extended attribute blob is available before save
    >-------------------------------------------------------------------------------------------------------------------------
    | If the extended attribute boundary is NotLoaded and at least one extended attribute is dirty, call EnsureLoaded first so
    | we write a complete snapshot rather than a partial one. When the boundary is NotLoaded and no extended attrs are dirty,
    | @ExtendedAttributes is omitted (NULL), leaving the persisted blob untouched (UpdateTopic guards on IS NOT NULL).
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!extendedBoundaryLoaded) {
      if (extendedAttributeList.Any(a => a.IsDirty)) {
        await EnsureLoaded(topic, TopicPayload.ExtendedAttributes).ConfigureAwait(false);
        extendedBoundaryLoaded  = true;
        extendedAttributeList   = GetAttributes(topic, isExtendedAttribute: true).ToList();
      }
      else {
        extendedAttributeList   = [];
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Detect whether anything has changed
    >-------------------------------------------------------------------------------------------------------------------------
    | If no associations have changed, and no attributes values have changed, and there aren't any mismatched attributes in
    | their respective lists, then there isn't anything new to persist to the database, and thus no benefit to executing the
    | current command. A more aggressive version of this would wrap much of the below logic in this, but this is just meant
    | as a quick fix to reduce the overhead of recursive saves.
    \-------------------------------------------------------------------------------------------------------------------------*/
    areAttributesDirty          =
      areAttributesDirty        ||
      indexedAttributeList.Any(a => a.IsExtendedAttribute == true) ||
      extendedAttributeList.Any(a => a.IsExtendedAttribute == false);

    var isDirty                 =
      isTopicDirty              ||
      areRelationshipsDirty     ||
      areReferencesDirty        ||
      areAttributesDirty;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Bypass is not dirty
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!isDirty) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Add indexed attributes that are dirty
    >-------------------------------------------------------------------------------------------------------------------------
    | Loop through the content type's supported attributes and add attribute to null attributes if topic does not contain it.
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var attributeValues   = new AttributeValuesDataTable();

    if (areAttributesDirty) {

      foreach (var attributeValue in indexedAttributeList) {
        attributeValues.AddRow(attributeValue.Key, attributeValue.Value);
      }

      foreach (var attribute in GetUnmatchedAttributes(topic)) {
        attributeValues.AddRow(attribute.Key);
      }

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Add extended attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var extendedAttributes      = (StringBuilder?)null;

    if (areAttributesDirty && extendedBoundaryLoaded) {

      extendedAttributes        = new();
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

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    var procedureName           = topic.IsNew? "CreateTopic" : "UpdateTopic";

    await connection.OpenAsync().ConfigureAwait(false);

    using var command           = new SqlCommand(procedureName, connection) {
      CommandType               = CommandType.StoredProcedure
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
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
      if (extendedAttributes is not null) {
        command.AddParameter("ExtendedAttributes", extendedAttributes);
      }
    }
    command.AddOutputParameter();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {

      if (topic.IsNew || isTopicDirty || areAttributesDirty) {
        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        topic.Id                = command.GetReturnCode();
      }

      Contract.Assume(
        !topic.IsNew,
        "The call to the CreateTopic stored procedure did not return the expected 'Id' parameter."
      );

      if (persistRelationships  && areRelationshipsDirty) {
        await PersistRelationships(topic, version, connection).ConfigureAwait(false);
      }

      if (persistRelationships  && areReferencesDirty) {
        await PersistReferences(topic, version, connection).ConfigureAwait(false);
      }

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException(
        $"Failed to save Topic '{topic.Key}' ({topic.Id}) via '{_connectionString}': '{exception.Message}'",
        exception
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Close connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    finally {
      connection.Close();
    }

  }

  /*============================================================================================================================
  | METHOD: MOVE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override sealed async Task MoveTopic(Topic topic, Topic target, Topic? sibling) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));
    Contract.Requires(target, nameof(target));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish database connection
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("MoveTopic", connection) {
      CommandType               = CommandType.StoredProcedure
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("TopicID", topic.Id);
    command.AddParameter("ParentID", target.Id);

    // Append sibling ID if set
    if (sibling is not null) {
      command.AddParameter("SiblingID", sibling.Id);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException(
        $"Failed to move Topic '{topic.Key}' ({topic.Id}) to '{target.Key}' ({target.Id}): '{exception.Message}'",
        exception
      );
    }

  }

  /*============================================================================================================================
  | METHOD: DELETE TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  protected override sealed async Task DeleteTopic(Topic topic) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topic, nameof(topic));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Delete from database
    \-------------------------------------------------------------------------------------------------------------------------*/
    using var connection        = new SqlConnection(_connectionString);
    using var command           = new SqlCommand("DeleteTopic", connection) {
      CommandType               = CommandType.StoredProcedure
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish query parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    command.AddParameter("TopicID", topic.Id);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process database query
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {
      await connection.OpenAsync().ConfigureAwait(false);
      await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException(
        $"Failed to delete Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
        exception
      );
    }

  }

  /*============================================================================================================================
  | METHOD: ADD ENSURE LOADED PARAMETERS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Configures a <see cref="SqlCommand"/> targeting <c>GetTopics</c> for use by the <see cref="ITopicLoadResolver"/>,
  ///   setting the payload parameters based on the requested <paramref name="payload"/>.
  /// </summary>
  /// <remarks>
  ///   Scope is always <c>None</c> (i.e., a single node) for resolver fills, as the caller is already in the graph. <see
  ///   cref="TopicPayload.History"/> is hardcoded to <c>false</c> here because its fill path is not yet implemented; once
  ///   it is, this method will map it from the <paramref name="payload"/> flag. Indexed attributes and associations are only
  ///   requested when filling the <see cref="TopicPayload.Children"/> boundary, as they are otherwise always loaded as part of
  ///   the initial <see cref="Load(int, Topic, bool, TopicPayload)"/> for existing topics.
  /// </remarks>
  private static void AddEnsureLoadedParameters(SqlCommand command, int topicId, TopicPayload payload) {

    // Set the topic we're working with
    command.AddParameter("TopicID",                             topicId);

    // Scope: LoadChildren when filling the Children, otherwise we're only interested in this topic's content
    command.AddParameter("LoadDescendants",                     false);
    command.AddParameter("LoadAscendants",                      false);
    command.AddParameter("LoadChildren",                        payload.HasFlag(TopicPayload.Children));

    // Payload: Include only what the requested payload requires; relationships and references are loaded during the initial
    // Load() call, so they do not need to be re-fetched
    command.AddParameter("IncludeIndexed",                      payload.HasFlag(TopicPayload.Children));
    command.AddParameter("IncludeExtended",                     payload.HasFlag(TopicPayload.ExtendedAttributes));
    command.AddParameter("IncludeRelationships",                payload.HasFlag(TopicPayload.Children));
    command.AddParameter("IncludeReferences",                   payload.HasFlag(TopicPayload.Children));
    command.AddParameter("IncludeHistory",                      false);

  }

  /*============================================================================================================================
  | METHOD: PERSIST RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Internal method that saves topic relationships to the n:n mapping table in SQL.
  /// </summary>
  /// <param name="topic">The topic object whose relationships should be persisted.</param>
  /// <param name="version">The version that should be associated with the updated value.</param>
  /// <param name="connection">The SQL connection.</param>
  private static async Task PersistRelationships(Topic topic, DateTime version, SqlConnection connection) {

    var rawTopic                = (ITopicBackingAccessor)topic;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return blank if the topic has no relations.
    \-------------------------------------------------------------------------------------------------------------------------*/
    // return if the topic has no relations
    if (rawTopic.Relationships.Keys.Count == 0) {
      return;
    }

    try {

      /*------------------------------------------------------------------------------------------------------------------------
      | Iterate through each scope and persist to SQL
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var key in rawTopic.Relationships.Keys) {

        using var targetIds     = new TopicListDataTable();
        using var command       = new SqlCommand("UpdateRelationships", connection) {
          CommandType           = CommandType.StoredProcedure
        };

        foreach (var targetTopic in rawTopic.Relationships.GetValues(key)) {
          if (!targetTopic.IsNew) {
            targetIds.AddRow(targetTopic.Id);
          }
        }

        // Add Parameters
        command.AddParameter("TopicID", topic.Id.ToString(CultureInfo.InvariantCulture));
        command.AddParameter("RelationshipKey", key);
        command.AddParameter("RelatedTopics", targetIds);
        command.AddParameter("Version", version);
        command.AddParameter("DeleteUnmatched", rawTopic.Relationships.LoadState is LoadState.Loaded);

        await command.ExecuteNonQueryAsync().ConfigureAwait(false);

      }

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException(
        $"Failed to persist relationships for Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
        exception
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return
    \-------------------------------------------------------------------------------------------------------------------------*/
    return;

  }

  /*============================================================================================================================
  | METHOD: PERSIST REFERENCES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Internal method that saves topic references to the 1:n mapping table in SQL.
  /// </summary>
  /// <param name="topic">The topic object whose references should be persisted.</param>
  /// <param name="version">The version that should be associated with the updated value.</param>
  /// <param name="connection">The SQL connection.</param>
  private static async Task PersistReferences(Topic topic, DateTime version, SqlConnection connection) {

    var rawTopic                = (ITopicBackingAccessor)topic;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Persist relations to database
    \-------------------------------------------------------------------------------------------------------------------------*/
    try {

      using var references      = new TopicReferencesDataTable();
      using var command         = new SqlCommand("UpdateReferences", connection) {
        CommandType             = CommandType.StoredProcedure
      };

      foreach (var relatedTopic in rawTopic.References) {
        if (!relatedTopic.Value?.IsNew?? false) {
          references.AddRow(relatedTopic.Key, relatedTopic.Value!.Id);
        }
      }

      // Add Parameters
      command.AddParameter("TopicID", topic.Id.ToString(CultureInfo.InvariantCulture));
      command.AddParameter("ReferencedTopics", references);
      command.AddParameter("Version", version);
      command.AddParameter("DeleteUnmatched", rawTopic.References.LoadState is LoadState.Loaded);

      await command.ExecuteNonQueryAsync().ConfigureAwait(false);

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Catch exception
    \-------------------------------------------------------------------------------------------------------------------------*/
    catch (SqlException exception) {
      throw new TopicRepositoryException(
        $"Failed to persist references for Topic '{topic.Key}' ({topic.Id}): '{exception.Message}'",
        exception
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return
    \-------------------------------------------------------------------------------------------------------------------------*/
    return;

  }

} //Class