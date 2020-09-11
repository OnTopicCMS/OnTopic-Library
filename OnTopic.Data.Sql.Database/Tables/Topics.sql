--------------------------------------------------------------------------------------------------------------------------------
-- TOPICS (TABLE)
--------------------------------------------------------------------------------------------------------------------------------
-- Represents the core topics hierarchy, using the nested set model. Also the source for generating the TopicID identity. Every
-- table in the database is keyed off of this table.
--------------------------------------------------------------------------------------------------------------------------------
CREATE
TABLE	[dbo].[Topics] (
  	  [Stack_Top]		INT	NULL,
	  [TopicID]		INT	IDENTITY (1, 1) NOT NULL,
	  [RangeLeft]		INT	NOT NULL,
	  [RangeRight]		INT	NOT NULL,
  CONSTRAINT	  [PK_Topics]	PRIMARY KEY
  CLUSTERED (     [TopicID]		ASC
	  )
);

GO

--------------------------------------------------------------------------------------------------------------------------------
-- RANGE LEFT (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides the primary index for evaluating topics as part of a hierarchy.
--------------------------------------------------------------------------------------------------------------------------------

CREATE	NONCLUSTERED
INDEX	[IX_Topics_RangeLeft_RangeRight]
  ON	[dbo].[Topics] (
	  [RangeLeft] ASC,
	  [RangeRight] ASC
  );

GO

--------------------------------------------------------------------------------------------------------------------------------
-- RANGE RIGHT (INDEX)
--------------------------------------------------------------------------------------------------------------------------------
-- Provides a secondary index used by queries which need to insert topics into or remove topics from the hierarchy, and will
-- thus filter by and update based on RangeRight relative to the target insertion point. See, for example, CreateTopic
-- and topic_DeleteTopic.
--------------------------------------------------------------------------------------------------------------------------------
CREATE	NONCLUSTERED
INDEX	[IX_Topics_RangeRight]
  ON	[dbo].[Topics] (
	  [RangeRight] ASC
  );