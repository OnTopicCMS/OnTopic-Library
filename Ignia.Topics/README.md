# `Ignia.Topics`
The `Ignia.Topics` assembly represents the core domain layer of the OnTopic library. It includes the primary entity ([`Topic`](Topic.cs)), abstractions (e.g., [`ITopicRepository`](Repositories/ITopicRepository.cs)), and associated classes (e.g., [`TopicCollection<>`](Collections/TopicCollection{T}.cs)).

## Entities
- **[`Topic`](Topic.cs)**: This is the core business object in OnTopic.

> *Note*: Any class that derives from `Topic` and is named `{ContentType}Topic` will automatically be loaded by [`TopicFactory.Create()`](TopicFactory.cs), thus allowing content type specific business logic to be added to any `Topic` instance.

### Editor
Out of the box, the OnTopic library contains two specially derived topics for supporting core infrastructure requirements:
- **[`ContentTypeDescriptor`](ContentTypeDescriptor.cs)**: A `ContentTypeDescriptor` is composed of multiple `AttributeDescriptor` instances which describe the schema of a content type. This is primarily used by editors. 
- **[`AttributeDescriptor`](AttributeDescriptor.cs)**: An `AttributeDescriptor` describes a single attribute on a `ContentTypeDescriptor`. This includes the `AttributeType`, `Description`, `DisplayGroup`, and whether or not it's required (`IsRequired`).

## Key Abstractions
- **[`ITopicRoutingService`](ITopicRoutingService.cs)**: Given contextual information, such as a URL and routing information, will identify the current `Topic` instance. What contextual information is required is environment-specific; for instance, the `MvcTopicRoutingService` requires an `ITopicRepository`, `Uri`, and `RouteData` collection.
- **[`ITopicRepository`](Repositories/ITopicRepository.cs)**: Defines the data access layer interface, with `Load()`, `Save()`, `Delete()`, and `Move()` methods.
- **[`ITopicMappingService`](Mapping)**: Defines the interface for a service that can convert a `Topic` class into any arbitrary data transfer object based on predetermined conventions.
- **[`ITypeLookupService`](ITypeLookupService.cs)**: Defines the interface that can identify `Type` objects based on a `GetType(typeName)` query. Used by e.g. `ITopicMappingService` to find corresponding `TopicViewModel` classes to map to.

## Implementations
- **[`TopicMappingService`](Mapping)**: A default implementation of the `ITopicMappingService`, with built-in conventions that should address that majority of mapping requirements. This also includes a number of attributes for annotating view models with hints that the `TopicMappingService` can use in populating target objects.
- **[`StaticTypeLookupService`](StaticTypeLookupService.cs)**: A basic implementation of the `ITypeLookupService` interface that allows types to be explicitly registered; useful when a small number of types are expected.
  - **[`DynamicTypeLookupService`](Reflection\DynamicTypeLookupService.cs)**: A reflection-based implementation of the `ITypeLookupService` interface that looks up types from all loaded assemblies based on a `Func<Type, bool>` delegate.
    - **[`TopicLookupService`](Reflection\TopicLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that derive from `Topic`; this is the default implementation for `TopicFactory`.
    - **[`TopicViewModeLookupService`](Reflection\TopicViewModeLookupService.cs)**: A version of `DynamicTypeLookupService` that returns all classes that end with `TopicViewModel`; this is useful for the `TopicMappingService`.

## Extension Methods
- **[`Querying`](Querying/Topic.cs)**: The `Topic` class exposes optional extension methods for querying a topic (and its descendants) based on attribute values. This includes the useful `Topic.FindAll(Func<Topic, bool>)` method for querying an entire topic graph and returning topics validated by a predicate.

## Collections
In addition to the above key classes, the `Ignia.Topics` assembly contains a number of specialized collections. These include:
- **[`TopicCollection{T}`](Collections/TopicCollection{T}.cs)**: A `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`TopicCollection`](Collections/TopicCollection.cs)**: A `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
    - **[`NamedTopicCollection`](Collections/NamedTopicCollection.cs)**: Provides a unique name to a `TopicCollection` so it can be keyed as part of a collection-of-collections.
- **[`ReadOnlyTopicCollection{T}`](Collections/ReadOnlyTopicCollection{T}.cs)**: A read-only `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`ReadOnlyTopicCollection`](Collections/ReadOnlyTopicCollection.cs)**: A read-only `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
- **[`RelatedTopicCollection`](Collections/RelatedTopicCollection.cs)**: A `KeyedCollection` of `NamedTopicCollection` objects, keyed by `Name`, thus providing a collection-of-collections. 
- **[`AttributeValueCollection`](collections/AttributeValueCollection.cs)**: A `KeyedCollection` of `AttributeValue` instances keyed by `AttributeValue.Key`.

### Editor
The following are intended to provide support for the Editor domain objects, `ContentTypeDescriptor` and `AttributeDescriptor`. 
- **[`ContentTypeDescriptorCollection`](Collections/ContentTypeDescriptorCollection.cs)**: A `KeyedCollection` of `ContentTypeDescriptor` objects keyed by `Id` and `Key`.
- **[`AttributeDescriptorCollection`](Collections/AttributeDescriptorCollection.cs)**: A `KeyedCollection` of `AttributeDescriptor` objects keyed by `Id` and `Key`.

## View Models
The core Topic library has been designed to be view model agnostic; i.e., view models should be defined for the specific presentation framework (e.g., ASP.NET MVC) and customer. That said, to facilitate reusability of features that work with view models, several interfaces are defined which can be applied as appropriate. These include:
- **[`ITopicViewModel`](ViewModels/ITopicViewModel.cs)**: Includes universal properties such as `Key`, `Id`, and `ContentType`.
- **[`IPageTopicViewModel`](ViewModels/IPageTopicViewModel.cs)**: Includes page-specific properties such as `Title`, `MetaKeywords`, and `WebPath`.
- **[`INavigationTopicViewModel<T>`](ViewModels/INavigationTopicViewModel{T}.cs)**: Includes `IPageTopicViewModel`, `Children`, and an `IsSelected()` view logic handler, for use with navigation menus.
 