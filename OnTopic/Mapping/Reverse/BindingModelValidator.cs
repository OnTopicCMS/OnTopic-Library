﻿/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using OnTopic.Internal.Reflection;
using OnTopic.Mapping.Annotations;
using OnTopic.Metadata;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping.Reverse {

  /*============================================================================================================================
  | CLASS: BINDING MODEL VALIDATOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The model validator is responsible for identifying issues with tyopic view or binding models by evaluating their types
  ///   against a <see cref="ContentTypeDescriptor"/>
  /// </summary>
  /// <remarks>
  ///   <para>
  ///     Technically, there is no reason for this code not to be in <see cref="ReverseTopicMappingService"/>, which it is
  ///     specific to. The main reason it is separated out into a separate internal class is due to the size. The length is not
  ///     due to the code being especially complicated; it just has detailed documentation and exception messages. If this were
  ///     intended to be used by other classes, it'd be configured as an injectible public dependency; as an internal helper
  ///     class, however, it is adequate as a set of static methods.
  ///   </para>
  ///   <para>
  ///     There may be value to providing a comparable class for <see cref="TopicMappingService"/> in the future. That said, it
  ///     is both easier to validate binding models (since they rely on interfaces, and aren't POCOs) and more critical (since
  ///     errors in mapping don't just result in missing data in the user interface, but potentially corrupt the database). As a
  ///     result, this class is provided to help developers avoid coding errors by validating design time decisions (the
  ///     binding model) against runtime behavior (the type of content type it is being utilized against).
  ///   </para>
  /// </remarks>
  static internal class BindingModelValidator {

    /*==========================================================================================================================
    | PRIVATE FIELDS
    \-------------------------------------------------------------------------------------------------------------------------*/
    static readonly ConcurrentBag<(Type, string)> _modelsValidated = new();

    /*==========================================================================================================================
    | INTERNAL: VALIDATE MODEL
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates the binding model against the associated content type to identify any potential
    ///   mapping errors relative to the schema.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     This helper function is intended to provide reporting to developers about errors in their model. As a result, it
    ///     will exclusively throw exceptions, as opposed to populating validation object for rendering to the view. Because
    ///     it's only evaluating the compiled model, which will not change during the application's life cycle, the <paramref
    ///     name="typeAccessor"/> and <paramref name="contentTypeDescriptor"/> are stored as a <see cref="Tuple"/> in a static
    ///     <see cref="ConcurrentBag{T}"/> once a particular combination has passed validation—that way, this check only needs
    ///     to be executed once for any combination, at least for the current application life cycle.
    ///   </para>
    ///   <para>
    ///     Nested <see cref="Type"/>s from e.g. Nested Topics, relationships, and topic references will not be evaluated until
    ///     they are called from <see cref="IReverseTopicMappingService.MapAsync(ITopicBindingModel)"/> (or one if its
    ///     overloads). This could be problematic since any downstream errors could leave the <see cref="ITopicRepository"/>
    ///     in an invalid state. That said, since this is identifying what are effectively bugs in the <see
    ///     cref="ITopicBindingModel"/> implementation, it is hoped that these will be identified during development, and never
    ///     make it to a production environment. To be safe, however, developers should be cautious when running mapping
    ///     existing <see cref="Topic"/> instances from the <see cref="ITopicRepository"/> when the <see
    ///     cref="ContentTypeDescriptor"/> may not have undergone explicit testing.
    ///   </para>
    /// </remarks>
    /// <param name="typeAccessor">
    ///   The <see cref="TypeAccessor"/> of the binding model to validate.
    /// </param>
    /// <param name="contentTypeDescriptor">
    ///   The <see cref="ContentTypeDescriptor"/> object against which to validate the model.
    /// </param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    static internal void ValidateModel(
      [AllowNull]TypeAccessor typeAccessor,
      [AllowNull]ContentTypeDescriptor contentTypeDescriptor,
      [AllowNull]string attributePrefix = ""
      ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(typeAccessor, nameof(typeAccessor));
      Contract.Requires(contentTypeDescriptor, nameof(contentTypeDescriptor));

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip validation if this type has already been validated for this content type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (_modelsValidated.Contains((typeAccessor.Type, contentTypeDescriptor.Key))) {
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
        ValidateProperty(typeAccessor.Type, property, contentTypeDescriptor, attributePrefix);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Add type, content type to model validation cache so it isn't checked again
      \-----------------------------------------------------------------------------------------------------------------------*/
      _modelsValidated.Add((typeAccessor.Type, contentTypeDescriptor.Key));

      return;

    }

    /*==========================================================================================================================
    | INTERNAL: VALIDATE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates a property of the binding model against the associated content type to identify any
    ///   potential mapping errors relative to the schema.
    /// </summary>
    /// <param name="sourceType">
    ///   The binding model <see cref="Type"/> to validate.
    /// </param>
    /// <param name="propertyAccessor">
    ///   A <see cref="MemberAccessor"/> describing a specific property of the <paramref name="sourceType"/>.
    /// </param>
    /// <param name="contentTypeDescriptor">
    ///   The <see cref="ContentTypeDescriptor"/> object against which to validate the model.
    /// </param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    static internal void ValidateProperty(
      [AllowNull]Type sourceType,
      [AllowNull]MemberAccessor propertyAccessor,
      [AllowNull]ContentTypeDescriptor contentTypeDescriptor,
      [AllowNull]string attributePrefix = ""
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceType, nameof(sourceType));
      Contract.Requires(propertyAccessor, nameof(propertyAccessor));
      Contract.Requires(contentTypeDescriptor, nameof(contentTypeDescriptor));

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = propertyAccessor.Configuration;
      var compositeAttributeKey = configuration.GetCompositeAttributeKey(attributePrefix);
      var attributeDescriptor   = contentTypeDescriptor.AttributeDescriptors.GetValue(compositeAttributeKey);
      var childCollections      = new[] { CollectionType.Children, CollectionType.NestedTopics };
      var relationships         = new[] { CollectionType.Relationship, CollectionType.IncomingRelationship };
      var listType              = typeof(object);

      /*------------------------------------------------------------------------------------------------------------------------
      | Skip properties decorated as [DisableMapping]
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
        var typeAccessor        = TypeAccessorCache.GetTypeAccessor(propertyAccessor.Type);

        ValidateModel(
          typeAccessor,
          contentTypeDescriptor,
          configuration.AttributePrefix
        );
        return;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Define list type (if it's a list)
      \-----------------------------------------------------------------------------------------------------------------------*/
      foreach (var type in propertyAccessor.Type.GetInterfaces()) {
        if (type.IsGenericType && typeof(IList<>) == type.GetGenericTypeDefinition()) {
          //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
          listType = type.GetGenericArguments().Last();
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle children
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.CollectionType is CollectionType.Children) {
        throw new MappingModelValidationException(
          $"The {nameof(ReverseTopicMappingService)} does not support mapping child topics. This property should be " +
          $"removed from the binding model, or otherwise decorated with the {nameof(DisableMappingAttribute)} to prevent " +
          $"it from being evaluated by the {nameof(ReverseTopicMappingService)}. If children must be mapped, then the " +
          $"caller should handle this on a per child basis, where it can better validate the merge logic given the current " +
          $"context of the target topic."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle parent
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.GetCompositeAttributeKey(attributePrefix) is "Parent") {
        throw new MappingModelValidationException(
          $"The {nameof(ReverseTopicMappingService)} does not support mapping Parent topics. This property should be " +
          $"removed from the binding model, or otherwise decorated with the {nameof(DisableMappingAttribute)} to prevent " +
          $"it from being evaluated by the {nameof(ReverseTopicMappingService)}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate attribute type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeDescriptor is null) {
        throw new MappingModelValidationException(
          $"A '{nameof(sourceType)}' object was provided with a content type set to '{contentTypeDescriptor.Key}'. This " +
          $"content type does not contain an attribute named '{compositeAttributeKey}', as requested by the " +
          $"'{propertyAccessor.Name}' property. If this property is not intended to be mapped by the " +
          $"{nameof(ReverseTopicMappingService)}, then it should be decorated with {nameof(DisableMappingAttribute)}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Detect non-mapped relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (attributeDescriptor.ModelType is ModelType.Relationship) {
        ValidateRelationship(sourceType, propertyAccessor, attributeDescriptor, listType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate the correct base class for nested topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        attributeDescriptor.ModelType is ModelType.NestedTopic &&
        !typeof(ITopicBindingModel).IsAssignableFrom(listType)
      ) {
        throw new MappingModelValidationException(
          $"The '{propertyAccessor.Name}' property on the '{sourceType.Name}' class has been determined to be a " +
          $"{configuration.CollectionType}, but the generic type '{listType.Name}' does not implement the " +
          $"{nameof(ITopicBindingModel)} interface. This is required for binding models. If this collection is not intended " +
          $"to be mapped as a {ModelType.NestedTopic} then update the definition in the associated " +
          $"{nameof(ContentTypeDescriptor)}. If this collection is not intended to be mapped at all, include the " +
          $"{nameof(DisableMappingAttribute)} to exclude it from mapping."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate the correct base class for reference
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (
        attributeDescriptor.ModelType is ModelType.Reference &&
        !typeof(IAssociatedTopicBindingModel).IsAssignableFrom(propertyAccessor.Type)
      ) {
        throw new MappingModelValidationException(
          $"The '{propertyAccessor.Name}' property on the '{sourceType.Name}' class has been determined to be a " +
          $"{ModelType.Reference}, but the generic type '{propertyAccessor.Type.Name}' does not implement the " +
          $"{nameof(IAssociatedTopicBindingModel)} interface. This is required for references. If this property is not " +
          $"intended to be mapped as a {ModelType.Reference} then update the definition in the associated " +
          $"{nameof(ContentTypeDescriptor)}. If this property is not intended to be mapped at all, include the " +
          $"{nameof(DisableMappingAttribute)} to exclude it from mapping."
        );
      }

    }

    /*==========================================================================================================================
    | INTERNAL: VALIDATE RELATIONSHIP
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates a relationship property on the binding model against the associated content type to
    ///   identify any potential mapping errors relative to the schema.
    /// </summary>
    /// <param name="sourceType">
    ///   The binding model <see cref="Type"/> to validate.
    /// </param>
    /// <param name="propertyAccessor">
    ///   A <see cref="MemberAccessor"/> describing a specific property of the <paramref name="sourceType"/>.
    /// </param>
    /// <param name="attributeDescriptor">
    ///   The <see cref="AttributeDescriptor"/> object against which to validate the model.
    /// </param>
    /// <param name="listType">The generic <see cref="Type"/> used for the corresponding <see cref="IList{T}"/>.</param>
    static internal void ValidateRelationship(
      [AllowNull]Type                      sourceType,
      [AllowNull]MemberAccessor            propertyAccessor,
      [AllowNull]AttributeDescriptor       attributeDescriptor,
      [DisallowNull]Type                   listType
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(sourceType, nameof(sourceType));
      Contract.Requires(propertyAccessor, nameof(propertyAccessor));
      Contract.Requires(attributeDescriptor, nameof(attributeDescriptor));

      /*------------------------------------------------------------------------------------------------------------------------
      | Define variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = propertyAccessor.Configuration;

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate list
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!typeof(IList).IsAssignableFrom(propertyAccessor.Type)) {
        throw new MappingModelValidationException(
          $"The '{propertyAccessor.Name}' property on the '{sourceType.Name}' class maps to a relationship attribute " +
          $"'{attributeDescriptor.Key}', but does not implement {nameof(IList)}. Relationships must implement " +
          $"{nameof(IList)} or derive from a collection that does."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate relationship type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!new[] { CollectionType.Any, CollectionType.Relationship }.Contains(configuration.CollectionType)) {
        throw new MappingModelValidationException(
          $"The '{propertyAccessor.Name}' property on the '{sourceType.Name}' class maps to a relationship attribute " +
          $"'{attributeDescriptor.Key}', but is configured as a {configuration.CollectionType}. The property should be " +
          $"flagged as either {nameof(CollectionType.Any)} or {nameof(CollectionType.Relationship)}."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate the correct base class for relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!typeof(IAssociatedTopicBindingModel).IsAssignableFrom(listType)) {
        throw new MappingModelValidationException(
          $"The '{propertyAccessor.Name}' property on the '{sourceType.Name}' class has been determined to be a " +
          $"{configuration.CollectionType}, but the generic type '{listType.Name}' does not implement the " +
          $"{nameof(IAssociatedTopicBindingModel)} interface. This is required for binding models. If this collection is not " +
          $"intended to be mapped as a {configuration.CollectionType} then update the definition in the associated " +
          $"{nameof(ContentTypeDescriptor)}. If this collection is not intended to be mapped at all, include the " +
          $"{nameof(DisableMappingAttribute)} to exclude it from mapping."
        );
      }

    }

  } //Class
} //Namespace