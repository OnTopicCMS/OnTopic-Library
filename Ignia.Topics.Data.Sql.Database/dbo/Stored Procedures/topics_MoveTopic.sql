--------------------------------------------------------------------------------------------------------------------------------
-- MOVE TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- Moves a topic and all of its children underneath another topic.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_MoveTopic]
	@TopicID		INT		,
	@ParentID		INT		,
	@SiblingID		INT	= -1
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@OriginalLeft		INT
DECLARE	@OriginalRight		INT
DECLARE	@InsertionPoint		INT
DECLARE	@OriginalRange		INT
DECLARE	@Offset		INT
DECLARE	@IsNestedTransaction	BIT;

BEGIN TRY

--------------------------------------------------------------------------------------------------------------------------------
-- BEGIN TRANSACTION
--------------------------------------------------------------------------------------------------------------------------------
IF (@@TRANCOUNT = 0)
  BEGIN
    SET @IsNestedTransaction = 0;
    BEGIN TRANSACTION;
  END
ELSE
  BEGIN
    SET @IsNestedTransaction = 1;
  END

--------------------------------------------------------------------------------------------------------------------------------
-- DEFINE SOURCE RANGE
--------------------------------------------------------------------------------------------------------------------------------
-- The source range defines the original boundaries (@OriginalLeft, @OriginalRight) and the width (@OriginalRange) of the source
-- subtree (@TopicID), which will be used to determine what nodes to move.
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@OriginalRange		= RangeRight - RangeLeft + 1,
	@OriginalLeft		= RangeLeft,
	@OriginalRight		= RangeRight
FROM	topics_Topics
WHERE	TopicID		= @TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- DEFINE INSERTION POINT
--------------------------------------------------------------------------------------------------------------------------------
-- The insertion point (@InsertionPoint) is the location where the source subtree is to be moved to. If a sibling (@SiblingID)
-- is defined then the insertion point is immediately to the right of the sibling's right edge; if no sibling is defined, then
-- the insertion point is the first location within the parent (@ParentID)'s range.
--------------------------------------------------------------------------------------------------------------------------------
-- EXAMPLE: If a sibling (@SiblingID) lives between 12 and 24, then the insertion point (@InsertionPoint) will be 25; if there
-- is no sibling, but a parent lives between 6 and 26, then the insertion point (@InsertionPoint) will be 7.
--------------------------------------------------------------------------------------------------------------------------------
IF @SiblingID < 0
  -- Place as the first sibling if a sibling isn't specified
  SELECT	@InsertionPoint		= RangeLeft + 1
  FROM	topics_Topics
  WHERE	TopicID			= @ParentID
ELSE
  -- Place immediately to the right of a sibling, if specified
  SELECT	@InsertionPoint		= RangeRight + 1
  FROM	topics_Topics
  WHERE	TopicID		= @SiblingID

--------------------------------------------------------------------------------------------------------------------------------
-- VALIDATE REQUEST
--------------------------------------------------------------------------------------------------------------------------------
-- Ensure that the source tree (@TopicID) exists (thus resulting in a valid @OriginalLeft and @OriginalRight) and that the
-- target location (@InsertionPoint) is not within the scope of the source tree (@TargetID); a tree cannot be moved to a child
-- of itself.
--------------------------------------------------------------------------------------------------------------------------------
IF @TopicID is null or @OriginalLeft is null or @OriginalRight is null
  BEGIN
    RAISERROR (
      N'The topic ("%n") could not be found.',
      15, -- Severity,
      1, -- State,
      @TopicID
    );
    RETURN
  END

IF @ParentID is null or @InsertionPoint is null
  BEGIN
    RAISERROR (
      N'The parent ("%n") could not be found.',
      15, -- Severity,
      1, -- State,
      @ParentID
    );
    RETURN
  END

IF @InsertionPoint >= @OriginalLeft AND @InsertionPoint <= @OriginalRight
  BEGIN
    RAISERROR (
      N'A topic ("%n") cannot be moved within a child of itself ("%n").',
      10, -- Severity,
      1, -- State,
      @TopicID,
      @ParentID
    );
    RETURN
  END

--------------------------------------------------------------------------------------------------------------------------------
-- DEFINE OFFSET
--------------------------------------------------------------------------------------------------------------------------------
-- The offset is the space between the source (@TopicID) and the target (@InsertionPoint), excluding the range (@OriginalRange)
-- itself. If a subtree is being moved to the left, then that's the gap between the target (@InsertionPoint) and the subtree's
-- left boundary (@OriginalLeft); if a subtree is being moved to the right, then that's the gap between the subtree's right
-- boundary (@OriginalRight) and the target (@InsertionPoint).
--------------------------------------------------------------------------------------------------------------------------------
-- EXAMPLE: If you have a source subtree (@TopicID) with a range of (52-58) and you're moving it to a start position of 25, then
-- the @Offset will be 27 (i.e., 52-25). If you are moving it to a start position of 75, however, then the @Offset will be 17
-- (75-58).
--------------------------------------------------------------------------------------------------------------------------------
SET	@Offset =
  CASE
    WHEN	@InsertionPoint < @OriginalLeft
    THEN	@OriginalLeft - @InsertionPoint
  ELSE	@InsertionPoint - @OriginalRight
  END

--------------------------------------------------------------------------------------------------------------------------------
-- MOVE SOURCE RANGE TO INSERTION POINT
--------------------------------------------------------------------------------------------------------------------------------
-- The basic idea behind moving nodes is that we're going to a) shift the target subtree (@Parent) by the delta (@Offset)
-- between its original position (@OriginalLeft, @OriginalRight) and the target location (@InsertionPoint), while b) closing the
-- gap left behind by shifting all intermediate nodes by the width of the target subtree (@OriginalRange).
--------------------------------------------------------------------------------------------------------------------------------
-- EXAMPLE: If we're moving a target subtree of width 12 down 26 nodes, then we'd a) subtract 26 (the @Offset) from all nodes
-- between RangeLeft (@OriginalLeft) and RangeRight (@OriginalRight) of the subtree (to move it to its new position), while b)
-- adding 12 (the @OriginalRange) to all nodes between the target location (@InsertionPoint) and the original subtree location
-- (either @OriginalLeft or @OriginalRight, depending on whether the tree has been moved up or down).
--------------------------------------------------------------------------------------------------------------------------------

