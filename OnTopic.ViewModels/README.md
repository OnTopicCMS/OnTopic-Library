# Topic View Models
The `OnTopic.ViewModels` assembly includes default implementations of basic view models which map to the stock content types that ship with OnTopic. These can optionally be used or extended by client implementations.

[![OnTopic.ViewModels package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/b22ec8a0-3966-4dc8-8bf5-69e6264dabd1/Badge)](https://www.nuget.org/packages/OnTopic.ViewModels/)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

> *Note:* It is not necessary to use or derive from these view models. They are provided exclusively for convenience so implementers don't need to recreate basic data models.

### Contents
- [Installation](#installation)
- [Inventory](#inventory)
- [Usage](#usage)
  - [`DynamicTopicViewModelLookupService`](#DynamicTopicViewModelLookupService) 
- [Design Considerations](#design-considerations)
  - [Parameterless Constructor](#parameterless-constructor)
  - [Inheritance](#inheritance)

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.ViewModels` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.ViewModels" Version="5.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).

## Inventory
- [`TopicViewModel`](TopicViewModel.cs)
  - [`PageTopicViewModel`](_contentTypes/PageTopicViewModel.cs)
    - [`ContentListTopicViewModel`](_contentTypes/ContentListTopicViewModel.cs) ([`ContentItemTopicViewModel`](_items/ContentItemTopicViewModel.cs))
    - [`IndexTopicViewModel`](_contentTypes/IndexTopicViewModel.cs)
    - [`SlideshowTopicViewModel`](_contentTypes/SlideshowTopicViewModel.cs) ([`SlideTopicViewModel`](_items/SlideTopicViewModel.cs))
    - [`VideoTopicViewModel`](_contentTypes/VideoTopicViewModel.cs)
  - [`SectionTopicViewModel`](_contentTypes/SectionTopicViewModel.cs)
    - [`PageGroupTopicViewModel`](_contentTypes/PageGroupTopicViewModel.cs)
  - [`NavigationTopicViewModel`](NavigationTopicViewModel.cs) 
  - [`ItemTopicViewModel`](_items/ItemTopicViewModel.cs)
    - [`ContentItemTopicViewModel`](_items/ContentItemTopicViewModel.cs)
    - [`LookupListItemTopicViewModel`](_items/LookupListItemTopicViewModel.cs)
    - [`SlideTopicViewModel`](_items/SlideTopicViewModel.cs)
- [`AssociatedTopicBindingModel`](BindingModels/AssociatedTopicBindingModel.cs)
- [`TopicViewModelLookupService`](TopicViewModelLookupService.cs)
- [`TopicViewModelCollection<>`](_collections/TopicViewModelCollection.cs)

## Usage
By default, the [`OnTopic.AspNetCore.Mvc`](../OnTopic.AspNetCore.Mvc/README.md)'s [`TopicController`](../OnTopic.AspNetCore.Mvc/Controllers/TopicController.cs) uses the out-of-the-box [`TopicMappingService`](../OnTopic/Mapping) to map topics to view models. For applications primarily relying on the out-of-the-box view models, it is recommended that the [`TopicViewModelLookupService`](TopicViewModelLookupService.cs) be used; this includes all of the out-of-the-box view models, and can be derived to add application-specific view models.

### `DynamicTopicViewModelLookupService`
For applications with a large number of view models, it may be preferable to use the `DynamicTopicViewModelLookupService`, which will attempt to map topics to view models from any assembly or namespace based on the naming convention, `{ContentType}TopicViewModel` or `{ContentType}ViewModel`. If a reference to the `OnTopic.ViewModels` package is included in a project's `csproj`, then these view models will be available to the lookup service and, thus, the mapping service. If any classes with the same name are available in _any other assembly or namespace_ then they will override the `ViewModels` from this assembly. That allows these classes to be treated as default fallbacks.

> *Note:* If a base class is overwritten, topics that derive from the original version will continue to do so unless they are _also_ overwritten. For example, if a `Theme` property is added to a customer-specific `PageTopicViewModel`, the `Theme` property won't be available on e.g. `SlideShowTopicViewModel` unless it is _also_ overwritten by the customer to inherit from their custom `PageTopicViewModel`.

## Design Considerations
As view models, not all attributes and associations are exposed. The properties chosen are optimized around values that are expected to be of common interest to most views.

### Parameterless Constructor
All of the view models assume a parameterless constructor (e.g., `new TopicViewModel()`), which can optionally be the default constructor if no other constructors are required. This is necessary to provide compatibility with the `TopicMappingService`, which will attempt to create new instances of view models based on the the topic's `ContentType`, using the view models parameterless constructor.

### Inheritance
The view models map to the hierarchy of the content types in OnTopic, with each view model only including properties that are _specific_ to that content type. So, for example, [`PageTopicViewModel`](_contentTypes/PageTopicViewModel.cs) includes a `Body` property, which is introduced by the `Page` content type, but doesn't include e.g. `Key`, `ContentType`, or `Title`; these are all inherited from the base [`TopicViewModel`](TopicViewModel.cs).

This is advantageous not only because it effectively models the familiar content type hierarchy, but also because it allows for polymorphism in the mapping library. So, for example, if a property accepts a `Collection<PageTopicViewModel>`, then this can also contain any view models that derive from the `PageTopicViewModel` (e.g., `SlideshowTopicViewModel`, `VideoTopicViewModel`, &c.).