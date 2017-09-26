CREATE VIEW [dbo].[topics_TopicAttributeIndex]
WITH SCHEMABINDING
AS

WITH TopicAttributes AS (
  SELECT	Attributes.TopicID,
		Attributes.AttributeKey,
		Attributes.AttributeValue,
		RowNumber = ROW_NUMBER() OVER (
		  PARTITION BY	Attributes.TopicID, Attributes.AttributeKey
		  ORDER BY	Attributes.Version DESC
		)
  FROM		[dbo].[topics_TopicAttributes] as Attributes
  WHERE		Attributes.AttributeKey not in ('Key', 'ParentID', 'ContentType')
)
SELECT		TopicAttributes.TopicID,
		TopicAttributes.AttributeKey,
		TopicAttributes.AttributeValue
FROM		TopicAttributes
WHERE		RowNumber = 1