UPDATE	topics_Topics
SET	RangeLeft = RangeLeft +
  CASE

  ------------------------------------------------------------------------------------------------------------------------------
  -- MOVE SOURCE TOPIC TO ITS LEFT
  ------------------------------------------------------------------------------------------------------------------------------

  WHEN	@InsertionPoint < @OriginalLeft
  THEN
    CASE

    -- Shift source topic (and children) left to its new location

    WHEN (	RangeLeft
      BETWEEN	@OriginalLeft
      AND	@OriginalRight
    )
    THEN	- @Offset

    -- Shift items between the destination and source topic to the right
    -- This pushes everything back by the width of the source topic, filling the gap it left behind
    -- This also makes room for the above shift of the source topic, preventing duplicate ranges

    WHEN (	RangeLeft
      BETWEEN	@InsertionPoint
      AND	@OriginalLeft
    )
    THEN	@OriginalRange
    ELSE	0

    END

  ------------------------------------------------------------------------------------------------------------------------------
  -- MOVE SOURCE TOPIC TO ITS RIGHT
  ------------------------------------------------------------------------------------------------------------------------------

  WHEN	@InsertionPoint > @OriginalRight
  THEN
    CASE

    -- Shift source topic (and children) right to its new location
    -- When shifting to the right, an offset of -1 is required to account for implied padding between numbers

    WHEN (	RangeLeft
      BETWEEN	@OriginalLeft
      AND	@OriginalRight
    )
    THEN	@Offset - 1

    -- Shift items between the source topic and the destination to the left
    -- This pulls everything back by the width of the source topic, filling the gap it left behind
    -- This also makes room for the above shift of the source topic, preventing duplicate ranges
    -- Because between is inclusive, includes offsets to only cover intermediate records

    WHEN (	RangeLeft
      BETWEEN	@OriginalRight + 1
      AND	@InsertionPoint - 1
    )
    THEN	- @OriginalRange
    ELSE	0

    END

  END,

	RangeRight = RangeRight +
  CASE

  ------------------------------------------------------------------------------------------------------------------------------
  -- MOVE SOURCE TOPIC TO ITS LEFT
  ------------------------------------------------------------------------------------------------------------------------------

  WHEN	@InsertionPoint < @OriginalLeft
  THEN
    CASE

    -- Shift source topic (and children) left to its new location

    WHEN (	RangeRight
      BETWEEN	@OriginalLeft
      AND	@OriginalRight
    )
    THEN	- @Offset

    -- Shift items between the destination and source topic to the right
    -- This pushes everything back by the width of the source topic, filling the gap it left behind
    -- This also makes room for the above shift of the source topic, preventing duplicate ranges

    WHEN (	RangeRight
      BETWEEN	@InsertionPoint
      AND	@OriginalLeft - 1
    )
    THEN	@OriginalRange
    ELSE	0

    END

  ------------------------------------------------------------------------------------------------------------------------------
  -- MOVE SOURCE TOPIC TO ITS RIGHT
  ------------------------------------------------------------------------------------------------------------------------------

  WHEN	@InsertionPoint > @OriginalRight
  THEN
    CASE

    -- Shift source topic (and children) right to its new location
    -- When shifting to the right, an offset of -1 is required to account for implied padding between numbers

    WHEN (	RangeRight
      BETWEEN	@OriginalLeft
      AND	@OriginalRight
    )
    THEN	@Offset - 1

    -- Shift items between the source topic and the destination to the left
    -- This pulls everything back by the width of the source topic, filling the gap it left behind
    -- This also makes room for the above shift of the source topic, preventing duplicate ranges
    -- Because between is inclusive, includes offsets to only cover intermediate records

    WHEN (	RangeRight
      BETWEEN	@OriginalRight + 1
      AND	@InsertionPoint - 1
    )
    THEN	- @OriginalRange
    ELSE	0

    END

  END

--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE PARENT ID
--------------------------------------------------------------------------------------------------------------------------------
UPDATE	topics_TopicAttributes
SET	AttributeValue		= CONVERT(NVarChar(255), @ParentID)
WHERE	TopicID		= @TopicID
  AND	AttributeKey		= 'ParentID'

--------------------------------------------------------------------------------------------------------------------------------
-- DEBUGGING DATA
--------------------------------------------------------------------------------------------------------------------------------
SELECT	@TopicID		TopicID,
	@ParentID		ParentID,
	@SiblingID		SiblingID,
	@OriginalLeft		OriginalLeft,
	@OriginalRight		OriginalRight,
	@InsertionPoint		InsertionPoint,
	@OriginalRange		OriginalRange,
	@Offset		Offset


--------------------------------------------------------------------------------------------------------------------------------
-- COMMIT TRANSACTION
--------------------------------------------------------------------------------------------------------------------------------
IF (@@TRANCOUNT > 0 AND @IsNestedTransaction = 0)
  BEGIN
    COMMIT
  END

END TRY

--------------------------------------------------------------------------------------------------------------------------------
-- HANDLE ERRORS
--------------------------------------------------------------------------------------------------------------------------------
BEGIN CATCH
  IF (@@TRANCOUNT > 0 AND @IsNestedTransaction = 0)
    BEGIN
      ROLLBACK;
    END;
  THROW
  RETURN;
END CATCH