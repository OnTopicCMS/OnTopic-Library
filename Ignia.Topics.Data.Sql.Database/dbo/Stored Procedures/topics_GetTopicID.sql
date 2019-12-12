--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
-- Given a particular topic key, finds the FIRST instance of the TopicID associated with that key. Be aware that since keys are
-- not guaranteed to be unique, this may yield unexpected results if multiple topics share the same key; in that case, the first
-- (and, thus, earliest) instance will be returned.
--------------------------------------------------------------------------------------------------------------------------------

CREATE PROCEDURE [dbo].[topics_GetTopicID]
	@TopicKey	varchar(255)	= null
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
DECLARE	@TopicID	int

--------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID BASED ON TOPIC KEY
--------------------------------------------------------------------------------------------------------------------------------
SELECT	TOP 1
	@TopicID		= Topics.TopicID
FROM	topics_TopicAttributes	Attributes
JOIN	topics_Topics		Topics
  ON	Attributes.TopicID	= Topics.TopicID
WHERE	AttributeKey		= 'Key'
  AND	AttributeValue		= CONVERT(NVarChar(255), @TopicKey)
ORDER BY	Topics.TopicID		desc

--------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
--------------------------------------------------------------------------------------------------------------------------------
RETURN	@TopicID