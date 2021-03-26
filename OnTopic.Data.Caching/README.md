# OnTopic Cached Repository
The `CachedTopicRepository` decorates another `ITopicRepository` implementation with an in-memory cache. It is recommended that web applications decorate their `ITopicRepository` implementation.

[![OnTopic.Data.Caching package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/3dfb3a0a-c049-407d-959e-546f714dcd0f/Badge)](https://www.nuget.org/packages/OnTopic.Data.Caching/)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

### Contents
- [Functionality](#functionality)
- [Installation](#installation)
- [Usage](#usage)

## Functionality
When topics are requested, they are pulled from the cache, if they exist; otherwise, they are pulled from the underlying `ITopicRepository` implementation, and then cached. Similarly, when topics are e.g. saved or moved, the updated versions are persisted to the underlying `ITopicRepository`, and then updated in the cache.

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.Data.Caching` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.Data.Caching" Version="5.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).

## Usage
```csharp
var sqlTopicRepository      = new SqlTopicRepository(connectionString);
var cachedTopicRepository   = new CachedTopicRepository(sqlTopicRepository);
var rootTopic               = cachedTopicRepository.Load();
```