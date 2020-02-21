--------------------------------------------------------------------------------------------------------------------------------
-- EXTENDED ATTRIBUTES (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Filters the Extended Attributes table by the latest version for each topic. For most use cases, this should be the primary
-- source for retrieving extended attributes, since it excludes historical versions.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
VIEW	[dbo].[ExtendedAttributeIndex]
WITH	SCHEMABINDING
AS

WITH	TopicExtendedAttributes AS (
  SELECT	TopicID,
	AttributesXml,
	Version,
	RowNumber = ROW_NUMBER() OVER (
	  PARTITION BY		TopicID
	  ORDER BY		Version		DESC
	)
  FROM	[dbo].[ExtendedAttributes]
)
SELECT	TopicID,
	AttributesXml,
	Version
FROM	TopicExtendedAttributes
WHERE	RowNumber		= 1