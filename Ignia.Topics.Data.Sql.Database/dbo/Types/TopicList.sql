--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC LIST TYPE
--------------------------------------------------------------------------------------------------------------------------------
-- Represents a list of TopicIDs. Useful for relaying a list of topics instead of needing to e.g. pass and parse a delimited
-- string.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TYPE	[dbo].[TopicList]
AS TABLE (
	TopicId		INT	NOT NULL
  PRIMARY KEY (	TopicId )
)