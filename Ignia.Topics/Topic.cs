/*==============================================================================================================================
| Author        Casey Margell, Ignia LLC
| Client        Ignia
| Project       Topics Library
|
| Purpose       The Topic object is a simple container for a particular node in the topic hierarchy. It contains the
|               metadata associated with the particular node, a list of children, etc....
|
>===============================================================================================================================
| Revisions     Date            Author                  Comments
| - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
|               03.24.09        Casey Margell           Initial version template
|               05.17.09        Jeremy Caney            Added lazy instantiation and improved caching. Added detail to
|                                                       documentation.
|               07.26.10        Hedley Robertson        Added handling for relationships
|               08.03.13        Jeremy Caney            Added support for namespaces and an implicit root to GetTopic().
|               08.03.13        Jeremy Caney            Added support for FindAllByAttribute().
|               09.23.13        Jeremy Caney            Added support for IncomingRelationships (as a reverse lookup).
|               09.27.13        Jeremy Caney            Renamed Name to Key, FullName to UniqueKey; refactored all dependency
|                                                       files.
|               09.30.13        Jeremy Caney            Changed base class to KeyedCollection; refactored all dependency files.
|               10.04.13        Jeremy Caney            Added new ContentType and Title properties.
|               10.12.13        Jeremy Caney            Updated FindAllByAttribute() to return Collection, and to support
|                                                       isRecursive.
|               10.28.13        Jeremy Caney            Corrected bug related to SetRelationship() when editing an existing topic.
|               10.28.13        Jeremy Caney            Added ChangeKey() to Key setter to ensure referential integrity with
|                                                       lookups.
|               08.06.13        Katherine Trunkey       Changed Topic.Attributes to an AttributeValueCollection
|                                                       (KeyedCollection<string, AttributeValue>).
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


  /*=============================================================================================================================
  | CLASS
  \----------------------------------------------------------------------------------------------------------------------------*/
  public class Topic : KeyedCollection<string, Topic>, IDisposable {


  /*===========================================================================================================================
  | DECLARE PRIVATE VARIABLES
  >============================================================================================================================
  | Declare variables for property use
  \--------------------------------------------------------------------------------------------------------------------------*/
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

  /*===========================================================================================================================
  | CONSTRUCTOR
  >============================================================================================================================
  | Constructors for the topic object.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public Topic() : base(StringComparer.OrdinalIgnoreCase) { }

    public Topic(string key) : base(StringComparer.OrdinalIgnoreCase) {
      this.Key                  = key;
    }

    public Topic(string key, string contentType) : base(StringComparer.OrdinalIgnoreCase) {
      this.Key                  = key;
      this.Attributes.Add(new AttributeValue("ContentType", contentType));
    }

  /*===========================================================================================================================
  | ###TODO JJC080314: An overload of the constructor should be created to accept an XmlDocument or XmlNode based on the
  | proposed Import/Export schema.
  >----------------------------------------------------------------------------------------------------------------------------
    public Topic(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) : base(StringComparer.OrdinalIgnoreCase) {
    //Process XML
    //Construct children objects
    //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects are
    //already created and available.
      }
  \--------------------------------------------------------------------------------------------------------------------------*/

  /*===========================================================================================================================
  | PROPERTY: ATTRIBUTES
  >============================================================================================================================
  | Attributes is a generic property bag for keeping track of either named or arbitrary attributes, thus providing
  | significant extensibility.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE JJC080313: The Attributes type should be changed to use either KeyedCollection (ideally, with the
  | INotifyCollectionChanged interface) or ObservableCollection (with a string indexer and duplicate key check).  To begin, it
  | is recommended that this be converted to use the more standard KeyedCollection, which can be upgraded to use
  | INotifyCollectionChanged at a later date.  When it is made observable, this can (and should) be used specifically to
  | intercept changes to either ParentID or Key, since they have specific implications in terms of the data integrity of the
  | collection.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE KLT081314: Attributes is now of type AttributeValueCollection (KeyedCollection<string, AttributeValue>). Extending
  | the collection to incorporate the INotifyCollectionChanged interface or converting it to an ObservableCollection remains an
  | item for future development.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE JJC080313: To implement a KeyedCollection, a new class will need to be created for storing Attribute values.  This
  | may be a point of confusion with TopicAttribute, which describes an ATTRIBUTE, as opposed to a particular INSTANCE of an
  | attribute.  We could explore a means of "Object Inheritance" (as opposed to class inheritance), but it's questionable
  | whether the default metadata of a TopicAttribute is really necessary when working with Attributes at the topics level.
  | As a result, this may require further evaluation.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE KLT081314: The AttributeValue class and AttributeValueCollection (KeyedCollection<string, AttributeValue>) have
  | been created. The question of "Object Inheritance" remains.
  \--------------------------------------------------------------------------------------------------------------------------*/
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

  /*===========================================================================================================================
  | PROPERTY: RELATIONSHIPS
  >============================================================================================================================
  | A dictionary of namespaced relationships to other topics.  Can be used for tags, related topics, &c.
  \--------------------------------------------------------------------------------------------------------------------------*/
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
  >============================================================================================================================
  | A dictionary of namespaced relationships FROM other topics.  Can be used for tags, related topics, &c.  A reverse index of
  | the Relationships property.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public Topic IncomingRelationships {
      get {
        if (_incomingRelationships == null) {
          _incomingRelationships = new Topic();
        }
        return _incomingRelationships;
      }
    }

  /*===========================================================================================================================
  | PROPERTY: PARENT
  >============================================================================================================================
  | Reference to the parent topic of this node, allowing code to traverse topics as a linked list.
  \--------------------------------------------------------------------------------------------------------------------------*/
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

  /*===========================================================================================================================
  | PROPERTY: DERIVED TOPIC
  >============================================================================================================================
  | Reference to the topic that this topic is derived from, if available.  Derived topics allow attribute values to be
  | inherited from another topic.
  \--------------------------------------------------------------------------------------------------------------------------*/
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

  /*===========================================================================================================================
  | PROPERTY: SORTED CHILDREN
  >============================================================================================================================
  | Provides a reference to the values collection, sorted by the SortOrder property.  Since Dictionaries do not guarantee
  | sort order, this is necessary for anything that expects to honor the order of topics in the database.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public IEnumerable<Topic> SortedChildren {
      get {
        return this.Items.OrderBy(topic => topic.SortOrder);
      }
    }

  /*===========================================================================================================================
  | PROPERTY: NESTED TOPICS
  >============================================================================================================================
  | Provides a reference to the values collection, filtered by Topics of the ContentType TopicsList, which represent Nested
  | Topics.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###TODO JJC080314: Ideally, this property should return a KeyedCollection of the underlying Topics filtered by ContentType,
  | but with the key removing the preceding underscore. This would need to be a specialized version of the KeyedCollection
  | class, possibly a dirivitive of the NestedTopics class. Preferrably, this will be dynamically created based on a reference
  | back to the parent class (this), in order to ensure synchronization between NestedTopics and the parent collection.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public Topic NestedTopics {
      get {
        throw new NotImplementedException();
      }
    }

  /*===========================================================================================================================
  | PROPERTY: CHILD TOPICS
  >============================================================================================================================
  | Provides a reference to the values collection, filtered by Topics that are NOT of the ContentType TopicList, which
  | represent Child Topics. This provides a complement to the NestedTopics collection.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public Topic ChildTopics {
      get {
        throw new NotImplementedException();
      }
    }

  /*===========================================================================================================================
  | PROPERTY: ID
  >============================================================================================================================
  | Getter/Setter for the topic's id according to the data provider
  \--------------------------------------------------------------------------------------------------------------------------*/
    public int Id {
      get {
        return _id;
      }
      set {
        _id = value;
      }
    }

  /*===========================================================================================================================
  | PROPERTY: ORIGINAL NAME
  >============================================================================================================================
  | Getter/Setter for the original name
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string OriginalName {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }

  /*===========================================================================================================================
  | PROPERTY: ORIGINAL KEY
  >============================================================================================================================
  | Getter/Setter for the original key
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string OriginalKey {
      get {
        return _originalKey;
      }
      set {
        _originalKey = value;
      }
    }

  /*===========================================================================================================================
  | PROPERTY: NAME
  >============================================================================================================================
  | Getter/Setter for the name attribute
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string Name {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }

  /*===========================================================================================================================
  | PROPERTY: KEY
  >============================================================================================================================
  | Getter/Setter for the key attribute
  \--------------------------------------------------------------------------------------------------------------------------*/
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

  /*===========================================================================================================================
  | METHOD: CHANGE KEY
  >----------------------------------------------------------------------------------------------------------------------------
  | Changes the key associated with a topic to maintain referential integrity.  By default, KeyedCollection doesn't permit
  | mutable keys; this mitigates that issue by allowing the collection's lookup dictionary to be updated whenever the key is
  | updated in the corresponding topic object.
  \--------------------------------------------------------------------------------------------------------------------------*/
    internal void ChangeKey(Topic topic, string newKey) {
      base.ChangeItemKey(topic, newKey);
    }

  /*===========================================================================================================================
  | PROPERTY: FULL NAME
  >============================================================================================================================
  | Gets the full name of the Topic including parents ("bar" vs. "foo:bar")
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string FullName {
      get {
        throw new NotImplementedException();
      }
    }

  /*===========================================================================================================================
  | PROPERTY: UNIQUE KEY
  >============================================================================================================================
  | Gets the fully qualified key of the Topic including parents ("bar" vs. "foo:bar")
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string UniqueKey {
      get {

      /*-----------------------------------------------------------------------------------------------------------------------
      | Crawl up tree to define uniqueKey
      \----------------------------------------------------------------------------------------------------------------------*/
        string          uniqueKey       = "";
        Topic           topic           = this;

        for (int i=0; i < 100; i++) {
          if (uniqueKey.Length > 0) uniqueKey = ":" + uniqueKey;
          uniqueKey     = topic.Key + uniqueKey;
          topic         = topic.Parent;
          if (topic == null) break;
        }

      /*-----------------------------------------------------------------------------------------------------------------------
      | Return value
      \----------------------------------------------------------------------------------------------------------------------*/
        return uniqueKey;

      }
    }

  /*===========================================================================================================================
  | PROPERTY: WEB PATH
  >============================================================================================================================
  | Gets the root-relative web path of the Topic, based on an assumption that the TopicRoot is bound to the root of the site.
  | (If this assumption is not true, the application will need to specially account for that).
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string WebPath {
      get {
        return UniqueKey.Replace("Root:", "/").Replace(":", "/") + "/";
      }
    }

  /*===========================================================================================================================
  | PROPERTY: CONTENT TYPE
  >============================================================================================================================
  | Getter for the ContentType attribute.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public ContentType ContentType {
      get {

      //Create singleton reference to content type object in repository
        if (_contentType == null) {
          if (Attributes.Contains("ContentType") && !String.IsNullOrEmpty(Attributes["ContentType"].Value) && TopicRepository.ContentTypes.Contains(Attributes["ContentType"].Value)) {
            _contentType = TopicRepository.ContentTypes[Attributes["ContentType"].Value];
          }
        }

      //If content type doesn't exist, default to Container.
        if (_contentType == null) {
          _contentType = TopicRepository.ContentTypes["Container"];
        }

        return _contentType;

      }
      set {
        _contentType = value;
      }
    }

  /*===========================================================================================================================
  | PROPERTY: LAST MODIFIED
  >============================================================================================================================
  | Getter/Setter for the LastModified attribute (converted to DateTime from its string value).
  \--------------------------------------------------------------------------------------------------------------------------*/
    public DateTime LastModified {
      get {
      //Return converted string attribute value, if available
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
        else {
        //Return minimum date value, if LastModified is not already populated
          return DateTime.MinValue;
        }
      }
      set {
        if (value != null) {
          Attributes.SetAttributeValue("LastModified", value.ToString());
        }
      }
    }

  /*===========================================================================================================================
  | PROPERTY: VERSIONS
  >============================================================================================================================
  | Getter for collection of previous versions that can be rolled back to.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###TODO JJC080314: The Versions property simply exposes a list of Versions that can be rolled back to. Each Version object
  | will contain the version date, and may contain the author's name and version notes.
  >----------------------------------------------------------------------------------------------------------------------------
    public VersionCollection Versions {
      get {
      //Since versions won't normally be required, except during rollback scenarios, may alternatively configure these to load
      //on demand.
        return _versions;
      }
    }
  \--------------------------------------------------------------------------------------------------------------------------*/

  /*===========================================================================================================================
  | PROPERTY: VERSION HISTORY
  >============================================================================================================================
  | Provides a collection of dates representing past versions of the topic, which can be rolled back to.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public List<DateTime> VersionHistory {
      get {
        if (_versionHistory == null) {
          _versionHistory = new List<DateTime>();
        }
        return _versionHistory;
      }
    }

  /*===========================================================================================================================
  | PROPERTY: VIEW
  >============================================================================================================================
  | Getter/Setter for the View attribute. This value can be set either at the topic level, or at the Content Type level.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string View {
      get {
      //Return current Topic's View Attribute or the default for the ContentType.
        return GetAttribute("View", ContentType.GetAttribute("View", ContentType.Key));
      }
      set {
        Attributes.SetAttributeValue("View", value);
      }
    }

  /*===========================================================================================================================
  | PROPERTY: TITLE
  >============================================================================================================================
  | Getter/Setter for the Title attribute.  Falls back to the key if not set.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string Title {
      get {
        return GetAttribute("Title", Key);
      }
      set {
        Attributes.SetAttributeValue("Title", value);
      }
    }

  /*===========================================================================================================================
  | PROPERTY: DESCRIPTION
  >============================================================================================================================
  | Getter/Setter for the description attribute.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string Description {
      get {
        return GetAttribute("Description");
      }
      set {
        Attributes.SetAttributeValue("Description", value);
      }
    }

  /*===========================================================================================================================
  | PROPERTY: SORT ORDER
  >============================================================================================================================
  | Getter/Setter for the topic's sort order.  Sort order should be assigned by the TopicDataProvider; it may be based on an
  | attribute or based on the physical order of records from the data source, depending on the capabilities of the storage
  | provider.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public int SortOrder {
      get {
        return _sortOrder;
      }
      set {
        _sortOrder = value;
      }
    }

  /*===========================================================================================================================
  | METHOD: SET RELATIONSHIP
  >----------------------------------------------------------------------------------------------------------------------------
  | Set the collection of related items (by scope) via a csv
  \--------------------------------------------------------------------------------------------------------------------------*/
    public void SetRelationship(string scope, string relatedCsv) {

    /*-------------------------------------------------------------------------------------------------------------------------
    | HANDLE DELETION
    \------------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(relatedCsv)) {
        if (this.Relationships.Contains(scope)) {
          this.Relationships.Remove(scope);
        }
        return;
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | BUILD COLLECTION FROM CSV
    \------------------------------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------------------------------
    | SET INCOMING RELATIONSHIPS
    \------------------------------------------------------------------------------------------------------------------------*/
      foreach (Topic relatedTopic in related) {
        relatedTopic.SetRelationship(scope, this, true);
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | SET PROPERTY VALUE
    \------------------------------------------------------------------------------------------------------------------------*/
      if (this.Relationships.Contains(related.Key)) {
        this.Relationships.Remove(related.Key);
      }
      this.Relationships.Add(related);

    }

  /*===========================================================================================================================
  | METHOD: SET RELATIONSHIP
  >----------------------------------------------------------------------------------------------------------------------------
  | Set a new relationship based on the relationship type, target topic and direction.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public void SetRelationship(string scope, Topic related, bool isIncoming = false) {

      Topic relationships = this.Relationships;

      if (isIncoming) {
        relationships = this.IncomingRelationships;
      }

    //Create new namespace, if not present
      if (!relationships.Contains(scope)) {
        relationships.Add(new Topic(scope));
      }

    //Add the relationship to the correct scoped key
      if (!relationships[scope].Contains(related.Key)) {
        relationships[scope].Add(related);
      }

    //Set incoming relationship
      if (!isIncoming) {
        related.SetRelationship(scope, this, true);
      }

    }

  /*===========================================================================================================================
  | METHOD: GET ATTRIBUTE
  >----------------------------------------------------------------------------------------------------------------------------
  | Gets a named attribute from the Attributes dictionary.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public string GetAttribute(string name, bool isRecursive = false) {
      return GetAttribute(name, "", isRecursive);
    }

    public string GetAttribute(string name, string defaultValue, bool isRecursive = false, int maxHops = 5) {

      string value = null;

    /*-------------------------------------------------------------------------------------------------------------------------
    | LOOKUP VALUE FROM ATTRIBUTES
    \------------------------------------------------------------------------------------------------------------------------*/
      if (Attributes.Contains(name)) {
        value = Attributes[name].Value;
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | LOOKUP VALUE FROM TOPIC POINTER
    \------------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && this.DerivedTopic != null && maxHops > 0) {
        value = this.DerivedTopic.GetAttribute(name, null, false, maxHops-1);
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | LOOKUP VALUE FROM PARENT
    \------------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(value) && isRecursive && Parent != null) {
        value = Parent.GetAttribute(name, defaultValue, isRecursive);
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | RETURN VALUE, IF FOUND
    \------------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(value)) {
        return value;
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | FINALLY, RETURN DEFAULT
    \------------------------------------------------------------------------------------------------------------------------*/
      return defaultValue;

    }

  /*===========================================================================================================================
  | METHOD: FIND ALL BY ATTRIBUTE
  >----------------------------------------------------------------------------------------------------------------------------
  | Retrieves a collection of topics based on an attribute name and value, optionally recursively.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###TODO JJC080313: Consider adding an overload of the out-of-the-box FindAll() method that supports recursion, thus
  | allowing a search by any criteria - including attributes.
  \--------------------------------------------------------------------------------------------------------------------------*/
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


  /*===========================================================================================================================
  | METHOD: LOAD
  >----------------------------------------------------------------------------------------------------------------------------
  | Pull data out of the storage provider to a given depth.
  \--------------------------------------------------------------------------------------------------------------------------*/

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

  /*===========================================================================================================================
  | ###TODO JJC080314: An overload to Load() should be created to accept an XmlDocument or XmlNode based on the proposed
  | Import/Export schema.
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE JJC080313: If the topic already exists, return the existing node, by calling its Merge() function. Otherwise,
  | construct a new node using its XmlNode constructor.
  >----------------------------------------------------------------------------------------------------------------------------
    public static Topic Load(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
    //Process XML
    //Construct children objects
    //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects are
    //already created and available.
    }
  \--------------------------------------------------------------------------------------------------------------------------*/

  /*===========================================================================================================================
  | METHOD: MERGE
  >----------------------------------------------------------------------------------------------------------------------------
  | ###TODO JJC080314: Similar to Load(), but should merge values with existing Topic rather than creating a new TOpic. Should
  | accept an XmlDocument or XmlNode based on the proposed Import/Export schema.
  >----------------------------------------------------------------------------------------------------------------------------
    public Topic Merge(XmlNode node, ImportStrategy importStrategy = ImportStrategy.Merge) {
    //Process XML
    //Construct children objects
    //###NOTE JJC080314: May need to cross-reference with Load() and/or TopicRepository to validate against whatever objects are
    //already created and available.
    }
  \--------------------------------------------------------------------------------------------------------------------------*/

  /*===========================================================================================================================
  | METHOD: ROLLBACK
  >----------------------------------------------------------------------------------------------------------------------------
  | Rolls back the current topic to a particular point in its version history by reloading legacy attributes and then saving
  | the new version.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public void Rollback(DateTime version) {

    //Retrieve topic from database
      Topic                     originalVersion         = Topic.Load(this.Id, version);

    //Rename topic, if necessary
      this.Key                                          = originalVersion.Key;

    //Mark each attribute as dirty
      foreach (AttributeValue attribute in originalVersion.Attributes) {
        if (!this.Attributes.Contains(attribute.Key) || this.Attributes[attribute.Key].Value != attribute.Value) {
          attribute.IsDirty = true;
        }
      }

    //Construct new AttributeCollection
      this.Attributes                                   = originalVersion.Attributes;

    //Save as new version
      this.Save();

    }

  /*===========================================================================================================================
  | METHOD: GET TOPIC
  >----------------------------------------------------------------------------------------------------------------------------
  | Pull data out of the storage provider to a given depth.  Returns null if the topic cannot be found
  >----------------------------------------------------------------------------------------------------------------------------
  | ###NOTE JJC080313: Provided an overload that allows a developer to explicitly state the namespace as a separate parameter.
  \--------------------------------------------------------------------------------------------------------------------------*/
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

    /*-------------------------------------------------------------------------------------------------------------------------
    | VALIDATE INPUT
    \------------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(topic)) return null;

    /*-------------------------------------------------------------------------------------------------------------------------
    | PROVIDE IMPLICIT ROOT
    >--------------------------------------------------------------------------------------------------------------------------
    | ###NOTE JJC080313: While a root topic is required by the data structure, it should be implicit from the perspective of
    | the calling application.  A developer should be able to call GetTopic("Namepace:TopicPath") to get to a topic, without
    | needing to be aware of the root.
    \------------------------------------------------------------------------------------------------------------------------*/
      if (
        !topic.StartsWith("Root:", StringComparison.OrdinalIgnoreCase) &&
        !topic.StartsWith("Categories:", StringComparison.OrdinalIgnoreCase) &&
        !topic.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
        !topic.Equals("Categories", StringComparison.OrdinalIgnoreCase)
        ) {
        topic = "Root:" + topic;
      }

    /*-------------------------------------------------------------------------------------------------------------------------
    | VALIDATE PARAMETERS
    \------------------------------------------------------------------------------------------------------------------------*/
      if (!topic.StartsWith(UniqueKey, StringComparison.OrdinalIgnoreCase)) return null;
      if (topic.Equals(UniqueKey, StringComparison.OrdinalIgnoreCase)) return this;

    /*-------------------------------------------------------------------------------------------------------------------------
    | DEFINE VARIABLES
    \------------------------------------------------------------------------------------------------------------------------*/
      string   remainder       = topic.Substring(UniqueKey.Length + 1);
      int      marker          = remainder.IndexOf(":", StringComparison.Ordinal);
      string   nextChild       = (marker < 0)? remainder : remainder.Substring(0, marker);

    /*-------------------------------------------------------------------------------------------------------------------------
    | FIND TOPIC
    \------------------------------------------------------------------------------------------------------------------------*/
      if (!this.Contains(nextChild)) return null;

      if (nextChild == remainder) return this[nextChild];

      return this[nextChild].GetTopic(topic);

    }

  /*===========================================================================================================================
  | METHOD: SAVE
  >----------------------------------------------------------------------------------------------------------------------------
  | Saves the topic information, optionally saving all children
  \--------------------------------------------------------------------------------------------------------------------------*/
    public int Save() {
      return Save(false, false);
    }

  //Overload for save as draft
    /*
    public int Save(bool isDraft) {
      return Save(false, isDraft);
      }
    */

    public int Save(bool isRecursive, bool isDraft) {
      Id = TopicRepository.Save(this, isRecursive, isDraft);
      return Id;
    }

  /*===========================================================================================================================
  | METHOD: REFRESH
  >----------------------------------------------------------------------------------------------------------------------------
  | Refreshes the topic data from the provider. Optionally refreshes all children as well.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public bool Refresh() {
      return Refresh(true);
    }

    public bool Refresh(bool isRecursive) {
      throw new NotSupportedException("The Refresh() method is a placeholder for future functionality and is not yet supported.");
    }

  /*===========================================================================================================================
  | METHOD: DELETE
  >----------------------------------------------------------------------------------------------------------------------------
  | Deletes the current topic (as well as all children). We may want to rethink this at some point and have functionality to
  | delete a node and elavate it's children or delete and reassign children or something.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public void Delete() {
      Delete(true);
    }

    public void Delete(bool isRecursive) {
      TopicRepository.Delete(this, isRecursive);
    }

  /*=========================================================================================================================
  | OVERRIDE: GET KEY FOR ITEM
  >==========================================================================================================================
  | Method must be overridden for the EntityCollection to extract the keys from the items.
  \------------------------------------------------------------------------------------------------------------------------*/
    protected override string GetKeyForItem(Topic item) {
      return item.Key;
    }

  /*===========================================================================================================================
  | METHOD: DISPOSE
  >----------------------------------------------------------------------------------------------------------------------------
  | Technically, there's nothing to be done when disposing a Topic.  However, this allows the topic attributes (and properties)
  | to be set using a using statement, which is syntactically convenient.
  \--------------------------------------------------------------------------------------------------------------------------*/
    public void Dispose() { }

  } //Class

} //Namespace
