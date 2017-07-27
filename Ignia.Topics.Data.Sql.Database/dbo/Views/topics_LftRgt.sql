CREATE VIEW [dbo].[topics_LftRgt] (seq) AS

SELECT		RangeLeft 
FROM		topics_Topics
UNION		ALL
SELECT		RangeRight 
FROM		topics_Topics;