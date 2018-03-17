/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Ignia.Topics.Collections;

namespace Ignia.Topics {

  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The Topic object is a simple container for a particular node in the topic hierarchy. It contains the metadata associated
  ///   with the particular node, a list of children, etc.
  /// </summary>
  public class Topic : IDisposable {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private                     int                             _id                             = -1;
    private                     string                          _key                            = null;
    private                     string                          _originalKey                    = null;
    private                     int                             _sortOrder                      = 25;
    private                     Topic                           _parent                         = null;
    private                     TopicCollection                 _children                       = null;
    private                     AttributeValueCollection        _attributes                     = null;
    private                     RelatedTopicCollection          _relationships                  = null;
    private                     RelatedTopicCollection          _incomingRelationships          = null;
    private                     Topic                           _derivedTopic                   = null;
    private                     List<DateTime>                  _versionHistory                 = null;

    #region Constructor

    /*==========================================================================================================================
    | CONSTRUCTOR
    >-----------=---------------------------------------------------------------------------------------------------------------
    | ### NOTE JJC082715: The empty constructor is a prerequisite of the factory method, which relies on Activator to create a
    | new instance of the object.
    \-----------=-------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="Topic"/> class.
    /// </summary>
    public Topic() { }

    /// <summary>
    ///   Deprecated. Initializes a new instance of the <see cref="Topic"/> class with the specified <see cref="Key"/> text
    ///   identifier.
    /// </summary>
    /// <remarks>
    ///   If available, topics should always be created using a strongly-typed derivative of the <see cref="Topic"/> class. This
    ///   is ensured by using the <see cref="TopicFactory.Create(String, String, Topic)"/> factory method. When constructing
    ///   derived types directly, however, this is implicit. In those cases, the derived class may create a constructor that
    ///   accepts "key" and calls this base constructor; it will automatically set the content type based on the derived class's
    ///   type.
    /// </remarks>
    /// <param name="key">
    ///   The string identifier for the <see cref="Topic"/>.
    /// </param>
    /// <requires description="The topic key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(topic)
    /// </requires>
    [Obsolete("The Topic(string) constructor is deprecated. Please use the static Create(string, string) factory method instead.", true)]
    protected Topic(string key) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      TopicFactory.ValidateKey(key);
      Key = key;
      ContentType = GetType().Name;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Topic"/> class with the specified <see cref="Key"/> text identifier and
    ///   <see cref="ContentType"/> name. Use the new <see cref="TopicFactory.Create(String, String, Topic)"/> factory method
    ///   instead, as this will return a strongly-typed version.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="Topic"/>.
    /// </param>
    /// <param name="contentType">
    ///   The text identifier for the Topic's <see cref="ContentType"/> Attribute.
    /// </param>
    /// <requires description="The topic key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(topic)
    /// </requires>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   description="The contentType key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    [Obsolete("The Topic(string, string) constructor is deprecated. Please use the static Create(string, string) factory method instead.", true)]
    public Topic(string key, string contentType) {
    }

    #endregion

    #region Core Properties

    /*==========================================================================================================================
    | PROPERTY: ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's integer identifier according to the data provider.
    /// </summary>
    /// <requires description="The id is expected to be a positive value." exception="T:System.ArgumentException">
    ///   value > 0
    /// </requires>
    public int Id {
      get => _id;
      set {
        Contract.Requires<ArgumentException>(value > 0, "The id is expected to be a positive value.");
        if (_id >= 0 && !_id.Equals(value)) {
          throw new ArgumentException("The value of this topic has already been set to " + _id + "; it cannot be changed.");
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
    /// <remarks>
    ///   While topics may be represented as a network graph via relationships, they are physically stored and primarily
    ///   represented via a hierarchy. As such, each topic may have at most a single parent. Note that the the root node will
    ///   have a null parent.
    /// </remarks>
    /// <requires description="The value for Parent must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    /// <requires description="A topic cannot be its own parent." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    public Topic Parent {
      get => _parent;
      set {

        /*----------------------------------------------------------------------------------------------------------------------
        | Check preconditions
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Requires<ArgumentNullException>(value != null, "The value for Parent must not be null.");
        Contract.Requires<ArgumentException>(value != this, "A topic cannot be its own parent.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Check that the topic's Parent is not the same before resetting it
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_parent == value) {
          return;
          }
        if (!value.Children.Contains(Key)) {
          value.Children.Add(this);
          }
        else {
          throw new Exception(
            "Duplicate key when setting Parent property: the topic with the name '" + Key +
            "' already exists in the '" + value.Key + "' topic."
            );
          }

        /*----------------------------------------------------------------------------------------------------------------------
        | Perform reordering and/or move
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_parent != null) {
          _parent.Children.Remove(Key);
        }

        _parent = value;

        SetAttributeValue("ParentID", value.Id.ToString(CultureInfo.InvariantCulture));

      }
    }

    /*==========================================================================================================================
    | PROPERTY: CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a keyed collection of child <see cref="Topic"/> instances associated with the current <see cref="Topic"/>.
    /// </summary>
    public TopicCollection Children {
      get {
        Contract.Ensures(Contract.Result<TopicCollection>() != null);
        if (_children == null) {
          _children = new TopicCollection(this);
        }
        return _children;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS EMPTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets whether the Topic's Key is invalid (null or empty).
    /// </summary>
    public bool IsEmpty => String.IsNullOrEmpty(Key);

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the key name of the content type that the current topic represents.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics
    ///   Editor (via the <see cref="ContentTypeDescriptor.AttributeDescriptors"/> property). The content type also determines,
    ///   by default, which view is rendered by the <see cref="Topics.ITopicRoutingService"/> (assuming the value isn't
    ///   overwritten down the pipe).
    /// </remarks>
    public string ContentType {
      get => Attributes.GetValue("ContentType");
      set => SetAttributeValue("ContentType", value);
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's Key attribute, the primary text identifier for the topic.
    /// </summary>
    /// <requires description="The value from the getter must not be null." exception="T:System.ArgumentNullException">
    ///   value != null
    /// </requires>
    /// <requires
    ///   description="The Key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    [AttributeSetter]
    public string Key {
      get => _key;
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        TopicFactory.ValidateKey(value);
        if (_originalKey == null) {
          _originalKey = Attributes.GetValue("Key", false);
        }
        //If an established key value is changed, the parent's index must be manually updated; this won't happen automatically.
        if (_originalKey != null && !value.Equals(_key) && Parent != null) {
          Parent.Children.ChangeKey(this, value);
        }
        SetAttributeValue("Key", value);
        _key = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the full, hierarchical identifier for the topic, including parents.
    /// </summary>
    /// <remarks>
    ///   The value for the UniqueKey property is a collated, colon-delimited representation of the topic and its parent(s).
    ///   Example: "Root:Configuration:ContentTypes:Page".
    /// </remarks>
    public string UniqueKey {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<string>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Crawl up tree to define uniqueKey
        \---------------------------------------------------------------------------------------------------------------------*/
        var uniqueKey = "";
        var topic = this;

