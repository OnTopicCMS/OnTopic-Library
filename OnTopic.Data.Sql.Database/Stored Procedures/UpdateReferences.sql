--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
-- Saves the 1:1 mappings for referenced topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateReferences]
	@TopicID		INT,
	@ReferencedTopics	TopicReferences	READONLY,
	@DeleteUnmatched	BIT	= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NOVEL VALUES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	TopicReferences (
	  Source_TopicID,
	  ReferenceKey,
	  Target_TopicID
	)
SELECT	@TopicID,
	Target.ReferenceKey,
	Target.TopicID
FROM	@ReferencedTopics	Target
LEFT JOIN	TopicReferences		Existing
  ON	Source_TopicID		= @TopicID
  AND	Existing.ReferenceKey	= Target.ReferenceKey
WHERE	ISNULL(Source_TopicID, '')	= ''
  AND	Target.TopicID		> 0

--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE EXISTING VALUES
--------------------------------------------------------------------------------------------------------------------------------
UPDATE	Existing
SET	Target_TopicID		= TopicID
FROM	@ReferencedTopics	Target
LEFT JOIN	TopicReferences		Existing
  ON	Source_TopicID		= @TopicID
  AND	Existing.ReferenceKey	= Target.ReferenceKey
WHERE	Source_TopicID		IS NOT NULL
  AND	Target.TopicID		!= Target_TopicID
  AND	Target.TopicID		> 0

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE UNMATCHED VALUES
--------------------------------------------------------------------------------------------------------------------------------
IF @DeleteUnmatched = 1
  BEGIN
    DELETE	Existing
    FROM	@ReferencedTopics	New
    RIGHT JOIN	TopicReferences		Existing
      ON	Source_TopicID		= @TopicID
      AND	Existing.ReferenceKey	= New.ReferenceKey
    WHERE	Source_TopicID		= @TopicID
      AND	ISNULL(TopicID, '')	= ''
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;