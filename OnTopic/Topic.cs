/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Internal.Diagnostics;
using OnTopic.Metadata;

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
    private                     int                             _id                             = -1;
    private                     string?                         _originalKey                    = null;
    private                     Topic?                          _parent                         = null;
    private                     Topic?                          _derivedTopic                   = null;

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
    public Topic(string key, string contentType, Topic parent, int id = -1) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      Children                  = new TopicCollection();
      Attributes                = new AttributeValueCollection(this);
      IncomingRelationships     = new RelatedTopicCollection(this, true);
      Relationships             = new RelatedTopicCollection(this, false);
      VersionHistory            = new List<DateTime>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Set core properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      Key                       = key;
      ContentType               = contentType;
      Parent                    = parent;

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize key
      \-----------------------------------------------------------------------------------------------------------------------*/
      //###HACK JJC20190924: The local backing field _key is always initialized at this point. But Roslyn's flow analysis
      //isn't smart enough to detect this. As such, the following effectively sets _key to itself.
      _key = Key;

      /*------------------------------------------------------------------------------------------------------------------------
      | If ID is set, ensure attributes are not marked as IsDirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (id >= 0) {
        Id                      = id;
        Attributes.SetValue("Key", key, false, false);
        Attributes.SetValue("ContentType", contentType, false, false);
        if (parent != null) {
          Attributes.SetValue("ParentId", parent.Id.ToString(CultureInfo.InvariantCulture), false, false);
        }
      }

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
          throw new ArgumentException($"The value of this topic has already been set to {_id}; it cannot be changed.");
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
    ///   value != null
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
    public TopicCollection Children { get; }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors" /> property). The content type also determines,
    ///   by default, which view is rendered by the <see cref="ITopicRoutingService" /> (assuming the value isn't overwritten
    ///   down the pipe).
    /// </remarks>
    /// <value>
    ///   The key of the current <see cref="Topic"/>'s <see cref="ContentTypeDescriptor"/>.
    /// </value>
    public string ContentType {
      get => Attributes.GetValue("ContentType")?? "";
      set => SetAttributeValue("ContentType", value);
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
    ///   value != null
    /// </requires>
    /// <requires
    ///   description="The Key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException"
    /// >
    ///   !value.Contains(" ")
    /// </requires>
    [AttributeSetter]
    public string Key {
      get => _key;
      set {
        TopicFactory.ValidateKey(value);
        if (_originalKey == null) {
          _originalKey = Attributes.GetValue("Key", _key, false, false);
        }
        //If an established key value is changed, the parent's index must be manually updated; this won't happen automatically.
        if (_originalKey != null && !value.Equals(_key, StringComparison.InvariantCultureIgnoreCase) && Parent != null) {
          Parent.Children.ChangeKey(this, value);
        }
        SetAttributeValue("Key", value);
        _key = value;
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
    ///   already set). This is, in turn, used by the <see cref="Repositories.RenameEventArgs" /> to represent the original
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
    ///   This value can be set via the query string (via the <see cref="ITopicRoutingService" /> class), via the Accepts header
    ///   (also via the <see cref="ITopicRoutingService" /> class), on the topic itself (via this property). By default, it will
    ///   be set to the name of the <see cref="ContentType" />; e.g., if the Content Type is "Page", then the view will be
    ///   "Page". This will cause the <see cref="ITopicRoutingService" /> to look for a view at, for instance,
    ///   /Common/Templates/Page/Page.aspx.
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
      get => Attributes.GetBoolean("IsHidden", false);
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
      get => Attributes.GetBoolean("IsDisabled", false);
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
      if (parent.GetUniqueKey().StartsWith(GetUniqueKey(), StringComparison.InvariantCultureIgnoreCase)) {
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
      if (_parent != null) {
        _parent.Children.Remove(Key);
      }
      var insertAt = (sibling != null)? parent.Children.IndexOf(sibling)+1 : 0;
      parent.Children.Insert(insertAt, this);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set parent values
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_parent != parent) {
        _parent = parent;
        SetAttributeValue("ParentID", parent.Id.ToString(CultureInfo.InvariantCulture));
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
    public string GetUniqueKey() {

      /*------------------------------------------------------------------------------------------------------------------------
      | Crawl up tree to define uniqueKey
      \-----------------------------------------------------------------------------------------------------------------------*/
      var uniqueKey = "";
      var topic = (Topic?)this;

      while (topic != null) {
        if (uniqueKey.Length > 0) uniqueKey = $":{uniqueKey}";
        uniqueKey = topic.Key + uniqueKey;
        topic = topic.Parent;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return uniqueKey;

    }

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
      var uniqueKey = GetUniqueKey().Replace("Root:", "/").Replace(":", "/") + "/";
      if (!uniqueKey.StartsWith("/", StringComparison.InvariantCulture)) {
        uniqueKey = $"/{uniqueKey}";
      }
      return uniqueKey;
    }

    #endregion

    #region Relationship and Collection Properties

    /*==========================================================================================================================
    | PROPERTY: DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reference to the topic that this topic is derived from, if available.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Derived topics allow attribute values to be inherited from another topic. When a derived topic is configured via the
    ///     TopicId attribute key, values from that topic are used when the <see cref="AttributeValueCollection.GetValue(String,
    ///     Boolean)" /> method unable to find a local value for the attribute.
    ///   </para>
    ///   <para>
    ///     Be aware that while multiple levels of derived topics can be configured, the <see
    ///     cref="AttributeValueCollection.GetValue(String, Boolean)" /> method defaults to a maximum level of five "hops".
    ///   </para>
    /// </remarks>
    /// <value>The <see cref="Topic"/> that values should be derived from, if not otherwise available.</value>
    /// <requires description="A topic key must not derive from itself." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    public Topic? DerivedTopic {
      get => _derivedTopic;
      set {
        Contract.Requires<ArgumentException>(
          value != this,
          "A topic may not derive from itself."
        );
        if (!_derivedTopic?.Equals(value)?? false) {
          return;
        }
        _derivedTopic = value;
        if (value != null) {
          SetAttributeValue("TopicID", value.Id.ToString(CultureInfo.InvariantCulture));
        }
        else {
          Attributes.Remove("TopicID");
        }
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
    public RelatedTopicCollection Relationships { get; }

    /*===========================================================================================================================
    | PROPERTY: INCOMING RELATIONSHIPS
    \--------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing related topics based on a relationship key; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The incoming relationships property provides a reverse index of the <see cref="Relationships" /> property, in order to
    ///   indicate which topics point to the current topic. This can be useful for traversing the topic tree as a network graph.
    ///   This is of particular use for tags, where the current topic represents a tag, and the incoming relationships represents
    ///   all topics associated with that tag.
    /// </remarks>
    /// <value>The current <see cref="Topic"/>'s incoming relationships.</value>
    public RelatedTopicCollection IncomingRelationships { get; }

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
    public List<DateTime> VersionHistory { get; }

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
    ///   call <see cref="AttributeValueCollection.SetValue(String, String, Boolean?, Boolean)"/> with the
    ///   <c>enforceBusinessLogic</c> flag set to <c>false</c>, or, if they're in a separate assembly, call this overload.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty" />. By default, it will be marked
    ///   as dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Repositories.ITopicRepository.Save(Topic, Boolean, Boolean)" />.
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