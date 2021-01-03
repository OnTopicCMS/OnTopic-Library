--------------------------------------------------------------------------------------------------------------------------------
-- TOPIC REFERENCES TYPE
--------------------------------------------------------------------------------------------------------------------------------
-- Represents a list of reference keys associated with TopicIDs. Useful for relaying a list of topics instead of needing to e.g.
-- pass and parse a delimited string.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TYPE	[dbo].[TopicReferences]
AS TABLE (
	ReferenceKey		VARCHAR(128)	NOT NULL,
	TopicID		INT	NOT NULL
  PRIMARY KEY (	ReferenceKey )
)