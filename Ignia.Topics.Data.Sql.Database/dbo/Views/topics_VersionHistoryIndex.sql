CREATE
VIEW	[dbo].[topics_VersionHistoryIndex]
WITH	SCHEMABINDING
AS

WITH	TopicVersions
AS (
  SELECT	DISTINCT
	Attributes.TopicID,
	Attributes.Version,
	RowNumber		= ROW_NUMBER() OVER (
	  PARTITION BY		Attributes.TopicID
	  ORDER BY		Version DESC
	)
  FROM	[dbo].[topics_TopicAttributes]	Attributes
  GROUP BY	Attributes.TopicID,
	Attributes.Version
)
SELECT	Versions.TopicId,
	Version
FROM	TopicVersions		Versions
WHERE	RowNumber		<= 5
