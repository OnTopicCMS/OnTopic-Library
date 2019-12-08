--------------------------------------------------------------------------------------------------------------------------------
-- Procedure	GET TOPIC ATTRIBUTES
--
-- Purpose	Returns a list of attributes associated with a particular topic.
--
-- History	John Mulhausen		04072009	Created initial version.
--	Jeremy Caney		05282010	Reformatted code and refactored identifiers for improved readability.
--	Jeremy Caney		09272013	Removed dependency on Attributes, in favor of Oroboros Configuration.
--------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [dbo].[topics_GetTopicAttributes]
	@TopicID		INT	= -1
AS

--------------------------------------------------------------------------------------------------------------------------------
-- DECLARE AND SET VARIABLES
--------------------------------------------------------------------------------------------------------------------------------
SET NOCOUNT ON;

--------------------------------------------------------------------------------------------------------------------------------
-- SELECT ATTRIBUTES
--------------------------------------------------------------------------------------------------------------------------------
SELECT	AttributeKey,
	AttributeValue
FROM	topics_TopicAttributes
WHERE	TopicID		= @TopicID
ORDER BY	AttributeKey