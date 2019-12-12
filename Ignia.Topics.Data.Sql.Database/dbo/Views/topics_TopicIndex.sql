CREATE
VIEW	[dbo].[topics_TopicIndex]
WITH	SCHEMABINDING
AS

WITH KeyTopicAttributes AS (
  SELECT	TopicID,
                AttributeKey,
                AttributeValue,
                RowNumber = ROW_NUMBER() OVER (
                  PARTITION BY		TopicID,
			AttributeKey
                  ORDER BY		Version DESC
                )
  FROM	[dbo].[topics_TopicAttributes]
  WHERE	AttributeKey		IN ('Key', 'ParentID', 'ContentType')
)
SELECT	TopicID,
	ContentType,
	ParentID,
	[Key]		AS 'TopicKey'
FROM	KeyTopicAttributes
PIVOT (	MIN(AttributeValue)
  FOR	AttributeKey IN (
	  [Key],
	  [ParentID],
	  [ContentType]
	)
)	AS Pvt
WHERE	RowNumber = 1