        for (var i = 0; i < 100; i++) {
          if (uniqueKey.Length > 0) uniqueKey = ":" + uniqueKey;
          uniqueKey = topic.Key + uniqueKey;
          topic = topic.Parent;
          if (topic == null) break;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return value
        \---------------------------------------------------------------------------------------------------------------------*/
        return uniqueKey;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: ORIGINAL KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's original key.
    /// </summary>
    /// <remarks>
    ///   The original key is automatically set by <see cref="Key"/> when its value is updated (assuming the original key isn't
    ///   already set). This is, in turn, used by the <see cref="Repositories.RenameEventArgs"/> to represent the original value,
    ///   and thus allow the <see cref="Repositories.ITopicRepository"/> (or derived providers) from updating the data store
    ///   appropriately.
    /// </remarks>
    /// <requires
    ///   description="The OriginalKey should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value?.Contains(" ")?? true
    /// </requires>
    internal string OriginalKey {
      get => _originalKey;
      set {
        TopicFactory.ValidateKey(value, true);
        _originalKey = value;
      }
    }

    #endregion

    #region Convenience Properties

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the root-relative web path of the Topic, based on an assumption that the root topic is bound to the root of the
    ///   site.
    /// </summary>
    /// <remarks>
    ///   Note: If the topic root is not bound to the root of the site, this needs to specifically accounted for in any views
    ///   that reference the web path (e.g., by providing a prefix).
    /// </remarks>
    public string WebPath {
      get {
        Contract.Ensures(Contract.Result<string>() != null);
        var uniqueKey = UniqueKey.Replace("Root:", "/").Replace(":", "/") + "/";
        if (!uniqueKey.StartsWith("/")) {
          uniqueKey = "/" + uniqueKey;
        }
        return uniqueKey;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute, representing the default view to be used for the topic.
    /// </summary>
    /// <remarks>
    ///   This value can be set via the query string (via the <see cref="ITopicRoutingService"/> class), via the Accepts header
    ///   (also via the <see cref="ITopicRoutingService"/> class), on the topic itself (via this property), or via the
    ///   <see cref="ContentType"/>. By default, it will be set to the name of the <see cref="ContentType"/>; e.g., if the
    ///   Content Type is "Page", then the view will be "Page". This will cause the <see cref="ITopicRoutingService"/> to look
    ///   for a view at, for instance, /Common/Templates/Page/Page.aspx.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The View should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value?.Contains(" ")?? true
    /// </requires>
    [AttributeSetter]
    public string View {
      get =>
        // Return current Topic's View Attribute or the default for the ContentType.
        Attributes.GetValue("View", Attributes.GetValue("View", ""));
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        TopicFactory.ValidateKey(value);
        SetAttributeValue("View", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: IS HIDDEN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is hidden.
    /// </summary>
    [AttributeSetter]
    public bool IsHidden {
      get => Attributes.GetValue("IsHidden", "0").Equals("1");
      set => SetAttributeValue("IsHidden", value ? "1" : "0");
    }

    /*==========================================================================================================================
    | PROPERTY: IS DISABLED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets whether the current topic is disabled.
    /// </summary>
    [AttributeSetter]
    public bool IsDisabled {
      get => Attributes.GetValue("IsDisabled", "0").Equals("1");
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
    public bool IsVisible(bool showDisabled = false) => !IsHidden && (showDisabled || !IsDisabled);

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute, which represents the friendly name of the topic.
    /// </summary>
    /// <remarks>
    ///   While the <see cref="Key"/> may not contain, for instance, spaces or symbols, there are no restrictions on what
    ///   characters can be used in the title. For this reason, it provides the default public value for referencing topics. If
    ///   the title is not set, then this property falls back to the topic's <see cref="Key"/>.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    public string Title {
      get => Attributes.GetValue("Title", Key);
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
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    public string Description {
      get => Attributes.GetValue("Description");
      set => SetAttributeValue("Description", value);
    }

    /*==========================================================================================================================
    | PROPERTY: SORT ORDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's sort order.
    /// </summary>
    /// <remarks>
    ///   Sort order should be assigned by the <see cref="Repositories.ITopicRepository"/> (or one of its derived providers);
    ///   it may be based on an attribute or based on the physical order of records from the data source, depending on the
    ///   capabilities of the storage provider.
    /// </remarks>
    public int SortOrder {
      get => _sortOrder;
      set => _sortOrder = value;
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
    ///   overwrite this value for various reasons (such as backdating a webpage).
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value.ToString())
    /// </requires>
    public DateTime LastModified {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Return minimum date value, if LastModified is not already populated
        \---------------------------------------------------------------------------------------------------------------------*/
        if (String.IsNullOrWhiteSpace(Attributes.GetValue("LastModified", ""))) {
          return DateTime.MinValue;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return converted string attribute value, if available
        \---------------------------------------------------------------------------------------------------------------------*/
        var lastModified = Attributes.GetValue("LastModified");

        // Return converted DateTime
        if (DateTime.TryParse(lastModified, out var dateTimeValue)) {
          return dateTimeValue;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Otherwise, return default of minimum value
        \---------------------------------------------------------------------------------------------------------------------*/
        else {
          return DateTime.MinValue;
        }

      }
      set => SetAttributeValue("LastModified", value.ToString());
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
    ///     Boolean)"/> method unable to find a local value for the attribute.
    ///   </para>
    ///   <para>
    ///     Be aware that while multiple levels of derived topics can be configured, the <see
    ///     cref="AttributeValueCollection.GetValue(String, Boolean)"/> method defaults to a maximum level of five "hops".
    ///   </para>
    /// </remarks>
    /// <requires description="A topic key must not derive from itself." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    public Topic DerivedTopic {
      get => _derivedTopic;
      set {
        Contract.Requires<ArgumentException>(
          value != this,
          "A topic may not derive from itself."
        );
        _derivedTopic = value;
        if (value != null) {
          SetAttributeValue("TopicID", value.Id.ToString());
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
    ///   Attributes are stored via an <see cref="AttributeValue"/> class which, in addition to the Attribute Key and Value,
    ///   also track other metadata for the attribute, such as the version (via the <see cref="AttributeValue.LastModified"/>
    ///   property) and whether it has been persisted to the database or not (via the <see cref="AttributeValue.IsDirty"/>
    ///   property).
    /// </remarks>
    public AttributeValueCollection Attributes {
      get {
        Contract.Ensures(Contract.Result<AttributeValueCollection>() != null);
        if (_attributes == null) {
          _attributes = new AttributeValueCollection(this);
        }
        return _attributes;
      }
      internal set {
        Contract.Requires<ArgumentNullException>(value != null, "A topic's AttributeValue collection cannot be null.");
        _attributes = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing related topics based on a scope name; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The relationships property exposes a <see cref="Topic"/> with child topics representing named relationships (e.g.,
    ///   "Related" for related topics); those child topics in turn have child topics representing references to each related
    ///   topic, thus allowing the topic hierarchy to be represented as a network graph.
    /// </remarks>
    public RelatedTopicCollection Relationships {
      get {
        Contract.Ensures(Contract.Result<RelatedTopicCollection>() != null);
        if (_relationships == null) {
          _relationships = new RelatedTopicCollection(this, false);
        }
        return _relationships;
      }
    }

    /*===========================================================================================================================
    | PROPERTY: INCOMING RELATIONSHIPS
    \--------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A façade for accessing related topics based on a scope name; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The incoming relationships property provides a reverse index of the <see cref="Relationships"/> property, in order to
    ///   indicate which topics point to the current topic. This can be useful for traversing the topic tree as a network graph.
    ///   This is of particular use for tags, where the current topic represents a tag, and the incoming relationships represents
    ///   all topics associated with that tag.
    /// </remarks>
    public RelatedTopicCollection IncomingRelationships {
      get {
        Contract.Ensures(Contract.Result<RelatedTopicCollection>() != null);
        if (_incomingRelationships == null) {
          _incomingRelationships = new RelatedTopicCollection(this, true);
        }
        return _incomingRelationships;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VERSION HISTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of dates representing past versions of the topic, which can be rolled back to.
    /// </summary>
    /// <remarks>
    ///   It is expected that this collection will be populated by the <see cref="Repositories.ITopicRepository"/> (or one of
    ///   its derived providers).
    /// </remarks>
    public List<DateTime> VersionHistory {
      get {
        Contract.Ensures(Contract.Result<List<DateTime>>() != null);
        if (_versionHistory == null) {
          _versionHistory = new List<DateTime>();
        }
        return _versionHistory;
      }
    }

    #endregion

    #region Collection Methods

    /*==========================================================================================================================
    | METHOD: FIND ALL BY ATTRIBUTE
    >===========================================================================================================================
    | ###TODO JJC080313: Consider adding an overload of the out-of-the-box FindAll() method that supports recursion, thus
    | allowing a search by any criteria - including attributes.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="value">The text value for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   decription="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !name.Contains(" ")
    /// </requires>
    public ReadOnlyTopicCollection<Topic> FindAllByAttribute(string name, string value) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), "The attribute name must be specified.");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value), "The attribute value must be specified.");
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Topic>>() != null);
      TopicFactory.ValidateKey(name);

      /*----------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      var results = new TopicCollection();

      if (
        !String.IsNullOrEmpty(Attributes.GetValue(name)) &&
        Attributes.GetValue(name).IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0
        ) {
        results.Add(this);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Search children, if recursive
      \---------------------------------------------------------------------------------------------------------------------*/
      foreach (var topic in Children) {
        var nestedResults = topic.FindAllByAttribute(name, value);
        foreach (var matchedTopic in nestedResults) {
          if (!results.Contains(matchedTopic.Key)) {
            results.Add(matchedTopic);
          }
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return results
      \---------------------------------------------------------------------------------------------------------------------*/
      return results.AsReadOnly();

    }

    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value, optionally recursively.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="value">The text value for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse over the topic's children when performing the find operation.
    /// </param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    [Obsolete("The isRecursive parameter is obsolete. Use FindAllByAttribute(string, string) instead.", true)]
    public ReadOnlyTopicCollection<Topic> FindAllByAttribute(string name, string value, bool isRecursive = false) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate input
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name), "The attribute name must be specified.");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value), "The attribute value must be specified.");
      Contract.Ensures(Contract.Result<ReadOnlyTopicCollection<Topic>>() != null);

      /*----------------------------------------------------------------------------------------------------------------------
      | Provide a warning if the isRecursive parameter is used
      \---------------------------------------------------------------------------------------------------------------------*/
      if (!isRecursive) {
        throw new NotImplementedException("The isRecursive flag is obsolete and should not be used");
      }

      return FindAllByAttribute(name, value);
    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    >---------------------------------------------------------------------------------------------------------------------------
    | ### TODO JJC082715: Ultimately, the topicId overload should be used exclusively on a RootTopic class, and this version
    | should be made internal or protected. It generally only makes sense to grab a topic by ID starting from the root.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a topic object based on the current topic scope and the specified integer identifier.
    /// </summary>
    /// <remarks>
    ///   If the specified ID does not match the identifier for the current topic, its children will be searched.
    /// </remarks>
    /// <param name="topicId">The integer identifier for the topic.</param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    /// <requires description="The topicId is expected to be a positive integer." exception="T:System.ArgumentException">
    ///   topicId &lt;= 0
    /// </requires>
    public Topic GetTopic(int topicId) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentException>(topicId >= 0, "The topicId is expected to be a non-negative integer.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Return if current
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (Id == topicId) return this;

      /*------------------------------------------------------------------------------------------------------------------------
      | Iterate through children
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in Children) {
        var topic = childTopic.GetTopic(topicId);
        if (topic != null) return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return null if not found
      \-----------------------------------------------------------------------------------------------------------------------*/
      return null;

    }

    /// <summary>
    ///   Retrieves a topic object based on the specified namespace (<see cref="UniqueKey"/>) prefix and topic key.
    /// </summary>
    /// <param name="namespaceKey">The string value for the (uniqueKey prefixing) namespace for the topic.</param>
    /// <param name="topic">The partial or full string value representing the uniqueKey for the topic.</param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    public Topic GetTopic(string namespaceKey, string topic) {
      return GetTopic(String.IsNullOrEmpty(namespaceKey) ? topic : namespaceKey + ":" + topic);
    }

    /// <summary>
    ///   Retrieves a topic object based on the specified partial or full (prefixed) topic key.
    /// </summary>
    /// <param name="uniqueKey">
    ///   The partial or full string value representing the key (or <see cref="UniqueKey"/>) for the topic.
    /// </param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    public Topic GetTopic(string uniqueKey) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrWhiteSpace(uniqueKey)) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide shortcut for local calls
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (uniqueKey.IndexOf(":") < 0 && !uniqueKey.Equals("Root")) {
        if (Children.Contains(uniqueKey)) {
          return Children[uniqueKey];
        }
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide implicit root
      >-------------------------------------------------------------------------------------------------------------------------
      | ###NOTE JJC080313: While a root topic is required by the data structure, it should be implicit from the perspective of
      | the calling application.  A developer should be able to call GetTopic("Namepace:TopicPath") to get to a topic, without
      | needing to be aware of the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        !uniqueKey.StartsWith("Root:", StringComparison.OrdinalIgnoreCase) &&
        !uniqueKey.Equals("Root", StringComparison.OrdinalIgnoreCase)
        ) {
        uniqueKey = "Root:" + uniqueKey;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!uniqueKey.StartsWith(UniqueKey, StringComparison.OrdinalIgnoreCase)) return null;
      if (uniqueKey.Equals(UniqueKey, StringComparison.OrdinalIgnoreCase)) return this;

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var remainder = uniqueKey.Substring(UniqueKey.Length + 1);
      var marker = remainder.IndexOf(":", StringComparison.Ordinal);
      var nextChild = (marker < 0) ? remainder : remainder.Substring(0, marker);

      /*------------------------------------------------------------------------------------------------------------------------
      | Find topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!Children.Contains(nextChild)) return null;

      if (nextChild == remainder) return Children[nextChild];

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      return Children[nextChild]?.GetTopic(uniqueKey);

    }

    /*==========================================================================================================================
    | METHOD: SET ATTRIBUTE VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Protected helper method that either adds a new <see cref="AttributeValue"/> object or updates the value of an existing
    ///   one, depending on whether that value already exists.
    /// </summary>
    /// <remarks>
    ///   When an attribute value is set and a corresponding, writable property exists on the topic, that property will be
    ///   called by the AttributeValueCollection.This is intended to enforce local business logic, and prevent callers from
    ///   introducing invalid data.To prevent a redirect loop, however, local properties need to inform the
    ///   AttributeValueCollection that the business logic has already been enforced.To do that, they must either call
    ///   SetValue() with the enforceBusinessLogic flag set to false, or, if they're in a separate assembly, call this overload.
    /// </remarks>
    /// <param name="key">The string identifier for the AttributeValue.</param>
    /// <param name="value">The text value for the AttributeValue.</param>
    /// <param name="isDirty">
    ///   Specified whether the value should be marked as <see cref="AttributeValue.IsDirty"/>. By default, it will be marked as
    ///   dirty if the value is new or has changed from a previous value. By setting this parameter, that behavior is
    ///   overwritten to accept whatever value is submitted. This can be used, for instance, to prevent an update from being
    ///   persisted to the data store on <see cref="Topic.Save(Boolean, Boolean)"/>.
    /// </param>
    /// <requires
    ///   description="The key must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   description="The value must be specified for the AttributeValue key/value pair."
    ///   exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The key should be an alphanumeric sequence; it should not contain spaces or symbols"
    ///   exception="T:System.ArgumentException">
    ///   !value.Contains(" ")
    /// </requires>
    protected void SetAttributeValue(string key, string value, bool? isDirty = null) {
      Contract.Requires(!String.IsNullOrWhiteSpace(key));
      Attributes.SetValue(key, value, isDirty, false);
    }

    #endregion

    #region Interface Implementations

    /*==========================================================================================================================
    | METHOD: DISPOSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Technically, there's nothing to be done when disposing a Topic. However, this allows the topic attributes (and
    ///   properties) to be set using a using statement, which is syntactically convenient.
    /// </summary>
    public virtual void Dispose() => GC.SuppressFinalize(this);

    #endregion

  } // Class

} // Namespace
