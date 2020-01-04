# `CachedTopicRepository`
The `CachedTopicRepository` provides an in-memory wrapper around an `ITopicRepository` implementation. When topics are requested, they are pulled from the cache, if they exist; otherwise, they are pulled from the underlying `ITopicRepository` implementation, and then cached. Similarly, when topics are e.g. saved, the updated versions are persisted to the underlying `ITopicRepository`, and then updated in the cache.

[![OnTopic.Data.Caching package in Internal feed in Azure Artifacts](https://igniasoftware.feeds.visualstudio.com/_apis/public/Packaging/Feeds/46d5f49c-5e1e-47bb-8b14-43be6c719ba8/Packages/3dfb3a0a-c049-407d-959e-546f714dcd0f/Badge)](https://igniasoftware.visualstudio.com/OnTopic/_packaging?_a=package&feed=46d5f49c-5e1e-47bb-8b14-43be6c719ba8&package=3dfb3a0a-c049-407d-959e-546f714dcd0f&preferRelease=true)
[![Build Status](https://igniasoftware.visualstudio.com/OnTopic/_apis/build/status/OnTopic-CI-V3?branchName=master)](https://igniasoftware.visualstudio.com/OnTopic/_build/latest?definitionId=7&branchName=master)

## Usage
```
var sqlTopicRepository = new SqlTopicRepository(connectionString);
var cachedTopicRepository = new CachedTopicRepository(sqlTopicRepository);

var rootTopic = cachedTopicRepository.Load();
```