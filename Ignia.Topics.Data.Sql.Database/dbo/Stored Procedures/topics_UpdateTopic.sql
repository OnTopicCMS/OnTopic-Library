﻿--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Used to update the attributes of a provided node
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_UpdateTopic]
	@TopicID		INT		= -1		,
	@Attributes		VARCHAR(1024)		= ''		,
	@NullAttributes		VARCHAR(1024)		= ''		,
	@ParentID		INT		= -1		,
	@Blob		XML		= null		,
	@Version		DATETIME		= null		,
	@IsDraft		BIT		= 0		,
	@DeleteRelationships	BIT		= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
AND	@IsDraft		= 0
SET	@Version		= getdate()

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE EXISTING ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- ###NOTE KLT08052014: This is no longer needed since versioning has been introduced; legacy attributes should be maintained.
/*
DELETE
FROM	topics_TopicAttributes
WHERE	TopicID = @TopicID

DELETE
FROM	topics_Blob
WHERE	TopicID = @TopicID
*/

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE ANY LEGACY KEYS
--------------------------------------------------------------------------------------------------------------------------------
-- ###NOTE KLT08052014: This is no longer needed since versioning has been introduced; legacy attributes should be maintained.
/*
DECLARE	@Key		VARCHAR(200)

SELECT	@Key		= AttributeValue
FROM	topics_TopicAttributes
WHERE	TopicID		= @TopicID
  AND	AttributeKey		= 'Key'

IF NOT ISNULL()
  BEGIN
    DELETE
    FROM	topics_TopicAttributes
    WHERE	TopicID		= @TopicID
      AND	AttributeKey		= 'Key'
  END
*/

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NEW ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	topics_TopicAttributes (
	  TopicID		,
	  AttributeKey		,
	  AttributeValue	,
	  Version
	)
SELECT	@TopicID		,
	Substring(s, 0, CharIndex('~~', s)),
	Substring(s, CharIndex('~~', s) + 2, Len(s)),
	@Version
FROM	Split(@Attributes, '``')
WHERE	s != ''

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE BLOB
--------------------------------------------------------------------------------------------------------------------------------
IF @Blob is not null
  BEGIN
    INSERT
    INTO	topics_Blob (
	  TopicID		,
	  Blob		,
	  Version
	)
    VALUES (
	@TopicID		,
	@Blob		,
	@Version
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NULL ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	topics_TopicAttributes (
	  TopicID		,
	  AttributeKey		,
	  AttributeValue	,
	  Version
	)
SELECT	@TopicID		,
	s		,
	''		,
	@Version
FROM	Split (@NullAttributes, ',')
WHERE (
  SELECT	TOP 1
	AttributeValue
  FROM	topics_TopicAttributes
  WHERE	TopicID		= @TopicID
    AND	AttributeKey		= s
  ORDER BY	Version DESC
) != ''

--------------------------------------------------------------------------------------------------------------------------------
-- REMOVE EXISTING RELATIONS
--------------------------------------------------------------------------------------------------------------------------------
-- Relationships will be re-added by the Data Access Layer using Topics_PersistRelationships.
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	[dbo].[topics_Relationships]
WHERE	Source_TopicID		= @TopicID
  AND	@DeleteRelationships	= 1

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID