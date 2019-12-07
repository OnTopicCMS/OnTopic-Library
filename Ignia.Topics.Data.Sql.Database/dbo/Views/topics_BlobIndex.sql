CREATE
VIEW	[dbo].[topics_BlobIndex]
WITH	SCHEMABINDING
AS

WITH	TopicBlob AS (
  SELECT	Blob.TopicID,
	Blob.Blob,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		Blob.TopicID
	  ORDER BY		Blob.Version DESC
	)
  FROM	[dbo].[topics_Blob]	AS Blob
)
SELECT	TopicBlob.TopicID,
	TopicBlob.Blob
FROM	TopicBlob
WHERE	RowNumber		= 1