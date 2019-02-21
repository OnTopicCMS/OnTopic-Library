/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using Ignia.Topics.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignia.Topics.Reflection;
using Ignia.Topics.Repositories;
using Ignia.Topics.ViewModels;

namespace Ignia.Topics.Mapping {

  /*============================================================================================================================
  | CLASS: REVERSE TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc/>
  public class ReverseTopicMappingService : IReverseTopicMappingService {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly             TypeMemberInfoCollection        _typeCache                      = new TypeMemberInfoCollection();

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository                = null;
    readonly                    ITypeLookupService              _typeLookupService              = null;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="ReverseTopicMappingService"/> with required dependencies.
    /// </summary>
    public ReverseTopicMappingService(ITopicRepository topicRepository, ITypeLookupService typeLookupService) {
      Contract.Requires<ArgumentNullException>(topicRepository != null, "An instance of an ITopicRepository is required.");
      Contract.Requires<ArgumentNullException>(typeLookupService != null, "An instance of an ITypeLookupService is required.");
      _topicRepository = topicRepository;
      _typeLookupService = typeLookupService;
    }

    /*==========================================================================================================================
    | METHOD: MAP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<Topic> MapAsync(ITopicBindingModel source) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \---------------------------------------------------------------------------------------------------------------------*/
      if (source == null) return null;

      /*----------------------------------------------------------------------------------------------------------------------
      | Instantiate target
      \---------------------------------------------------------------------------------------------------------------------*/
      var topic = TopicFactory.Create(source.Key, source.ContentType);

      /*----------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \---------------------------------------------------------------------------------------------------------------------*/
      return await MapAsync(source, topic).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<T> MapAsync<T>(ITopicBindingModel source) where T : Topic {
      return (T)await MapAsync(
        source,
        TopicFactory.Create(source.Key, source.ContentType)
      ).ConfigureAwait(false);
    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<Topic> MapAsync(ITopicBindingModel source, Topic target) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source == null) {
        return target;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task>();
      foreach (var property in _typeCache.GetMembers<PropertyInfo>(target.GetType())) {
        taskQueue.Add(SetPropertyAsync(topic, target, relationships, property, cache));
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
    ///   <see cref="ITopicBindingModel"/> based on predetermined conventions.
    /// </summary>
    /// <param name="source">The binding model—any plain old C# object—to derive the data from.</param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="property">Information related to the current property.</param>
    protected async Task SetPropertyAsync(
      ITopicBindingModel source,
      Topic target,
      PropertyInfo property
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration(property);
      var topicReferenceId      = source.Attributes.GetInteger($"{property.Name}Id", 0);

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign default value
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DefaultValue != null) {
        property.SetValue(target, configuration.DefaultValue);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle by type, attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_typeCache.HasSettableProperty(target.GetType(), property.Name)) {
        SetScalarValue(source, target, configuration);
      }
      else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
        await SetCollectionValueAsync(source, target, relationships, configuration, cache).ConfigureAwait(false);
      }
      else if (configuration.AttributeKey == "Parent" && relationships.HasFlag(Relationships.Parents)) {
        await SetTopicReferenceAsync(source.Parent, target, configuration, cache).ConfigureAwait(false);
      }
      else if (topicReferenceId > 0 && relationships.HasFlag(Relationships.References)) {
        var topicReference = _topicRepository.Load(topicReferenceId);
        await SetTopicReferenceAsync(topicReference, target, configuration, cache).ConfigureAwait(false);
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
    ///   cref="SetScalarValue(ITopicBindingModel, Topic, PropertyConfiguration)"/> method will attempt to set the property on
    ///   the <paramref name="target"/> based on, in order, the <paramref name="source"/>'s <c>Get{Property}()</c> method,
    ///   <c>{Property}</c> property, and, finally, its <see cref="Topic.Attributes"/> collection (using <see
    ///   cref="Collections.AttributeValueCollection.GetValue(String, Boolean)"/>). If the property is not of a settable type,
    ///   or the source value cannot be identified on the <paramref name="source"/>, then the property is not set.
    /// </remarks>
    /// <param name="source">The binding model—any plain old C# object—to derive the data from.</param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static void SetScalarValue(ITopicBindingModel source, Topic target, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!_typeCache.HasSettableProperty(target.GetType(), configuration.Property.Name)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from target.Get{Property}()
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetMethodValue(source, $"Get{configuration.AttributeKey}")?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from the binding model property
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
    ///   collection on the <paramref name="source"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested target (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the <paramref name="target"/> DTO.
    /// </remarks>
    /// <param name="source">The binding model—any plain old C# object—to derive the data from.</param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected async Task SetCollectionValueAsync(
      ITopicBindingModel source,
      Topic target,
      PropertyConfiguration configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!typeof(IList).IsAssignableFrom(configuration.Property.PropertyType)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure source list is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList<ITopicBindingModel>)configuration.Property.GetValue(target, null)?? new List<ITopicBindingModel>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      //#### TODO JC20190219: This is now just an IList and can be retrieved using Configuration.Property.GetValue()
      //#### TODO JC20190219: May need to create a child List ContentType that IsHidden for Nested Topics
      var targetList = GetTargetCollection(target, configuration);

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate that source collection was identified
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceList == null) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Map the topics from the source collection, and add them to the target collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      //#### TODO JC20190219: If it's a relationships collection (and that's all we should be supporting right now!) then it
      //should be using target.SetRelationship().
      await PopulateTargetCollectionAsync(sourceList, targetList, configuration).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | PROTECTED: GET TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a target target and a property configuration, attempts to identify the source collection that maps to the
    ///   property.
    /// </summary>
    /// <remarks>
    ///   Given a collection <paramref name="configuration"/> on the source binding model, attempts to identify a source
    ///   collection on the <paramref name="target"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
    ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested target (which will be part of
    ///   <see cref="Topic.Children"/>). By default, <see cref="ReverseTopicMappingService"/> will attempt to map based on the
    ///   property name, though this behavior can be modified using the <paramref name="configuration"/>, based on annotations
    ///   on the source DTO.
    /// </remarks>
    /// <param name="target">The binding model—any plain old C# object—to derive the data from.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected IList<Topic> GetTargetCollection(Topic target, PropertyConfiguration configuration) {

      //#### TODO JC20190219: If this is still needed, it likely needs to be flipped, such that it's finding the TARGET
      //collection on the target. That said, this really only provides value if Children and Nested Topics are supported.

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
        () => target.Children.ToList()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (outgoing) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = GetRelationship(
        RelationshipType.Relationship,
        target.Relationships.Contains,
        () => target.Relationships.GetTopics(relationshipKey)
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
        target.IncomingRelationships.Contains,
        () => target.IncomingRelationships.GetTopics(relationshipKey)
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle Metadata relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (listSource.Count == 0 && !String.IsNullOrWhiteSpace(configuration.MetadataKey)) {
        var metadataKey = $"Root:Configuration:Metadata:{configuration.MetadataKey}:LookupList";
        listSource = _topicRepository.Load(metadataKey)?.Children.ToList();
      }

       /*------------------------------------------------------------------------------------------------------------------------
      | Handle flattening of children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.FlattenChildren) {
        var flattenedList = new List<Topic>();
        listSource.ToList().ForEach(t => PopulateChildTopics(t, flattenedList));
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
          (relationshipType.Equals(RelationshipType.Any) || relationshipType.Equals(relationship)) &&
          (relationshipType.Equals(RelationshipType.Children) || !relationship.Equals(RelationshipType.Children)) &&
          (targetRelationships.Equals(Relationships.None) || relationships.HasFlag(targetRelationships)) &&
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
    protected async Task PopulateTargetCollectionAsync(
      IList<ITopicBindingModel> sourceList,
      IList<Topic> targetList,
      PropertyConfiguration configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Determine the type of item in the list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var listType = typeof(ITopicBindingModel);
      if (configuration.Property.PropertyType.IsGenericType) {
        //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
        listType = configuration.Property.PropertyType.GetGenericArguments().Last();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<Topic>>();

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
          taskQueue.Add(MapAsync(childTopic, configuration.CrawlRelationships, cache));
        } else {
          AddToList(childDto);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0) {
        var dtoTask = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        taskQueue.Remove(dtoTask);
        AddToList(await dtoTask.ConfigureAwait(false));
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Function: Add to List
      \-----------------------------------------------------------------------------------------------------------------------*/
      void AddToList(Topic dto) {
        if (listType.IsAssignableFrom(dto.GetType())) {
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
      Topic source,
      object target,
      PropertyConfiguration configuration,
      ConcurrentDictionary<int, object> cache
    ) {
      var topicDto = await MapAsync(source, configuration.CrawlRelationships, cache).ConfigureAwait(false);
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
    /// <param name="source">The <see cref="Topic"/> entity pull the data from.</param>
    /// <param name="targetList">The list of <see cref="Topic"/> instances to add each child to.</param>
    /// <param name="includeNestedTopics">Optionally enable including nested topics in the list.</param>
    protected IList<Topic> PopulateChildTopics(Topic source, IList<Topic> targetList, bool includeNestedTopics = false) {
      if (source.IsDisabled) return targetList;
      if (source.ContentType == "List" && !includeNestedTopics) return targetList;
      targetList.Add(source);
      source.Children.ToList().ForEach(t => PopulateChildTopics(t, targetList));
      return targetList;
    }

  } //Class
} //Namespace