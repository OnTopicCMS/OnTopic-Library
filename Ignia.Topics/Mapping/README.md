# Topic Mapping Service
The [`ITopicMappingService`](ITopicMappingService.cs) provides the abstract interface for a service that maps `Topic` entities to any arbitrary data transfer object. It is intended primarily to aid in mapping the `Topic` entity to view model instances, such as the ones provided in the [`ViewModels` assembly](../../Ignia.Topics.ViewModels)

## `TopicMappingService`
The [`TopicMappingService`](TopicMappingService.cs) provides a concrete implementation that is expected to satisfy the requirements of most consumers. This supports the following conventions.

- The view model is either explicitly provided to the `ITopicMappingService.Map<>()` method, or is of the naming convention `_ContentType_TopicViewModel`.
- Property is a scalar value of type `bool`, `int`, `string`, or `DateTime` and maps to either an attribute (via `topic.Attributes.GetAttribute(propertyName)`) or a getter method (`GetPropertyName()`) of the same name. 
- Property is a collection whose name maps to an outgoing relationship, incoming relationship, nested topic set, or the children collection.
- Property is named `Parent` and references the parent view model.

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

