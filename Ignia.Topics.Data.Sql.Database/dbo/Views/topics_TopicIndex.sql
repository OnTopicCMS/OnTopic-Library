CREATE VIEW [dbo].[topics_TopicIndex] 
WITH SCHEMABINDING
AS 

WITH KeyTopicAttributes AS (
  SELECT	Attributes.TopicID,
                Attributes.AttributeKey,
                Attributes.AttributeValue,
                RowNumber = ROW_NUMBER() OVER (
                  PARTITION BY  Attributes.TopicID, Attributes.AttributeKey
                  ORDER BY Version DESC
                )
  FROM		[dbo].[topics_TopicAttributes] as Attributes
  WHERE		AttributeKey IN ('Key', 'ParentID', 'ContentType')
)
SELECT		TopicID, 
		ContentType, 
		ParentID, 
		[Key] AS 'TopicKey'
FROM		KeyTopicAttributes
PIVOT (		MIN(AttributeValue)
  FOR		AttributeKey IN ([Key], [ParentID], [ContentType])
)		AS Pvt 
WHERE		RowNumber = 1