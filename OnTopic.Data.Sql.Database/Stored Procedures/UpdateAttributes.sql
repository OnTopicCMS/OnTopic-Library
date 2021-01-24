--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Saves a set of AttributeValues to the Attributes table, while optionally accounting for deleted or unmatched attributes.
-- Optionally update ExtendedAttributes values if XML is included.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateAttributes]
	@TopicID		INT,
	@Attributes		AttributeValues		READONLY		,
	@Version		DATETIME		= NULL		,
	@DeleteUnmatched	BIT		= 0
AS

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
	ISNULL(AttributeValue, ''),
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
WHERE	ISNULL(ExistingValue, '')	!= ISNULL(AttributeValue, '')

--------------------------------------------------------------------------------------------------------------------------------
-- DELETE UNMATCHED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
IF @DeleteUnmatched = 1
  BEGIN
    INSERT
    INTO	Attributes
    SELECT	@TopicID,
	Existing.AttributeKey,
	'',
	@Version
    FROM	AttributeIndex		Existing
    LEFT JOIN	@Attributes		New
      ON	Existing.TopicID	= @TopicID
      AND	Existing.AttributeKey	= New.AttributeKey
    WHERE	ISNULL(New.AttributeKey, '')	= ''
      AND	Existing.TopicID	= @TopicID
  END

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;