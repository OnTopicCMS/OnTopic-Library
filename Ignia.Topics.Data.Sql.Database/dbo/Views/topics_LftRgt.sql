--------------------------------------------------------------------------------------------------------------------------------
-- LEFT/RIGHT (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a list of all boundaries within the nested set hierarchy by combining RangeLeft and RangeRight values. This is
-- useful for a variety of operations, such as identifying gaps in the hierarchy. (A healthy hierarchy should provide an
-- uninterrupted sequence between the first row's RangeLeft and RangeRight.)
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[topics_LftRgt] (seq) AS

SELECT	RangeLeft
FROM	topics_Topics
UNION	ALL
SELECT	RangeRight
FROM	topics_Topics;