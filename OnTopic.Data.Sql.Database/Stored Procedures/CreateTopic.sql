--------------------------------------------------------------------------------------------------------------------------------
-- CREATE TOPIC
--------------------------------------------------------------------------------------------------------------------------------
-- Creates a new topic.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[CreateTopic]
	@Key		VARCHAR(128)		,
	@ContentType		VARCHAR(128)		,
	@ParentID		INT		= -1,
	@Attributes		AttributeValues		READONLY,
	@ExtendedAttributes 	XML		= NULL,
	@References		TopicReferences		READONLY,
	@Version		DATETIME		= NULL
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
SET	@Version		= GETUTCDATE()

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@RangeRight		INT	--Right Most Sibling
SET	@RangeRight		= 0

--------------------------------------------------------------------------------------------------------------------------------
-- RESERVE SPACE FOR NEW CHILD.
--------------------------------------------------------------------------------------------------------------------------------
IF (@ParentID > -1)
  BEGIN
    SELECT	@RangeRight		= RangeRight
    FROM	Topics
    WHERE	TopicID		= @ParentID

    UPDATE	Topics
      SET	RangeLeft		=
        CASE
          WHEN	RangeLeft		> @RangeRight
          THEN	RangeLeft		+ 2
          ELSE	RangeLeft
        END,
	RangeRight		=
        CASE
          WHEN	RangeRight		>= @RangeRight
          THEN	RangeRight		+ 2
          ELSE	RangeRight
        END
    WHERE	RangeRight		>= @RangeRight
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE NEW TOPIC
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	Topics (
	RangeLeft,
	RangeRight,
	TopicKey,
	ContentType,
	ParentID
)
Values (
	@RangeRight,
	@RangeRight		+ 1,
	@Key,
	@ContentType,
	@ParentID
)

DECLARE	@TopicID		INT

SELECT	@TopicID		= SCOPE_IDENTITY()

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE ATTRIBUTES FROM STRING
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	Attributes (
	TopicID		,
	AttributeKey		,
	AttributeValue		,
	Version
)
SELECT	@TopicID,
	AttributeKey,
	AttributeValue,
	@Version
FROM	@Attributes
WHERE	ISNULL(AttributeValue, '')	!= ''

--------------------------------------------------------------------------------------------------------------------------------
-- ADD EXTENDED ATTRIBUTES (XML)
--------------------------------------------------------------------------------------------------------------------------------
IF @ExtendedAttributes IS NOT NULL
  BEGIN
    INSERT
    INTO	ExtendedAttributes (
	TopicID		,
	AttributesXml		,
	Version
    )
    VALUES (	@TopicID		,
	@ExtendedAttributes 	,
	@Version
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- ADD REFERENCES
--------------------------------------------------------------------------------------------------------------------------------
EXEC	UpdateReferences	@TopicID,
			@References,
			1

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID