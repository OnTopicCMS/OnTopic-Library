--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
-- Saves the n:n mappings for related topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateRelationships]
	@TopicID		INT	= -1,
	@RelationshipKey	VARCHAR(255)	= 'related',
	@RelatedTopics		TopicList	READONLY,
	@DeleteUnmatched	BIT	= 1
AS

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NOVEL VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	Relationships (
	  Source_TopicID,
	  RelationshipKey,
	  Target_TopicID
	)
SELECT	@TopicID,
	@RelationshipKey,
	TopicID
FROM	@RelatedTopics		Target
LEFT JOIN	Relationships		Existing
  ON	Target_TopicID		= TopicID
  AND	Source_TopicID		= @TopicID
WHERE	Target_TopicID		IS NULL

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE UNMATCHED VALUES
--------------------------------------------------------------------------------------------------------------------------------
IF @DeleteUnmatched = 1
  BEGIN
    DELETE	EXISTING
    FROM	@RelatedTopics		Relationships
    RIGHT JOIN	Relationships		Existing
      ON	Target_TopicID		= TopicID
    WHERE	Source_TopicID		= @TopicID
      AND	ISNULL(TopicID, '')	= ''
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;