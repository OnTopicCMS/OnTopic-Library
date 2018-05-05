﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Ignia.Topics.Collections;
using Ignia.Topics.Reflection;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="ITopicMappingService"/> interface provides an abstraction for mapping <see cref="Topic"/> instances to
  ///   Data Transfer Objects, such as View Models.
  /// </summary>
  public class TopicMappingService : ITopicMappingService {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly             Dictionary<string, Type>        _typeLookup                     = null;
    static readonly             TypeCollection                  _typeCache                      = new TypeCollection();

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository                = null;

    /*==========================================================================================================================
    | CONSTRUCTOR (STATIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    #pragma warning disable CA1810 // Initialize reference type static fields inline
    /// <summary>
    ///   Establishes static variables for the <see cref="TopicMappingService"/>.
    /// </summary>
    static TopicMappingService() {

      /*----------------------------------------------------------------------------------------------------------------------
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      var typeLookup = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
      var matchedTypes = AppDomain
        .CurrentDomain
        .GetAssemblies()
        .SelectMany(t => t.GetTypes())
        .Where(t => t.IsClass && t.Name.EndsWith("TopicViewModel", StringComparison.InvariantCultureIgnoreCase))
        .OrderBy(t => t.Namespace.Equals("Ignia.Topics.ViewModels"))
        .ToList();
      foreach (var type in matchedTypes) {
        var associatedContentType = type.Name.Replace("TopicViewModel", "");
        if (!typeLookup.ContainsKey(associatedContentType)) {
          typeLookup.Add(associatedContentType, type);
        }
      }
      _typeLookup               = typeLookup;

    }
    #pragma warning restore CA1810 // Initialize reference type static fields inline

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TopicMappingService"/> with required dependencies.
    /// </summary>
    public TopicMappingService(ITopicRepository topicRepository) {
      Contract.Requires<ArgumentNullException>(topicRepository != null, "An instance of an ITopicRepository is required.");
      _topicRepository = topicRepository;
    }

    /*==========================================================================================================================
    | METHOD: GET VIEW MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Static helper method for looking up a class type based on a string name.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     According to the default mapping rules, the following will each be mapped:
    ///     <list type="bullet">
    ///       <item>Scalar values of types <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="String"/></item>
    ///       <item>Properties named <c>Parent</c> (will reference <see cref="Topic.Parent"/>)</item>
    ///       <item>Collections named <c>Children</c> (will reference <see cref="Topic.Children"/>)</item>
    ///       <item>
    ///         Collections starting with <c>Related</c> (will reference corresponding <see cref="Topic.Relationships"/>)
    ///       </item>
    ///       <item>
    ///         Collections corresponding to any <see cref="Topic.Children"/> of the same name, and the type <c>ListItem</c>,
    ///         representing a nested topic list
    ///       </item>
    ///     </list>
    ///   </para>
    ///   <para>
    ///     Currently, this method uses reflection to lookup all types ending with <c>TopicViewModel</c> across all assemblies
    ///     and namespaces. This is incredibly non-performant, and can take over a second to execute. As such, this data is
    ///     cached for the duration of the application (it is not expected that new classes will be generated during the scope
    ///     of the application).
    ///   </para>
    /// </remarks>
    /// <param name="contentType">A string representing the key of the target content type.</param>
    /// <returns>A class type corresponding to the specified string, and ending with "TopicViewModel".</returns>
    /// <requires description="The contentType key must be specified." exception="T:System.ArgumentNullException">
    ///   !String.IsNullOrWhiteSpace(contentType)
    /// </requires>
    /// <requires
    ///   decription="The contentType should be an alphanumeric sequence; it should not contain spaces or symbols."
    ///   exception="T:System.ArgumentException">
    ///   !contentType.Contains(" ")
    /// </requires>
    private static Type GetViewModelType(string contentType) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate contracts
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires<ArgumentNullException>(!String.IsNullOrWhiteSpace(contentType));
      Contract.Ensures(Contract.Result<Type>() != null);
      TopicFactory.ValidateKey(contentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Return cached entry
      \---------------------------------------------------------------------------------------------------------------------*/
      if (_typeLookup.Keys.Contains(contentType)) {
        return _typeLookup[contentType];
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Return default
      \---------------------------------------------------------------------------------------------------------------------*/
      return typeof(object);

    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly-typed, then prefer <see cref="Map{T}(Topic, Relationships)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    public object Map(Topic topic, Relationships relationships = Relationships.All) =>
      Map(topic, relationships, new Dictionary<int, object>());

    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly-typed, then prefer <see cref="Map{T}(Topic, Relationships)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    ///   <para>
    ///     This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///     recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with
    ///     <see cref="FollowAttribute"/> set to include <see cref="Relationships.Parents"/>).
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    private object Map(Topic topic, Relationships relationships, Dictionary<int, object> cache) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \---------------------------------------------------------------------------------------------------------------------*/
      if (topic == null) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (cache.ContainsKey(topic.Id)) {
        return cache[topic.Id];
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Instantiate object
      \---------------------------------------------------------------------------------------------------------------------*/
      var contentType = topic.ContentType;
      var viewModelType = TopicMappingService.GetViewModelType(contentType);
      var target = Activator.CreateInstance(viewModelType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \---------------------------------------------------------------------------------------------------------------------*/
      var mappedTarget = Map(topic, target, relationships, cache);

      /*----------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \---------------------------------------------------------------------------------------------------------------------*/
      return mappedTarget;

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and a generic type, will instantiate a new instance of the generic type and populate it according to the
    ///   rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>
    ///   An instance of the requested View Model <typeparamref name="T"/> with properties appropriately mapped.
    /// </returns>
    public T Map<T>(Topic topic, Relationships relationships = Relationships.All) where T : class, new() {

      if (typeof(Topic).IsAssignableFrom(typeof(T))) {
        return topic as T;
      }
      var target = new T();
      return (T)Map(topic, target, relationships);

    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    public object Map(Topic topic, object target, Relationships relationships = Relationships.All) =>
      Map(topic, target, relationships, new Dictionary<int, object>());

    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <remarks>
    ///   This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///   recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with
    ///   <see cref="FollowAttribute"/> set to include <see cref="Relationships.Parents"/>).
    /// </remarks>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    private object Map(Topic topic, object target, Relationships relationships, Dictionary<int, object> cache) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic == null || topic.IsDisabled) {
        return target;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (typeof(Topic).IsAssignableFrom(target.GetType())) {
        return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (cache.ContainsKey(topic.Id)) {
        return cache[topic.Id];
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic.Id > 0 && !cache.ContainsKey(topic.Id)) {
        cache.Add(topic.Id, target);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var property in _typeCache.GetMembers<PropertyInfo>(target.GetType())) {
        SetProperty(topic, target, relationships, property, cache);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

    /*==========================================================================================================================
    | PRIVATE: SET PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates each property on the target object and attempts to retrieve a value from the source
    ///   <see cref="Topic"/> based on predetermined conventions.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="property">Information related to the current property.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    private void SetProperty(
      Topic topic,
      object target,
      Relationships relationships,
      PropertyInfo property,
      Dictionary<int, object> cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceType            = topic.GetType();
      var targetType            = target.GetType();
      var configuration         = new PropertyConfiguration(property);
      var topicReferenceId      = topic.Attributes.GetInteger(property.Name + "Id", 0);

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Assign default value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DefaultValue != null) {
        property.SetValue(target, configuration.DefaultValue);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Scalar Value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_typeCache.HasSettableProperty(targetType, property.Name)) {
        SetScalarValue(topic, target, configuration);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Collections
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
        SetCollectionValue(topic, target, relationships, cache, configuration);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (configuration.AttributeKey.Equals("Parent") && relationships.HasFlag(Relationships.Parents)) {
        SetTopicReference(topic.Parent, target, configuration, cache);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Topic Reference
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (topicReferenceId > 0 && relationships.HasFlag(Relationships.References)) {
        var topicReference = _topicRepository.Load(topicReferenceId);
        SetTopicReference(topicReference, target, configuration, cache);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      configuration.Validate(target);

    }

    /*==========================================================================================================================
    | PROTECTED: SET SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a scalar property on a target DTO.
    /// </summary>
    /// <remarks>
    ///   Assuming the <paramref name="configuration"/>'s <see cref="PropertyConfiguration.Property"/> is of the type <see
    ///   cref="String"/>, <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="DateTime"/>, the <see
    ///   cref="SetScalarValue(Topic,Object, PropertyConfiguration)"/> method will attempt to set the property on the <paramref
    ///   name="target"/> based on, in order, the <paramref name="topic"/>'s <c>Get{Property}()</c> method, <c>{Property}</c>
    ///   property, and, finally, its <see cref="Topic.Attributes"/> collection (using <see
    ///   cref="AttributeValueCollection.GetValue(String, Boolean)"/>). If the property is not of a settable type, or the source
    ///   value cannot be identified on the <paramref name="topic"/>, then the property is not set.
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static void SetScalarValue(Topic topic, object target, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_typeCache.HasSettableProperty(target.GetType(), configuration.Property.Name)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.Get{Property}()
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetMethodValue(topic, "Get" + configuration.AttributeKey)?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.{Property}
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = _typeCache.GetPropertyValue(topic, configuration.AttributeKey)?.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Otherwise, attempt to retrieve value from topic.Attributes.GetValue({Property})
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = topic.Attributes.GetValue(
          configuration.AttributeKey,
          configuration.DefaultValue?.ToString(),
          configuration.InheritValue
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assuming a value was retrieved, set it
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeValue != null) {
        _typeCache.SetPropertyValue(target, configuration.Property.Name, attributeValue);
      }

    }

    /*==========================================================================================================================
    | PROTECTED: SET COLLECTION VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a collection property, identifies a source collection, maps the values to DTOs, and attempts to add them to the
    ///   target collection.
    /// </summary>
    /// <remarks>
    ///   Given a collection <paramref name="configuration"/> on a <paramref name="target"/> DTO, attempts to identify a source
    ///   collection on the <paramref name="topic"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the <paramref name="target"/> DTO.
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected void SetCollectionValue(
      Topic topic,
      object target,
      Relationships relationships,
      Dictionary<int, object> cache,
      PropertyConfiguration configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!typeof(IList).IsAssignableFrom(configuration.Property.PropertyType)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure target list is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      var targetList = (IList)configuration.Property.GetValue(target, null);
      if (targetList == null) {
        targetList = (IList)Activator.CreateInstance(configuration.Property.PropertyType);
        configuration.Property.SetValue(target, targetList);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = GetSourceCollection(topic, relationships, configuration);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate that source collection was identified
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceList == null) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map the topics from the source collection, and add them to the target collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      PopulateTargetCollection(configuration, cache, sourceList, targetList);


    }

    /*==========================================================================================================================
    | PROTECTED: GET SOURCE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source topic and a property configuration, attempts to identify a source collection that maps to the property.
    /// </summary>
    /// <remarks>
    ///   Given a collection <paramref name="configuration"/> on a <paramref name="target"/> DTO, attempts to identify a source
    ///   collection on the <paramref name="topic"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the target DTO.
    /// </remarks>
    /// <param name="topic">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected IList<Topic> GetSourceCollection(Topic topic, Relationships relationships, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      IList<Topic> listSource = new Topic[] { };

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        (configuration.RelationshipKey.Equals("Children") || configuration.RelationshipType.Equals(RelationshipType.Children)) &&
        IsCurrentRelationship(RelationshipType.Children)
      ) {
        listSource = topic.Children.ToList();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (outgoing) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (IsCurrentRelationship(RelationshipType.Relationship)) {
        if (topic.Relationships.Contains(configuration.RelationshipKey)) {
          listSource = topic.Relationships.GetTopics(configuration.RelationshipKey);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics, or children corresponding to the property name
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (IsCurrentRelationship(RelationshipType.NestedTopics)) {
        if (topic.Children.Contains(configuration.RelationshipKey)) {
          listSource = topic.Children[configuration.RelationshipKey].Children.ToList();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (incoming) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (IsCurrentRelationship(RelationshipType.IncomingRelationship)) {
        if (topic.IncomingRelationships.Contains(configuration.RelationshipKey)) {
          listSource = topic.IncomingRelationships.GetTopics(configuration.RelationshipKey);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle Metadata relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (listSource.Count == 0 && !String.IsNullOrWhiteSpace(configuration.MetadataKey)) {
        listSource = _topicRepository.Load("Root:Configuration:Metadata:" + configuration.MetadataKey + ":LookupList")?
          .Children.ToList();
      }


      /*------------------------------------------------------------------------------------------------------------------------
      | Handle flattening of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.FlattenChildren) {
        var flattenedList = new List<Topic>();
        foreach (var childTopic in listSource) {
          PopulateChildTopics(childTopic, flattenedList);
        }
        listSource = flattenedList;
      }

      return listSource;

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide local function for evaluating current relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      bool IsCurrentRelationship(RelationshipType targetRelationshipType) => (
        listSource.Count == 0 &&
        (
          configuration.RelationshipType.Equals(RelationshipType.Any) ||
          configuration.RelationshipType.Equals(targetRelationshipType)
        ) &&
        (
          RelationshipMap.Mappings[targetRelationshipType].Equals(Relationships.None) ||
          relationships.HasFlag(RelationshipMap.Mappings[targetRelationshipType])
        )
      );


    }

    /*==========================================================================================================================
    | PROTECTED: POPULATE TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source list, will populate a target list based on the configured behavior of the target property.
    /// </summary>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="sourceList">The <see cref="IList{Topic}"/> to pull the source <see cref="Topic"/> objects from.</param>
    /// <param name="targetList">The target <see cref="IList"/> to add the mapped <see cref="Topic"/> objects to.</param>
    protected void PopulateTargetCollection(
      PropertyConfiguration configuration,
      Dictionary<int, object> cache,
      IList<Topic> sourceList,
      IList targetList
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine the type of item in the list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var listType = typeof(ITopicViewModel);
      if (configuration.Property.PropertyType.IsGenericType) {
        //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
        listType = configuration.Property.PropertyType.GetGenericArguments().Last();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Populate the target collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in sourceList) {

        //Ensure the source topic matches any [FilterByAttribute()] settings
        if (!configuration.SatisfiesAttributeFilters(childTopic)) {
          continue;
        }

        //Ensure the source topic isn't disabled; disabled topics should never be returned to the presentation layer
        if (childTopic.IsDisabled) {
          continue;
        }

        //Map child topic to target DTO
        var childDto = (object)childTopic;
        if (!typeof(Topic).IsAssignableFrom(listType)) {
          childDto = Map(childTopic, configuration.CrawlRelationships, cache);
        }

        //Ensure the list item derives from the list type
        if (listType.IsAssignableFrom(childDto.GetType())) {
          try {
            targetList.Add(childDto);
          }
          catch (ArgumentException) {
            //Ignore exceptions caused by duplicate keys
          }
        }

      }
    }

    /*==========================================================================================================================
    | PROTECTED: SET TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a reference to an external topic, attempts to match it to a matching property.
    /// </summary>
    /// <param name="topic">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected void SetTopicReference(
      Topic topic,
      object target,
      PropertyConfiguration configuration,
      Dictionary<int, object> cache
    ) {
      var topicDto = Map(topic, configuration.CrawlRelationships, cache);
      if (topicDto != null && configuration.Property.PropertyType.IsAssignableFrom(topicDto.GetType())) {
        configuration.Property.SetValue(target, topicDto);
      }
    }

    /*==========================================================================================================================
    | PROTECTED: POPULATE CHILD TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function recursively iterates through children and adds each to a collection.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity pull the data from.</param>
    /// <param name="topics">The list of <see cref="Topic"/> instances to add each child to.</param>
    /// <param name="includeNestedTopics">Optionally enable including nested topics in the list.</param>
    protected IList<Topic> PopulateChildTopics(Topic topic, IList<Topic> topics, bool includeNestedTopics = false) {
      if (topic.IsDisabled) return topics;
      if (topic.ContentType.Equals("List") && !includeNestedTopics) return topics;
      topics.Add(topic);
      foreach (var child in topic.Children) {
        PopulateChildTopics(child, topics);
      }
      return topics;
    }

  } //Class
} //Namespace
