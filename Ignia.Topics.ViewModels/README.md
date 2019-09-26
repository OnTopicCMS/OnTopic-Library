# Topic View Models
The `Ignia.Topics.ViewModels` assembly includes default implementations of basic view models which map to the stock content types that ship with OnTopic. These can optionally be used or extended by client implementations.

> *Note:* It is not necessary to use or derive from these view models. They are provided exclusively for convenience so implementers don't need to recreate basic data models.

## Inventory
- [`TopicViewModel`](TopicViewModel.cs)
  - [`PageTopicViewModel`](PageTopicViewModel.cs)
    - [`ContentListTopicViewModel`](ContentListTopicViewModel.cs) ([`ContentItemTopicViewModel`](ContentItemTopicViewModel.cs))
    - [`IndexTopicViewModel`](IndexTopicViewModel.cs)
    - [`SlideshowTopicViewModel`](SlideshowTopicViewModel.cs) ([`SlideTopicViewModel`](SlideTopicViewModel.cs))
    - [`SlideTopicViewModel`](SlideTopicViewModel.cs)
    - [`VideoTopicViewModel`](VideoTopicViewModel.cs)
  - [`SectionTopicViewModel`](SectionTopicViewModel.cs)
    - [`PageGroupTopicViewModel`](PageGroupTopicViewModel.cs)
  - [`ItemTopicViewModel`](ItemTopicViewModel.cs)
    - [`ContentItemTopicViewModel`](ContentItemTopicViewModel.cs)
    - [`LookupListItemTopicViewModel`](LookupListItemTopicViewModel.cs)
- [`TopicViewModelLookupService`](TopicViewModelLookupService.cs)
- [`TopicViewModelCollection<>`](TopicViewModelCollection.cs)

## Usage
By default, the [`Ignia.Topics.Web.Mvc`](../Ignia.Topics.Web.Mvc)'s [`TopicController`](../Ignia.Topics.Web.Mvc/Controllers/TopicController.cs) uses the out-of-the-box [`TopicMappingService`](../Ignia.Topics/Mapping) to map topics to view models. For applications primarily relying on the out-of-the-box view models, it is recommended that the [`TopicViewModelLookupService`](TopicViewModelLookupService.cs) be used; this includes all of the out-of-the-box view models, and can be derived to add application-specific view models.

### `DynamicTopicViewModelLookupService`
For applications with a large number of view models, it may be preferable to use the `DynamicTopicViewModelLookupService`, which will attempt to map topics to view models based on the naming convention `{ContentType}TopicViewModel`, from any assembly or namespace. If the `Ignia.Topics.ViewModels.dll` is in an application's `/bin` directory then these view models will be available to the lookup service and, thus, the mapping service. If any classes with the same name are available in _any other assembly or namespace_ then they will override the `ViewModels`  from this assembly. That allows these classes to be treated as default fallbacks.

> *Note:* If a base class is overwritten then topics that derive from the original version will continue to do so unless they are _also_ overwritten. For example, if a `Theme` property is added to a customer-specific `PageTopicViewModel`, the `Theme` property won't be available on e.g. `SlideShowTopicViewModel` unless it is _also_ overwritten by the customer to inherit from their `PageTopicViewModel`.


## Design Considerations
As view models, not all attributes and relationships are exposed by the view models. The properties chosen are optimized around values that are expected to be of common interest to most views.

### Default Constructor
All of the view models assume a default constructor (e.g., `new TopicViewModel()`). This is necessary to provide compatibility with the `TopicMappingService` which will attempt to create new instances of view models based on the default constructor.

### Inheritance
The view models map to the hierarchy of the content types in OnTopic, with each view model only including properties that are _specific_ to that content type. So, for example, [`PageTopicViewModel`](PageTopicViewModel.cs) includes a `Body` property, which is introduced by the `Page` content type, but doesn't include e.g. `Key`, `ContentType`, or `Title`; these are all inherited from the base [`TopicViewModel`](TopicViewModel.cs).

This is advantageous not only because it effectively models the familiar content type hierarchy, but also because it allows for polymorphism in the mapping library. So, for example, if a property accepts a `List<PageTopicViewModel>` then this can contain any view models that implement or derive from `PageTopicViewModel` (e.g., `SlideshowTopicViewModel`, `VideoTopicViewModel`, &c.).

