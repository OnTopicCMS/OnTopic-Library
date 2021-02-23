# Topic Mapping Service
The [`ITopicMappingService`](ITopicMappingService.cs) defines a service for mapping `Topic` entities to any arbitrary model class. It is intended primarily to aid in mapping the `Topic` entity to view model instances, such as the ones provided in the [`ViewModels`](../../OnTopic.ViewModels/README.md) package.

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
  - [ReverseTopicMappingService](#reversetopicmappingservice)
  - [Example](#example-1)
- [Polymorphism](#polymorphism)
  - [Filtering](#filtering)
  - [Topics](#topics)
- [Caching](#caching)
  - [Internal Caching](#internal-caching)
  - [`CachedTopicMappingService`](#cachedtopicmappingservice)
    - [Limitations](#limitations)
- [Exceptions](#exceptions)

## `TopicMappingService`
The [`TopicMappingService`](TopicMappingService.cs) provides a concrete implementation that is expected to satisfy the requirements of most consumers. This supports the following conventions.

### Target Object
- The data transfer object type can be explicitly specified via the `ITopicMappingService.Map<>()` method.
  - E.g., `topicMappingService.Map<TopicViewModel>(topic)` will map the `topic` to the `TopicViewModel` class.
- The data transfer object can also be assigned by convention via the `ITopicMappingService.Map()` method.
  - The target type must be named `{ContentType}TopicViewModel` or `{ContentType}ViewModel`, based on the `ContentType` of the source `Topic`.
  - E.g., If the source topic's `ContentType` is "Example", it will look for `ExampleTopicViewModel` or `ExampleViewModel`.

> *Note:* Data transfer objects _must_ have a parameterless constructor so that either of the above methods can dynamically construct a new instance of the model classes.

### Properties
The mapping service will automatically attempt to map any properties on a model to values on the source `Topic`. To do so, it uses the following conventions:

#### Scalar Values
If a property is of the type `bool`, `int`, `string`, or `DateTime`, then:
- It will pull the value from a parameterless getter method with the same name.
- It will pull the value from a property of the same name.
- Otherwise, it will pull the value from the `topic.Attributes.GetValue()` method.

For example, if a property on a view model is named `Author`, it will automatically look for, in order:
- `topic.GetAuthor()`
- `topic.Author`
- `topic.Attributes.GetValue("Author")`

#### Collections
If a property implements `IList` (e.g., `List<>`, `Collection<>`, `TopicViewModelCollection<>`), then:
- It will pull the value from a collection with the same name as the property.
- If the property is explicitly named `Children`, then it will load the `topic.Children`.
- It will search, in order, `topic.Relationships`, `topic.IncomingRelationships`, and finally `topic.Children`.
- E.g., If a `List<>` property is named `Cousins` then it might match `topic.Relationships.GetValues("Cousins")`.

#### References
Topic references relate a single topic to another topic by key. If a property corresponds to the key of a topic reference, and that `Topic` maps to an object that is assignable to the original property, then the `Topic` will be loaded, mapped, and assigned to that property. For instance, the following property:
```csharp
public AuthorViewModel Author { get; set; }
```
Would be mapped to an `AuthorTopicViewModel` if `topic.References.GetValue("Author")` returns a `Topic` with a `ContentType` set to `Author`.

> *_Note_*: For backward compatibility, topic references can also be created from attributes following the `{Property}Id` nomenclature—e.g., `topic.Attributes.GetValue("AuthorId")`. Implementors should prefer `topic.References` instead.

#### Parent
If a property is named `Parent`, then the `TopicMappingService` will pull the value from `topic.Parent`. This acts as a special version of a [Topic Reference](#references).

> *Note:* By default, associations to other topics stored in collections or reference properties of associated topics will not be pulled. For instance, if a `TopicViewModel` has a `Children` collection, then the relationships, references, nested topics, and children of those instances will not be populated. This is meant to constrain the size of the object graph delivered.

### Example
The following is an example of a view model that `TopicMappingService` might consume:
```csharp
public class CustomViewModel {
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
- `Cousins`: A relationship or nested topic set named `Cousins`, with each association mapped to a `TopicViewModel`.
- `NestedTopics`: A relationship or nested topic set named `NestedTopics` (the name doesn't have any special meaning).

## Attributes
To fine-tune the mapping process, a variety of `[Attribute]`s are provided for decorating models:

- **`[Validation]`**: Enforces the rules established by any of the [`ValidationAttribute`](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.validationattribute%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396) subclasses.
  - This includes e.g., `[Required]`, `[MaxLength()]`, `[RegularExpression()]`, &c.
- **`[DefaultValue()]`**: Sets the default value for the property, if one isn't explicitly provided.
- **`[Inherit]`**: If the corresponding call comes back `null`, checks parent topics until a value is found.
  - This is the equivalent of calling `topic.Attributes.GetValue(attributeKey, true)`.
- **`[Metadata(key)]`**: Populates a collection with a list of `LookupItem` values from `Root:Configuration:Metadata`.
  - This is useful for including a list of items to filter collections by.
  - E.g., If child objects have a `TopicLookup` referencing the `Countries` metadata collection.
- **`[AttributeKey(key)]`**: Instructs the `TopicMappingService` to use the specified `key` instead of the property name when calling `topic.Attributes.GetValue()` or `topic.References.GetValue()`.
- **`[FilterByAttribute(key, value)]`**: Ensures that all items in a collection have an attribute named `key` with a value of `value`; all others will be excluded. Multiple instances can be stacked together.
- **`[Collection(key, type)]`**: For a collection, optionally specifies the `key` to look for, instead of the property name, and the `CollectionType`, in case the `key` is ambiguous.
- **`[Include(associationTypes)]`**: Instructs the code to populate the specified associations on any view models within a collection, thus expanding the scope of the mapping process.
- **`[Flatten]`**: Includes all descendants for every item in the collection. If the collection enforces uniqueness, duplicates will be removed.
- **`[MapToParent(prefix)]`**: Allows the attributes from a single topic to be applied to a complex child view model, optionally mapping attributes that begin with the supplied `prefix`.
- **`[DisableMapping]`**: Prevents the mapping service from attempting to map the property to an attribute.

### Example
The following is an example of a model that implements the above attributes:
```csharp
public class CompanyViewModel {

  [DefaultValue("Ignia")]
  [MaxLength(100)]
  public string CompanyName { get; set; }

  [Inherit]
  [AttributeKey("IsHidden")]
  public bool HideFromDirectory { get; set; }

  [Metadata("Countries")]
  public TopicViewModelCollection<LookupListItemTopicViewModel> Countries { get; set; }

  [Collection("Companies", Type=CollectionType.IncomingRelationship)]
  [Include(AssociationTypes.Children)]
  public TopicViewModelCollection<CaseStudyTopicViewModel> CaseStudies { get; set; }

  [Include(AssociationTypes.Relationships)]
  public TopicViewModelCollection<TopicViewModel> Children { get; set; }

  [Collection("Employees", Type=CollectionType.NestedTopics)]
  [FilterByAttribute("IsActive", "1")]
  [FilterByAttribute("Role", "Account Manager")]
  [Flatten]
  public TopicViewModelCollection<EmployeeTopicViewModel> Contacts { get; set; }

}
```

In this example, the properties would map to:
- `CompanyName`: Would default to "Ignia" if not otherwise set; would throw an error if the value exceeded 100 characters.
- `HideFromDirectory`: Maps to an attribute named `IsHidden` or a method named `GetIsHidden()`. If `null`, will search the `Parent` topics.
- `Countries`: Loads all `LookupListItem` instances in the `Root:Configuration:Metadata:Countries` metadata collection.
- `CaseStudies`: A collection of `CaseStudy` topics pointing to the current company via an incoming `Companies` relationship. Will additionally load the children of each case study.
- `Children`: A collection of child topics. Will additionally load the relationships of each child topic.
- `Contacts`: A list of `Employee` nested topics, filtered by those with `IsActive` set to `1` (`true`) and `Role` set to `Account Manager`. Additionally includes any descendants of the nested topics that meet the previous criteria.

> *Note*: Often times, models won't require any attributes. These are only needed if the properties don't follow the built-in conventions and require additional hints. For instance, the `[Collection(…)]` attribute is useful if the collection key is ambiguous between outgoing relationships and incoming relationships.

## Polymorphism
If a reference type (e.g., `TopicViewModel Parent`) or a strongly-typed collection property (e.g., `List<TopicViewModel>`) is defined, then any target instances must be assignable by the base type (in these cases, `TopicViewModel`). If they cannot be, then they will not be included; no error will occur.

### Filtering
This can be useful for filtering a collection. For instance, if a `CompanyTopicViewModel` includes an `Employees` collection of type `List<ManagerTopicViewModel>` then it will only be populated by topics that can be mapped to either `ManagerTopicViewModel` or a derivative (perhaps, `ExecutiveTopicViewModel`). Other types (e.g., `EmployeeTopicViewModel`) will be excluded, even though they might otherwise be referenced by the `Employees` collection.

> *Note:* For this reason, it is recommended that view models use inheritance based on the content type hierarchy. This provides an intuitive mapping to content type definitions, avoids needing to redefine base properties, and allows for polymorphism in assigning derived types.

### Topics
While it's not a best practice, this also works for strongly-typed collections of `Topic` objects. Typically, collections should return view models, but if the collection is strongly typed to `Topic` (or a derivative) then the source `Topic` will not be mapped, and will be used as-is, assuming it implements (or derives from) the target `Topic` type. This can be useful for scenarios where a view needs full access to the object graph. In such cases, it is impractical to map the entirety of an object graph, along with all attributes, to a corresponding view model graph, and makes more sense to simply return the `Topic` graph.

## Caching
By default, the `TopicMappingService` will cache a reference to all `MemberInfo` objects associated with each of view model it maps. That mitigates much of the performance hit associated with the use of reflection. Despite that, simply setting properties—and, especially, on large object graphs—can require a lot of processing time. To address this, OnTopic also offers two approaches.

### Internal Caching
When a request is made to `TopicMappingService`, and internal cache is constructed. If any mapping requests refer to a `Topic` that's already been mapped as part of the _current_ object graph, then that object will be returned. This prevents unnecessary duplication of mapping, and also avoids the potential for infinite loops. For instance, if a view model includes `Children`, and those children are set to `[Include(AssociationTypes.Parents)]`, the `TopicMappingService` will point back to the originally-mapped `Parent` object, instead of mapping a new instance of that `Topic`. If the mapping calls to include additional associations that weren't originally mapped, then those will be added to the cahced instance.

### `CachedTopicMappingService`
The [`CachedTopicMappingService`](CachedTopicMappingService.cs) decorator, which accepts a concrete implementation of an `ITopicMappingService`, provides caching across requests based on `topic.Id`, `Type`, and `AssociationTypes`. Because the cache is based on all three of these, it will differentiate between the results of e.g.,
- `topicMappingService.Map<TopicViewModel>(topic, AssociationTypes.All)`
- `topicMappingService.Map<TopicViewModel>(topic, AssociationTypes.Children)`
- `topicMappingService.Map<PageTopicViewModel>(topic, AssociationTypes.Children)`

To implement the caching decorator, use the following construction as a singleton lifestyle in your composer:
```csharp
var topicRepository = new SqlTopicRepository(…);
var topicMappingService = new TopicMappingService(topicRepository);
var cachedTopicMappingService = new CachedTopicMappingService(topicMappingService);
```

> _**Important**_: Due to limitations discussed below, the application of the `CachedTopicMappingService` is quite restricted. It is likely inapprorpiate for page content, since that won't reflect changes made via the editor. 


#### Limitations
While the `CachedTopicMappingService` can be useful for particular scenarios, it introduces several limitations that should be accounted for.

1. It may take up considerable memory, depending on how many permutations of mapped objects the application has. This is especially true since it caches each unique object graph; no effort is made to centralize object instances referenced by e.g. associations in multiple graphs.
2. It makes no effort to validate or evict cache entries. Topics whose values change during the lifetime of the `CachedTopicMappingService` will not be reflected in the mapped responses.
3. If a graph is manually constructed (by e.g. programmatically mapping `Children`) then the first instance of a topic will be cached independent of the parent graph, thus potentially allowing it to be shared between multiple graphs. This can introduce concerns if edge maintenance is important (e.g., one instance should include children, while another does not).

## Exceptions
The topic mapping services will throw a [`TopicMappingException`](_exceptions/TopicMappingException.cs) if a foreseeable exception occurs. Specifically, the exceptions expected will be:
- **[`InvalidTypeException`](_exceptions/InvalidTypeException.cs):** The [`TopicMappingService`](TopicMappingService.cs) throws this exception if the source `Topic`'s `ContentType` maps to a `TopicViewModel` which cannot be located in the supplied `ITypeLookupService`.
- **[`MappingModelValidationException`](_exceptions/MappingModelValidationException.cs):** The [`ReverseTopicMappingService`](Reverse/ReverseTopicMappingService.cs) throws this exception if the source model has any discrepancies with the target `Topic` which may introduce unexpected data integrity or data loss once that `Topic` is saved.