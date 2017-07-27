-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	COMPRESS TOPICS
--
-- Purpose	Remove gaps within nested set model created when non-leaf nodes are deleted
--
-- History	Hedley Robertson	06222010  Created initial version.
-----------------------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_CompressTopics]
AS

SET NOCOUNT ON;

-----------------------------------------------------------------------------------------------------------------------------------------------
-- MIND THE GAP!
-----------------------------------------------------------------------------------------------------------------------------------------------
UPDATE		topics_Topics 
SET 
  RangeLeft	= (SELECT COUNT(*) FROM topics_LftRgt WHERE seq <= RangeLeft),
  RangeRight	= (SELECT COUNT(*) FROM topics_LftRgt WHERE seq <= RangeRight);