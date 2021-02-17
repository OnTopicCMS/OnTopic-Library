# OnTopic Metapackage
The `OnTopic.All` metapackage includes a reference to the core OnTopic libraries that most implementations will require. It is recommended that implementers reference this package instead of referencing each of the OnTopic packages individually, unless they have a specific need to customize which packages are referenced.

[![OnTopic.Data.Caching package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/3dfb3a0a-c049-407d-959e-546f714dcd0f/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=3dfb3a0a-c049-407d-959e-546f714dcd0f&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)

### Contents
- [Scope](#scope)
- [Installation](#installation)

## Scope
The `OnTopic.All` metapackage maintains a reference to the following packages:
- [`OnTopic`](../OnTopic/README.md): The core OnTopic library.
- [`OnTopic.AspNetCore.Mvc`](../OnTopic.AspNetCore.Mvc/README.md): The ASP.NET Core implementation, with support for both ASP.NET Core 3.x and ASP.NET Core 5.x.
- [`OnTopic.Data.Caching`](../OnTopic.Data.Caching/README.md): An `ITopicRepository` decorator for caching the topic graph in memory.
- [`OnTopic.Data.Sql`](../OnTopic.Data.Sql/README.md): An `ITopicRepository` implementation for persisting topic data in a SQL Server database.
- [`OnTopic.ViewModels`](../OnTopic.ViewModels/README.md): A set of reference view models and binding models mapping to the out-of-the-box schema for the standard content types.

## Installation
Installation can be performed by providing a `<PackageReference /`> to the `OnTopic.All` **NuGet** package.
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  …
  <ItemGroup>
    <PackageReference Include="OnTopic.All" Version="5.0.0" />
  </ItemGroup>
</Project>
```

> *Note:* This package is currently only available on Ignia's private **NuGet** repository. For access, please contact [Ignia](http://www.ignia.com/).