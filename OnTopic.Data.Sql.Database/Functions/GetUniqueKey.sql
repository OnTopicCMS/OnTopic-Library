﻿--------------------------------------------------------------------------------------------------------------------------------
-- GET UNIQUE KEY
--------------------------------------------------------------------------------------------------------------------------------
-- Given a TopicID, will return the unique key for the topic.
--------------------------------------------------------------------------------------------------------------------------------

CREATE
FUNCTION	[dbo].[GetUniqueKey]
(
	@TopicID		INT
)
RETURNS	VARCHAR(MAX)
AS

BEGIN

  ------------------------------------------------------------------------------------------------------------------------------
  -- DECLARE AND DEFINE VARIABLES
  ------------------------------------------------------------------------------------------------------------------------------
  DECLARE	@RangeLeft		INT
  DECLARE	@RangeRight		INT
  DECLARE	@UniqueKey		VARCHAR(MAX)

  ------------------------------------------------------------------------------------------------------------------------------
  -- FIND RANGE
  ------------------------------------------------------------------------------------------------------------------------------
  SELECT	@RangeLeft		= RangeLeft,
	@RangeRight		= RangeRight
  FROM	Topics
  WHERE	TopicID		= @TopicID

  ------------------------------------------------------------------------------------------------------------------------------
  -- CONSTRUCT UNIQUE KEY
  ------------------------------------------------------------------------------------------------------------------------------
  SELECT	@UniqueKey		= COALESCE(@UniqueKey + ':' + TopicKey, TopicKey)
  FROM	Topics
  WHERE	RangeLeft		<= @RangeLeft
  AND	RangeRight		>= @RangeRight
  ORDER BY	RangeLeft

  ------------------------------------------------------------------------------------------------------------------------------
  -- RETURN VALUE
  ------------------------------------------------------------------------------------------------------------------------------
  RETURN	@UniqueKey

END