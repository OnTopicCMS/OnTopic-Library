--------------------------------------------------------------------------------------------------------------------------------
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
FROM	@Attributes		New
OUTER APPLY (
    SELECT	TOP 1
	AttributeValue		AS ExistingValue
    FROM	Attributes
    WHERE	TopicID		= @TopicID
      AND	AttributeKey		= New.AttributeKey
    ORDER BY	Version		DESC
  )			Existing
WHERE	AttributeKey		!= 'ParentId'
  AND	ISNULL(AttributeValue, '')	!= ''
  AND 	ISNULL(ExistingValue, '')	!= AttributeValue

--------------------------------------------------------------------------------------------------------------------------------
-- PULL PREVIOUS EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@PreviousExtendedAttributes	XML

SELECT	TOP 1
	@PreviousExtendedAttributes	= AttributesXml
FROM	ExtendedAttributes
WHERE	TopicID		= @TopicID
ORDER BY	Version		DESC

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
FROM	@Attributes		New
CROSS APPLY (
  SELECT	TOP 1
	AttributeValue		AS ExistingValue
  FROM	Attributes
  WHERE	TopicID		= @TopicID
    AND	AttributeKey		= New.AttributeKey
  ORDER BY	Version DESC
)			Existing
WHERE	IsNull(AttributeValue, '')	= ''
  AND	ExistingValue		!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID