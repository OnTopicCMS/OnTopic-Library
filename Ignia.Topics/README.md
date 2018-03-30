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

## Implementations
- **[`TopicMappingService`](Mapping)**: A default implementation of the `ITopicMappingService`, with built-in conventions that should address that majority of mapping requirements. This also includes a number of attributes for annotating view models with hints that the `TopicMappingService` can use in populating target objects.

## Extension Methods
- **[`Querying`](Querying/Topic.cs)**: The `Topic` class exposes optional extension methods for querying a topic (and its descendants) based on attribute values. 

## Collections
In addition to the above key classes, the `Ignia.Topics` assembly contains a number of specialized collections. These include:
- **[`TopicCollection{T}`](Collections/TopicCollection{T}.cs)**: A `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`TopicCollection`](Collections/TopicCollection.cs)**: A `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
    - **[`NamedTopicCollection`](Collections/NamedTopicCollection.cs)**: Proviedes a unique name to a `TopicCollection` so it can be keyed as part of a collection-of-collections.
- **[`ReadOnlyTopicCollection{T}`](Collections/ReadOnlyTopicCollection{T}.cs)**: A read-only `KeyedCollection` of a `Topic` (or derivative) keyed by `Id` and `Key`.
  - **[`ReadOnlyTopicCollection`](Collections/ReadOnlyTopicCollection.cs)**: A read-only `KeyedCollection` of `Topic` keyed by `Id` and `Key`.
- **[`RelatedTopicCollection`](Collections/RelatedTopicCollection.cs)**: A `KeyedCollection` of `NamedTopicCollection` objects, keyed by `Name`, thus providing a collection-of-collections. 
- **[`AttributeValueCollection`](collections/AttributeValueCollection.cs)**: A `KeyedCollection` of `AttributeValue` instances keyed by `AttributeValue.Key`.

### Editor
The following are intended to provide support for the Editor domain objects, `ContentTypeDescriptor` and `AttributeDescriptor`. 
- **[`ContentTypeDescriptorCollection`](Collections/ContentTypeDescriptorCollection.cs)**: A `KeyedCollection` of `ContentTypeDescriptor` objects keyed by `Id` and `Key`.
- **[`AttributeDescriptorCollection`](Collections/AttributeDescriptorCollection.cs)**: A `KeyedCollection` of `AttributeDescriptor` objects keyed by `Id` and `Key`.
  