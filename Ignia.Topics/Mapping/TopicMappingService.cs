/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
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
    static                      Dictionary<string, Type>        _typeLookup                     = null;
    static                      TypeCollection                  _typeCache                      = new TypeCollection();

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository                = null;

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
      | Ensure cache is populated
      \---------------------------------------------------------------------------------------------------------------------*/
      if (_typeLookup == null) {
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
        _typeLookup = typeLookup;
      }

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
    public object Map(Topic topic, Relationships relationships = Relationships.All) {
      return Map(topic, relationships, new Dictionary<int, object>());

    }

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
    ///     <see cref="RecurseAttribute"/> set to include <see cref="Relationships.Parents"/>).
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
    public object Map(Topic topic, object target, Relationships relationships = Relationships.All) {
      return Map(topic, target, relationships, new Dictionary<int, object>());
    }

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
    ///   <see cref="RecurseAttribute"/> set to include <see cref="Relationships.Parents"/>).
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
      var defaultValue          = (string)null;
      var inheritValue          = false;
      var attributeKey          = property.Name;
      var relationshipKey       = property.Name;
      var relationshipType      = RelationshipType.Any;
      var crawlRelationships    = Relationships.None;
      var metadataKey           = (string)null;
      var attributeFilters      = new Dictionary<string, string>();
      var topicReferenceId      = topic.Attributes.GetInteger(property.Name + "Id", 0);

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Assign default value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var defaultValueAttribute = (DefaultValueAttribute)property.GetCustomAttribute(typeof(DefaultValueAttribute), true);
      if (defaultValueAttribute != null) {
        property.SetValue(target, defaultValueAttribute.Value);
        defaultValue = defaultValueAttribute.Value.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Determine inheritance
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (property.GetCustomAttribute(typeof(InheritAttribute), true) != null) {
        inheritValue = true;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Determine attribute key
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeKeyAttribute = (AttributeKeyAttribute)property.GetCustomAttribute(typeof(AttributeKeyAttribute), true);
      if (attributeKeyAttribute != null) {
        attributeKey = attributeKeyAttribute.Value;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Determine relationship key and type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var relationshipAttribute = (RelationshipAttribute)property.GetCustomAttribute(typeof(RelationshipAttribute), true);
      if (relationshipAttribute != null) {
        relationshipKey = relationshipAttribute.Key?? relationshipKey;
        relationshipType = relationshipAttribute.Type;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Determine recursion settings
      \-----------------------------------------------------------------------------------------------------------------------*/
      var recurseAttribute = (RecurseAttribute)property.GetCustomAttribute(typeof(RecurseAttribute), true);
      if (recurseAttribute != null) {
        crawlRelationships = recurseAttribute.Relationships;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Determine metadata key, if present
      \-----------------------------------------------------------------------------------------------------------------------*/
      var metadataAttribute = (MetadataAttribute)property.GetCustomAttribute(typeof(MetadataAttribute), true);
      if (metadataAttribute != null) {
        metadataKey = metadataAttribute.Key;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attributes: Set attribute filters
      \-----------------------------------------------------------------------------------------------------------------------*/
      var filterByAttribute = property.GetCustomAttributes<FilterByAttributeAttribute>(true);
      if (filterByAttribute != null && filterByAttribute.Count() > 0) {
        foreach (var filter in filterByAttribute) {
          attributeFilters.Add(filter.Key, filter.Value);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Identity
      >-------------------------------------------------------------------------------------------------------------------------
      | ### NOTE JJC043018: The identity property requires special handling since it isn't stored as an attribute on topic.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (property.Name.Equals("Id", StringComparison.InvariantCultureIgnoreCase)) {
        _typeCache.SetPropertyValue(target, property.Name, topic.Id.ToString());
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Scalar Value
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (_typeCache.HasSettableProperty(targetType, property.Name)) {
        //Attempt to get value from topic.Get{Property}()
        var attributeValue = _typeCache.GetMethodValue(topic, "Get" + property.Name) as string;
        //Otherwise, attempts to get value from topic.Attributes.GetValue({Property})
        if (String.IsNullOrEmpty(attributeValue)) {
          attributeValue = topic.Attributes.GetValue(attributeKey, defaultValue, inheritValue);
        }
        if (attributeValue != null) {
          _typeCache.SetPropertyValue(target, property.Name, attributeValue);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Collections
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {

        //Determine the type of item in the list
        var listType = typeof(ITopicViewModel);
        if (property.PropertyType.IsGenericType) {
          //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
          listType = property.PropertyType.GetGenericArguments().Last();
        }

        //Get source for list
        IList listSource = new Topic[] { };

        //Handle children
        if (
          (relationshipKey.Equals("Children") || relationshipType.Equals(RelationshipType.Children)) &&
          relationships.HasFlag(Relationships.Children)
        ) {
          listSource = topic.Children.ToList();
        }

        //Handle (outgoing) relationships
        if (
          listSource.Count == 0 &&
          (relationshipType.Equals(RelationshipType.Any) || relationshipType.Equals(RelationshipType.Relationship)) &&
          relationships.HasFlag(Relationships.Relationships)
        ) {
          if (topic.Relationships.Contains(relationshipKey)) {
            listSource = topic.Relationships.GetTopics(relationshipKey);
          }
        }

        //Handle nested topics, or children corresponding to the property name
        if (
          listSource.Count == 0 &&
          (relationshipType.Equals(RelationshipType.Any) || relationshipType.Equals(RelationshipType.NestedTopics))
        ) {
          if (topic.Children.Contains(relationshipKey)) {
            listSource = topic.Children[relationshipKey].Children.ToList();
          }
        }

        //Handle (incoming) relationships
        if (
          listSource.Count == 0 &&
          (relationshipType.Equals(RelationshipType.Any) || relationshipType.Equals(RelationshipType.IncomingRelationship)) &&
          relationships.HasFlag(Relationships.IncomingRelationships)
        ) {
          if (topic.IncomingRelationships.Contains(relationshipKey)) {
            listSource = topic.IncomingRelationships.GetTopics(relationshipKey);
          }
        }

        //Handle Metadata relationship
        if (listSource.Count == 0 && !String.IsNullOrWhiteSpace(metadataKey)) {
          listSource = _topicRepository.Load("Root:Configuration:Metadata:" + metadataKey + ":LookupList")?.Children.ToList();
        }

        //Ensure list is created
        var list = (IList)property.GetValue(target, null);
        if (list == null) {
          list = (IList)Activator.CreateInstance(property.PropertyType);
          property.SetValue(target, list);
        }

        //Validate and populate target collection
        if (listSource != null) {
          foreach (Topic childTopic in listSource) {
            if (filterByAttribute.Any(f => !childTopic.Attributes.GetValue(f.Key, "").Equals(f.Value))) {
              continue;
            }
            if (!childTopic.IsDisabled) {
              //Handle scenario where the list type derives from Topic
              if (typeof(Topic).IsAssignableFrom(listType)) {
                //Ensure the list item derives from the list type (which may be more derived than Topic)
                if (listType.IsAssignableFrom(childTopic.GetType())) {
                  list.Add(childTopic);
                }
              }
              //Otherwise, assume the list type is a DTO
              else {
                var childDto = Map(childTopic, crawlRelationships, cache);
                //Ensure the mapped type derives from the list type
                if (listType.IsAssignableFrom(childDto.GetType())) {
                  list.Add(childDto);
                }
              }
            }
          }
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (attributeKey.Equals("Parent") && relationships.HasFlag(Relationships.Parents)) {
        if (topic.Parent != null) {
          var parent = Map(topic.Parent, crawlRelationships, cache);
          if (property.PropertyType.IsAssignableFrom(parent.GetType())) {
            property.SetValue(target, parent);
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Property: Topic Reference
      \-----------------------------------------------------------------------------------------------------------------------*/
      else if (topicReferenceId > 0 && relationships.HasFlag(Relationships.References)) {
        var topicReference = _topicRepository.Load(topicReferenceId);
        var viewModelReference = Map(topicReference, crawlRelationships, cache);
        if (property.PropertyType.IsAssignableFrom(viewModelReference.GetType())) {
          property.SetValue(target, viewModelReference);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (ValidationAttribute validator in property.GetCustomAttributes(typeof(ValidationAttribute))) {
        validator.Validate(property.GetValue(target), property.Name);
      }

    }

  } //Class
} //Namespace
