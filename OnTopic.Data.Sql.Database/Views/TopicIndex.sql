--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Retrieves the latest version of the key attributes for each topic and pivots them into a single record for each topic. When
-- loading or reporting topics, it's often useful to start with the Key, ContentType, and ParentID; once those are established,
-- other attributes and relationships can be pulled. This helps in that process by making all of those items available in a
-- single query.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[TopicIndex]
WITH	SCHEMABINDING
AS

WITH KeyAttributes AS (
  SELECT	TopicID,
                AttributeKey,
                AttributeValue,
                RowNumber = ROW_NUMBER() OVER (
                  PARTITION BY		TopicID,
			AttributeKey
                  ORDER BY		Version DESC
                )
  FROM	[dbo].[Attributes]
  WHERE	AttributeKey
  IN (	'Key',
	'ParentID',
	'ContentType'
  )
)
SELECT	TopicID,
	ContentType,
	ParentID,
	[Key]		AS 'TopicKey'
FROM	KeyAttributes
PIVOT (	MIN(AttributeValue)
  FOR	AttributeKey IN (
	  [Key],
	  [ParentID],
	  [ContentType]
	)
)	AS Pvt
WHERE	RowNumber = 1