--------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GET VERSION
--
-- Purpose	Retrieves data associated with an individual topic version, as a means of either comparing or restoring a previous version.
--
-- History      Jeremy Caney		07262017	Initial Creation based on original topics_getTopics code.
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_GetVersion]
	@TopicID		int	= -1,
	@Version		datetime	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	KeyTopicAttributes
AS (
  SELECT	Attributes.TopicID,
                Attributes.AttributeKey,
                Attributes.AttributeValue,
                RowNumber		= ROW_NUMBER() OVER (
                  PARTITION BY		Attributes.TopicID,
			Attributes.AttributeKey
                  ORDER BY		Version	DESC
                )
  FROM	topics_TopicAttributes	AS Attributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
    AND	AttributeKey		IN ('Key', 'ParentID', 'ContentType')
)
SELECT	TopicID,
	ContentType,
	ParentID,
	[Key]		AS 'TopicKey',
	1		AS 'SortOrder'
FROM	KeyTopicAttributes
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
	  PARTITION BY		Attributes.TopicID, Attributes.AttributeKey
	  ORDER BY		Attributes.Version DESC
	)
  FROM	topics_TopicAttributes	AS Attributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
    AND 	Attributes.AttributeKey	not in ('Key', 'ParentID', 'ContentType')
)
SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue
FROM	Attributes
WHERE	RowNumber		= 1
--ORDER BY	Storage.SortOrder	ASC,
--	TopicAttributes.AttributeKey	ASC

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT BLOB
--------------------------------------------------------------------------------------------------------------------------------
;WITH TopicBlob
AS (
  SELECT	Blob.TopicID,
	Blob.Blob,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Blob.TopicID
	  ORDER BY		Blob.Version DESC
	)
  FROM	topics_Blob		AS Blob
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
;SELECT	Relationships.RelationshipTypeID,
	Relationships.Source_TopicID,
	Relationships.Target_TopicID
FROM	topics_Relationships	Relationships
WHERE	Source_TopicID		= @TopicID


--------------------------------------------------------------------------------------------------------------------------------
-- SELECT HISTORY
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicVersions
AS (
  SELECT	Attributes.TopicID,
	Attributes.Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Attributes.TopicID
	  ORDER BY		Version DESC
	)
  FROM	topics_TopicAttributes	Attributes
  WHERE	TopicID		= @TopicID
)
SELECT	DISTINCT
	TopicId,
	Version
From	TopicVersions
Where	RowNumber		>= 6