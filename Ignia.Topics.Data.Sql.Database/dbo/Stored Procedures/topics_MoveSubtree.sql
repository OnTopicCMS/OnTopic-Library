-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	MOVE SUBTREE
--
-- Purpose	Moves an entire tree elsewhere in the hierarchy.
--
-- History	Casey Margell		04062009  Initial Creation baseaed on code from Celko's Sql For Smarties.
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_MoveSubtree]
		@TopicID	INT	,	-- The source node/subtree
		@TargetLeft	INT		-- The destination left-value
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------

DECLARE		@NewPos		INT
DECLARE		@SrcLeft	INT
DECLARE		@SrcRight	INT
DECLARE		@TreeSize	INT

-- Determine source details
SELECT		@SrcLeft	= RangeLeft,
		@SrcRight	= RangeRight
FROM		topics_Topics 
WHERE		TopicID		= @TopicID

SET		@TreeSize	= @SrcRight - @SrcLeft + 1;
	
--- make space in the destination range
EXEC		topics_ShiftRLValues @TargetLeft, @TreeSize

--- src was shifted too? --
IF		@SrcLeft	>= @TargetLeft
BEGIN
  SET		@SrcLeft	= @SrcLeft + @TreeSize;
  SET		@SrcRight	= @SrcRight + @TreeSize;
END

-- Determine source details, again
SELECT		@SrcLeft	= RangeLeft,
		@SrcRight	= RangeRight
FROM		topics_Topics 
WHERE		TopicID		= @TopicID

SET		@TreeSize	= @SrcRight - @SrcLeft + 1;

-- now there's enough room next to target to move the subtree
DECLARE		@First		INT
DECLARE		@Last		INT
DECLARE		@Delta		INT

SET		@First		= @SrcLeft;
SET     	@Last		= @SrcRight;
SET	     	@Delta		= @TargetLeft - @SrcLeft;

-- shiftRLRange(srcLeft, srcRight, arguments.to - srcLeft)>

UPDATE		topics_Topics
SET		RangeLeft	= RangeLeft + @Delta
WHERE		RangeLeft	>= @First
  AND		RangeLeft	<= @Last

UPDATE		topics_Topics
SET		RangeRight	= RangeRight + @Delta
WHERE		RangeRight	>= @First
  AND		RangeRight	<= @Last

DECLARE		@NewPosLeft	INT
DECLARE		@NewPosRight	INT

SET		@NewPosLeft	= @First + @Delta;
SET		@NewPosRight	= @Last  + @Delta;

DECLARE		@NewRight	INT
SET		@NewRight	= @SrcRight  + 1;

DECLARE		@InvTreeSize	INT
SET		@InvTreeSize	= @TreeSize  * -1;

-- correct values after source
EXEC		topics_ShiftRLValues @SrcRight, @InvTreeSize