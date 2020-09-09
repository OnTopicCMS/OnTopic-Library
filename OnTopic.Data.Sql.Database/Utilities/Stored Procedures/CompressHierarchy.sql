--------------------------------------------------------------------------------------------------------------------------------
-- COMPRESS HIERARCHY
--------------------------------------------------------------------------------------------------------------------------------
-- Remove gaps within nested set model created when non-leaf nodes are deleted
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [Utilities].[CompressHierarchy]
AS

SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- MIND THE GAP!
--------------------------------------------------------------------------------------------------------------------------------
UPDATE	Topics
SET	RangeLeft = (
  SELECT	COUNT(*)
  FROM	[Utilities].[LeftRightRange]
  WHERE	seq <= RangeLeft
),
	RangeRight = (
  SELECT	COUNT(*)
  FROM	[Utilities].[LeftRightRange]
  WHERE	seq <= ISNULL(RangeRight, 0)
);