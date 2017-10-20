/*==============================================================================================================================
| Author        Ignia LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Diagnostics.Contracts;
using Ignia.Topics.Collections;

namespace Ignia.Topics.Web.Migrations {

  /*============================================================================================================================
  | CLASS: TOPICS SETUP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The Topics Setup class provides specific-purpose properties and methods associated with initial setup for the Topics
  ///   Library and CMS editor.
  /// </summary>
  public class TopicsSetup {

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    private     static          Topic                           _rootTopic                      = null;
    private     static          Topic                           _configuration                  = null;
    private     static          ContentTypeDescriptorCollection _contentTypes                   = null;

    /*==========================================================================================================================
    | PROPERTY: ROOT TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a local cache of the entire topic repository.
    /// </summary>
    /// <remarks>
    ///   Loading the cache locally prevents a conflict of references between the local and site-wide cache.
    /// </remarks>
    public static Topic RootTopic {
      get {
        Contract.Ensures(Contract.Result<Topic>() != null);
        if (_rootTopic == null) {
          _rootTopic = TopicRepository.RootTopic;
        }
        return _rootTopic;
      }
      set => _rootTopic = value;
    }

    /*==========================================================================================================================
    | PROPERTY: CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides access to the configuration data made available by the execution of the setup configuration script.
    /// </summary>
    public static Topic Configuration {
      get {
        Contract.Ensures(Contract.Result<Topic>() != null);
        if (_configuration == null) {
          _configuration = RootTopic.GetTopic("Configuration");
        }
        return _configuration;
      }
      set => _configuration = value;
    }

    /*==========================================================================================================================
    | PROPERTY: CONTENT TYPES
    >---------------------------------------------------------------------------------------------------------------------------
    | ### NOTE JJC 052115: Copied and adapted logic for loading Content Types from TopicRepository. In the future, this should
    | be consolidated as a factory method off of ContentTypeCollection and abstracted to support any RootTopic source.
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a container <see cref="ContentType" /> topic objects added to or loaded from the <see cref="RootTopic"/>
    ///   "Configuration" namespace, for use with the setup configuration script.
    /// </summary>
    /// <remarks>
    ///   Any new topic must be associated with a Content Type when created/initially saved, so the collection of
    ///   <see cref="ContentType"/> topics must operationally be available before executing other portions of the setup
    ///   configuration script.
    /// </remarks>
    public static ContentTypeDescriptorCollection ContentTypes {
      get {

        Contract.Ensures(Contract.Result<ContentTypeDescriptorCollection>() != null);

        if (_contentTypes == null) {

          _contentTypes = new ContentTypeDescriptorCollection();

          /*--------------------------------------------------------------------------------------------------------------------
          | Add any available Content Types to the collection
          \-------------------------------------------------------------------------------------------------------------------*/
          if (RootTopic.GetTopic("Configuration:ContentTypes") != null) {
            foreach (var topic in RootTopic.GetTopic("Configuration:ContentTypes").FindAllByAttribute("ContentType", "ContentType")) {

              // Add ContentType Topic to collection if not already added
              if (topic is ContentTypeDescriptor contentType && !_contentTypes.Contains(contentType.Key)) {
                _contentTypes.Add(contentType);
              }

            }
          }

        }
        return _contentTypes;
      }
    }

    /*==========================================================================================================================
    | METHOD: SAVE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Will save any topic and, optionally, its children.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     In addition to acting as a pass-through for <see cref="Topic.Save(Boolean, Boolean)"/>, this also ensures that
    ///     a) the topic is saved twice (to account for serialized topic references, such as <c>ParentId</c>), and b) the <see
    ///     cref="ContentTypes"/> collection is first synchronized with the <see cref="TopicRepository"/>, and then the
    ///     TopicRepository state is restored to its original.
    ///   </para>
    ///   <para>
    ///     The one dependency that remains on the TopicRepository is in the Save() method, which relies on it as a centralized
    ///     source for attribute metadata - and, specifically, whether an attribute should be indexed or not.To mitigate this,
    ///     this method will ensure all local content types are saved to the TopicRepository.
    ///   </para>
    /// </remarks>
    /// <note>
    ///   IMPORTANT: If a <see cref="ContentTypeDescriptor"/> with the same key exists, it will be overwritten. This ensures any
    ///   attributes added via the configuration scripts are reflected in the database. It also means, however, that the
    ///   TopicRepository's <see cref="TopicRepository.ContentTypes"/> collection will be orphaned from the TopicRepository's
    ///   configuration namespace. For that reason, the ContentTypes collection must be reset after a Save();
    /// </note>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    /// <param name="isDraft">Boolean indicator as to the topic's publishing status.</param>
    public void Save(Topic topic, bool isRecursive = false, bool isDraft = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must be specified.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure TopicRepository references the local instances of the ContentTypes
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository.ContentTypes = ContentTypes;

      /*------------------------------------------------------------------------------------------------------------------------
      | Save new configuration
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository.DataProvider.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Update Derived Topic references to ensure any newly assigned IDs are available
      \-----------------------------------------------------------------------------------------------------------------------*/
      UpdateDerivedTopics(topic, isRecursive);

      /*------------------------------------------------------------------------------------------------------------------------
      | Update TopicID references
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository.DataProvider.Save(topic, isRecursive, isDraft);

      /*------------------------------------------------------------------------------------------------------------------------
      | Reset the TopicRepository's ContentType's cache so it isn't orphaned from the TopicRepository's configuration
      \-----------------------------------------------------------------------------------------------------------------------*/
      TopicRepository.ContentTypes = null;

      }

    /*==========================================================================================================================
    | METHOD: UPDATE DERIVED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   When a topic is first saved, references to newly created <see cref="Topic.DerivedTopic"/> references will be set to
    ///   -1, since those objects don't yet have an <see cref="Topic.Id"/>. To remedy this, this function loops through the
    ///   topic tree and, for every topic that derives from another topic and reset the reference in order to force the TopicId
    ///   to update. Then, during the second save, all IDs will be correct.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <param name="isRecursive">
    ///   Boolean indicator nothing whether to recurse through the topic's descendants and save them as well.
    /// </param>
    public void UpdateDerivedTopics(Topic topic, bool isRecursive = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must be specified.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Update the topic reference/derivation
      \-----------------------------------------------------------------------------------------------------------------------*/
      topic.DerivedTopic = topic.DerivedTopic;

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over child topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (isRecursive) {
        foreach (var childTopic in topic.Children) {
          UpdateDerivedTopics(childTopic, isRecursive);
        }
      }

    }

    /*==========================================================================================================================
    | METHOD: DELETE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Deletes a Topic, if the Topic exists.
    /// </summary>
    /// <param name="topicName">
    ///   The <see cref="Topic.UniqueKey"/> or <see cref="Topic.Key"/> for the topic to be deleted.
    /// </param>
    public void DeleteTopic(string topicName) {
      var topic = RootTopic.GetTopic(topicName);
      if (topic != null) {
        TopicRepository.DataProvider.Delete(topic);
      }
      return;
    }

    /*==========================================================================================================================
    | METHOD: SET TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks up a topic; if it doesn't exist, creates it.
    /// </summary>
    /// <param name="parentTopic">The topic's <see cref="Topic.Parent"/>.</param>
    /// <param name="key">The topic's <see cref="Topic.Key"/>.</param>
    /// <param name="contentType">The topic's <see cref="Topic.ContentType"/> key.</param>
    /// <returns>The configured topic object.</returns>
    public Topic SetTopic(Topic parentTopic, string key, string contentType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(parentTopic != null, "The parent topic must be specified.");
      Topic.ValidateKey(key);

      Topic topic = null;
      if (!parentTopic.Children.Contains(key)) {

        /*----------------------------------------------------------------------------------------------------------------------
        | Create a strongly-typed ContentType object if the contentType key is set to "ContentType"
        \---------------------------------------------------------------------------------------------------------------------*/
        if (contentType.Equals("ContentType")) {
          topic = new ContentTypeDescriptor();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Create a strongly-typed AttributeDescriptor object if the contentType key is set to either "Attribute" or
        | "AttributeDescriptor".
        \---------------------------------------------------------------------------------------------------------------------*/
        else if (contentType.Equals("Attribute") || contentType.Equals("AttributeDescriptor")) {
          topic = new AttributeDescriptor();
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Create a strongly-typed ContentType object if the contentType key is set to "ContentType"
        \---------------------------------------------------------------------------------------------------------------------*/
        else {
          topic = Topic.Create(key, contentType);
        }

        /*----------------------------------------------------------------------------------------------------------------------
        | Set the primary topic properties (Key, ContentType, and Parent)
        \---------------------------------------------------------------------------------------------------------------------*/
        Contract.Assume(topic != parentTopic);
        topic.Key = key;
        topic.Attributes.SetValue("Key", key);
        topic.ContentType = null;
        topic.Attributes.SetValue("ContentType", contentType);
        topic.Parent = parentTopic;
        topic.Attributes.SetValue("ParentID", parentTopic.Id.ToString());
      }
      else {

        /*----------------------------------------------------------------------------------------------------------------------
        | Update the primary topic properties (Key, ContentType, and Parent)
        \---------------------------------------------------------------------------------------------------------------------*/
        topic = parentTopic.Children[key];
        topic.Key = key;
        topic.Attributes.SetValue("Key", key);
        topic.ContentType = null;
        topic.Attributes.SetValue("ContentType", contentType);
        topic.Attributes.SetValue("ParentID", parentTopic.Id.ToString());

      }
      return parentTopic.Children[key];
    }

    /*==========================================================================================================================
    | METHOD: SET CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks up a content type topic; if it doesn't exist, creates it. Returns a Topic representing the attributes.
    /// </summary>
    /// <param name="parentTopic">The topic's <see cref="Topic.Parent"/>.</param>
    /// <param name="key">The topic's <see cref="Topic.Key"/>.</param>
    public Topic SetContentType(Topic parentTopic, string key) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(parentTopic != null, "The parent topic must be specified.");
      Topic.ValidateKey(key);

      /*------------------------------------------------------------------------------------------------------------------------
      | Create content type, if not already present
      \-----------------------------------------------------------------------------------------------------------------------*/
      var contentType = (ContentTypeDescriptor)SetTopic(parentTopic, key, "ContentType");
      var attributes = SetTopic(contentType, "Attributes", "List");

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure content type is in ContentTypes collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!ContentTypes.Contains(key)) {
        ContentTypes.Add(contentType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return attributes collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      return attributes;

    }

    /*==========================================================================================================================
    | METHOD: ALLOW CONTENT TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Associates one content type with another, allowing Topics of the target content type to be created under Topics of the
    ///   source content type.
    /// </summary>
    /// <param name="parentContentType">The Content Type key for the parent topic.</param>
    /// <param name="childContentType">The Content Type key for the child topic.</param>
    public void AllowContentType(string parentContentType, string childContentType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(parentContentType),
        "The parentContentType is required"
      );
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(childContentType),
        "The childContentType is required"
      );
      Topic.ValidateKey(parentContentType);
      Topic.ValidateKey(childContentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      ContentTypeDescriptor parent = null;
      ContentTypeDescriptor child = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up Parent Content Type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (ContentTypes.Contains(parentContentType)) {
        parent = ContentTypes[parentContentType];
      }
      else {
        throw new Exception("The source ContentType '" + parentContentType + "' cannot be found in the ContentType Repository.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up child Content Type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (ContentTypes.Contains(childContentType)) {
        child = ContentTypes[childContentType];
      }
      else {
        throw new Exception("The target ContentType '" + childContentType + "' cannot be found in the ContentType Repository.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      AllowContentType(parent, child);

    }

    /// <summary>
    ///   Associates one content type with another, allowing Topics of the target content type to be created under Topics of the
    ///   source content type.
    /// </summary>
    /// <param name="parentContentType">The <see cref="Topic.ContentType"/> topic for the parent topic.</param>
    /// <param name="childContentType">The ContentType topic for the child topic.</param>
    public void AllowContentType(Topic parentContentType, Topic childContentType) {
      parentContentType?.Relationships.SetTopic("ContentTypes", childContentType);
    }


    /*==========================================================================================================================
    | METHOD: DISABLE CHILD TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Prevents Topics from being created under Topics of a particular ContentType.
    /// </summary>
    /// <param name="parentContentType">The Content Type key for the parent topic.</param>
    public void DisableChildTopics(string parentContentType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(
        !String.IsNullOrWhiteSpace(parentContentType),
        "The parent ContentType key must be specified."
      );
      Topic.ValidateKey(parentContentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      ContentTypeDescriptor parent = null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Look up parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (ContentTypes.Contains(parentContentType)) {
        parent = ContentTypes[parentContentType];
      }
      else {
        throw new Exception("The source ContentType '" + parentContentType + "' cannot be found in the ContentType Repository.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set the DisableChildTopics attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      DisableChildTopics(parent);

    }

    /// <summary>
    ///   Prevents Topics from being created under Topics of a particular ContentType.
    /// </summary>
    /// <param name="parentContentType">The <see cref="Topic.ContentType"/> topic for the parent topic.</param>
    public void DisableChildTopics(Topic parentContentType) {
      Contract.Requires<ArgumentNullException>(parentContentType != null, "The parent ContentType must be specified.");
      parentContentType.Attributes.SetValue("DisableChildTopics", "1");
    }

    /*==========================================================================================================================
    | METHOD: SET ATTRIBUTE REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Looks up an Topic (assumed to be an attribute) from the provided Topic collection, finds the attribute with the
    ///   associated name, and creates a Topic Pointer that points to that attribute.
    /// </summary>
    /// <param name="contentType">The <see cref="ContentTypeDescriptor"/>for the attribute topic.</param>
    /// <param name="attributes">The collection of attributes for the attribute topic.</param>
    /// <param name="key">The <see cref="Topic.Key"/> for the attribute to be referenced.</param>
    /// <returns>The topic object for the referencing topic.</returns>
    public Topic SetAttributeReference(Topic contentType, Topic attributes, string key) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(attributes != null, "The attributes topic must be specified.");
      Contract.Requires<ArgumentNullException>(contentType != null, "The contentTYpe topic must be specified.");
      Contract.Requires<ArgumentNullException>(String.IsNullOrWhiteSpace(key), "The key must be specified.");
      Contract.Ensures(Contract.Result<Topic>() != null);
      Topic.ValidateKey(key);

      if (!attributes.Children.Contains(key)) {
        throw new Exception("The attribute with the key '" + key + "' does not exist in the '" + attributes.Key + "' Topic.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish the derived and target attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attribute = attributes.Children[key];
      var attributeReference = SetTopic(contentType, attribute.Key, attribute.Attributes.GetValue("ContentType"));

      /*------------------------------------------------------------------------------------------------------------------------
      | Set the attribute reference/derivation
      \-----------------------------------------------------------------------------------------------------------------------*/
      attributeReference.DerivedTopic = attribute;

      /*------------------------------------------------------------------------------------------------------------------------
      | Return referencing attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      return attributeReference;

    }

    /*==========================================================================================================================
    | METHOD: WRITE TOPIC
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Writes a topic to the browser for confirmation or debugging purposes.
    /// </summary>
    /// <param name="topic">The topic object.</param>
    /// <returns>
    ///   A string value representing topic property values (e.g., the Content Type key, a list of the topic's attributes,
    ///   etc.).
    /// </returns>
    public string WriteTopic(Topic topic) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(topic != null, "The topic must be specified.");
      Contract.Ensures(Contract.Result<string>() != null);

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish topic properties string and primary properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      var output = ""
      + "  <h2>" + topic.Key + " <span class=\"ContentType\">(" + topic.Attributes.GetValue("ContentType") + ")</span></h2>"
      + "  <div>"
      + "    <h3>Attributes:</h3>"
      + "    <ul class=\"Attributes\">"
      + "      <li><span class=\"Attribute\">ID:</span> " + topic.Id + "</li>";

      /*------------------------------------------------------------------------------------------------------------------------
      | Write out additional attributes
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var attribute in topic.Attributes) {
        output += "<li><span class=\"Attribute\">" + attribute.Key + ":</span> " + attribute.Value + "</li>";
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Write out derived topic information
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!String.IsNullOrEmpty(topic.Attributes.GetValue("TopicID"))) {
        output += "<li><span class=\"Attribute\">DerivedTopic? " + (topic.DerivedTopic != null) + "</li>";
      }
      if (topic.DerivedTopic != null) {
        output += "<li><span class=\"Attribute\">DerivedTopic: " + topic.DerivedTopic.Key + ", " + topic.DerivedTopic.Id + "</li>";
      }

      output = output
      + "    </ul>"
      + "    <ul>";

      /*------------------------------------------------------------------------------------------------------------------------
      | Recurse over child topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in topic.Children) {
        output = output + WriteTopic(childTopic);
      }

      output = output
      + "    </ul>"
      + "  </div>";

      return output;

    }

  } // Class

} // Namespace
