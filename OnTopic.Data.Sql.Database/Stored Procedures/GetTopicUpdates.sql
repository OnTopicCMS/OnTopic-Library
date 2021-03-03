--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC UPDATES
--------------------------------------------------------------------------------------------------------------------------------
-- Retrieves any data persisted to the database since the last query.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopicUpdates]
	@Since		DATETIME2(7)
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicID,
  	ContentType,
  	ParentID,
  	TopicKey,
  	0 AS SortOrder
FROM	Topics
WHERE	LastModified		> @Since

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicAttributes
AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID,
			AttributeKey
	  ORDER BY		Version		DESC
	)
  FROM	Attributes
  WHERE	Version		> @Since
)
SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	Version
FROM	TopicAttributes
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicExtendedAttributes
AS (
  SELECT	TopicID,
	AttributesXml,
	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version		DESC
	)
  FROM	ExtendedAttributes
  WHERE	Version		> @Since
)
SELECT	TopicID,
	AttributesXml,
	Version
FROM	TopicExtendedAttributes
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
;WITH Relationships AS (
  SELECT	Source_TopicID,
	RelationshipKey,
	Target_TopicID,
	IsDeleted,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID,
			RelationshipKey
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[Relationships]
  WHERE	Version		> @Since
)
SELECT	Relationships.Source_TopicID,
	Relationships.RelationshipKey,
	Relationships.Target_TopicID,
	Relationships.IsDeleted,
	Relationships.Version
FROM	Relationships
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
;WITH TopicReferences AS (
  SELECT	Source_TopicID,
	ReferenceKey,
	Target_TopicID,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID,
			ReferenceKey
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[TopicReferences]
  WHERE	Version		> @Since
)
SELECT	TopicReferences.Source_TopicID,
	TopicReferences.ReferenceKey,
	TopicReferences.Target_TopicID,
	TopicReferences.Version
FROM	TopicReferences
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicID,
	Version
FROM	VersionHistoryIndex
WHERE	Version		> @Since