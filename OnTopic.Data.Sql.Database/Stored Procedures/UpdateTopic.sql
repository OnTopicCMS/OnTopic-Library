--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Used to update the attributes of a provided node
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateTopic]
	@TopicID		INT				,
	@Key		VARCHAR(128)		= NULL		,
	@ContentType		VARCHAR(128)		= NULL		,
	@Attributes		AttributeValues		READONLY		,
	@ExtendedAttributes	XML		= NULL		,
	@References		TopicReferences		READONLY		,
	@Version		DATETIME		= NULL,
	@DeleteUnmatched	BIT		= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
SET	@Version		= GetDate()

--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE KEY ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------

IF @Key IS NOT NULL OR @ContentType IS NOT NULL
  BEGIN
    UPDATE	Topics
    SET	TopicKey		=
      CASE
        WHEN	@Key		IS NULL
        THEN	TopicKey
        ELSE	@Key
      END,
	ContentType		=
      CASE
        WHEN	@ContentType		IS NULL
        THEN	TopicKey
        ELSE	@ContentType
      END
    WHERE	TopicID		= @TopicID
  END

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
WHERE	ISNULL(AttributeValue, '')	!= ''
  AND 	ISNULL(ExistingValue, '')	!= ISNULL(AttributeValue, '')

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
WHERE	ISNULL(AttributeValue, '')	= ''
  AND	ExistingValue		!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@ReferenceCount		INT
SELECT	@ReferenceCount		= COUNT(ReferenceKey)
FROM	@References

IF @ReferenceCount > 0
  BEGIN
    EXEC	UpdateReferences	@TopicID,
			@References,
			@DeleteUnmatched
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID