/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;
using OnTopic.References;

namespace OnTopic {

  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The Topic object is a simple container for a particular node in the topic hierarchy. It contains the metadata associated
  ///   with the particular node, a list of children, etc.
  /// </summary>
  public class Topic {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     string                          _key;
    private                     string                          _contentType;
    private                     int                             _id                             = -1;
    private                     string?                         _originalKey;
    private                     Topic?                          _parent;
    private                     bool                            _isDirty;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of a <see cref="Topic"/> class with a <see cref="Key"/>, <see cref="ContentType"/>, and,
    ///   optionally, <see cref="Parent"/>, <see cref="Id"/>.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Key"/> and <see
    ///   cref="ContentType"/> will be set to <see cref="AttributeValue.IsDirty"/>, which is required in order to correctly save
    ///   new topics to the database. When the <paramref name="id"/> parameter is set, however, the <see
    ///   cref="AttributeValue.IsDirty"/> property is set to <c>false</c>on <see cref="Key"/> and <see cref="ContentType"/>, as
    ///   it is assumed these are being set to the same values currently used in the persistence store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <param name="parent">Optional topic to set as the new topic's parent.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public Topic(string key, string contentType, Topic? parent = null, int id = -1) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      Children                  = new();
      Attributes                = new(this);
      IncomingRelationships     = new(this, true);
      Relationships             = new(this, false);
      References                = new(this);
      VersionHistory            = new();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set entity identifier, if present
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (id >= 0) {
        Id                      = id;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set core properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key                       = key;
      ContentType               = contentType;

      if (parent is not null) {
        Parent                  = parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize key fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      //###HACK JJC20190924: The local backing fields _key and _contentType are always initialized at this point. But Roslyn's
      //flow analysis isn't smart enough to detect this. As such, the following effectively sets _key and _contentType to
      //themselves.
      _key                      = Key;
      _contentType              = ContentType;

    }

    #region Core Properties

    /*==========================================================================================================================
    | PROPERTY: ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's integer identifier according to the data provider.
    /// </summary>
    /// <value>
    ///   The unique identifier for the <see cref="Topic"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    ///   The value of this topic has already been set to {_id}; it cannot be changed.
    /// </exception>
    /// <requires description="The id is expected to be a positive value." exception="T:System.ArgumentException">
    ///   value &gt; 0
    /// </requires>
    public int Id {
      get => _id;
      set {
        Contract.Requires<ArgumentOutOfRangeException>(value > 0, "The id is expected to be a positive value.");
        if (_id > 0 && !_id.Equals(value)) {
          throw new InvalidOperationException($"The value of this topic has already been set to {_id}; it cannot be changed.");
        }
        _id = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reference to the parent topic of this node, allowing code to traverse topics as a linked list.
    /// </summary>
    /// <value>
    ///   The current <see cref="Topic"/>'s parent <see cref="Topic"/>.
    /// </value>
    /// <remarks>
    ///   While topics may be represented as a network graph via relationships, they are physically stored and primarily
    ///   represented via a hierarchy. As such, each topic may have at most a single parent. Note that the root node will
    ///   have a null parent.
    /// </remarks>
    /// <requires description="The value for Parent must not be null." exception="T:System.ArgumentNullException">
    ///   value is not null
    /// </requires>
    /// <requires description="A topic cannot be its own parent." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    [DisallowNull]
    public Topic? Parent {
      get => _parent;
      set {
        if (_parent != value) {
          Contract.Requires(value, "Parent cannot be explicitly set to null.");
          SetParent(value, value?.Children?.LastOrDefault());
        }
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a keyed collection of child <see cref="Topic" /> instances associated with the current <see cref="Topic" />.
    /// </summary>
    /// <value>
    ///   The children of the current <see cref="Topic"/>.
    /// </value>
    public KeyedTopicCollection Children { get; }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors" /> property).
    /// </remarks>
    /// <value>
    ///   The key of the current <see cref="Topic"/>'s <see cref="ContentTypeDescriptor"/>.
    /// </value>
    public string ContentType {
      get => _contentType;
      set {
        TopicFactory.ValidateKey(value);
        if (_contentType == value) {
          return;
        }
        else if (_contentType is not null || IsNew) {
          _isDirty              = true;
        }
        _contentType            = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's Key attribute, the primary text identifier for the topic.
    /// </summary>
    /// <value>
    ///   The current <see cref="Topic"/>'s key, which is guaranteed to be unique among its siblings.
    /// </value>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value is not null
    /// </requires>
    /// <requires
    ///   description="The Key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException"
    /// >
    ///   !value.Contains(" ")
    /// </requires>
    public string Key {
      get => _key;
      set {
        TopicFactory.ValidateKey(value);
        if (_key == value) {
          return;
        }
        else if (_key is not null || IsNew) {
          _isDirty              = true;
        }
        if (_originalKey is null) {
          _originalKey = _key;
        }
        //If an established key value is changed, the parent's index must be manually updated; this won't happen automatically.
        if (_originalKey is not null && !value.Equals(_key, StringComparison.OrdinalIgnoreCase) && Parent is not null) {
          Parent.Children.ChangeKey(this, value);
        }
        _key                    = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: ORIGINAL KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's original key.
    /// </summary>
    /// <remarks>
    ///   The original key is automatically set by <see cref="Key" /> when its value is updated (assuming the original key isn't
    ///   already set). This is, in turn, used by the <see cref="Repositories.TopicRenameEventArgs" /> to represent the original
    ///   value, and thus allow the <see cref="Repositories.ITopicRepository" /> (or derived providers) from updating the data
    ///   store appropriately.
    /// </remarks>
    /// <value>
    ///   The key, as represented in the persistence layer.
    /// </value>
    /// <requires
    ///   description="The OriginalKey should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException"
    /// >
    ///   !value?.Contains(" ")?? true
    /// </requires>
    internal string? OriginalKey {
      get => _originalKey;
      set {
        TopicFactory.ValidateKey(value, true);
        _originalKey = value;
      }
    }

    #endregion

    #region Convenience Properties

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute, representing the default view to be used for the topic.
    /// </summary>
    /// <remarks>
    ///   This value can be set via the query string (via the <c>TopicViewResultExecutor</c> class), via the Accepts header
    ///   (also via the <c>TopicViewResultExecutor</c> class), on the topic itself (via this property). By default, it will
    ///   be set to the name of the <see cref="ContentType" />; e.g., if the Content Type is "Page", then the view will be
    ///   "Page". This will cause the <c>TopicViewResultExecutor</c> to look for a view at, for instance,
    ///   <c>/Views/Page/Page.cshtml</c>.
    /// </remarks>
    /// <value>
    ///   The view, as specified by the current <see cref="Topic"/>.
    /// </value>
    /// <requires
    ///   description="The View should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException"
    /// >
    ///   !value?.Contains(" ")?? true
    /// </requires>
    [AttributeSetter]
    public string? View {
      get =>
        Attributes.GetValue("View", "");
      set {
        TopicFactory.ValidateKey(value, true);
        SetAttributeValue("View", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS NEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not the current <see cref="Topic"/> has been saved to an underlying <see
    ///   cref="Repositories.ITopicRepository"/>. If not, returns <c>true</c>.
    /// </summary>
    /// <remarks>
    ///   This property does <i>not</i> reflect whether the current <i>state</i> of the topic has been saved. It <i>only</i>
    ///   determines if the <see cref="Topic"/> maps to an existing entity in the underlying <see
    ///   cref="Repositories.ITopicRepository"/>.
    /// </remarks>
    /// <value>
    ///   <c>true</c> if it has been saved; otherwise, <c>false</c>.
    /// </value>
    public bool IsNew => Id < 0;

    /*==========================================================================================================================
    | PROPERTY: IS HIDDEN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is hidden.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is hidden; otherwise, <c>false</c>.
    /// </value>
    [AttributeSetter]
    public bool IsHidden {
      get => Attributes.GetBoolean("IsHidden");
      set => SetAttributeValue("IsHidden", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | PROPERTY: IS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is disabled.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disabled; otherwise, <c>false</c>.
    /// </value>
    [AttributeSetter]
    public bool IsDisabled {
      get => Attributes.GetBoolean("IsDisabled");
      set => SetAttributeValue("IsDisabled", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | METHOD: IS VISIBLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines whether or not a topic should be visible based on IsHidden, IsDisabled, and an optional parameter
    ///   specifying whether or not to show disabled items (which may by triggered if, for example, a user is an administrator).
    /// </summary>
    /// <remarks>
    ///   If an item is not marked as IsVisible, then the item will not be visible independent of whether showDisabled is set.
    /// </remarks>
    /// <param name="showDisabled">Determines whether or not items marked as IsDisabled should be displayed.</param>
    /// <returns>
    ///   <c>true</c> if the <see cref="Topic" /> is visible; otherwise, <c>false</c>.
    /// </returns>
    public bool IsVisible(bool showDisabled = false) => !IsHidden && (showDisabled || !IsDisabled);

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute, which represents the friendly name of the topic.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="Key" /> may not contain, for instance, spaces or symbols, there are no restrictions on what
    ///   characters can be used in the title. For this reason, it provides the default public value for referencing topics. If
    ///   the title is not set, then this property falls back to the topic's <see cref="Key" />.
    /// </remarks>
    /// <value>
    ///   The current <see cref="Topic"/>'s title.
    /// </value>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    public string Title {
      get => Attributes.GetValue("Title", Key)?? Key;
      set => SetAttributeValue("Title", value);
    }

    /*==========================================================================================================================
    | PROPERTY: DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Description attribute.
    /// </summary>
    /// <remarks>
    ///   The Description attribute is primarily used by the editor to display help content for an attribute topic, noting
    ///   how the attribute is used, what is the expected input format or value, etc.
    /// </remarks>
    /// <value>
    ///   The current <see cref="Topic"/>'s description.
    /// </value>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    [Obsolete("The Description convenience property will be removed in OnTopic Library 5.0. Use Attributes.SetValue() instead.", true)]
    public string? Description {
      get => Attributes.GetValue("Description");
      set => SetAttributeValue("Description", value);
    }

    /*==========================================================================================================================
    | PROPERTY: LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's last modified attribute.
    /// </summary>
    /// <remarks>
    ///   The value is stored in the database as a string (Attribute) value, but converted to DateTime for use in the system. It
    ///   is important to note that the last modified attribute is not tied to the system versioning (which operates at an
    ///   attribute level) nor is it guaranteed to be correct for auditing purposes; for example, the author may explicitly
    ///   overwrite this value for various reasons (such as backdating a web page).
    /// </remarks>
    /// <value>
    ///   The date that the current <see cref="Topic"/> was last modified.
    /// </value>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value.ToString())
    /// </requires>
    public DateTime LastModified {
      get => Attributes.GetDateTime("LastModified", VersionHistory.DefaultIfEmpty(DateTime.MinValue).LastOrDefault());
      set => SetAttributeValue("LastModified", value.ToString(CultureInfo.InvariantCulture));
    }

    #endregion

    #region Relationship and Collection Methods

    /*==========================================================================================================================
    | METHOD: SET PARENT
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the current <see cref="Parent" /> while simultaneously ensuring that the sort order of the topics is
    ///   maintained, assuming a <paramref name="sibling" /> is set.
    /// </summary>
    /// <remarks>
    ///   If no <paramref name="sibling" /> is provided, then the item is added to the <i>beginning</i> of the collection. If
    ///   the intent is to add it to the <i>end</i> of the collection, then set the <paramref name="sibling" /> to e.g.
    ///   <c>parent.Children.LastOrDefault()</c>.
    /// </remarks>
    /// <param name="parent">The <see cref="Topic" /> to move this <see cref="Topic" /> under.</param>
    /// <param name="sibling">The <see cref="Topic" /> to move this <see cref="Topic" /> to the right of.</param>
    /// <exception cref="ArgumentOutOfRangeException">parent - A descendant cannot be its own parent.</exception>
    /// <exception cref="InvalidKeyException">
    ///   Duplicate key when setting Parent property: the topic with the name '{Key}' already exists in the '{parent.Key}'
    ///   topic.
    /// </exception>
    public void SetParent(Topic parent, Topic? sibling = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Check preconditions
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(parent, "The value for Parent must not be null.");
      Contract.Requires<ArgumentOutOfRangeException>(parent != this, "A topic cannot be its own parent.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Check to ensure that the topic isn't being moved to a descendant (topics cannot be their own grandpa)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parent.GetUniqueKey().StartsWith(GetUniqueKey(), StringComparison.OrdinalIgnoreCase)) {
        throw new ArgumentOutOfRangeException(nameof(parent), "A descendant cannot be its own parent.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Check to ensure that the topic isn't being moved to a parent with a duplicate key
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parent != _parent && parent.Children.Contains(Key)) {
        throw new InvalidKeyException(
          $"Duplicate key when setting Parent property: the topic with the name '{Key}' already exists in the '{parent.Key}' " +
          $"topic."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Move topic to new location
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_parent is not null) {
        _parent.Children.Remove(Key);
      }
      var insertAt = (sibling is not null)? parent.Children.IndexOf(sibling)+1 : 0;
      parent.Children.Insert(insertAt, this);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set parent values
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_parent != parent) {
        _parent = parent;
      }


    }

    /*==========================================================================================================================
    | METHOD: GET UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the full, hierarchical identifier for the topic, including parents.
    /// </summary>
    /// <remarks>
    ///   The value for the UniqueKey property is a collated, colon-delimited representation of the topic and its parent(s).
    ///   Example: "Root:Configuration:ContentTypes:Page".
    /// </remarks>
    /// <returns>The unique key of the current <see cref="Topic"/>.</returns>
    #pragma warning disable CA1024 // Use properties where appropriate
    public string GetUniqueKey() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Crawl up tree to define uniqueKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      var uniqueKey = "";
      var topic = (Topic?)this;

      while (topic is not null) {
        if (uniqueKey.Length > 0) uniqueKey = $":{uniqueKey}";
        uniqueKey = topic.Key + uniqueKey;
        topic = topic.Parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return uniqueKey;

    }
    #pragma warning restore CA1024 // Use properties where appropriate

    /*==========================================================================================================================
    | METHOD: GET WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the root-relative web path of the Topic, based on an assumption that the root topic is bound to the root of the
    ///   site.
    /// </summary>
    /// <remarks>
    ///   Note: If the topic root is not bound to the root of the site, this needs to specifically accounted for in any views
    ///   that reference the web path (e.g., by providing a prefix).
    /// </remarks>
    /// <returns>The HTTP-based path to the current <see cref="Topic"/>.</returns>
    public string GetWebPath() {
      var uniqueKey = GetUniqueKey()
        .Replace("Root:", "/", StringComparison.Ordinal)
        .Replace(":", "/", StringComparison.Ordinal) + "/";
      if (!uniqueKey.StartsWith("/", StringComparison.Ordinal)) {
        uniqueKey = $"/{uniqueKey}";
      }
      return uniqueKey;
    }

    /*==========================================================================================================================
    | METHOD: IS DIRTY?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Determines if the topic is dirty, optionally checking <see cref="Relationships"/> and <see cref="Attributes"/>.
    /// </summary>
    /// <param name="checkCollections">
    ///   Determines if <see cref="Attributes"/>, <see cref="Relationships"/>, and <see cref="References"/> should be checked.
    /// </param>
    /// <param name="excludeLastModified">
    ///   Optionally excludes <see cref="AttributeValue"/>s whose keys start with <c>LastModified</c>. This is useful for
    ///   excluding the byline (<c>LastModifiedBy</c>) and dateline (<c>LastModified</c>) since these values are automatically
    ///   generated by e.g. the OnTopic Editor and, thus, may be irrelevant updates if no other attribute values have changed.
    /// </param>
    /// <returns>
    ///   Returns <c>true</c> if the <see cref="Key"/>, <see cref="ContentType"/>, or, optionally, any collections have been
    ///   modified.
    /// </returns>
    public bool IsDirty(bool checkCollections = false, bool excludeLastModified = false) {
      if (!_isDirty && checkCollections) {
        _isDirty = Attributes.IsDirty(excludeLastModified);
      }
      if (!_isDirty && checkCollections) {
        _isDirty = Relationships.IsDirty();
      }
      if (!_isDirty && checkCollections) {
        _isDirty = References.IsDirty();
      }
      return _isDirty;
    }

    /*==========================================================================================================================
    | METHOD: MARK CLEAN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Resets the <see cref="IsDirty"/> status of the <see cref="Topic"/>—and, optionally, that of all collections, using the
    ///   <paramref name="includeCollections"/> parameter.
    /// </summary>
    /// <param name="includeCollections">
    ///   Determines if <see cref="Attributes"/>, <see cref="Relationships"/>, and <see cref="References"/> should be included.
    /// </param>
    /// <param name="version">
    ///   The <see cref="DateTime"/> value that the attributes were last saved. This corresponds to the <see cref="Topic.
    ///   VersionHistory"/>.
    /// </param>
    public void MarkClean(bool includeCollections = false, DateTime? version = null) {
      _isDirty = false;
      if (includeCollections) {
        Attributes.MarkClean(version);
        Relationships.MarkClean();
        References.MarkClean();
      }
    }

    #endregion

    #region Relationship and Collection Properties

    /*==========================================================================================================================
    | PROPERTY: BASE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reference to the topic that this topic inherits from, if available.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Base topics allow attribute values to be inherited from another topic. When a <see cref="BaseTopic"/> is configured
    ///     as a <c>BaseTopic</c> <see cref="Topic.References"/>, values from that <see cref="Topic"/> are used when the <see
    ///     cref="AttributeValueCollection.GetValue(String, Boolean)" /> method is unable to find a local value for the
    ///     attribute.
    ///   </para>
    ///   <para>
    ///     Be aware that while multiple levels of <see cref="BaseTopic"/>s can be configured, the <see
    ///     cref="AttributeValueCollection.GetValue(String, Boolean)" /> method defaults to a maximum level of five "hops" in
    ///     order to help avoid an infinite loop.
    ///   </para>
    ///   <para>
    ///     The underlying value of the <see cref="BaseTopic"/> is stored as a topic reference with the <see cref="
    ///     KeyValuesPair{String, Topic}.Key"/> of <c>BaseTopic</c> in <see cref="Topic.References"/>. If the <see cref="
    ///     Topic"/> hasn't been saved, then the relationship will be established, but the <c>BaseTopic</c> won't be persisted
    ///     to the underlying repository upon <see cref="Repositories.ITopicRepository.Save"/>. That said, when <see cref="
    ///     Repositories.ITopicRepository.Save(Topic, Boolean)"/> is called, the <see cref="BaseTopic"/> will be reevaluated
    ///     and, if it has subsequently been saved, and the <c>BaseTopic</c> will be updated accordingly. This allows in-memory
    ///     topic graphs to be constructed, while preventing invalid <see cref="Topic.Id"/>s from being persisted to the
    ///     underlying data storage. As a result, however, a <see cref="Topic"/> referencing an <see cref="BaseTopic"/> that is
    ///     unsaved will need to be saved again once the <see cref="BaseTopic"/> has been saved, assuming it's otherwise outside
    ///     the scope of the original <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)"/> call.
    ///   </para>
    /// </remarks>
    /// <value>The <see cref="Topic"/> that values should be inherited from, if not otherwise available.</value>
    /// <requires description="A topic key must not derive from itself." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    [ReferenceSetter]
    public Topic? BaseTopic {
      get => References.GetTopic("BaseTopic", false);
      set {
        Contract.Requires<ArgumentException>(
          value != this,
          "A topic may not derive from itself."
        );
        References.SetTopic("BaseTopic", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attributes is a generic property bag for keeping track of either named or arbitrary attributes, thus providing
    ///   significant extensibility.
    /// </summary>
    /// <remarks>
    ///   Attributes are stored via an <see cref="AttributeValue" /> class which, in addition to the Attribute Key and Value,
    ///   also track other metadata for the attribute, such as the version (via the <see cref="AttributeValue.LastModified" />
    ///   property) and whether it has been persisted to the database or not (via the <see cref="AttributeValue.IsDirty" />
    ///   property).
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s attributes.</value>
    public AttributeValueCollection Attributes { get; }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing related topics based on a relationship key; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The relationships property exposes a <see cref="Topic" /> with child topics representing named relationships (e.g.,
    ///   "Related" for related topics); those child topics in turn have child topics representing references to each related
    ///   topic, thus allowing the topic hierarchy to be represented as a network graph.
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s relationships.</value>
    public TopicRelationshipMultiMap Relationships { get; }

    /*==========================================================================================================================
    | PROPERTY: REFERENCES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing referenced topics based on a reference key; can be used for base topics, etc.
    /// </summary>
    /// <remarks>
    ///   The references property exposes a <see cref="Topic" /> with child topics representing named references (e.g.,
    ///   <c>BaseTopic</c> for a <see cref="Topic.BaseTopic"/>).
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s relationships.</value>
    public TopicReferenceDictionary References { get; }

    /*==========================================================================================================================
    | PROPERTY: INCOMING RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing related topics based on a relationship key; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The incoming relationships property provides a reverse index of the <see cref="Relationships" /> property, in order to
    ///   indicate which topics point to the current topic. This can be useful for traversing the topic tree as a network graph.
    ///   This is of particular use for tags, where the current topic represents a tag, and the incoming relationships
    ///   represents all topics associated with that tag.
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s incoming relationships.</value>
    public TopicRelationshipMultiMap IncomingRelationships { get; }

    /*==========================================================================================================================
    | PROPERTY: VERSION HISTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of dates representing past versions of the topic, which can be rolled back to.
    /// </summary>
    /// <remarks>
    ///   It is expected that this collection will be populated by the <see cref="Repositories.ITopicRepository" /> (or one of
    ///   its derived providers).
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s version history.</value>
    public Collection<DateTime> VersionHistory { get; }

    #endregion

    #region Collection Methods

    /*==========================================================================================================================
    | METHOD: SET ATTRIBUTE VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Protected helper method that either adds a new <see cref="AttributeValue" /> object or updates the value of an
    ///   existing one, depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   When an attribute value is set and a corresponding, writable property exists on the topic, that property will be
    ///   called by the <see cref="AttributeValueCollection"/>. This is intended to enforce local business logic, and prevent
    ///   callers from introducing invalid data.To prevent a redirect loop, however, local properties need to inform the
    ///   <see cref="AttributeValueCollection"/> that the business logic has already been enforced. To do that, they must either
    ///   call <see cref="AttributeValueCollection.SetValue(String, String, Boolean?, Boolean, DateTime?, Boolean?)"/> with the
    ///   <c>enforceBusinessLogic</c> flag set to <c>false</c>, or, if they're in a separate assembly, call this overload.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty" />. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean)" />.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException"
    /// >
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException"
    /// >
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException"
    /// >
    ///   !value.Contains(" ")
    /// </requires>
    protected void SetAttributeValue(string key, string? value, bool? isDirty = null) {
      Contract.Requires(!String.IsNullOrWhiteSpace(key));
      Attributes.SetValue(key, value, isDirty, false);
    }

    #endregion

  } //Class
} //Namespace