CREATE
TABLE	[dbo].[topics_Topics] (
  	  [Stack_Top]		INT	NULL,
	  [TopicID]		INT	IDENTITY (1, 1) NOT NULL,
	  [RangeLeft]		INT	NOT NULL,
	  [RangeRight]		INT	NULL,
  CONSTRAINT	  [PK_topics_Topics]	PRIMARY KEY	CLUSTERED (
	    [TopicID]		ASC
	  )
);

GO
CREATE NONCLUSTERED INDEX [IX_topics_Topics]
  ON	[dbo].[topics_Topics] (
	  [RangeLeft] ASC,
	  [RangeRight] ASC
  );