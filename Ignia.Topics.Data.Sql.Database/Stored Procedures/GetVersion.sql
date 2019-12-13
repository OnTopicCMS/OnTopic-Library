--------------------------------------------------------------------------------------------------------------------------------
-- GET VERSION
--------------------------------------------------------------------------------------------------------------------------------
-- Retrieves data associated with an individual topic version, as a means of either comparing or restoring a previous version.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[GetVersion]
	@TopicID		int	= -1,
	@Version		datetime	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	KeyAttributes
AS (
  SELECT	Attributes.TopicID,
                Attributes.AttributeKey,
                Attributes.AttributeValue,
                RowNumber		= ROW_NUMBER() OVER (
                  PARTITION BY		Attributes.TopicID,
			Attributes.AttributeKey
                  ORDER BY		Version		DESC
                )
  FROM	Attributes		AS Attributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
    AND	AttributeKey
    IN (	'Key',
	'ParentID',
	'ContentType'
    )
)
SELECT	TopicID,
	ContentType,
	ParentID,
	[Key]		AS 'TopicKey',
	1		AS 'SortOrder'
FROM	KeyAttributes
PIVOT (	MIN(AttributeValue)
  FOR	AttributeKey
  IN (	[ContentType],
	[ParentID],
	[Key]
  )
)	AS Pvt
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	Attributes
AS (
  SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Attributes.TopicID,
			Attributes.AttributeKey
	  ORDER BY		Attributes.Version	DESC
	)
  FROM	Attributes		AS Attributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
    AND 	Attributes.AttributeKey
    NOT IN (	'Key',
	'ParentID',
	'ContentType'
    )
)
SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue
FROM	Attributes
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT BLOB
--------------------------------------------------------------------------------------------------------------------------------
;WITH TopicBlob
AS (
  SELECT	Blob.TopicID,
	Blob.Blob,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Blob.TopicID
	  ORDER BY		Blob.Version		DESC
	)
  FROM	Blob		AS Blob
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
)
SELECT	TopicBlob.TopicID,
	TopicBlob.Blob
FROM	TopicBlob
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
;SELECT	RelationshipTypeID,
	Source_TopicID,
	Target_TopicID
FROM	Relationships
WHERE	Source_TopicID		= @TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TopicID,
	Version
FROM	VersionHistoryIndex
WHERE	TopicID		= @TopicID