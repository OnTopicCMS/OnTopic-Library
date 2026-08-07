/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using OnTopic.Collections;
using OnTopic.Internal.Reflection;
using OnTopic.Metadata;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Reverse;

/*==============================================================================================================================
| CLASS: REVERSE TOPIC MAPPING SERVICE
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <inheritdoc />
public class ReverseTopicMappingService : IReverseTopicMappingService {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      ITopicRepository                _topicRepository;

  /*============================================================================================================================
  | CONSTRUCTOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Establishes a new instance of a <see cref="ReverseTopicMappingService"/> with required dependencies.
  /// </summary>
  public ReverseTopicMappingService(ITopicRepository topicRepository) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(topicRepository, "An instance of an ITopicRepository is required.");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set dependencies
    \-------------------------------------------------------------------------------------------------------------------------*/
    _topicRepository            = topicRepository;

  }

  /*============================================================================================================================
  | METHOD: MAP (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<Topic?> MapAsync(ITopicBindingModel source) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle null source
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source is null) return  null;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(source.Key, $"The 'source' ITopicBindingModel must contain a 'Key' value.");
    Contract.Requires(source.ContentType, $"The 'source' ITopicBindingModel must contain a 'ContentType' value.");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Instantiate target
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topic                   = TopicFactory.Create(source.Key, source.ContentType);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide mapping
    \-------------------------------------------------------------------------------------------------------------------------*/
    return await MapAsync(source, topic).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHOD: MAP (T)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<T?> MapAsync<T>(ITopicBindingModel? source) where T : Topic {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle null source
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source is null) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(source.Key, $"The 'source' ITopicBindingModel must contain a 'Key' value.");
    Contract.Requires(source.ContentType, $"The 'source' ITopicBindingModel must contain a 'ContentType' value.");

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map source
    \-------------------------------------------------------------------------------------------------------------------------*/
    return (T?)await MapAsync(
      source,
      TopicFactory.Create(source.Key, source.ContentType)
    ).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | METHOD: MAP (TOPIC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<Topic?> MapAsync(ITopicBindingModel? source, Topic target) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle null source
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source is null) return  target;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    Contract.Requires(target, nameof(target));
    Contract.Assume(source.ContentType, nameof(source.ContentType));

    //Ensure the content type is valid
    if (!GetContentTypeDescriptors().Contains(source.ContentType)) {
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

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map source to target
    \-------------------------------------------------------------------------------------------------------------------------*/
    return await MapAsync(source, target, null).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | PRIVATE: GET CONTENT TYPE DESCRIPTORS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Retrieves the <see cref="ContentTypeDescriptorCollection"/> from the <see cref="ITopicRepository"/>.
  /// </summary>
  /// <remarks>
  ///   Called per-use rather than cached into a field, since content types can be added after this service is constructed, and
  ///   a local cache would silently exclude any updates for the life of the service. Further, <see cref="TopicRepository"/>,
  ///   the base class for every production <see cref="ITopicRepository"/> in this library, already caches the result after the
  ///   first call and maintains the live collection in place (e.g. <c>Delete</c> refreshes it), so the per-call access here is
  ///   expected to be cheap, acknowledging that's a property of that base class, not a guarantee of the <see cref=
  ///   "ITopicRepository"/> interface itself.
  /// </remarks>
  private ContentTypeDescriptorCollection GetContentTypeDescriptors() {
    var contentTypeDescriptors  = _topicRepository.GetContentTypeDescriptors();
    Contract.Assume(
      contentTypeDescriptors,
      $"The {nameof(ITopicRepository.GetContentTypeDescriptors)}() method returned null. This could indicate a corrupt " +
      $"data source."
    );
    return contentTypeDescriptors;
  }

  /*============================================================================================================================
  | PRIVATE: MAP (TOPIC)
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  /// <remarks>
  ///   Properties are mapped sequentially, in source order, rather than concurrently; this avoids concurrent mutation of the
  ///   association collections (<see cref="Topic.Relationships"/>, <see cref="Topic.References"/>, etc.), which aren't thread
  ///   safe, and which individual property mappers write to on the shared <paramref name="target"/>. As a result, an exception
  ///   thrown while mapping one property surfaces immediately, without waiting for or aggregating exceptions from subsequent
  ///   properties, and any properties mapped before the failure remain applied to <paramref name="target"/>.
  /// </remarks>
  private async Task<Topic?> MapAsync(object? source, Topic target, string? attributePrefix) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle null source
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source is null) return  target;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Warm extended attributes
    >---------------------------------------------------------------------------------------------------------------------------
    | Without this, TrackedRecordCollection.SetValue() potentially runs against an unloaded extended attributes, and thus marks
    | attributes as dirty even if they're unchanged, causing needless version rows on save.
    \-------------------------------------------------------------------------------------------------------------------------*/
    await ((ITopicLazyLoadable)target).EnsureLoaded(TopicPayload.ExtendedAttributes).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate model
    \-------------------------------------------------------------------------------------------------------------------------*/
    var typeAccessor            = TypeAccessorCache.GetTypeAccessor(source.GetType());
    var contentTypeDescriptor   = GetContentTypeDescriptors().GetValue(target.ContentType);

    BindingModelValidator.ValidateModel(typeAccessor, contentTypeDescriptor, attributePrefix);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Loop through properties, mapping each one
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
      await SetPropertyAsync(source, target, property, attributePrefix).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return result
    \-------------------------------------------------------------------------------------------------------------------------*/
    return target;

  }

  /*============================================================================================================================
  | PRIVATE: SET PROPERTY (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Helper function that evaluates each property on the source <see cref="ITopicBindingModel"/> and then attempts to
  ///   locate and set the associated attribute, collection, or property on the target <see cref="Topic"/> based on
  ///   predetermined conventions.
  /// </summary>
  /// <param name="source">
  ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
  /// </param>
  /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
  /// <param name="memberAccessor">Information related to the current property.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  private async Task SetPropertyAsync(
    object                      source,
    Topic                       target,
    MemberAccessor              memberAccessor,
    string?                     attributePrefix                 = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish per-property variables
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = memberAccessor.Configuration;
    var contentTypeDescriptor   = GetContentTypeDescriptors().GetValue(target.ContentType);
    var compositeAttributeKey   = configuration.GetCompositeAttributeKey(attributePrefix);

    Contract.Assume(contentTypeDescriptor, nameof(contentTypeDescriptor));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Skip properties decorated with [DisableMapping]
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.DisableMapping) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Skip properties injected by the compiler for record types
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (memberAccessor.Name is  "EqualityContract") {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle mapping properties from referenced objects
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.MapToParent) {
      await MapAsync(
        memberAccessor.GetValue(source),
        target,
        configuration.AttributePrefix
      ).ConfigureAwait(false);
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Retrieve attribute descriptor
    \-------------------------------------------------------------------------------------------------------------------------*/
    var attributeType           = contentTypeDescriptor.AttributeDescriptors.GetValue(compositeAttributeKey);

    if (attributeType is null)  {
      throw new MappingModelValidationException(
        $"The attribute '{configuration.GetCompositeAttributeKey(attributePrefix)}' mapped by the {source.GetType()} could not be found on the " +
        $"'{contentTypeDescriptor.Key}' content type.");
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate fields
    \-------------------------------------------------------------------------------------------------------------------------*/
    memberAccessor.Validate(source);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle property by type
    \-------------------------------------------------------------------------------------------------------------------------*/
    switch (attributeType.ModelType) {
      case ModelType.ScalarValue:
        SetScalarValue(source,  target, memberAccessor, attributePrefix);
        return;
      case ModelType.Relationship:
        await SetRelationships(source, target, memberAccessor, attributePrefix).ConfigureAwait(false);
        return;
      case ModelType.NestedTopic:
        await SetNestedTopicsAsync(source, target, memberAccessor, attributePrefix).ConfigureAwait(false);
        return;
      case ModelType.Reference:
        await SetReference(source, target, memberAccessor, attributePrefix).ConfigureAwait(false);
        return;
    }

  }

  /*============================================================================================================================
  | PRIVATE: SET SCALAR VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Sets an attribute on the target <see cref="Topic"/> with a scalar value from the source binding model.
  /// </summary>
  /// <remarks>
  ///   Assuming the <paramref name="memberAccessor"/>'s <see cref="ItemMetadata.Type"/> property is of the type <see cref=
  ///   "String"/>, <see cref="Boolean"/>, <see cref="Int32"/>, or <see cref="DateTime"/>, the <see cref="SetScalarValue(
  ///   Object, Topic, MemberAccessor, String?)"/> method will attempt to set the property on the <paramref name="target"/>.
  ///   If the value is not set on the <paramref name="source"/> then the <see cref="DefaultValueAttribute"/> will be
  ///   evaluated as a fallback. If the property is not of a settable type then the property is not set. If the value is
  ///   empty, then it will be treated as <c>null</c> in the <paramref name="target"/>'s <see cref="AttributeCollection"/>.
  /// </remarks>
  /// <param name="source">
  ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
  /// </param>
  /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
  /// <param name="memberAccessor">The <see cref="MemberAccessor"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <autogeneratedoc />
  private static void SetScalarValue(
    object                      source,
    Topic                       target,
    MemberAccessor              memberAccessor,
    string?                     attributePrefix
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to retrieve value from the binding model property
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = memberAccessor.Configuration;
    var attributeValue          = memberAccessor.GetValue(source)?.ToString();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Fall back to default, if configured
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (String.IsNullOrEmpty(attributeValue) && configuration.DefaultValue is not null) {
      attributeValue            = configuration.DefaultValue.ToString();
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle type conversion
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (attributeValue is not null && memberAccessor.Type.Name is nameof(Boolean)) {
      attributeValue            = attributeValue is "True" ? "1" : "0";
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set the value (to null, if appropriate)
    \-------------------------------------------------------------------------------------------------------------------------*/
    target.Attributes.SetValue(configuration.GetCompositeAttributeKey(attributePrefix), attributeValue);

  }

  /*============================================================================================================================
  | PRIVATE: SET RELATIONSHIPS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a relationship property, identifies the target <see cref="Topic"/> for each related item, and sets it on the
  ///   source <see cref="Topic"/>'s <see cref="Topic.Relationships"/> collection.
  /// </summary>
  /// <param name="source">
  ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
  /// </param>
  /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
  /// <param name="memberAccessor">The <see cref="MemberAccessor"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  private async Task SetRelationships(
    object                      source,
    Topic                       target,
    MemberAccessor              memberAccessor,
    string?                     attributePrefix
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = memberAccessor.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Retrieve source list
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourceList              = (IList?)memberAccessor.GetValue(source);

    sourceList                  ??= new List<IAssociatedTopicBindingModel>();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Clear existing relationships
    \-------------------------------------------------------------------------------------------------------------------------*/
    target.Relationships.Clear(configuration.GetCompositeAttributeKey(attributePrefix));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set relationships for each
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (IAssociatedTopicBindingModel relationship in sourceList) {
      var targetTopic           = await _topicRepository.Load(relationship.UniqueKey, target).ConfigureAwait(false);
      if (targetTopic is null)  {
        throw new MappingModelValidationException(
          $"The relationship '{relationship.UniqueKey}' mapped in the '{memberAccessor.Name}' property could not be " +
          $"located in the repository."
        );
      }
      target.Relationships.SetValue(configuration.GetCompositeAttributeKey(attributePrefix), targetTopic);
    }

  }

  /*============================================================================================================================
  | PRIVATE: SET NESTED TOPICS
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a nested topic property, serializes a topic for each property, and sets it on the target <see cref="Topic"/>'s
  ///   <see cref="Topic.Children"/> collection.
  /// </summary>
  /// <param name="source">
  ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
  /// </param>
  /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
  /// <param name="memberAccessor">The <see cref="MemberAccessor"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  private async Task SetNestedTopicsAsync(
    object                      source,
    Topic                       target,
    MemberAccessor              memberAccessor,
    string?                     attributePrefix
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = memberAccessor.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Retrieve source list
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourceList              = (IList?)memberAccessor.GetValue(source) ?? new List<ITopicBindingModel>();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Warm target's children
    >---------------------------------------------------------------------------------------------------------------------------
    | Replaces the Children getter's synchronous autoload with an explicit, asynchronous warm-up prior to the below probe
    \-------------------------------------------------------------------------------------------------------------------------*/
    await ((ITopicLazyLoadable)target).EnsureLoaded(TopicPayload.Children).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish target collection to store mapped topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    var container               = target.Children.GetValue(configuration.GetCompositeAttributeKey(attributePrefix));
    if (container is null) {
      container                 = TopicFactory.Create(configuration.GetCompositeAttributeKey(attributePrefix), "List", target);
      container.IsHidden        = true;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Warm container's children
    >---------------------------------------------------------------------------------------------------------------------------
    | The container can be NotLoaded even when target is loaded; PopulateTargetCollectionAsync()'s Contains() check for existing
    | children as well as it's check for orphans require the complete set
    \-------------------------------------------------------------------------------------------------------------------------*/
    await ((ITopicLazyLoadable)container).EnsureLoaded(TopicPayload.Children).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map the topics from the source collection, and add them to the target collection
    \-------------------------------------------------------------------------------------------------------------------------*/
    await PopulateTargetCollectionAsync(sourceList, container.Children).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | PRIVATE: SET REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a reference property, lookup the associated topic and set its <see cref="Topic.Id"/> on the <paramref
  ///   name="target"/>'s <see cref="Topic.Attributes"/> collection.
  /// </summary>
  /// <param name="source">
  ///   The binding model from which to derive the data. Must inherit from <see cref="ITopicBindingModel"/>.
  /// </param>
  /// <param name="target">The <see cref="Topic"/> entity to map the data to.</param>
  /// <param name="memberAccessor">The <see cref="MemberAccessor"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  private async Task SetReference(
    object                      source,
    Topic                       target,
    MemberAccessor              memberAccessor,
    string?                     attributePrefix
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = memberAccessor.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Retrieve source value
    \-------------------------------------------------------------------------------------------------------------------------*/
    var modelReference          = (IAssociatedTopicBindingModel?)memberAccessor.GetValue(source);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide error handling
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (modelReference is null  || modelReference.UniqueKey is null) {
      throw new MappingModelValidationException(
        $"The {memberAccessor.Name} property must reference an object with its `UniqueKey` property set The " +
        $"value may be empty, but it should not be null."
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify target value
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicReference          = await _topicRepository.Load(modelReference.UniqueKey, target).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide error handling
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (modelReference.UniqueKey.Length > 0 && topicReference is null) {
      throw new MappingModelValidationException(
        $"The topic '{modelReference.UniqueKey}' referenced by the '{source.GetType()}' model's " +
        $"'{memberAccessor.Name}' property could not be found."
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Set target attribute
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.GetCompositeAttributeKey(attributePrefix).EndsWith("Id", StringComparison.Ordinal)) {
      target.Attributes.SetValue(configuration.GetCompositeAttributeKey(attributePrefix), topicReference?.Id.ToString(CultureInfo.InvariantCulture));
    }
    else {
      target.References.SetValue(configuration.GetCompositeAttributeKey(attributePrefix), topicReference);
    }

  }

  /*============================================================================================================================
  | PRIVATE: POPULATE TARGET COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a source list, will populate a target list based on the configured behavior of the source property.
  /// </summary>
  /// <param name="sourceList">The <see cref="IList{ITopicBindingModel}"/> to pull the binding models from.</param>
  /// <param name="targetList">The target <see cref="IList{Topic}"/> to add the mapped <see cref="Topic"/> objects to.</param>
  /// <remarks>
  ///   Children are mapped and added sequentially, in <paramref name="sourceList"/> order, rather than concurrently; this
  ///   avoids concurrent mutation on the shared target <see cref="Topic"/> and guarantees <paramref name="targetList"/>'s
  ///   resulting order matches the binding model, instead of varying with completion order.
  /// </remarks>
  private async Task PopulateTargetCollectionAsync(
    IList                       sourceList,
    KeyedTopicCollection        targetList
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Remove orphaned topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (var childTopic in targetList.ToArray()) {
      if (sourceList.Cast<ITopicBindingModel>().Any(model => model.Key == childTopic.Key)) {
        continue;
      }
      targetList.Remove(childTopic);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map and add children in source order
    >---------------------------------------------------------------------------------------------------------------------------
    | Sequential by design: concurrent MapAsync() calls would mutate non-thread-safe collections on the shared target Topic in
    | parallel, and completion-order nondeterminism would make targetList's resulting order unpredictable.
    \-------------------------------------------------------------------------------------------------------------------------*/
    foreach (ITopicBindingModel childBindingModel in sourceList) {

      Contract.Assume(childBindingModel.Key);

      var topic                 = targetList.Contains(childBindingModel.Key)
        ? await MapAsync(childBindingModel, targetList.GetValue(childBindingModel.Key)!).ConfigureAwait(false)
        : await MapAsync(childBindingModel).ConfigureAwait(false);

      if (topic is not null && !targetList.Contains(topic.Key)) {
        targetList.Add(topic);
      }

    }

  }

} //Class