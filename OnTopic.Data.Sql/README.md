# OnTopic SQL Repository
The `SqlTopicRepository` provides an implementation of the `ITopicRepository` interface for use with Microsoft SQL Server. All requests are sent to the database, with no effort to cache data.

[![OnTopic.Data.Sql package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/15c8a666-efa5-4b23-b08b-1de907478d2d/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=15c8a666-efa5-4b23-b08b-1de907478d2d&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

> *Note:* The schema for the Microsoft SQL Server implementation can be found at [`OnTopic.Data.Sql.Database`](../OnTopic.Data.Sql.Database/README.md). It is not currently distributed as part of the `SqlTopicRepository` and must be deployed separately.

### Contents
- [Installation](#installation)
- [Usage](#usage)

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.Data.Sql` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.Data.Sql" Version="4.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).

## Usage
```csharp
var sqlTopicRepository = new SqlTopicRepository(connectionString);
var rootTopic = sqlTopicRepository.Load();
```
> *Note:* In real-world applications, it is recommended that the `SqlTopicRepository` be wrapped by the [`CachedTopicRepository`](../OnTopic.Data.Caching/README.md), which provides an in-memory cache of any `ITopicRepository` implementation.