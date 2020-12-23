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
;WITH	KeyAttributes
AS (
  SELECT	TopicID,
                AttributeKey,
                AttributeValue,
                RowNumber		= ROW_NUMBER() OVER (
                  PARTITION BY		TopicID,
			AttributeKey
                  ORDER BY		Version		DESC
                )
  FROM	Attributes
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
;WITH	TopicAttributes
AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID,
			AttributeKey
	  ORDER BY		Version		DESC
	)
  FROM	Attributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
    AND 	AttributeKey
    NOT IN (	'Key',
	'ParentID',
	'ContentType'
    )
)
SELECT	TopicID,
	AttributeKey,
	AttributeValue
FROM	TopicAttributes
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
;WITH	TopicExtendedAttributes
AS (
  SELECT	TopicID,
	AttributesXml,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version		DESC
	)
  FROM	ExtendedAttributes
  WHERE	TopicID		= @TopicID
    AND	Version		<= @Version
)
SELECT	TopicID,
	AttributesXml
FROM	TopicExtendedAttributes
WHERE	RowNumber		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
;SELECT	Source_TopicID,
	RelationshipKey,
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