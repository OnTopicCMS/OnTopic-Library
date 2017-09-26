-----------------------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GET TOPIC ID
--
-- Purpose	Given a particular topic key, finds the FIRST instance of the TopicID associated with that key. Be aware that since keys are
--		not guaranteed to be unique, this may yield unexpected results if multiple topics share the same key; in that case, the first
--		(and, thus, earliest) instance will be returned.
--
-- History      Jeremy Caney		07262017  Initial Creation based on getTopics code.
-----------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_GetTopicID]
		@TopicKey	varchar(64)		= null
AS

-----------------------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND DEFINE VARIABLES
-----------------------------------------------------------------------------------------------------------------------------------------------
DECLARE		@TopicID	int

-----------------------------------------------------------------------------------------------------------------------------------------------
-- GET TOPIC ID BASED ON TOPIC KEY
-----------------------------------------------------------------------------------------------------------------------------------------------
SELECT		TOP 1
		@TopicID = TopicID
FROM		topics_TopicAttributes
WHERE		AttributeKey = 'Key'
  AND		AttributeValue like @TopicKey
ORDER BY	TopicID desc

-----------------------------------------------------------------------------------------------------------------------------------------------
-- RETURN TOPIC ID
-----------------------------------------------------------------------------------------------------------------------------------------------
RETURN		@TopicID