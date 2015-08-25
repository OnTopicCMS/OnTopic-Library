/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace Ignia.Topics {
  
  /*============================================================================================================================
  | CLASS: TOPIC
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The Topic object is a simple container for a particular node in the topic hierarchy. It contains the metadata associated
  ///   with the particular node, a list of children, etc.
  /// </summary>
  public class Topic : KeyedCollection<string, Topic>, IDisposable {


    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     Topic                           _parent                 = null;
    private     AttributeValueCollection        _attributes             = null;
    private     int                             _id                     = -1;
    private     string                          _key                    = null;
    private     ContentType                     _contentType            = null;
    private     string                          _originalKey            = null;
    private     Topic                           _relationships          = null;
    private     Topic                           _incomingRelationships  = null;
    private     int                             _sortOrder              = 25;
    private     Topic                           _derivedTopic           = null;
    private     List<DateTime>                  _versionHistory         = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-----------=-------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Initializes a new instance of the <see cref="Topic"/> class.
    /// </summary>
    public Topic() : base(StringComparer.OrdinalIgnoreCase) { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Topic"/> class with the specified <see cref="Key"/> text identifier.
    /// </summary>
    /// <param name="key">
    ///   The string identifier for the <see cref="Topic"/>.
    /// </param>
    /// <requires description="The topic key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(topic)
    /// </requires>
    public Topic(string key) : base(StringComparer.OrdinalIgnoreCase) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      this.Key                  = key;
    }

    /// <summary>
    ///   Deprecated. Initializes a new instance of the <see cref="Topic"/> class with the specified <see cref="Key"/> text identifier and
    ///   <see cref="ContentType"/> name. Use the new <see cref="Create(string, string)"/> factory method instead, as this will return a 
    ///   strongly-typed version. 
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
    public Topic(string key, string contentType) : base(StringComparer.OrdinalIgnoreCase) {
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key), "key");
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType), "contentType");
      Contract.Requires<ArgumentException>(
        !contentType.Contains(" "),
        "The contentType key should be an alphanumeric sequence; it should not contain spaces or symbols"
      );
      this.Key                  = key;
      this.Attributes.Add(new AttributeValue("ContentType", contentType));
    }

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
      get {
        return _id;
      }
      set {
        Contract.Requires<ArgumentException>(value > 0, "The id is expected to be a positive value.");
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
      get {
        return _parent;
      }
      set {

        Contract.Requires<ArgumentNullException>(value != null, "The value for Parent must not be null.");
        Contract.Requires<ArgumentException>(value != this, "A topic cannot be its own parent.");

        /*----------------------------------------------------------------------------------------------------------------------
        | Check that the topic's Parent is not the same before resetting it
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_parent == value) {
          return;
          }
        if (!value.Contains(this.Key)) {
          value.Add(this);
          }
        else {
          throw new Exception(
            "Duplicate key when setting Parent property: the topic with the name '" + this.Key + 
            "' already exists in the '" + value.Key + "' topic."
            );
          }

        /*----------------------------------------------------------------------------------------------------------------------
        | Perform reordering and/or move
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_parent != null) {
          ReorderSiblings(value);
          TopicRepository.DataProvider.Move(this, value);
          _parent.Remove(this.Key);
        }

        _parent = value;

        Attributes.Set("ParentID", value.Id.ToString(CultureInfo.InvariantCulture));

      }
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the strongly-typed <see cref="ContentType"/> attribute.
    /// </summary>
    /// <remarks>
    ///   Each topic is associated with a content type. The content type determines which attributes are displayed in the Topics 
    ///   Editor (via the <see cref="ContentType.SupportedAttributes"/> property). The content type also determines, by default, 
    ///   which view is rendered by the <see cref="Topics.Web.TopicsRouteHandler"/> (assuming the value isn't overwritten down 
    ///   the pipe). 
    /// </remarks>
    public ContentType ContentType {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Create singleton reference to content type object in repository
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_contentType == null) {
          if (
            Attributes.Contains("ContentType") &&
            !String.IsNullOrEmpty(Attributes["ContentType"].Value) &&
            TopicRepository.ContentTypes.Contains(Attributes["ContentType"].Value)
          ) {
            _contentType = TopicRepository.ContentTypes[Attributes["ContentType"].Value];
          }
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | If content type doesn't exist, default to Container.
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_contentType == null) {
          _contentType = TopicRepository.ContentTypes["Container"];
        }

        return _contentType;

      }
      set {
        _contentType = value;
      }
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
    public string Key {
      get {
        return _key;
      }
      set {
        Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
        Contract.Requires<ArgumentException>(
          !value.Contains(" "),
          "The Key should be an alphanumeric sequence; it should not contain spaces or symbols"
        );
        if (_originalKey == null) {
          _originalKey = Attributes.Get("Key", null);
        }
        if (_originalKey != null && !value.Equals(Key) && Parent != null) {
          Parent.ChangeKey(this, value);
        }
        Attributes.Set("Key", value);
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
        Contract.Ensures(Contract.Result<String>() != null);

        /*----------------------------------------------------------------------------------------------------------------------
        | Crawl up tree to define uniqueKey
        \---------------------------------------------------------------------------------------------------------------------*/
        string uniqueKey = "";
        Topic topic = this;

        for (int i = 0; i < 100; i++) {
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
    >---------------------------------------------------------------------------------------------------------------------------
    | ### TODO JJC081115: Is it necessary for this to have a setter? I would assume this would only be set internally by, for 
    | instance, changing the Key property. If so, allowing external access may cause problems. 
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's original key.
    /// </summary>
    /// <remarks>
    ///   The original key is automatically set by <see cref="Key"/> when its value is updated (assuming the original key isn't
    ///   already set). This is, in turn, used by the <see cref="Providers.RenameEventArgs"/> to represent the original value, 
    ///   and thus allow the <see cref="Providers.TopicDataProviderBase"/> (or derived providers) from updating the data store
    ///   appropriately.
    /// </remarks>
    /// <requires
    ///   description="The OriginalKey should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value?.Contains(" ")?? true
    /// </requires>
    public string OriginalKey {
      get {
        return _originalKey;
      }
      set {
        Contract.Requires<ArgumentException>(
          !value?.Contains(" ")?? true,
          "The OriginalKey should be an alphanumeric sequence; it should not contain spaces or symbols"
        );
        _originalKey = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the root-relative web path of the Topic, based on an assumption that the <see cref="TopicRepository.RootTopic"/>
    ///   is bound to the root of the site.
    /// </summary>
    /// <remarks>
    ///   Note: If the topic root is not bound to the root of the site, this needs to specifically accounted for in any views 
    ///   that reference the web path (e.g., by providing a prefix).
    /// </remarks>
    public string WebPath {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Validate return value
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Ensures(Contract.Result<String>() != null);

        return UniqueKey.Replace("Root:", "/").Replace(":", "/") + "/";
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute, representing the default view to be used for the topic.
    /// </summary>
    /// <remarks>
    ///   This value can be set via the query string (via the <see cref="Ignia.Topics.Web.TopicsRouteHandler"/> class), via the
    ///   Accepts header (also via the <see cref="Ignia.Topics.Web.TopicsRouteHandler"/> class), on the topic itself (via this
    ///   property), or via the <see cref="ContentType"/>. By default, it will be set to the name of the 
    ///   <see cref="ContentType"/>; e.g., if the Content Type is "Page", then the view will be "Page". This will cause the 
    ///   <see cref="Ignia.Topics.Web.TopicsRouteHandler"/> to look for a view at, for instance, 
    ///   /Common/Templates/Page/Page.aspx.
    /// </remarks>
    /// <requires description="The value from the getter must be provided." exception="T:System.ArgumentNullException">
    ///   !string.IsNullOrWhiteSpace(value)
    /// </requires>
    /// <requires
    ///   description="The View should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !value?.Contains(" ")?? true
    /// </requires>
    public string View {
      get {
        // Return current Topic's View Attribute or the default for the ContentType.
        return Attributes.Get("View", ContentType.Attributes.Get("View", ContentType.Key));
      }
      set {
        Contract.Requires(!string.IsNullOrWhiteSpace(value));
        Contract.Requires<ArgumentException>(
          !value?.Contains(" ")?? true,
          "The View should be an alphanumeric sequence; it should not contain spaces or symbols"
        );
        Attributes.Set("View", value);
      }
    }

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
      get {
        return Attributes.Get("Title", Key);
      }
      set {
        Contract.Requires(!string.IsNullOrWhiteSpace(value));
        Attributes.Set("Title", value);
      }
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
      get {
        return Attributes.Get("Description");
      }
      set {
        Contract.Requires(!string.IsNullOrWhiteSpace(value));
        Attributes.Set("Description", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: SORT ORDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's sort order.
    /// </summary>
    /// <remarks>
    ///   Sort order should be assigned by the <see cref="Providers.TopicDataProviderBase"/> (or one of its derived providers); 
    ///   it may be based on an attribute or based on the physical order of records from the data source, depending on the 
    ///   capabilities of the storage provider.
    /// </remarks>
    public int SortOrder {
      get {
        return _sortOrder;
      }
      set {
        _sortOrder = value;
      }
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
        if (String.IsNullOrEmpty(Attributes.Get("LastModified", ""))) {
          return DateTime.MinValue;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return converted string attribute value, if available
        \---------------------------------------------------------------------------------------------------------------------*/
        string lastModified = Attributes.Get("LastModified");
        DateTime dateTimeValue;

        // Return converted DateTime
        if (DateTime.TryParse(lastModified, out dateTimeValue)) {
          return dateTimeValue;
        }

        // Return minimum date value if datetime cannot be parsed from attribute
        else {
          return DateTime.MinValue;
        }

      }
      set {
        Contract.Requires(!string.IsNullOrWhiteSpace(value.ToString()));
        Attributes.Set("LastModified", value.ToString());
      }
    }

    /*==========================================================================================================================
    | PROPERTY: DERIVED TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Reference to the topic that this topic is derived from, if available.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Derived topics allow attribute values to be inherited from another topic. When a derived topic is configured via the 
    ///     TopicId attribute key, values from that topic are used when the <see cref="AttributeValueCollection.Get(string, 
    ///     bool)"/> method unable to find a local value for the attribute. 
    ///   </para>  
    ///   <para>
    ///     Be aware that while multiple levels of derived topics can be configured, the <see 
    ///     cref="AttributeValueCollection.Get(string, bool)"/> method defaults to a maximum level of five "hops". 
    ///   </para>
    /// </remarks>
    /// <requires description="A topic key must not derive from itself." exception="T:System.ArgumentException">
    ///   value != this
    /// </requires>
    public Topic DerivedTopic {
      get {
        if (_derivedTopic == null && this.Attributes.Contains("TopicID")) {
          Contract.Assume(
            !String.IsNullOrWhiteSpace(this.Attributes["TopicID"].Value),
            "Assume the topic's TopicID AttributeValue Value is not null"
            );
          int topicId = 0;
          bool success = Int32.TryParse(this.Attributes["TopicID"].Value.ToString(), out topicId);
          if (!success || topicId == 0) return null;
          _derivedTopic = TopicRepository.RootTopic.GetTopic(topicId);
        }
        return _derivedTopic;
      }
      set {
        Contract.Requires<ArgumentException>(
          value != this,
          "A topic may not derive from itself."
        );
        _derivedTopic = value;
        if (value != null) {
          this.Attributes.Set("TopicID", value.Id.ToString());
        }
        else if (this.Attributes.Contains("TopicID")) {
          this.Attributes.Remove("TopicID");
        }
      }
    }

    /*==========================================================================================================================
    | ###TODO JJC080314: An overload of the constructor should be created to accept an XmlDocument or XmlNode based on the
    | proposed Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
      public Topic(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) : base(StringComparer.OrdinalIgnoreCase) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects
      //are already created and available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | PROPERTY: ATTRIBUTES
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: The Attributes type should be changed to use either KeyedCollection (ideally, with the
    | INotifyCollectionChanged interface) or ObservableCollection(with a string indexer and duplicate key check). To
    | begin, it is recommended that this be converted to use the more standard KeyedCollection, which can be upgraded to
    | use INotifyCollectionChanged at a later date. When it is made observable, this can (and should) be used specifically
    | to intercept changes to either ParentID or Key, since they have specific implications in terms of the data integrity
    | of the collection.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE KLT081314: Attributes is now of type AttributeValueCollection
    | (KeyedCollection<string, AttributeValue">). Extending the collection to incorporate the
    | INotifyCollectionChanged interface or converting it to an ObservableCollection remains an item for future development.
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
      set {
        Contract.Requires<ArgumentNullException>(value != null, "A topic's AttributeValue collection cannot be null.");
        _attributes = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A dictionary of namespaced relationships to other topics; can be used for tags, related topics, etc.
    /// </summary>
    /// <remarks>
    ///   The relationships property exposes a <see cref="Topic"/> with child topics representing named relationships (e.g., 
    ///   "Related" for related topics); those child topics in turn have child topics representing references to each related 
    ///   topic, thus allowing the topic hierarchy to be represented as a network graph.
    /// </remarks>
    public Topic Relationships {
      get {
        Contract.Ensures(Contract.Result<Topic>() != null);
        if (_relationships == null) {
          _relationships = new Topic();
        }
        return _relationships;
      }
    }

    /*===========================================================================================================================
    | PROPERTY: INCOMING RELATIONSHIPS
    \--------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A dictionary of namespaced relationships from other topics; can be used for tags, related topics, etc. 
    /// </summary>
    /// <remarks>
    ///   The incoming relationships property provides a reverse index of the <see cref="Relationships"/> property, in order to
    ///   indicate which topics point to the current topic. This can be useful for traversing the topic tree as a network graph.
    ///   This is of particular use for tags, where the current topic represents a tag, and the incoming relationships represents
    ///   all topics associated with that tag.
    /// </remarks>
    public Topic IncomingRelationships {
      get {
        Contract.Ensures(Contract.Result<Topic>() != null);
        if (_incomingRelationships == null) {
          _incomingRelationships = new Topic();
        }
        return _incomingRelationships;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: SORTED CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the values collection, sorted by the <see cref="SortOrder"/> property.  
    /// </summary>
    /// <remarks>
    ///   Since Dictionaries do not guarantee sort order, this is necessary for any code that expects to honor the order of
    ///   topics in the database. Simply calling <see cref="Collection{T}.Items"/> may return topics in the correct order, but
    ///   this cannot be assumed.
    /// </remarks>
    public IEnumerable<Topic> SortedChildren {
      get {
        Contract.Ensures(Contract.Result<IEnumerable<Topic>>() != null);
        return this.Items.OrderBy(topic => topic.SortOrder);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: NESTED TOPICS
    >---------------------------------------------------------------------------------------------------------------------------
    | ###TODO JJC080314: Ideally, this property should return a KeyedCollection of the underlying Topics filtered by
    | ContentType, but with the key removing the preceding underscore.This would need to be a specialized version of the
    | KeyedCollection class, possibly a derivitive of the NestedTopics class. Preferrably, this will be dynamically created
    | based on a reference back to the parent class (this), in order to ensure synchronization between NestedTopics and the
    | parent collection.
    >---------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///   Provides a reference to the values collection, filtered by Topics of the <see cref="ContentType"/> TopicsList, which
    ///   represent Nested Topics.
    /// </summary>
    public Topic NestedTopics {
      get {
        throw new NotImplementedException();
      }
    }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | PROPERTY: CHILD TOPICS
    >---------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///   Provides a reference to the values collection, filtered by Topics that are NOT of the <see cref="ContentType"/>
    ///   TopicList, which represent Child Topics. This provides a complement to the <see cref="NestedTopics"/> collection.
    /// </summary>
    public Topic ChildTopics {
      get {
        throw new NotImplementedException();
      }
    }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: CHANGE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the key associated with a topic to maintain referential integrity.
    /// </summary>
    /// <remarks>
    ///   By default, <see cref="KeyedCollection{TKey, TItem}"/> doesn't permit mutable keys; this mitigates that issue by
    ///   allowing the collection's lookup dictionary to be updated whenever the key is updated in the corresponding topic
    ///   object.
    /// </remarks>
    /// <param name="topic">The topic object for which the <see cref="Key"/> should be changed.</param>
    /// <param name="newKey">The string value for the new key.</param>
    internal void ChangeKey(Topic topic, string newKey) {
      base.ChangeItemKey(topic, newKey);
    }

    /*==========================================================================================================================
    | PROPERTY: VERSIONS
    >===========================================================================================================================
    | Getter for collection of previous versions that can be rolled back to.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###TODO JJC080314: The Versions property simply exposes a list of Versions that can be rolled back to. Each Version object
    | will contain the version date, and may contain the author's name and version notes.
    >---------------------------------------------------------------------------------------------------------------------------
      public VersionCollection Versions {
        get {
        //Since versions won't normally be required, except during rollback scenarios, may alternatively configure these to load
        //on demand.
          return _versions;
        }
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | PROPERTY: VERSION HISTORY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a collection of dates representing past versions of the topic, which can be rolled back to.
    /// </summary>
    /// <remarks>
    ///   It is expected that this collection will be populated by the <see cref="Providers.TopicDataProviderBase"/> (or one of
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

    /*==========================================================================================================================
    | METHOD: SET RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Set the collection of related items (by scope) via a CSV file.
    /// </summary>
    /// <remarks>
    ///   Not currently referenced by the library.
    /// </remarks>
    /// <param name="scope">The string identifier describing the type of relationship (e.g., "Related").</param>
    /// <param name="relatedCsv">The source CSV file containing the relationship data.</param>
    /// <requires description="The scope must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(scope)
    /// </requires>
    /// <requires
    ///   description="The scope should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentNullException">
    ///   !scope.Contains(" ")
    /// </requires>
    public void SetRelationship(string scope, string relatedCsv) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope), "scope");
      Contract.Requires<ArgumentException>(
        !scope.Contains(" "),
        "The scope should be an alphanumeric sequence; it should not contain spaces or symbols"
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle Deletion
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(relatedCsv)) {
        if (this.Relationships.Contains(scope)) {
          this.Relationships.Remove(scope);
        }
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Build collection from CSV
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic related = new Topic(scope);
      char[] stringSeparators = new char[] {','};

      string[] csv = relatedCsv.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

      for (int i=0; i < csv.Length; i++) {
        int number;
        bool result = Int32.TryParse(csv[i], out number);
        if (result) {
          Topic topic = TopicRepository.RootTopic.GetTopic(number);
          if (topic == null) {
            throw new ArgumentException("SetRelationship failed to find TopicId: " + number.ToString(CultureInfo.InvariantCulture));
          }
          if (!related.Contains(topic.Key)) {
            related.Add(topic);
          }
        }
        else {
          if (csv[i] == null) csv[i] = "";
          throw new ArgumentException("Attempted conversion of '" + csv[i] + "' failed.");
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set incoming relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (Topic relatedTopic in related) {
        relatedTopic.SetRelationship(scope, this, true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set property value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (this.Relationships.Contains(related.Key)) {
        this.Relationships.Remove(related.Key);
      }
      this.Relationships.Add(related);

    }

    /*==========================================================================================================================
    | METHOD: SET RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a new relationship based on the relationship type, target topic and direction.
    /// </summary>
    /// <param name="scope">The string identifier describing the type of relationship (e.g., "Related").</param>
    /// <param name="related">The target topic object for which to set the relationship.</param>
    /// <param name="isIncoming">Boolean value indicating the direction of the relationship.</param>
    /// <requires description="The scope must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(scope)
    /// </requires>
    /// <requires description="The related topic must be specified." exception="T:System.ArgumentNullException">
    ///   related != null
    /// </requires>
    /// <requires
    ///   description="The scope should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentNullException">
    ///   !scope.Contains(" ")
    /// </requires>
    /// <requires description="A topic cannot be related to itself." exception="T:System.ArgumentException">related != this</requires>
    public void SetRelationship(string scope, Topic related, bool isIncoming = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(scope));
      Contract.Requires<ArgumentNullException>(related != null);
      Contract.Requires<ArgumentException>(
        !scope.Contains(" "),
        "The scope should be an alphanumeric sequence; it should not contain spaces or symbols"
      );
      // Contract.Requires<ArgumentException>(related != this, "A topic cannot be related to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic relationships = this.Relationships;

      if (isIncoming) {
        relationships = this.IncomingRelationships;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Create new namespace, if not present
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!relationships.Contains(scope)) {
        relationships.Add(new Topic(scope));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add the relationship to the correct scoped key
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(relationships[scope] != null, "Assumes the Relationships item has been created.");
      if (!relationships[scope].Contains(related.Key)) {
        relationships[scope].Add(related);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set incoming relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!isIncoming) {
        related.SetRelationship(scope, this, true);
      }

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL BY ATTRIBUTE
    >===========================================================================================================================
    | ###TODO JJC080313: Consider adding an overload of the out-of-the-box FindAll() method that supports recursion, thus
    | allowing a search by any criteria - including attributes.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value, optionally recursively.
    /// </summary>
    /// <param name="name">The string identifier for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="value">The text value for the <see cref="AttributeValue"/> against which to be searched.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse over the topic's children when performing the find operation.
    /// </param>
    /// <returns>A collection of topics matching the input parameters.</returns>
    /// <requires description="The attribute name must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(name)
    /// </requires>
    /// <requires
    ///   decription="The name should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !name.Contains(" ")
    /// </requires>
    public Collection<Topic> FindAllByAttribute(string name, string value, bool isRecursive = false) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate input
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(name));
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(value));
      Contract.Requires<ArgumentException>(
        !name.Contains(" "),
        "The name should be an alphanumeric sequence; it should not contain spaces or symbols"
      );

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Collection<Topic>>() != null);

      /*----------------------------------------------------------------------------------------------------------------------
      | Search attributes
      \---------------------------------------------------------------------------------------------------------------------*/
      Collection<Topic> results = new Collection<Topic>();

      Contract.Assume(this.Attributes[name] != null, "Assumes the AttributeValue is available.");
      if (this.Attributes.Get(name).IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) {
        results.Add(this);
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Search children, if recursive
      \---------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (Topic topic in this) {
          Collection<Topic> nestedResults = topic.FindAllByAttribute(name, value, true);
          foreach (Topic matchedTopic in nestedResults) {
            results.Add(matchedTopic);
          }
        }
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return results
      \---------------------------------------------------------------------------------------------------------------------*/
      return results;

    }

    /*==========================================================================================================================
    | METHOD: CREATE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Factory method for creating new strongly-typed instances of the topics class, assuming a strongly-typed subclass is
    ///   available.
    /// </summary>
    /// <remarks>
    ///   The create method will look in the Ignia.Topics namespace for a class with the same name as the content type. For 
    ///   instance, if the content type is "Page", it will look for an "Ignia.Topics.Page" class. If found, it will confirm that
    ///   the class derives from the <see cref="Topic"/> class and, if so, return a new instance of that class. If the class 
    ///   exists but does not derive from <see cref="Topic"/>, then an exception will be thrown. And otherwise, a new instance
    ///   of the generic <see cref="Topic"/> class will be created.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    /// <requires description="The topic key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(key)
    /// </requires>
    /// <requires
    ///   decription="The key should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !key.Contains(" ")
    /// </requires>
    /// <requires description="The content type key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    public static Topic Create(string key, string contentType) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate inputs
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(key));
      Contract.Requires<ArgumentException>(
        !key.Contains(" "),
        "The key should be an alphanumeric sequence; it should not contain spaces or symbols"
      );
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType));
      Contract.Requires<ArgumentException>(
        !contentType.Contains(" "),
        "The contentType should be an alphanumeric sequence; it should not contain spaces or symbols"
      );

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate return value
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Ensures(Contract.Result<Topic>() != null);

      /*----------------------------------------------------------------------------------------------------------------------
      | Determine target type
      \---------------------------------------------------------------------------------------------------------------------*/
      Type baseType = Type.GetType("Ignia.Topics.Topic");
      Type targetType = Type.GetType("Ignia.Topics." + contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate type
      \---------------------------------------------------------------------------------------------------------------------*/
      if (targetType == null) {
        targetType = baseType;
      }
      else if (!targetType.IsSubclassOf(baseType)) {
        targetType = baseType;
        throw new ArgumentException("The topic \"Ignia.Topics." + contentType + "\" does not derive from \"Ignia.Topics.Topic\".");
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Identify the appropriate topic
      \---------------------------------------------------------------------------------------------------------------------*/
      Topic topic = (Topic)Activator.CreateInstance(targetType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Set the topic's Key and Content Type
      \---------------------------------------------------------------------------------------------------------------------*/
      topic.Key = key;
      topic.Attributes.Set("ContentType", contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Return the topic
      \---------------------------------------------------------------------------------------------------------------------*/
      return topic;

    }

    /// <summary>
    ///   Factory method for creating new strongly-typed instances of the topics class, assuming a strongly-typed subclass is
    ///   available. Used for cases where a <see cref="Topic"/> is being deserialized from an existing instance, as indicated
    ///   by the <paramref name="id"/> parameter.
    /// </summary>
    /// <remarks>
    ///   By default, when creating new attributes, the <see cref="AttributeValue"/>s for both <see cref="Key"/> and <see 
    ///   cref="ContentType"/> will be set to true, which is required in order to correctly save new topics to the database. 
    ///   When the <paramref name="id"/> parameter is set, however, the <see cref="Key"/> and <see cref="ContentType"/> on the 
    ///   new <see cref="Topic"/> are set to false, as it is assumed these are being set to the same values currently used in 
    ///   the persistance store.
    /// </remarks>
    /// <param name="key">A string representing the key for the new topic instance.</param>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <param name="id">The unique identifier assigned by the data store for an existing topic.</param>
    /// <exception cref="ArgumentException">
    ///   Thrown when the class representing the content type is found, but doesn't derive from <see cref="Topic"/>.
    /// </exception>
    /// <returns>A strongly-typed instance of the <see cref="Topic"/> class based on the target content type.</returns>
    public static Topic Create(string key, string contentType, int id) {
      Topic topic = Create(key, contentType);
      topic.Id = id;
      topic.Attributes["Key"].IsDirty = false;
      topic.Attributes["ContentType"].IsDirty = false;
      return topic;
    }

    /*==========================================================================================================================
    | METHOD: MERGE
    >---------------------------------------------------------------------------------------------------------------------------
    | ###TODO JJC080314: Similar to Load(), but should merge values with existing Topic rather than creating a new TOpic. Should
    | accept an XmlDocument or XmlNode based on the proposed Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
      public Topic Merge(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects
      //are already created and available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: ROLLBACK
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Rolls back the current topic to a particular point in its version history by reloading legacy attributes and then
    ///   saving the new version.
    /// </summary>
    /// <param name="version">The selected Date/Time for the version to which to roll back.</param>
    /// <requires 
    ///   description="The version requested for rollback does not exist in the version history."
    ///   exception="T:System.ArgumentNullException">
    ///   !VersionHistory.Contains(version)
    /// </requires>
    public void Rollback(DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentException>(
        !VersionHistory.Contains(version), 
        "The version requested for rollback does not exist in the version history"
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assert(version != null, "Confirms the version is available.");
      Topic originalVersion     = TopicRepository.DataProvider.Load(this.Id, version);

      /*------------------------------------------------------------------------------------------------------------------------
      | Rename topic, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Key                  = originalVersion.Key;

      /*------------------------------------------------------------------------------------------------------------------------
      | Mark each attribute as dirty
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (AttributeValue attribute in originalVersion.Attributes) {
        if (!this.Attributes.Contains(attribute.Key) || this.Attributes[attribute.Key].Value != attribute.Value) {
          attribute.IsDirty = true;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Construct new AttributeCollection
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Attributes           = originalVersion.Attributes;

      /*------------------------------------------------------------------------------------------------------------------------
      | Save as new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Save();

    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
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

      Contract.Requires<ArgumentException>(topicId >= 0, "The topicId is expected to be a non-negative integer.");

      if (this.Id == topicId) return this;

      foreach (Topic childTopic in this) {
        Topic topic = childTopic.GetTopic(topicId);
        if (topic != null) return topic;
      }

      return null;

    }

    /// <summary>
    ///   Retrieves a topic object based on the specified namespace (<see cref="UniqueKey"/>) prefix and topic key.
    /// </summary>
    /// <param name="namespaceKey">The string value for the (uniqueKey prefixing) namespace for the topic.</param>
    /// <param name="topic">The partial or full string value representing the uniqueKey for the topic.</param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    public Topic GetTopic(string namespaceKey, string topic) {
      return GetTopic(String.IsNullOrEmpty(namespaceKey)? topic : namespaceKey + ":" + topic);
    }

    /// <summary>
    ///   Retrieves a topic object based on the specified partial or full (prefixed) topic key.
    /// </summary>
    /// <param name="topic">
    ///   The partial or full string value representing the key (or <see cref="UniqueKey"/>) for the topic.
    /// </param>
    /// <returns>The topic or null, if the topic is not found.</returns>
    public Topic GetTopic(string topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(topic)) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide implicit root
      >-------------------------------------------------------------------------------------------------------------------------
      | ###NOTE JJC080313: While a root topic is required by the data structure, it should be implicit from the perspective of
      | the calling application.  A developer should be able to call GetTopic("Namepace:TopicPath") to get to a topic, without
      | needing to be aware of the root.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        !topic.StartsWith("Root:", StringComparison.OrdinalIgnoreCase) &&
        !topic.StartsWith("Categories:", StringComparison.OrdinalIgnoreCase) &&
        !topic.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
        !topic.Equals("Categories", StringComparison.OrdinalIgnoreCase)
        ) {
        topic = "Root:" + topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!topic.StartsWith(UniqueKey, StringComparison.OrdinalIgnoreCase)) return null;
      if (topic.Equals(UniqueKey, StringComparison.OrdinalIgnoreCase)) return this;

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      string   remainder        = topic.Substring(UniqueKey.Length + 1);
      int      marker           = remainder.IndexOf(":", StringComparison.Ordinal);
      string   nextChild        = (marker < 0)? remainder : remainder.Substring(0, marker);

      /*------------------------------------------------------------------------------------------------------------------------
      | Find topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!this.Contains(nextChild)) return null;

      if (nextChild == remainder) return this[nextChild];

      /*------------------------------------------------------------------------------------------------------------------------
      | Return the topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(this[nextChild] != null, "Assumes the child topic is available.");
      return this[nextChild].GetTopic(topic);

    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Saves the topic information.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows for specifying whether all children should also be saved, as well as whether the topic should
    ///   be marked as having Draft status.
    /// </remarks>
    /// <returns>The topic's integer identifier.</returns>
    public int Save() {
      return Save(false, false);
    }

    /// <summary>
    ///   Saves the topic information, optionally saving the information for all of its descendants; additionally, optionally
    ///   marks the topic as having a draft status.
    /// </summary>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>The topic's integer identifier.</returns>
    public int Save(bool isRecursive = false, bool isDraft = false) {
      Id = TopicRepository.DataProvider.Save(this, isRecursive, isDraft);
      return Id;
    }

    /*==========================================================================================================================
    | METHOD: REFRESH
    >---------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    ///   Reloads the topic data from the provider and the data for all of the topic's descendants.
    /// </summary>
    /// <returns>Boolean value representing whether the topic has been refreshed/reloaded.</returns>
    public bool Refresh() {
      return Refresh(true);
    }

    /// <summary>
    ///   Reloads the topic data from the provider, optionally reloading all of the topic's descendants.
    /// </summary>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and refresh them as well.
    /// </param>
    /// <returns>Boolean value representing whether the topic has been refreshed/reloaded.</returns>
    public bool Refresh(bool isRecursive) {
      throw new NotSupportedException("The Refresh() method is a placeholder for future functionality and is not yet supported.");
    }
    \-------------------------------------------------------------------------------------------------------------------------*/

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes the current topic (as well as all children).
    /// </summary>
    /// <remarks>
    ///   We may want to rethink this at some point and have functionality to delete a node and elavate it's children or
    ///   delete and reassign children or something.
    /// </remarks>
    public void Delete() {
      Delete(true);
    }

    /// <summary>
    ///   Deletes the current topic, optionally deleting all of the topic's descendants.
    /// </summary>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse over the topic's children and delete them as well.
    /// </param>
    public void Delete(bool isRecursive) {
      TopicRepository.DataProvider.Delete(this, isRecursive);
    }

    /*==========================================================================================================================
    | METHOD: MOVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Moves the topic to a new parent.
    /// </summary>
    /// <remarks>
    ///   For ordering: Optionally accepts the sibling topic to place topic behind. Defaults to placing topic in front of
    ///   existing siblings.
    /// </remarks>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    public bool Move(Topic target) {
      ReorderSiblings();
      return TopicRepository.DataProvider.Move(this, target);
    }

    /// <summary>
    ///   Moves the topic to a new parent, using one of the target topic's children as a reference point adjacent to which the 
    ///   source topic should be moved.
    /// </summary>
    /// <param name="target">A topic object under which to move the source topic.</param>
    /// <param name="sibling">A topic object representing a sibling adjacent to which the topic should be moved.</param>
    /// <returns>Boolean value representing whether the operation completed successfully.</returns>
    /// <requires
    ///   description="The topic may not be its own parent." exception="T:System.ArgumentException">
    ///   topic != target
    /// </requires>
    /// <requires
    ///   description="The topic cannot be moved or reordered relative to itself." exception="T:System.ArgumentException">
    ///   topic != sibling
    /// </requires>
    public bool Move(Topic target, Topic sibling) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentException>(this != target, "The topic may not be its own parent.");
      Contract.Requires<ArgumentException>(this != sibling, "The topic cannot be moved or reordered relative to itself.");

      ReorderSiblings(sibling);
      return TopicRepository.DataProvider.Move(this, target, sibling);
    }

    /*==========================================================================================================================
    | METHOD: REORDER SIBLINGS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static method that updates the sort order of topics at a particular level.
    /// </summary>
    /// <param name="sibling">
    ///   The topic object that if provided, represents the topic after which the source topic should be ordered.
    /// </param>
    /// <requires description="The source topic must be specified." exception="T:System.ArgumentNullException">
    ///   source != null
    /// </requires>
    /// <requires description="The source topic cannot be reordered relative to itself." exception="T:System.ArgumentException">
    ///   source != sibling
    /// </requires>
    private void ReorderSiblings(Topic sibling = null) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(this != null, "source");
      Contract.Requires<ArgumentException>(this != sibling, "The source cannot be reordered relative to itself.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic parent = this.Parent;
      int sortOrder = -1;

      /*------------------------------------------------------------------------------------------------------------------------
      | If there is no sibling, inject the source at the beginning of the collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sibling == null) {
        this.SortOrder = sortOrder++;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through each topic to assign a new priority order
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (Topic topic in parent.SortedChildren) {
        // Assuming the topic isn't the source, increment the sortOrder
        if (topic != this) {
          topic.SortOrder = sortOrder++;
        }
        // If the topic is the sibling, then assign the next sortOrder to the source
        if (topic == sibling) {
          this.SortOrder = sortOrder++;
        }
      }

    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    /// <param name="item">The <see cref="Topic"/> object from which to extract the key.</param>
    /// <returns>The key for the specified collection item.</returns>
    protected override string GetKeyForItem(Topic item) {
      Contract.Assume(item != null, "Assumes the item is available when deriving its key.");
      return item.Key;
    }

    /*==========================================================================================================================
    | METHOD: DISPOSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Technically, there's nothing to be done when disposing a Topic. However, this allows the topic attributes (and
    ///   properties) to be set using a using statement, which is syntactically convenient.
    /// </summary>
    public virtual void Dispose() {
      _incomingRelationships.Dispose();
      _relationships.Dispose();
      GC.SuppressFinalize(this);
    }

  } // Class

} // Namespace
