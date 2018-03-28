# OnTopic Library
The OnTopic library (`Ignia.Topics.*`) is a .NET Framework-based content management system (CMS) established on the separation of concerns (SoC) design principle. 

### Roles
Specifically, it attempts to ensure that the responsibilities of the developer, designer, and content owner are compartmentalized and optimized for the needs of each.

- **Content owners** have access to an *editor* that focuses exclusively on providing access to *structured data*; this includes support for custom content types (e.g., "Job Posting", "Blog Post", &c.)
- **Backend developers** have access to *data repositories*, *services*, and a rich *domain model* in C# for consuming the structured data and implementing any *business logic* via code.
- **Designers and graphic producers** have access to light-weight *views* based on purpose-build *view models*, thus allowing them to focus exclusively on presnetation concerns.

This is contrasted to most traditional CMSs, which attempt to coordinate all of these via an editor by exposing design responsibilities (via themes, templates, and layouts) as well as development responsibilities  (via plugins or components). This works well for a small project without distinct design or development resources, but introduces a lot of complexity for larger teams with established responsibilities.

### Multi-Device Optimized
In addition, OnTopic is optimized for multi-client/multi-device scenarios since the content editor focuses exclusively on structured data. This allows entirely distinct presentation layers to be established. For instance, the same content can be accessed by an iOS app, a website, and even a web-based API for third-party consumption. By contrast, most CMSs are designed for one client only: a website (which may be mobile-friendly via responsive templates.)

## Library

### Domain Layer
- [`Ignia.Topics`](Ignia.Topics): Core domain model including `Topic` entity and service abstractions.

### Data Access Layer
- [`Ignia.Topics.Data.Caching`](Ignia.Topics.Data.Caching): [`ITopicRepository`](Ignia.Topics/Repositories/ITopicRepository.cs) façade that caches data accessed in memory for fast subsequent retrieval.
- [`Ignia.Topics.Data.Sql`](Ignia.Topics.Data.Sql): [`ITopicRepository`](Ignia.Topics/Repositories/ITopicRepository.cs) implementation for storing and retrieving [`Topic`](Ignia.Topics/Topic.cs) entities in a Microsoft SQL Server database.
  - [`Ignia.Topics.Data.Sql.Database`](Ignia.Topics.Data.Sql.Database): Microsoft SQL Server database definition, including tables, views, and stored procedures needed to support the [`Ignia.Topics.Data.Sql`](Ignia.Topics.Data.Sql) library.

> *Note*: Additional data access layers can be created by implementing the [`ITopicRepository`](Ignia.Topics/Repositories/ITopicRepository.cs) interface. 

### Presentation Layer
- [`Ignia.Topics.Web`](Ignia.Topics.Web): ASP.NET WebForms implementation, allowing templates to be created using `*.aspx` pages. This is considered deprecated, and intended exclusively for backward compatibility.
- [`Ignia.Topics.Web.Mvc`](Ignia.Topics.Web.Mvc): ASP.NET MVC 5.x implementation, including a default [`TopicController`](Ignia.Topics.Web.Mvc/TopicController.cs) implementation, allowing templates to be created using `*.cshtml` pages.
- [`Ignia.Topics.ViewModels`](Ignia.Topics.ViewModels): Standard view models for exposing factory default schemas of shared content types. These can be extended, overwritten, or ignored entirely by the presentation layer implementation; they are provided for convenience.

### Unit Tests
- [`Ignia.Topics.Tests`](Ignia.Topics.Tests): .NET Unit Tests, broken down by target class.

### Editor
- [`Ignia.Topics.Editor`](https://github.com/Ignia/Topic-Editor/): ASP.NET WebForms implementation of the editor interface.
- [`Ignia.Topics.Editor.Mvc`](https://github.com/Ignia/Topic-Editor-MVC/): ASP.NET MVC implementation of the editor interface.


## Credits
OnTopic is owned and maintained by [Ignia](http://www.ignia.com/). 
