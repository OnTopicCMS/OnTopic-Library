# SQL Schema
The `Ignia.Topics.Data.Sql.Database` provides a default schema for supporting the [`SqlTopicRepository`](../Ignia.Topics.Data.Sql). 

> *Note:* Not all SQL objects are documented here. Missing objects are primarily intended for infrastructure support and used exclusively by stored procedures or administrators.

## Tables
The following is a summary of the most relevant tables. 
- **[`topics_Topics`](dbo/Tables/topics_Topics.sql)**: Represents the core hierarchy of topics, encoded in a nested set format.
- **[`topics_TopicAttributes`](dbo/Tables/topics_Topics.sql)**: Represents key/value pairs of topic attributes, including historical versions.
- **[`topics_Blob`](dbo/Tables/topics_Blob.sql)**: Represents an XML-based blob of non-indexed attributes, which are too long for `topics_TopicAttributes`.
- **[`topics_Relationships`](dbo/Tables/topics_Relationships.sql)**: Represents relationships between topics, segmented by namespace. 

> *Note:* Neither `topics_Topics` nor `topics_Relationships` are subject to tracking versions. Changes to these records are permanent.

## Stored Procedures
The following is a summary of the most relevant stored procedures. 

### Querying
- **[`topics_GetTopics`](dbo/Stored%20Procedures/topics_GetTopics.sql)**: Based on an optional `@TopicId` or `@TopicKey`, retrieves a hierarchy of topics, sorted by hierarchy, alongside separate data sets for corresponding records from `topics_TopicAttributes`, `topics_Blob`, `topics_Relationships`, and version history. Only retrieves the latest version of each attribute.
- **[`topics_GetGetTopicID`](dbo/Stored%20Procedures/topics_GetTopicID.sql)**: Retrieves a topic's `Id` based on a corresponding `Key`. 
- **[`topics_GetVersion`](dbo/Stored%20Procedures/topics_GetVersion.sql)**: Retrieves a single instance of a topic based on an `@Id` and `@Version`.

### Updating
- **[`topics_CreateTopic`](dbo/Stored%20Procedures/topics_CreateTopic.sql)**: Creates a new topic based on a `@ParentId`, an array of `@Attributes`, and an XML `@Blob`. Returns a new `@@Identity`.
- **[`topics_DeleteTopic`](dbo/Stored%20Procedures/topics_DeleteTopic.sql)**: Deletes an existing topic based on a `@Id`.
- **[`topics_MoveTopic`](dbo/Stored%20Procedures/topics_MoveTopic.sql)**: Moves an existing topic based on an `@Id`, `@ParentId`, and `@SiblingId`.
- **[`topics_UpdateTopic`](dbo/Stored%20Procedures/topics_UpdateTopic.sql)**: Updates an existing topic based on an `@Id`, an array of `@Attributes`, and a `@Blob`. Optionally deletes all relationships; these will need to be re-added using `topics_PersistRelations`. Old attributes are persisted as previous versions.
- **[`topics_PersistRelations`](dbo/Stored%20Procedures/topics_PersistRelations.sql)**: Associates a relationship with a topic based on a `@Source_TopicId`, array of `@Target_TopicIds`, and `@RelationshipTypeID` (which can be any string label).

## Views
The majority of the views provide records corresponding to the latest version of records for each topic. These include:
- **[`topics_TopicIndex`](dbo/Views/topics_TopicIndex.sql)**: Includes the core topic attributes, `Id`, `Key`, `ParentId`, and `ContentType`.
- **[`topics_TopicIndex`](dbo/Views/topics_TopicAttributeIndex.sql)**: Includes the `Id`, `AttributeKey` and `AttributeValue`.
- **[`topics_BlobIndex`](dbo/Views/topics_BlobIndex.sql)**: Includes the `Id` and `Blob`.

