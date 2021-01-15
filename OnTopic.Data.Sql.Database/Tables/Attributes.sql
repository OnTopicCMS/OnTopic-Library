--------------------------------------------------------------------------------------------------------------------------------
-- ATTRIBUTES (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides the primary storage for topic attributes, including not only core identifiers such as Key, ContentType, and
-- ParentID, but additionally any other key/value pairs associated with a topic. This table limits values to 255 and is intended
-- for "indexed" attributes—i.e., attributes that are widely referenced and should be easy to access. Longer values, or those
-- only needed in narrow cases, should instead be store in the attribute AttributeXml.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[Attributes] (
	  [TopicID]		INT	NOT NULL,
	  [AttributeKey]	VARCHAR (128)	NOT NULL,
	  [AttributeValue]	NVARCHAR (255)	NOT NULL,
	  [Version]		DATETIME	NOT NULL	DEFAULT GETUTCDATE()
  CONSTRAINT	  [PK_Attributes]	PRIMARY KEY
  CLUSTERED (	    [TopicID]		ASC,
	    [AttributeKey]	ASC,
	    [Version]		DESC
  ),
  CONSTRAINT	  [FK_Attributes_TopicID]
  FOREIGN KEY (	    [TopicID]
	  )
  REFERENCES	  [dbo].[Topics] (
	    [TopicID]
	  )
);

GO

--------------------------------------------------------------------------------------------------------------------------------
-- CORE ATTRIBUTES (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a filtered index of the core attributes needed to establish a topic entity—namely the Key, ContentType, and
-- ParentID.
--------------------------------------------------------------------------------------------------------------------------------
CREATE	NONCLUSTERED
INDEX	[IX_Attributes_AttributeKey]
  ON	[dbo].[Attributes] (
	  [TopicID]		ASC,
	  [AttributeKey]	ASC,
	  [Version]		DESC
	)
  INCLUDE (	[AttributeValue]
  )
  WHERE (	[AttributeKey]
    IN (	'Key',
	'ParentID',
	'ContentType'
    )
  );