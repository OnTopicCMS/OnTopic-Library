/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Ignia.Topics.Collections;
using Ignia.Topics.Internal.Diagnostics;
using Ignia.Topics.Internal.Mapping;
using Ignia.Topics.Internal.Reflection;
using Ignia.Topics.Metadata;
using Ignia.Topics.Models;
using Ignia.Topics.Repositories;
using System.ComponentModel;

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
    readonly                    ITopicRepository                _topicRepository;
    readonly                    ITypeLookupService              _typeLookupService;
    readonly                    ContentTypeDescriptorCollection _contentTypeDescriptors;

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Establishes a new instance of a <see cref="ReverseTopicMappingService"/> with required dependencies.
    /// </summary>
    public ReverseTopicMappingService(ITopicRepository topicRepository, ITypeLookupService typeLookupService) {

      /*----------------------------------------------------------------------------------------------------------------------
      | Validate dependencies
      \---------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(topicRepository, "An instance of an ITopicRepository is required.");
      Contract.Requires(typeLookupService, "An instance of an ITypeLookupService is required.");

      /*----------------------------------------------------------------------------------------------------------------------
      | Set dependencies
      \---------------------------------------------------------------------------------------------------------------------*/
      _topicRepository          = topicRepository;
      _typeLookupService        = typeLookupService;
      _contentTypeDescriptors   = topicRepository.GetContentTypeDescriptors();

    }

    /*==========================================================================================================================
    | METHOD: MAP (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<Topic?> MapAsync(ITopicBindingModel source) {

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
    public async Task<T?> MapAsync<T>(ITopicBindingModel source) where T : Topic => (T?)await MapAsync(
      source,
      TopicFactory.Create(source.Key, source.ContentType)
    ).ConfigureAwait(false);

    /*==========================================================================================================================
    | METHOD: MAP (TOPIC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc/>
    public async Task<Topic?> MapAsync(ITopicBindingModel source, Topic target) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source == null) {
        return target;
      }

      //Ensure the content type is valid
      if (!_contentTypeDescriptors.Contains(source.ContentType)) {
        throw new InvalidEnumArgumentException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}'. There " +
          $"no matching content type in the ITopicRepository provided. This suggests that the binding model is invalid. If " +
          $"this is expected—e.g., if the content type is being added as part of this operation—then it needs to be added " +
          $"to the same ITopicRepository instance prior to creating any instances of it."
        );
      }

      //Ensure the content types match
      if (source.ContentType != target.ContentType) {
        throw new InvalidEnumArgumentException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}', while " +
          $"the {nameof(target)} object (with the key '{source.Key}') has a content type of '{target.ContentType}'. It is not" +
          $"permitted to change the topic's content type during a mapping operation, as this interferes with the validation. If" +
          $"this is by design, change the content type on the target topic prior to invoking MapAsync()."
        );
      }

      //Ensure the keys match
      if (source.Key != target.Key && !String.IsNullOrEmpty(source.Key)) {
        throw new InvalidEnumArgumentException(
          $"The {nameof(source)} object has a key of '{source.Key}', while the {nameof(target)} object has a key of " +
          $"'{target.Key}'. It is not permitted to change the topic'key during a mapping operation, as this suggests in " +
          $"invalid target. If this is by design, change the key on the target topic prior to invoking MapAsync()."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var properties = _typeCache.GetMembers<PropertyInfo>(source.GetType());

      BindingModelValidator.ValidateModel(source.GetType(), properties, _contentTypeDescriptors.GetTopic(target.ContentType));

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task>();
      foreach (var property in properties) {
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
    ///   Helper function that evaluates each property on the source <see cref="ITopicBindingModel"/> and then attempts to
    ///   locate and set the associated attribute, collection, or property on the target <see cref="Topic"/> based on
    ///   predetermined conventions.
    /// </summary>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="property">Information related to the current property.</param>
    protected async Task SetPropertyAsync(
      ITopicBindingModel        source,
      Topic                     target,
      PropertyInfo              property
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration(property);
      var contentType           = _contentTypeDescriptors[source.ContentType];
      var attributeType         = contentType.AttributeDescriptors.GetTopic(configuration.AttributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip properties decorated with [DisableMapping]
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DisableMapping) {
        return;
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
    ///   cref="SetScalarValue(ITopicBindingModel, Topic, PropertyConfiguration)"/> method will attempt to set the property on
    ///   the <paramref name="target"/>. If the value is not set on the <paramref name="source"/> then the <see
    ///   cref="DefaultValueAttribute"/> will be evaluated as a fallback. If the property is not of a settable type then the property
    ///   is not set. If the value is empty, then it will be treated as <c>null</c> in the <paramref name="target"/>'s <see
    ///   cref="AttributeValueCollection"/>.
    /// </remarks>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    protected static void SetScalarValue(ITopicBindingModel source, Topic target, PropertyConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from the binding model property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeValue = _typeCache.GetPropertyValue(source, configuration.Property.Name)?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Fall back to default, if configured
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue) && configuration.DefaultValue != null) {
        attributeValue = configuration.DefaultValue.ToString();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle type conversion
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeValue != null) {
        switch (configuration.Property.PropertyType.Name) {
          case nameof(Boolean):
            attributeValue = attributeValue == "True" ? "1" : "0";
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
      ITopicBindingModel        source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList)configuration.Property.GetValue(source, null);

      if (sourceList == null) {
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
      ITopicBindingModel source,
      Topic target,
      PropertyConfiguration configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList)configuration.Property.GetValue(source, null) ?? new List<ITopicBindingModel>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish target collection to store mapped topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var container = target.Children.GetTopic(configuration.AttributeKey);
      if (container == null) {
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
      ITopicBindingModel source,
      Topic target,
      PropertyConfiguration configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var modelReference = (IRelatedTopicBindingModel)configuration.Property.GetValue(source);

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass if reference (or value) is null (or empty)
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(modelReference?.UniqueKey)) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set target value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicReference = _topicRepository.Load(modelReference.UniqueKey);
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
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<Topic?>>();

      //Map child binding model to target collection on the target
      foreach (ITopicBindingModel childBindingModel in sourceList) {
        if (targetList.Contains(childBindingModel.Key)) {
          taskQueue.Add(MapAsync(childBindingModel, targetList.GetTopic(childBindingModel.Key)));
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
        if (!targetList.Contains(topic.Key)) {
          targetList.Add(topic);
        }
      }

    }

  } //Class
} //Namespace