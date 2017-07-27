-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	MOVE TOPICS
--
-- Purpose	Moves a topic and all of its children underneath another topic.
--
-- History	John Mulhausen		06302009  Created initial version.
--		Jeremy Caney		05282010  Reformatted code and refactored identifiers for improved readability. 
--              Hedley Robertson	07062010  Added support for SiblingID (Ordering)
--              Hedley Robertson	08122010  Inline test cases, debugging statements and check for re-parenting; 
--						  now avoids moving item to start of SET when sibling move requested 
--						  and parentid has not changed
--              Hedley Robertson	08172010  Rebuilt with externalized MoveSubTree function
--		Jeremy Caney		09222014  Updated logic for ParentID attribute to be based on Key, not ID
-----------------------------------------------------------------------------------------------------------------------------------------------
-- See EOF for inline manual test cases
-----------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_MoveTopic]
		@TopicID		INT			,
		@ParentID		INT			,
		@SiblingID		INT		= -1
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
DECLARE		@RangeLeft		INT
DECLARE		@RangeRight		INT
DECLARE		@RangeWidth		INT
DECLARE		@ParentRangeLeft	INT 
DECLARE		@ParentRangeRight	INT
DECLARE		@LeftBoundary		INT 
DECLARE		@RightBoundary		INT
DECLARE		@OffSET			INT 
DECLARE		@leftrange		INT 
DECLARE		@rightrange		INT
DECLARE		@SiblingLeft		INT		= -1
DECLARE		@SiblingRight		INT		= -1
DECLARE		@SiblingIDLeft		INT		= -1
DECLARE		@SiblingOffSET		INT


SET NOCOUNT ON;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DEFINE RANGE AND PARENT RANGE
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		@RangeLeft		= RangeLeft,
		@RangeRight		= RangeRight
FROM		topics_Topics 
WHERE		TopicID			= @TopicID

SELECT		@ParentRangeLeft	= RangeLeft,
		@ParentRangeRight	= RangeRight
FROM		topics_Topics 
WHERE		TopicID			= @ParentID


-----------------------------------------------------------------------------------------------------------------------------------------------
-- DETERMINE DIRECTION AND BOUNDARY OFFSET
-----------------------------------------------------------------------------------------------------------------------------------------------
If @RangeLeft > @ParentRangeLeft -- move up!
  BEGIN
    SET		@OffSET			= @ParentRangeLeft - @RangeLeft + 1; -- 2 - 42 + 1 = -39
    SET		@LeftBoundary		= @ParentRangeLeft + 1;              -- 3
    SET		@RightBoundary		= @RangeLeft - 1;                    -- 41
    SET		@RangeWidth		= @RangeRight - @RangeLeft + 1;      -- 57 - 42 + 1 = 16
    SET		@leftrange		= @RangeRight;                       -- 57
    SET		@rightrange		= @ParentRangeLeft;                  --  2  
  END
ELSE -- move down!
  BEGIN
    SET		@OffSET			= @ParentRangeLeft - @RangeRight;
    SET		@LeftBoundary		= @RangeRight + 1;
    SET		@RightBoundary		= @ParentRangeLeft;
    SET		@RangeWidth		= @RangeLeft - @RangeRight - 1;
    SET		@leftrange		= @ParentRangeLeft + 1;
    SET		@rightrange		= @RangeLeft;
  END 
-----------------------------------------------------------------------------------------------------------------------------------------------
-- UPDATE BOUNDARIES TO REFLECT OFFSET, UNLESS WE ARE JUST MOVING WITHIN SAME PARENT AND SIBLING REQUESTED!
-----------------------------------------------------------------------------------------------------------------------------------------------
DECLARE		@CurrentParentID	INT

SELECT		@CurrentParentID        = AttributeValue
FROM		topics_TopicAttributes
WHERE		TopicID			= @TopicID
And		AttributeKey		= 'ParentID'

-- Try to find missing parent ID
IF		@CurrentParentID	IS null 
  OR		@CurrentParentID	< 0
BEGIN
  SELECT	@CurrentParentID	= dbo.topics_GetParentID(@TopicID)                                        
END

IF		@CurrentParentID	IS null 
  OR		@CurrentParentID	< 0
BEGIN
  RAISERROR (	N'FAILED TO FIND PARENT ID!',
		15,			-- Severity.
		1			-- State.
  );
END

IF		(@CurrentParentID	IS null) 
  OR (		@CurrentParentID	= @ParentID 
    AND		@SiblingID		> -1
  )
  BEGIN
    RAISERROR (	N'The Parent is the same (%d) and sibling ID (%d) requested, do NOT reparent item.  Moving a tree that contains a sibling to be under that same ... sibling... :S',
		10,			-- Severity.
		1,			-- State.
		@ParentID, 
		@SiblingID
    );    
  END
ELSE
  BEGIN

    RAISERROR (		N'Reparenting item',
			10,			-- Severity.
			1			-- State.
    );

    UPDATE		topics_Topics
    SET			RangeLeft		= 
      CASE
        WHEN (		RangeLeft 
          BETWEEN	@LeftBoundary 
          AND		@RightBoundary
        ) 
        THEN		RangeLeft		+ @RangeWidth
        WHEN  (		RangeLeft 
          BETWEEN	@RangeLeft 
          AND		@RangeRight
        ) 
        THEN		RangeLeft		+ @OffSET
        ELSE		RangeLeft 
      END,
			RangeRight		= 
      CASE
        WHEN (		RangeRight 
          BETWEEN	@LeftBoundary 
          AND		@RightBoundary
        ) 
        THEN		RangeRight		+ @RangeWidth
        WHEN (		RangeRight 
          BETWEEN	@RangeLeft 
          AND		@RangeRight
        ) 
        THEN		RangeRight		+ @OffSET
        ELSE		RangeRight 
      END
    WHERE		RangeLeft		< @leftrange
      OR		RangeRight		> @rightrange
  END
-----------------------------------------------------------------------------------------------------------------------------------------------
-- MOVE BEHIND SIBLING
-----------------------------------------------------------------------------------------------------------------------------------------------
IF		@SiblingID		>= 0 -- SiblingID is who we want to be in BEHIND of....
  BEGIN

	-- Determine sibling details
    SELECT	@SiblingLeft		= RangeLeft,
		@SiblingRight		= RangeRight
    FROM	topics_Topics 
    WHERE	TopicID			= @SiblingID
	
	
    DECLARE	@TargetLeft		INT

    SET		@TargetLeft		= @SiblingRight + 1;
	

    EXEC	topics_MoveSubtree	@TopicID, @TargetLeft
	
    EXEC	topics_CompressTopics;
    
  END

-----------------------------------------------------------------------------------------------------------------------------------------------
-- UPDATE PARENT ID
-----------------------------------------------------------------------------------------------------------------------------------------------
UPDATE		topics_TopicAttributes
SET		AttributeValue		= @ParentID
WHERE		TopicID			= @TopicID
  AND		AttributeKey		= 'ParentID'

-- for debugging remove this later:

SELECT		@SiblingID		SiblingID, 
		@LeftBoundary		LeftBoundary, 
		@RightBoundary		RightBoundary, 
		@ParentRangeLeft	ParentRangeLeft,
		@ParentRangeRight	ParentRangeRight, 
		@RangeLeft		RangeLeft, 
		@RangeRight		RangeRight,
		@leftrange		leftrange, 
		@rightrange		rightrange, 
		@SiblingOffSET		SiblingOffSET, 
		@CurrentParentID	CurrentParentID