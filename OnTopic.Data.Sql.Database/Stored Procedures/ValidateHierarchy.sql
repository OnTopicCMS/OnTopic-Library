--------------------------------------------------------------------------------------------------------------------------------
-- VALIDATE HIERARCHY
--------------------------------------------------------------------------------------------------------------------------------
-- Helps identify potential sources of corruption in the nested set hierarchy
--------------------------------------------------------------------------------------------------------------------------------
-- ### NOTE JJC20191211: The nested set model is incredibly useful to fast reads of a hierarchy. But it is also susceptible to
-- corruption from e.g., errant queries or poorly handled transactions. Ideally, this script shouldn't be necessary. But, as a
-- safety precaution, it provides a means of identifying potential sources of corruption. It won't resolve the corruption, nor
-- fully identify where it is. But it'll help identify scenarios that should never occur.
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[ValidateHierarchy]
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DETECT RANGE OVERLAPS
--------------------------------------------------------------------------------------------------------------------------------
-- Ranges should always be inclusive. The following query identifies scenarios where a range starts before a set, and ends in
-- the middle of it. For example, if you have a range 2-5 and another with a range of 3-6, that's a scenario that should never
-- occur, and which the following query will identify. Note that this query is SLOW; it is not optimized for performance!
--------------------------------------------------------------------------------------------------------------------------------
PRINT	'Detect range overlaps'

SELECT	TopicID,
	RangeLeft,
	RangeRight
FROM	Topics		OuterTopics
WHERE (
  SELECT	COUNT(TopicID)
  FROM	Topics		InnerTopics
  WHERE (	RangeLeft		< OuterTopics.RangeLeft
    AND	ISNULL(RangeRight, -1)	< ISNULL(OuterTopics.RangeRight, -1)
    AND	ISNULL(RangeRight, -1)	> OuterTopics.RangeLeft
  )
  OR (	RangeLeft		> OuterTopics.RangeLeft
    AND	ISNULL(RangeRight, -1)	> ISNULL(OuterTopics.RangeRight, -1)
    AND	RangeLeft		< ISNULL(OuterTopics.RangeRight, -1)
  )
) > 0

--------------------------------------------------------------------------------------------------------------------------------
-- DETECT RANGE DUPLICATES
--------------------------------------------------------------------------------------------------------------------------------
-- The RangeLeft and RangeRight should be unique within the database. This isn't enforced, however, as it complicates making
-- updates to the hierarchy. If there is ever a scenario where two topics have the same RangeLeft or RangeRight value as another
-- range, that indicates a problem.
--------------------------------------------------------------------------------------------------------------------------------
PRINT	'Detect range duplicates'

SELECT	TopicID,
	RangeLeft,
	RangeRight
FROM	Topics		OuterTopics
WHERE (
  SELECT	COUNT(TopicID)
  FROM	Topics		InnerTopics
  WHERE	TopicID		!= OuterTopics.TopicID
    AND (	RangeLeft
      IN (	  OuterTopics.RangeLeft,
	  ISNULL(OuterTopics.RangeRight, -1)
        )
    OR	ISNULL(RangeRight, 0)
      IN (	  OuterTopics.RangeLeft,
	  ISNULL(OuterTopics.RangeRight, -1)
      )
    )
) > 0

--------------------------------------------------------------------------------------------------------------------------------
-- DETECT RANGE MISMATCHES
--------------------------------------------------------------------------------------------------------------------------------
-- The LeftRange should always be lower than the RightRange. If that's not the case, something is wrong.
--------------------------------------------------------------------------------------------------------------------------------
PRINT	'Detect range mismatches'

SELECT	TopicID,
	RangeLeft,
	RangeRight
FROM	Topics
WHERE	RangeLeft	>= ISNULL(RangeRight, -1)

--------------------------------------------------------------------------------------------------------------------------------
-- DETECT PARENT ID MISMATCHES
--------------------------------------------------------------------------------------------------------------------------------
-- The ParentID attribute is a cached version of the topic's parent in the nested set hierarchy. These values should match. If
-- that's not the case, something is wrong.
--------------------------------------------------------------------------------------------------------------------------------
PRINT	'Detect ParentID mismatches'

SELECT	TopicID,
	AttributeValue
FROM	Attributes
WHERE	AttributeKey		= 'ParentID'
AND	AttributeValue		!= dbo.GetParentID(TopicID)