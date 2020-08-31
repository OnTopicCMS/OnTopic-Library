--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID BY UNIQUE KEY
--------------------------------------------------------------------------------------------------------------------------------
-- Given a fully-qualified unique key, finds the TopicID associated with that key. Unlike [GetTopicID], this is guaranteed to
-- return an exclusive instance.
--------------------------------------------------------------------------------------------------------------------------------

CREATE
FUNCTION	[dbo].[GetTopicIDByUniqueKey]
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
	CAST(NULL AS NVARCHAR(255))	AS ParentID,
	CAST('Root' AS NVARCHAR(255))	AS UniqueKey
    FROM	Topics		root
    WHERE	root.TopicID		= 1

    UNION	ALL

    SELECT	p.TopicID,
	p.AttributeValue	AS ParentID,
	CAST(recursive.UniqueKey + ':' + TopicKey AS NVARCHAR(255)) AS UniqueKey
    FROM	Attributes		p
    CROSS APPLY (
      SELECT	AttributeValue		AS TopicKey
      FROM	[dbo].[Attributes]	k
      WHERE	k.TopicID		= p.TopicID
        AND	k.AttributeKey		= 'Key'
    )	TopicKey
    INNER JOIN	RCTE		recursive
      ON	p.AttributeValue	= CAST(recursive.TopicID AS NVARCHAR(10))
    WHERE	p.AttributeKey		= 'ParentID'
      AND	@UniqueKey		LIKE CAST(recursive.UniqueKey + ':' + TopicKey AS NVARCHAR(255)) + '%'
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