/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Diagnostics;
using System.Net;
using OnTopic.Collections.Specialized;
using OnTopic.Querying;
using OnTopic.Repositories;

namespace OnTopic.Data.Sql;

/*==============================================================================================================================
| CLASS: SQL DATA READER EXTENSIONS
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Extension methods for the <see cref="IDataReader"/> class.
/// </summary>
/// <remarks>
///   Most of the extensions are optimized for reading the data returned from the <c>GetTopics</c> and <c>GetTopicVersion</c>
///   stored procedures, which require parsing multiple data sets in order to populate a topic graph. For that purpose, the
///   main entry point is <see cref="LoadTopicGraph"/>. It is supported by a number of <c>private</c> extensions which allow
///   it to handle individual records from particular data sets (e.g., the <see cref="SetExtendedAttributes"/> method maps to
///   data returned from the <c>ExtendedAttributeIndex</c> view). That said, the <c>Get</c> extensions (e.g., <see
///   cref="GetString(IDataReader, String)"/>) are not specific to this format, and remain useful for a variety of database
///   queries, should they be needed, and thus are marked as <c>internal</c>.
/// </remarks>
internal static class SqlDataReaderExtensions {

  /*============================================================================================================================
  | METHOD: LOAD TOPIC GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <see cref="IDataReader"/> from a call to the <c>GetTopics</c> stored procedure, will extract a list of
  ///   topics and populate their attributes, associations, and children.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> with output from the <c>GetTopics</c> stored procedure.</param>
  /// <param name="seedTopicId">
  ///   The <see cref="Topic.Id"/> that was passed to the underlying query (i.e., the root of the requested subtree or the topic
  ///   whose ancestors were requested). Used to identify which newly loaded topics are ancestors so that their <see cref=
  ///   "Topic.Children"/> can be correctly stamped as <see cref="LoadState.NotLoaded"/>. The default is <c>-1</c>, which
  ///   applies when the root was loaded, or when no single seed applies (e.g., the <c>GetTopicUpdates</c> path).
  /// </param>
  /// <param name="referenceTopic">
  ///   When loading a single topic or branch, offers a reference topic graph that can be used to ensure that topic
  ///   associations—such as references, relationships, and <see cref="Topic.Parent"/>—are integrated with existing entities.
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
  /*============================================================================================================================
  | METHOD: LOAD TOPIC GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  internal static async Task<Topic?> LoadTopicGraph(
    this DbDataReader reader,
    int seedTopicId             = -1,
    Topic? referenceTopic       = null,
    bool? markDirty             = null,
    bool includeExternalReferences = true,
    CancellationToken cancellationToken = default
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish topic index
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topics                  = referenceTopic is not null? referenceTopic.GetRootTopic().GetTopicIndex() : new();
    var rootTopic               = (Topic?)null;
    HashSet<int> preExistingIds = [..topics.Keys];
    var hasChildrenMap          = new Dictionary<int, bool>();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Populate topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): AddTopic() [" + DateTime.Now + "]");
    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {

      // Add the topic to the topic graph
      var addedTopic            = reader.AddTopic(topics, markDirty);

      // The first topic returned is the root topic; store it for the return value
      rootTopic                 ??= addedTopic;

      var rawTopic              = (ITopicBackingAccessor)addedTopic;

      // HasExtendedAttribute is NULL when extended attributes are included: Converge to Loaded, even on a pre-existing topic
      // whose extended boundary was previously NotLoaded, and even if the topic has no extended attributes to read.
      // HasExtendedAttribute is true when the blob wasn't loaded, but exists: Downgrade to NotLoaded, deferring the fetch.
      // HasExtendedAttribute is false when the topic has no extended attributes at all: nothing to defer, so Loaded.
      rawTopic.Attributes.LoadState = reader.GetNullableBoolean("HasExtendedAttributes") is true?
        LoadState.NotLoaded :
        LoadState.Loaded;

      // HasChildren is NULL when the column is not applicable (e.g., in version or update paths); skip those topics.
      // Pre-existing topics are excluded; their LoadState is already established, and they may have the lazy resolver wired up.
      if (!preExistingIds.Contains(addedTopic.Id) && reader.GetNullableBoolean("HasChildren") is { } hasChildren) {
        hasChildrenMap[addedTopic.Id] = hasChildren;
      }

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Stamp Children.LoadState
    \-------------------------------------------------------------------------------------------------------------------------*/
    // Identifies the ancestor tree, stopping at the first pre-existing (i.e., not newly loaded) topic. Newly introduced
    // ancestors have exactly one child loaded (from the ancestor crawl), but may have more; as such, they will be marked as
    // NotLoaded. Note: An ancestor whose sole database child is part of the ancestor chain is still marked NotLoaded, since we
    // don't have enough information to verify that. This is an unlikely scenario, but will cost one extra round-trip to verify.
    HashSet<int> ancestorIds    = [];
    if (topics.TryGetValue(seedTopicId, out var seedTopic)) {
      var ancestor              = seedTopic.Parent;
      while (ancestor is not null && hasChildrenMap.ContainsKey(ancestor.Id)) {
        ancestorIds.Add(ancestor.Id);
        ancestor                = ancestor.Parent;
      }
    }

    // HasChildren NULL (i.e., absent from the map) means the topic was not newly loaded or the column is not applicable;
    // either way, skip. Ancestors with children are NotLoaded (partial load); other topics check whether any children were
    // loaded, implying that @HasChildren or @LoadDescendants was passed, and thus its children are fully loaded.
    foreach (var (id, hasChildren) in hasChildrenMap) {
      var topic                 = topics[id];
      var isNotLoaded           = hasChildren && (ancestorIds.Contains(id) || topic.Children.Count == 0);
      topic.Children.LoadState  = isNotLoaded? LoadState.NotLoaded : LoadState.Loaded;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Read attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): SetIndexedAttributes() [" + DateTime.Now + "]");

    // Move to TopicAttributes dataset
    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
      reader.SetIndexedAttributes(topics, markDirty);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Read extended attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): SetExtendedAttributes() [" + DateTime.Now + "]");

    // Move to extended attributes dataset
    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

    // Loop through each extended attribute record associated with a specific topic
    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
      (reader as SqlDataReader)?.SetExtendedAttributes(topics, markDirty);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Read related items
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): SetRelationships() [" + DateTime.Now + "]");

    // Move to the relationships dataset
    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

    // Loop through each relationship; multiple records may exist per topic
    if (includeExternalReferences) {
      while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
        reader.SetRelationships(topics, markDirty);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Read referenced items
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): SetReferences() [" + DateTime.Now + "]");

    // Move to the version history dataset
    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

    // Loop through each version; multiple records may exist per topic
    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
      reader.SetReferences(topics, markDirty);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Read version history
    \-------------------------------------------------------------------------------------------------------------------------*/
    Debug.WriteLine("SqlTopicRepository.Load(): SetVersionHistory() [" + DateTime.Now + "]");

    // Move to the version history dataset
    await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);

    // Loop through each version; multiple records may exist per topic
    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
      reader.SetVersionHistory(topics);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return objects
    \-------------------------------------------------------------------------------------------------------------------------*/
    return seedTopicId >= 0 && topics.TryGetValue(seedTopicId, out var requestedTopic)
      ? requestedTopic
      : rootTopic;

  }

  /*============================================================================================================================
  | METHOD: ADD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  private static Topic AddTopic(this IDataReader reader, TopicIndex topics, bool? markDirty) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicId                 = reader.GetTopicId();
    var key                     = reader.GetString("TopicKey");
    var contentType             = reader.GetString("ContentType");
    var parentId                = reader.GetInteger("ParentID");
    var wasDirty                = false;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!topics.TryGetValue(topicId, out var current)) {
      current                   = TopicFactory.Create(key, contentType, topicId);
      topics.Add(current.Id, current);
      // Default to NotLoaded; a corresponding row in the version history dataset, if any, promotes this to Loaded
      ((ITopicBackingAccessor)current).VersionHistory.LoadState = LoadState.NotLoaded;
    }
    else {
      wasDirty                  = current.IsDirty();
      current.Key               = key;
      current.ContentType       = contentType;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Assign parent
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (parentId >= 0 && current.Parent?.Id != parentId && topics.TryGetValue(parentId, out var parentTopic)) {
      current.Parent            = parentTopic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Mark clean
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (wasDirty is false && markDirty is false) {
      current.MarkClean();
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return the topic created
    \-------------------------------------------------------------------------------------------------------------------------*/
    return current;

  }

  /*============================================================================================================================
  | METHOD: FILL CHILDREN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Reads the children result set (the first result set) from a <c>GetTopics</c> response, adds each child to the <paramref
  ///   name="topics"/> index via <see cref="AddChildTopic"/>, and marks the <paramref name="parent"/>'s <c>Children</c> as <see
  ///   cref="LoadState.Loaded"/> after a successful fill.
  /// </summary>
  /// <param name="reader">
  ///   The <see cref="IDataReader"/>, positioned at the first result set of the <c>GetTopics</c> response.
  /// </param>
  /// <param name="parent">The topic whose immediate children are being loaded.</param>
  /// <param name="topics">The <see cref="TopicIndex"/> to populate with the new child topics.</param>
  internal static async Task FillChildren(
    this DbDataReader reader,
    Topic parent,
    TopicIndex topics,
    CancellationToken cancellationToken
  ) {

    // Loop through each record, delegating to the shared AddChildTopic()
    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
      reader.AddChildTopic(parent, topics);
    }

    // Mark confirmed children payload as Loaded
    ((ITopicLazyLoadable)parent).SetLoadState(TopicPayload.Children, LoadState.Loaded);

  }

  /*============================================================================================================================
  | METHOD: SET INDEXED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  internal static void SetIndexedAttributes(this IDataReader reader, TopicIndex topics, bool? markDirty) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicId                 = reader.GetTopicId();
    var attributeKey            = reader.GetString("AttributeKey");
    var attributeValue          = reader.GetNullableString("AttributeValue");
    var version                 = reader.GetVersion();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    var current                 = topics[topicId];
    var rawTopic                = (ITopicBackingAccessor)current;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set attribute value
    \-------------------------------------------------------------------------------------------------------------------------*/
    rawTopic.Attributes.SetValue(attributeKey, attributeValue, markDirty, version, false);

  }

  /*============================================================================================================================
  | METHOD: SET EXTENDED ATTRIBUTES
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  /// <param name="preserveDirty">
  ///   When <c>true</c>, skips any attribute key whose in-memory record is already dirty. This is used by the lazy-load
  ///   resolver so that a value set by the call while the extended boundary was <see cref="LoadState.NotLoaded"/> is not
  ///   silently overwritten by the blob merge.
  /// </param>
  internal static void SetExtendedAttributes(
    this SqlDataReader reader,
    TopicIndex topics,
    bool? markDirty,
    bool preserveDirty          = false
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicId                 = reader.GetTopicId();
    var version                 = reader.GetVersion();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Load SQL XML into XmlDocument
    \-------------------------------------------------------------------------------------------------------------------------*/
    var xmlData                 = reader.GetSqlXml(1);
    var xmlReader               = xmlData.CreateReader();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify the current topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    var current                 = topics[topicId];
    var rawTopic                = (ITopicBackingAccessor)current;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle scenario where there isn't an <attribute /> element
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!xmlReader.ReadToFollowing("attribute")) return;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Loop through nodes to set attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    do {

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeKey          = (string?)xmlReader.GetAttribute("key");
      var attributeValue        = WebUtility.HtmlDecode(xmlReader.ReadInnerXml());

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate assumptions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        attributeKey,
        $"The @key attribute of the <attribute /> element is missing for Topic '{topicId}'; the data is not in the " +
        $"expected format."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Set attribute value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) continue;

      // Skip keys already dirty in memory to avoid clobbering unsaved values during a lazy fill
      if (preserveDirty && rawTopic.Attributes.IsDirty(attributeKey)) continue;

      rawTopic.Attributes.SetValue(attributeKey, attributeValue, markDirty, version, true);

    } while (xmlReader.Name is  "attribute");

  }

  /*============================================================================================================================
  | METHOD: SET RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  internal static void SetRelationships(this IDataReader reader, TopicIndex topics, bool? markDirty = false) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourceTopicId           = reader.GetTopicId("Source_TopicID");
    var targetTopicId           = reader.GetTopicId("Target_TopicID");
    var relationshipKey         = reader.GetString("RelationshipKey");
    var isDeleted               = reader.GetBoolean("IsDeleted");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify affected topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    var current                 = topics[sourceTopicId];
    var rawTopic                = (ITopicBackingAccessor)current;
    var related                 = (Topic?)null;

    // Fetch the related topic
    if (topics.TryGetValue(targetTopicId, out var relatedTopic)) {
      related                   = relatedTopic;
    }

    // When the target is absent, defer it for resolution on next access
    if (related is null) {
      rawTopic.Relationships.Deferred.SetValue(relationshipKey, targetTopicId);
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set relationship on object
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!isDeleted) {
      rawTopic.Relationships.SetValue(relationshipKey, related, markDirty);
    }
    else if (rawTopic.Relationships.Contains(relationshipKey, related)) {
      rawTopic.Relationships.Remove(relationshipKey, related);
    }

  }

  /*============================================================================================================================
  | METHOD: SET REFERENCES
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Adds topic references to their associated topics.
  /// </summary>
  /// <remarks>
  ///   Topics can be cross-referenced with each other topics via a one-to-one associations. Once the topics are populated in
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
  internal static void SetReferences(this IDataReader reader, TopicIndex topics, bool? markDirty) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourceTopicId           = reader.GetTopicId("Source_TopicID");
    var referenceKey            = reader.GetString("ReferenceKey");
    var targetTopicId           = reader.GetNullableTopicId("Target_TopicID");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify affected topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    var current                 = topics[sourceTopicId];
    var rawTopic                = (ITopicBackingAccessor)current;
    var referenced              = (Topic?)null;

    // This happens when the reference has been deleted, so SetValue() will remove the reference
    if (targetTopicId is null) {
    }

    // Attempt to get a reference to the target via the topic index
    else if (topics.TryGetValue(targetTopicId.Value, out var referencedTopic)) {
      referenced                = referencedTopic;
    }

    // When the target isn't (yet) available, defer it to be lazy loaded when the references are accessed
    else {
      rawTopic.References.Deferred.SetValue(referenceKey, targetTopicId.Value);
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set reference on object
    \-------------------------------------------------------------------------------------------------------------------------*/
    rawTopic.References.SetValue(referenceKey, referenced, markDirty);

  }


  /*============================================================================================================================
  | METHOD: ADD CHILD TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Processes a single row from the children result set of a <c>GetTopics</c> response: Adds the child to the <paramref
  ///   name="topics"/> index via <see cref="AddTopic"/>, then stamps its <c>Attributes.LoadState</c> and <c>Children.LoadState
  ///   </c> based on the <c>HasExtendedAttributes</c> and <c>HasChildren</c> database hints. Returns <see langword="null"/>
  ///   when the row represents the <paramref name="parent"/> itself (which the stored procedure includes alongside its
  ///   children) so callers can skip it.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/>, positioned at a row in the children result set.</param>
  /// <param name="parent">The topic whose children are being loaded; rows matching this ID are skipped.</param>
  /// <param name="topics">The <see cref="TopicIndex"/> to populate.</param>
  private static Topic? AddChildTopic(this IDataReader reader, Topic parent, TopicIndex topics) {

    // Add or update the topic in the index
    var addedTopic              = reader.AddTopic(topics, markDirty: false);

    // Skip the parent record, which the stored procedure returns alongside its children
    if (addedTopic.Id == parent.Id) {
      return null;
    }

    var rawTopic                = (ITopicBackingAccessor)addedTopic;

    // Set the extended-attribute load state based on the database hint
    if (reader.GetNullableBoolean("HasExtendedAttributes") is true) {
      rawTopic.Attributes.LoadState = LoadState.NotLoaded;
    }

    // Set the children load state based on the database hint
    rawTopic.Children.LoadState = reader.GetNullableBoolean("HasChildren") is true
      ? LoadState.NotLoaded
      : LoadState.Loaded;

    // Return the topic created
    return addedTopic;

  }

  /*============================================================================================================================
  | METHOD: SET VERSION HISTORY
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  internal static void SetVersionHistory(this IDataReader reader, TopicIndex topics) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify attributes
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicId                 = reader.GetTopicId();
    var dateTime                = reader.GetVersion();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    var current                 = topics[topicId];
    var rawTopic                = (ITopicBackingAccessor)current;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set history
    >-------------------------------------------------------------------------------------------------------------------------
    | A row being present, regardless of its content, means version history was fetched for this topic; promote the state
    | to Loaded so subsequent access doesn't trigger a redundant fill.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!rawTopic.VersionHistory.Contains(dateTime)) {
      rawTopic.VersionHistory.Add(dateTime);
    }
    rawTopic.VersionHistory.LoadState = LoadState.Loaded;

  }

  /*============================================================================================================================
  | METHOD: GET STRING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a string value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static string GetString(this IDataReader reader, string columnName) =>
    reader.GetString(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET NULLABLE STRING
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a nullable string value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static string? GetNullableString(this IDataReader reader, string columnName) =>
    reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET BOOLEAN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a boolean value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static bool GetBoolean(this IDataReader reader, string columnName) =>
    reader.GetBoolean(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET NULLABLE BOOLEAN
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a nullable boolean value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static bool? GetNullableBoolean(this IDataReader reader, string columnName) =>
    reader.IsDBNull(reader.GetOrdinal(columnName))? null : reader.GetBoolean(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET INTEGER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves an integer value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static int GetInteger(this IDataReader reader, string columnName) =>
    Int32.TryParse(reader.GetValue(reader.GetOrdinal(columnName)).ToString(), out var output)? output : -1;

  /*============================================================================================================================
  | METHOD: GET TOPIC ID
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <see cref="Topic.Id"/> value by column name.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static int GetTopicId(this IDataReader reader, string columnName = "TopicID") =>
    reader.GetInt32(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET NULLABLE TOPIC ID
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves a <see cref="Topic.Id"/> value by column name, while accepting null values.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  /// <param name="columnName">The name of the column to retrieve the value from.</param>
  private static int? GetNullableTopicId(this IDataReader reader, string columnName = "TopicID") =>
    reader.IsDBNull(reader.GetOrdinal(columnName))? null : reader.GetInt32(reader.GetOrdinal(columnName));

  /*============================================================================================================================
  | METHOD: GET VERSION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves the version column, with precisions appropriate for setting the <see cref="Topic.VersionHistory"/>.
  /// </summary>
  /// <param name="reader">The <see cref="IDataReader"/> object.</param>
  private static DateTime GetVersion(this IDataReader reader) =>
    reader.GetDateTime(reader.GetOrdinal("Version"));

} //Class