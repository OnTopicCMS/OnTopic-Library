CREATE FUNCTION [dbo].[topics_GetParentID](@TopicID int)
RETURNS int
AS
BEGIN
  DECLARE	@CurrentParentID		int
  SELECT       	@CurrentParentID = (
    SELECT	TOP 1
		TopicID
    FROM	topics_Topics t2
    WHERE	t2.RangeLeft < t1.RangeLeft
      AND	ISNULL(t2.RangeRight, 0) > ISNULL(t1.RangeRight, 0)
    ORDER BY	t2.RangeRight-t1.RangeRight ASC
  )
  FROM		topics_Topics t1
  WHERE		TopicID = @TopicID
  ORDER BY	RangeRight-RangeLeft DESC
  RETURN	@CurrentParentID
END