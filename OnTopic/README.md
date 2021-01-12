# OnTopic Library

[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)
[![OnTopic package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/fb67677f-2b83-4318-9007-0c46b4da55c1/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=fb67677f-2b83-4318-9007-0c46b4da55c1&preferRelease=true)

The `OnTopic` assembly represents the core domain layer of the OnTopic library. It includes the primary entity ([`Topic`](Topic.cs)), abstractions (e.g., [`ITopicRepository`](Repositories/ITopicRepository.cs)), and associated classes (e.g., [`TopicCollection<>`](Collections/TopicCollection{T}.cs)).

### Contents
- [Entities](#entities)
  - [Editor](#editor)
- [Key Abstractions](#key-abstractions) 
- [Implementation](#implementation)
- [Extension Methods](#extension-methods)
- [Collections](#collections)
  - [Editor](#editor-1)
- [View Models](#view-models) 

## Entities
- **[`Topic`](Topic.cs)**: This is the core business object in OnTopic.

> *Note*: Any class that derives from `Topic` and is named `{ContentType}Topic` will automatically be loaded by [`TopicFactory.Create()`](TopicFactory.cs), thus allowing content type specific business logic to be added to any `Topic` instance.

### Editor
Out of the box, the OnTopic library contains two specially derived topics for supporting core infrastructure requirements:
- **[`ContentTypeDescriptor`](Metadata/ContentTypeDescriptor.cs)**: A `ContentTypeDescriptor` is composed of multiple `AttributeDescriptor` instances which describe the schema of a content type. This is primarily used by editors.
- **[`AttributeDescriptor`](Metadata/AttributeDescriptor.cs)**: An `AttributeDescriptor` describes a single attribute on a `ContentTypeDescriptor`. This includes the `AttributeType`, `Description`, `DisplayGroup`, and whether or not it's required (`IsRequired`).

> As of v4.0.0, there are additionally attribute _type_ descriptors which are derived from `AttributeDescriptor`, such as [`BooleanAttribute`](Metadata/AttributeTypes/BooleanAttribute.cs). Currently, these don't fully model their corresponding content types—that is done via view models in the OnTopic Editor. By deriving them in the `OnTopic` library, however, we ensure that they are still created as derivatives of `AttributeDescriptor`, and thus properly included anywhere that `AttributeDescriptor`s are used.

## Key Abstractions
- **[`ITopicRepository`](Repositories/ITopicRepository.cs)**: Defines the data access layer interface, with `Load()`, `Save()`, `Delete()`, `Move()`, and `Rollback()` methods.
- **[`ITopicMappingService`](Mapping/README.md)**: Defines interface for a service that can convert a `Topic` class into any arbitrary data transfer object based on predetermined conventions—or vice versa (via the `IReverseTopicMappingService`. 
- **[`IHierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: Defines an interface for applying the `ITopicMappingService` to hierarchical data with constraints on depth. Used primarily for mapping navigation, such as in the [`NavigationTopicViewComponentBase<T>`](../OnTopic.AspNetCore.Mvc/Components/NavigationTopicViewComponentBase{T}.cs).
- **[`ITypeLookupService`](ITypeLookupService.cs)**: Defines the interface that can identify `Type` objects based on a `GetType(typeName)` query. Used by e.g. `ITopicMappingService` to find corresponding `TopicViewModel` classes to map to.

## Implementations
- **[`TopicMappingService`](Mapping/README.md)**: A default implementation of the `ITopicMappingService`, with built-in conventions that should address that majority of mapping requirements. This also includes a number of attributes for annotating view models with hints that the `TopicMappingService` can use in populating target objects.
  - **[`CachedTopicMappingService`](Mapping/README.md)**: Provides an optional caching layer for the `TopicMappingService`—or any `ITopicMappingService` implementation.
- **[`ReverseTopicMappingService`](Mapping/Reverse/README.md)**: A default implementation of the `IReverseTopicMappingService`, honoring similar conventions and attribute hints as the `TopicMappingService`.
- **[`HierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: A default implementation of the `IHierarchicalTopicMappingService<T>`, which accepts an `ITopicMappingService` for mapping each individual node in the hierarchy.
  - **[`CachedHierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: Provides an optional caching layer for the `HierarchicalTopicMappingService`—or any `IHierarchicalTopicMappingService` implementation.
- **[`StaticTypeLookupService`](StaticTypeLookupService.cs)**: A basic implementation of the `ITypeLookupService` interface that allows types to be explicitly registered; useful when a small number of types are expected.
  - **[`DynamicTypeLookupService`](Reflection/DynamicTypeLookupService.cs)**: A reflection-based implementation of the `ITypeLookupService` interface that looks up types from all loaded assemblies based on a `Func<Type, bool>` delegate.
    - **[`DynamicTopicLookupService`](Reflection/DynamicTopicLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that derive from `Topic`; this is the default implementation for `TopicFactory`.
    - **[`DynamicTopicViewModeLookupService`](Reflection/DynamicTopicViewModelLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that end with `TopicViewModel`; this is useful for the `TopicMappingService`.
    - **[`DynamicTopicBindingModelLookupService`](Reflection/DynamicTopicBindingModelLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that implement `ITopicBindingModel`; this is useful for the `ReverseTopicMappingService`.

## Extension Methods
- **[`Querying`](Querying/TopicExtensions.cs)**: The `TopicExtensions` class exposes optional extension methods for querying a topic (and its descendants) based on attribute values. This includes the useful `Topic.FindAll(Func<Topic, bool>)` method for querying an entire topic graph and returning topics validated by a predicate.
- **[`Attributes`](Attributes/AttributeValueCollectionExtensions.cs)**: The `AttributeValueCollectionExtensions` class exposes optional extension methods for strongly typed access to the [`AttributeValueCollection`](Collections/AttributeValueCollection.cs). This includes e.g., `GetBooleanValue()` and `SetBooleanValue()`, which takes care of the conversion to and from the underlying `string`value type.

## Collections
In addition to the above key classes, the `OnTopic` assembly contains a number of specialized collections. These include:
- **[`TopicCollection{T}`](Collections/TopicCollection{T}.cs)**: A `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`TopicCollection`](Collections/TopicCollection.cs)**: A `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
    - **[`NamedTopicCollection`](Collections/NamedTopicCollection.cs)**: Provides a unique name to a `TopicCollection` so it can be keyed as part of a collection-of-collections.
- **[`ReadOnlyTopicCollection{T}`](Collections/ReadOnlyTopicCollection{T}.cs)**: A read-only `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`ReadOnlyTopicCollection`](Collections/ReadOnlyTopicCollection.cs)**: A read-only `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
- **[`TopicRelationshipMultiMap`](Collections/TopicRelationshipMultiMap.cs)**: A `KeyedCollection` of `NamedTopicCollection` objects, keyed by `Name`, thus providing a collection-of-collections.
- **[`AttributeValueCollection`](collections/AttributeValueCollection.cs)**: A `KeyedCollection` of `AttributeValue` instances keyed by `AttributeValue.Key`.

### Editor
The following are intended to provide support for the Editor domain objects, `ContentTypeDescriptor` and `AttributeDescriptor`.
- **[`ContentTypeDescriptorCollection`](Metadata/ContentTypeDescriptorCollection.cs)**: A `KeyedCollection` of `ContentTypeDescriptor` objects keyed by `Id` and `Key`.
- **[`AttributeDescriptorCollection`](Metadata/AttributeDescriptorCollection.cs)**: A `KeyedCollection` of `AttributeDescriptor` objects keyed by `Id` and `Key`.

## View Models
The core Topic library has been designed to be view model agnostic; i.e., view models should be defined for the specific presentation framework (e.g., ASP.NET MVC) and customer. That said, to facilitate reusability of features that work with view models, several interfaces are defined which can be applied as appropriate. These include:
- **[`ITopicViewModel`](Models/ITopicViewModel.cs)**: Includes universal properties such as `Key`, `Id`, and `ContentType`.
  - **[`IPageTopicViewModel`](Models/IPageTopicViewModel.cs)**: Includes page-specific properties such as `Title`, `MetaKeywords`, and `WebPath`.
- **[`INavigationTopicViewModel<T>`](Models/INavigationTopicViewModel{T}.cs)**: Includes `IPageTopicViewModel`, `Children`, and an `IsSelected()` view logic handler, for use with navigation menus.
- **[`ITopicBindingModel`](Models/ITopicBindingModel.cs)**: Includes the bare minimum properties—namely `Key` and `ContentType`—needed to support a binding model that will be consumed by the `IReverseTopicMappingService`.
- **[`IRelatedTopicBindingModel`](Models/IRelatedTopicBindingModel.cs)**: Includes the bare minimum properties—namely `UniqueKey`—needed to reference another topic on a binding model that will be consumed by the `IReverseTopicMappingService`.
