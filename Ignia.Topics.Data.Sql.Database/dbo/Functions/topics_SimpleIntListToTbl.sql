--------------------------------------------------------------------------------------------------------------------------------
-- SIMPLE INT LIST TO TABLE (FUNCTION)
--------------------------------------------------------------------------------------------------------------------------------
-- Given a comma delimited list of integers, such as TopicIDs, parses them into a table. Similar to the Split function, but more
-- specialized. That said, we should prefer using user-defined table valued types to relay lists of values, such as the
-- TopicList type.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
FUNCTION	[dbo].[topics_SimpleIntListToTbl] (
	@list		NVARCHAR(MAX)
)
RETURNS	@tbl		TABLE (
	number		INT	NOT NULL
)
AS
BEGIN
  DECLARE	@pos		INT,
	@nextpos		INT,
	@valuelen		INT,
	@value		VARCHAR(20)

  SELECT	@pos		= 0,
	@nextpos		= 1

  WHILE	@nextpos		> 0
  BEGIN
    SELECT	@nextpos		= charindex(',', @list, @pos + 1)
    SELECT	@valuelen		= CASE
      WHEN	@nextpos		> 0
      THEN	@nextpos
      ELSE	len(@list)		+ 1
      END	- @pos		- 1
    IF	@valuelen		= 0
      CONTINUE
    INSERT	@tbl (
	  number
	)
      VALUES (	convert(int, substring(@list, @pos + 1, @valuelen))
      )
      SELECT	@pos		= @nextpos
   END
 RETURN
END