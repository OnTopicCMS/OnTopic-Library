/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using OnTopic.Attributes;
using OnTopic.Collections;
using OnTopic.Internal.Reflection;
using OnTopic.Mapping.Internal;
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
        throw new MappingModelValidationException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}'. There " +
          $"are no matching content types in the ITopicRepository provided. This suggests that the binding model is invalid. " +
          $"If this is expected—e.g., if the content type is being added as part of this operation—then it needs to be added " +
          $"to the same ITopicRepository instance prior to creating any instances of it."
        );
      }

      //Ensure the content types match
      if (source.ContentType != target.ContentType) {
        throw new MappingModelValidationException(
          $"The {nameof(source)} object (with the key '{source.Key}') has a content type of '{source.ContentType}', while " +
          $"the {nameof(target)} object (with the key '{target.Key}') has a content type of '{target.ContentType}'. It is not" +
          $"permitted to change the topic's content type during a mapping operation, as this interferes with the validation. " +
          $"If this is by design, change the content type on the target topic prior to invoking MapAsync()."
        );
      }

      //Ensure the keys match
      if (source.Key != target.Key && !String.IsNullOrEmpty(source.Key)) {
        throw new MappingModelValidationException(
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
    | PRIVATE: MAP (TOPIC)
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
    private async Task<Topic?> MapAsync(object? source, Topic target, string? attributePrefix) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle null source
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source is null) return target;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate model
      \-----------------------------------------------------------------------------------------------------------------------*/
      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(source.GetType());
      var contentTypeDescriptor = _contentTypeDescriptors.GetValue(target.ContentType);

      BindingModelValidator.ValidateModel(typeAccessor, contentTypeDescriptor, attributePrefix);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task>();
      foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
        taskQueue.Add(SetPropertyAsync(source, target, property, attributePrefix));
      }
      await Task.WhenAll(taskQueue.ToArray()).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

    /*==========================================================================================================================
    | PRIVATE: SET PROPERTY (ASYNC)
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
    /// <param name="propertyAccessor">Information related to the current property.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    private async Task SetPropertyAsync(
      object                    source,
      Topic                     target,
      MemberAccessor            propertyAccessor,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration(propertyAccessor, attributePrefix);
      var contentTypeDescriptor = _contentTypeDescriptors.GetValue(target.ContentType);
      var compositeAttributeKey = configuration.AttributeKey;

      Contract.Assume(contentTypeDescriptor, nameof(contentTypeDescriptor));

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip properties decorated with [DisableMapping]
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DisableMapping) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip properties injected by the compiler for record types
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (propertyAccessor.Name is "EqualityContract") {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle mapping properties from referenced objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.MapToParent) {
        await MapAsync(
          propertyAccessor.GetValue(source),
          target,
          configuration.AttributePrefix
        ).ConfigureAwait(false);
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve attribute descriptor
      \-----------------------------------------------------------------------------------------------------------------------*/
      var attributeType = contentTypeDescriptor.AttributeDescriptors.GetValue(compositeAttributeKey);

      if (attributeType is null) {
        throw new MappingModelValidationException(
          $"The attribute '{configuration.AttributeKey}' mapped by the {source.GetType()} could not be found on the " +
          $"'{contentTypeDescriptor.Key}' content type.");
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      propertyAccessor.Validate(source);

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
    | PRIVATE: SET SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Sets an attribute on the target <see cref="Topic"/> with a scalar value from the source binding model.
    /// </summary>
    /// <remarks>
    ///   Assuming the <paramref name="configuration"/>'s <see cref="PropertyConfiguration.MemberAccessor"/> is of the type <see
    ///   cref="String"/>, <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="DateTime"/>, the <see cref="SetScalarValue(
    ///   Object, Topic, PropertyConfiguration)"/> method will attempt to set the property on the <paramref name="target"/>. If
    ///   the value is not set on the <paramref name="source"/> then the <see cref="DefaultValueAttribute"/> will be evaluated
    ///   as a fallback. If the property is not of a settable type then the property is not set. If the value is empty, then it
    ///   will be treated as <c>null</c> in the <paramref name="target"/>'s <see cref="AttributeCollection"/>.
    /// </remarks>
    /// <param name="source">
    ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
    /// </param>
    /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <autogeneratedoc />
    private static void SetScalarValue(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from the binding model property
      \-----------------------------------------------------------------------------------------------------------------------*/
      var memberAccessor        = configuration.MemberAccessor;
      var attributeValue        = memberAccessor.GetValue(source)?.ToString();

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
        switch (memberAccessor.Type.Name) {
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
    | PRIVATE: SET RELATIONSHIPS
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
    private void SetRelationships(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList?)configuration.MemberAccessor.GetValue(source);

      if (sourceList is null) {
        sourceList = new List<IAssociatedTopicBindingModel>();
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Clear existing relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      target.Relationships.Clear(configuration.AttributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set relationships for each
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (IAssociatedTopicBindingModel relationship in sourceList) {
        var targetTopic = _topicRepository.Load(relationship.UniqueKey, target);
        if (targetTopic is null) {
          throw new MappingModelValidationException(
            $"The relationship '{relationship.UniqueKey}' mapped in the '{configuration.MemberAccessor.Name}' property could " +
            $"not be located in the repository."
          );
        }
        target.Relationships.SetValue(configuration.AttributeKey, targetTopic);
      }

    }

    /*==========================================================================================================================
    | PRIVATE: SET NESTED TOPICS
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
    private async Task SetNestedTopicsAsync(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = (IList?)configuration.MemberAccessor.GetValue(source) ?? new List<ITopicBindingModel>();

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish target collection to store mapped topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      var container = target.Children.GetValue(configuration.AttributeKey);
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
    | PRIVATE: SET REFERENCE
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
    private void SetReference(
      object                    source,
      Topic                     target,
      PropertyConfiguration     configuration
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Retrieve source value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var modelReference = (IAssociatedTopicBindingModel?)configuration.MemberAccessor.GetValue(source);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide error handling
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (modelReference is null || modelReference.UniqueKey is null) {
        throw new MappingModelValidationException(
          $"The {configuration.MemberAccessor.Name} property must reference an object with its `UniqueKey` property set The " +
          $"value may be empty, but it should not be null."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify target value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicReference = _topicRepository.Load(modelReference.UniqueKey, target);

      /*------------------------------------------------------------------------------------------------------------------------
      | Provide error handling
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (modelReference.UniqueKey.Length > 0 && topicReference is null) {
        throw new MappingModelValidationException(
          $"The topic '{modelReference.UniqueKey}' referenced by the '{source.GetType()}' model's " +
          $"'{configuration.MemberAccessor.Name}' property could not be found."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Set target attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.AttributeKey.EndsWith("Id", StringComparison.Ordinal)) {
        target.Attributes.SetValue(configuration.AttributeKey, topicReference?.Id.ToString(CultureInfo.InvariantCulture));
      }
      else {
        target.References.SetValue(configuration.AttributeKey, topicReference);
      }

    }

    /*==========================================================================================================================
    | PRIVATE: POPULATE TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source list, will populate a target list based on the configured behavior of the source property.
    /// </summary>
    /// <param name="sourceList">The <see cref="IList{ITopicBindingModel}"/> to pull the binding models from.</param>
    /// <param name="targetList">The target <see cref="IList{Topic}"/> to add the mapped <see cref="Topic"/> objects to.</param>
    private async Task PopulateTargetCollectionAsync(
      IList                     sourceList,
      KeyedTopicCollection      targetList
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Queue up mapping tasks
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue = new List<Task<Topic?>>();

      //Map child binding model to target collection on the target
      foreach (ITopicBindingModel childBindingModel in sourceList) {
        Contract.Assume(childBindingModel.Key);
        if (targetList.Contains(childBindingModel.Key)) {
          taskQueue.Add(MapAsync(childBindingModel, targetList.GetValue(childBindingModel.Key)!));
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