--------------------------------------------------------------------------------------------------------------------------------
-- GET ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
-- Returns the most recent value of each attribute associated with a particular topic.
--------------------------------------------------------------------------------------------------------------------------------

CREATE
FUNCTION	[dbo].[GetAttributes] (
	@TopicID		INT	= -1
)
RETURNS	@Attributes		TABLE
(
	AttributeKey		NVARCHAR(255)	NOT NULL,
	AttributeValue		NVARCHAR(MAX)	NOT NULL,
	IsExtendedAttribute	BIT,
	Version		DATETIME
)
AS

BEGIN

  ------------------------------------------------------------------------------------------------------------------------------
  -- SETUP INSERT
  ------------------------------------------------------------------------------------------------------------------------------
  INSERT
  INTO	@Attributes

  ------------------------------------------------------------------------------------------------------------------------------
  -- SELECT MOST RECENT ATTRIBUTES
  ------------------------------------------------------------------------------------------------------------------------------
  SELECT	AttributeKey,
	AttributeValue,
	0		AS IsExtendedAttribute,
	Version
  FROM	AttributeIndex
  WHERE	TopicID		= @TopicID

  UNION

  ------------------------------------------------------------------------------------------------------------------------------
  -- PARSE MOST RECENT EXTENDED ATTRIBUTES
  ------------------------------------------------------------------------------------------------------------------------------
  SELECT	Attributes.Loc.value(
	  '@key',
	  'VARCHAR(128)'
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

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETURN
  ------------------------------------------------------------------------------------------------------------------------------
  RETURN

END