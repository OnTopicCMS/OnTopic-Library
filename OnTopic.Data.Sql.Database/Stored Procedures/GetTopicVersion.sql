--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC VERSION
--------------------------------------------------------------------------------------------------------------------------------
-- Retrieves data associated with an individual topic version, as a means of either comparing or restoring a previous version.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetTopicVersion]
	@TopicID		INT	= -1,
	@Version		DATETIME	= NULL
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicID,
  	ContentType,
  	ParentID,
  	TopicKey,
  	0 AS SortOrder
FROM	Topics
WHERE	TopicID		= @TopicID

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
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
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
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
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
  WHERE	Source_TopicID		= @TopicID
    AND	Version		<= @Version
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
  WHERE	Source_TopicID		= @TopicID
    AND	Version		<= @Version
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
WHERE	TopicID		= @TopicID