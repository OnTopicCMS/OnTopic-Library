--------------------------------------------------------------------------------------------------------------------------------
-- ATTRIBUTES (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the attributes table by the latest version for each topic and attribute key. For most use cases, this should be the
-- primary sources for retrieving attributes, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[AttributeIndex]
WITH	SCHEMABINDING
AS

WITH Attributes AS (
  SELECT	TopicID,
	AttributeKey,
	AttributeValue,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		TopicID,
			AttributeKey
	  ORDER BY		Version	DESC
	)
  FROM	[dbo].[Attributes]
  WHERE	AttributeKey
  NOT IN (	'Key',
	'ParentID',
	'ContentType'
  )
)
SELECT	Attributes.TopicID,
	Attributes.AttributeKey,
	Attributes.AttributeValue,
	Attributes.Version
FROM	Attributes
WHERE	RowNumber		= 1