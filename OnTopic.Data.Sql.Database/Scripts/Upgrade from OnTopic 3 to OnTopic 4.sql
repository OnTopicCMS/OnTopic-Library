--------------------------------------------------------------------------------------------------------------------------------
-- UPGRADE FROM ONTOPIC 3.x TO ONTOPIC 4.x
--------------------------------------------------------------------------------------------------------------------------------
-- There are a few data schema differences that cannot be handled as part of the schema comparison. These should be executed
-- prior to running migrations.
--------------------------------------------------------------------------------------------------------------------------------

--------------------------------------------------------------------------------------------------------------------------------
-- DROP COLUMNS
--------------------------------------------------------------------------------------------------------------------------------
-- Migrations won't drop columns that have data in them. The following drop columns that are no longer needed. This also drops
-- stored procedures that reference those columns—with the knowledge that their replacements will be recreated by the
-- migrations.
--------------------------------------------------------------------------------------------------------------------------------

ALTER
TABLE	topics_TopicAttributes
DROP
COLUMN	AttributeID

--------------------------------------------------------------------------------------------------------------------------------
-- INHERIT TYPES
--------------------------------------------------------------------------------------------------------------------------------
-- Attribute Descriptors will be converted to more specific content types based on their legacy attribute type. Attribute types
-- may be inherited from base topics, however. This script identifies any cases where the attribute type is inherited, and
-- updates the target topic with the inherited value. This may need to be run multiple times if there are multiple layers of
-- inheritance (e.g., an attribute derives from an attribute which derives from another attribute).
--------------------------------------------------------------------------------------------------------------------------------

INSERT
INTO	topics_TopicAttributes
SELECT	SourceTopicID,
	'Type',
	AttributeTypes.AttributeValue,
	SYSUTCDATETIME()
FROM (
  SELECT	TopicID		AS SourceTopicID,
	AttributeKey,
	AttributeValue
  FROM	Topics_TopicAttributes
)	Attributes
PIVOT (	MAX(AttributeValue)
  FOR	AttributeKey IN (
	  [Type],
	  [ContentType],
	  [TopicID]
	)
)	AS Attributes
JOIN	Topics_TopicAttributes	AttributeTypes
  ON	Attributes.TopicID = CAST(AttributeTypes.TopicID AS VARCHAR(10))
  AND	AttributeTypes.AttributeKey	= 'Type'
WHERE	ContentType		= 'AttributeDescriptor'
AND	Type		IS NULL
AND	ISNULL(Attributes.TopicID, -1)	> 0
ORDER BY	SourceTopicID,
	ContentType

--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE CONTENT TYPES
--------------------------------------------------------------------------------------------------------------------------------
-- Based on the attribute types, update the content type to the corresponding attribute descriptor. In some cases, the names
-- have changed, and so an explicit mapping is required.
--------------------------------------------------------------------------------------------------------------------------------

UPDATE	ContentTypes
SET	ContentTypes.AttributeValue	=
	CASE
	  WHEN AttributeTypes.AttributeValue LIKE 'DateTime%'	THEN 'DateTimeAttribute'
	  WHEN AttributeTypes.AttributeValue = 'File.ascx'	THEN 'FileListAttribute'
	  WHEN AttributeTypes.AttributeValue = 'FormField.ascx'	THEN 'TextAttribute'
	  WHEN AttributeTypes.AttributeValue = 'Relationships.ascx'	THEN 'RelationshipAttribute'
	  WHEN AttributeTypes.AttributeValue = 'TopicList.ascx'	THEN 'NestedTopicListAttribute'
	  WHEN AttributeTypes.AttributeValue = 'TopicLookup.ascx'	THEN 'TopicListAttribute'
	  WHEN AttributeTypes.AttributeValue = 'TopicPointer.ascx'	THEN 'TopicReferenceAttribute'
	  WHEN AttributeTypes.AttributeValue = 'WYSIWYG.ascx'	THEN 'HtmlAttribute'
	  ELSE REPLACE(AttributeTypes.AttributeValue, '.ascx', '') + 'Attribute'
	END
FROM	topics_TopicAttributes	ContentTypes
INNER JOIN	topics_TopicAttributes	AttributeTypes
  ON	AttributeTypes.TopicID	= ContentTypes.TopicID
  AND	AttributeTypes.AttributeKey	= 'Type'
WHERE	ContentTypes.AttributeKey	= 'ContentType'