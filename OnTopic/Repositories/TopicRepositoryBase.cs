/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.Querying;
using Microsoft;
using OnTopic.Attributes;
using System.Collections.Generic;
using System.Linq;
using OnTopic.Collections;
using OnTopic.Metadata.AttributeTypes;

#pragma warning disable CS0618 // Type or member is obsolete; used to hide known deprecation of events until v5.0.0

namespace OnTopic.Repositories {

  /*============================================================================================================================
  | CLASS: TOPIC DATA PROVIDER BASE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Defines a base abstract class for taxonomy data providers.
  /// </summary>
  public abstract class TopicRepositoryBase : ITopicRepository {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private ContentTypeDescriptorCollection? _contentTypeDescriptors = null;

    /*==========================================================================================================================
    | EVENT HANDLERS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    public event EventHandler<DeleteEventArgs>? DeleteEvent;

    /// <inheritdoc />
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    public event EventHandler<MoveEventArgs>? MoveEvent;

    /// <inheritdoc />
    [Obsolete("The TopicRepository events will be removed in OnTopic Library 5.0.", false)]
    public event EventHandler<RenameEventArgs>? RenameEvent;

    /*==========================================================================================================================
    | GET CONTENT TYPE DESCRIPTORS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual ContentTypeDescriptorCollection GetContentTypeDescriptors() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize content types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypeDescriptors == null) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Load configuration data
        \---------------------------------------------------------------------------------------------------------------------*/
        var configuration = Load("Configuration");

