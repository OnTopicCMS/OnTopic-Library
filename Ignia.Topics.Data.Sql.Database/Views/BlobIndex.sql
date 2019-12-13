--------------------------------------------------------------------------------------------------------------------------------
-- BLOB (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the Blob table by the latest version for each topic. For most use cases, this should be the primary sources for
-- retrieving a blob, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[BlobIndex]
WITH	SCHEMABINDING
AS

WITH	TopicBlob AS (
  SELECT	TopicID,
	Blob,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version DESC
	)
  FROM	[dbo].[Blobs]
)
SELECT	TopicBlob.TopicID,
	TopicBlob.Blob
FROM	TopicBlob
WHERE	RowNumber		= 1