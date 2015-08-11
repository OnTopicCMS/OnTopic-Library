/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               08.06.14        Katherine Trunkey       Updated all instances of Attributes[key] to
|                                                       Attributes[key].Value.
|               08.06.14        Katherine Trunkey       Updated instances of Attributes.Keys.Contains([key]) to
|                                                       Attributes.Contains([key]) for compatibility with KeyedCollection.
|               08.11.14        Katherine Trunkey       Updated ContentType property to correspond to strongly-typed object;
|                                                       removed setter as it is no longer needed. Finalized View property.
|               09.11.14        Katherine Trunkey       Updated FindAllByAttribute() to be IndexOf-inclusive.
|               09.27.14        Jeremy Caney            Added support for version history.
\-----------------------------------------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Globalization;

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
    /// <remarks>
    ///   Optional overloads allow object to be constructed based on the Topic's <see cref="Key"/> and/or
    ///   <see cref="ContentType"/> properties. 
    /// </remarks>
    /// <param name="key">
    ///   The string identifier for the <see cref="Topic"/>.
    /// </param>
    /// <param name="contentType">
    ///   The string value text for the Topic's ContentType Attribute.
    /// </param>
    public Topic() : base(StringComparer.OrdinalIgnoreCase) { }

    public Topic(string key) : base(StringComparer.OrdinalIgnoreCase) {
      this.Key                  = key;
    }

    public Topic(string key, string contentType) : base(StringComparer.OrdinalIgnoreCase) {
      this.Key                  = key;
      this.Attributes.Add(new AttributeValue("ContentType", contentType));
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
    \----=--------------------------------------------------------------------------------------------------------------------*/

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
    | (KeyedCollection<string, <see cref=">AttributeValue"/>>). Extending the collection to incorporate the
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
        if (_attributes == null) {
          _attributes = new AttributeValueCollection();
        }
        return _attributes;
      }
      set {
        _attributes = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   A dictionary of namespaced relationships to other topics; can be used for tags, related topics, &c.
    /// </summary>
    /// <remarks>
    ///   The relationships property exposes a <see cref="Topic"/> with child topics representing named relationships (e.g., 
    ///   "Related" for related topics); those child topics in turn have child topics representing references to each related 
    ///   topic, thus allowing the topic hierarchy to be represented as a network graph.
    /// </remarks>
    public Topic Relationships {
      get {
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
    ///   A dictionary of namespaced relationships from other topics; can be used for tags, related topics, &c. 
    /// </summary>
    /// <remarks>
    ///   The incoming relationships property provides a reverse index of the Relationships property, in order to indicate which
    ///   topics point to the current topic. This can be useful for traversing the topic tree as a network graph. This is of 
    ///   particular use for tags, where the current topic represents a tag, and the incoming relationships represents all topics
    ///   associated with that tag.
    /// </remarks>
    public Topic IncomingRelationships {
      get {
        if (_incomingRelationships == null) {
          _incomingRelationships = new Topic();
        }
        return _incomingRelationships;
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
    public Topic Parent {
      get {
        return _parent;
      }
      set {
        if (value == null) {
          throw new InvalidOperationException("The value for Parent must not be null.");
        }
        if (_parent == value) {
          return;
        }
        if (!value.Contains(this.Key)) {
          value.Add(this);
        }
        else {
          throw new Exception("Duplicate key when setting Parent property: the topic with the name '" + this.Key + "' already exists in the '" + value.Key + "' topic.");
        }
        if (_parent != null) {
          TopicRepository.Move(this, value);
          _parent.Remove(this.Key);
        }
        _parent = value;
        Attributes.Remove("ParentID");
        Attributes.Add(new AttributeValue("ParentID", value.Id.ToString(CultureInfo.InvariantCulture)));
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
    ///     TopicId attribute key, values from that topic are used when the <see cref="GetAttribute(string, bool)"/> method is 
    ///     unable to find a local value for the attribute. 
    ///   </para>  
    ///   <para>
    ///     Be aware that while multiple levels of derived topics can be configured, the <see 
    ///     cref="GetAttribute(string, bool)"/> method defaults to a maximum level of five "hops". This can be optionally 
    ///     overwritten by client code by calling the <see cref="GetAttribute(string, string, bool, int)"/> overload and 
    ///     explicitly defining the number of hops.
    ///   </para>
    /// </remarks>
    public Topic DerivedTopic {
      get {
        if (_derivedTopic == null && this.Attributes.Contains("TopicID")) {
          int topicId = 0;
          bool success = Int32.TryParse(this.Attributes["TopicID"].Value.ToString(), out topicId);
          if (!success || topicId == 0) return null;
          _derivedTopic = TopicRepository.RootTopic.GetTopic(topicId);
        }
        return _derivedTopic;
      }
      set {
        _derivedTopic = value;
        if (value != null) {
          this.Attributes.SetAttributeValue("TopicID", value.Id.ToString());
        }
        else if (this.Attributes.Contains("TopicID")) {
          this.Attributes.Remove("TopicID");
        }
      }
    }

    /*==========================================================================================================================
    | PROPERTY: SORTED CHILDREN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the values collection, sorted by the SortOrder property.  
    /// </summary>
    /// <remarks>
    ///   Since Dictionaries do not guarantee sort order, this is necessary for any code that expects to honor the order of
    ///   topics in the database. Simply calling <see cref="Collection{T}.Items"/> may return topics in the correct order, but
    ///   this cannot be assumed.
    /// </remarks>
    public IEnumerable<Topic> SortedChildren {
      get {
        return this.Items.OrderBy(topic => topic.SortOrder);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: NESTED TOPICS
    >---------------------------------------------------------------------------------------------------------------------------
    | ###TODO JJC080314: Ideally, this property should return a KeyedCollection of the underlying Topics filtered by
    | ContentType, but with the key removing the preceding underscore.This would need to be a specialized version of the
    | KeyedCollection class, possibly a dirivitive of the NestedTopics class. Preferrably, this will be dynamically created
    | based on a reference back to the parent class (this), in order to ensure synchronization between NestedTopics and the
    | parent collection.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the values collection, filtered by Topics of the ContentType TopicsList, which represent
    ///   Nested Topics.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public Topic NestedTopics {
      get {
        throw new NotImplementedException();
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CHILD TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a reference to the values collection, filtered by Topics that are NOT of the ContentType TopicList, which
    ///   represent Child Topics.This provides a complement to the NestedTopics collection.
    /// </summary>
    public Topic ChildTopics {
      get {
        throw new NotImplementedException();
      }
    }

    /*==========================================================================================================================
    | PROPERTY: ID
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's id according to the data provider.
    /// </summary>
    public int Id {
      get {
        return _id;
      }
      set {
        _id = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: ORIGINAL NAME
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the original name.
    /// </summary>
    /// <remarks>
    ///   Deprecated/no longer used.
    /// </remarks>
    public string OriginalName {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
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
    public string OriginalKey {
      get {
        return _originalKey;
      }
      set {
        _originalKey = value;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's Key attribute, the primary text identifier for the topic.
    /// </summary>
    public string Key {
      get {
        return _key;
      }
      set {
        if (_originalKey == null) {
          _originalKey = GetAttribute("Key", null);
        }
        if (_originalKey != null && !value.Equals(Key) && Parent != null) {
          Parent.ChangeKey(this, value);
        }
        Attributes.SetAttributeValue("Key", value);
        _key = value;
      }
    }

    /*==========================================================================================================================
    | METHOD: CHANGE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Changes the key associated with a topic to maintain referential integrity.
    /// </summary>
    /// <remarks>
    ///   By default, KeyedCollection doesn't permit mutable keys; this mitigates that issue by allowing the collection's
    ///   lookup dictionary to be updated whenever the key is updated in the corresponding topic object.
    /// </remarks>
      internal void ChangeKey(Topic topic, string newKey) {
      base.ChangeItemKey(topic, newKey);
    }

    /*==========================================================================================================================
    | PROPERTY: UNIQUE KEY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the full name of the Topic including parents ("bar" vs. "foo:bar").
    /// </summary>
    public string UniqueKey {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Crawl up tree to define uniqueKey
        \---------------------------------------------------------------------------------------------------------------------*/
        string          uniqueKey       = "";
        Topic           topic           = this;

        for (int i=0; i < 100; i++) {
          if (uniqueKey.Length > 0) uniqueKey = ":" + uniqueKey;
          uniqueKey     = topic.Key + uniqueKey;
          topic         = topic.Parent;
          if (topic == null) break;
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return value
        \---------------------------------------------------------------------------------------------------------------------*/
        return uniqueKey;

      }
    }

    /*==========================================================================================================================
    | PROPERTY: WEB PATH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets the root-relative web path of the Topic, based on an assumption that the TopicRoot is bound to the root of the
    ///   site.
    /// </summary>
    /// <remarks>
    ///   (If this assumption is not true, the application needs to specifically account for that).
    /// </remarks>
    public string WebPath {
      get {
        return UniqueKey.Replace("Root:", "/").Replace(":", "/") + "/";
      }
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Getter for the ContentType attribute.
    /// </summary>
    public ContentType ContentType {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Create singleton reference to content type object in repository
        \---------------------------------------------------------------------------------------------------------------------*/
        if (_contentType == null) {
          if (Attributes.Contains("ContentType") && !String.IsNullOrEmpty(Attributes["ContentType"].Value) && TopicRepository.ContentTypes.Contains(Attributes["ContentType"].Value)) {
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
    | PROPERTY: LAST MODIFIED
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's LastModified attribute.
    /// </summary>
    /// <remarks>
    ///   The value is stored in the database as a string (Attribute) value, but converted to DateTime for use in the system.
    /// </remarks>
    public DateTime LastModified {
      get {

        /*----------------------------------------------------------------------------------------------------------------------
        | Return converted string attribute value, if available
        \---------------------------------------------------------------------------------------------------------------------*/
        if (!String.IsNullOrEmpty(GetAttribute("LastModified", ""))) {
          string lastModified = GetAttribute("LastModified");
          DateTime dateTimeValue;

          //Return converted DateTime
          if (DateTime.TryParse(lastModified, out dateTimeValue)) {
            return dateTimeValue;
          }

          //Return minimum date value if datetime cannot be parsed from attribute
          else {
            return DateTime.MinValue;
          }

        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Return minimum date value, if LastModified is not already populated
        \---------------------------------------------------------------------------------------------------------------------*/
        else {
          return DateTime.MinValue;
        }

      }
      set {
        if (value != null) {
          Attributes.SetAttributeValue("LastModified", value.ToString());
        }
      }
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
    public List<DateTime> VersionHistory {
      get {
        if (_versionHistory == null) {
          _versionHistory = new List<DateTime>();
        }
        return _versionHistory;
      }
    }

    /*==========================================================================================================================
    | PROPERTY: VIEW
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the View attribute.
    /// </summary>
    /// <remarks>
    ///   This value can be set either at the topic level, or at the Content Type level.
    /// </remarks>
    public string View {
      get {
      //Return current Topic's View Attribute or the default for the ContentType.
        return GetAttribute("View", ContentType.GetAttribute("View", ContentType.Key));
      }
      set {
        Attributes.SetAttributeValue("View", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: TITLE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Title attribute.
    /// </summary>
    /// <remarks>
    ///   Falls back to the topic's <see cref="Key"/> if not set.
    /// </remarks>
    public string Title {
      get {
        return GetAttribute("Title", Key);
      }
      set {
        Attributes.SetAttributeValue("Title", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: DESCRIPTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the Description attribute.
    /// </summary>
    public string Description {
      get {
        return GetAttribute("Description");
      }
      set {
        Attributes.SetAttributeValue("Description", value);
      }
    }

    /*==========================================================================================================================
    | PROPERTY: SORT ORDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets or sets the topic's sort order.
    /// </summary>
    /// <remarks>
    ///   Sort order should be assigned by the <see cref="TopicDataProvider"/>; it may be based on an attribute or based on the physical
    ///   order of records from the data source, depending on the capabilities of the storageprovider.
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
    | METHOD: SET RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Set the collection of related items (by scope) via a CSV.
    /// </summary>
    /// <remarks>
    ///   Not currently referenced by the library.
    /// </remarks>
    public void SetRelationship(string scope, string relatedCsv) {

      /*-----------------------------------------------------------------------------------------------------------------------
      | Handle Deletion
      \----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(relatedCsv)) {
        if (this.Relationships.Contains(scope)) {
          this.Relationships.Remove(scope);
        }
        return;
      }

      /*-----------------------------------------------------------------------------------------------------------------------
      | Build collection from CSV
      \----------------------------------------------------------------------------------------------------------------------*/
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
    ///   Set a new relationship based on the relationship type, target topic and direction.
    /// </summary>
    public void SetRelationship(string scope, Topic related, bool isIncoming = false) {

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
    | METHOD: GET ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a named attribute from the Attributes dictionary.
    /// </summary>
    /// <remarks>
    ///   Optional overload.
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="isRecursive"></param>
    /// <param name="defaultValue"></param>
    /// <param name="maxHops"></param>
    /// <returns>Returns the string value for the Attribute.</returns>
    public string GetAttribute(string name, bool isRecursive = false) {
      return GetAttribute(name, "", isRecursive);
    }

    public string GetAttribute(string name, string defaultValue, bool isRecursive = false, int maxHops = 5) {

      string value = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from Attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (Attributes.Contains(name)) {
        value = Attributes[name].Value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from topic pointer
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && this.DerivedTopic != null && maxHops > 0) {
        value = this.DerivedTopic.GetAttribute(name, null, false, maxHops-1);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up value from parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && isRecursive && Parent != null) {
        value = Parent.GetAttribute(name, defaultValue, isRecursive);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value, if found
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(value)) {
        return value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Finaly, return default
      \-----------------------------------------------------------------------------------------------------------------------*/
      return defaultValue;

    }

    /*==========================================================================================================================
    | METHOD: FIND ALL BY ATTRIBUTE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Retrieves a collection of topics based on an attribute name and value, optionally recursively.
    /// </summary>
    /// <remarks>
    ///   ###TODO JJC080313: Consider adding an overload of the out-of-the-box FindAll() method that supports recursion, thus
    ///   allowing a search by any criteria - including attributes.
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="isRecursive"></param>
    /// <returns>Returns a collection of topics matching the input parameters.</returns>
    public Collection<Topic> FindAllByAttribute(string name, string value, bool isRecursive = false) {
      Collection<Topic> results = new Collection<Topic>();
      if (this.GetAttribute(name).IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0) {
        results.Add(this);
      }
      if (isRecursive) {
        foreach (Topic topic in this) {
          Collection<Topic> nestedResults = topic.FindAllByAttribute(name, value, true);
          foreach (Topic matchedTopic in nestedResults) {
            results.Add(matchedTopic);
          }
        }
      }
      return results;
    }


    /*==========================================================================================================================
    | METHOD: LOAD
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Pull topics data out of the storage provider to a given depth.
    /// </summary>
    /// <remarks>
    ///   Optionsl overloads.
    /// </remarks>
    /// <param name="deepLoad"></param>
    /// <param name="topic"></param>
    /// <param name="depth"></param>
    /// <param name="version"></param>
    /// <param name="topicId"></param>
    /// <returns>Returns a topic object and N number of child topics, depending on depth specified.</returns>

    //Load by string name
    public static Topic Load() {
      return Load("", -1);
    }

    public static Topic Load(bool deepLoad) {
      return Load("", deepLoad?-1:0);
    }

    public static Topic Load(string topic) {
      return Load(topic, -1);
    }

    public static Topic Load(string topic, bool deepLoad) {
      return Load(topic, deepLoad?-1:0);
    }

    public static Topic Load(string topic, int depth) {
      return TopicRepository.Load(topic, depth);
    }

    public static Topic Load(string topic, DateTime version) {
      return TopicRepository.Load(topic, 0, version);
    }

  //Load by topicId
    public static Topic Load(int topicId) {
      return Load(topicId, -1);
    }

    public static Topic Load(int topicId, bool deepLoad) {
      return Load(topicId, deepLoad?-1:0);
    }

    public static Topic Load(int topicId, int depth) {
      return TopicRepository.Load(topicId, depth);
    }

    public static Topic Load(int topicId, DateTime version) {
      return TopicRepository.Load(topicId, 0, version);
    }

    /*==========================================================================================================================
    | ###TODO JJC080314: An overload to Load() should be created to accept an XmlDocument or XmlNode based on the proposed
    | Import/Export schema.
    >---------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: If the topic already exists, return the existing node, by calling its Merge() function. Otherwise,
    | construct a new node using its XmlNode constructor.
    >---------------------------------------------------------------------------------------------------------------------------
      public static Topic Load(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
      //Process XML
      //Construct children objects
      //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects
      //are already created and available.
      }
    \-------------------------------------------------------------------------------------------------------------------------*/

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
    public void Rollback(DateTime version) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve topic from database
      \-----------------------------------------------------------------------------------------------------------------------*/
      Topic originalVersion         = Topic.Load(this.Id, version);

      /*------------------------------------------------------------------------------------------------------------------------
      | Rename topic, if necessary
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Key                                          = originalVersion.Key;

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
      this.Attributes                                   = originalVersion.Attributes;

      /*------------------------------------------------------------------------------------------------------------------------
      | Save as new version
      \-----------------------------------------------------------------------------------------------------------------------*/
      this.Save();

    }

    /*==========================================================================================================================
    | METHOD: GET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Pull data out of the storage provider to a given depth.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Returns null if the topic cannot be found.
    ///   </para>
    ///   <para>
    ///     Optional overloads allow the developer to explicitly state the namespace as a separate parameter, or look up the
    ///     topic by ID or by part or all of a <see cref="UniqueKey"/> string.
    ///   </para>
    /// </remarks>
    /// <param name="topicId">The integer identifier for the topic.</param>
    /// <param name="namespaceKey">The string value for the (uniqueKey prefixing) namespace for the topic.</param>
    /// <param name="topic">The partial or full string value representing the uniqueKey for the topic.</param>
    public Topic GetTopic(int topicId) {

      if (this.Id == topicId) return this;

      foreach (Topic childTopic in this) {
        Topic topic = childTopic.GetTopic(topicId);
        if (topic != null) return topic;
      }

      return null;

    }

    public Topic GetTopic(string namespaceKey, string topic) {
      return GetTopic(String.IsNullOrEmpty(namespaceKey)? topic : namespaceKey + ":" + topic);
    }

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
      string   remainder       = topic.Substring(UniqueKey.Length + 1);
      int      marker          = remainder.IndexOf(":", StringComparison.Ordinal);
      string   nextChild       = (marker < 0)? remainder : remainder.Substring(0, marker);

      /*------------------------------------------------------------------------------------------------------------------------
      | Find topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!this.Contains(nextChild)) return null;

      if (nextChild == remainder) return this[nextChild];

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
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's children and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    /// <returns>Returns the topic's integer identifier.</returns>
    public int Save() {
      return Save(false, false);
    }

    public int Save(bool isRecursive = false, bool isDraft = false) {
      Id = TopicRepository.Save(this, isRecursive, isDraft);
      return Id;
    }

    /*==========================================================================================================================
    | METHOD: REFRESH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Refreshes the topic data from the provider.
    /// </summary>
    /// <remarks>
    ///   Optional overload allows for specifying whether the topic's children should be refreshed as well.
    /// </remarks>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's children and refresh them as well.
    /// </param>
    public bool Refresh() {
      return Refresh(true);
    }

    public bool Refresh(bool isRecursive) {
      throw new NotSupportedException("The Refresh() method is a placeholder for future functionality and is not yet supported.");
    }

    /*==========================================================================================================================
    | METHOD: DELETE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes the current topic (as well as all children).
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Optional overload allows for specifying whether the topic's children should be deleted as well. Default behavior is
    ///     to delete children.
    ///   </para>
    ///   <para>
    ///     We may want to rethink this at some point and have functionality to delete a node and elavate it's children or
    ///     delete and reassign children or something.
    ///   </para>
    /// </remarks>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's children and delete them as well.
    /// </param>
    public void Delete() {
      Delete(true);
    }

    public void Delete(bool isRecursive) {
      TopicRepository.Delete(this, isRecursive);
    }

    /*==========================================================================================================================
    | OVERRIDE: GET KEY FOR ITEM
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Method must be overridden for the EntityCollection to extract the keys from the items.
    /// </summary>
    protected override string GetKeyForItem(Topic item) {
      return item.Key;
    }

    /*==========================================================================================================================
    | METHOD: DISPOSE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Technically, there's nothing to be done when disposing a Topic. However, this allows the topic attributes (and
    ///   properties) to be set using a using statement, which is syntactically convenient.
    /// </summary>
    public void Dispose() { }

  } //Class

} //Namespace
