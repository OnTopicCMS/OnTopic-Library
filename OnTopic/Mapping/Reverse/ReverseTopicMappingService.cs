/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Mapping;
using OnTopic.Internal.Reflection;
using OnTopic.Metadata;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Reverse {

  /*============================================================================================================================
  | CLASS: REVERSE TOPIC MAPPING SERVICE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public class ReverseTopicMappingService : IReverseTopicMappingService {

    /*==========================================================================================================================
    | STATIC VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly             TypeMemberInfoCollection        _typeCache                      = new();

    /*==========================================================================================================================
    | PRIVATE VARIABLES
    \-------------------------------------------------------------------------------------------------------------------------*/
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ContentTypeDescriptorCollection _contentTypeDescriptors;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="ReverseTopicMappingService"/> with required dependencies.
    /// </summary>
    public ReverseTopicMappingService(ITopicRepository topicRepository) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, "An instance of an ITopicRepository is required.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Set dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = topicRepository;
      _contentTypeDescriptors   = topicRepository.GetContentTypeDescriptors();

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Assume(
        _contentTypeDescriptors,
        $"The {nameof(ITopicRepository.GetContentTypeDescriptors)}() method returned null. This could indicate a corrupt " +
        $"or data source."
      );

    }

    /*==========================================================================================================================
    | METHOD: MAP (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<Topic?> MapAsync(ITopicBindingModel source) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source is null) return null;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source.Key, $"The 'source' ITopicBindingModel must contain a 'Key' value.");
      Contract.Requires(source.ContentType, $"The 'source' ITopicBindingModel must contain a 'ContentType' value.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Instantiate target
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topic = TopicFactory.Create(source.Key, source.ContentType);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide mapping
      \-----------------------------------------------------------------------------------------------------------------------*/
      return await MapAsync(source, topic).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> MapAsync<T>(ITopicBindingModel? source) where T : Topic {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source.Key, $"The 'source' ITopicBindingModel must contain a 'Key' value.");
      Contract.Requires(source.ContentType, $"The 'source' ITopicBindingModel must contain a 'ContentType' value.");

      /*------------------------------------------------------------------------------------------------------------------------
      | Map source
      \-----------------------------------------------------------------------------------------------------------------------*/
      return (T?)await MapAsync(
        source,
        TopicFactory.Create(source.Key, source.ContentType)
      ).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | METHOD: MAP (TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<Topic?> MapAsync(ITopicBindingModel? source, Topic target) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source is null) return target;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Assume(source.ContentType, nameof(source.ContentType));

      //Ensure the content type is valid
      if (!_contentTypeDescriptors.Contains(source.ContentType)) {
        throw new TopicMappingException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}'. There " +
          $"are no matching content types in the ITopicRepository provided. This suggests that the binding model is invalid. " +
          $"If this is expected—e.g., if the content type is being added as part of this operation—then it needs to be added " +
          $"to the same ITopicRepository instance prior to creating any instances of it."
        );
      }

      //Ensure the content types match
      if (source.ContentType != target.ContentType) {
        throw new TopicMappingException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}', while " +
          $"the {nameof(target)} object (with the key '{target.Key}') has a content type of '{target.ContentType}'. It is not" +
          $"permitted to change the topic's content type during a mapping operation, as this interferes with the validation. " +
          $"If this is by design, change the content type on the target topic prior to invoking MapAsync()."
        );
      }

      //Ensure the keys match
      if (source.Key != target.Key && !String.IsNullOrEmpty(source.Key)) {
        throw new TopicMappingException(
          $"The {nameof(source)} object has a key of '{source.Key}', while the {nameof(target)} object has a key of " +
          $"'{target.Key}'. It is not permitted to change the topic's key during a mapping operation, as this suggests an " +
          $"invalid target. If this is by design, change the key on the target topic prior to invoking MapAsync()."
        );
      }

      /*----------------------------------------------------------------------------------------------------------------------
      | Map source to target
      \---------------------------------------------------------------------------------------------------------------------*/
      return await MapAsync(source, target, null).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | PROTECTED: MAP (TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a binding model and an existing <see cref="Topic"/>, will map the properties of the binding model to attributes
    ///   on the <see cref="Topic"/>, optionally prefixing the attributes with the <paramref name="attributePrefix"/>.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <returns>
    ///   An instance of provided <see cref="Topic"/> with attributes appropriately mapped.
    /// </returns>
    protected async Task<Topic?> MapAsync(object? source, Topic target, string? attributePrefix) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source is null) return target;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(target, nameof(target));
      Contract.Assume(target.ContentType, nameof(target.ContentType));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var properties = _typeCache.GetMembers<PropertyInfo>(source.GetType());
      var contentTypeDescriptor = _contentTypeDescriptors.GetTopic(target.ContentType);

      BindingModelValidator.ValidateModel(source.GetType(), properties, contentTypeDescriptor, attributePrefix);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task>();
      foreach (var property in properties) {
        taskQueue.Add(SetPropertyAsync(source, target, property, attributePrefix));
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
    ///   Helper function that evaluates each property on the source <see cref="ITopicBindingModel"/> and then attempts to
    ///   locate and set the associated attribute, collection, or property on the target <see cref="Topic"/> based on
    ///   predetermined conventions.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="property">Information related to the current property.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    protected async Task SetPropertyAsync(
      object                    source,
      Topic                     target,
      PropertyInfo              property,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(property, nameof(property));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration(property, attributePrefix);
      var contentTypeDescriptor = _contentTypeDescriptors.GetTopic(target.ContentType);
      var compositeAttributeKey = configuration.AttributeKey;

      Contract.Assume(contentTypeDescriptor, nameof(contentTypeDescriptor));

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip properties decorated with [DisableMapping]
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DisableMapping) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle mapping properties from referenced objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.MapToParent) {
        await MapAsync(
          property.GetValue(source),
          target,
          configuration.AttributePrefix
        ).ConfigureAwait(false);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve attribute descriptor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeType = contentTypeDescriptor.AttributeDescriptors.GetTopic(compositeAttributeKey);

      if (attributeType is null) {
        throw new TopicMappingException(
          $"The attribute '{configuration.AttributeKey}' mapped by the {source.GetType()} could not be found on the " +
          $"'{contentTypeDescriptor.Key}' content type.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      configuration.Validate(source);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle property by type
      \-----------------------------------------------------------------------------------------------------------------------*/
      switch (attributeType.ModelType) {
        case ModelType.ScalarValue:
          SetScalarValue(source, target, configuration);
          return;
        case ModelType.Relationship:
          SetRelationships(source, target, configuration);
          return;
        case ModelType.NestedTopic:
          await SetNestedTopicsAsync(source, target, configuration).ConfigureAwait(false);
          return;
        case ModelType.Reference:
          SetReference(source, target, configuration);
          return;
      }

    }

    /*==========================================================================================================================
    | PROTECTED: SET SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the target <see cref="Topic"/> with a scalar value from the source binding model.
    /// </summary>
    /// <remarks>
    ///   Assuming the <paramref name="configuration"/>'s <see cref="PropertyConfiguration.Property"/> is of the type <see
    ///   cref="String"/>, <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="DateTime"/>, the <see
    ///   cref="SetScalarValue(Object, Topic, PropertyConfiguration)"/> method will attempt to set the property on the
    ///   <paramref name="target"/>. If the value is not set on the <paramref name="source"/> then the <see
    ///   cref="DefaultValueAttribute"/> will be evaluated as a fallback. If the property is not of a settable type then the
    ///   property is not set. If the value is empty, then it will be treated as <c>null</c> in the <paramref name="target"/>'s
    ///   <see cref="AttributeValueCollection"/>.
    /// </remarks>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static void SetScalarValue(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    )
    {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from the binding model property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetPropertyValue(source, configuration.Property.Name)?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Fall back to default, if configured
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue) && configuration.DefaultValue is not null) {
        attributeValue = configuration.DefaultValue.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle type conversion
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeValue is not null) {
        switch (configuration.Property.PropertyType.Name) {
          case nameof(Boolean):
            attributeValue = attributeValue is "True" ? "1" : "0";
            break;
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set the value (to null, if appropriate)
      \-----------------------------------------------------------------------------------------------------------------------*/
      target.Attributes.SetValue(configuration.AttributeKey, attributeValue);

    }

    /*==========================================================================================================================
    | PROTECTED: SET RELATIONSHIPS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a relationship property, identifies the target <see cref="Topic"/> for each related item, and sets it on the
    ///   source <see cref="Topic"/>'s <see cref="Topic.Relationships"/> collection.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected void SetRelationships(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    )
    {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList)configuration.Property.GetValue(source, null);

      if (sourceList is null) {
        sourceList = new List<IRelatedTopicBindingModel>();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Clear existing relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      target.Relationships.ClearTopics(configuration.AttributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships for each
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (IRelatedTopicBindingModel relationship in sourceList) {
        var targetTopic = _topicRepository.Load(relationship.UniqueKey);
        if (targetTopic is null) {
          throw new TopicMappingException(
            $"The relationship '{relationship.UniqueKey}' mapped in the '{configuration.Property.Name}' property could not " +
            $"be located in the repository."
          );
        }
        target.Relationships.SetTopic(configuration.AttributeKey, targetTopic);
      }

    }

    /*==========================================================================================================================
    | PROTECTED: SET NESTED TOPICS
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a nested topic property, serializes a topic for each property, and sets it on the target <see cref="Topic"/>'s
    ///   <see cref="Topic.Children"/> collection.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected async Task SetNestedTopicsAsync(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList)configuration.Property.GetValue(source, null) ?? new List<ITopicBindingModel>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish target collection to store mapped topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var container = target.Children.GetTopic(configuration.AttributeKey);
      if (container is null) {
        container = TopicFactory.Create(configuration.AttributeKey, "List", target);
        container.IsHidden = true;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map the topics from the source collection, and add them to the target collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      await PopulateTargetCollectionAsync(sourceList, container.Children).ConfigureAwait(false);

    }

    /*==========================================================================================================================
    | PROTECTED: SET REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a reference property, lookup the associated topic and set its <see cref="Topic.Id"/> on the <paramref
    ///   name="target"/>'s <see cref="Topic.Attributes"/> collection.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    protected void SetReference(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var modelReference = (IRelatedTopicBindingModel)configuration.Property.GetValue(source);

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass if reference (or value) is null (or empty)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (modelReference is null || String.IsNullOrEmpty(modelReference.UniqueKey)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify target value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicReference = _topicRepository.Load(modelReference.UniqueKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide error handling
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicReference is null) {
        throw new TopicMappingException(
          $"The topic '{modelReference.UniqueKey}' referenced by the '{source.GetType()}' model's " +
          $"'{configuration.Property.Name}' property could not be found."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set target attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      target.Attributes.SetInteger(configuration.AttributeKey, topicReference.Id);

    }

    /*==========================================================================================================================
    | PROTECTED: POPULATE TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source list, will populate a target list based on the configured behavior of the source property.
    /// </summary>
    /// <param name="sourceList">The <see cref="IList{ITopicBindingModel}"/> to pull the binding models from.</param>
    /// <param name="targetList">The target <see cref="IList{Topic}"/> to add the mapped <see cref="Topic"/> objects to.</param>
    protected async Task PopulateTargetCollectionAsync(
      IList                     sourceList,
      TopicCollection           targetList
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceList, nameof(sourceList));
      Contract.Requires(targetList, nameof(targetList));

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<Topic?>>();

      //Map child binding model to target collection on the target
      foreach (ITopicBindingModel childBindingModel in sourceList) {
        Contract.Assume(childBindingModel.Key);
        if (targetList.Contains(childBindingModel.Key)) {
          taskQueue.Add(MapAsync(childBindingModel, targetList.GetTopic(childBindingModel.Key)!));
        }
        else {
          taskQueue.Add(MapAsync(childBindingModel));
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Remove orphaned topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var childTopic in targetList.ToArray()) {
        if (sourceList.Cast<ITopicBindingModel>().Any(model => model.Key == childTopic.Key)) {
          continue;
        }
        targetList.Remove(childTopic);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Process mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      while (taskQueue.Count > 0) {
        var topicTask = await Task.WhenAny(taskQueue).ConfigureAwait(false);
        taskQueue.Remove(topicTask);
        var topic = await topicTask.ConfigureAwait(false);
        if (topic is not null && !targetList.Contains(topic.Key)) {
          targetList.Add(topic);
        }
      }

    }

  } //Class
} //Namespace