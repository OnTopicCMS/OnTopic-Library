--------------------------------------------------------------------------------------------------------------------------------
-- UPGRADE FROM ONTOPIC 4.x TO ONTOPIC 5.x
--------------------------------------------------------------------------------------------------------------------------------
-- There are a few data schema differences that cannot be handled as part of the schema comparison. These should be executed
-- prior to running migrations.
--------------------------------------------------------------------------------------------------------------------------------

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
