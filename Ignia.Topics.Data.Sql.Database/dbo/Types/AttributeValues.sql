CREATE
TYPE	[dbo].[AttributeValues]
AS TABLE (
	AttributeKey		VARCHAR(128)	NOT NULL,
	AttributeValue		VARCHAR(255)
  PRIMARY KEY (	AttributeKey )
)