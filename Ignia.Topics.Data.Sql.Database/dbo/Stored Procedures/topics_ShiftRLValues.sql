-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	SHIFT RIGHT-LEFT VALUES
--
-- Purpose	Shifts the right and left index values after the position '@First'
--
-- History	Hedley Robertson	08162010  Created initial version.
-----------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_ShiftRLValues]
		@First		INT	,  -- The destination left-value
		@Delta		INT	-- Width to shift
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- UPDATE RANGE
-----------------------------------------------------------------------------------------------------------------------------------------------
UPDATE		topics_Topics
SET		RangeLeft	= RangeLeft + @Delta
WHERE		RangeLeft	>= @First

UPDATE		topics_Topics
SET		RangeRight	= RangeRight + @Delta
WHERE		RangeRight	>= @First