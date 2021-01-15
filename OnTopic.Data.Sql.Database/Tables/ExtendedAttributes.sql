--------------------------------------------------------------------------------------------------------------------------------
-- EXTENDED ATTRIBUTES (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a storage AttributeXml (in XML format) for attribute key/value pairs. Attributes can also be stored in the
-- Attributes table. The difference is that the latter limits attribute values to 255 characters per AttributeValue,
-- whereas the AttributeXml offers virtually unlimited storage capacity (at least in practical terms).
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[ExtendedAttributes] (
	  [TopicID]		INT	NOT NULL,
	  [AttributesXml]	XML	NOT NULL,
	  [Version]		DATETIME	NOT NULL	DEFAULT GETUTCDATE(),
  CONSTRAINT	  [PK_ExtendedAttributes]		PRIMARY KEY	CLUSTERED (
	    [TopicID]		ASC,
	    [Version]		DESC
	  ),
  CONSTRAINT	  [FK_ExtendedAttributes_Topics]
  FOREIGN KEY (	    [TopicID]
	)
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	)
);