# OnTopic SQL Repository
The `SqlTopicRepository` provides an implementation of the `ITopicRepository` interface for use with Microsoft SQL Server. All requests are sent to the database, with no effort to cache data.

> *Note:* The schema for the Microsoft SQL Server implementation can be found at [`Ignia.Topics.Data.Sql.Database`](../Ignia.Topics.Data.Sql.Database). It is not currently distributed as part of the `SqlTopicRepository` and must be deployed separately.

[![OnTopic.Data.Sql package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/15c8a666-efa5-4b23-b08b-1de907478d2d/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=15c8a666-efa5-4b23-b08b-1de907478d2d&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)

## Usage
```
var sqlTopicRepository = new SqlTopicRepository(connectionString);
var rootTopic = sqlTopicRepository.Load();
```
> *Note:* In real-world applications, it is recommended that the `SqlTopicRepository` be wrapped by the [`CachedTopicRepository`](../Ignia.Topics.Data.Caching), which provides an in-memory cache of any `ITopicRepository` implementation.