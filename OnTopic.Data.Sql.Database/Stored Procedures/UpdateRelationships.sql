--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
-- Saves the n:n mappings for related topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateRelationships]
	@TopicID		INT	= -1,
	@RelationshipKey	VARCHAR(255)	= 'related',
	@RelatedTopics		TopicList	READONLY
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES

DECLARE	@Existing_TopicIDs	TopicList

--------------------------------------------------------------------------------------------------------------------------------
-- IDENTIFY EXISTING VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	@Existing_TopicIDs (
	  TopicId
	)
SELECT	Target_TopicId
FROM	Relationships
WHERE	Source_TopicId		= @TopicID
  AND	RelationshipKey		= @RelationshipKey

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NOVEL VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	Relationships (
	  Source_TopicId,
	  RelationshipKey,
	  Target_TopicId
	)
SELECT	@TopicId,
	@RelationshipKey,
	Target.TopicId
FROM	@RelatedTopics		Target
FULL JOIN	@Existing_TopicIDs	Existing
  ON	Existing.TopicId	= Target.TopicId
WHERE	Existing.TopicId	is null

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;