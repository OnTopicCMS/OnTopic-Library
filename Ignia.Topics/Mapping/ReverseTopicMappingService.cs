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
using System.Globalization;

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
      foreach (var property in _typeCache.GetMembers<PropertyInfo>(source.GetType())) {
        taskQueue.Add(SetPropertyAsync(source, target, property));
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

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      configuration.Validate(source);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle by type, attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      // ### TODO JJC20190220: This should use the `ContentTypeDescriptor` to determine if it's an attribute. Or assume based on
      // a fixed number of convertible types (e.g., string, int, bool, DateTime).
      if (_typeCache.HasSettableProperty(source.GetType(), property.Name)) {
        SetScalarValue(source, target, configuration);
      }
      else if (typeof(IList).IsAssignableFrom(property.PropertyType)) {
        await SetCollectionValueAsync(source, target, configuration).ConfigureAwait(false);
      }
      else if (configuration.AttributeKey == "Parent") {
        throw new InvalidOperationException(
          $"The {nameof(ReverseTopicMappingService)} does not support mapping Parent topics. This property should be removed " +
          $"from the binding model, or otherwise decorated with the {nameof(DisableMappingAttribute)} to prevent it from being" +
          $"evaluated by the {nameof(ReverseTopicMappingService)}."
        );
      }
      else if (typeof(IRelatedTopicBindingModel).IsAssignableFrom(property.PropertyType)) {
        var topicReference = _topicRepository.Load(((IRelatedTopicBindingModel)property.GetValue(source)).UniqueKey);
        //#### TODO JJC20190221: Should this enforce the appending of an ID to maintain the convention? Or assume the
        //developer knows to set an alias, or is deliberately breaking the convention?
        target.Attributes.SetInteger(configuration.AttributeKey, topicReference.Id);
      }

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
      //#### TODO JC20190219: Validate using the ITopicRepository's ContentTypeDescriptor to ensure it's a valid attribute

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from the binding model property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetPropertyValue(source, configuration.Property.Name)?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Fall back to default, if configured
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = configuration.DefaultValue.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assuming a value was retrieved, set it
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeValue != null) {
        target.Attributes.SetValue(configuration.AttributeKey, attributeValue);
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
      if (!typeof(IList<ITopicBindingModel>).IsAssignableFrom(configuration.Property.PropertyType)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure source list is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList<ITopicBindingModel>)configuration.Property.GetValue(target, null)?? new List<ITopicBindingModel>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish target collection to store topics to be mapped
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
        target.Children.Contains,
        () => target.Children[relationshipKey].Children
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

      foreach (var childBindingModel in sourceList) {

        //Map child binding model to target collection on the target
        if (typeof(Topic).IsAssignableFrom(listType)) {
          taskQueue.Add(MapAsync(childBindingModel));
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

  } //Class
} //Namespace