        Contract.Assume(configuration, $"The 'Root:Configuration' section could not be loaded from the 'ITopicRepository'.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Load root content type
        \---------------------------------------------------------------------------------------------------------------------*/
        var allowedContentTypes = configuration.Children.GetTopic("ContentTypes") as ContentTypeDescriptor;

        Contract.Assume(allowedContentTypes, "Unable to load section 'Configuration:ContentTypes'.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Add available Content Types to the collection
        \---------------------------------------------------------------------------------------------------------------------*/
        _contentTypeDescriptors = GetContentTypeDescriptors(allowedContentTypes);

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
    ///   cref="ITopicRepository.Save(Topic, Boolean, Boolean)"/> when the topic graph being saved includes any <see
    ///   cref="ContentTypeDescriptor"/>s.
    /// </remarks>
    /// <param name="contentTypeDescriptors">
    ///   The root of a <see cref="ContentTypeDescriptor"/> topic graph to merge into the collection for <see
    ///   cref="GetContentTypeDescriptors()"/>. The code will process not only the root <see cref="ContentTypeDescriptor"/>, but
    ///   also any descendents.
    /// </param>
    /// <returns></returns>
    protected virtual ContentTypeDescriptorCollection GetContentTypeDescriptors(ContentTypeDescriptor contentTypeDescriptors) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize the collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_contentTypeDescriptors == null) {
        _contentTypeDescriptors = new ContentTypeDescriptorCollection();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (contentTypeDescriptors == null) {
        return _contentTypeDescriptors;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add available Content Types to the collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in contentTypeDescriptors.FindAllByAttribute("ContentType", "ContentType")) {
        // Ensure the Topic is used as the strongly-typed ContentType
        // Add ContentType Topic to collection if not already added
        if (
          topic is ContentTypeDescriptor contentTypeDescriptor &&
          !_contentTypeDescriptors.Contains(contentTypeDescriptor.Key)
        ) {
          _contentTypeDescriptors.Add(contentTypeDescriptor);
        }
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

      if (contentTypeDescriptor != null) {
        return contentTypeDescriptor;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        sourceTopic.GetByUniqueKey("Root:Configuration:ContentTypes") is ContentTypeDescriptor sourceContentTypes &&
        !contentTypes.Contains(sourceContentTypes)
      ) {
        contentTypes            = GetContentTypeDescriptors(sourceContentTypes);
      }

      return contentTypes.Contains(contentType)? contentTypes[contentType] : null;

    }

    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public abstract Topic? Load(int topicId, bool isRecursive = true);

    /// <inheritdoc />
    public abstract Topic? Load(string? topicKey = null, bool isRecursive = true);

    /// <inheritdoc />
    public abstract Topic? Load(int topicId, DateTime version);

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual void Rollback([ValidatedNotNull, NotNull]Topic topic, DateTime version) {

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
      var originalVersion = Load(topic.Id, version);

      Contract.Assume(
        originalVersion,
        "The version requested for rollback does not exist in the Topic repository or database."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark each attribute as dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in originalVersion.Attributes) {
        if (!topic.Attributes.Contains(attribute.Key) || topic.Attributes.GetValue(attribute.Key) != attribute.Value) {
          attribute.IsDirty = true;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct new AttributeCollection
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.Clear();
      foreach (var attribute in originalVersion.Attributes) {
        topic.Attributes.Add(attribute);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Rename topic, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Key == originalVersion.Key) {
        topic.Attributes.SetValue("Key", topic.Key, false);
      }
      else {
        topic.Key = originalVersion.Key;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure Parent, ContentType are maintained
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.Attributes.SetValue("ContentType", topic.ContentType, topic.ContentType != originalVersion.ContentType);
      topic.Attributes.SetValue("ParentId", topic.Parent?.Id.ToString(CultureInfo.InvariantCulture)?? "-1", false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Save as new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      Save(topic, false);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual int Save([ValidatedNotNull, NotNull]Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      _contentTypeDescriptors = GetContentTypeDescriptors();
      if (!_contentTypeDescriptors.Contains(topic.ContentType)) {
        throw new ArgumentException(
          $"The Content Type \"{topic.ContentType}\" referenced by \"{topic.Key}\" could not be found under " +
          $"\"Configuration:ContentTypes\". There are currently {_contentTypeDescriptors.Count} ContentTypes in the Repository."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Update content types collection, if appropriate
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is ContentTypeDescriptor && !_contentTypeDescriptors.Contains(topic.Key)) {
        _contentTypeDescriptors.Add((ContentTypeDescriptor)topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.OriginalKey != null && topic.OriginalKey != topic.Key) {
        var args = new RenameEventArgs(topic);
        RenameEvent?.Invoke(this, args);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Perform reordering and/or move
      \---------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null && topic.Attributes.IsDirty("ParentId") && topic.Id >= 0) {
        var topicIndex = topic.Parent.Children.IndexOf(topic);
        if (topicIndex > 0) {
          Move(topic, topic.Parent, topic.Parent.Children[topicIndex - 1]);
        }
        else {
          Move(topic, topic.Parent);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If new content type, remove from cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        topic.Id < 0 &&
        topic is ContentTypeDescriptor &&
        _contentTypeDescriptors != null &&
        !_contentTypeDescriptors.Contains(topic)
      ) {
        _contentTypeDescriptors.Add((ContentTypeDescriptor)topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If new attribute, refresh cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Id < 0 && IsAttributeDescriptor(topic)) {
        ResetAttributeDescriptors(topic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset original key
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.OriginalKey = null;
      return -1;

    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual void Move([ValidatedNotNull, NotNull]Topic topic, [ValidatedNotNull, NotNull]Topic target) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target != topic);
      Contract.Requires(topic, "The topic parameter must be specified.");
      Contract.Requires(target, "The target parameter must be specified.");
      if (topic.Parent != target) {
        MoveEvent?.Invoke(this, new MoveEventArgs(topic, target));
        topic.SetParent(target);
      }

    }

    /// <summary>
    ///   Interface method that supports moving a topic from one position to another.
    /// </summary>
    /// <param name="topic">The topic object to be moved.</param>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    public virtual void Move([ValidatedNotNull, NotNull]Topic topic, [ValidatedNotNull, NotNull]Topic target, Topic? sibling) {

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
        sibling != null &&
        topic.Parent != null &&
        topic.Parent.Children.IndexOf(sibling) == topic.Parent.Children.IndexOf(topic)-1) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Perform base logic
      \-----------------------------------------------------------------------------------------------------------------------*/
      MoveEvent?.Invoke(this, new MoveEventArgs(topic, target));
      topic.SetParent(target, sibling);

      /*------------------------------------------------------------------------------------------------------------------------
      | If a content type descriptor is being moved to a new parent, refresh cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != target && topic is ContentTypeDescriptor) {
        ResetAttributeDescriptors(topic);
      }

    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public virtual void Delete([ValidatedNotNull, NotNull]Topic topic, bool isRecursive) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topic, nameof(topic));

      /*------------------------------------------------------------------------------------------------------------------------
      | Trigger event
      \-----------------------------------------------------------------------------------------------------------------------*/
      var         args    = new DeleteEventArgs(topic);
      DeleteEvent?.Invoke(this, args);

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Parent != null) {
        topic.Parent.Children.Remove(topic.Key);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If content type, remove from cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is ContentTypeDescriptor && _contentTypeDescriptors != null) {
        foreach (var contentTypeDescriptor in topic.FindAll(t => t is ContentTypeDescriptor).Cast<ContentTypeDescriptor>()) {
          if (_contentTypeDescriptors.Contains(contentTypeDescriptor)) {
            _contentTypeDescriptors.Remove(contentTypeDescriptor);
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | If attribute type, refresh cache
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (IsAttributeDescriptor(topic)) {
        ResetAttributeDescriptors(topic);
      }

    }

    /*==========================================================================================================================
    | METHOD: GET ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <see cref="Topic"/>, returns a list of <see cref="AttributeValue"/>, optionally filtering based on <see
    ///   cref="AttributeDescriptor.IsExtendedAttribute"/> and <see cref="AttributeValue.IsDirty"/>.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> from which to pull the attributes.</param>
    /// <param name="isExtendedAttribute">
    ///   Whether or not to filter by <see cref="AttributeDescriptor.IsExtendedAttribute"/>. If <c>null</c>, all <see
    ///   cref="AttributeValue"/>s are returned.
    /// </param>
    /// <param name="isDirty">
    ///   Whether or not to filter by <see cref="AttributeValue.IsDirty"/>. If <c>null</c>, all <see cref="AttributeValue"/>s
    ///   are returned.
    /// </param>
    protected IEnumerable<AttributeValue> GetAttributes(Topic topic, bool? isExtendedAttribute, bool? isDirty = null) {

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
        "The Topics repository or database does not contain a ContentTypeDescriptor for the Page content type."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get indexed attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = new List<AttributeValue>();

      foreach (var attributeValue in topic.Attributes.Where(a => isDirty == null || a.IsDirty == isDirty)) {

        var key                 = attributeValue.Key;
        var attribute           = (AttributeDescriptor?)null;

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
        if (String.IsNullOrEmpty(attributeValue.Value)) {
          continue;
        }

        //Add the attribute based on the isExtendedAttribute paramter. Add all parameters if isExtendedAttribute is null. Assume
        //an attribute is extended if the corresponding attribute descriptor cannot be located and the value is over 255
        //characters.
        if (isExtendedAttribute?.Equals(attribute?.IsExtendedAttribute?? attributeValue.Value?.Length > 255)?? true) {
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
        $"The Topics repository or database does not contain a ContentTypeDescriptor for the {topic.ContentType} content type."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Get unmatched attribute descriptors
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributes            = new TopicCollection<AttributeDescriptor>();

      foreach (var attribute in contentType.AttributeDescriptors) {

        // Ignore unsaved topics
        if (topic.Id == -1) {
          continue;
        }

        // Ignore system attributes
        if (attribute.Key == "Key" || attribute.Key == "ContentType" || attribute.Key == "ParentID") {
          continue;
        }

        // Ignore valid attributes
        if (
          topic.Attributes.Contains(attribute.Key) &&
          !String.IsNullOrEmpty(topic.Attributes.GetValue(attribute.Key, null, false, false))
        ) {
          continue;
        };

        // Ignore relationships and nested topics
        if (attribute.ModelType.Equals(ModelType.Relationship) || attribute.ModelType.Equals(ModelType.NestedTopic)) {
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
      foreach (var attribute in topic.Attributes.Where(a => String.IsNullOrEmpty(a.Value))) {
        if (!attributes.Contains(attribute.Key)) {
          attributes.Add((TextAttribute)TopicFactory.Create(attribute.Key, "TextAttribute"));
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
      topic is AttributeDescriptor &&
      topic.Parent?.Key == "Attributes" &&
      topic.Parent.Parent is ContentTypeDescriptor;

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
      else if (topic is ContentTypeDescriptor) {
        ((ContentTypeDescriptor)topic).ResetAttributeDescriptors();
      }
    }

  } //Class
} //Namespace

#pragma warning restore CS0618 // Type or member is obsolete