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

PRINT('Dropping legacy columns...');

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

ALTER
TABLE	ExtendedAttributes
DROP
CONSTRAINT	[DF_ExtendedAttributes_DateModified]

ALTER
TABLE	ExtendedAttributes
DROP
COLUMN	DateModified;

PRINT('Dropped legacy columns');

--------------------------------------------------------------------------------------------------------------------------------
-- MIGRATE CORE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- In OnTopic 5, core attributes that don't utilize versioning have been moved from the Attributes table to the Topics table.
-- This includes Key, ContentType, and ParentID. Previously, these required a lot of workaround since they frequently utilized
-- in a way that's inconsistent with other attributes. By moving them to Topic, we better acknowledge their unique status.
--------------------------------------------------------------------------------------------------------------------------------

PRINT('Migrating core attributes...');

ALTER TABLE	[dbo].[Topics]
ADD	[TopicKey]		VARCHAR(128)	NULL,
	[ContentType]		VARCHAR(128)	NULL,
	[ParentID]		INT	NULL;

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

ALTER TABLE	[dbo].[Topics]
ALTER COLUMN	TopicKey		VARCHAR(128)	NOT NULL;

ALTER TABLE	[dbo].[Topics]
ALTER COLUMN	ContentType		VARCHAR(128)	NOT NULL;

DELETE
FROM	Attributes
WHERE	AttributeKey
IN (	'Key',
	'ContentType',
	'ParentID'
)

PRINT('Migrated core attributes');

--------------------------------------------------------------------------------------------------------------------------------
-- MIGRATE TOPIC REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
-- In OnTopic 5, references to other topics—such as `BaseTopic`—have been moved from the Attributes table to a new
-- TopicReferences table, where they act more like relationships. This allows referential integrity to be enforced through
-- foreign key constraints, and formalizes the relationship so we don't need to rely on hacks in e.g. the Topic Data Transer
-- service to infer which attributes represent relationships in order to translate their values from `TopicID` to `UniqueKey`.
--------------------------------------------------------------------------------------------------------------------------------

PRINT('Migrating topic references...');

CREATE
TABLE	[dbo].[TopicReferences] (
	  [Source_TopicID]	INT	NOT NULL,
	  [ReferenceKey]	VARCHAR(128)	NOT NULL,
	  [Target_TopicID]	INT	NOT NULL,
	  [Version]		DATETIME2(7)	NULL
);

INSERT
INTO	TopicReferences
SELECT	Attributes.TopicID,
	SUBSTRING(AttributeKey, 0, LEN(AttributeKey)-1),
	AttributeValue,
	Version
FROM	Attributes
JOIN	Topics
  ON	Topics.TopicID		= CONVERT(INT, AttributeValue)
WHERE	AttributeKey		LIKE '%ID'
  AND	ISNUMERIC(AttributeValue)	= 1
  AND	Topics.TopicID		IS NOT NULL

PRINT('Migrated core attributes');

--------------------------------------------------------------------------------------------------------------------------------
-- MIGRATE DERIVED TOPICS
--------------------------------------------------------------------------------------------------------------------------------
-- The above migration to topic references includes the DerivedTopic. To better clarify the purpose and intent of that
-- relationship, we're renaming the attribute from 'DerivedTopic' to 'BaseTopic', and the actual storage field from 'Topic(ID)'
-- to 'BaseTopic'. This is not only a more accurate identifier, but also unifies the label between the attribute descriptor
-- and how its 'ReferenceKey'.
--------------------------------------------------------------------------------------------------------------------------------

PRINT('Migrating base topics...');

UPDATE	TopicReferences
SET	ReferenceKey		= 'BaseTopic'
WHERE	ReferenceKey		= 'Topic'

UPDATE	Topics
SET	TopicKey		= 'BaseTopic'
WHERE	TopicKey		IN ('TopicID', 'InheritedTopic', 'DerivedTopic')
AND	ContentType		= 'TopicReferenceAttribute'

PRINT('Migrated base topics');

--------------------------------------------------------------------------------------------------------------------------------
-- MIGRATE ATTRIBUTE KEYS
--------------------------------------------------------------------------------------------------------------------------------
-- In OnTopic 5, attribute content types have been renamed to have the suffix "AttributeDescriptor" instead of just "Attribute".
-- This has a number of benefits, including consistency with the base "AttributeDescriptor" content type, and avoiding a naming
-- conflict with .NET's own "*Attribute" convention (which is usually reserved for actual attributes).
--------------------------------------------------------------------------------------------------------------------------------

PRINT('Migrating attribute descriptors...');

UPDATE	Topics
SET	TopicKey		= TopicKey + 'Descriptor'
WHERE	TopicKey		LIKE '%Attribute'
AND	ContentType		= 'ContentTypeDescriptor'

UPDATE	Topics
SET	ContentType		= ContentType + 'Descriptor'
WHERE	ContentType		LIKE '%Attribute'

PRINT('Migrated attribute descriptors');