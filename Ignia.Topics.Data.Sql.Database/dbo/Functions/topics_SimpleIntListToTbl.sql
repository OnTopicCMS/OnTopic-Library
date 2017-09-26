CREATE FUNCTION [dbo].[topics_SimpleIntListToTbl] (@list nvarchar(MAX))
RETURNS @tbl TABLE (number int NOT NULL) AS
BEGIN
  DECLARE	@pos		int,
		@nextpos	int,
		@valuelen	int,
		@value		varchar(20)

  SELECT	@pos		= 0,
		@nextpos	= 1

  WHILE		@nextpos	> 0
  BEGIN
    SELECT	@nextpos	= charindex(',', @list, @pos + 1)
    SELECT	@valuelen	= CASE
      WHEN	@nextpos	> 0
      THEN	@nextpos
      ELSE	len(@list) + 1
      END	- @pos - 1
    IF		@valuelen = 0	CONTINUE
    INSERT	@tbl (number)
      VALUES	(convert(int, substring(@list, @pos + 1, @valuelen)))
      SELECT	@pos		= @nextpos
   END
 RETURN
END