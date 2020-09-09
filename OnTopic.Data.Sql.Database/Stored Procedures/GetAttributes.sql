--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Returns the most recent value of each attribute associated with a particular topic.
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[GetAttributes]
	@TopicID		INT	= -1
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT MOST RECENT ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	AttributeKey,
	AttributeValue,
	0		AS IsExtendedAttribute,
	Version
FROM	AttributeIndex
WHERE	TopicID		= @TopicID

UNION

--------------------------------------------------------------------------------------------------------------------------------
-- PARSE MOST RECENT EXTENDED ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	Attributes.Loc.value(
	  '@key',
	  'VARCHAR(255)'
	) AS		AttributeKey,
	Attributes.Loc.value(
	  '.[1]',
	  'VARCHAR(MAX)'
	) AS		AttributeValue,
	1		AS IsExtendedAttribute,
	Version
FROM	ExtendedAttributeIndex
CROSS APPLY	AttributesXml.nodes(
	  '/attributes/attribute'
	) AS		Attributes(Loc)
WHERE	TopicID		= @TopicID
ORDER BY	AttributeKey