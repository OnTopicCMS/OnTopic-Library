# `TestDoubles`
Provides common test doubles for use in testing the **OnTopic Library**. 

> _Note:_ This package is primarily intended for use by other OnTopic libraries that benefit from sharing testing infrastructure; most OnTopic implementations should not require this package. 

[![OnTopic.TestDoubles package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/3a741b7a-7fa1-4bdb-bc55-efbac3f04e6c/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=3a741b7a-7fa1-4bdb-bc55-efbac3f04e6c&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

### Contents
- [Installation](#installation)
- [Inventory](#inventory)
  - [Dummies](#dummies)
  - [Stubs](#stubs) 

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.TestDoubles` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.TestDoubles" Version="5.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).

## Inventory

### Dummies
Dummies provide no actual functionality and are not expected to function correctly if called. They are simply provided to satisfy the inferface requirement of dependencies, so that a service can be constructed.

- [`DummyTopicMappingService`](DummyTopicMappingService.cs)
- [`DummyTopicRepository`](DummyTopicRepository.cs)

### Stubs
Stubs not only satisfy the interface, but will return canned data that tests can operate against, thus allowing unit tests to interact with predetermined scenarios against the service.

#### `StubTopicRepository`

The [`StubTopicRepository`](StubTopicRepository.cs) automatically generates an in-memory topic graph with the following structure:

- `Root` (`Container`)
  - `Configuration` (`Container`)
    - `ContentTypes` (`ContentTypeDescriptor`)
      - `ContentTypeDescriptor` (`ContentTypeDescriptor`)
      - `AttributeDescriptor` (`ContentTypeDescriptor`)  
        - `BooleanAttributeDescriptor` (`ContentTypeDescriptor`)
        - `NestedTopicListAttributeDescriptor` (`ContentTypeDescriptor`)
        - `NumberAttributeDescriptor` (`ContentTypeDescriptor`)
        - `RelationshipAttributeDescriptor` (`ContentTypeDescriptor`)
        - `TextAttributeDescriptor` (`ContentTypeDescriptor`)
        - `TopicReferenceAttributeDescriptor` (`ContentTypeDescriptor`)
      - `Page` (`ContentTypeDescriptor`)
      - `Contact` (`ContentTypeDescriptor`)
    - `Metadata` (`Container`)
      - `Categories` (`Lookup`)
  - `Web` (`Container`)
    - `Web_0` (`Page`)
      - `Web_0_0` (`Page`)
        - `Web_0_0_0` (`Page`)
          - `Web_0_0_0_0` (`Page`)
          - `Web_0_0_0_1` (`Page`)
        - `Web_0_0_1` (`Page`)
          - `Web_0_0_1_0` (`Page`)
          - `Web_0_0_1_1` (`Page`)
    - `Web_1` (`Page`)
      - `Web_1_0` (`Page`)
        - `Web_1_0_0` (`Page`)
          - `Web_1_0_0_0` (`Page`)
          - `Web_1_0_0_1` (`Page`)
        - `Web_1_0_1` (`Page`)
          - `Web_1_0_1_0` (`Page`)
          - `Web_1_0_1_1` (`Page`)
      - `Web_1_1` (`Page`)
        - `Web_1_1_0` (`Page`)
          - `Web_1_1_0_0` (`Page`)
          - `Web_1_1_0_1` (`Page`)
        - `Web_1_1_1` (`Page`)
          - `Web_1_1_1_0` (`Page`)
          - `Web_1_1_1_1` (`Page`)
    - `Web_3` (`PageGroup`)
      - `Web_3_0` (`Page`)
      - `Web_3_1` (`Page`)