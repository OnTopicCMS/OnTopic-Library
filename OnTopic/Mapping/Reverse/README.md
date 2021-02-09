# Reverse Topic Mapping Service
The [`IReverseTopicMappingService`](IReverseTopicMappingService.cs) and its concrete implementation, [`ReverseTopicMappingService`](ReverseTopicMappingService.cs), provide handling for mapping data transfer objects (and typing binding models) _back_ to topics. Generally, it follows similar conventions as the [`ITopicMappingService`](../README.md) and honors most of the same attribute hints, albeit with some important restrictions and limitations outlined below.

### Contents
- [Model Validation](#model-validation)
- [`ITopicRepository` Integration](#itopicrepository-integration)
- [Mapping Hierarchies](#mapping-hierarchies)
- [Complex Models](#complex-models)

## Interfaces
Unlike the `TopicMappingService`, the `ReverseTopicMappingService` will not map any plain-old C# object (POCO); binding models must implement the `ITopicBindingModel` interface, which requires a `Key` and `ContentType` property; without these, it can't create a new `Topic` instance. For associations, it expects implementation of the `IAssociatedTopicBindingModel`, which has a single `UniqueKey` property.

## Model Validation
The `ReverseTopicMappingService` is deliberately more conservative than the `TopicMappingService` since assumptions in mapping won't just impact a view, but will be committed to the database.  As a result, all properties are validated against the corresponding `ContentTypeDescriptor`'s `AttributeDescriptors` collection. If a property cannot be mapped back to an attribute, a `MappingModelValidationException` is thrown.

> _Important:_ If a binding model contains properties that are not intended to be mapped, they must explicitly be excluded from mapping using the `[DisableMapping]` attribute.

## `ITopicRepository` Integration
While the `ReverseTopicMappingService` maintains a dependency on the `ITopicRepository`, it makes no effort to look up and map to an existing topic. If an existing `Topic` is passed in, it will map to it. Otherwise, it will always return a new `Topic`. It is up to the controller to set the parent topic, if appropriate, and call `ITopicRepository.Save()`. This allows more flexibility, and avoids potential security issues, by allowing the caller to explicitly control what topics are modified based on its own business logic.

## Mapping Hierarchies
Because the `ReverseTopicMappingService` doesn't map directly to the `ITopicRepository`, it makes no effort to crawl a hierarchy of binding models. If this is needed, the calling code can iterate over the hierarchy, determining how best to handle e.g. new v. existing topics. That said, this is generally not expected to be needed, since form pages will usually only model a single topic.

## Complex Models
The `ReverseTopicMappingService` allows complex models with nested objects to be mapped to a single `Topic` by using the `[MapToParent]` attribute. By default, any properties on a complex property will be prefixed with the property name. This prefix can be modified—or even removed—by passing an `AttributePrefix` argument to `[MapToParent]`. For example:
```csharp
public class ContentBindingModel: ITopicBindingModel {
  public string Key { get; set; }
  public string ContentType { get; set; }
  [MapToParent(AttributePrefix="Billing")]
  public AddressBindingModel BillingContact { get; set; }
}
```
In this case, a `City` property on the `AddressBindingModel` would attempt to bind to a `BillingCity` attribute on the target `Topic`. If the `[MapToParent]` attribute is not present, then properties with complex types are ignored.
