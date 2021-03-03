--------------------------------------------------------------------------------------------------------------------------------
-- RELATIONSHIPS (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the Relationships table by the latest version for each topic and relationship key. For most use cases, this should be
-- the primary sources for retrieving topic relationships, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[RelationshipIndex]
WITH	SCHEMABINDING
AS

WITH Relationships AS (
  SELECT	Source_TopicID,
	RelationshipKey,
	Target_TopicID,
	IsDeleted,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		Source_TopicID,
			RelationshipKey,
			Target_TopicID
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[Relationships]
)
SELECT	Relationships.Source_TopicID,
	Relationships.RelationshipKey,
	Relationships.Target_TopicID,
	Relationships.IsDeleted,
	Relationships.Version
FROM	Relationships
WHERE	RowNumber		= 1