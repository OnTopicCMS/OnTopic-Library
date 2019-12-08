--------------------------------------------------------------------------------------------------------------------------------
-- PERSIST RELATIONS
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_PersistRelations]
	@RelationshipTypeID	VARCHAR(64)	= 'related',
	@Source_TopicID		INT	= -1,
	@Target_TopicIDs	VARCHAR(max)	= ''
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@pos		INT,
	@nextpos		INT,
	@valuelen		INT

SELECT	@pos		= 0,
	@nextpos		= 1

--------------------------------------------------------------------------------------------------------------------------------
-- ADD RELATED TOPIC IDS THAT ARE NOT ALREADY ADDED
--------------------------------------------------------------------------------------------------------------------------------

-- Parse out and loop through @Target_TopicIDs

DECLARE	@TgtId		NCHAR(5)
DECLARE	@RowNum		INT
DECLARE	SourceIdList
  CURSOR FOR
    SELECT	number
    FROM	topics_SimpleIntListToTbl(@Target_TopicIDs);

OPEN	SourceIdList
  FETCH	NEXT
  FROM	SourceIdList
  INTO	@TgtId
  SET	@RowNum		= 0

WHILE	@@FETCH_STATUS		= 0
  BEGIN
    SET	@RowNum = @RowNum + 1
    PRINT	CAST(@RowNum as char(1)) + ' ' + @TgtId
    PRINT	CAST(@RowNum as char(1)) + ' ' + @RelationshipTypeID
    PRINT	CAST(@RowNum as char(1)) + ' ' + cast(@Source_TopicID as char(6))
	  -- Perform insert if not exists

    IF ((
      SELECT	count(*)
      FROM	[dbo].[topics_Relationships]
      WHERE (	RelationshipTypeID	= @RelationshipTypeID
        AND	Source_TopicID		= @Source_TopicID
        AND	Target_TopicID		= CONVERT(Int, @TgtId)
      )
    )	<= 0)
      BEGIN
        -- Not already related, perform insert
        INSERT
        INTO	[dbo].[topics_Relationships] (RelationshipTypeID, Source_TopicID, Target_TopicID)
        VALUES (
	@RelationshipTypeID,
	@Source_TopicID,
	CONVERT(Int, @TgtId)
        )
      END

    -- Get next row
    FETCH	NEXT
    FROM	SourceIdList
    INTO	@TgtId
  END

CLOSE	SourceIdList
DEALLOCATE	SourceIdList
--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@Source_TopicID;