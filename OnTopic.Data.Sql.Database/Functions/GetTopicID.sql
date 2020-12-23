--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
-- Given a fully-qualified unique key, finds the TopicID associated with that key. Unlike [GetTopicID], this is guaranteed to
-- return an exclusive instance.
--------------------------------------------------------------------------------------------------------------------------------

CREATE
FUNCTION	[dbo].[GetTopicID]
(
	@UniqueKey		NVARCHAR(2500)
)
RETURNS	INT
AS

BEGIN

  ------------------------------------------------------------------------------------------------------------------------------
  -- DECLARE AND DEFINE VARIABLES
  ------------------------------------------------------------------------------------------------------------------------------
  DECLARE	@TopicID		INT	= -1

  ------------------------------------------------------------------------------------------------------------------------------
  -- GET TOPIC ID BASED ON UNIQUE TOPIC KEY
  ------------------------------------------------------------------------------------------------------------------------------
  ;WITH RCTE AS (

    SELECT	TopicID,
	ParentID,
	CAST('Root' AS NVARCHAR(255))	AS UniqueKey
    FROM	Topics		root
    WHERE	root.TopicID		= 1

    UNION	ALL

    SELECT	Topics.TopicID,
	Topics.ParentID,
	CAST(recursive.UniqueKey + ':' + TopicKey AS NVARCHAR(255)) AS UniqueKey
    FROM	Topics
    INNER JOIN	RCTE		recursive
      ON	Topics.ParentID		= recursive.TopicID
    WHERE	@UniqueKey		LIKE CAST(recursive.UniqueKey + ':' + TopicKey AS NVARCHAR(255)) + '%'
    )
  SELECT	@TopicID		= TopicID
  FROM	RCTE		AS hierarchy
  WHERE	UniqueKey		= @UniqueKey
  ORDER BY	UniqueKey		ASC
  OPTION (
    OPTIMIZE
    FOR (	@UniqueKey		= 'Root'
    )
  )

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETURN TOPIC ID
  ------------------------------------------------------------------------------------------------------------------------------
  RETURN	@TopicID

END