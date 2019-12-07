CREATE
TABLE	[dbo].[topics_Blob] (
	  [TopicID]		INT	NOT NULL,
	  [Blob]		XML	NOT NULL,
	  [DateModified]	DATETIME	CONSTRAINT [DF_Blob_DateModified] DEFAULT (getdate()) NOT NULL,
	  [Version]		DATETIME	CONSTRAINT [DF_Blob_Version] DEFAULT (getdate()) NOT NULL,
  CONSTRAINT	[PK_Blob]		PRIMARY KEY	CLUSTERED ([TopicID] ASC, [Version] DESC),
  CONSTRAINT	[FK_Blob_Topics]	FOREIGN KEY	([TopicID]) REFERENCES [dbo].[topics_Topics] ([TopicID])
);

