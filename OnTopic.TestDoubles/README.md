# `TestDoubles`
Provides common test doubles for use in testing the **OnTopic Library**.

[![OnTopic.TestDoubles package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/3a741b7a-7fa1-4bdb-bc55-efbac3f04e6c/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=3a741b7a-7fa1-4bdb-bc55-efbac3f04e6c&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.TestDoubles` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.TestDoubles" Version="4.0.0" />
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

- [`StubTopicRepository`](StubTopicRepository.cs)
