CREATE
VIEW	[dbo].[topics_VersionHistoryIndex]
WITH	SCHEMABINDING
AS

WITH	TopicVersions
AS (
  SELECT	DISTINCT
	TopicID,
	Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version DESC
	)
  FROM	[dbo].[topics_TopicAttributes]
  GROUP BY	TopicID,
	Version
)
SELECT	Versions.TopicId,
	Version
FROM	TopicVersions		Versions
WHERE	RowNumber		<= 5
