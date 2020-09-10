--------------------------------------------------------------------------------------------------------------------------------
-- GENERATE NESTED SET
--------------------------------------------------------------------------------------------------------------------------------
-- Creates an adjacency list using the _ParentID fields in Attributes then takes the newly created adjacency list
-- and uses it to generate a nested set based table in Topics.  Useful for recovering from a corrupted nested set model.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[GenerateNestedSet]
AS


--------------------------------------------------------------------------------------------------------------------------------
-- RECREATE ADJACENCY LIST
--------------------------------------------------------------------------------------------------------------------------------

-- Delete any prexisting content
DELETE
FROM	AdjacencyList

-- Insert data from Topics, Attributes
INSERT
INTO	AdjacencyList
SELECT	Topics.TopicID,
	CONVERT(
	  INT,
	  AttributeValue
	) AS		ParentID,
	GETDATE(),
	RangeLeft
FROM	Topics
LEFT JOIN	Attributes
  ON	Attributes.TopicID	= Topics.TopicID
  AND	ISNULL(
	  Attributes.AttributeKey,
	  'ParentID'
	)		= 'ParentID'
ORDER BY	RangeLeft

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE TEMPORARY STORAGE
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	#Topics (
	  [TopicID]		INT	NOT NULL,
	  [RangeLeft]		INT	NOT NULL,
	  [RangeRight]		INT	NULL,
  	  [Stack_Top]		INT	NULL
);

--------------------------------------------------------------------------------------------------------------------------------
-- Celko's conversion model (not properly formatted; copied from Celko)
--------------------------------------------------------------------------------------------------------------------------------
BEGIN
  DECLARE	@RangeLeft_RangeRight	INTEGER,
	@pointer		INTEGER,
	@max_RangeLeft_RangeRight	INTEGER;

SET	@max_RangeLeft_RangeRight	= 2 * (
  SELECT	COUNT(*)
  FROM	AdjacencyList
);

INSERT
INTO	#Topics (
	  TopicID,
	  RangeLeft,
	  RangeRight,
	  Stack_Top
	)
SELECT	TopicID,
	1,
	@max_RangeLeft_RangeRight,
	1
FROM	AdjacencyList
WHERE	Parent_TopicID		IS NULL;

SET	@RangeLeft_RangeRight	= 2;
SET	@pointer		= 1;

DELETE
FROM	AdjacencyList
WHERE	Parent_TopicID		IS NULL;

-- The topics is now loaded and ready to use

WHILE	(@RangeLeft_RangeRight < @max_RangeLeft_RangeRight)
BEGIN
  IF EXISTS (
    SELECT	*
    FROM	#Topics		AS S1
    JOIN	AdjacencyList		AS T1
      ON	S1.TopicID		= T1.Parent_TopicID
      AND	S1.Stack_Top		= @pointer
  )
    BEGIN

      -- push when Stack_Top has subordinates and set RangeLeft value
      INSERT
      INTO	#Topics (
	  TopicID,
	  RangeLeft,
	  RangeRight,
	  Stack_Top
	)
      SELECT	TOP 1
	T1.TopicID,
	@RangeLeft_RangeRight,
	NULL,
	@pointer + 1
      FROM	#Topics		AS S1
      JOIN	AdjacencyList		AS T1
        ON	S1.TopicID		= T1.Parent_TopicID
        AND	S1.Stack_Top		= @pointer
      ORDER BY	T1.SortOrder,
	T1.TopicID

      -- remove this row from hierarchy
      DELETE
      FROM	AdjacencyList
      WHERE	TopicID		= (
        SELECT	TopicID
        FROM	#Topics
        WHERE	ISNULL(Stack_Top, 0)	= @pointer + 1);
      SET	@pointer = @pointer + 1;
    END -- push
  ELSE
    BEGIN
      -- pop the topics and set RangeRight value
      UPDATE	#Topics
      SET	RangeRight		= @RangeLeft_RangeRight,
	Stack_Top		= -Stack_Top
      WHERE	ISNULL(Stack_Top, 0)	= @pointer
      SET	@pointer		= @pointer - 1;
    END; -- pop
    SET	@RangeLeft_RangeRight	= @RangeLeft_RangeRight + 1;
  END; -- if
END; -- while

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE EXISTING TOPIC GRAPH
--------------------------------------------------------------------------------------------------------------------------------

-- Disable constraints
ALTER
TABLE	Attributes
NOCHECK
CONSTRAINT	FK_Attributes_TopicID;

ALTER
TABLE	ExtendedAttributes
NOCHECK
CONSTRAINT	FK_ExtendedAttributes_Topics;

ALTER
TABLE	Relationships
NOCHECK
CONSTRAINT	FK_Relationships_Source;

ALTER
TABLE	Relationships
NOCHECK
CONSTRAINT	FK_Relationships_Target;

-- Delete topics
DELETE
FROM	Topics

--------------------------------------------------------------------------------------------------------------------------------
-- RECREATE TOPIC GRAPH
--------------------------------------------------------------------------------------------------------------------------------

SET IDENTITY_INSERT Topics ON

-- Insert topics
INSERT
INTO	Topics (
	  TopicID,
	  RangeLeft,
	  RangeRight
	)
SELECT	TopicID,
	RangeLeft,
	RangeRight
FROM	#Topics
ORDER BY	TopicID;

SET IDENTITY_INSERT Topics OFF

-- Reenable constraints
ALTER
TABLE	Attributes
CHECK
CONSTRAINT	FK_Attributes_TopicID;

ALTER
TABLE	ExtendedAttributes
CHECK
CONSTRAINT	FK_ExtendedAttributes_Topics;

ALTER
TABLE	Relationships
CHECK
CONSTRAINT	FK_Relationships_Source;

ALTER
TABLE	Relationships
CHECK
CONSTRAINT	FK_Relationships_Target;

--------------------------------------------------------------------------------------------------------------------------------
-- Drop temporary table
--------------------------------------------------------------------------------------------------------------------------------
DROP TABLE	#Topics

