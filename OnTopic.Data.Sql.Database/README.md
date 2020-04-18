# SQL Schema
The `OnTopic.Data.Sql.Database` provides a default schema for supporting the [`SqlTopicRepository`](../OnTopic.Data.Sql).

> *Note:* Not all SQL objects are documented here. Missing objects are primarily intended for infrastructure support and used exclusively by stored procedures or administrators.

### Contents
- [Tables](#tables)
- [Stored Procedures](#stored-procedures)
  - [Querying](#querying)
  - [Updating](#updating)
- [Functions](#functions)
- [Views](#views)
- [Types](#types)

## Tables
The following is a summary of the most relevant tables.
- **[`Topics`](Tables/Topics.sql)**: Represents the core hierarchy of topics, encoded using a nested set model.
- **[`Attributes`](Tables/Attributes.sql)**: Represents key/value pairs of topic attributes, including historical versions.
- **[`ExtendedAttributes`](Tables/ExtendedAttributes.sql)**: Represents an XML-based representation of non-indexed attributes, which are too long for `Attributes`.
- **[`Relationships`](Tables/Relationships.sql)**: Represents relationships between topics, segmented by a `RelationshipKey`.

> *Note:* Neither `Topics` nor `Relationships` are subject to tracking versions. Changes to these records are permanent.

## Stored Procedures
The following is a summary of the most relevant stored procedures.

### Querying
- **[`GetTopics`](Stored%20Procedures/GetTopics.sql)**: Based on an optional `@TopicId` or `@TopicKey`, retrieves a hierarchy of topics, sorted by hierarchy, alongside separate data sets for corresponding records from `Attributes`, `ExtendedAttributes`, `Relationships`, and version history. Only retrieves the latest version data for each topic.
- **[`GetTopicVersion`](Stored%20Procedures/GetTopicVersion.sql)**: Retrieves a single instance of a topic based on a `@TopicId` and `@Version`. Not that the `@Version` must include miliseconds.

### Updating
- **[`CreateTopic`](Stored%20Procedures/CreateTopic.sql)**: Creates a new topic based on a `@ParentId`, an `AttributeValues` list of `@Attributes`, and an XML `@ExtendedAttributes`. Returns a new `@TopicId`.
- **[`DeleteTopic`](Stored%20Procedures/DeleteTopic.sql)**: Deletes an existing topic based on a `@TopicId`.
- **[`MoveTopic`](Stored%20Procedures/MoveTopic.sql)**: Moves an existing topic based on a `@TopicId`, `@ParentId`, and `@SiblingId`.
- **[`UpdateTopic`](Stored%20Procedures/UpdateTopic.sql)**: Updates an existing topic based on a `@TopicId`, an `AttributeValues` list of `@Attributes`, and an XML `@ExtendedAttributes`. Optionally deletes all relationships; these will need to be re-added using `UpdateRelationships`. Old attributes are persisted as previous versions.
- **[`UpdateRelationships`](Stored%20Procedures/UpdateRelationships.sql)**: Associates a relationship with a topic based on a `@TopicId`, `TopicList` array of `@Target_TopicIds`, and a `@RelationshipKey` (which can be any string label).

## Functions
- **[`GetTopicID`](Functions/GetTopicID.sql)**: Retrieves a topic's `TopicId` based on a corresponding `@TopicKey`.
- **[`GetParentID`](Functions/GetParentID.sql)**: Retrieves a topic's parent's `TopicId` based the child's `@TopicId`.

## Views
The majority of the views provide records corresponding to the latest version of records for each topic. These include:
- **[`TopicIndex`](Views/TopicIndex.sql)**: Includes the core topic attributes, `topicId`, `Key`, `ParentId`, and `ContentType`.
- **[`AttributeIndex`](Views/AttributeIndex.sql)**: Includes the `TopicId`, `AttributeKey` and `AttributeValue`.
- **[`ExtendedAttributesIndex`](Views/ExtendedAttributeIndex.sql)**: Includes the `TopicId` and `AttributeXml`.
- **[`VersionHistoryIndex`](Views/VersionHistoryIndex.sql)**: Includes up to the last five `Version` records for every `TopicId`.

## Types
User-defined table valued types are used to relay arrays of information to (and between) the stored procedures. These can be mimicked in C# using e.g. a `DataTable`. These include:
- **[`AttributeValues`](Types/AttributeValues.sql)**: Defines a table with an `AttributeKey` `Varchar(128)` and `AttributeValue` `Varchar(255)` columns.
- **[`TopicList`](Types/TopicList.sql)**: Defines a table with a single `TopicId` `Int` column for passing lists of topics.