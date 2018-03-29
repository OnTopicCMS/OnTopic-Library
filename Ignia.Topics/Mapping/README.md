# Topic Mapping Service
The [`ITopicMappingService`](ITopicMappingService.cs) provides the abstract interface for a service that maps `Topic` entities to any arbitrary data transfer object. It is intended primarily to aid in mapping the `Topic` entity to view model instances, such as the ones provided in the [`ViewModels` assembly](../../Ignia.Topics.ViewModels)

## `TopicMappingService`
The [`TopicMappingService`](TopicMappingService.cs) provides a concrete implementation that is expected to satisfy the requirements of most consumers. This supports the following conventions.

- The data transfer object is either explicitly provided to the `ITopicMappingService.Map<>()` method, or is of the naming convention `ContentTypeTopicViewModel` (where `ContentType` is the `ContentType` of the source `Topic`).
  - E.g., If the source topic's `ContentType` is "Example", it will look for `ExampleTopicViewModel`.
- A property is a scalar value of type `bool`, `int`, `string`, or `DateTime` and maps to either an attribute (via `topic.Attributes.GetAttribute(propertyName)`) or a getter method (`GetPropertyName()`) of the same name. 
  - E.g., If a property is named `Author`, it will call `topic.Attributes.GetValue("Author")` or call a `GetAuthor()` method, if available.
- A property is a collection whose name maps to an outgoing relationship, incoming relationship, nested topic set, or the children collection.
  - E.g., If a `List<>` property is named `Cousins` then it would match, for instance, `topic.Relationships.GetTopics("Cousins")`, if available.
- A property is named `Parent` and references a view model; this will be populated with a reference to the parent topic.

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
- `Parent`: The parent `Topic`.
- `CustomPropertyA`: An attribute named `CustomPropertyA` or a method named `GetCustomPropertyA()`.
- `CustomPropertyB`: An attribute named `CustomPropertyB` or a method named `GetCustomPropertyB()`.
- `LastModified`: An attribute named `LastModified` or a property named `GetLastModified`.
- `Children`: The `Children` collection.
- `Cousins`: A relationship or nested topic set with named `Cousins`.
- `NestedTopics`: A relationship or nested topic set named `NestedTopics`.

## Polymorphism
If a reference type (e.g., `TopicViewModel Parent`) or a strongly-typed collection property (e.g., `List<TopicViewModel>`) are defined, then all any target instances must be assignable by the base type (in these cases, `TopicViewModel`). If they cannot be, then they will not be included. 

This can be useful for filtering a collection. For instance, if a `CompanyTopicViewModel` includes an `Employees` collection of type `List<ManagerTopicViewModel>` then it will only be populated by topics that can be mapped to either `ManagerTopicViewModel` or a derivative (say, `ExecutiveTopicViewModel`). Other types (e.g., `EmployeeTopicViewModel`) will be excluded, even though they might otherwise be referenced by the relationship.

> *Note:* For this reason, it is recommended that view models use inheritance based on the content type hierarchy. This provides an intuitive mapping to content type definitions, avoids needing to redefine base properties, and allows for polymorphism in assigning derived types.

## Attributes 
To support the mapping, a variety of `Attribute` classes are provided for decorating data transfer objects.

- `[Validation]`: Ensures the property value follows the rules established by any of the [`ValidationAttribute`](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.validationattribute%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396) subclasses (e.g., `[Required]`, `[MaxLength()]`, `[RegularExpression()]`, &c.)
- `[DefaultValue()]`: Sets the default value for the property.
- `[Inherit]`: If the corresponding call to `topic.Attributes.GetValue()` comes back null, checks parent topics until a value is found.
- `[Metadata(key)]`: Populates a collection with a list of `LookupItem` values from the `Root:Configuration:Metadata` namespace.
- `[AttributeKey(key)]`: Instructs the `TopicMappingService` to use the specified `key` instead of the property name when calling `topic.Attributes.GetValue()`.
- `[FilterByAttribute(key, value)]`: Ensures that all items in a collection have an attribute named "Key" with a value of "Value"; all else will be excluded. Multiple instances can be stacked.
- `[Relationship(key, type)]`: For a collection, optionally specifies the name of the key to look for, instead of the property name, and the relationship type, in case the key name is ambiguous.
- `[Recurse(relationships)]`: Instructs the code to populate the specified relationships on any view models within a collection. 

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

  [Relationship("Companies", RelationshipType.IncomingRelationship)]
  [Recurse(Relationships.Children)]
  public TopicViewModelCollection<CaseStudyTopicViewModel> CaseStudies { get; set; }

  [Recurse(Relationships.Relationships)]
  public TopicViewModelCollection<TopicViewModel> Children { get; set; }

  [Relationship("Employees", RelationshipType.NestedTopics)]
  [FilterByAttribute("IsActive", "1")]
  [FilterByAttribute("Role", "Account Manager")]
  public TopicViewModelCollection<EmployeeTopicViewModel> Contacts { get; set; }

}
```

In this example, the properties would map to:
- `CompanyName`: Would default to "Ignia" if not otherwise set; would throw an error if the value exceeded 100 characters.
- `HideFromDirectory`: An attribute named `IsHidden` or a method named `GetIsHidden()`. If null, will look in `Parent` topics.
- `Countries`: Loads all `LookupListItem` instances in the `Root:Configuration:Metadata:Countries` metadata collection.
- `CaseStudies`: A collection of `CaseStudy` topics pointing to the current `Company` via a "Companies" relationship. Will load the children of each case study.
- `Children`: A collection of child topics, with all relationships loaded. 
- `Contacts`: A list of `Employee` nested topics, filtered by those with `IsActive` set to `1` (`true`) and `Role` set to "Account Manager".

> *Note*: Often times, data transfer objects won't require any attributes. These are only needed if the properties don't follow the built-in conventions and require additional help. For instance, the `[Relationship(…)]` attribute is useful if the relationship key is ambigious between outgoing relationships and incoming relationships. 

