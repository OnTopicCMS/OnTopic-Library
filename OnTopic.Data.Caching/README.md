# `CachedTopicRepository`
The `CachedTopicRepository` provides an in-memory wrapper around an `ITopicRepository` implementation. When topics are requested, they are pulled from the cache, if they exist; otherwise, they are pulled from the underlying `ITopicRepository` implementation, and then cached. Similarly, when topics are e.g. saved, the updated versions are persisted to the underlying `ITopicRepository`, and then updated in the cache.

## Usage
```
var sqlTopicRepository = new SqlTopicRepository(connectionString);
var cachedTopicRepository = new CachedTopicRepository(sqlTopicRepository);

var rootTopic = cachedTopicRepository.Load();
```