CREATE FUNCTION [dbo].[Split] (@s varchar(8000), @sep char(1))
RETURNS table
AS
RETURN (
  WITH		Pieces(pn, start, stop) AS (
    SELECT	1, 1, CHARINDEX(@sep, @s)
    UNION	ALL
    SELECT	pn + 1, stop + 1, CHARINDEX(@sep, @s, stop + 1)
    FROM	Pieces
    WHERE	stop > 0
  )
  SELECT	pn,
		SUBSTRING(@s, start, CASE WHEN stop > 0 THEN stop-start ELSE 8000 END) AS s
    FROM	Pieces
)