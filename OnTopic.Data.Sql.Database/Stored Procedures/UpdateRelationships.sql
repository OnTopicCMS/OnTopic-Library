--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE RELATIONSHIPS
--------------------------------------------------------------------------------------------------------------------------------
-- Saves the n:n mappings for related topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateRelationships]
	@TopicID		INT,
	@RelationshipKey	VARCHAR(255),
	@RelatedTopics		TopicList		READONLY		,
	@Version		DATETIME		= NULL		,
	@DeleteUnmatched	BIT		= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
SET	@Version		= GETUTCDATE()

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NOVEL VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	Relationships (
	  Source_TopicID,
	  RelationshipKey,
	  Target_TopicID,
	  IsDeleted,
	  Version
	)
SELECT	@TopicID,
	@RelationshipKey,
	TopicID,
	0,
	@Version
FROM	@RelatedTopics		New
OUTER APPLY (
  SELECT	TOP 1
	IsDeleted		AS ExistingValue
  FROM	Relationships
  WHERE	Source_TopicID		= @TopicID
    AND	RelationshipKey		= @RelationshipKey
    AND	Target_TopicID		= New.TopicID
  ORDER BY	Version		DESC
)			Existing
WHERE	ISNULL(ExistingValue, 1)	= 1

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE UNMATCHED VALUES
--------------------------------------------------------------------------------------------------------------------------------
IF @DeleteUnmatched = 1
  BEGIN
    INSERT
    INTO	Relationships (
	  Source_TopicID,
	  RelationshipKey,
	  Target_TopicID,
	  IsDeleted,
	  Version
    )
    SELECT	@TopicID,
	Existing.RelationshipKey,
	Existing.Target_TopicID,
	1,
	@Version
    FROM	@RelatedTopics		Relationships
    RIGHT JOIN	Relationships		Existing
      ON	Target_TopicID		= TopicID
    WHERE	Source_TopicID		= @TopicID
      AND	ISNULL(TopicID, '')	= ''
      AND	RelationshipKey		= @RelationshipKey
      AND	IsDeleted		= 0
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;