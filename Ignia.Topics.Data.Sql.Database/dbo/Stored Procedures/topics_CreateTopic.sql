--------------------------------------------------------------------------------------------------------------------------------
-- Procedure	CREATE TOPIC
--
-- Purpose	Creates a new topic.
--
-- History	Casey Margell		04062009	Initial Creation based on code from Celko's SQL For Smarties.
--	Jeremy Caney		05282010	Reformatted code and refactored identifiers for improved readability.
--	Katherine Trunkey	08142014	Updated topic_TopicAttributes insertion script to use uncommon, multi-character
--				delimiters rather than a colon and semicolon in order to provide better
--				escaping safety for @Attributes.
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_CreateTopic]
	@ParentID		int	= -1,
	@Attributes		varchar(max)	= '',
	@Blob		Xml	= null,
	@Version		datetime	= null,
	@IsDraft		bit	= 0
AS

--------------------------------------------------------------------------------------------------------------------------------
-- SET DEFAULT VERSION DATETIME
--------------------------------------------------------------------------------------------------------------------------------
IF	@Version		IS NULL
AND	@IsDraft		= 0
SET	@Version		= getdate()

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@RangeRight		Integer	--Right Most Sibling
SET	@RangeRight		= 0

--------------------------------------------------------------------------------------------------------------------------------
-- RESERVE SPACE FOR NEW CHILD.
--------------------------------------------------------------------------------------------------------------------------------
IF (@ParentID > -1)
  BEGIN
    SELECT	@RangeRight		= RangeRight
    FROM	topics_Topics
    WHERE	TopicID		= @ParentID

    UPDATE	topics_Topics
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
    WHERE	ISNULL(RangeRight, 0)	>= @RangeRight
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE NEW TOPIC
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	topics_Topics (
	RangeLeft,
	RangeRight
)
Values (
	@RangeRight,
	@RangeRight		+ 1
)

DECLARE	@TopicID		INT

SELECT	@TopicID		= SCOPE_IDENTITY()

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE ATTRIBUTES FROM STRING
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	topics_TopicAttributes (
	TopicID		,
	AttributeKey		,
	AttributeValue		,
	Version
)
SELECT	@TopicID		,
	Substring(s, 0, CharIndex('~~', s)),
	Substring(s, CharIndex('~~', s) + 2, Len(s)),
	@Version
FROM	Split (@Attributes, '``')
WHERE	s not like 'ParentID~~%'
  AND	s != ''

--------------------------------------------------------------------------------------------------------------------------------
-- CREATE BLOB
--------------------------------------------------------------------------------------------------------------------------------
IF @Blob is not null
  BEGIN
    INSERT
    INTO	topics_Blob (
	TopicID		,
	Blob		,
	Version
    )
    VALUES (	@TopicID		,
	@Blob		,
	@Version
    )
  END

--------------------------------------------------------------------------------------------------------------------------------
-- CACHE PARENT ID FOR DATA INTEGRITY PURPOSES
--------------------------------------------------------------------------------------------------------------------------------
INSERT INTO	topics_TopicAttributes (
	TopicID		,
	AttributeKey		,
	AttributeValue		,
	Version
)
VALUES (	@TopicID		,
	'ParentID'		,
	CONVERT(NVarChar(255), @ParentID),
	@Version
)

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID