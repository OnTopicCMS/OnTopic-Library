/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Topics Library
\=============================================================================================================================*/
using System.Collections;
using System.Reflection;
using OnTopic.Collections.Specialized;
using OnTopic.Internal.Reflection;
using OnTopic.Lookup;
using OnTopic.Mapping.Annotations;
using OnTopic.Mapping.Internal;
using OnTopic.Models;
using OnTopic.Repositories;

namespace OnTopic.Mapping;

/*==============================================================================================================================
| CLASS: TOPIC MAPPING SERVICE
\-----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
///   Provides a concrete implementation of the <see cref="ITopicMappingService"/> for mapping <see cref="Topic"/> instances
///   to data transfer objects (such as view models) based on set conventions and attribute-based hints.
/// </summary>
/// <param name="topicRepository">
///   An instance of an <see cref="ITopicRepository"/> for looking up associated topics during mapping.
/// </param>
/// <param name="typeLookupService">
///   An instance of an <see cref="ITypeLookupService"/> for resolving view model types by name.
/// </param>
public class TopicMappingService(ITopicRepository topicRepository, ITypeLookupService typeLookupService) : ITopicMappingService {

  /*============================================================================================================================
  | PRIVATE VARIABLES
  \---------------------------------------------------------------------------------------------------------------------------*/
  readonly                      ITopicRepository                _topicRepository                = Contract.Requires(topicRepository);
  readonly                      ITypeLookupService              _typeLookupService              = Contract.Requires(typeLookupService);
  static readonly               TypeAccessor                    _topicTypeAccessor              = TypeAccessorCache.GetTypeAccessor<Topic>();

  /*============================================================================================================================
  | METHOD: MAP (DYNAMIC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  [return: NotNullIfNotNull(nameof(topic))]
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
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
  private async Task<object?>   MapAsync(
    Topic?                      topic,
    AssociationTypes            associations,
    MappedTopicCache            cache,
    string?                     attributePrefix                 = null,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Lookup type
    \-------------------------------------------------------------------------------------------------------------------------*/
    var viewModelType           = _typeLookupService.Lookup($"{topic.ContentType}TopicViewModel", $"{topic.ContentType}ViewModel");

    if (viewModelType is null)  {
      throw new InvalidTypeException(
        $"No class named '{topic.ContentType}TopicViewModel' could be located in any loaded assemblies. This is required " +
        $"to map the topic '{topic.GetUniqueKey()}'."
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Perform mapping
    \-------------------------------------------------------------------------------------------------------------------------*/
    return await MapAsync(topic, viewModelType, associations, cache, attributePrefix, mapPath).ConfigureAwait(false);

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
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  /// <returns>An instance of the dynamically determined View Model with properties appropriately mapped.</returns>
  private async Task<object?>   MapAsync(
    Topic?                      topic,
    Type                        type,
    AssociationTypes            associations,
    MappedTopicCache            cache,
    string?                     attributePrefix                 = null,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null || type is null) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle cached objects
    >---------------------------------------------------------------------------------------------------------------------------
    | Included entries that are still initializing, so a circular constructor reference (i.e., the same topic and type are
    | already under construction higher up the current chain) can be distinguished from concurrent siblings (i.e., two
    | independent branches mapping the same topic and type at once). The former throws; the latter awaits for the first one to
    | finish construction and then uses the same cached view model.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (cache.TryGetValue(topic.Id, type, out var cacheEntry, includeInitializing: true)) {
      return await resolveCachedEntry(cacheEntry, mapPath).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Identify parameters
    \-------------------------------------------------------------------------------------------------------------------------*/
    var typeAccessor            = TypeAccessorCache.GetTypeAccessor(type);
    var properties              = typeAccessor.GetMembers(MemberTypes.Property);
    var parameters              = typeAccessor.ConstructorParameters;
    var arguments               = new object?[parameters.Count];
    var attributeArguments      = (IDictionary<string, string?>)new Dictionary<string, string?>();
    var parameterQueue          = new Dictionary<int, Task<object?>>();

    /*--------------------------------------------------------------------------------------------------------------------------
    | Pre-cache entry
    >-------------------------------------------------------------------------------------------------------------------------
    | In property mapping, we deal with circular references by returning a cached reference. That isn't practical with circular
    | references in constructor mapping. To help avoid these, we register a pre-cache entry as IsInitializing, but without a
    | mapped object. If a concurrent sibling wins the race to preregister the same topic and view model type, we defer to its
    | result rather than constructing a duplicate. If we're constructing the same topic and view model type as we're already in
    | the middle of constructing further up the MapPath chain, however, that's a true circular construction reference, handled
    | via the local resolveCachedEntry() function.
    \-------------------------------------------------------------------------------------------------------------------------*/
    var (entry, isNew)          = cache.Preregister(topic.Id, type);

    if (!isNew) {
      return await resolveCachedEntry(entry, mapPath).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish mapping path chain
    >---------------------------------------------------------------------------------------------------------------------------
    | Now that this pass owns constructing a new view model, establish the initial topic and view model type in the MapPath, so
    | any nested mapping that arrives back to the same pair while it is still initializing is recognized as a true circular
    | constructor reference rather than harmless concurrency among independent siblings.
    \-------------------------------------------------------------------------------------------------------------------------*/
    mapPath                     = new(topic.Id, type, mapPath);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Construct and register the target
    >---------------------------------------------------------------------------------------------------------------------------
    | Only the construction span is guarded, so the entry's completion is always settled: cache.Register() settles it on
    | success and the catch faults it on failure, so a pass awaiting this entry observes the failure instead of hanging.
    | Property mapping runs afterward, outside the guard, since the entry is already settled by then.
    \-------------------------------------------------------------------------------------------------------------------------*/
    object? target;

    try {

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle AttributeDictionary constructor
      >-------------------------------------------------------------------------------------------------------------------------
      | A model may optionally expose a constructor with a single parameter accepting an AttributeDictionary. In this scenario,
      | the TopicMappingService may optionally pass a lightweight AttributeDictionary, allowing the model's constructor to
      | populate scalar values, instead of relying on reflection.
      \-----------------------------------------------------------------------------------------------------------------------*/
      if (parameters.Count is 1 && parameters[0].Type == typeof(AttributeDictionary)) {

        // This strategy is only performant if there are quite a several scalar properties and they are well-covered by the
        // attributes. As a fast heuristic to evaluate this, we expect five or more attributes and three or more compatible
        // properties. In practice, this should be benefitial with any more than mapped attributes, but we also expect that most
        // topics will have 2-3 excluded or unmapped attributes (e.g., Title, LastModified). With models, we can be a bit more
        // intelligent, by excluding any members that are likely compatible with Topic properties, thus exluding e.g., Id, Key,
        // WebPath, etc. This doesn't guarantee that the attributes map to the properties, but a more accurate evaluation would
        // undermine the performance benefits of this optimization.
        if (topic.Attributes.Count >= 5 && properties.Count(p => !p.MaybeCompatible) >= 3) {
          var attributes        = topic.Attributes.AsAttributeDictionary(true);
          arguments[0]          = attributes;
          attributeArguments    = attributes;
        }
        else {
          parameters            = new();
          arguments             = [];
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Handle other constructors
      >-------------------------------------------------------------------------------------------------------------------------
      | A model may optionally expose a constructor with multiple parameters, which can be defined via reflection in the same
      | way as properties would be. This is especially useful for records using the positional syntax (i.e., where properties
      | are defined using the constructor). This also, optionally, provides the model with more control, where needed, over how
      | it's constructed.
      \-----------------------------------------------------------------------------------------------------------------------*/
      else {

        foreach (var parameter in parameters) {
          parameterQueue.Add(parameter.ParameterInfo.Position, GetParameterAsync(topic, associations, parameter, cache, attributePrefix, mapPath));
        }

        await Task.WhenAll(parameterQueue.Values).ConfigureAwait(false);

        foreach (var parameter in parameterQueue) {
          arguments[parameter.Key] = await parameter.Value.ConfigureAwait(false);
        }

      }

      /*------------------------------------------------------------------------------------------------------------------------
      | Initialize object
      \-----------------------------------------------------------------------------------------------------------------------*/
      target                    = Activator.CreateInstance(type, arguments);

      Contract.Assume(
        target,
        $"The target type '{type}' could not be properly constructed, as required to map the topic '{topic.GetUniqueKey()}'."
      );

      /*------------------------------------------------------------------------------------------------------------------------
      | Cache object
      \-----------------------------------------------------------------------------------------------------------------------*/
      cache.Register(topic.Id, associations, target);

    }

    // Construction failed before the entry was registered, so fault it; a concurrent pass awaiting this entry's completion then
    // observes the failure instead of hanging on a map that will never complete
    catch (Exception exception) {
      entry.Fault(exception);
      throw;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Loop through properties, mapping each one
    \-------------------------------------------------------------------------------------------------------------------------*/
    List<Task> propertyQueue    = [];
    var mappedParameters        = parameters.Select(p => p.Name).Union(attributeArguments.Select(a => a.Key)).ToArray();

    foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
      if (!mappedParameters.Contains(property.Name, StringComparer.OrdinalIgnoreCase)) {
        propertyQueue.Add(SetPropertyAsync(topic, target, associations, property, cache, attributePrefix, false, mapPath));
      }
    }

    await Task.WhenAll(propertyQueue).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return target
    \-------------------------------------------------------------------------------------------------------------------------*/
    return target;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Resolve cached entry
    >---------------------------------------------------------------------------------------------------------------------------
    | Returns a cached instance, expanding it with any missing associations when needed. If the entry is still initializing, it
    | is either a true circular constructor reference on the current path (in which we throw an exception) or it's concurrent
    | mapping of two independent siblings of the same topic and view model (in which we simply await its completion, then
    | return the shared instance as a normal cache hit).
    \-------------------------------------------------------------------------------------------------------------------------*/
    async Task<object?> resolveCachedEntry(MappedTopicCacheEntry pendingEntry, MapPath? path) {

      // Distinguish a constructor cycle from sibling concurrency for an entry that is still being constructed
      if (pendingEntry.IsInitializing) {
        if (path?.Contains(topic.Id, type) == true) {
          throw new TopicMappingException(
            $"A circular reference was detected while constructing the '{type.Name}' instance for topic '{topic.Id}'. Circular " +
            $"references must be mapped as properties, not as constructor parameters, so that a cached instance can be returned."
          );
        }
        // Not on the current path: a concurrent sibling owns construction, so await its completion before resolving. A mutual
        // constructor cycle split across two concurrently mapped branches is the one case this cannot distinguish and would
        // deadlock; such cycles are unsupported, and still throw when reached from a single branch via the path check above.
        await pendingEntry.Completion.ConfigureAwait(false);
      }

      // Return the cached instance as-is when it already covers the requested associations
      var cachedTarget          = pendingEntry.MappedTopic;
      if (pendingEntry.GetMissingAssociations(associations) is AssociationTypes.None) {
        return cachedTarget;
      }

      // Otherwise expand the cached instance, mapping only the missing associations
      return await MapAsync(topic, cachedTarget, associations, cache, attributePrefix, path).ConfigureAwait(false);

    }

  }

  /*============================================================================================================================
  | METHOD: MAP (T)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <inheritdoc />
  public async Task<T?> MapAsync<T>(Topic? topic, AssociationTypes associations = AssociationTypes.All) where T : class {
    if (typeof(Topic).IsAssignableFrom(typeof(T))) {
      return topic as T;
    }
    return (T?)await MapAsync(topic, typeof(T), associations, new()).ConfigureAwait(false);
  }

  /*============================================================================================================================
  | METHOD: MAP (OBJECTS)
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  /// <remarks>
  ///   This internal version passes a private cache of mapped objects from this run. This helps prevent problems with
  ///   recursion in case <see cref="Topic"/> is referred to multiple times (e.g., a <c>Children</c> collection with <see cref
  ///   ="IncludeAttribute"/> set to include <see cref="AssociationTypes.Parents"/>).
  /// </remarks>
  /// <returns>
  ///   The target view model with the properties appropriately mapped.
  /// </returns>
  private async Task<object> MapAsync(
    Topic?                      topic,
    object                      target,
    AssociationTypes            associations,
    MappedTopicCache            cache,
    string?                     attributePrefix                 = null,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate input
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (topic is null) {
      return target;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (target is Topic) {
      return topic;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle cached objects
    >---------------------------------------------------------------------------------------------------------------------------
    | If the cache contains an entry, check to make sure it includes all of the requested associations. If it does, return it.
    | Otherwise, add the missing associations to the cache entry and map only the subset this pass added, so that a concurrent
    | pass doesn't remap the same associations.
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (cache.TryGetValue(topic.Id, target.GetType(), out var cacheEntry)) {
      associations              = cacheEntry.AddMissingAssociations(associations);
      target                    = cacheEntry.MappedTopic;
      if (associations is AssociationTypes.None) {
        return cacheEntry.MappedTopic;
      }
    }
    else if (!topic.IsNew) {
      cache.Register(
        topic.Id,
        associations,
        target
      );
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Loop through properties, mapping each one
    \-------------------------------------------------------------------------------------------------------------------------*/
    List<Task> taskQueue        = [];
    var typeAccessor            = TypeAccessorCache.GetTypeAccessor(target.GetType());

    foreach (var property in typeAccessor.GetMembers(MemberTypes.Property)) {
      taskQueue.Add(SetPropertyAsync(topic, target, associations, property, cache, attributePrefix, cacheEntry is not null, mapPath));
    }
    await Task.WhenAll(taskQueue).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return result
    \-------------------------------------------------------------------------------------------------------------------------*/
    return target;

  }

  /*============================================================================================================================
  | PRIVATE: GET PARAMETER (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a <paramref name="parameter"/>, retrieves the appropriate value from the corresponding <paramref name="source"/>
  ///   topic, while honoring <paramref name="associations"/>.
  /// </summary>
  /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
  /// <param name="associations">Determines what associations the mapping should include, if any.</param>
  /// <param name="parameter">Information related to the current parameter.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task<object?>   GetParameterAsync(
    Topic source,
    AssociationTypes associations,
    ParameterMetadata parameter,
    MappedTopicCache cache,
    string? attributePrefix     = null,
    MapPath? mapPath            = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish per-property variables
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = parameter.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Bypass if mapping is disabled
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.DisableMapping) {
      return parameter.ParameterInfo.DefaultValue;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle [MapToParent] attribute
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.MapToParent) {
      return await MapAsync(
        source,
        parameter.Type,
        associations,
        cache,
        configuration.AttributePrefix + attributePrefix,
        mapPath
      ).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Determine value
    \-------------------------------------------------------------------------------------------------------------------------*/
    var value                   = await GetValue(source, parameter.Type, associations, parameter, cache, attributePrefix, false, mapPath).ConfigureAwait(false);

    if (value is null && parameter.IsList) {
      return await getList(parameter.Type).ConfigureAwait(false);
    }

    return value;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Get List Function
    \-------------------------------------------------------------------------------------------------------------------------*/
    async Task<IList?> getList(Type targetType) {

      var sourceList            = await GetSourceCollectionAsync(source, associations, parameter, attributePrefix, false).ConfigureAwait(false);
      var targetList            = InitializeCollection(targetType);

      if (targetList is null) {
        return null;
      }

      await PopulateTargetCollectionAsync(sourceList, targetList, parameter, cache, targetList, mapPath).ConfigureAwait(false);

      return targetList;

    }

  }

  /*============================================================================================================================
  | PRIVATE: SET PROPERTY (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Helper function that evaluates each property on the target object and attempts to retrieve a value from the source
  ///   <see cref="Topic"/> based on predetermined conventions.
  /// </summary>
  /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
  /// <param name="target">The target object to map the data to.</param>
  /// <param name="associations">Determines what associations the mapping should include, if any.</param>
  /// <param name="propertyAccessor">Information related to the current property.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task SetPropertyAsync(
    Topic                       source,
    object                      target,
    AssociationTypes            associations,
    MemberAccessor              propertyAccessor,
    MappedTopicCache            cache,
    string?                     attributePrefix                 = null,
    bool                        mapAssociationsOnly             = false,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish per-property variables
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = propertyAccessor.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Bypass if mapping is disabled
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.DisableMapping) {
      return;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle [MapToParent] attribute
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.MapToParent) {
      var targetProperty        = propertyAccessor.GetValue(target);
      if (targetProperty is not null) {
        await MapAsync(
          source,
          targetProperty,
          associations,
          cache,
          configuration.AttributePrefix + attributePrefix,
          mapPath
        ).ConfigureAwait(false);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Determine value
    \-------------------------------------------------------------------------------------------------------------------------*/
    else {
      var value                 = await GetValue(source, propertyAccessor.Type, associations, propertyAccessor, cache, attributePrefix, mapAssociationsOnly, mapPath).ConfigureAwait(false);
      if (value is null && propertyAccessor.IsList) {
        await SetCollectionValueAsync(source, target, associations, propertyAccessor, cache, attributePrefix, mapAssociationsOnly, mapPath).ConfigureAwait(false);
      }
      else if (value != null && propertyAccessor.CanWrite) {
        propertyAccessor.SetValue(target, value, true);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate fields
    \-------------------------------------------------------------------------------------------------------------------------*/
    propertyAccessor.Validate(target);

  }

  /*============================================================================================================================
  | PRIVATE: GET VALUE (ASYNC)
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Helper function that retrieves a value from the source <see cref="Topic"/> based on predetermined conventions.
  /// </summary>
  /// <param name="source">The <see cref="Topic"/> entity to derive the data from.</param>
  /// <param name="targetType">The <see cref="Type"/> of the target parameter or property.</param>
  /// <param name="associations">Determines what associations the mapping should include, if any.</param>
  /// <param name="itemMetadata">Information related to the current parameter or property.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task<object?>   GetValue(
    Topic source,
    Type targetType,
    AssociationTypes associations,
    ItemMetadata itemMetadata,
    MappedTopicCache cache,
    string? attributePrefix     = "",
    bool mapAssociationsOnly    = false,
    MapPath? mapPath            = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Assign default value
    \-------------------------------------------------------------------------------------------------------------------------*/
    var value                   = (object?)null;
    if (!mapAssociationsOnly && configuration.DefaultValue is not null) {
      value                     = configuration.DefaultValue;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle by type, attribute
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!mapAssociationsOnly && TryGetCompatibleProperty(source, targetType, itemMetadata, attributePrefix, out var compatibleValue)) {
      value                     = compatibleValue;
    }
    else if (itemMetadata.IsConvertible) {
      if (!mapAssociationsOnly) {
        value                   = GetScalarValue(source, itemMetadata, attributePrefix);
      }
    }
    else if (itemMetadata.IsList) {
      return null;
    }
    else if (configuration.GetCompositeAttributeKey(attributePrefix) is "Parent") {
      if (associations.HasFlag(AssociationTypes.Parents) && source.Parent is not null) {
        value                   = await GetTopicReferenceAsync(source.Parent, targetType, itemMetadata, cache, mapPath).ConfigureAwait(false);
      }
    }
    else if (configuration.MapToParent) {
      return null;
    }
    else if (itemMetadata.Type.IsClass && associations.HasFlag(AssociationTypes.References)) {
      var topicReference        = await getTopicReference().ConfigureAwait(false);
      if (topicReference is not null) {
        value                   = await GetTopicReferenceAsync(topicReference, targetType, itemMetadata, cache, mapPath).ConfigureAwait(false);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return value
    \-------------------------------------------------------------------------------------------------------------------------*/
    return value;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Get Topic Reference
    \-------------------------------------------------------------------------------------------------------------------------*/
    async Task<Topic?> getTopicReference()  {

      // Check for standard topic reference
      var topicReference        = source.References.GetValue(configuration.GetCompositeAttributeKey(attributePrefix));
      if (topicReference is not null) {
        return topicReference;
      }

      int topicReferenceId;
      if (configuration.GetCompositeAttributeKey(attributePrefix).EndsWith("Id", StringComparison.OrdinalIgnoreCase)) {
        topicReferenceId        = source.Attributes.GetInteger(configuration.GetCompositeAttributeKey(attributePrefix), 0);
      }
      else {
        topicReferenceId        = source.Attributes.GetInteger($"{configuration.GetCompositeAttributeKey(attributePrefix)}Id", 0);
      }
      if (topicReferenceId > 0) {
        topicReference          = await _topicRepository.Load(topicReferenceId, source).ConfigureAwait(false);
      }

      return topicReference;

    }

  }

  /*============================================================================================================================
  | PRIVATE: GET SCALAR VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Gets a scalar property from a <see cref="Topic"/>.
  /// </summary>
  /// <remarks>
  ///   The <see cref="GetScalarValue(Topic, ItemMetadata, String)"/> method will attempt to retrieve the value from the
  ///   <paramref name="source"/> based on, in order, the <paramref name="source"/>'s <c>Get{Property}()</c> method,
  ///   <c>{Property}</c> property, and, finally, its <see cref="Topic.Attributes"/> collection (using <see cref=
  ///   "TrackedRecordCollection{TItem, TValue}.GetValue(String, Boolean)"/>).
  /// </remarks>
  /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
  /// <param name="itemMetadata">The <see cref="ItemMetadata"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <autogeneratedoc />
  private static string? GetScalarValue(Topic source, ItemMetadata itemMetadata, string? attributePrefix) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to retrieve value from topic.Get{Property}()
    \-------------------------------------------------------------------------------------------------------------------------*/
    var typeAccessor            = GetTopicAccessor(source.GetType());

    var attributeValue          = (string?)null;
    var maybeCompatible         = source.GetType() != typeof(Topic) || itemMetadata.MaybeCompatible;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to retrieve value from topic.{Property}
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (maybeCompatible) {
      attributeValue            = typeAccessor.GetMethodValue(source, $"Get{configuration.GetCompositeAttributeKey(attributePrefix)}")?.ToString();
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to retrieve value from topic.{Property}
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (maybeCompatible && attributeValue is null) {
      attributeValue            = typeAccessor.GetPropertyValue(source, configuration.GetCompositeAttributeKey(attributePrefix))?.ToString();
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Otherwise, attempt to retrieve value from topic.Attributes.GetValue({Property})
    \-------------------------------------------------------------------------------------------------------------------------*/
    attributeValue              ??= source.Attributes.GetValue(
      configuration.GetCompositeAttributeKey(attributePrefix),
      configuration.DefaultValue?.ToString(),
      configuration.InheritValue
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return value
    \-------------------------------------------------------------------------------------------------------------------------*/
    return attributeValue;

  }

  /*============================================================================================================================
  | PRIVATE: INITIALIZE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a collection type, attempts to initialize a compatible type.
  /// </summary>
  /// <param name="targetType">The <see cref="Type"/> of collection to initialize.</param>
  private static IList? InitializeCollection(Type targetType) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to create specific type
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!targetType.IsInterface && !targetType.IsAbstract) {
      return (IList?)Activator.CreateInstance(targetType);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle types that don't implement IList or IEnumerable
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!targetType.IsGenericType) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to create generic list
    \-------------------------------------------------------------------------------------------------------------------------*/
    var parameters              = targetType.GetGenericArguments();

    if (parameters.Length != 1) {
      return null;
    }

    var genericType             = typeof(List<>);
    var concreteType            = genericType.MakeGenericType(parameters);

    return (IList?)Activator.CreateInstance(concreteType);

  }

  /*============================================================================================================================
  | PRIVATE: SET COLLECTION VALUE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a collection property, identifies a source collection, maps the values to DTOs, and attempts to add them to the
  ///   target collection.
  /// </summary>
  /// <remarks>
  ///   Given a collection <paramref name="memberAccessor"/> on a <paramref name="target"/> DTO, attempts to identify a source
  ///   collection on the <paramref name="source"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
  ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
  ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
  ///   property name, though this behavior can be modified using the <paramref name="memberAccessor"/>, based on annotations
  ///   on the <paramref name="target"/> DTO.
  /// </remarks>
  /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
  /// <param name="target">The target DTO on which to set the property value.</param>
  /// <param name="associations">Determines what associations the mapping should include, if any.</param>
  /// <param name="memberAccessor">The <see cref="MemberAccessor"/> with details about the property's attributes.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task SetCollectionValueAsync(
    Topic                       source,
    object                      target,
    AssociationTypes            associations,
    MemberAccessor              memberAccessor,
    MappedTopicCache            cache,
    string?                     attributePrefix,
    bool                        mapAssociationsOnly,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish per-entry lock
    >---------------------------------------------------------------------------------------------------------------------------
    | Two concurrent passes expanding the same cached target with disjoint associations (see e.g., the cache-hit path in
    | MapAsync) can populate the same member's target list from different sources, so its creation and population are
    | synchronized on the shared cache entry. A target that isn't cached (i.e., a new topic) is never shared across passes, so
    | its own instance serves as a sufficient and uncontended lock.
    \-------------------------------------------------------------------------------------------------------------------------*/
    cache.TryGetValue(source.Id, target.GetType(), out var cacheEntry);
    var collectionLock          = (object?)cacheEntry?? target;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Ensure target list is created
    >---------------------------------------------------------------------------------------------------------------------------
    | Locked so two concurrent passes can't both observe a null list and instantiate competing instances, which would orphan the
    | items added to whichever collection is overwritten. The lock is synchronous and never held across an await.
    \-------------------------------------------------------------------------------------------------------------------------*/
    IList? targetList;
    lock (collectionLock) {
      targetList                = (IList?)memberAccessor.GetValue(target);
      if (targetList is null) {
        targetList              = InitializeCollection(memberAccessor.Type);
        memberAccessor.SetValue(target, targetList);
      }
    }

    Contract.Assume(
      targetList,
      $"The target list type, '{memberAccessor.Type}', could not be properly constructed, as required to " +
      $"map the '{memberAccessor.Name}' property on the '{target?.GetType().Name}' object."
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish source collection to store topics to be mapped
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourceList              = await GetSourceCollectionAsync(source, associations, memberAccessor, attributePrefix, mapAssociationsOnly).ConfigureAwait(false);

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate that source collection was identified
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (sourceList is null) return;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map the topics from the source collection, and add them to the target collection
    \-------------------------------------------------------------------------------------------------------------------------*/
    await PopulateTargetCollectionAsync(sourceList, targetList, memberAccessor, cache, collectionLock, mapPath).ConfigureAwait(false);

  }

  /*============================================================================================================================
  | PRIVATE: GET SOURCE COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a source topic and a property configuration, attempts to identify a source collection that maps to the property.
  /// </summary>
  /// <remarks>
  ///   Given a collection <paramref name="itemMetadata"/> on a target DTO, attempts to identify a source collection on the
  ///   <paramref name="source"/>. Collections can be mapped to <see cref="Topic.Children"/>, <see
  ///   cref="Topic.Relationships"/>, <see cref="Topic.IncomingRelationships"/> or to a nested topic (which will be part of
  ///   <see cref="Topic.Children"/>). By default, <see cref="TopicMappingService"/> will attempt to map based on the
  ///   property name, though this behavior can be modified using the <paramref name="itemMetadata"/>, based on annotations
  ///   on the target DTO.
  /// </remarks>
  /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
  /// <param name="associations">Determines what associations the mapping should include, if any.</param>
  /// <param name="itemMetadata">The <see cref="ItemMetadata"/> with details about the property's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="mapAssociationsOnly">Determines if properties not associated with associations should be mapped.</param>
  private async Task<IList<Topic>> GetSourceCollectionAsync(
    Topic                       source,
    AssociationTypes            associations,
    ItemMetadata                itemMetadata,
    string?                     attributePrefix,
    bool                        mapAssociationsOnly
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish source collection to store topics to be mapped
    \-------------------------------------------------------------------------------------------------------------------------*/
    var                         configuration                   = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Warm lazy-loaded payload before probing collections
    \-------------------------------------------------------------------------------------------------------------------------*/
    await ((ITopicLazyLoadable)source).EnsureLoaded(AssociationMap.PayloadMappings[configuration.CollectionType]).ConfigureAwait(false);

    var                         listSource                      = (IList<Topic>)[];
    var                         collectionKey                   = configuration.CollectionKey;
    var                         collectionType                  = configuration.CollectionType;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle children
    \-------------------------------------------------------------------------------------------------------------------------*/
    listSource                  = getCollection(
      CollectionType.Children,
      s => true,
      () => [.. source.Children]
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle (outgoing) relationships
    \-------------------------------------------------------------------------------------------------------------------------*/
    listSource                  = getCollection(
      CollectionType.Relationship,
      key => source.Relationships.Contains(key),
      () => source.Relationships.GetValues(collectionKey)
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle nested topics, or children corresponding to the property name
    \-------------------------------------------------------------------------------------------------------------------------*/
    listSource                  = getCollection(
      CollectionType.NestedTopics,
      key => source.Children.Contains(key),
      () => source.Children[collectionKey].Children
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle (incoming) relationships
    \-------------------------------------------------------------------------------------------------------------------------*/
    listSource                  = getCollection(
      CollectionType.IncomingRelationship,
      key => source.IncomingRelationships.Contains(key),
      () => source.IncomingRelationships.GetValues(collectionKey)
    );

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle other strongly typed source collections
    \-------------------------------------------------------------------------------------------------------------------------*/
    //The following allows a target collection to be mapped to an IList<Topic> source collection. This is valuable for custom,
    //curated collections defined on e.g. derivatives of Topic, but which don't otherwise map to a specific collection type.
    //For example, the ContentTypeDescriptor's AttributeDescriptors collection, which provides a rollup of AttributeDescriptors
    //from the current ContentTypeDescriptor, as well as all of its ascendants. On an expansion pass, this fallback runs only
    //when MappedCollections is among the claimed associations, avoiding a redundant reflective source-property read (and its
    //re-enumeration) for passes that don't claim it.
    if (listSource.Count == 0 && (!mapAssociationsOnly || associations.HasFlag(AssociationTypes.MappedCollections))) {
      var sourceProperty        = TypeAccessorCache.GetTypeAccessor(source.GetType()).GetMember(configuration.GetCompositeAttributeKey(attributePrefix));
      if (
        sourceProperty?.GetValue(source) is IList sourcePropertyValue &&
        sourcePropertyValue.Count > 0 &&
        sourcePropertyValue[0]  is Topic
      ) {
        listSource              = getCollection(
          CollectionType.MappedCollection,
          s => true,
          () => [.. sourcePropertyValue.Cast<Topic>()]
        );
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle Metadata relationship
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (!mapAssociationsOnly && listSource.Count == 0 && !String.IsNullOrWhiteSpace(configuration.MetadataKey)) {
      var metadataKey           = $"Root:Configuration:Metadata:{configuration.MetadataKey}:LookupList";
      var metadataParent        = await _topicRepository.Load(metadataKey, source, TopicPayload.Children).ConfigureAwait(false);
      if (metadataParent is not null) {
        listSource              = [.. metadataParent.Children];
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Handle flattening of children
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (configuration.FlattenChildren) {
      List<Topic> flattenedList = [];
      listSource.ToList().ForEach(t => FlattenTopicGraph(t, flattenedList));
      listSource                = flattenedList;
    }

    return listSource;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Provide local function for evaluating current collection
    \-------------------------------------------------------------------------------------------------------------------------*/
    IList<Topic> getCollection(CollectionType collection, Func<string, bool> contains, Func<IList<Topic>> getTopics) {
      var targetAssociations    = AssociationMap.Mappings[collection];
      var preconditionsMet      =
        listSource.Count == 0 &&
        (!mapAssociationsOnly || targetAssociations is not AssociationTypes.None) &&
        (collectionType is CollectionType.Any || collectionType.Equals(collection)) &&
        (collectionType is CollectionType.Children || collection is not CollectionType.Children) &&
        (targetAssociations is  AssociationTypes.None || associations.HasFlag(targetAssociations)) &&
        contains(configuration.CollectionKey);
      return preconditionsMet?  getTopics() : listSource;
    }

  }

  /*============================================================================================================================
  | PRIVATE: POPULATE TARGET COLLECTION
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a source list, will populate a target list based on the configured behavior of the target property.
  /// </summary>
  /// <param name="sourceList">The <see cref="IList{Topic}"/> to pull the source <see cref="Topic"/> objects from.</param>
  /// <param name="targetList">The target <see cref="IList"/> to add the mapped <see cref="Topic"/> objects to.</param>
  /// <param name="itemMetadata">The <see cref="ItemMetadata"/> with details about the property's attributes.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="collectionLock">
  ///   The object to lock on while adding to <paramref name="targetList"/>, so concurrent passes populating a shared target's
  ///   list from disjoint sources don't modify it simultaneously. Callers pass the shared cache entry for a cached target, or
  ///   the (unshared) list itself when populating a constructor parameter.
  /// </param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task PopulateTargetCollectionAsync(
    IList<Topic>                sourceList,
    IList                       targetList,
    ItemMetadata                itemMetadata,
    MappedTopicCache            cache,
    object                      collectionLock,
    MapPath?                    mapPath                         = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Determine the type of item in the list
    \-------------------------------------------------------------------------------------------------------------------------*/
    var listType                = typeof(ITopicViewModel);
    foreach (var type in targetList.GetType().GetInterfaces()) {
      if (type.IsGenericType && typeof(IList<>) == type.GetGenericTypeDefinition()) {
        //Uses last argument in case it's a KeyedCollection; in that case, we want the TItem type
        listType                = type.GetGenericArguments().Last();
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Queue up mapping tasks
    \-------------------------------------------------------------------------------------------------------------------------*/
    List<Task<object?>> taskQueue = [];

    foreach (var childTopic in  sourceList) {

      //Ensure the source topic isn't disabled; disabled topics should never be returned to the presentation layer unless
      //explicitly requested by a top-level request.
      if (childTopic.IsDisabled) {
        continue;
      }

      //Skip nested topics; those should be explicitly mapped to their own collection or topic reference
      if (childTopic.ContentType.Equals("List", StringComparison.OrdinalIgnoreCase)) {
        continue;
      }

      //Ensure the source topic matches any [FilterByContentType()] settings
      if (
        configuration.ContentTypeFilter is not null &&
        !childTopic.ContentType.Equals(configuration.ContentTypeFilter, StringComparison.OrdinalIgnoreCase)
      ) {
        continue;
      }

      //Ensure the source topic matches any [FilterByAttribute()] settings
      if (!configuration.SatisfiesAttributeFilters(childTopic)) {
        continue;
      }

      //Map child topic to target DTO
      var childDto              = (object)childTopic;
      if (!typeof(Topic).IsAssignableFrom(listType)) {
        var mappingType         = GetValidatedMappingType(configuration.MapAs, listType)?? GetValidatedMappingType(childTopic, listType);
        if (mappingType is not  null) {
          taskQueue.Add(MapAsync(childTopic, mappingType, configuration.IncludeAssociations, cache, mapPath: mapPath));
        }
      }
      else {
        addToList(childDto);
      }

    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Process mapping tasks
    >---------------------------------------------------------------------------------------------------------------------------
    | Awaited as a batch, then added in the order the tasks were queued in, rather than completion order; this keeps sibling
    | order deterministic regardless of which child mappings genuinely await
    \-------------------------------------------------------------------------------------------------------------------------*/
    var dtos                    = await Task.WhenAll(taskQueue).ConfigureAwait(false);

    foreach (var dto in dtos) {
      if (dto is not null) {
        addToList(dto);
      }
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Function: Add to List
    >---------------------------------------------------------------------------------------------------------------------------
    | Locked so a concurrent pass populating the same shared list from a disjoint source can't add at the same time; the lock
    | is synchronous and never held across an await, as child mapping happens outside of it via the above task queue.
    \-------------------------------------------------------------------------------------------------------------------------*/
    void addToList(object dto)  {
      lock (collectionLock) {
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

  /*============================================================================================================================
  | PRIVATE: GET VALIDATED MODEL TYPE
  \---------------------------------------------------------------------------------------------------------------------------*/
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

  /*============================================================================================================================
  | PRIVATE: GET TOPIC REFERENCE
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Attempts to retrieve a topic reference form the <paramref name="source"/>.
  /// </summary>
  /// <param name="source">The source <see cref="Topic"/> from which to pull the value.</param>
  /// <param name="targetType">The <see cref="Type"/> expected for the mapped <paramref name="source"/>.</param>
  /// <param name="itemMetadata">The <see cref="ItemMetadata"/> with details about the item's attributes.</param>
  /// <param name="cache">A cache to keep track of already-mapped object instances.</param>
  /// <param name="mapPath">The current mapping request's path, used to detect circular references during construction.</param>
  private async Task<object?>   GetTopicReferenceAsync(
    Topic source,
    Type targetType,
    ItemMetadata itemMetadata,
    MappedTopicCache cache,
    MapPath? mapPath            = null
  ) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Bypass disabled topics
    \-------------------------------------------------------------------------------------------------------------------------*/
    //Ensure the source topic isn't disabled; disabled topics should never be returned to the presentation layer unless
    //explicitly requested by a top-level request.
    if (source.IsDisabled) {
      return null;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Map referenced topic
    \-------------------------------------------------------------------------------------------------------------------------*/
    var topicDto                = (object?)null;
    var mappingType             = GetValidatedMappingType(configuration.MapAs, targetType)?? GetValidatedMappingType(source, targetType);

    if (mappingType is not null) {
      topicDto                  = await MapAsync(source, mappingType, configuration.IncludeAssociations, cache, mapPath: mapPath).ConfigureAwait(false);
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return type
    \-------------------------------------------------------------------------------------------------------------------------*/
    return topicDto;

  }

  /*============================================================================================================================
  | PRIVATE: FLATTEN TOPIC GRAPH
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Helper function recursively iterates through children and adds each to a collection.
  /// </summary>
  /// <param name="source">The <see cref="Topic"/> entity pull the data from.</param>
  /// <param name="targetList">The list of <see cref="Topic"/> instances to add each child to.</param>
  private static IList<Topic>   FlattenTopicGraph(Topic source, IList<Topic> targetList) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Validate source properties
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source.IsDisabled) return targetList;
    if (source.ContentType is "List") return targetList;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Merge source list into target list
    \-------------------------------------------------------------------------------------------------------------------------*/
    targetList.Add(source);
    source.Children.ToList().ForEach(t => FlattenTopicGraph(t, targetList));
    return targetList;

  }

  /*============================================================================================================================
  | PRIVATE: TRY GET COMPATIBLE PROPERTY
  \---------------------------------------------------------------------------------------------------------------------------*/
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
  /// <param name="itemMetadata">The <see cref="ItemMetadata"/> with details about the item's attributes.</param>
  /// <param name="attributePrefix">The prefix to apply to the attributes.</param>
  /// <param name="value">The compatible property, if it is available.</param>
  private static bool TryGetCompatibleProperty(Topic source, Type targetType, ItemMetadata itemMetadata, string? attributePrefix, out object? value) {

    /*--------------------------------------------------------------------------------------------------------------------------
    | Establish configuration
    \-------------------------------------------------------------------------------------------------------------------------*/
    var configuration           = itemMetadata.Configuration;

    /*--------------------------------------------------------------------------------------------------------------------------
    | Rely on MaybeCompatible to bypass known incompatible types
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (source.GetType() == typeof(Topic) && !itemMetadata.MaybeCompatible) {
      value                     = null;
      return false;
    };

    /*--------------------------------------------------------------------------------------------------------------------------
    | Attempt to retrieve value from topic.{Property}
    \-------------------------------------------------------------------------------------------------------------------------*/
    var sourcePropertyAccessor  = GetTopicAccessor(source.GetType()).GetMember(configuration.GetCompositeAttributeKey(attributePrefix));

    /*--------------------------------------------------------------------------------------------------------------------------
    | Escape clause if preconditions are not met
    \-------------------------------------------------------------------------------------------------------------------------*/
    if (sourcePropertyAccessor  is null || !targetType.IsAssignableFrom(sourcePropertyAccessor.Type)) {
      value                     = null;
      return false;
    }

    /*--------------------------------------------------------------------------------------------------------------------------
    | Return value
    \-------------------------------------------------------------------------------------------------------------------------*/
    value                       = sourcePropertyAccessor.GetValue(source);

    return true;

  }

  /*============================================================================================================================
  | PRIVATE: GET TOPIC ACCESSOR
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Given a specific <see cref="Type"/> for a <see cref="Topic"/>, returns the appropriate <see cref="TypeAccessor"/> from
  ///   the <see cref="TypeAccessorCache"/>.
  /// </summary>
  /// <param name="topicType">The <see cref="Type"/> of the source <see cref="Topic"/>.</param>
  private static TypeAccessor   GetTopicAccessor(Type topicType) =>
    topicType == typeof(Topic)  ? _topicTypeAccessor : TypeAccessorCache.GetTypeAccessor(topicType);

} //Class