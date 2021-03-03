--------------------------------------------------------------------------------------------------------------------------------
-- UPDATE EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Saves ExtendedAttributes values if XML is included.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[UpdateExtendedAttributes]
	@TopicID		INT,
	@ExtendedAttributes	XML		= NULL		,
	@Version		DATETIME2(7)		= NULL		,
	@DeleteUnmatched	BIT		= 0
AS

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
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID;