CREATE
VIEW	[dbo].[topics_TopicAttributeIndex]
WITH	SCHEMABINDING
AS

WITH TopicAttributes AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		TopicID,
			AttributeKey
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[topics_TopicAttributes]
  WHERE	AttributeKey	not in ('Key', 'ParentID', 'ContentType')
)
SELECT	TopicAttributes.TopicID,
	TopicAttributes.AttributeKey,
	TopicAttributes.AttributeValue
FROM	TopicAttributes
WHERE	RowNumber		= 1