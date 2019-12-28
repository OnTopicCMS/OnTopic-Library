# `SqlTopicRepository`
The `SqlTopicRepository` provides an implementation of the `ITopicRepository` interface for use with Microsoft SQL Server. All requests are sent to the database, with no effort to cache data.

> *Note:* The schema for the Microsoft SQL Server implementation can be found at [`Ignia.Topics.Data.Sql.Database`](../Ignia.Topics.Data.Sql.Database). It is not currently distributed as part of the `SqlTopicRepository` and must be deployed separately.

## Usage
```
var sqlTopicRepository = new SqlTopicRepository(connectionString);
var rootTopic = sqlTopicRepository.Load();
```
> *Note:* In real-world applications, it is recommended that the `SqlTopicRepository` be wrapped by the [`CachedTopicRepository`](../Ignia.Topics.Data.Caching), which provides an in-memory cache of any `ITopicRepository` implementation.