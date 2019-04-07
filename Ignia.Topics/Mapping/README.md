# Topic Mapping Service
The [`ITopicMappingService`](ITopicMappingService.cs) provides the abstract interface for a service that maps `Topic` entities to any arbitrary data transfer object. It is intended primarily to aid in mapping the `Topic` entity to view model instances, such as the ones provided in the [`ViewModels` assembly](../../Ignia.Topics.ViewModels)

### Contents
- [`TopicMappingService`](#topicmappingservice)
  - [Target Object](#target-object)
  - [Properties](#properties)
    - [Scalar Values](#scalar-values)
    - [Collections](#collections)
    - [References](#references)
    - [Parent](#parent)
  - [Example](#example) 
- [Attributes](#attributes)
  - [Example](#example-1) 
- [Polymorphism](#polymorphism)
  - [Filtering](#filtering)
  - [Topics](#topics)
- [Caching](#caching)
  - [Internal Caching](#internal-caching)
  - [`CachedTopicMappingService`](#cachedtopicmappingservice)
- [`HierarchicalTopicMappingService`](#hierarchicaltopicmappingservice)
  - [`CachedHierarchicalTopicMappingService`](#cachedhierarchicaltopicmappingservice)
  - [Example](#example-2)

## `TopicMappingService`
The [`TopicMappingService`](TopicMappingService.cs) provides a concrete implementation that is expected to satisfy the requirements of most consumers. This supports the following conventions.

### Target Object
- The data transfer object type can be explicitly specified via the `ITopicMappingService.Map<>()` method.
  - E.g., `topicMappingService.Map<TopicViewModel>(topic)` will map the `topic` to the `TopicViewModel` class.
- The data transfer object can also be assigned by convention via the `ITopicMappingService.Map()` method.
  - The target type must be named `ContentTypeTopicViewModel`, based on the `ContentType` of the source `Topic`.
  - E.g., If the source topic's `ContentType` is "Example", it will look for `ExampleTopicViewModel`.
> *Note:* Data transfer objects must have a default constructor so that either of the above methods can dynamically construct a new instance of them.

### Properties
The mapping service will automatically attempt to map any properties on a data transfer object to values on the source `Topic`. To do so, it uses the following conventions:

#### Scalar Values
If a property is of the type `bool`, `int`, `string`, or `DateTime`, then:
- Will pull the value from a parameterless getter method with the same name.
- Will pull the value from a property of the same name.
- Otherwise, will pull the value from the `topic.Attributes.GetValue()` method. 

For example, if a property on a view model is named `Author`, it will automatically look, in order, for:
- `topic.GetAuthor()`
- `topic.Author`
- `topic.Attributes.GetValue("Author")`

#### Collections
If a property implements `IList` (e.g., `List<>`, `Collection<>`, `TopicViewModelCollection<>`), then: 
- Will pull the value from a collection with the same name as the property.
- If the property is explicitly named `Children` then it will load the `topic.Children`.
- Will search, in order, `topic.Relationships`, `topic.IncomingRelationships`, `topic.Children`.
- E.g., If a `List<>` property is named `Cousins` then it might match `topic.Relationships.GetTopics("Cousins")`.

#### References
Topic references relate a single topic to another topic. If a property corresponds to an attribute named `{Property}Id`, that identifier refers to a `Topic`, and that `Topic` maps to an object that is assignable to the original property, then the `Topic` will be loaded, mapped, and assigned to that property. For instance, the following property:
```
public AuthorTopicViewModel Author { get; set; }
```
Would be mapped to an `AuthorTopicViewModel` if `topic.Attributes.GetValue("AuthorId")` returns the identifier of a `Topic` with a `ContentType` set to `Author`. 

#### Parent
If a property is named `Parent`, then the `TopicMappingService` will pull the value from `topic.Parent`. This acts as a special version of a [Topic Reference](#references).

> *Note:* By default, collections and reference properties of related topics will not be pulled. For instance, if a `TopicViewModel` has a `Children` collection, then the relationships, nested topics, and children of those instances will not be populated. This is meant to constrain the size of the object graph delivered.

### Example
The following is an example of a data transfer object (specifically, a view model) that `TopicMappingService` might consume:
```
public class CustomTopicViewModel {
  public TopicViewModel Parent { get; set; }
  public string CustomPropertyA { get; set; }
  public int CustomPropertyB { get; set; }
  public DateTime LastModified { get; set; }
  public TopicViewModelCollection<TopicViewModel> Children { get; set; }
  public TopicViewModelCollection<TopicViewModel> Cousins { get; set; }
  public TopicViewModelCollection<TopicViewModel> NestedTopics { get; set; }
}
```
In this example, the properties would map to:
- `Parent`: The parent `Topic` as a `TopicViewModel`.
- `CustomPropertyA`: A `string` attribute named `CustomPropertyA` or a method named `GetCustomPropertyA()`.
- `CustomPropertyB`: An `int` attribute named `CustomPropertyB` or a method named `GetCustomPropertyB()`.
- `LastModified`: A `DateTime` attribute named `LastModified` or a method named `GetLastModified()`.
- `Children`: The `Children` collection, with each child mapped to a `TopicViewModel`.
- `Cousins`: A relationship or nested topic set named `Cousins`, with each relationship mapped to a `TopicViewModel`.
- `NestedTopics`: A relationship or nested topic set named `NestedTopics` (the name doesn't have any special meaning).

## Attributes 
To support the mapping, a variety of `Attribute` classes are provided for decorating data transfer objects.

- **`[Validation]`**: Enforces the rules established by any of the [`ValidationAttribute`](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.validationattribute%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396) subclasses.
  - This includes e.g., `[Required]`, `[MaxLength()]`, `[RegularExpression()]`, &c.
- **`[DefaultValue()]`**: Sets the default value for the property.
- **`[Inherit]`**: If the corresponding call comes back null, checks parent topics until a value is found.
  - This is the equivalent of calling `topic.Attributes.GetValue(attributeKey, true)`. 
- **`[Metadata(key)]`**: Populates a collection with a list of `LookupItem` values from `Root:Configuration:Metadata`.
  - This is useful for including a list of items to filter collections by.
  - E.g., If child objects have a `TopicLookup` referencing the `Countries` metadata collection. 
- **`[AttributeKey(key)]`**: Instructs the `TopicMappingService` to use the specified `key` instead of the property name when calling `topic.Attributes.GetValue()`.
- **`[FilterByAttribute(key, value)]`**: Ensures that all items in a collection have an attribute named "Key" with a value of "Value"; all else will be excluded. Multiple instances can be stacked.
- **`[Relationship(key, type)]`**: For a collection, optionally specifies the name of the key to look for, instead of the property name, and the relationship type, in case the key name is ambiguous.
- **`[Follow(relationships)]`**: Instructs the code to populate the specified relationships on any view models within a collection.
- **`[Flatten]`**: Includes all descendants for every item in the collection. If the collection enforces uniqueness, duplicates will be removed.

### Example
The following is an example of a data transfer object that implements the above attributes:
```
public class CompanyTopicViewModel {

  [DefaultValue("Ignia")]
  [MaxLength(100)]
  public string CompanyName { get; set; }

  [Inherit]
  [AttributeKey("IsHidden")]
  public bool HideFromDirectory { get; set; }

  [Metadata("Countries")]
  public TopicViewModelCollection<LookupListItemTopicViewModel> Countries { get; set; }

  [Relationship("Companies", Type=RelationshipType.IncomingRelationship)]
  [Follow(Relationships.Children)]
  public TopicViewModelCollection<CaseStudyTopicViewModel> CaseStudies { get; set; }

  [Follow(Relationships.Relationships)]
  public TopicViewModelCollection<TopicViewModel> Children { get; set; }

  [Relationship("Employees", Type=RelationshipType.NestedTopics)]
  [FilterByAttribute("IsActive", "1")]
  [FilterByAttribute("Role", "Account Manager")]
  [Flatten]
  public TopicViewModelCollection<EmployeeTopicViewModel> Contacts { get; set; }

}
```

In this example, the properties would map to:
- `CompanyName`: Would default to "Ignia" if not otherwise set; would throw an error if the value exceeded 100 characters.
- `HideFromDirectory`: An attribute named `IsHidden` or a method named `GetIsHidden()`. If null, will look in `Parent` topics.
- `Countries`: Loads all `LookupListItem` instances in the `Root:Configuration:Metadata:Countries` metadata collection.
- `CaseStudies`: A collection of `CaseStudy` topics pointing to the current `Company` via a "Companies" relationship. Will load the children of each case study.
- `Children`: A collection of child topics, with all relationships (but not e.g. grandchildren) loaded. 
- `Contacts`: A list of `Employee` nested topics, filtered by those with `IsActive` set to `1` (`true`) and `Role` set to "Account Manager". Includes any descendants of the nested topics that meet the previous criteria.

> *Note*: Often times, data transfer objects won't require any attributes. These are only needed if the properties don't follow the built-in conventions and require additional help. For instance, the `[Relationship(…)]` attribute is useful if the relationship key is ambiguous between outgoing relationships and incoming relationships. 

## Polymorphism
If a reference type (e.g., `TopicViewModel Parent`) or a strongly-typed collection property (e.g., `List<TopicViewModel>`) are defined, then any target instances must be assignable by the base type (in these cases, `TopicViewModel`). If they cannot be, then they will not be included; no error will occur.

### Filtering
This can be useful for filtering a collection. For instance, if a `CompanyTopicViewModel` includes an `Employees` collection of type `List<ManagerTopicViewModel>` then it will only be populated by topics that can be mapped to either `ManagerTopicViewModel` or a derivative (perhaps, `ExecutiveTopicViewModel`). Other types (e.g., `EmployeeTopicViewModel`) will be excluded, even though they might otherwise be referenced by the `Employees` relationship.

> *Note:* For this reason, it is recommended that view models use inheritance based on the content type hierarchy. This provides an intuitive mapping to content type definitions, avoids needing to redefine base properties, and allows for polymorphism in assigning derived types.

### Topics
While it's not a best practice, this also works for strongly-typed collections of `Topic` objects. Typically, collections should return view models, but if the collection is strongly-typed to `Topic` (or a derivative) then the source `Topic` will not be mapped, and will be used as-is assuming it implements (or derives from) the target `Topic` type. This can be useful for scenarios where a view needs full access to the object graph (such as the `SitemapController`). In such cases, it is impractical to map the entirety of an object graph, along with all attributes, to a corresponding view model graph, and makes more sense to simply return the `Topic` graph.

## Caching
By default, the `TopicMappingService` will cache a reference to all `MemberInfo` objects associated with each of view model it maps. That mitigates much of the performance hit associated with the use of reflection. Despite that, simply setting properties—and, especially, on large object graphs—can require a lot of processing time. To address this, OnTopic also offers two approaches. 

### Internal Caching
When a request is made to `TopicMappingService`, and internal cache is constructed. If any mapping requests refer to a `Topic` that's already been mapped as part of the _current_ object graph, then that object will be returned. This prevents unnecessary duplication of mapping, and also avoids the potential for infinite loops. For instance, if a view model includes `Children`, and those children are set to `[Follow(Relationships.Parents)]`, the `TopicMappingService` will point back to the originally-mapped `Parent` object, instead of mapping a new instance of that `Topic`.

### `CachedTopicMappingService`
The [`CachedTopicMappingService`](CachedTopicMappingService.cs) Decorator, which accepts a concrete implementation of an `ITopicMappingService`, provides caching across requests based on `topic.Id`, `Type`, and `Relationships`. Because the cache is based on all three of these, it will differentiate between the results of e.g.,
- `topicMappingService.Map<TopicViewModel>(topic, Relationships.All)`
- `topicMappingService.Map<TopicViewModel>(topic, Relationships.Children)`
- `topicMappingService.Map<PageTopicViewModel>(topic, Relationships.Children)`

To implement the caching decorator, use the following construction as a Singleton lifestyle in your composer:
```
var topicRepository = new SqlTopicRepository(…);
var topicMappingService = new TopicMappingService(topicRepository);
var cachedTopicMappingService = new CachedTopicMappingService(topicMappingService);
```

> _**Important**_: Due to limitations discussed below, the application of the `CachedTopicMappingService` is quite restricted. It is likely inapprorpiate for page content, since that wouldn't reflect changes made via the editor. And it isn't appropriate for e.g. the `LayoutControllerBase{T}`, since it manually constructs its tree.


#### Limitations
While the `CachedTopicMappingService` can be useful for particular scenarios, it introduces several limitations that should be accounted for.

1. It may take up considerable memory, depending on how many permutations of mapped objects the application has. This is especially true since it caches each unique object graph; no effort is made to centralize object instances referenced by e.g. relationships in multiple graphs.
2. It makes no effort to validate or evict cache entries. Topics whose values change during the lifetime of the `CachedTopicMappingService` will not be reflected in the mapped responses.
3. If a graph is manually constructed (by e.g. programmatically mapping `Children`) then each instance will be separated cached, thus potentially allowing an instance to be shared between multiple graphs. This can introduce concerns if edge maintenance is important.

## `HierarchicalTopicMappingService`
The [`IHierarchicalTopicMappingService<T>`](Hierarchical/IHierarchicalTopicMappingService{T}.cs) and its concrete implementation, [`HierarchicalTopicMappingService<T>`](Hierarchical/HierarchicalTopicMappingService{T}.cs), provide special handling for traversing hierarchical trees of view models. While the [`TopicMappingService`](#topicmappingservice) is capable of populating trees on its own, it is exclusively bound to honoring the rules defined by the attributes (such as `[Follow(relationships)]` and `[Flatten]`). By contrast, the `IHierarchicalTopicMappingService<T>` offers three additional capabilities:

1. The number of tiers in the hierarchy can be restricted to a set number (via the `tiers` parameter on `GetRootViewModelAsync()` and `GetViewModelAsync()`).
2. The topics included can be constrained by specifying a method or lamda expression that accepts a `Topic` as the parameter, and returns `true` (if the `Topic` should be mapped) or `false` (if it should be skipped). 
3. The type that all _children_ will be mapped to can be specified, instead of letting the model type be determined exclusively by the `Topic.ContentType` property. 

In many cases, these are not needed. They do, however, provide additional flexibility for particular scenarios. For example, these are valuable for constructing the navigation used by e.g. the [`LayoutControllerBase<T>`](../../Ignia.Topics.Web.Mvc/Controllers/LayoutControllerBase{T}.cs), which should be restricted to three tiers, should be mapped to a [`NavigationTopicViewModel`](../../Ignia.Topics.ViewModels/NavigationTopicViewModel.cs`), and, in the case of the many navigation, should exclude any topics of the content type `PageGroup`.

### `CachedHierarchicalTopicMappingService`
The [`CachedHierarchicalTopicMappingService<T>`](Hierarchical/CachedHierarchicalTopicMappingService{T}.cs) caches entries keyed based on the `Topic.Id` of the root `Topic` as well as the `T` argument of the type. Because of the mechanics of the `HierarchicalTopicMappingService<T>`, this cannot simply use the `CachedTopicMappingService` for caching, since each tier of navigation is mapped independently. This is necessary to apply the above business logic to the hierarchy, but makes it impossible for the `CachedTopicMappingService` to understand whether each request is suitable for caching. 

> *Note:* As with the `CachedTopicMappingService`, the `CachedHierarchicalTopicMapping` service should be used with caution. It will not be (immediately) updated if the underlying database or topic graph are updated. And since the topic graph is already cached, it effectively doubles the memory footprint of the graph by storing it both as topics as well as view models. That said, this is useful for large view model graphs that are frequently reused—such as those that show up in the navigation of a site.

### Example
The first code block demonstrates how to construct a new instance of a `IHierarchicalTopicMappingService<T>`. In this case, it wraps the default `HierarchicalTopicMappingService<T>` in a `CachedHierarchicalTopicMappingService<T>` for caching, and maps children to the `NavigationTopicViewModel` class from the [`Ignia.Topics.ViewModels`](../../Ignia.Topics.ViewModels/) project. Typically, this would be done in the _Composition Root_ of an application, with the service passed into e.g. a `Controller` as an `IHierarchicalTopicMappingService<T>` dependency.
```
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

```
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
