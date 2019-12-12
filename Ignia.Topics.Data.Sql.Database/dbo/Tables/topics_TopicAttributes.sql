CREATE
TABLE	[dbo].[topics_TopicAttributes] (
	  [TopicID]		INT	NOT NULL,
	  [AttributeKey]	VARCHAR (128)	NOT NULL,
	  [AttributeValue]	NVARCHAR (255)	NOT NULL,
	  [DateModified]	DATETIME
  CONSTRAINT	  [DF_Attributes_DateModified]	DEFAULT	(GetDate())	NOT NULL,
	  [Version]		DATETIME
  CONSTRAINT	  [DF_Attributes_Version]	DEFAULT	(GetDate())	NOT NULL,
  CONSTRAINT	  [PK_TopicAttributes]	PRIMARY KEY
  CLUSTERED (	    [TopicID]		ASC,
	    [AttributeKey]	ASC,
	    [Version]		DESC
  ),
  CONSTRAINT	  [FK_TopicAttributes_TopicID]
  FOREIGN KEY (	    [TopicID]
	  )
  REFERENCES	  [dbo].[topics_Topics] (
	    [TopicID]
	  )
);

GO
CREATE	NONCLUSTERED
INDEX	[IX_topics_TopicAttributes_AttributeKey]
  ON	[dbo].[topics_TopicAttributes] (
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