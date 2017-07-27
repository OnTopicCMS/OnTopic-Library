-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	SWAP SIBLINGS
--
-- Purpose	Switches a node (and descendants) with a sibling node
--
-- History	Hedley Robertson	07062010  Added support for SiblingID (Ordering)
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_SwapSiblings]
		@Lft_Sibling		INT,
		@Rgt_Sibling		INT
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
DECLARE		@i0			INTEGER;
DECLARE		@i1			INTEGER;
DECLARE		@i2			INTEGER;
DECLARE		@i3			INTEGER;

SET		@i0			= (SELECT RangeLeft  FROM topics_Topics WHERE TopicID = @Lft_Sibling);
SET		@i1			= (SELECT RangeRight FROM topics_Topics WHERE TopicID = @Lft_Sibling);
SET		@i2			= (SELECT RangeLeft  FROM topics_Topics WHERE TopicID = @Rgt_Sibling);
SET		@i3			= (SELECT RangeRight FROM topics_Topics WHERE TopicID = @Rgt_Sibling); 

UPDATE		topics_Topics
  SET		RangeLeft = 
    CASE 
      WHEN	RangeLeft 
        BETWEEN @i0 
	AND	@i1
        THEN	@i3 + RangeLeft - @i1
      WHEN	RangeLeft 
        BETWEEN	@i2 
	AND	@i3
        THEN	@i0 + RangeLeft - @i2
      ELSE	@i0 + @i3 + RangeLeft - @i1 - @i2 
    END, 
		RangeRight = 
    CASE 
      WHEN	RangeRight 
        BETWEEN	@i0 
        AND	@i1
        THEN	@i3 + RangeRight - @i1
      WHEN	RangeRight 
        BETWEEN	@i2 
	AND	@i3
        THEN	@i0 + RangeRight - @i2
      ELSE	@i0 + @i3 + RangeRight - @i1 - @i2 
    END
WHERE		RangeLeft 
  BETWEEN	@i0 
  AND		@i3
  AND		@i0 < @i1
  AND		@i1 < @i2
  AND		@i2 < @i3;