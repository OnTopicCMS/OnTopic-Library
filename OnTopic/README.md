# OnTopic Library

The `OnTopic` assembly represents the core domain layer of the OnTopic library. It includes the primary entity ([`Topic`](Topic.cs)), abstractions (e.g., [`ITopicRepository`](Repositories/ITopicRepository.cs)), and associated classes (e.g., [`KeyedTopicCollection<>`](Collections/KeyedTopicCollection{T}.cs)).

[![OnTopic package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/fb67677f-2b83-4318-9007-0c46b4da55c1/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=fb67677f-2b83-4318-9007-0c46b4da55c1&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

### Contents
- [Entities](#entities)
  - [Editor](#editor)
- [Key Abstractions](#key-abstractions) 
- [Implementation](#implementation)
- [Extension Methods](#extension-methods)
- [Collections](#collections)
  - [Specialty Collections](#specialty-collections)
  - [Editor](#editor-1)
- [View Models](#view-models) 

## Entities
- **[`Topic`](Topic.cs)**: This is the core entity in OnTopic, and models all attributes, relationships, and references associated with a topic record.

> *Note*: Any class that derives from `Topic` will automatically be loaded by [`TopicFactory.Create()`](TopicFactory.cs), thus allowing content type specific business logic to be added to any `Topic` instance. This is especially useful for custom `AttributeDescriptor` types used to extend the OnTopic Editor.

### Editor
Out of the box, the OnTopic library contains two specially derived topics for supporting core infrastructure requirements:
- **[`ContentTypeDescriptor`](Metadata/ContentTypeDescriptor.cs)**: A `ContentTypeDescriptor` is composed of multiple `AttributeDescriptor` instances which describe the schema of a content type. This is primarily used by the OnTopic Editor.
- **[`AttributeDescriptor`](Metadata/AttributeDescriptor.cs)**: An `AttributeDescriptor` describes a single attribute on a `ContentTypeDescriptor`. This includes the `AttributeType`, `Description`, `DisplayGroup`, and whether or not it's required (`IsRequired`).

> *Note*: In addition, the OnTopic Editor can be extended through types derived from `AttributeDescriptor`, such as the `BooleanAttributeDescriptor`. Plugins may optionally implement these in order to overwrite the `EditorType` or `ModelType` of the attribute, and thus determine where and how their values will be stored.

## Key Abstractions
- **[`ITopicRepository`](Repositories/ITopicRepository.cs)**: Defines an interface for data access, with `Load()`, `Save()`, `Delete()`, `Move()`, `Refresh()`, and `Rollback()` methods.
- **[`ITopicMappingService`](Mapping/README.md)**: Defines an interface for a service that can convert a `Topic` into any arbitrary data transfer object based on predetermined conventions—or vice versa (via the [`IReverseTopicMappingService`](Mapping/Reverse/IReverseTopicMappingService.cs)). 
- **[`IHierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: Defines an interface for applying the `ITopicMappingService` to hierarchical data with constraints on depth. Used primarily for mapping data for navigation components, such as the [`NavigationTopicViewComponentBase<T>`](../OnTopic.AspNetCore.Mvc/Components/NavigationTopicViewComponentBase{T}.cs).
- **[`ITypeLookupService`](lookup/ITypeLookupService.cs)**: Defines an interface that can identify `Type` objects based on a `Lookup(typeName)` query. Used by e.g. `ITopicMappingService` to find corresponding `TopicViewModel` classes to map to.

## Implementations
- **[`TopicMappingService`](Mapping/README.md)**: A default implementation of the `ITopicMappingService`, with built-in conventions that should address the majority of mapping requirements. This also includes a number of attributes for annotating view models with hints that the `TopicMappingService` can use to fine-tune the mapping process.
  - **[`CachedTopicMappingService`](Mapping/README.md)**: Provides an optional caching layer for decorating the `TopicMappingService`—or any `ITopicMappingService` implementation.
- **[`ReverseTopicMappingService`](Mapping/Reverse/README.md)**: A default implementation of the `IReverseTopicMappingService`, honoring similar conventions and attribute hints as the `TopicMappingService`. Useful for merging binding models with topics as part of form processing.
- **[`HierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: A default implementation of the `IHierarchicalTopicMappingService<T>`, which accepts an `ITopicMappingService` for mapping each individual node in the hierarchy.
  - **[`CachedHierarchicalTopicMappingService<T>`](Mapping/Hierarchical/README.md)**: Provides an optional caching layer for the `HierarchicalTopicMappingService`—or any `IHierarchicalTopicMappingService` implementation.
- **[`StaticTypeLookupService`](Lookup/StaticTypeLookupService.cs)**: A basic implementation of the `ITypeLookupService` interface that allows types to be explicitly registered; useful when a small number of well-known types are expected.
  - **[`DynamicTypeLookupService`](Lookup/DynamicTypeLookupService.cs)**: A reflection-based implementation of the `ITypeLookupService` interface that looks up types from all loaded assemblies based on a `Func<Type, bool>` delegate passed to the constructor.
    - **[`DynamicTopicLookupService`](Lookup/DynamicTopicLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that derive from `Topic`; this is the default implementation for `TopicFactory`.
    - **[`DynamicTopicViewModeLookupService`](Lookup/DynamicTopicViewModelLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that end with `ViewModel`; this is useful for the `TopicMappingService`.
    - **[`DynamicTopicBindingModelLookupService`](Lookup/DynamicTopicBindingModelLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that implement `ITopicBindingModel`; this is useful for the `ReverseTopicMappingService`.

## Extension Methods
- **[`Querying`](Querying/TopicExtensions.cs)**: The [`TopicExtensions`](Querying/TopicExtensions.cs) class exposes optional extension methods for querying a topic (and its descendants) based on attribute values. This includes the useful `Topic.FindAll(Func<Topic, bool>)` method for querying an entire topic graph and returning topics validated by a predicate. There are also specialty extensions for querying [`IEnumerable<Topic>`](Querying/TopicCollectionExtensions.cs).
- **[`Attributes`](Attributes/AttributeCollectionExtensions.cs)**: The `AttributeCollectionExtensions` class exposes optional extension methods for strongly typed access to the [`AttributeCollection`](Attributes/AttributeCollection.cs). This includes e.g., `GetBooleanValue()` and `SetBooleanValue()`, which takes care of the conversion to and from the underlying `string` value type.

## Collections
The `OnTopic` assembly contains a number of generic, keyed, and/or read-only collections for working with topics. These include:

|                               | Read-Write                            | Read-Only
| ----------------------------- | ------------------------------------- | -------------------------------------
| Unkeyed                       | [`TopicCollection`][1]                | [`ReadOnlyTopicCollection`][4]
| Keyed                         | [`KeyedTopicCollection`][2]           | [`ReadOnlyKeyedTopicCollection`][5]
| Keyed (Generic)               | [`KeyedTopicCollection<T>`][3]        | [`ReadOnlyKeyedTopicCollection`][6]

[1]: Collections/TopicCollection.cs
[2]: Collections/KeyedTopicCollection.cs
[3]: Collections/KeyedTopicCollection{T}.cs
[4]: Collections/ReadOnlyTopicCollection.cs
[5]: Collections/ReadOnlyKeyedTopicCollection.cs
[6]: Collections/ReadOnlyKeyedTopicCollection{T}.cs

### Specialty Collections
The `OnTopic.Collections.Specialized` namespace includes a number of collections that are used by the OnTopic library, but won't generally be used directly by implementors, except as exposed by the core library. These include:
- **[`TrackedRecordCollection{TItem, TValue, TAttribute}`](Collections/Specialized/TrackedRecordCollection{TItem,TValue,TAttribute}.cs)**: A `KeyedCollection<TItem, TValue>` of `TrackedRecord<TValue>` instances which tracks the `IsDirty` status and `DeletedItems`, while also enforcing business logic against corresponding properties on the associated `Topic`.
  - **[`AttributeCollection`](attributes/AttributeCollection.cs)**: A `TrackedRecordCollection` of [`AttributeRecord`](Attributes/AttributeRecord.cs) instances keyed by `AttributeRecord.Key`; exposed by `Topic.Attributes`.
  - **[`TopicReferenceCollection`](associations/TopicReferenceCollection.cs)**: A `TrackedRecordCollection` of [`TopicReferenceRecord`](Associations/TopicReferenceRecord.cs) instances keyed by `TopicReference.Key`; exposed by `Topic.References`.
- **[`TopicMultiMap`](Collections/Specialized/TopicMultiMap.cs)**: Provides a multi-map (or collection-of-collections) for topics organized by a collection key.
  - **[`ReadOnlyTopicMultiMap`](Collections/Specialized/ReadOnlyTopicMultiMap.cs)**: A read-only interface to the `TopicMultiMap`, thus allowing simple enumeration of the collection withouthout exposing any write access.
    - **[`TopicRelationshipMultiMap`](associations/TopicRelationshipMultiMap.cs)**: A `TopicMultiMap` of [`KeyValuesPair`](Collections/Specialized/KeyValuesPair.cs) instances keyed by `KeyValuesPair.Key`; exposed by `Topic.Relationships`.

### Editor
The following are intended to provide support for the Editor domain objects, `ContentTypeDescriptor` and `AttributeDescriptor`.
- **[`ContentTypeDescriptorCollection`](Metadata/ContentTypeDescriptorCollection.cs)**: A `KeyedCollection` of `ContentTypeDescriptor` objects keyed by `Id` and `Key`.
- **[`AttributeDescriptorCollection`](Metadata/AttributeDescriptorCollection.cs)**: A `KeyedCollection` of `AttributeDescriptor` objects keyed by `Id` and `Key`.

## View Models
The core Topic library has been designed to be view model agnostic; i.e., view models should be defined for the specific presentation framework (e.g., ASP.NET Core) and customer. That said, to facilitate reusability of features that work with view models, several interfaces are defined which can be applied as appropriate. These include:
- **[`ITopicViewModel`](Models/ITopicViewModel.cs)**: Includes universal properties such as `Key`, `UniqueKey`, `Id`, `ContentType`, and `Title`.
  - **[`IPageTopicViewModel`](Models/IPageTopicViewModel.cs)**: Includes page-specific properties such as `MetaKeywords` and `MetaDescription`.
- **[`INavigationTopicViewModel<T>`](Models/INavigationTopicViewModel{T}.cs)**: Includes `IPageTopicViewModel`, `Children`, and an `IsSelected()` view logic handler, for use with navigation menus.
- **[`ITopicBindingModel`](Models/ITopicBindingModel.cs)**: Includes the bare minimum properties—namely `Key` and `ContentType`—needed to support a binding model that will be consumed by the `IReverseTopicMappingService`.
- **[`IRelatedTopicBindingModel`](Models/IRelatedTopicBindingModel.cs)**: Includes the bare minimum properties—namely `UniqueKey`—needed to reference another topic on a binding model that will be consumed by the `IReverseTopicMappingService`.

In addition to these interfaces, a set of concrete implementations of view models corresponding to the default schemas for the out-of-the-box content types can be found in the [`OnTopic.ViewModels`](../OnTopic.ViewModels/README.md) package.