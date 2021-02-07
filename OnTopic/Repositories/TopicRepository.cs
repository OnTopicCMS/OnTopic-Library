/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Querying;

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a base class for <see cref="ITopicRepository"/> which are responsible for persisting data.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     The <see cref="TopicRepository"/> is a highly opinionated base implementation of <see cref="ITopicRepository"/>.
  ///     In addition to validating parameters and raising events on <see cref="Save(Topic, Boolean)"/>, <see cref="Delete(
  ///     Topic, Boolean)"/>, and <see cref="Move(Topic, Topic, Topic?)"/>, it also provides a number of (protected) methods to
  ///     aid implementors in evaluating and parsing <see cref="Topic"/> data, such as <see cref="GetUnmatchedAttributes(Topic)
  ///     "/>. It is recommended that all concrete implementations of <see cref="ITopicRepository"/> that are responsible for
  ///     persisting data to a data store use this as a base class.
  ///   </para>
  ///   <para>
  ///     Implementations of <see cref="ITopicRepository"/> which need to use different business logic, or do not need to
  ///     implement business logic (such as unit test doubles) may instead opt to derive directly from the <see cref="
  ///     ObservableTopicRepository"/>, which handles the basic event handling, and nothing else. Implementations of decorators
  ///     should instead derive from the <see cref="TopicRepositoryDecorator"/>.
  ///   </para>
  /// </remarks>
  public abstract class TopicRepository : ObservableTopicRepository {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private readonly            ContentTypeDescriptorCollection _contentTypeDescriptors         = new();

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override ContentTypeDescriptorCollection GetContentTypeDescriptors() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize content types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypeDescriptors.Count == 0) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Load configuration data
        \---------------------------------------------------------------------------------------------------------------------*/
        var configuration       = (Topic?)null;

        try {
          configuration         = Load("Root:Configuration");
        }
        catch (TopicNotFoundException) {
          //Swallow missing configuration, as this is an expected condition when working with a new database
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Load root content type
        \---------------------------------------------------------------------------------------------------------------------*/
        var contentTypes        = configuration?.Children.GetTopic("ContentTypes") as ContentTypeDescriptor;

        /*----------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \---------------------------------------------------------------------------------------------------------------------*/
        _contentTypeDescriptors.Refresh(contentTypes);

      }

      return _contentTypeDescriptors;

    }

    /// <summary>
    ///   Optional overload of <see cref="GetContentTypeDescriptors()"/> allows for a new topic graph to be supplied for
    ///   updating the list of cached <see cref="ContentTypeDescriptor"/>s.
    /// </summary>
    /// <remarks>
    ///   By default, the <see cref="GetContentTypeDescriptors()"/> method will load data from the concrete implementation of
    ///   the <see cref="ITopicRepository"/>'s data store. There are cases, however, where it may be preferrable to instead load
    ///   these topics from a local, in-memory source. Namely, when first instantiating a new OnTopic database, and when saving
    ///   modifications to existing content types. As such, this <c>protected</c> overload is useful to call from <see
    ///   cref="ITopicRepository.Save(Topic, Boolean)"/> when the topic graph being saved includes any <see cref=
    ///   "ContentTypeDescriptor"/>s.
    /// </remarks>
    /// <param name="contentTypeDescriptors">
    ///   The root of a <see cref="ContentTypeDescriptor"/> topic graph to merge into the collection for <see
    ///   cref="GetContentTypeDescriptors()"/>. The code will process not only the root <see cref="ContentTypeDescriptor"/>, but
    ///   also any descendents.
    /// </param>
    /// <returns></returns>
    [Obsolete("Deprecated. Instead, use the new SetContentTypeDescriptors() method, which provides the same function.", true)]
    protected virtual ContentTypeDescriptorCollection GetContentTypeDescriptors(ContentTypeDescriptor? contentTypeDescriptors)
      => SetContentTypeDescriptors(contentTypeDescriptors);

    /*==========================================================================================================================
    | SET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Allows for the content type descriptor cache to be (re)initialized based on an in-memory topic graph, by first
    ///   identifying the root <see cref="ContentTypeDescriptor"/> from within the current content graph.
    /// </summary>
    /// <param name="sourceTopic">
    ///   A <see cref="Topic"/> within the source topic graph. This overload will use this topic to identify the root <see cref=
    ///   "ContentTypeDescriptor"/> within the graph.
    /// </param>
    /// <returns></returns>
    protected ContentTypeDescriptorCollection SetContentTypeDescriptors(Topic? sourceTopic) =>
      SetContentTypeDescriptors(sourceTopic?.GetByUniqueKey("Root:Configuration:ContentTypes") as ContentTypeDescriptor);

    /// <summary>
    ///   Allows for the content type descriptor cache to be (re)initialized based on an in-memory topic graph, starting with
    ///   the root <see cref="ContentTypeDescriptor"/>.
    /// </summary>
    /// <remarks>
    ///   By default, the <see cref="SetContentTypeDescriptors(ContentTypeDescriptor?)"/> method will load data from the
    ///   concrete implementation of the <see cref="ITopicRepository"/>'s data store. There are cases, however, where it may be
    ///   preferrable to instead load these topics from a local, in-memory source. Namely, when first instantiating a new
    ///   OnTopic database, and when saving modifications to existing content types. As such, the <c>protected</c> <see cref=
    ///   "SetContentTypeDescriptors(ContentTypeDescriptor?)"/> method is useful to call from <see cref="ITopicRepository.Save(
    ///   Topic, Boolean)"/> when the topic graph being saved includes any new <see cref="ContentTypeDescriptor"/>s.
    /// </remarks>
    /// <param name="rootContentType">
    ///   The root of a <see cref="ContentTypeDescriptor"/> topic graph to merge into the collection for <see
    ///   cref="SetContentTypeDescriptors(ContentTypeDescriptor?)"/>. The code will process not only the root <see cref=
    ///   "ContentTypeDescriptor"/>, but also any descendents.
    /// </param>
    /// <returns></returns>
    protected ContentTypeDescriptorCollection SetContentTypeDescriptors(ContentTypeDescriptor? rootContentType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish cache
      >-------------------------------------------------------------------------------------------------------------------------
      | Instead of attempting to merge the source collection with the cached collection, we'll just recreate the cached
      | collection based on the source collection. This is faster, and helps avoid potential versioning conflicts caused by
      | mixing objects from different topic graphs. Instead, we always assume that the source collection is the most accurate
      | and relevant.
      \-----------------------------------------------------------------------------------------------------------------------*/
      _contentTypeDescriptors.Refresh(rootContentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate cache
      >-------------------------------------------------------------------------------------------------------------------------
      | If the source cache collection is empty then we'll want to defer to the existing version�or retrieve it from the
      | persistence layer�via GetContentTypeDescriptors().
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypeDescriptors.Count == 0) {
        GetContentTypeDescriptors();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add available Content Types to the collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      return _contentTypeDescriptors;

    }

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to identify the <see cref="ContentTypeDescriptor"/> for the provided <paramref name="sourceTopic"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GetContentTypeDescriptor(Topic)"/> method will attempt to get the <see cref="ContentTypeDescriptor"/>
    ///   from the <see cref="GetContentTypeDescriptors()"/> method using the <paramref name="sourceTopic"/>'s <see
    ///   cref="Topic.ContentType"/>. If that can't be found, however, then it will instead look in the <paramref
    ///   name="sourceTopic"/>'s topic graph to see if the <see cref="ContentTypeDescriptor"/> can be found there. This is
    ///   useful for cases where new topic graphs are being imported and a new <see cref="Topic"/> references a new <see
    ///   cref="ContentTypeDescriptor"/> prior to it having been saved. In this case, that new version will be added to the
    ///   locally cached collection used by <see cref="GetContentTypeDescriptors()"/>.
    /// </remarks>
    /// <param name="sourceTopic"></param>
    /// <returns></returns>
    protected ContentTypeDescriptor? GetContentTypeDescriptor(Topic sourceTopic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceTopic, nameof(sourceTopic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType           = sourceTopic.ContentType;
      var contentTypes          = GetContentTypeDescriptors();
      var contentTypeDescriptor = contentTypes.Contains(contentType)? contentTypes[contentType] : null;

      if (contentTypeDescriptor is not null) {
        return contentTypeDescriptor;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to update content types from source graph
      \-----------------------------------------------------------------------------------------------------------------------*/
      SetContentTypeDescriptors(sourceTopic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      return contentTypes.Contains(contentType)? contentTypes[contentType] : null;

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override Topic? Load(Topic topic, DateTime version) {
      Contract.Requires(topic, nameof(topic));
      return Load(topic.Id, version, topic);
    }

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override void Rollback([ValidatedNotNull]Topic topic, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(version, nameof(version));
      Contract.Requires<ArgumentException>(
        topic.VersionHistory.Contains(version),
        "The version requested for rollback does not exist in the version history"
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      Load(topic, version);

      /*------------------------------------------------------------------------------------------------------------------------
      | Save as new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      Save(topic, false);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override sealed void Save([ValidatedNotNull] Topic topic, bool isRecursive = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      var version               = DateTime.UtcNow;
      var unresolvedTopics      = new TopicCollection();
      var isNew                 = topic.IsNew;

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle first pass
      \-----------------------------------------------------------------------------------------------------------------------*/
      Save(topic, isRecursive, unresolvedTopics, version);

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to resolve outstanding associations
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var unresolvedTopic in unresolvedTopics.ToList()) {
        unresolvedTopics.Remove(unresolvedTopic);
        Save(unresolvedTopic, false, unresolvedTopics, version);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Throw exception if unresolved topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (unresolvedTopics.Count > 0) {
        throw new ReferentialIntegrityException(
          $"The call to ITopicRepository.Save() introduced unresolved references on {unresolvedTopics.Count} topics, " +
          $"including '{unresolvedTopics.LastOrDefault().GetUniqueKey()}'. This is usually due to relationships or topic " +
          $"references outside the scope of the Save() which, themselves, have not yet been persisted to the data store. If " +
          $"this is not resolved, these items will not be correctly persisted."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      OnTopicSaved(new(topic, isRecursive, isNew));

    }

    /// <summary>
    ///   Recursive method that validates an individual topic, calls the derived implementation, and then recurses over any
    ///   child topics.
    /// </summary>
    /// <remarks>
    ///   When recursively saving a topic graph, it is conceivable that references to other topics—such as <see
    ///   cref="Topic.Relationships"/> or <see cref="Topic.BaseTopic"/>—can't yet be persisted because the target <see
    ///   cref="Topic"/> hasn't yet been saved, and thus the <see cref="Topic.Id"/> is still set to <c>-1</c>. To mitigate
    ///   this, the <paramref name="unresolvedTopics"/> allows this private overload to keep track of unresolved
    ///   associations. The public <see cref="Save(Topic, Boolean)"/> overload uses this list to resave any topics
    ///   that include such references. This adds some overhead due to the duplicate <see cref="Save(Topic, Boolean)"/>, but
    ///   helps avoid potential data loss when working with complex topic graphs.
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> to save.</param>
    /// <param name="isRecursive">Determines whether or not to recursively save <see cref="Topic.Children"/>.</param>
    /// <param name="unresolvedTopics">A list of <see cref="Topic"/>s with unresolved topic references.</param>
    /// <param name="version">The version to assign to the <paramref name="topic"/> updates.</param>
    private void Save([NotNull]Topic topic, bool isRecursive, TopicCollection unresolvedTopics, DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var isNew                 = topic.IsNew;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentTypeDescriptors= GetContentTypeDescriptors();
      var contentTypeDescriptor = GetContentTypeDescriptor(topic);

      if (contentTypeDescriptor is null) {
        throw new ReferentialIntegrityException(
          $"The Content Type \"{topic.ContentType}\" referenced by \"{topic.Key}\" could not be found under " +
          $"\"Configuration:ContentTypes\". There are currently {contentTypeDescriptors.Count} ContentTypes in the Repository."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle unresolved associations
      >-------------------------------------------------------------------------------------------------------------------------
      | If it's a recursive save and there are any unresolved associations, come back to this after the topic graph has been
      | saved; that ensures that any associations within the topic graph have been saved and can be properly persisted.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        topic.Relationships.Any(r => r.Values.Any(t => t.Id < 0)) ||
        topic.References.Any(t => t.Value?.Id < 0)
      ) {
        unresolvedTopics.Add(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Execute core implementation
      \-----------------------------------------------------------------------------------------------------------------------*/
      SaveTopic(topic, version, !isRecursive || !unresolvedTopics.Contains(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Update version history
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!topic.VersionHistory.Contains(version)) {
        topic.VersionHistory.Insert(0, version);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Perform reordering and/or move
      \---------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent is not null && topic.Attributes.IsDirty("ParentId") && !topic.IsNew) {
        var topicIndex = topic.Parent.Children.IndexOf(topic);
        if (topicIndex > 0) {
          Move(topic, topic.Parent, topic.Parent.Children[topicIndex - 1]);
        }
        else {
          Move(topic, topic.Parent);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If new content type, add to cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      var asContentType         = topic as ContentTypeDescriptor;
      if (
        isNew &&
        asContentType is not null &&
        _contentTypeDescriptors is not null &&
        !_contentTypeDescriptors.Contains(topic.Key)
      ) {
        _contentTypeDescriptors.Add(asContentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If content type, and relationships have been updated, refresh permitted content types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (asContentType is not null && asContentType.Relationships.IsDirty()) {
        asContentType.ResetPermittedContentTypes();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If new attribute, refresh cache
      >-------------------------------------------------------------------------------------------------------------------------
      | Every ContentTypeDescriptor has an AttributeDescriptors property which includes references to all local and inherited
      | AttributeDescriptors. When a new AttributeDescriptor is added, these collections are reset for the current
      | ContentTypeDescriptor and all descendents to ensure that they all reflect the new AttributeDescriptor.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isNew && IsAttributeDescriptor(topic)) {
        ResetAttributeDescriptors(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset original key
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.OriginalKey = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey is not null && topic.OriginalKey != topic.Key) {
        OnTopicRenamed(new(topic, topic.Key, topic.OriginalKey));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          Save(childTopic, isRecursive, unresolvedTopics, version);
        }
      }

    }

    /*==========================================================================================================================
    | METHOD: SAVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   The core implementation logic for saving an individual <see cref="Topic"/> during a <see cref="Save(Topic, Boolean)"/>
    ///   operation.
    /// </summary>
    /// <remarks>
    ///   The main <see cref="Save(Topic, Boolean)"/> implementation handles advanced validation of the parameters, updating the
    ///   <see cref="Topic.VersionHistory"/>, attempting to pick up any unresolved topics, updating <see cref="
    ///   ContentTypeDescriptor"/> and <see cref="AttributeDescriptor"/> instances as appropriate, raising the <see cref="
    ///   ITopicRepository.TopicRenamed"/>, if needed, and recursing over children. The derived implementation of <see cref="
    ///   SaveTopic(Topic, DateTime, Boolean)"/> is then left to focus exclusively on the core logic of persisting the changes
    ///   to the individual <paramref name="topic"/> to the underlying data store, and optionally updating its <see cref="Topic.
    ///   Relationships"/> and <see cref="Topic.References"/>, assuming <paramref name="persistRelationships"/> is set to <c>
    ///   true</c>.
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> to save.</param>
    /// <param name="version">The version to assign to the <paramref name="topic"/> updates.</param>
    /// <param name="persistRelationships">
    ///   Determines whether or not <see cref="Topic.References"/> and <see cref="Topic.Relationships"/> should be persisted.
    ///   This may be set to <c>false</c> if not all references have yet been saved, and the <see cref="Save(Topic, Boolean)"/>
    ///   call <c>isRecursive</c>; in that case, <see cref="Save(Topic, Boolean)"/> will circle back and attempt to save them
    ///   after the rest of the topic graph has been saved.
    /// </param>
    protected abstract void SaveTopic([NotNull] Topic topic, DateTime version, bool persistRelationships);

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override sealed void Move([ValidatedNotNull]Topic topic, [ValidatedNotNull]Topic target, Topic? sibling = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target != topic);
      Contract.Requires(topic, nameof(topic));
      Contract.Requires(target, nameof(target));
      Contract.Requires<ArgumentException>(topic != target, "A topic cannot be its own parent.");
      Contract.Requires<ArgumentException>(topic != sibling, "A topic cannot be moved relative to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Ignore requests
      \-----------------------------------------------------------------------------------------------------------------------*/
      //If the target is already positioned after the sibling, then no actual change is registered
      if (
        sibling is not null &&
        topic.Parent is not null &&
        topic.Parent.Children.IndexOf(sibling) == topic.Parent.Children.IndexOf(topic)-1) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Execute core implementation
      \-----------------------------------------------------------------------------------------------------------------------*/
      MoveTopic(topic, target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | Perform base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var previousParent        = topic.Parent;
      if (sibling is null) {
        topic.SetParent(target);
      }
      else {
        topic.SetParent(target, sibling);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If a content type descriptor is being moved to a new parent, refresh cache
      >-------------------------------------------------------------------------------------------------------------------------
      | Attributes are inherited from parent content types, and stored in the ContentTypeDescriptor.AttributeDescriptors
      | collection. As such, when a content type is moved to a new location, the attribute descriptors for itself and all
      | descendants needs to be reset to ensure the inheritance structure is updated.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (previousParent != target && topic is ContentTypeDescriptor) {
        SetContentTypeDescriptors(topic);
        ResetAttributeDescriptors(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      OnTopicMoved(new (topic, previousParent, target, sibling));

    }

    /*==========================================================================================================================
    | METHOD: MOVE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="Move(Topic, Topic, Topic?)" />
    /// <summary>
    ///   The core implementation logic for moving a <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   The main <see cref="Move(Topic, Topic, Topic?)"/> implementation handles advanced validation of the parameters,
    ///   updating the <see cref="Topic"/>'s location within the topic graph, updating <see cref="ContentTypeDescriptor"/> and
    ///   <see cref="AttributeDescriptor"/> instances as appropriate, and raising the <see cref="ITopicRepository.TopicMoved"/>.
    ///   The derived implementation of <see cref="MoveTopic(Topic, Topic, Topic?)"/> is then left to focus exclusively on the
    ///   core logic of persisting the change to the underlying data store.
    /// </remarks>
    protected abstract void MoveTopic(Topic topic, Topic target, Topic? sibling = null);

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public override sealed void Delete([ValidatedNotNull]Topic topic, bool isRecursive = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate descendants
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isRecursive && topic.Children.Any(t => !t.ContentType.Equals("List", StringComparison.OrdinalIgnoreCase))) {
        throw new ReferentialIntegrityException(
          $"The topic '{topic.GetUniqueKey()}' cannot be deleted. It has child topics, but '{nameof(isRecursive)}' is set to " +
          $"false. To delete '{topic.GetUniqueKey()}' and all of its descendants, set '{nameof(isRecursive)}' to true."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate base topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var childTopics           = topic.FindAll();
      var allTopics             = topic.GetRootTopic().FindAll().Except(childTopics);
      var baseTopic             = allTopics.FirstOrDefault(t => t.BaseTopic is not null && childTopics.Contains(t.BaseTopic));

      if (baseTopic is not null) {
        throw new ReferentialIntegrityException(
          $"The topic '{topic.GetUniqueKey()}' cannot be deleted. The topic '{baseTopic.GetUniqueKey()}' derives from the " +
          $"topic '{baseTopic.BaseTopic!.GetUniqueKey()}'. Deleting this would cause violate the integrity of the " +
          $"persistence store."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Execute core implementation
      \-----------------------------------------------------------------------------------------------------------------------*/
      DeleteTopic(topic);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent is not null) {
        topic.Parent.Children.Remove(topic.Key);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      var descendantTopics      = topic.FindAll();

      foreach (var descendantTopic in descendantTopics) {
        foreach (var relationship in descendantTopic.Relationships) {
          foreach (var relatedTopic in relationship.Values.ToList()) {
            if (!descendantTopics.Contains(relatedTopic)) {
              descendantTopic.Relationships.RemoveTopic(relationship.Key, relatedTopic);
            }
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove references
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var descendantTopic in descendantTopics) {
        foreach (var reference in descendantTopic.References) {
          if (reference.Value is not null && !descendantTopics.Contains(reference.Value)) {
            descendantTopic.References.Remove(reference.Key);
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove incoming relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var descendantTopic in descendantTopics) {
        foreach (var relationship in descendantTopic.IncomingRelationships) {
          foreach (var relatedTopic in relationship.Values.ToList()) {
            if (!descendantTopics.Contains(relatedTopic)) {
              if (relatedTopic.Relationships.Contains(relationship.Key, descendantTopic)) {
                relatedTopic.Relationships.RemoveTopic(relationship.Key, descendantTopic);
              }
              else if (relatedTopic.References.Contains(relationship.Key)) {
                relatedTopic.References.Remove(relationship.Key);
              }
            }
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If content type, remove from cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      SetContentTypeDescriptors(topic.Parent);

      /*------------------------------------------------------------------------------------------------------------------------
      | If attribute type, refresh cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (IsAttributeDescriptor(topic)) {
        ResetAttributeDescriptors(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Raise event
      \-----------------------------------------------------------------------------------------------------------------------*/
      var args = new TopicEventArgs(topic);
      OnTopicDeleted(args);

    }

    /*==========================================================================================================================
    | METHOD: DELETE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc cref="Delete(Topic, Boolean)" />
    /// <summary>
    ///   The core implementation logic for deleting a <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   The main <see cref="Delete(Topic, Boolean)"/> implementation handles advanced validation of the parameters,
    ///   removing the <see cref="Topic"/> from the topic graph, updating <see cref="ContentTypeDescriptor"/> and <see cref="
    ///   AttributeDescriptor"/> instances as appropriate, and raising the <see cref="ITopicRepository.TopicDeleted"/>. The
    ///   derived implementation of <see cref="DeleteTopic(Topic)"/> is then left to focus exclusively on the core logic of
    ///   persisting the change to the underlying data store.
    /// </remarks>
    protected abstract void DeleteTopic(Topic topic);

    /*==========================================================================================================================
    | METHOD: GET ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, returns a list of <see cref="AttributeValue"/>, optionally filtering based on <see
    ///   cref="AttributeDescriptor.IsExtendedAttribute"/> and <see cref="TrackedItem{T}.IsDirty"/>.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> from which to pull the attributes.</param>
    /// <param name="isExtendedAttribute">
    ///   Whether or not to filter by <see cref="AttributeDescriptor.IsExtendedAttribute"/>. If <c>null</c>, all <see
    ///   cref="AttributeValue"/>s are returned.
    /// </param>
    /// <param name="isDirty">
    ///   Whether or not to filter by <see cref="TrackedItem{T}.IsDirty"/>. If <c>null</c>, all <see cref="AttributeValue"/>s
    ///   are returned.
    /// </param>
    /// <param name="excludeLastModified">Exclude any attributes that start with <c>LastModified</c>.</param>
    protected IEnumerable<AttributeValue> GetAttributes(
      Topic topic,
      bool? isExtendedAttribute,
      bool? isDirty = null,
      bool excludeLastModified = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Get associated content type descriptor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType           = GetContentTypeDescriptor(topic);

      Contract.Assume(
        contentType,
        $"The topics repository does not contain a ContentTypeDescriptor for the '{topic.ContentType}' content type."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get indexed attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = new List<AttributeValue>();

      foreach (var attributeValue in topic.Attributes) {

        var key                 = attributeValue.Key;
        var attribute           = (AttributeDescriptor?)null;

        //Optionally exclude LastModified attributes
        if (excludeLastModified && attributeValue.Key.StartsWith("LastModified", StringComparison.OrdinalIgnoreCase)) {
          continue;
        }

        //Reset cached attribute descriptors just in case a new attribute has been added
        if (!contentType.AttributeDescriptors.Contains(key)) {
          contentType.ResetAttributeDescriptors();
        }

        //Attempt to retrieve the corresponding attribute descriptor
        if (contentType.AttributeDescriptors.Contains(key)) {
          attribute             = contentType.AttributeDescriptors[key];
        }

        //Skip if the value is null or empty; these values are not persisted to storage and should be treated as equivalent to
        //non-existent values.
        if (attributeValue.Value is null || attributeValue.Value.Length == 0) {
          continue;
        }

        //Skip if attribute's isDirty flag doesn't match the callers preference. Alternatively, if the IsExtendedAttribute value
        //doesn't match the source, as this implies the storage location has changed, and the attribute should be treated as
        //isDirty.
        if (
          isDirty is null ||
          attributeValue.IsDirty == isDirty ||
          isDirty == IsExtendedAttributeMismatch(attribute, attributeValue)
        ) {
        }
        else {
          continue;
        }

        //Add the attribute based on the isExtendedAttribute paramter. Add all parameters if isExtendedAttribute is null. Assume
        //an attribute is extended if the corresponding attribute descriptor cannot be located and the value is over 255
        //characters.
        if (isExtendedAttribute?.Equals(attribute?.IsExtendedAttribute?? attributeValue.Value.Length > 255)?? true) {
          attributes.Add(attributeValue);
        }

      }

      return attributes;

    }

    /*==========================================================================================================================
    | METHOD: GET UNMATCHED ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, identifies <see cref="AttributeValue"/>s that are defined based on the <see
    ///   cref="ContentTypeDescriptor"/>, but aren't defined in the <see cref="AttributeValueCollection"/>.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> from which to pull the attributes.</param>
    protected IEnumerable<AttributeDescriptor> GetUnmatchedAttributes(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Get associated content type descriptor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType           = GetContentTypeDescriptor(topic);

      Contract.Assume(
        contentType,
        $"The topics repository does not contain a ContentTypeDescriptor for the '{topic.ContentType}' content type."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get unmatched attribute descriptors
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = new AttributeDescriptorCollection();

      foreach (var attribute in contentType.AttributeDescriptors) {

        // Ignore unsaved topics
        if (topic.IsNew) {
          continue;
        }

        // Ignore (now legacy) core attributes
        if (attribute.Key is "Key" or "ContentType" or "ParentID") {
          continue;
        }

        // Ignore valid attributes
        if (
          topic.Attributes.Contains(attribute.Key) &&
          !String.IsNullOrEmpty(topic.Attributes.GetValue(attribute.Key, null, false, false))
        ) {
          continue;
        };

        // Ignore associations
        if (attribute.ModelType is ModelType.Relationship or ModelType.NestedTopic or ModelType.Reference) {
          continue;
        }

        attributes.Add(attribute);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Get arbitrary attributes
      >-------------------------------------------------------------------------------------------------------------------------
      | ###HACK JJC20200502: Arbitrary attributes are those that don't map back to the scheme. These aren't picked up by the
      | AttributeDescriptors check above. This means there's no way to programmatically delete arbitrary (or orphaned)
      | attributes. To mitigate this, any null or empty attribute values should be included. By definition, though, arbitrary
      | attributes don't have corresponding AttributeDescriptors. To mitigate this, an ad hoc AttributeDescriptor object will be
      | created for each empty AttributeDescriptor.
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeKeys         = topic.Attributes
        .Where(a => String.IsNullOrEmpty(a.Value))
        .Select(a => a.Key)
        .Union(topic.Attributes.DeletedItems);
      foreach (var attributeKey in attributeKeys) {
        if (!attributes.Contains(attributeKey)) {
          attributes.Add((AttributeDescriptor)TopicFactory.Create(attributeKey, "TextAttributeDescriptor"));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return values
      \-----------------------------------------------------------------------------------------------------------------------*/
      return attributes;

    }

    /*==========================================================================================================================
    | METHOD: IS ATTRIBUTE DESCRIPTOR?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, determines if it derives from <see cref="AttributeDescriptor"/> and is associated with
    ///   a <see cref="ContentTypeDescriptor"/>.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> to evaluate as an <see cref="AttributeDescriptor"/>.</param>
    private static bool IsAttributeDescriptor(Topic topic) =>
      topic is AttributeDescriptor and { Parent: { Key: "Attributes", Parent: ContentTypeDescriptor } };

    /*==========================================================================================================================
    | METHOD: IS EXTENDED ATTRIBUTE MISMATCH?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not there's a mismatch between the <see cref="AttributeDescriptor.IsExtendedAttribute"/> and the
    ///   <see cref="AttributeValue.IsExtendedAttribute"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     The <see cref="AttributeDescriptor.IsExtendedAttribute"/> determines where an attribute <i>should</i> be stored; the
    ///     <see cref="AttributeValue.IsExtendedAttribute"/> determines where an attribute <i>was</i> stored. If these two
    ///     values are in conflict, that suggests the coniguration for <see cref="AttributeDescriptor.IsExtendedAttribute"/> has
    ///     changed since the attribute value was last saved. In that case, it should be treated as <see
    ///     cref="TrackedItem{T}.IsDirty"/> <i>even though</i> its value hasn't changed to ensure that its storage location is
    ///     updated.
    ///   </para>
    ///   <para>
    ///     If <see cref="AttributeDescriptor"/> cannot be found then the <see cref="AttributeValue"/> is arbitrary attribute
    ///     not mapped to the schema. In that case, its storage location is dynamically determined based on its length, and thus
    ///     it should only change locations when it <see cref="TrackedItem{T}.IsDirty"/>. Otherwise, its length will remain the
    ///     same, and thus the storage location should remain unchanged.
    ///   </para>
    /// </remarks>
    /// <param name="attributeDescriptor">The source <see cref="AttributeDescriptor"/>, if available.</param>
    /// <param name="attributeValue">The target <see cref="AttributeValue"/>.</param>
    /// <returns></returns>
    private static bool IsExtendedAttributeMismatch(AttributeDescriptor? attributeDescriptor, AttributeValue attributeValue) =>
      attributeDescriptor is not null &&
      attributeValue.IsExtendedAttribute is not null &&
      attributeDescriptor.IsExtendedAttribute != attributeValue.IsExtendedAttribute;

    /*==========================================================================================================================
    | METHOD: RESET ATTRIBUTE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Assuming a topic is either a <see cref="ContentTypeDescriptor"/> or an <see cref="AttributeDescriptor"/>, will
    ///   reset the cached <see cref="AttributeDescriptor"/>s on the associated <see cref="ContentTypeDescriptor"/> and all
    ///   children.
    /// </summary>
    /// <remarks>
    ///   Each <see cref="ContentTypeDescriptor"/> has a <see cref="ContentTypeDescriptor.AttributeDescriptors"/> collection
    ///   which includes not only the <see cref="AttributeDescriptor"/>s associated with that <see
    ///   cref="ContentTypeDescriptor"/>, but <i>also</i> any <see cref="AttributeDescriptor"/>s from any parent <see
    ///   cref="ContentTypeDescriptor"/>s in the topic graph. This reflects the fact that attributes are inherited from parent
    ///   content types. As a result, however, when an <see cref="AttributeDescriptor"/> is added or removed, or a <see
    ///   cref="ContentTypeDescriptor"/> is moved to a new parent, this cache should be reset on the associated <see
    ///   cref="ContentTypeDescriptor"/> and all descendent <see cref="ContentTypeDescriptor"/>s to ensure the change is
    ///   reflected.
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> to evaluate as an <see cref="AttributeDescriptor"/>.</param>
    private static void ResetAttributeDescriptors(Topic topic) {
      if (IsAttributeDescriptor(topic)) {
        ((ContentTypeDescriptor)topic.Parent!.Parent!).ResetAttributeDescriptors();
      }
      else if (topic is ContentTypeDescriptor descriptor) {
        descriptor.ResetAttributeDescriptors();
      }
    }

  } //Class
} //Namespace