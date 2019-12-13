--------------------------------------------------------------------------------------------------------------------------------
-- ATTRIBUTE VALUES TYPE
--------------------------------------------------------------------------------------------------------------------------------
-- Represents a collection of attribute key/value pairs for a single topic. Useful for relaying collections of attributes
-- instead of needing to e.g. pass and parse a delimited string.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TYPE	[dbo].[AttributeValues]
AS TABLE (
	AttributeKey		VARCHAR(128)	NOT NULL,
	AttributeValue		VARCHAR(255)
  PRIMARY KEY (	AttributeKey )
)