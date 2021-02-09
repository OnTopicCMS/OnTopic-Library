# OnTopic Library
The OnTopic library is a .NET Core-based content management system (CMS) designed around structured schemas ("Content Types") and optimized to simplify team-based workflows with distinct roles for content owners, backend developers, and graphic producers.

[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)
![NuGet Deployment Status](https://rmsprodscussu1.vsrm.visualstudio.com/A09668467-721c-4517-8d2e-aedbe2a7d67f/_apis/public/Release/badge/bd7f03e0-6fcf-4ec6-939d-4e995668d40f/2/2)
[![OnTopic package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/fb67677f-2b83-4318-9007-0c46b4da55c1/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=fb67677f-2b83-4318-9007-0c46b4da55c1&preferRelease=true)

### Roles
The OnTopic library acknowledges that the roles of developers, designers, and content owners are usually compartmentalized and, thus, optimizes for the needs of each.

- **Content owners** have access to an [editor](https://github.com/OnTopicCMS/OnTopic-Editor-AspNetCore) that focuses _exclusively_ on exposing *structured data*; this includes support for custom content types (e.g., "Job Posting", "Blog Post", &c.)
- **Backend developers** have access to *data repositories*, *services*, and a rich *entity* model in C# for consuming the structured data and implementing any *business logic* via code.
- **Frontend developers** have access to light-weight *views* based on purpose-built *view models*, thus allowing them to focus exclusively on presentation concerns, without any platform-specific knowledge.

This is contrasted to most traditional CMSs, which attempt to coordinate all of these via an editor by exposing design responsibilities (via themes, templates, and layouts) as well as development responsibilities (via plugins or components). This works well for a small project without distinct design or development resources, but introduces a lot of complexity for more mature teams with well-established roles.

### Multi-Device Optimized
In addition, OnTopic is optimized for multi-client/multi-device scenarios since the content editor focuses _exclusively_ on structured data. This allows entirely distinct presentation layers to be established without the CMS attempting attempting to influence or determing design decisions via e.g. per-page layout. For instance, the same content can be accessed by an iOS app, a website, and even a web-based API for third-party consumption. By contrast, most CMSs are designed for one client only: a website (which may be mobile-friendly via responsive templates.)

### Extensible
Fundamentally, OnTopic is based on structured schemas ("Content Types") which can be modified via the editor itself. This allows new data structures to be introduced without needing to modify the database or creating extensive plugins. So, for example, if a site includes job postings, it might create a `JobPosting` content type that describes the structure of a job posting, such as _job title_, _job description_, _job requirements_, &c. By contrast, some CMSsï¿½such as WordPressï¿½try to fit all items into a single data modelï¿½such as a _blog post_ï¿½or require extensive customizations of database objects and intermediate queries in order to extend the data model. OnTopic is designed with extensibility in mind, so updates to the data model are comparatively trivial to implement.

## Library

### Domain Layer
- **[`OnTopic.Topics`](OnTopic/README.md)**: Core domain model including the `Topic` entity and service abstractions such as `ITopicRepository`.

### Data Access Layer
- **[`OnTopic.Data.Sql`](OnTopic.Data.Sql/README.md)**: [`ITopicRepository`](OnTopic/Repositories/ITopicRepository.cs) implementation for storing and retrieving [`Topic`](OnTopic/Topic.cs) entities in a Microsoft SQL Server database.
  - **[`OnTopic.Data.Sql.Database`](OnTopic.Data.Sql.Database/README.md)**: Microsoft SQL Server database schema, including tables, views, functions, and stored procedures needed to support the [`OnTopic.Data.Sql`](OnTopic.Data.Sql/README.md) library.
- **[`OnTopic.Data.Caching`](OnTopic.Data.Caching/README.md)**: [`ITopicRepository`](OnTopic/Repositories/ITopicRepository.cs) façade that caches data accessed in memory for fast subsequent retrieval.

> *Note*: Additional data access layers can be created by implementing the [`ITopicRepository`](OnTopic/Repositories/ITopicRepository.cs) interface.

### Presentation Layer
- **[`OnTopic.AspNetCore.Mvc`](OnTopic.AspNetCore.Mvc/README.md)**: ASP.NET Core 2.x implementation, including a default [`TopicController`](OnTopic.AspNetCore.Mvc/Controllers/TopicController.cs), allowing templates to be created using `*.cshtml` pages and _view components_.
- **[`OnTopic.Web.Mvc`](https://github.com/OnTopicCMS/OnTopic-MVC)**: ASP.NET MVC 5.x implementation, including a default [`TopicController`](https://github.com/OnTopicCMS/OnTopic-MVC/blob/master/OnTopic.Web.Mvc/Controllers/TopicController.cs), allowing templates to be created using `*.cshtml` pages.
- **[`OnTopic.Web`](https://github.com/OnTopicCMS/OnTopic-WebForms/)**: Legacy ASP.NET WebForms implementation, allowing templates to be created using `*.aspx` pages. This is considered obsolete, and intended exclusively for migration to new versions.
- **[`OnTopic.ViewModels`](OnTopic.ViewModels/README.md)**: Standard view models for exposing factory=default schemas of shared content types. These can be extended, overwritten, or ignored entirely by the presentation layer implementation; they are provided for convenience.

### Unit Tests
- **[`OnTopic.Tests`](OnTopic.Tests)**: .NET Unit Tests, broken down by target class.
- **[`OnTopic.AspNetCore.Mvc.Tests`](OnTopic.AspNetCore.Mvc.Tests)**: .NET Unit Tests for the `OnTopic.AspNetCore.Mvc` implementation.

### Editor
- **[`OnTopic.Editor.AspNetCore`](https://github.com/OnTopicCMS/OnTopic-Editor-AspNetCore/)**: ASP.NET Core 3.1 implementation of the editor interface.
- **[`OnTopic.Editor`](https://github.com/OnTopicCMS/OnTopic-Editor-WebForms/)**: Legacy ASP.NET WebForms implementation of the editor interface.

## Credits
OnTopic is owned and maintained by [Ignia](http://www.ignia.com/).