﻿--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Used to update the attributes of a provided node
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateTopic]
	@TopicID		INT		= -1		,
	@Attributes		AttributeValues		READONLY		,
	@ExtendedAttributes	XML		= null		,
	@Version		DATETIME		= null		,
	@DeleteRelationships	BIT		= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
SET	@Version		= GetDate()

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NEW ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
INSERT
INTO	Attributes (
	  TopicID		,
	  AttributeKey		,
	  AttributeValue	,
	  Version
	)
SELECT	@TopicID,
	AttributeKey,
	AttributeValue,
	@Version
FROM	@Attributes
WHERE	AttributeKey		!= 'ParentId'
  AND	IsNull(AttributeValue, '')	!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- PULL PREVIOUS EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@PreviousExtendedAttributes	XML

SELECT	@PreviousExtendedAttributes	= AttributesXml
FROM	ExtendedAttributeIndex
WHERE	TopicID		= @TopicID

--------------------------------------------------------------------------------------------------------------------------------
-- ADD EXTENDED ATTRIBUTES, IF CHANGED
--------------------------------------------------------------------------------------------------------------------------------
IF CAST(@ExtendedAttributes AS NVARCHAR(MAX)) != CAST(@PreviousExtendedAttributes AS NVARCHAR(MAX))
  BEGIN
    INSERT
    INTO	ExtendedAttributes (
	  TopicID		,
	  AttributesXml		,
	  Version
	)
    VALUES (
	@TopicID		,
	@ExtendedAttributes	,
	@Version
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- INSERT NULL ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	Attributes (
	  TopicID		,
	  AttributeKey		,
	  AttributeValue	,
	  Version
	)
SELECT	@TopicID,
	AttributeKey,
	'',
	@Version
FROM	@Attributes		NullAttributes
WHERE	IsNull(AttributeValue, '')	= ''
  AND (
    SELECT	TOP 1
	AttributeValue
    FROM	Attributes
    WHERE	TopicID		= @TopicID
      AND	AttributeKey		= NullAttributes.AttributeKey
    ORDER BY	Version DESC
  )			!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- REMOVE EXISTING RELATIONS
--------------------------------------------------------------------------------------------------------------------------------
-- Relationships will be re-added by the Data Access Layer using Topics_PersistRelationships.
--------------------------------------------------------------------------------------------------------------------------------
DELETE
FROM	[dbo].[Relationships]
WHERE	Source_TopicID		= @TopicID
  AND	@DeleteRelationships	= 1

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID