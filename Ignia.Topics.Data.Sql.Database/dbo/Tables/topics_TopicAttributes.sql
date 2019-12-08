CREATE
TABLE	[dbo].[topics_TopicAttributes] (
	  [TopicID]		INT	NOT NULL,
	  [AttributeKey]	VARCHAR (128)	NOT NULL,
	  [AttributeValue]	NVARCHAR (255)	NOT NULL,
	  [DateModified]	DATETIME	CONSTRAINT [DF_topics_TopicAttributes_DateModified] DEFAULT (getdate()) NOT NULL,
	  [Version]		DATETIME	CONSTRAINT [DF_topics_TopicAttributes_Version] DEFAULT (getdate()) NOT NULL,
  CONSTRAINT	  [PK_TopicAttributes]	PRIMARY KEY	CLUSTERED ([TopicID] ASC, [AttributeKey] ASC, [Version] DESC)
);

GO
CREATE NONCLUSTERED INDEX [IX_topics_TopicAttributes_AttributeKey]
  ON	[dbo].[topics_TopicAttributes] (
	  [TopicID]		ASC,
	  [AttributeKey]	ASC,
	  [Version]		DESC
	)
  INCLUDE (	[AttributeValue]
  )
  WHERE (	[AttributeKey]		IN	('Key', 'ParentID', 'ContentType')
  );

