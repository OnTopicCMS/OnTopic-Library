--------------------------------------------------------------------------------------------------------------------------------
-- GENERATE NESTED SET
--------------------------------------------------------------------------------------------------------------------------------
-- Creates an adjacency list using the _ParentID fields in Attributes then takes the newly created adjacency list
-- and uses it to generate a nested set based table in Topics.  Useful for recovering from a corrupted nested set model.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[GenerateNestedSet]
AS

SET IDENTITY_INSERT Topics ON

--------------------------------------------------------------------------------------------------------------------------------
-- RECREATE TOPICS_HIERARCHY
--------------------------------------------------------------------------------------------------------------------------------
-- Delete original content
DELETE
FROM	Hierarchy

-- Insert data from Attributes
INSERT
INTO	Hierarchy
SELECT	TopicID		AS TopicID,
	CONVERT(Int, AttributeValue)	AS ParentID,
	GETDATE()		AS DateAdded
FROM	Attributes
WHERE	AttributeKey		= 'ParentID'

-- Address root node
UPDATE	Hierarchy
SET	Parent_TopicID		= null
WHERE	ISNULL(Parent_TopicID, -1)	= -1

--------------------------------------------------------------------------------------------------------------------------------
-- Celko's conversion model (not properly formatted; copied from Celko)
--------------------------------------------------------------------------------------------------------------------------------
BEGIN
  DECLARE	@RangeLeft_RangeRight	INTEGER,
	@pointer		INTEGER,
	@max_RangeLeft_RangeRight	INTEGER;

SET	@max_RangeLeft_RangeRight	= 2 * (
  SELECT	COUNT(*)
  FROM	Hierarchy
);

INSERT
INTO	Topics (
	  Stack_Top,
	  TopicID,
	  RangeLeft,
	  RangeRight
	)
SELECT	1,
	TopicID,
	1,
	@max_RangeLeft_RangeRight
FROM	Hierarchy
WHERE	Parent_TopicID		IS NULL;

SET	@RangeLeft_RangeRight	= 2;
SET	@pointer	= 1;

DELETE
FROM	Hierarchy
WHERE	Parent_TopicID		IS NULL;

-- The topics is now loaded and ready to use

WHILE	(@RangeLeft_RangeRight < @max_RangeLeft_RangeRight)
BEGIN
  IF EXISTS (
    SELECT	*
    FROM	Topics		AS S1
    JOIN	Hierarchy		AS T1
      ON	S1.TopicID		= T1.Parent_TopicID
      AND	S1.Stack_Top		= @pointer
  )
    BEGIN

      -- push when Stack_Top has subordinates and set RangeLeft value
      INSERT
      INTO	Topics (
	  Stack_Top,
	  TopicID,
	  RangeLeft,
	  RangeRight
	)
      SELECT	(@pointer + 1),
	MIN(T1.TopicID),
	@RangeLeft_RangeRight,
	NULL
      FROM	Topics		AS S1
      JOIN	Hierarchy		AS T1
        ON	S1.TopicID		= T1.Parent_TopicID
        AND	S1.Stack_Top		= @pointer;

      -- remove this row from hierarchy
      DELETE
      FROM	hierarchy
      WHERE	TopicID		= (
        SELECT	TopicID
        FROM	Topics
        WHERE	ISNULL(Stack_Top, 0)	= @pointer + 1);
      SET	@pointer = @pointer + 1;
    END -- push
  ELSE
    BEGIN
      -- pop the topics and set RangeRight value
      UPDATE	Topics
      SET	RangeRight		= @RangeLeft_RangeRight,
	Stack_Top		= -Stack_Top
      WHERE	ISNULL(Stack_Top, 0)	= @pointer
      SET	@pointer		= @pointer - 1;
    END; -- pop
    SET	@RangeLeft_RangeRight	= @RangeLeft_RangeRight + 1;
  END; -- if
END; -- while

SELECT	Stack_Top,
	TopicId,
	RangeLeft,
	RangeRight
FROM	Topics
ORDER BY	RangeLeft;

SET IDENTITY_INSERT Topics OFF
