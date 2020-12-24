--------------------------------------------------------------------------------------------------------------------------------
-- UPGRADE FROM ONTOPIC 4.x TO ONTOPIC 5.x
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
TABLE	Topics
DROP
COLUMN	Stack_Top;

ALTER
TABLE	Attributes
DROP
CONSTRAINT	DF_Attributes_DateModified;

ALTER
TABLE	Attributes
DROP
COLUMN	DateModified;

--------------------------------------------------------------------------------------------------------------------------------
-- MIGRATE CORE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- In OnTopic 5, core attributes that don't utilize versioning have been moved from the Attributes table to the Topics table.
-- This includes Key, ContentType, and ParentID. Previously, these required a lot of workaround since they frequently utilized
-- in a way that's inconsistent with other attributes. By moving them to Topic, we better acknowledge their unique status.
--------------------------------------------------------------------------------------------------------------------------------

ALTER TABLE [dbo].[Topics]
    ADD [TopicKey]    VARCHAR (128) NULL,
        [ContentType] VARCHAR (128) NULL,
        [ParentID]    INT           NULL;

WITH KeyAttributes AS (
  SELECT	TopicID,
                AttributeKey,
                AttributeValue,
                RowNumber = ROW_NUMBER() OVER (
                  PARTITION BY		TopicID,
			AttributeKey
                  ORDER BY		Version DESC
                )
  FROM	[dbo].[Attributes]
  WHERE	AttributeKey
  IN (	'Key',
	'ContentType',
	'ParentID'
  )
)
UPDATE	Topics
SET	Topics.TopicKey		= Pvt.[Key],
	Topics.ContentType	= Pvt.ContentType,
	Topics.ParentID		= Pvt.ParentID
FROM	KeyAttributes
PIVOT (	MIN(AttributeValue)
  FOR	AttributeKey IN (
	  [Key],
	  [ContentType],
	  [ParentID]
	)
)	AS Pvt
WHERE	RowNumber		= 1
AND	Topics.TopicID		= Pvt.TopicID

DELETE
FROM	Attributes
WHERE	AttributeKey
IN (	'Key',
	'ContentType',
	'ParentID'
)