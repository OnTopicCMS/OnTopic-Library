CREATE
TABLE	[dbo].[topics_Hierarchy] (
	  [TopicID]		INT	NOT NULL,
	  [Parent_TopicID]	INT	NULL,
	  [DateAdded]		DATETIME	NOT NULL,
  CONSTRAINT	  [PK_Topics_Hierarchy]	PRIMARY KEY	CLUSTERED ([TopicID] ASC)
);

