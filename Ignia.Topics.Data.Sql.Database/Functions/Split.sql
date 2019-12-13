--------------------------------------------------------------------------------------------------------------------------------
-- SPLIT (FUNCTION)
--------------------------------------------------------------------------------------------------------------------------------
-- Given a string and a delimited, will parse the string by the delimiter and return a table as a result. Preferably, we should
-- instead use user-defined table valued types, such as AttributeValues and TopicList.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
FUNCTION	[dbo].[Split] (
	@s		varchar(8000),
	@sep		char(1)
)
RETURNS	TABLE
AS
RETURN (
  WITH	Pieces(
	  pn,
	  start,
	  stop
	)
  AS (
    SELECT	1,
	1,
	CHARINDEX(@sep, @s)
    UNION	ALL
    SELECT	pn + 1,
	stop + 1,
	CHARINDEX(@sep, @s, stop + 1)
    FROM	Pieces
    WHERE	stop > 0
  )
  SELECT	pn,
	SUBSTRING(@s, start, CASE WHEN stop > 0 THEN stop-start ELSE 8000 END) AS s
    FROM	Pieces
)