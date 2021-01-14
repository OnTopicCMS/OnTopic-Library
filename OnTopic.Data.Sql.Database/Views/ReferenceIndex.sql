--------------------------------------------------------------------------------------------------------------------------------
-- REFERENCES (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the TopicReferences table by the latest version for each topic and reference key. For most use cases, this should be
-- the primary sources for retrieving topic references, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[ReferenceIndex]
WITH	SCHEMABINDING
AS

WITH TopicReferences AS (
  SELECT	Source_TopicID,
	ReferenceKey,
	Target_TopicID,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID,
			ReferenceKey
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[TopicReferences]
)
SELECT	TopicReferences.Source_TopicID,
	TopicReferences.ReferenceKey,
	TopicReferences.Target_TopicID,
	TopicReferences.Version
FROM	TopicReferences
WHERE	RowNumber		= 1