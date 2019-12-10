--------------------------------------------------------------------------------------------------------------------------------
-- PERSIST RELATIONS
--------------------------------------------------------------------------------------------------------------------------------
-- Removes and saves the n:n mappings for scoped related topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_PersistRelations]
	@RelationshipTypeID	VARCHAR(255)	= 'related',
	@Source_TopicID		INT	= -1,
	@Target_TopicIDs	TopicList	READONLY
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@Existing_TopicIDs	TopicList

--------------------------------------------------------------------------------------------------------------------------------
-- IDENTIFY EXISTING VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	@Existing_TopicIDs (
	TopicID
)
SELECT	Target_TopicID
FROM	topics_Relationships
WHERE	Source_TopicID		= @Source_TopicID
  AND	RelationshipTypeId	= @RelationshipTypeId

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NOVEL VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	topics_Relationships (
	RelationshipTypeId,
	Source_TopicId,
	Target_TopicId
)
SELECT	@RelationshipTypeId,
	@Source_TopicId,
	Target.TopicId
FROM	@Target_TopicIDs	Target
FULL JOIN	@Existing_TopicIDs	Existing
  ON	Existing.TopicID	= Target.TopicID
WHERE	Existing.TopicID	is null

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@Source_TopicID;