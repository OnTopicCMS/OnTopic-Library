/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OnTopic.Attributes;
using OnTopic.Internal.Collections;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Mapping;
using OnTopic.Internal.Reflection;
using OnTopic.Mapping.Annotations;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping {

  /*============================================================================================================================
  | CLASS: TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Provides a concrete implementation of the <see cref="ITopicMappingService"/> for mapping <see cref="Topic"/> instances
  ///   to data transfer objects (such as view models) based on set conventions and attribute-based hints.
  /// </summary>
  public class TopicMappingService : ITopicMappingService {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly             TypeMemberInfoCollection        _typeCache                      = new();

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITypeLookupService              _typeLookupService;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="TopicMappingService"/> with required dependencies.
    /// </summary>
    public TopicMappingService(ITopicRepository topicRepository, ITypeLookupService typeLookupService) {
      Contract.Requires(topicRepository, "An instance of an ITopicRepository is required.");
      Contract.Requires(typeLookupService, "An instance of an ITypeLookupService is required.");
      _topicRepository = topicRepository;
      _typeLookupService = typeLookupService;
    }

    /*==========================================================================================================================
    | METHOD: MAP (DYNAMIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    [return: NotNullIfNotNull("topic")]
    public async Task<object?> MapAsync(Topic? topic, Relationships relationships = Relationships.All) =>
      await MapAsync(topic, relationships, new()).ConfigureAwait(false);

    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly-typed, prefer <see cref="MapAsync{T}(Topic, Relationships)"/>.
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
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    private async Task<object?> MapAsync(
      Topic?                    topic,
      Relationships             relationships,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      #pragma warning disable IDE0078 // Use pattern matching
      if (topic is null || topic is { IsDisabled: true }) {
        return null;
      }
      #pragma warning restore IDE0078 // Use pattern matching

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      object? target;

      if (cache.TryGetValue(topic.Id, out var cacheEntry)) {
        target                  = cacheEntry.MappedTopic;
        if (cacheEntry.GetMissingRelationships(relationships) == Relationships.None) {
          return target;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate object
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {

        var viewModelType       = _typeLookupService.Lookup($"{topic.ContentType}TopicViewModel");

        if (viewModelType is null || !viewModelType.Name.EndsWith("TopicViewModel", StringComparison.CurrentCultureIgnoreCase)) {
          throw new TopicMappingException(
            $"No class named '{topic.ContentType}TopicViewModel' could be located in any loaded assemblies. This is required " +
            $"to map the topic '{topic.GetUniqueKey()}'."
          );
        }

        target                  = Activator.CreateInstance(viewModelType);

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \-----------------------------------------------------------------------------------------------------------------------*/
      return await MapAsync(topic, target, relationships, cache, attributePrefix).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> MapAsync<T>(Topic? topic, Relationships relationships = Relationships.All) where T : class, new() {
      if (typeof(Topic).IsAssignableFrom(typeof(T))) {
        return topic as T;
      }
      return (T?)await MapAsync(topic, new T(), relationships).ConfigureAwait(false);
    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<object?> MapAsync(Topic? topic, object target, Relationships relationships = Relationships.All) {
      Contract.Requires(target, nameof(target));
      return await MapAsync(topic, target, relationships, new()).ConfigureAwait(false);
    }

    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <remarks>
    ///   This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///   recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with
    ///   <see cref="FollowAttribute"/> set to include <see cref="Relationships.Parents"/>).
    /// </remarks>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    private async Task<object> MapAsync(
      Topic?                    topic,
      object                    target,
      Relationships             relationships,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      #pragma warning disable IDE0078 // Use pattern matching
      if (topic is null || topic is { IsDisabled: true }) {
        return target;
      }
      #pragma warning restore IDE0078 // Use pattern matching

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (typeof(Topic).IsAssignableFrom(target.GetType())) {
        return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      >-------------------------------------------------------------------------------------------------------------------------
      | If the cache contains an entry, check to make sure it includes all of the requested relationships. If it does, return
      | it. If it doesn't, determine the missing relationships and request to have those  mapped.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (cache.TryGetValue(topic.Id, out var cacheEntry)) {
        relationships           = cacheEntry.GetMissingRelationships(relationships);
        target                  = cacheEntry.MappedTopic;
        if (relationships == Relationships.None) {
          return cacheEntry.MappedTopic;
        }
        cacheEntry.AddMissingRelationships(relationships);
      }
      else if (!topic.IsNew) {
        cache.GetOrAdd(
          topic.Id,
          new MappedTopicCacheEntry() {
            MappedTopic         = target,
            Relationships       = relationships
          }
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task>();
      foreach (var property in _typeCache.GetMembers<PropertyInfo>(target.GetType())) {
        taskQueue.Add(SetPropertyAsync(topic, target, relationships, property, cache, attributePrefix, cacheEntry != null));
      }
      await Task.WhenAll(taskQueue.ToArray()).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

    /*==========================================================================================================================
    | PROTECTED: SET PROPERTY (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates each property on the target object and attempts to retrieve a value from the source
    ///   <see cref="Topic"/> based on predetermined conventions.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="property">Information related to the current property.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <param name="mapRelationshipsOnly">Determines if properties not associated with properties should be mapped.</param>
    protected async Task SetPropertyAsync(
      Topic                     source,
      object                    target,
      Relationships             relationships,
      PropertyInfo              property,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null,
      bool                      mapRelationshipsOnly            = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(relationships, nameof(relationships));
      Contract.Requires(property, nameof(property));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration(property, attributePrefix);
      var topicReferenceId      = source.Attributes.GetInteger($"{configuration.AttributeKey}Id", 0);

      if (topicReferenceId == 0 && configuration.AttributeKey.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase)) {
        topicReferenceId        = source.Attributes.GetInteger(configuration.AttributeKey, 0);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign default value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!mapRelationshipsOnly && configuration.DefaultValue is not null) {
        property.SetValue(target, configuration.DefaultValue);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle by type, attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DisableMapping) {
        return;
      }
      else if (SetCompatibleProperty(source, target, configuration)) {
        //Performed 1:1 mapping between source and target
      }
      else if (!mapRelationshipsOnly && _typeCache.HasSettableProperty(target.GetType(), property.Name)) {
        SetScalarValue(source, target, configuration);
      }
      else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
        await SetCollectionValueAsync(source, target, relationships, configuration, cache).ConfigureAwait(false);
      }
      else if (configuration.AttributeKey is "Parent" && relationships.HasFlag(Relationships.Parents)) {
        if (source.Parent is not null) {
          await SetTopicReferenceAsync(source.Parent, target, configuration, cache).ConfigureAwait(false);
        }
      }
      else if (topicReferenceId > 0 && relationships.HasFlag(Relationships.References)) {
        var topicReference = _topicRepository.Load(topicReferenceId);
        if (topicReference is not null) {
          await SetTopicReferenceAsync(topicReference, target, configuration, cache).ConfigureAwait(false);
        }
      }
      else if (configuration.MapToParent) {
        await MapAsync(
          source,
          property.GetValue(target),
          relationships,
          cache,
          configuration.AttributePrefix
        ).ConfigureAwait(false);
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
    ///   name="target"/> based on, in order, the <paramref name="source"/>'s <c>Get{Property}()</c> method, <c>{Property}</c>
    ///   property, and, finally, its <see cref="Topic.Attributes"/> collection (using <see
    ///   cref="Collections.AttributeValueCollection.GetValue(String, Boolean)"/>). If the property is not of a settable type,
    ///   or the source value cannot be identified on the <paramref name="source"/>, then the property is not set.
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static void SetScalarValue(Topic source, object target, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_typeCache.HasSettableProperty(target.GetType(), configuration.Property.Name)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.Get{Property}()
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetMethodValue(source, $"Get{configuration.AttributeKey}")?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.{Property}
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = _typeCache.GetPropertyValue(source, configuration.AttributeKey)?.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Otherwise, attempt to retrieve value from topic.Attributes.GetValue({Property})
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = source.Attributes.GetValue(
          configuration.AttributeKey,
          configuration.DefaultValue?.ToString(),
          configuration.InheritValue
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assuming a value was retrieved, set it
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeValue is not null) {
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
    ///   collection on the <paramref name="source"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the <paramref name="target"/> DTO.
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    protected async Task SetCollectionValueAsync(
      Topic                     source,
      object                    target,
      Relationships             relationships,
      PropertyConfiguration     configuration,
      MappedTopicCache          cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(relationships, nameof(relationships));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!typeof(IList).IsAssignableFrom(configuration.Property.PropertyType)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure target list is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      var targetList = (IList)configuration.Property.GetValue(target, null);
      if (targetList is null) {
        targetList = (IList)Activator.CreateInstance(configuration.Property.PropertyType);
        configuration.Property.SetValue(target, targetList);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = GetSourceCollection(source, relationships, configuration);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate that source collection was identified
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceList is null) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Map the topics from the source collection, and add them to the target collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      await PopulateTargetCollectionAsync(sourceList, targetList, configuration, cache).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | PROTECTED: GET SOURCE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source topic and a property configuration, attempts to identify a source collection that maps to the property.
    /// </summary>
    /// <remarks>
    ///   Given a collection <paramref name="configuration"/> on a target DTO, attempts to identify a source collection on the
    ///   <paramref name="source"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the target DTO.
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="relationships">Determines what relationships the mapping should follow, if any.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected IList<Topic> GetSourceCollection(Topic source, Relationships relationships, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(relationships, nameof(relationships));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       listSource                      = (IList<Topic>)Array.Empty<Topic>();
      var                       relationshipKey                 = configuration.RelationshipKey;
      var                       relationshipType                = configuration.RelationshipType;

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle children
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = GetRelationship(
        RelationshipType.Children,
        s => true,
        () => source.Children.ToList()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (outgoing) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = GetRelationship(
        RelationshipType.Relationship,
        source.Relationships.Contains,
        () => source.Relationships.GetTopics(relationshipKey)
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics, or children corresponding to the property name
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = GetRelationship(
        RelationshipType.NestedTopics,
        source.Children.Contains,
        () => source.Children[relationshipKey].Children
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (incoming) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = GetRelationship(
        RelationshipType.IncomingRelationship,
        source.IncomingRelationships.Contains,
        () => source.IncomingRelationships.GetTopics(relationshipKey)
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle other strongly typed source collections
      \-----------------------------------------------------------------------------------------------------------------------*/
      //The following allows a target collection to be mapped to an IList<Topic> source collection. This is valuable for custom,
      //curated collections defined on e.g. derivatives of Topic, but which don't otherwise map to a specific relationship type.
      //For example, the ContentTypeDescriptor's AttributeDescriptors collection, which provides a rollup of
      //AttributeDescriptors from the current ContentTypeDescriptor, as well as all of its ascendents.
      if (listSource.Count == 0) {
        var sourceProperty = _typeCache.GetMember<PropertyInfo>(source.GetType(), configuration.AttributeKey);
        if (sourceProperty is not null && typeof(IList).IsAssignableFrom(sourceProperty.PropertyType)) {
          if (
            sourceProperty.GetValue(source) is IList sourcePropertyValue &&
            sourcePropertyValue.Count > 0 &&
            typeof(Topic).IsAssignableFrom(sourcePropertyValue[0].GetType())
          ) {
            listSource = GetRelationship(
              RelationshipType.MappedCollection,
              s => true,
              () => sourcePropertyValue.Cast<Topic>().ToList()
            );
          }
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle Metadata relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (listSource.Count == 0 && !String.IsNullOrWhiteSpace(configuration.MetadataKey)) {
        var metadataKey = $"Root:Configuration:Metadata:{configuration.MetadataKey}:LookupList";
        var metadataParent = _topicRepository.Load(metadataKey);
        if (metadataParent is not null) {
          listSource = metadataParent.Children.ToList();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle flattening of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.FlattenChildren) {
        var flattenedList = new List<Topic>();
        listSource.ToList().ForEach(t => FlattenTopicGraph(t, flattenedList));
        listSource = flattenedList;
      }

      return listSource;

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide local function for evaluating current relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      IList<Topic> GetRelationship(RelationshipType relationship, Func<string, bool> contains, Func<IList<Topic>> getTopics) {
        var targetRelationships = RelationshipMap.Mappings[relationship];
        var preconditionsMet    =
          listSource.Count == 0 &&
          (relationshipType is RelationshipType.Any || relationshipType.Equals(relationship)) &&
          (relationshipType is RelationshipType.Children || relationship is not RelationshipType.Children) &&
          (targetRelationships is Relationships.None || relationships.HasFlag(targetRelationships)) &&
          contains(configuration.RelationshipKey);
        return preconditionsMet? getTopics() : listSource;
      }

    }

    /*==========================================================================================================================
    | PROTECTED: POPULATE TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source list, will populate a target list based on the configured behavior of the target property.
    /// </summary>
    /// <param name="sourceList">The <see cref="IList{Topic}"/> to pull the source <see cref="Topic"/> objects from.</param>
    /// <param name="targetList">The target <see cref="IList"/> to add the mapped <see cref="Topic"/> objects to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    protected async Task PopulateTargetCollectionAsync(
      IList<Topic>              sourceList,
      IList                     targetList,
      PropertyConfiguration     configuration,
      MappedTopicCache          cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceList, nameof(sourceList));
      Contract.Requires(targetList, nameof(targetList));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine the type of item in the list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var listType = typeof(ITopicViewModel);
      if (configuration.Property.PropertyType.IsGenericType) {
        //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
        listType = configuration.Property.PropertyType.GetGenericArguments().Last();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<object?>>();

      foreach (var childTopic in sourceList) {

        //Ensure the source topic matches any [FilterByAttribute()] settings
        if (!configuration.SatisfiesAttributeFilters(childTopic)) {
          continue;
        }

        if (
          configuration.ContentTypeFilter is not null &&
          !childTopic.ContentType.Equals(configuration.ContentTypeFilter, StringComparison.OrdinalIgnoreCase)
        ) {
          continue;
        }

        //Skip nested topics; those should be explicitly mapped to their own collection or topic reference
        if (childTopic.ContentType.Equals("List", StringComparison.OrdinalIgnoreCase)) {
          continue;
        }

        //Ensure the source topic isn't disabled or hidden; disabled and hidden topics should never be returned to the
        //presentation layer
        /*
        if (!childTopic.IsVisible()) {
          continue;
        }
        */

        //Map child topic to target DTO
        var childDto = (object)childTopic;
        if (!typeof(Topic).IsAssignableFrom(listType)) {
          taskQueue.Add(MapAsync(childTopic, configuration.CrawlRelationships, cache));
        }
        else {
          AddToList(childDto);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0) {
        var dtoTask             = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        var dto                 = await dtoTask.ConfigureAwait(false);
        taskQueue.Remove(dtoTask);
        if (dto is not null) {
          AddToList(dto);
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Function: Add to List
      \-----------------------------------------------------------------------------------------------------------------------*/
      void AddToList(object dto) {
        if (dto is not null && listType.IsAssignableFrom(dto.GetType())) {
          try {
            targetList.Add(dto);
          }
          catch (ArgumentException) {
            //Ignore exceptions caused by duplicate keys, in case the IList represents a keyed collection
            //We would defensively check for this, except IList doesn't provide a suitable method to do so
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
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    protected async Task SetTopicReferenceAsync(
      Topic                     source,
      object                    target,
      PropertyConfiguration     configuration,
      MappedTopicCache          cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Map referenced topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicDto = (object?)null;
      try {
        topicDto = await MapAsync(source, configuration.CrawlRelationships, cache).ConfigureAwait(false);
      }
      catch (InvalidOperationException) {
        //Disregard errors caused by unmapped view models; those are functionally equivalent to IsAssignableFrom() mismatches
      }
      if (topicDto is not null && configuration.Property.PropertyType.IsAssignableFrom(topicDto.GetType())) {
        configuration.Property.SetValue(target, topicDto);
      }
    }

    /*==========================================================================================================================
    | PROTECTED: FLATTEN TOPIC GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function recursively iterates through children and adds each to a collection.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity pull the data from.</param>
    /// <param name="targetList">The list of <see cref="Topic"/> instances to add each child to.</param>
    /// <param name="includeNestedTopics">Optionally enable including nested topics in the list.</param>
    protected IList<Topic> FlattenTopicGraph(Topic source, IList<Topic> targetList, bool includeNestedTopics = false) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(targetList, nameof(targetList));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate source properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source.IsDisabled) return targetList;
      if (source.ContentType is "List" && !includeNestedTopics) return targetList;

      /*------------------------------------------------------------------------------------------------------------------------
      | Merge source list into target list
      \-----------------------------------------------------------------------------------------------------------------------*/
      targetList.Add(source);
      source.Children.ToList().ForEach(t => FlattenTopicGraph(t, targetList));
      return targetList;

    }

    /*==========================================================================================================================
    | PROTECTED: SET COMPATIBLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets a property on the target view model to a compatible value on the source object.
    /// </summary>
    /// <remarks>
    ///   Even if the property values can't be set by the <see cref="TypeMemberInfoCollection"/>, properties should be settable
    ///   assuming the source and target types are compatible. In this case, <see cref="TopicMappingService"/> needn't know
    ///   anything about the property type as it doesn't need to do a conversion; it can just do a one-to-one mapping.
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="target">The target DTO on which to set the property value.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static bool SetCompatibleProperty(Topic source, object target, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.{Property}
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceProperty = _typeCache.GetMember<PropertyInfo>(source.GetType(), configuration.AttributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceProperty is null || !configuration.Property.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) {
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assuming a value was retrieved, set it
      \-----------------------------------------------------------------------------------------------------------------------*/
      configuration.Property.SetValue(target, sourceProperty.GetValue(source));

      return true;

    }

  } //Class
} //Namespace