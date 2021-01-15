--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
-- Saves the 1:1 mappings for referenced topics.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateReferences]
	@TopicID		INT,
	@ReferencedTopics	TopicReferences		READONLY		,
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
INTO	TopicReferences (
	  Source_TopicID,
	  ReferenceKey,
	  Target_TopicID,
	  Version
	)
SELECT	@TopicID,
	New.ReferenceKey,
	New.TopicID,
	@Version
FROM	@ReferencedTopics	New
OUTER APPLY (
  SELECT	TOP 1
	Target_TopicID		AS ExistingValue
  FROM	TopicReferences
  WHERE	Source_TopicID		= @TopicID
    AND	ReferenceKey		= New.ReferenceKey
  ORDER BY	Version		DESC
)			Existing
WHERE	ISNULL(ExistingValue, '')	!= New.TopicID
  AND	New.TopicID		> 0

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE UNMATCHED VALUES
--------------------------------------------------------------------------------------------------------------------------------
IF @DeleteUnmatched = 1
  BEGIN
    INSERT
    INTO	TopicReferences
    SELECT	@TopicID,
	Existing.ReferenceKey,
	NULL,
	@Version
    FROM	@ReferencedTopics	New
    RIGHT JOIN	ReferenceIndex		Existing
      ON	Source_TopicID		= @TopicID
      AND	Existing.ReferenceKey	= New.ReferenceKey
    WHERE	Source_TopicID		= @TopicID
      AND	ISNULL(TopicID, '')	= ''
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;