# Hierarchical Topic Mapping Service
The [`IHierarchicalTopicMappingService<T>`](IHierarchicalTopicMappingService{T}.cs) and its concrete implementation, [`HierarchicalTopicMappingService<T>`](HierarchicalTopicMappingService{T}.cs), provide special handling for traversing hierarchical trees of view models. 

### Contents
- [Motivation](#motivation)
- [`CachedHierarchicalTopicMappingService`](#cachedhierarchicaltopicmappingservice)
- [Example](#example-2)

## Motivation
While the [`TopicMappingService`](../README.md) is capable of populating trees on its own, it is exclusively bound to honoring the rules defined by the attributes (such as `[Include(associationTypes)]` and `[Flatten]`). By contrast, the `IHierarchicalTopicMappingService<T>` offers three additional capabilities:

1. The number of tiers in the hierarchy can be restricted to a set number (via the `tiers` parameter on `GetRootViewModelAsync()` and `GetViewModelAsync()`).
2. The topics included can be constrained by specifying a method or lamda expression that accepts a `Topic` as the parameter, and returns `true` (if the `Topic` should be mapped) or `false` (if it should be skipped).
3. The type that all _children_ will be mapped to can be specified, instead of letting the model type be determined exclusively by the `Topic.ContentType` property.

In many cases, these are not needed. They do, however, provide additional flexibility for particular scenarios. For example, these are valuable for constructing the navigation used by e.g. the [`MenuViewComponentBase<T>`](../../../OnTopic.AspNetCore.Mvc/Components/MenuViewComponentBase{T}.cs), which should be restricted to three tiers, should be mapped to a [`NavigationTopicViewModel`](../../../OnTopic.ViewModels/NavigationTopicViewModel.cs), and, in the case of the many navigation, should exclude any topics of the content type `PageGroup`.


## `CachedHierarchicalTopicMappingService`
The [`CachedHierarchicalTopicMappingService<T>`](CachedHierarchicalTopicMappingService{T}.cs) caches entries keyed based on the `Topic.Id` of the root `Topic` as well as the `T` argument of the type. Because of the mechanics of the `HierarchicalTopicMappingService<T>`, this cannot simply use the `CachedTopicMappingService` for caching, since each tier of navigation is mapped independently. This is necessary to apply the above business logic to the hierarchy, but makes it impossible for the `CachedTopicMappingService` to understand whether each request is suitable for caching.

> *Note:* As with the `CachedTopicMappingService`, the `CachedHierarchicalTopicMapping` service should be used with caution. It will not be (immediately) updated if the underlying database or topic graph are updated. And since the topic graph is already cached, it effectively doubles the memory footprint of the graph by storing it both as topics as well as view models. That said, this is useful for large view model graphs that are frequently reused—such as those that show up in the navigation of a site.

## Example
The first code block demonstrates how to construct a new instance of a `IHierarchicalTopicMappingService<T>`. In this case, it wraps the default `HierarchicalTopicMappingService<T>` in a `CachedHierarchicalTopicMappingService<T>` for caching, and maps children to the `NavigationTopicViewModel` class from the [`Ignia.Topics.ViewModels`](../../../OnTopic.ViewModels/) project. Typically, this would be done in the _Composition Root_ of an application, with the service passed into e.g. a `Controller` as an `IHierarchicalTopicMappingService<T>` dependency.
```csharp
var hierarchicalTopicMappingService = new CachedHierarchicalTopicMappingService<NavigationTopicViewModel>(
  new HierarchicalTopicMappingService<NavigationTopicViewModel>(
    _topicRepository,
    _topicMappingService
  );
);
```
Once the `IHierarchicalTopicMappingService<T>` is constructed, it can by calling the main entry point, `GetRootViewModelAsync()`, which accepts three arguments:

1. **`Topic sourceTopic`:** The topic representing the root of the hierarchy. This could be the root topic in the database, but will more likely be the root of a subtree.
2. **`int tiers = 1`:** The number of tiers to crawl. While the `TopicMappingService` implementation will crawl indefinitely, given the right conditions, the `IHierarchicalTopicMappingService<T>` can be constrained to a particular depth by the caller.
3. **`Func<Topic, bool> validationDelegate = null`:** A validation function that accepts a `Topic` as input and returns `true` if the `Topic` (and its descendants) should be included, and otherwise `false`.

```csharp
await hierarchicalTopicMappingService.GetRootViewModelAsync(
  hierarchicalTopicMappingService.GetHierarchicalRoot(currentTopic, 2, "Web"),
  2,
  t => t.ContentType != "PageGroup"
).ConfigureAwait(false),
```

In this code example, the following arguments are used:

1. **`Topic sourceTopic`:** The `GetHierarchicalRoot()` helper function is used to find a root that is at the second tier of the topic graph (right below the database root), but within the path of the current topic. So, for instance, if the current topic is at `Root:Customers:Support:Email`, then the `GetHierarchicalRoot()` would return `Root:Customers`.
2. **`int tiers = 1`:** The number of tiers is set to 2. So in the above example, `Root:Customers:Support:Email` would be included (since `Email` is two tiers from the hierarchical root), but e.g. `Root:Customers:Support:Email:Priority` wouldn't be. '
3. **`Func<Topic, bool> validationDelegate = null`:** The validation delegate will reject any topics of the type `PageGroup`. Typically, pages of type `PageGroup` have their own internal navigation, which shouldn't be duplicated in the primary navigation of the site.