--------------------------------------------------------------------------------------------------------------------------------
-- FIND TOPIC IDS
--------------------------------------------------------------------------------------------------------------------------------
-- Given an attribute key and attribute value, returns the TopicID of each child topic that has an attribute with that value.
--------------------------------------------------------------------------------------------------------------------------------

CREATE
FUNCTION [dbo].[FindTopicIDs]
(
	@TopicID		INT,
	@AttributeKey		VARCHAR(255),
	@AttributeValue		NVARCHAR(255),
	@IsExtendedAttribute	BIT		= NULL,
	@UsePartialMatch	BIT		= 0
)
RETURNS	@Topics		TABLE
(
	TopicID		INT
)
AS

BEGIN

  ------------------------------------------------------------------------------------------------------------------------------
  -- DEFINE VARIABLES
  ------------------------------------------------------------------------------------------------------------------------------
  DECLARE	@RangeLeft		INT		= 0
  DECLARE	@RangeRight		INT		= 0

  SELECT	@RangeLeft		= RangeLeft,
	@RangeRight		= RangeRight
  FROM	Topics
  WHERE	TopicID		= @TopicID

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETRIEVE KEY ATTRIBUTES
  ------------------------------------------------------------------------------------------------------------------------------
  IF (@AttributeKey IN ('Key', 'ContentType', 'ParentID'))
    BEGIN
      INSERT
      INTO	@Topics
      SELECT	TopicID
      FROM	Topics
      WHERE (	@AttributeKey		= 'Key'
        AND	TopicKey		= @AttributeValue
        OR	@AttributeKey		= 'ContentType'
        AND	ContentType		= @AttributeValue
        OR	@AttributeKey		= 'ParentID'
        AND	ParentID		= @AttributeValue
      )
      RETURN
    END

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETRIEVE INDEXED ATTRIBUTES
  ------------------------------------------------------------------------------------------------------------------------------
  IF (ISNULL(@IsExtendedAttribute, 0) = 0)
    INSERT
    INTO	@Topics
    SELECT	Attributes.TopicID
    FROM	AttributeIndex		Attributes
    JOIN	Topics
      ON	Topics.TopicID		= Attributes.TopicID
    WHERE	AttributeKey		= @AttributeKey
      AND	RangeLeft		>= @RangeLeft
      AND	RangeRight		<= @RangeRight
      AND (	@UsePartialMatch	= 1
        AND	AttributeValue		LIKE '%' + @AttributeValue + '%'
        OR	AttributeValue		= @AttributeValue
      )

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETRIEVE EXTENDED ATTRIBUTES
  ------------------------------------------------------------------------------------------------------------------------------
  IF (ISNULL(@IsExtendedAttribute, 1) = 1)
    INSERT
    INTO	@Topics
    SELECT	Attributes.TopicID
    FROM	ExtendedAttributeIndex	Attributes
    JOIN	Topics
      ON	Topics.TopicID		= Attributes.TopicID
    WHERE	RangeLeft		>= @RangeLeft
      AND	RangeRight		<= @RangeRight
      AND (	@UsePartialMatch	= 1
      AND	AttributesXml
	  .exist(
	    '/attributes/attribute[
	      @key=sql:variable("@AttributeKey") and
	      text()[contains(.,sql:variable("@AttributeValue"))]
	    ]') = 1
        OR	@UsePartialMatch	= 0
        AND	AttributesXml
	  .exist(
	    '/attributes/attribute[
	      @key=sql:variable("@AttributeKey") and
	      text() = sql:variable("@AttributeValue")
	    ]') = 1
      )

  RETURN

END