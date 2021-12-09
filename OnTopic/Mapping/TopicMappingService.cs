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
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Diagnostics;
using OnTopic.Internal.Reflection;
using OnTopic.Lookup;
using OnTopic.Mapping.Annotations;
using OnTopic.Mapping.Internal;
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
    public async Task<object?> MapAsync(Topic? topic, AssociationTypes associations = AssociationTypes.All) =>
      await MapAsync(topic, associations, new()).ConfigureAwait(false);

    /// <summary>
    ///   Given a topic, will identify any View Models named, by convention, "{ContentType}TopicViewModel" and populate them
    ///   according to the rules of the mapping implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly typed, prefer <see cref="MapAsync{T}(Topic, AssociationTypes)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    ///   <para>
    ///     This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///     recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with
    ///     <see cref="IncludeAttribute"/> set to include <see cref="AssociationTypes.Parents"/>).
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    private async Task<object?> MapAsync(
      Topic?                    topic,
      AssociationTypes          associations,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Lookup type
      \-----------------------------------------------------------------------------------------------------------------------*/
      var viewModelType = _typeLookupService.Lookup($"{topic.ContentType}TopicViewModel", $"{topic.ContentType}ViewModel");

      if (viewModelType is null) {
        throw new InvalidTypeException(
          $"No class named '{topic.ContentType}TopicViewModel' could be located in any loaded assemblies. This is required " +
          $"to map the topic '{topic.GetUniqueKey()}'."
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Perform mapping
      \-----------------------------------------------------------------------------------------------------------------------*/
      return await MapAsync(topic, viewModelType, associations, cache, attributePrefix).ConfigureAwait(false);

    }

    /// <summary>
    ///   Will map a given <paramref name="topic"/> to a given <paramref name="type"/>, according to the rules of the mapping
    ///   implementation.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     Because the class is using reflection to determine the target View Models, the return type is <see cref="Object"/>.
    ///     These results may need to be cast to a specific type, depending on the context. That said, strongly-typed views
    ///     should be able to cast the object to the appropriate View Model type. If the type of the View Model is known
    ///     upfront, and it is imperative that it be strongly-typed, prefer <see cref="MapAsync{T}(Topic, AssociationTypes)"/>.
    ///   </para>
    ///   <para>
    ///     Because the target object is being dynamically constructed, it must implement a default constructor.
    ///   </para>
    ///   <para>
    ///     This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///     recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with
    ///     <see cref="IncludeAttribute"/> set to include <see cref="AssociationTypes.Parents"/>).
    ///   </para>
    /// </remarks>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="type">The <see cref="Type"/> that should be used for the View Model.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
    private async Task<object?> MapAsync(
      Topic?                    topic,
      Type                      type,
      AssociationTypes          associations,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null || type is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      \-----------------------------------------------------------------------------------------------------------------------*/
      var target                = (object?)null;

      if (cache.TryGetValue(topic.Id, type, out var cacheEntry)) {
        target                  = cacheEntry.MappedTopic;
        if (cacheEntry.GetMissingAssociations(associations) == AssociationTypes.None) {
          return target;
        }
        //Call MapAsync() with target object to map missing attributes
        return await MapAsync(topic, target, associations, cache, attributePrefix).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Identify parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(type);
      var constructorInfo       = typeAccessor.GetPrimaryConstructor();
      var parameters            = constructorInfo?.GetParameters()?? Array.Empty<ParameterInfo>();
      var arguments             = new object?[parameters.Length];

      /*------------------------------------------------------------------------------------------------------------------------
      | Pre-cache entry
      >-------------------------------------------------------------------------------------------------------------------------
      | In property mapping, we deal with circular references by returning a cached reference. That isn't practical with
      | circular references in constructor mapping. To help avoid these, we register a pre-cache entry as IsInitializing, but
      | without a mapped object; the TopicMappingCache is expected to throw an exception if an attempt to map that topic to that
      | type occurs again prior to the constructor mapping being completed.
      \-----------------------------------------------------------------------------------------------------------------------*/
      cache.Preregister(topic.Id, type);

      /*------------------------------------------------------------------------------------------------------------------------
      | Set parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      var parameterQueue        = new Dictionary<int, Task<object?>>();

      foreach (var parameter in parameters) {
        parameterQueue.Add(parameter.Position, GetParameterAsync(topic, associations, parameter, cache, attributePrefix));
      }

      await Task.WhenAll(parameterQueue.Values).ConfigureAwait(false);

      foreach (var parameter in parameterQueue) {
        arguments[parameter.Key] = await parameter.Value.ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize object
      \-----------------------------------------------------------------------------------------------------------------------*/
      target = Activator.CreateInstance(type, arguments);

      Contract.Assume(
        target,
        $"The target type '{type}' could not be properly constructed, as required to map the topic '{topic.GetUniqueKey()}'."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache object
      \-----------------------------------------------------------------------------------------------------------------------*/
      cache.Register(topic.Id, associations, target);

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var propertyQueue         = new List<Task>();
      var mappedParameters      = parameters.Select(p => p.Name);

      foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
        if (!mappedParameters.Contains(property.Name, StringComparer.OrdinalIgnoreCase)) {
          propertyQueue.Add(SetPropertyAsync(topic, target, associations, property, cache, attributePrefix, false));
        }
      }

      await Task.WhenAll(propertyQueue.ToArray()).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return target
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

    /*==========================================================================================================================
    | METHOD: MAP (T)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<T?> MapAsync<T>(Topic? topic, AssociationTypes associations = AssociationTypes.All) where T : class {
      if (typeof(Topic).IsAssignableFrom(typeof(T))) {
        return topic as T;
      }
      return (T?)await MapAsync(topic, typeof(T), associations, new()).ConfigureAwait(false);
    }

    /*==========================================================================================================================
    | METHOD: MAP (OBJECTS)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <inheritdoc />
    public async Task<object?> MapAsync(Topic? topic, object target, AssociationTypes associations = AssociationTypes.All) {
      Contract.Requires(target, nameof(target));
      return await MapAsync(topic, target, associations, new()).ConfigureAwait(false);
    }

    /// <summary>
    ///   Given a topic and an instance of a DTO, will populate the DTO according to the default mapping rules.
    /// </summary>
    /// <param name="topic">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <remarks>
    ///   This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
    ///   recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with <see cref
    ///   ="IncludeAttribute"/> set to include <see cref="AssociationTypes.Parents"/>).
    /// </remarks>
    /// <returns>
    ///   The target view model with the properties appropriately mapped.
    /// </returns>
    private async Task<object> MapAsync(
      Topic?                    topic,
      object                    target,
      AssociationTypes          associations,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate input
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topic is null) {
        return target;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (target is Topic) {
        return topic;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle cached objects
      >-------------------------------------------------------------------------------------------------------------------------
      | If the cache contains an entry, check to make sure it includes all of the requested associations. If it does, return it.
      | If it doesn't, determine the missing associations and request to have those mapped.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (cache.TryGetValue(topic.Id, target.GetType(), out var cacheEntry)) {
        associations            = cacheEntry.GetMissingAssociations(associations);
        target                  = cacheEntry.MappedTopic;
        if (associations == AssociationTypes.None) {
          return cacheEntry.MappedTopic;
        }
        cacheEntry.AddMissingAssociations(associations);
      }
      else if (!topic.IsNew) {
        cache.Register(
          topic.Id,
          associations,
          target
        );
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Loop through properties, mapping each one
      \-----------------------------------------------------------------------------------------------------------------------*/
      var taskQueue             = new List<Task>();
      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(target.GetType());

      foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
        taskQueue.Add(SetPropertyAsync(topic, target, associations, property, cache, attributePrefix, cacheEntry is not null));
      }
      await Task.WhenAll(taskQueue.ToArray()).ConfigureAwait(false);

      /*------------------------------------------------------------------------------------------------------------------------
      | Return result
      \-----------------------------------------------------------------------------------------------------------------------*/
      return target;

    }

    /*==========================================================================================================================
    | PRIVATE: GET PARAMETER (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="parameter"/>, retrieves the appropriate value from the corresponding <paramref name="source"/>
    ///   topic, while honoring <paramref name="associations"/>.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="parameter">Information related to the current parameter.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    private async Task<object?> GetParameterAsync(
      Topic source,
      AssociationTypes associations,
      ParameterInfo parameter,
      MappedTopicCache cache,
      string? attributePrefix = null
    ) {

      var configuration = new ItemConfiguration(parameter, parameter.Name, attributePrefix);

      if (configuration.DisableMapping) {
        return parameter.DefaultValue;
      }

      var value = await GetValue(source, parameter.ParameterType, associations, configuration, cache, false).ConfigureAwait(false);

      if (value is null && IsList(parameter.ParameterType)) {
        return await getList(parameter.ParameterType, configuration).ConfigureAwait(false);
      }
      else if (configuration.MapToParent) {
        return await MapAsync(
          source,
          parameter.ParameterType,
          associations,
          cache,
          configuration.AttributePrefix
        ).ConfigureAwait(false);
      }

      return value;

      /*------------------------------------------------------------------------------------------------------------------------
      | Get List Function
      \-----------------------------------------------------------------------------------------------------------------------*/
      async Task<IList?> getList(Type targetType, ItemConfiguration configuration) {

        var sourceList = GetSourceCollection(source, associations, configuration);
        var targetList = InitializeCollection(targetType);

        if (sourceList is null || targetList is null) {
          return (IList?)null;
        }

        await PopulateTargetCollectionAsync(sourceList, targetList, configuration, cache).ConfigureAwait(false);

        return targetList;

      }

    }

    /*==========================================================================================================================
    | PRIVATE: SET PROPERTY (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that evaluates each property on the target object and attempts to retrieve a value from the source
    ///   <see cref="Topic"/> based on predetermined conventions.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="target">The target object to map the data to.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="property">Information related to the current property.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
    /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
    private async Task SetPropertyAsync(
      Topic                     source,
      object                    target,
      AssociationTypes          associations,
      MemberAccessor            property,
      MappedTopicCache          cache,
      string?                   attributePrefix                 = null,
      bool                      mapAssociationsOnly             = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(target, nameof(target));
      Contract.Requires(associations, nameof(associations));
      Contract.Requires(property, nameof(property));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var configuration         = new PropertyConfiguration((PropertyInfo)property.MemberInfo, attributePrefix);

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle by type, attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (configuration.DisableMapping) {
        return;
      }

      var value = await GetValue(source, property.Type, associations, configuration, cache, mapAssociationsOnly).ConfigureAwait(false);

      if (value is null && IsList(property.Type)) {
        await SetCollectionValueAsync(source, target, associations, configuration, cache).ConfigureAwait(false);
      }
      else if (configuration.MapToParent) {
        var targetProperty = property.GetValue(target);
        if (targetProperty is not null) {
          await MapAsync(
            source,
            targetProperty,
            associations,
            cache,
            configuration.AttributePrefix
          ).ConfigureAwait(false);
        }
      }
      else if (value != null && property.CanWrite) {
        property.SetValue(target, value, true);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate fields
      \-----------------------------------------------------------------------------------------------------------------------*/
      configuration.Validate(target);

    }

    /*==========================================================================================================================
    | PRIVATE: GET VALUE (ASYNC)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function that retrieves a value from the source <see cref="Topic"/> based on predetermined conventions.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
    /// <param name="targetType">The <see cref="Type"/> of the target parameter or property.</param>
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="configuration">Information related to the current parameter or property.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
    private async Task<object?> GetValue(
      Topic source,
      Type targetType,
      AssociationTypes associations,
      ItemConfiguration configuration,
      MappedTopicCache cache,
      bool mapAssociationsOnly = false
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(associations, nameof(associations));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish per-property variables
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicReferenceId = source.Attributes.GetInteger($"{configuration.AttributeKey}Id", 0);
      var topicReference = source.References.GetValue(configuration.AttributeKey);

      if (topicReferenceId == 0 && configuration.AttributeKey.EndsWith("Id", StringComparison.OrdinalIgnoreCase)) {
        topicReferenceId = source.Attributes.GetInteger(configuration.AttributeKey, 0);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Assign default value
      \-----------------------------------------------------------------------------------------------------------------------*/
      var value                 = (object?)null;
      if (!mapAssociationsOnly && configuration.DefaultValue is not null) {
        value = configuration.DefaultValue;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle by type, attribute
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (TryGetCompatibleProperty(source, targetType, configuration, out var compatibleValue)) {
        value = compatibleValue;
      }
      else if (!mapAssociationsOnly && AttributeValueConverter.IsConvertible(targetType)) {
        value = GetScalarValue(source, configuration);
      }
      else if (IsList(targetType)) {
        return null;
      }
      else if (configuration.AttributeKey is "Parent" && associations.HasFlag(AssociationTypes.Parents)) {
        if (source.Parent is not null) {
          value = await GetTopicReferenceAsync(source.Parent, targetType, configuration, cache).ConfigureAwait(false);
        }
      }
      else if (
        topicReference is not null &&
        associations.HasFlag(AssociationTypes.References)
      ) {
        value = await GetTopicReferenceAsync(topicReference, targetType, configuration, cache).ConfigureAwait(false);
      }
      else if (topicReferenceId > 0 && associations.HasFlag(AssociationTypes.References)) {
        topicReference = _topicRepository.Load(topicReferenceId, source);
        if (topicReference is not null) {
          value = await GetTopicReferenceAsync(topicReference, targetType, configuration, cache).ConfigureAwait(false);
        }
      }
      else if (configuration.MapToParent) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return value;

    }

    /*==========================================================================================================================
    | PRIVATE: GET SCALAR VALUE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a scalar property from a <see cref="Topic"/>.
    /// </summary>
    /// <remarks>
    ///   The <see cref="GetScalarValue(Topic, ItemConfiguration)"/> method will attempt to retrieve the value from the
    ///   <paramref name="source"/> based on, in order, the <paramref name="source"/>'s <c>Get{Property}()</c> method, <c>
    ///   {Property}</c> property, and, finally, its <see cref="Topic.Attributes"/> collection (using <see cref="
    ///   TrackedRecordCollection{TItem, TValue, TAttribute}.GetValue(String, Boolean)"/>).
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="configuration">The <see cref="PropertyConfiguration"/> with details about the property's attributes.</param>
    /// <autogeneratedoc />
    private static object? GetScalarValue(Topic source, ItemConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.Get{Property}()
      \-----------------------------------------------------------------------------------------------------------------------*/
      var typeAccessor          = TypeAccessorCache.GetTypeAccessor(source.GetType());
      var attributeValue        = typeAccessor.GetMethodValue(source, $"Get{configuration.AttributeKey}")?.ToString();

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.{Property}
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (String.IsNullOrEmpty(attributeValue)) {
        attributeValue = typeAccessor.GetPropertyValue(source, configuration.AttributeKey)?.ToString();
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
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      return attributeValue;

    }

    /*==========================================================================================================================
    | PRIVATE: IS LIST?
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a type, determines whether it's a list that is recognized by the <see cref="TopicMappingService"/>.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     To qualify, the <paramref name="targetType"/> must either implement <see cref="IList"/>, or it must be of type <see
    ///     cref="IEnumerable{T}"/>, <see cref="ICollection{T}"/>, or <see cref="IList{T}"/>—any of which, if null, will be
    ///     instantiated as a new <see cref="List{T}"/>.
    ///   </para>
    ///   <para>
    ///     It is technically possible for the <paramref name="targetType"/> to implement one of the interfaces, such as <see
    ///     cref="IList{T}"/>, while the assigned reference type is not compatible with the <see cref="IList"/> interface
    ///     required by e.g. <see cref="PopulateTargetCollectionAsync(IList{Topic}, IList, ItemConfiguration, MappedTopicCache)"
    ///     />. Detecting this requires looping through the interface implementations which is comparatively more costly given
    ///     the number of times <see cref="IsList(Type)"/> gets called. In practice, collections that implement e.g. <see cref="
    ///     IList{T}"/> are expected to also support <see cref="IList"/>. If they don't, however, the mapping will throw an
    ///     exception since the assigned value will not be castable to an <see cref="IList"/>.
    ///   </para>
    /// </remarks>
    /// <param name="targetType">The <see cref="Type"/> of collection to initialize.</param>
    private static bool IsList(Type targetType) =>
      typeof(IList).IsAssignableFrom(targetType) ||
      targetType.IsGenericType &&
      (
        targetType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
        targetType.GetGenericTypeDefinition() == typeof(ICollection<>) ||
        targetType.GetGenericTypeDefinition() == typeof(IList<>)
      );

    /*==========================================================================================================================
    | PRIVATE: INITIALIZE COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a collection type, attempts to initialize a compatible type.
    /// </summary>
    /// <param name="targetType">The <see cref="Type"/> of collection to initialize.</param>
    private static IList? InitializeCollection(Type targetType) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(targetType, nameof(targetType));

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!IsList(targetType)) {
        return (IList?)null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to create specific type
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!targetType.IsInterface && !targetType.IsAbstract) {
        return (IList?)Activator.CreateInstance(targetType);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle types that don't implement IList or IEnumerable
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!targetType.IsGenericType) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to create generic list
      \-----------------------------------------------------------------------------------------------------------------------*/
      var parameters            = targetType.GetGenericArguments();

      if (parameters.Length != 1) {
        return null;
      }

      var genericType           = typeof(List<>);
      var concreteType          = genericType.MakeGenericType(parameters);

      return (IList?)Activator.CreateInstance(concreteType);

    }

    /*==========================================================================================================================
    | PRIVATE: SET COLLECTION VALUE
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
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="configuration">
    ///   The <see cref="PropertyConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    private async Task SetCollectionValueAsync(
      Topic                     source,
      object                    target,
      AssociationTypes          associations,
      PropertyConfiguration     configuration,
      MappedTopicCache          cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(associations, nameof(associations));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (!IsList(configuration.Property.PropertyType)) return;

      /*------------------------------------------------------------------------------------------------------------------------
      | Ensure target list is created
      \-----------------------------------------------------------------------------------------------------------------------*/
      var targetList = (IList?)configuration.Property.GetValue(target, null);
      if (targetList is null) {
        targetList = InitializeCollection(configuration.Property.PropertyType);
        configuration.Property.SetValue(target, targetList);
      }

      Contract.Assume(
        targetList,
        $"The target list type, '{configuration.Property.PropertyType}', could not be properly constructed, as required to " +
        $"map the '{configuration.Property.Name}' property on the '{target?.GetType().Name}' object."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceList = GetSourceCollection(source, associations, configuration);

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
    | PRIVATE: GET SOURCE COLLECTION
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
    /// <param name="associations">Determines what associations the mapping should include, if any.</param>
    /// <param name="configuration">
    ///   The <see cref="ItemConfiguration"/> with details about the property's attributes.
    /// </param>
    private IList<Topic> GetSourceCollection(Topic source, AssociationTypes associations, ItemConfiguration configuration) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(associations, nameof(associations));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Establish source collection to store topics to be mapped
      \-----------------------------------------------------------------------------------------------------------------------*/
      var                       listSource                      = (IList<Topic>)Array.Empty<Topic>();
      var                       collectionKey                   = configuration.CollectionKey;
      var                       collectionType                  = configuration.CollectionType;

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle children
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = getCollection(
        CollectionType.Children,
        s => true,
        () => source.Children.ToList()
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (outgoing) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = getCollection(
        CollectionType.Relationship,
        source.Relationships.Contains,
        () => source.Relationships.GetValues(collectionKey)
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle nested topics, or children corresponding to the property name
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = getCollection(
        CollectionType.NestedTopics,
        source.Children.Contains,
        () => source.Children[collectionKey].Children
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle (incoming) relationships
      \-----------------------------------------------------------------------------------------------------------------------*/
      listSource = getCollection(
        CollectionType.IncomingRelationship,
        source.IncomingRelationships.Contains,
        () => source.IncomingRelationships.GetValues(collectionKey)
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle other strongly typed source collections
      \-----------------------------------------------------------------------------------------------------------------------*/
      //The following allows a target collection to be mapped to an IList<Topic> source collection. This is valuable for custom,
      //curated collections defined on e.g. derivatives of Topic, but which don't otherwise map to a specific collection type.
      //For example, the ContentTypeDescriptor's AttributeDescriptors collection, which provides a rollup of
      //AttributeDescriptors from the current ContentTypeDescriptor, as well as all of its ascendents.
      if (listSource.Count == 0) {
        var sourceProperty = TypeAccessorCache.GetTypeAccessor(source.GetType()).GetMember(configuration.AttributeKey);
        if (
          sourceProperty?.GetValue(source) is IList sourcePropertyValue &&
          sourcePropertyValue.Count > 0 &&
          sourcePropertyValue[0] is Topic
        ) {
          listSource = getCollection(
            CollectionType.MappedCollection,
            s => true,
            () => sourcePropertyValue.Cast<Topic>().ToList()
          );
        }
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle Metadata relationship
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (listSource.Count == 0 && !String.IsNullOrWhiteSpace(configuration.MetadataKey)) {
        var metadataKey = $"Root:Configuration:Metadata:{configuration.MetadataKey}:LookupList";
        var metadataParent = _topicRepository.Load(metadataKey, source);
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
      | Provide local function for evaluating current collection
      \-----------------------------------------------------------------------------------------------------------------------*/
      IList<Topic> getCollection(CollectionType collection, Func<string, bool> contains, Func<IList<Topic>> getTopics) {
        var targetAssociations = AssociationMap.Mappings[collection];
        var preconditionsMet    =
          listSource.Count == 0 &&
          (collectionType is CollectionType.Any || collectionType.Equals(collection)) &&
          (collectionType is CollectionType.Children || collection is not CollectionType.Children) &&
          (targetAssociations is AssociationTypes.None || associations.HasFlag(targetAssociations)) &&
          contains(configuration.CollectionKey);
        return preconditionsMet? getTopics() : listSource;
      }

    }

    /*==========================================================================================================================
    | PRIVATE: POPULATE TARGET COLLECTION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a source list, will populate a target list based on the configured behavior of the target property.
    /// </summary>
    /// <param name="sourceList">The <see cref="IList{Topic}"/> to pull the source <see cref="Topic"/> objects from.</param>
    /// <param name="targetList">The target <see cref="IList"/> to add the mapped <see cref="Topic"/> objects to.</param>
    /// <param name="configuration">
    ///   The <see cref="ItemConfiguration"/> with details about the property's attributes.
    /// </param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    private async Task PopulateTargetCollectionAsync(
      IList<Topic>              sourceList,
      IList                     targetList,
      ItemConfiguration         configuration,
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
      foreach (var type in targetList.GetType().GetInterfaces()) {
        if (type.IsGenericType && typeof(IList<>) == type.GetGenericTypeDefinition()) {
          //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
          listType = type.GetGenericArguments().Last();
        }
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

        //Ensure the source topic isn't disabled; disabled topics should never be returned to the presentation layer unless
        //explicitly requested by a top-level request.
        if (childTopic.IsDisabled) {
          continue;
        }

        //Map child topic to target DTO
        var childDto            = (object)childTopic;
        if (!typeof(Topic).IsAssignableFrom(listType)) {
          var mappingType       = GetValidatedMappingType(configuration.MapAs, listType)?? GetValidatedMappingType(childTopic, listType);
          if (mappingType is not null) {
            taskQueue.Add(MapAsync(childTopic, mappingType, configuration.IncludeAssociations, cache));
          }
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
        if (dto is not null) {
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
    | PRIVATE: GET VALIDATED MODEL TYPE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Given a <paramref name="sourceTopic"/> <see cref="Topic"/>, identifies the target model type associated with it and
    ///   validates it against the <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="sourceTopic">The source <see cref="Topic"/> that the target type should be inferred from.</param>
    /// <param name="expectedType">The expected <see cref="Type"/> that the inferred type must be compatible with.</param>
    /// <returns>The inferred target type, if valid, otherwise null.</returns>
    private Type? GetValidatedMappingType(Topic sourceTopic, Type expectedType) =>
      GetValidatedMappingType(
        _typeLookupService.Lookup($"{sourceTopic.ContentType}TopicViewModel", $"{sourceTopic.ContentType}ViewModel"),
        expectedType
      );

    /// <summary>
    ///   Given a <paramref name="mappingType"/> <see cref="Type"/>, validates it against the <paramref name="expectedType"/>.
    ///   If it is compatible, returns the <paramref name="mappingType"/>. Otherwise, returns null.
    /// </summary>
    /// <param name="mappingType">The source <see cref="Type"/> that should be validated.</param>
    /// <param name="expectedType">
    ///   The expected <see cref="Type"/> that the <paramref name="mappingType"/> must be compatible with.
    /// </param>
    /// <returns>The <paramref name="mappingType"/>, if valid, otherwise null.</returns>
    private static Type? GetValidatedMappingType(Type? mappingType, Type expectedType) =>
      expectedType.IsAssignableFrom(mappingType)? mappingType : null;

    /*==========================================================================================================================
    | PRIVATE: GET TOPIC REFERENCE
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Attempts to retrieve a topic reference form the <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="targetType">The <see cref="Type"/> expected for the mapped <paramref name="source"/>.</param>
    /// <param name="configuration">The <see cref="ItemConfiguration"/> with details about the item's attributes.</param>
    /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
    private async Task<object?> GetTopicReferenceAsync(
      Topic source,
      Type targetType,
      ItemConfiguration configuration,
      MappedTopicCache cache
    ) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(targetType, nameof(targetType));
      Contract.Requires(configuration, nameof(configuration));
      Contract.Requires(cache, nameof(cache));

      /*------------------------------------------------------------------------------------------------------------------------
      | Bypass disabled topics
      \-----------------------------------------------------------------------------------------------------------------------*/
      //Ensure the source topic isn't disabled; disabled topics should never be returned to the presentation layer unless
      //explicitly requested by a top-level request.
      if (source.IsDisabled) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Map referenced topic
      \-----------------------------------------------------------------------------------------------------------------------*/
      var topicDto              = (object?)null;
      var mappingType           = GetValidatedMappingType(configuration.MapAs, targetType)?? GetValidatedMappingType(source, targetType);

      if (mappingType is not null) {
        topicDto = await MapAsync(source, mappingType, configuration.IncludeAssociations, cache).ConfigureAwait(false);
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate results
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (topicDto is null) {
        return null;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return type
      \-----------------------------------------------------------------------------------------------------------------------*/
      return topicDto;

    }

    /*==========================================================================================================================
    | PRIVATE: FLATTEN TOPIC GRAPH
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Helper function recursively iterates through children and adds each to a collection.
    /// </summary>
    /// <param name="source">The <see cref="Topic"/> entity pull the data from.</param>
    /// <param name="targetList">The list of <see cref="Topic"/> instances to add each child to.</param>
    private IList<Topic> FlattenTopicGraph(Topic source, IList<Topic> targetList) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(targetList, nameof(targetList));

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate source properties
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (source.IsDisabled) return targetList;
      if (source.ContentType is "List") return targetList;

      /*------------------------------------------------------------------------------------------------------------------------
      | Merge source list into target list
      \-----------------------------------------------------------------------------------------------------------------------*/
      targetList.Add(source);
      source.Children.ToList().ForEach(t => FlattenTopicGraph(t, targetList));
      return targetList;

    }

    /*==========================================================================================================================
    | PRIVATE: TRY GET COMPATIBLE PROPERTY
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Gets a property on the <paramref name="source"/> that is compatible to the <paramref name="targetType"/>.
    /// </summary>
    /// <remarks>
    ///   Even if the property values can't be set by the <see cref="TypeAccessor"/>, properties should be settable assuming the
    ///   source and target types are compatible. In this case, <see cref="TopicMappingService"/> needn't know anything about
    ///   the property type as it doesn't need to do a conversion; it can just do a one-to-one mapping.
    /// </remarks>
    /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
    /// <param name="targetType">The target <see cref="Type"/>.</param>
    /// <param name="configuration">The <see cref="ItemConfiguration"/> with details about the item's attributes.</param>
    /// <param name="value">The compatible property, if it is available.</param>
    private static bool TryGetCompatibleProperty(Topic source, Type targetType, ItemConfiguration configuration, out object? value) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Validate parameters
      \-----------------------------------------------------------------------------------------------------------------------*/
      Contract.Requires(source, nameof(source));
      Contract.Requires(targetType, nameof(targetType));
      Contract.Requires(configuration, nameof(configuration));

      /*------------------------------------------------------------------------------------------------------------------------
      | Attempt to retrieve value from topic.{Property}
      \-----------------------------------------------------------------------------------------------------------------------*/
      var sourceProperty = TypeAccessorCache.GetTypeAccessor(source.GetType()).GetMember(configuration.AttributeKey);

      /*------------------------------------------------------------------------------------------------------------------------
      | Escape clause if preconditions are not met
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (sourceProperty is null || !targetType.IsAssignableFrom(sourceProperty.Type)) {
        value = null;
        return false;
      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Return value
      \-----------------------------------------------------------------------------------------------------------------------*/
      value = sourceProperty.GetValue(source);

      return true;

    }

  } //Class
} //Namespace