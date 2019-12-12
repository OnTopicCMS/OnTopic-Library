CREATE
VIEW	[dbo].[topics_BlobIndex]
WITH	SCHEMABINDING
AS

WITH	TopicBlob AS (
  SELECT	TopicID,
	Blob,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version DESC
	)
  FROM	[dbo].[topics_Blob]
)
SELECT	TopicBlob.TopicID,
	TopicBlob.Blob
FROM	TopicBlob
WHERE	RowNumber		= 1