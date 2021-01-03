--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Used to update the attributes of a provided topic, including core, indexed, and extended attributes.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateTopic]
	@TopicID		INT				,
	@Key		VARCHAR(128)		= NULL		,
	@ContentType		VARCHAR(128)		= NULL		,
	@Attributes		AttributeValues		READONLY		,
	@ExtendedAttributes	XML		= NULL		,
	@Version		DATETIME		= NULL		,
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
-- UPDATE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
IF EXISTS (SELECT TOP 1 NULL FROM @Attributes)
  BEGIN
    EXEC	UpdateAttributes	@TopicID,
			@Attributes,
			@Version,
			@DeleteUnmatched
  END

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
IF @ExtendedAttributes IS NOT NULL AND CAST(@ExtendedAttributes AS NVARCHAR(MAX)) != CAST(@PreviousExtendedAttributes AS NVARCHAR(MAX))
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
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID