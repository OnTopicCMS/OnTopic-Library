--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC ATTRIBUTES (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the attributes table by the latest version for each topic and attribute key. For most use cases, this should be the
-- primary sources for retrieving attributes, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
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