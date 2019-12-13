--------------------------------------------------------------------------------------------------------------------------------
-- LEFT/RIGHT RANGE (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a list of all boundaries within the nested set hierarchy by combining RangeLeft and RangeRight values. This is
-- useful for a variety of operations, such as identifying gaps in the hierarchy. (A healthy hierarchy should provide an
-- uninterrupted sequence between the first row's RangeLeft and RangeRight.)
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[LeftRightRange] (seq) AS

SELECT	RangeLeft
FROM	Topics
UNION	ALL
SELECT	RangeRight
FROM	Topics;
